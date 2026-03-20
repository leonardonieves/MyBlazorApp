using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using MyBlazorApp.Data;
using MyBlazorApp.Models;
using MyBlazorApp.Hubs;
using Stripe;

namespace MyBlazorApp.Services;

/// <summary>
/// Service to sync Stripe Products with local Raffles
/// </summary>
public class StripeSyncService
{
    private readonly AppDbContext _context;
    private readonly StripeService _stripeService;
    private readonly IHubContext<RaffleHub> _hubContext;
    private readonly ILogger<StripeSyncService> _logger;
    private readonly IConfiguration _configuration;

    // Reservation timeout in minutes
    private const int RESERVATION_TIMEOUT_MINUTES = 10;

    public StripeSyncService(
        AppDbContext context,
        StripeService stripeService,
        IHubContext<RaffleHub> hubContext,
        ILogger<StripeSyncService> logger,
        IConfiguration configuration)
    {
        _context = context;
        _stripeService = stripeService;
        _hubContext = hubContext;
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Debug: Get all Stripe products with their metadata for troubleshooting
    /// </summary>
    public async Task<List<object>> GetStripeProductsDebugAsync()
    {
        var stripeProducts = await _stripeService.GetProductsAsync();
        var result = new List<object>();

        foreach (var product in stripeProducts)
        {
            var prices = await _stripeService.GetPricesForProductAsync(product.Id);
            var activePrice = prices.FirstOrDefault(p => p.Active);

            result.Add(new
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Active = product.Active,
                Images = product.Images,
                Metadata = product.Metadata,
                IsDetectedAsRaffle = IsRaffleProduct(product),
                ActivePriceId = activePrice?.Id,
                PriceAmount = activePrice != null ? (decimal)(activePrice.UnitAmount ?? 0) / 100 : 0,
                Currency = activePrice?.Currency
            });
        }

        return result;
    }

    /// <summary>
    /// Sync all active Stripe products to local Raffles
    /// </summary>
    public async Task<List<Raffle>> SyncProductsFromStripeAsync()
    {
        _logger.LogInformation("=== Starting Stripe product sync ===");

        try
        {
            // Get all active products from Stripe
            var stripeProducts = await _stripeService.GetProductsAsync();
            _logger.LogInformation("Found {Count} total products in Stripe", stripeProducts.Count);

            var syncedRaffles = new List<Raffle>();

            foreach (var product in stripeProducts.Where(p => p.Active))
            {
                _logger.LogInformation("Checking product: {Name} (ID: {Id})", product.Name, product.Id);
                _logger.LogInformation("  - Metadata: {Metadata}", 
                    product.Metadata != null ? string.Join(", ", product.Metadata.Select(m => $"{m.Key}={m.Value}")) : "none");

                // Check if this is a raffle product (has raffle metadata or specific naming)
                var isRaffle = IsRaffleProduct(product);
                _logger.LogInformation("  - Is Raffle Product: {IsRaffle}", isRaffle);

                if (!isRaffle)
                    continue;

                var raffle = await SyncSingleProductAsync(product);
                if (raffle != null)
                {
                    syncedRaffles.Add(raffle);
                    _logger.LogInformation("  - Synced as Raffle ID: {RaffleId}", raffle.Id);
                }
            }

            _logger.LogInformation("=== Sync complete: {Count} raffles synced ===", syncedRaffles.Count);
            return syncedRaffles;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing products from Stripe");
            throw;
        }
    }

    /// <summary>
    /// Check if a Stripe product is a raffle product
    /// </summary>
    private bool IsRaffleProduct(Product product)
    {
        // Check metadata for raffle indicator
        if (product.Metadata != null)
        {
            if (product.Metadata.ContainsKey("type") && product.Metadata["type"] == "raffle")
                return true;
            if (product.Metadata.ContainsKey("is_raffle") && product.Metadata["is_raffle"] == "true")
                return true;
        }

        // Or check if name contains "raffle" or "sweepstakes"
        var name = product.Name?.ToLower() ?? "";
        return name.Contains("raffle") || name.Contains("sweepstakes") || name.Contains("ticket");
    }

