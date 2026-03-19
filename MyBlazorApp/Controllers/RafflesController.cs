using Microsoft.AspNetCore.Mvc;
using MyBlazorApp.Models;
using MyBlazorApp.Models.DTOs;
using MyBlazorApp.Services;
using Stripe;

namespace MyBlazorApp.Controllers;

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

            var raffleDtos = raffles.Select(r => new RaffleDto
            {
                Id = r.Id,
                Title = r.Title,
                Description = r.Description,
                PrizeDetails = r.PrizeDetails,
                TicketPrice = r.TicketPrice,
                TotalTickets = r.TotalTickets,
                TicketsSold = r.TicketsSold,
                TicketsAvailable = r.TotalTickets - r.TicketsSold,
                ImageUrl = r.ImageUrl,
                DrawDate = r.DrawDate,
                IsActive = r.IsActive,
                IsDrawn = r.IsDrawn
            }).ToList();

            return Ok(raffleDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting raffles: {ex.Message}");
            return StatusCode(500, new { error = "Error fetching raffles" });
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

            var raffleDto = new RaffleDto
            {
                Id = raffle.Id,
                Title = raffle.Title,
                Description = raffle.Description,
                PrizeDetails = raffle.PrizeDetails,
                TicketPrice = raffle.TicketPrice,
                TotalTickets = raffle.TotalTickets,
                TicketsSold = raffle.TicketsSold,
                TicketsAvailable = raffle.TotalTickets - raffle.TicketsSold,
                ImageUrl = raffle.ImageUrl,
                DrawDate = raffle.DrawDate,
                IsActive = raffle.IsActive,
                IsDrawn = raffle.IsDrawn
            };

            return Ok(raffleDto);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting raffle {id}: {ex.Message}");
            return StatusCode(500, new { error = "Error fetching raffle" });
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
