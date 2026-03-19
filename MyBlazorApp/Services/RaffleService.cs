using Microsoft.EntityFrameworkCore;
using MyBlazorApp.Data;
using MyBlazorApp.Models;

namespace MyBlazorApp.Services;

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

    /// <summary>
    /// Get all active raffles
    /// </summary>
    public async Task<List<Raffle>> GetActiveRafflesAsync()
    {
        return await _context.Raffles
            .Where(r => r.IsActive && !r.IsDrawn && r.DrawDate > DateTime.UtcNow)
            .Include(r => r.Tickets)
            .OrderBy(r => r.DrawDate)
            .ToListAsync();
    }

    /// <summary>
    /// Get raffle by ID
    /// </summary>
    public async Task<Raffle?> GetRaffleByIdAsync(int id)
    {
        return await _context.Raffles
            .Include(r => r.Tickets)
            .Include(r => r.Winner)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    /// <summary>
    /// Create a new raffle
    /// </summary>
    public async Task<Raffle> CreateRaffleAsync(Raffle raffle)
    {
        raffle.CreatedAt = DateTime.UtcNow;
        raffle.UpdatedAt = DateTime.UtcNow;
        raffle.IsActive = true;
        raffle.IsDrawn = false;
        raffle.TicketsSold = 0;

        _context.Raffles.Add(raffle);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"Created raffle: {raffle.Id} - {raffle.Title}");
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

        _logger.LogInformation($"Updated raffle: {raffle.Id}");
        return raffle;
    }

    /// <summary>
    /// Get available tickets count for a raffle
    /// </summary>
    public async Task<int> GetAvailableTicketsCountAsync(int raffleId)
    {
        var raffle = await GetRaffleByIdAsync(raffleId);
        if (raffle == null) return 0;

        return raffle.TotalTickets - raffle.TicketsSold;
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
    /// Generate unique ticket number
    /// </summary>
    public async Task<string> GenerateTicketNumberAsync(int raffleId)
    {
        var raffle = await GetRaffleByIdAsync(raffleId);
        if (raffle == null)
            throw new InvalidOperationException("Raffle not found");

        string ticketNumber;
        bool exists;

        do
        {
            // Format: RAFFLE{ID}-TICKET{RANDOM}
            ticketNumber = $"R{raffleId:D4}-T{Random.Shared.Next(100000, 999999)}";
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
        ticket.Status = "pending";

        _context.Tickets.Add(ticket);

        // Update raffle tickets sold count
        var raffle = await _context.Raffles.FindAsync(ticket.RaffleId);
        if (raffle != null)
        {
            raffle.TicketsSold++;
            raffle.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation($"Created ticket: {ticket.TicketNumber} for raffle {ticket.RaffleId}");
        return ticket;
    }

    /// <summary>
    /// Confirm ticket payment
    /// </summary>
    public async Task<Ticket?> ConfirmTicketPaymentAsync(string paymentIntentId)
    {
        var ticket = await _context.Tickets
            .FirstOrDefaultAsync(t => t.StripePaymentIntentId == paymentIntentId);

        if (ticket != null)
        {
            ticket.Status = "confirmed";
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Confirmed ticket payment: {ticket.TicketNumber}");
        }

        return ticket;
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
    /// Draw a winner for a raffle
    /// </summary>
    public async Task<Winner?> DrawWinnerAsync(int raffleId)
    {
        var raffle = await _context.Raffles
            .Include(r => r.Tickets.Where(t => t.Status == "confirmed"))
            .Include(r => r.Winner)
            .FirstOrDefaultAsync(r => r.Id == raffleId);

        if (raffle == null)
        {
            _logger.LogWarning($"Raffle not found: {raffleId}");
            return null;
        }

        if (raffle.Winner != null)
        {
            _logger.LogWarning($"Raffle already has a winner: {raffleId}");
            return raffle.Winner;
        }

        var confirmedTickets = raffle.Tickets.Where(t => t.Status == "confirmed").ToList();

        if (!confirmedTickets.Any())
        {
            _logger.LogWarning($"No confirmed tickets for raffle: {raffleId}");
            return null;
        }

        // Pick random winner
        var randomIndex = Random.Shared.Next(confirmedTickets.Count);
        var winningTicket = confirmedTickets[randomIndex];

        var winner = new Winner
        {
            RaffleId = raffleId,
            TicketId = winningTicket.Id,
            AnnouncedAt = DateTime.UtcNow,
            ContactedWinner = false,
            PrizeDelivered = false
        };

        _context.Winners.Add(winner);

        raffle.IsDrawn = true;
        raffle.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation($"Winner drawn for raffle {raffleId}: Ticket {winningTicket.TicketNumber}");

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
}
