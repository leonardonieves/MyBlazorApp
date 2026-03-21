using Microsoft.EntityFrameworkCore;
using MyBlazorApp.Api.Data;
using MyBlazorApp.Api.Models;

namespace MyBlazorApp.Api.Services;

/// <summary>
/// Service to handle raffle operations
/// </summary>
public class RaffleService
{
    private readonly AppDbContext _context;
    private readonly ILogger<RaffleService> _logger;

    public RaffleService(AppDbContext context, ILogger<RaffleService> logger)
    {
        _context = context;
        _logger = logger;
    }

    #region Raffle CRUD Operations

        /// <summary>
        /// Get all active raffles that are available for purchase
        /// Filters: Status must be Active AND DrawDate must be in the future
        /// </summary>
        public async Task<List<Raffle>> GetActiveRafflesAsync()
        {
            var now = DateTime.UtcNow;

            _logger.LogInformation("GetActiveRafflesAsync: Checking raffles at {Now}", now);

            // First, log what's in the database for debugging
            var allRaffles = await _context.Raffles.ToListAsync();
            foreach (var r in allRaffles)
            {
                _logger.LogInformation("Raffle {Id}: Status={Status}, DrawDate={DrawDate}, SalesStart={SalesStart}, SalesEnd={SalesEnd}",
                    r.Id, r.Status, r.DrawDate, r.SalesStartDate, r.SalesEndDate);
            }

            // Filter: Active status and draw date hasn't passed yet
            var activeRaffles = await _context.Raffles
                .Where(r => r.Status == RaffleStatus.Active && r.DrawDate > now)
                .Include(r => r.Prizes.OrderBy(p => p.DisplayOrder))
                .Include(r => r.Images.OrderBy(i => i.DisplayOrder))
                .OrderByDescending(r => r.IsFeatured)
                .ThenBy(r => r.DrawDate)
                .ToListAsync();

            _logger.LogInformation("GetActiveRafflesAsync: Found {Count} active raffles", activeRaffles.Count);

            return activeRaffles;
        }

