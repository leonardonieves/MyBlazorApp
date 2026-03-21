using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MyBlazorApp.Api.Services;
using MyBlazorApp.Api.Models;
using MyBlazorApp.Shared.Models;

namespace MyBlazorApp.Api.Controllers;

/// <summary>
/// API controller for admin/sync operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class SyncController : ControllerBase
{
    private readonly StripeSyncService _syncService;
    private readonly RaffleService _raffleService;
    private readonly ILogger<SyncController> _logger;

    public SyncController(
        StripeSyncService syncService,
        RaffleService raffleService,
        ILogger<SyncController> logger)
    {
        _syncService = syncService;
        _raffleService = raffleService;
        _logger = logger;
    }

    /// <summary>
    /// POST: api/sync/stripe
    /// Sync all Stripe products to local raffles
    /// </summary>
    [HttpPost("stripe")]
    public async Task<ActionResult<SyncResponse>> SyncFromStripe()
    {
        try
        {
            _logger.LogInformation("Starting Stripe sync...");

            var raffles = await _syncService.SyncProductsFromStripeAsync();

            return Ok(new SyncResponse
            {
                Success = true,
                Message = $"Synced {raffles.Count} raffles from Stripe",
                SyncedCount = raffles.Count,
                Raffles = raffles.Select(r => new SyncedRaffleInfo
                {
                    Id = r.Id,
                    Title = r.Title,
                    StripeProductId = r.StripeProductId,
                    TotalTickets = r.TotalTickets,
                    TicketPrice = r.TicketPrice
                }).ToList()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing from Stripe");
            return StatusCode(500, new SyncResponse
            {
                Success = false,
                Message = $"Error syncing from Stripe: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// POST: api/sync/generate-tickets/{raffleId}
    /// Generate tickets for a specific raffle
    /// </summary>
    [HttpPost("generate-tickets/{raffleId}")]
    public async Task<ActionResult> GenerateTickets(int raffleId, [FromQuery] int count = 100)
    {
        try
        {
            var raffle = await _raffleService.GetRaffleByIdAsync(raffleId);
            if (raffle == null)
                return NotFound(new { error = "Raffle not found" });

            await _syncService.GenerateTicketsForRaffleAsync(raffleId, count);

            return Ok(new
            {
                success = true,
                message = $"Generated {count} tickets for raffle {raffleId}",
                raffleId,
                ticketCount = count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating tickets for raffle {RaffleId}", raffleId);
            return StatusCode(500, new { error = $"Error generating tickets: {ex.Message}" });
        }
    }

    /// <summary>
    /// GET: api/sync/status
    /// Get sync status and stats
    /// </summary>
    [HttpGet("status")]
    public async Task<ActionResult<SyncStatus>> GetSyncStatus()
    {
        try
        {
            var raffles = await _raffleService.GetActiveRafflesAsync();

            return Ok(new SyncStatus
            {
                TotalRaffles = raffles.Count,
                ActiveRaffles = raffles.Count(r => r.Status == RaffleStatus.Active),
                TotalTicketsGenerated = raffles.Sum(r => r.TotalTickets),
                TotalTicketsSold = raffles.Sum(r => r.TicketsSold),
                LastChecked = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sync status");
            return StatusCode(500, new { error = "Error getting sync status" });
        }
    }

    /// <summary>
    /// GET: api/sync/stripe-products
    /// Debug endpoint: List all products from Stripe with their metadata
    /// </summary>
    [HttpGet("stripe-products")]
    public async Task<ActionResult> GetStripeProducts()
    {
        try
        {
            var products = await _syncService.GetStripeProductsDebugAsync();
            return Ok(products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching Stripe products");
            return StatusCode(500, new { error = $"Error fetching Stripe products: {ex.Message}" });
        }
    }
}

/// <summary>
/// Sync status DTO
/// </summary>
public class SyncStatus
{
    public int TotalRaffles { get; set; }
    public int ActiveRaffles { get; set; }
    public int TotalTicketsGenerated { get; set; }
    public int TotalTicketsSold { get; set; }
    public DateTime LastChecked { get; set; }
}