    /// <summary>
    /// Sync a single Stripe product to a local Raffle
    /// </summary>
    public async Task<Raffle?> SyncSingleProductAsync(Product product)
    {
        try
        {
            // Check if raffle already exists
            var existingRaffle = await _context.Raffles
                .FirstOrDefaultAsync(r => r.StripeProductId == product.Id);

            // Get the price for this product
            var prices = await _stripeService.GetPricesForProductAsync(product.Id);
            var activePrice = prices.FirstOrDefault(p => p.Active);

            if (activePrice == null)
            {
                _logger.LogWarning("No active price found for product {ProductId}", product.Id);
                return existingRaffle;
            }

            var ticketPrice = (decimal)(activePrice.UnitAmount ?? 0) / 100;
            var totalTickets = GetTotalTicketsFromMetadata(product.Metadata);
            var drawDate = GetDrawDateFromMetadata(product.Metadata);
            var maxPerUser = GetMaxPerUserFromMetadata(product.Metadata);

            if (existingRaffle != null)
            {
                // Update existing raffle
                existingRaffle.Title = product.Name ?? existingRaffle.Title;
                existingRaffle.ShortDescription = product.Description ?? existingRaffle.ShortDescription;
                existingRaffle.TicketPrice = ticketPrice;
                existingRaffle.StripePriceId = activePrice.Id;
                existingRaffle.PrimaryImageUrl = product.Images?.FirstOrDefault();
                existingRaffle.UpdatedAt = DateTime.UtcNow;
                
                // Only update these if they were specified in metadata
                if (totalTickets > 0 && existingRaffle.TotalTickets != totalTickets)
                {
                    existingRaffle.TotalTickets = totalTickets;
                    // Generate additional tickets if needed
                    await EnsureTicketsGeneratedAsync(existingRaffle.Id, totalTickets);
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Updated raffle {RaffleId} from Stripe product {ProductId}", 
                    existingRaffle.Id, product.Id);

                return existingRaffle;
            }
            else
            {
                // Create new raffle
                var newRaffle = new Raffle
                {
                    Title = product.Name ?? "Untitled Raffle",
                    ShortDescription = product.Description ?? "Enter for a chance to win!",
                    FullDescription = product.Description ?? "",
                    TicketPrice = ticketPrice,
                    TotalTickets = totalTickets > 0 ? totalTickets : 100, // Default 100 tickets
                    MaxTicketsPerUser = maxPerUser > 0 ? maxPerUser : 10,
                    StripeProductId = product.Id,
                    StripePriceId = activePrice.Id,
                    PrimaryImageUrl = product.Images?.FirstOrDefault(),
                    Status = RaffleStatus.Active,
                    SalesStartDate = DateTime.UtcNow,
                    SalesEndDate = drawDate.AddDays(-1),
                    DrawDate = drawDate,
                    IsFeatured = product.Metadata?.ContainsKey("featured") == true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Raffles.Add(newRaffle);
                await _context.SaveChangesAsync();

                // Generate tickets for this raffle
                await GenerateTicketsForRaffleAsync(newRaffle.Id, newRaffle.TotalTickets);

                // Sync prizes from metadata if available
                await SyncPrizesFromMetadataAsync(newRaffle.Id, product.Metadata);

                _logger.LogInformation("Created new raffle {RaffleId} from Stripe product {ProductId}", 
                    newRaffle.Id, product.Id);

                return newRaffle;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing product {ProductId}", product.Id);
            return null;
        }
    }

    /// <summary>
    /// Generate pre-generated tickets for a raffle
    /// </summary>
    public async Task GenerateTicketsForRaffleAsync(int raffleId, int totalTickets)
    {
        _logger.LogInformation("Generating {Count} tickets for raffle {RaffleId}", totalTickets, raffleId);

        var tickets = new List<Ticket>();
        var existingCount = await _context.Tickets.CountAsync(t => t.RaffleId == raffleId);

        for (int i = existingCount + 1; i <= totalTickets; i++)
        {
            tickets.Add(new Ticket
            {
                RaffleId = raffleId,
                DisplayNumber = i.ToString("D4"), // "0001", "0002", etc.
                TicketNumber = $"R{raffleId:D4}-{i:D4}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}",
                Status = TicketStatus.Available,
                CreatedAt = DateTime.UtcNow
            });

            // Batch insert every 500 tickets
            if (tickets.Count >= 500)
            {
                _context.Tickets.AddRange(tickets);
                await _context.SaveChangesAsync();
                tickets.Clear();
            }
        }

        // Insert remaining tickets
        if (tickets.Any())
        {
            _context.Tickets.AddRange(tickets);
            await _context.SaveChangesAsync();
        }

        _logger.LogInformation("Generated tickets for raffle {RaffleId}", raffleId);
    }

    /// <summary>
    /// Ensure tickets are generated up to the total count
    /// </summary>
    private async Task EnsureTicketsGeneratedAsync(int raffleId, int totalTickets)
    {
        var existingCount = await _context.Tickets.CountAsync(t => t.RaffleId == raffleId);
        if (existingCount < totalTickets)
        {
            await GenerateTicketsForRaffleAsync(raffleId, totalTickets);
        }
    }

    /// <summary>
    /// Sync prizes from product metadata
    /// </summary>
    private async Task SyncPrizesFromMetadataAsync(int raffleId, IDictionary<string, string>? metadata)
    {
        if (metadata == null) return;

        // Look for prize_1, prize_2, etc. in metadata
        var prizes = new List<RafflePrize>();
        for (int i = 1; i <= 10; i++)
        {
            var key = $"prize_{i}";
            if (metadata.ContainsKey(key))
            {
                prizes.Add(new RafflePrize
                {
                    RaffleId = raffleId,
                    Description = metadata[key],
                    Icon = metadata.ContainsKey($"prize_{i}_icon") ? metadata[$"prize_{i}_icon"] : "fas fa-gift",
                    DisplayOrder = i - 1
                });
            }
        }

        if (prizes.Any())
        {
            _context.RafflePrizes.AddRange(prizes);
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Get total tickets from metadata
    /// </summary>
    private int GetTotalTicketsFromMetadata(IDictionary<string, string>? metadata)
    {
        if (metadata == null) return 100;
        if (metadata.ContainsKey("total_tickets") && int.TryParse(metadata["total_tickets"], out var total))
            return total;
        return 100;
    }

    /// <summary>
    /// Get draw date from metadata
    /// </summary>
    private DateTime GetDrawDateFromMetadata(IDictionary<string, string>? metadata)
    {
        if (metadata == null) return DateTime.UtcNow.AddDays(30);
        if (metadata.ContainsKey("draw_date") && DateTime.TryParse(metadata["draw_date"], out var date))
            return date;
        return DateTime.UtcNow.AddDays(30);
    }

    /// <summary>
    /// Get max tickets per user from metadata
    /// </summary>
    private int GetMaxPerUserFromMetadata(IDictionary<string, string>? metadata)
    {
        if (metadata == null) return 10;
        if (metadata.ContainsKey("max_per_user") && int.TryParse(metadata["max_per_user"], out var max))
            return max;
        return 10;
    }

    #region Ticket Reservation

    /// <summary>
    /// Reserve tickets for a user (temporary hold during checkout)
    /// </summary>
    public async Task<ReserveTicketsResult> ReserveTicketsAsync(int raffleId, int userId, List<int> ticketIds)
    {
        var result = new ReserveTicketsResult();

        try
        {
            // First, release any expired reservations
            await ReleaseExpiredReservationsAsync(raffleId);

            var tickets = await _context.Tickets
                .Where(t => t.RaffleId == raffleId && ticketIds.Contains(t.Id))
                .ToListAsync();

            var availableTickets = tickets.Where(t => t.Status == TicketStatus.Available).ToList();
            var alreadyReserved = tickets.Where(t => t.Status == TicketStatus.Reserved).ToList();
            var alreadySold = tickets.Where(t => t.Status == TicketStatus.Sold).ToList();

            if (alreadySold.Any())
            {
                result.Success = false;
                result.Message = $"Some tickets are already sold: {string.Join(", ", alreadySold.Select(t => t.DisplayNumber))}";
                result.UnavailableTicketIds = alreadySold.Select(t => t.Id).ToList();
                return result;
            }

            if (alreadyReserved.Any(t => t.UserId != userId))
            {
                result.Success = false;
                result.Message = $"Some tickets are reserved by another user: {string.Join(", ", alreadyReserved.Where(t => t.UserId != userId).Select(t => t.DisplayNumber))}";
                result.UnavailableTicketIds = alreadyReserved.Where(t => t.UserId != userId).Select(t => t.Id).ToList();
                return result;
            }

            // Reserve the available tickets
            var now = DateTime.UtcNow;
            var expiresAt = now.AddMinutes(RESERVATION_TIMEOUT_MINUTES);

            foreach (var ticket in availableTickets)
            {
                ticket.Status = TicketStatus.Reserved;
                ticket.UserId = userId;
                ticket.ReservedAt = now;
                ticket.ReservationExpiresAt = expiresAt;
            }

            await _context.SaveChangesAsync();

            // Broadcast the update via SignalR
            await BroadcastTicketUpdatesAsync(raffleId, availableTickets);

            result.Success = true;
            result.ReservedTicketIds = availableTickets.Select(t => t.Id).ToList();
            result.ExpiresAt = expiresAt;
            result.Message = $"Reserved {availableTickets.Count} tickets";

            _logger.LogInformation("User {UserId} reserved {Count} tickets for raffle {RaffleId}", 
                userId, availableTickets.Count, raffleId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reserving tickets");
            result.Success = false;
            result.Message = "Error reserving tickets";
        }

        return result;
    }

    /// <summary>
    /// Release reserved tickets (user cancelled or timeout)
    /// </summary>
    public async Task ReleaseReservationAsync(int raffleId, int userId, List<int>? ticketIds = null)
    {
        var query = _context.Tickets
            .Where(t => t.RaffleId == raffleId && 
                       t.UserId == userId && 
                       t.Status == TicketStatus.Reserved);

        if (ticketIds != null && ticketIds.Any())
        {
            query = query.Where(t => ticketIds.Contains(t.Id));
        }

        var tickets = await query.ToListAsync();

        foreach (var ticket in tickets)
        {
            ticket.Status = TicketStatus.Available;
            ticket.UserId = null;
            ticket.ReservedAt = null;
            ticket.ReservationExpiresAt = null;
        }

        await _context.SaveChangesAsync();

        // Broadcast the update
        await BroadcastTicketUpdatesAsync(raffleId, tickets);

        _logger.LogInformation("Released {Count} reservations for user {UserId} on raffle {RaffleId}", 
            tickets.Count, userId, raffleId);
    }

    /// <summary>
    /// Release all expired reservations for a raffle
    /// </summary>
    public async Task ReleaseExpiredReservationsAsync(int? raffleId = null)
    {
        var now = DateTime.UtcNow;

        var query = _context.Tickets
            .Where(t => t.Status == TicketStatus.Reserved && t.ReservationExpiresAt <= now);

        if (raffleId.HasValue)
        {
            query = query.Where(t => t.RaffleId == raffleId.Value);
        }

        var expiredTickets = await query.ToListAsync();

        if (!expiredTickets.Any()) return;

        // Group by raffle for broadcasting
        var ticketsByRaffle = expiredTickets.GroupBy(t => t.RaffleId);

        foreach (var ticket in expiredTickets)
        {
            ticket.Status = TicketStatus.Available;
            ticket.UserId = null;
            ticket.ReservedAt = null;
            ticket.ReservationExpiresAt = null;
        }

        await _context.SaveChangesAsync();

        // Broadcast updates for each raffle
        foreach (var group in ticketsByRaffle)
        {
            await BroadcastTicketUpdatesAsync(group.Key, group.ToList());
        }

        _logger.LogInformation("Released {Count} expired reservations", expiredTickets.Count);
    }

    /// <summary>
    /// Confirm ticket purchase (called from webhook)
    /// </summary>
    public async Task<List<Ticket>> ConfirmTicketPurchaseAsync(
        int raffleId, 
        int userId, 
        List<int> ticketIds, 
        string paymentIntentId, 
        string? sessionId,
        string buyerEmail,
        string? buyerName,
        decimal amountPaid)
    {
        var tickets = await _context.Tickets
            .Where(t => t.RaffleId == raffleId && ticketIds.Contains(t.Id))
            .ToListAsync();

        var now = DateTime.UtcNow;
        var pricePerTicket = tickets.Count > 0 ? amountPaid / tickets.Count : 0;

        foreach (var ticket in tickets)
        {
            ticket.Status = TicketStatus.Sold;
            ticket.UserId = userId;
            ticket.BuyerEmail = buyerEmail;
            ticket.BuyerName = buyerName;
            ticket.StripePaymentIntentId = paymentIntentId;
            ticket.StripeSessionId = sessionId;
            ticket.AmountPaid = pricePerTicket;
            ticket.SoldAt = now;
            ticket.ReservedAt = null;
            ticket.ReservationExpiresAt = null;
        }

        // Update raffle sold count
        var raffle = await _context.Raffles.FindAsync(raffleId);
        if (raffle != null)
        {
            raffle.TicketsSold = await _context.Tickets
                .CountAsync(t => t.RaffleId == raffleId && t.Status == TicketStatus.Sold);
            raffle.UpdatedAt = now;
        }

        await _context.SaveChangesAsync();

        // Broadcast the update
        await BroadcastTicketUpdatesAsync(raffleId, tickets);

        _logger.LogInformation("Confirmed purchase of {Count} tickets for user {UserId} on raffle {RaffleId}", 
            tickets.Count, userId, raffleId);

        return tickets;
    }

    #endregion

    #region SignalR Broadcasting

    /// <summary>
    /// Broadcast ticket status updates to all clients viewing a raffle
    /// </summary>
    private async Task BroadcastTicketUpdatesAsync(int raffleId, List<Ticket> tickets)
    {
        var updates = tickets.Select(t => new TicketStatusUpdate
        {
            TicketId = t.Id,
            RaffleId = t.RaffleId,
            DisplayNumber = t.DisplayNumber,
            Status = t.Status.ToString().ToLower(),
            ReservedByUserId = t.UserId,
            ExpiresAt = t.ReservationExpiresAt
        }).ToList();

        var groupName = RaffleHub.GetRaffleGroupName(raffleId);
        await _hubContext.Clients.Group(groupName).SendAsync("TicketStatusChanged", updates);

        _logger.LogDebug("Broadcast {Count} ticket updates for raffle {RaffleId}", updates.Count, raffleId);
    }

    /// <summary>
    /// Get all tickets for a raffle (for initial load)
    /// </summary>
    public async Task<List<TicketStatusUpdate>> GetTicketStatusesAsync(int raffleId)
    {
        // First release expired reservations
        await ReleaseExpiredReservationsAsync(raffleId);

        var tickets = await _context.Tickets
            .Where(t => t.RaffleId == raffleId)
            .OrderBy(t => t.DisplayNumber)
            .Select(t => new TicketStatusUpdate
            {
                TicketId = t.Id,
                RaffleId = t.RaffleId,
                DisplayNumber = t.DisplayNumber,
                Status = t.Status.ToString().ToLower(),
                ReservedByUserId = t.UserId,
                ExpiresAt = t.ReservationExpiresAt
            })
            .ToListAsync();

        return tickets;
    }

    #endregion
}

/// <summary>
/// Result of ticket reservation
/// </summary>
public class ReserveTicketsResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<int> ReservedTicketIds { get; set; } = new();
    public List<int> UnavailableTicketIds { get; set; } = new();
    public DateTime? ExpiresAt { get; set; }
}
