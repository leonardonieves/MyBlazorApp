using Microsoft.AspNetCore.Mvc;
using MyBlazorApp.Api.Models;
using MyBlazorApp.Api.Models.DTOs;
using MyBlazorApp.Api.Services;
using Stripe;

namespace MyBlazorApp.Api.Controllers;

/// <summary>
/// API controller for raffle operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class RafflesController : ControllerBase
{
    private readonly RaffleService _raffleService;
    private readonly StripeService _stripeService;
    private readonly ILogger<RafflesController> _logger;

    public RafflesController(
        RaffleService raffleService,
        StripeService stripeService,
        ILogger<RafflesController> logger)
    {
        _raffleService = raffleService;
        _stripeService = stripeService;
        _logger = logger;
    }

    /// <summary>
    /// GET: api/raffles
    /// Get all active raffles
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<RaffleDto>>> GetRaffles()
    {
        try
        {
            var raffles = await _raffleService.GetActiveRafflesAsync();
            _logger.LogInformation("GetRaffles: Found {Count} active raffles", raffles.Count);
            var raffleDtos = raffles.Select(r => r.ToDto()).ToList();
            return Ok(raffleDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting raffles");
            return StatusCode(500, new { error = "Error fetching raffles" });
        }
    }

    /// <summary>
    /// GET: api/raffles/all
    /// Get ALL raffles (debug endpoint - no filters)
    /// </summary>
    [HttpGet("all")]
    public async Task<ActionResult<List<RaffleDto>>> GetAllRaffles()
    {
        try
        {
            var raffles = await _raffleService.GetAllRafflesAsync();
            _logger.LogInformation("GetAllRaffles: Found {Count} total raffles", raffles.Count);
            return Ok(raffles.Select(r => r.ToDto()).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all raffles");
            return StatusCode(500, new { error = "Error fetching raffles" });
        }
    }

    /// <summary>
    /// GET: api/raffles/featured
    /// Get featured raffles
    /// </summary>
    [HttpGet("featured")]
    public async Task<ActionResult<List<RaffleDto>>> GetFeaturedRaffles([FromQuery] int count = 3)
    {
        try
        {
            var raffles = await _raffleService.GetFeaturedRafflesAsync(count);
            var raffleDtos = raffles.Select(r => r.ToDto()).ToList();
            return Ok(raffleDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting featured raffles");
            return StatusCode(500, new { error = "Error fetching featured raffles" });
        }
    }

    /// <summary>
    /// GET: api/raffles/{id}
    /// Get raffle details by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<RaffleDto>> GetRaffle(int id)
    {
        try
        {
            var raffle = await _raffleService.GetRaffleByIdAsync(id);

            if (raffle == null)
                return NotFound(new { error = "Raffle not found" });

            return Ok(raffle.ToDto());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting raffle {RaffleId}", id);
            return StatusCode(500, new { error = "Error fetching raffle" });
        }
    }

    /// <summary>
    /// GET: api/raffles/{id}/stats
    /// Get raffle statistics
    /// </summary>
    [HttpGet("{id}/stats")]
    public async Task<ActionResult<RaffleStatistics>> GetRaffleStats(int id)
    {
        try
        {
            var stats = await _raffleService.GetRaffleStatisticsAsync(id);
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting stats for raffle {RaffleId}", id);
            return StatusCode(500, new { error = "Error fetching raffle statistics" });
        }
    }

    /// <summary>
    /// POST: api/raffles/{id}/draw
    /// Draw a winner for the raffle (Admin only - add auth later)
    /// </summary>
    [HttpPost("{id}/draw")]
    public async Task<ActionResult<WinnerDto>> DrawWinner(int id)
    {
        try
        {
            var winner = await _raffleService.DrawWinnerAsync(id);

            if (winner == null)
                return BadRequest(new { error = "Could not draw winner. Check if raffle exists and has confirmed tickets." });

            var winnerDto = new WinnerDto
            {
                Id = winner.Id,
                RaffleId = winner.RaffleId,
                RaffleTitle = winner.Raffle.Title,
                TicketNumber = winner.Ticket.TicketNumber,
                WinnerEmail = winner.Ticket.BuyerEmail,
                WinnerName = winner.Ticket.BuyerName,
                AnnouncedAt = winner.AnnouncedAt
            };

            return Ok(winnerDto);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error drawing winner for raffle {id}: {ex.Message}");
            return StatusCode(500, new { error = "Error drawing winner" });
        }
    }

    /// <summary>
    /// GET: api/raffles/{id}/winner
    /// Get winner for a raffle
    /// </summary>
    [HttpGet("{id}/winner")]
    public async Task<ActionResult<WinnerDto>> GetWinner(int id)
    {
        try
        {
            var winner = await _raffleService.GetWinnerAsync(id);

            if (winner == null)
                return NotFound(new { error = "No winner announced yet" });

            var winnerDto = new WinnerDto
            {
                Id = winner.Id,
                RaffleId = winner.RaffleId,
                RaffleTitle = winner.Raffle.Title,
                TicketNumber = winner.Ticket.TicketNumber,
                WinnerEmail = winner.Ticket.BuyerEmail,
                WinnerName = winner.Ticket.BuyerName,
                AnnouncedAt = winner.AnnouncedAt
            };

            return Ok(winnerDto);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting winner for raffle {id}: {ex.Message}");
            return StatusCode(500, new { error = "Error fetching winner" });
        }
    }
}