        /// <summary>
        /// Get ALL raffles without filters (for debugging)
        /// </summary>
        public async Task<List<Raffle>> GetAllRafflesAsync()
        {
            return await _context.Raffles
                .Include(r => r.Prizes.OrderBy(p => p.DisplayOrder))
                .Include(r => r.Images.OrderBy(i => i.DisplayOrder))
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Get featured raffles
        /// </summary>
        public async Task<List<Raffle>> GetFeaturedRafflesAsync(int count = 3)
    {
        return await _context.Raffles
            .Where(r => r.Status == RaffleStatus.Active && 
                        r.IsFeatured &&
                        r.SalesEndDate > DateTime.UtcNow)
            .Include(r => r.Prizes.OrderBy(p => p.DisplayOrder))
            .OrderBy(r => r.DrawDate)
            .Take(count)
            .ToListAsync();
    }

    /// <summary>
    /// Get raffle by ID with all related data
    /// </summary>
    public async Task<Raffle?> GetRaffleByIdAsync(int id)
    {
        return await _context.Raffles
            .Include(r => r.Prizes.OrderBy(p => p.DisplayOrder))
            .Include(r => r.Images.OrderBy(i => i.DisplayOrder))
            .Include(r => r.Winner)
                .ThenInclude(w => w!.Ticket)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    /// <summary>
    /// Get raffle by Stripe Product ID
    /// </summary>
    public async Task<Raffle?> GetRaffleByStripeProductIdAsync(string stripeProductId)
    {
        return await _context.Raffles
            .FirstOrDefaultAsync(r => r.StripeProductId == stripeProductId);
    }

    /// <summary>
    /// Create a new raffle
    /// </summary>
    public async Task<Raffle> CreateRaffleAsync(Raffle raffle)
    {
        raffle.CreatedAt = DateTime.UtcNow;
        raffle.UpdatedAt = DateTime.UtcNow;
        raffle.Status = RaffleStatus.Draft;
        raffle.TicketsSold = 0;

        _context.Raffles.Add(raffle);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created raffle: {RaffleId} - {Title}", raffle.Id, raffle.Title);
        return raffle;
    }

    /// <summary>
    /// Update raffle
    /// </summary>
    public async Task<Raffle> UpdateRaffleAsync(Raffle raffle)
    {
        raffle.UpdatedAt = DateTime.UtcNow;
        _context.Raffles.Update(raffle);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated raffle: {RaffleId}", raffle.Id);
        return raffle;
    }

    /// <summary>
    /// Activate a raffle (make it available for purchase)
    /// </summary>
    public async Task<bool> ActivateRaffleAsync(int raffleId)
    {
        var raffle = await _context.Raffles.FindAsync(raffleId);
        if (raffle == null) return false;

        if (string.IsNullOrEmpty(raffle.StripePriceId))
        {
            _logger.LogWarning("Cannot activate raffle {RaffleId} without Stripe Price ID", raffleId);
            return false;
        }

        raffle.Status = RaffleStatus.Active;
        raffle.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Activated raffle: {RaffleId}", raffleId);
        return true;
    }

    /// <summary>
    /// Close sales for a raffle
    /// </summary>
    public async Task<bool> CloseSalesAsync(int raffleId)
    {
        var raffle = await _context.Raffles.FindAsync(raffleId);
        if (raffle == null) return false;

        raffle.Status = RaffleStatus.SalesClosed;
        raffle.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Closed sales for raffle: {RaffleId}", raffleId);
        return true;
    }

    #endregion

    #region Prize Management

    /// <summary>
    /// Add prize to a raffle
    /// </summary>
    public async Task<RafflePrize> AddPrizeAsync(int raffleId, string description, string icon = "fas fa-gift")
    {
        var maxOrder = await _context.RafflePrizes
            .Where(p => p.RaffleId == raffleId)
            .MaxAsync(p => (int?)p.DisplayOrder) ?? -1;

        var prize = new RafflePrize
        {
            RaffleId = raffleId,
            Description = description,
            Icon = icon,
            DisplayOrder = maxOrder + 1
        };

        _context.RafflePrizes.Add(prize);
        await _context.SaveChangesAsync();

        return prize;
    }

    #endregion

    #region Ticket Operations

    /// <summary>
    /// Get available tickets count for a raffle
    /// </summary>
    public async Task<int> GetAvailableTicketsCountAsync(int raffleId)
    {
        var raffle = await _context.Raffles.FindAsync(raffleId);
        return raffle?.TicketsAvailable ?? 0;
    }

    /// <summary>
    /// Check if raffle has available tickets
    /// </summary>
    public async Task<bool> HasAvailableTicketsAsync(int raffleId, int quantity)
    {
        var available = await GetAvailableTicketsCountAsync(raffleId);
        return available >= quantity;
    }

    /// <summary>
    /// Check how many tickets a user has purchased for a raffle
    /// </summary>
    public async Task<int> GetUserTicketCountAsync(int raffleId, int userId)
    {
        return await _context.Tickets
            .CountAsync(t => t.RaffleId == raffleId && 
                            t.UserId == userId && 
                            t.Status == TicketStatus.Confirmed);
    }

    /// <summary>
    /// Check if user can purchase more tickets
    /// </summary>
    public async Task<(bool canPurchase, int remainingAllowed)> CanUserPurchaseAsync(int raffleId, int userId, int quantity)
    {
        var raffle = await _context.Raffles.FindAsync(raffleId);
        if (raffle == null) return (false, 0);

        if (!raffle.CanPurchase) return (false, 0);

        var userTicketCount = await GetUserTicketCountAsync(raffleId, userId);
        var remainingAllowed = raffle.MaxTicketsPerUser == 0 
            ? int.MaxValue 
            : raffle.MaxTicketsPerUser - userTicketCount;

        var canPurchase = remainingAllowed >= quantity && raffle.TicketsAvailable >= quantity;
        return (canPurchase, remainingAllowed);
    }

    /// <summary>
    /// Generate unique ticket number
    /// </summary>
    public async Task<string> GenerateTicketNumberAsync(int raffleId)
    {
        string ticketNumber;
        bool exists;

        do
        {
            // Format: R{RaffleID}-{Random6Digits}
            ticketNumber = $"R{raffleId:D4}-{Random.Shared.Next(100000, 999999)}";
            exists = await _context.Tickets.AnyAsync(t => t.TicketNumber == ticketNumber);
        }
        while (exists);

        return ticketNumber;
    }

    /// <summary>
    /// Create ticket (called after payment confirmation)
    /// </summary>
    public async Task<Ticket> CreateTicketAsync(Ticket ticket)
    {
        // Generate unique ticket number if not provided
        if (string.IsNullOrEmpty(ticket.TicketNumber))
        {
            ticket.TicketNumber = await GenerateTicketNumberAsync(ticket.RaffleId);
        }

        ticket.CreatedAt = DateTime.UtcNow;
        ticket.Status = TicketStatus.Pending;

        _context.Tickets.Add(ticket);

        // Update raffle tickets sold count
        var raffle = await _context.Raffles.FindAsync(ticket.RaffleId);
        if (raffle != null)
        {
            raffle.TicketsSold++;
            raffle.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Created ticket: {TicketNumber} for raffle {RaffleId}", 
            ticket.TicketNumber, ticket.RaffleId);
        return ticket;
    }

    /// <summary>
    /// Create multiple tickets for bulk purchase
    /// </summary>
    public async Task<List<Ticket>> CreateTicketsAsync(int raffleId, int userId, string email, 
        string name, int quantity, string paymentIntentId, string? sessionId, decimal amountPaid)
    {
        var tickets = new List<Ticket>();

        for (int i = 0; i < quantity; i++)
        {
            var ticket = new Ticket
            {
                RaffleId = raffleId,
                UserId = userId,
                BuyerEmail = email,
                BuyerName = name,
                TicketNumber = await GenerateTicketNumberAsync(raffleId),
                StripePaymentIntentId = paymentIntentId,
                StripeSessionId = sessionId,
                AmountPaid = amountPaid / quantity, // Price per ticket
                Status = TicketStatus.Confirmed,
                CreatedAt = DateTime.UtcNow
            };

            tickets.Add(ticket);
            _context.Tickets.Add(ticket);
        }

        // Update raffle tickets sold count
        var raffle = await _context.Raffles.FindAsync(raffleId);
        if (raffle != null)
        {
            raffle.TicketsSold += quantity;
            raffle.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Created {Count} tickets for raffle {RaffleId}, User {UserId}", 
            quantity, raffleId, userId);

        return tickets;
    }

    /// <summary>
    /// Confirm ticket payment
    /// </summary>
    public async Task<List<Ticket>> ConfirmTicketPaymentAsync(string paymentIntentId)
    {
        var tickets = await _context.Tickets
            .Where(t => t.StripePaymentIntentId == paymentIntentId)
            .ToListAsync();

        foreach (var ticket in tickets)
        {
            ticket.Status = TicketStatus.Confirmed;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Confirmed {Count} tickets for payment: {PaymentIntentId}", 
            tickets.Count, paymentIntentId);

        return tickets;
    }

    /// <summary>
    /// Get tickets by user ID
    /// </summary>
    public async Task<List<Ticket>> GetTicketsByUserIdAsync(int userId)
    {
        return await _context.Tickets
            .Include(t => t.Raffle)
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Get tickets by email
    /// </summary>
    public async Task<List<Ticket>> GetTicketsByEmailAsync(string email)
    {
        return await _context.Tickets
            .Include(t => t.Raffle)
            .Where(t => t.BuyerEmail == email)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Get all confirmed tickets for a raffle
    /// </summary>
    public async Task<List<Ticket>> GetConfirmedTicketsAsync(int raffleId)
    {
        return await _context.Tickets
            .Where(t => t.RaffleId == raffleId && t.Status == TicketStatus.Confirmed)
            .OrderBy(t => t.TicketNumber)
            .ToListAsync();
    }

    #endregion

    #region Winner Drawing

    /// <summary>
    /// Draw a winner for a raffle using cryptographically secure random selection
    /// </summary>
    public async Task<Winner?> DrawWinnerAsync(int raffleId)
    {
        var raffle = await _context.Raffles
            .Include(r => r.Winner)
            .FirstOrDefaultAsync(r => r.Id == raffleId);

        if (raffle == null)
        {
            _logger.LogWarning("Raffle not found: {RaffleId}", raffleId);
            return null;
        }

        if (raffle.Winner != null)
        {
            _logger.LogWarning("Raffle already has a winner: {RaffleId}", raffleId);
            return raffle.Winner;
        }

        // Get all confirmed tickets
        var confirmedTickets = await _context.Tickets
            .Where(t => t.RaffleId == raffleId && t.Status == TicketStatus.Confirmed)
            .ToListAsync();

        if (!confirmedTickets.Any())
        {
            _logger.LogWarning("No confirmed tickets for raffle: {RaffleId}", raffleId);
            return null;
        }

        // Update raffle status to Drawing
        raffle.Status = RaffleStatus.Drawing;
        await _context.SaveChangesAsync();

        // Pick random winner using secure random
        var randomIndex = Random.Shared.Next(confirmedTickets.Count);
        var winningTicket = confirmedTickets[randomIndex];

        // Mark ticket as winner
        winningTicket.Status = TicketStatus.Winner;

        var winner = new Winner
        {
            RaffleId = raffleId,
            TicketId = winningTicket.Id,
            AnnouncedAt = DateTime.UtcNow,
            ContactedWinner = false,
            PrizeDelivered = false
        };

        _context.Winners.Add(winner);

        // Update raffle status
        raffle.Status = RaffleStatus.Completed;
        raffle.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Winner drawn for raffle {RaffleId}: Ticket {TicketNumber}, User {Email}", 
            raffleId, winningTicket.TicketNumber, winningTicket.BuyerEmail);

        return winner;
    }

    /// <summary>
    /// Get winner for a raffle
    /// </summary>
    public async Task<Winner?> GetWinnerAsync(int raffleId)
    {
        return await _context.Winners
            .Include(w => w.Ticket)
            .Include(w => w.Raffle)
            .FirstOrDefaultAsync(w => w.RaffleId == raffleId);
    }

    /// <summary>
    /// Mark winner as contacted
    /// </summary>
    public async Task<bool> MarkWinnerContactedAsync(int winnerId)
    {
        var winner = await _context.Winners.FindAsync(winnerId);
        if (winner == null) return false;

        winner.ContactedWinner = true;
        await _context.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Mark prize as delivered
    /// </summary>
    public async Task<bool> MarkPrizeDeliveredAsync(int winnerId)
    {
        var winner = await _context.Winners.FindAsync(winnerId);
        if (winner == null) return false;

        winner.PrizeDelivered = true;
        await _context.SaveChangesAsync();

        return true;
    }

    #endregion

    #region Statistics

    /// <summary>
    /// Get raffle statistics
    /// </summary>
    public async Task<RaffleStatistics> GetRaffleStatisticsAsync(int raffleId)
    {
        var raffle = await _context.Raffles.FindAsync(raffleId);
        if (raffle == null) return new RaffleStatistics();

        var confirmedTickets = await _context.Tickets
            .Where(t => t.RaffleId == raffleId && t.Status == TicketStatus.Confirmed)
            .ToListAsync();

        return new RaffleStatistics
        {
            TotalTickets = raffle.TotalTickets,
            TicketsSold = raffle.TicketsSold,
            TicketsAvailable = raffle.TicketsAvailable,
            TotalRevenue = confirmedTickets.Sum(t => t.AmountPaid),
            UniqueParticipants = confirmedTickets.Select(t => t.BuyerEmail).Distinct().Count(),
            SoldPercentage = raffle.SoldPercentage
        };
    }

    #endregion
}

/// <summary>
/// Statistics for a raffle
/// </summary>
public class RaffleStatistics
{
    public int TotalTickets { get; set; }
    public int TicketsSold { get; set; }
    public int TicketsAvailable { get; set; }
    public decimal TotalRevenue { get; set; }
    public int UniqueParticipants { get; set; }
    public double SoldPercentage { get; set; }
}
