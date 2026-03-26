using Microsoft.AspNetCore.Mvc;
using MyBlazorApp.Api.Services;

namespace MyBlazorApp.Api.Controllers;

/// <summary>
/// Testing controller for SignalR functionality
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TestingController : ControllerBase
{
    private readonly SignalRNotificationService _signalR;
    private readonly ILogger<TestingController> _logger;

    public TestingController(
        SignalRNotificationService signalR,
        ILogger<TestingController> logger)
    {
        _signalR = signalR;
        _logger = logger;
    }

    /// <summary>
    /// GET: api/testing/ping
    /// Simple ping to verify API is running
    /// </summary>
    [HttpGet("ping")]
    public IActionResult Ping()
    {
        _logger.LogInformation("Ping received at {Time}", DateTime.UtcNow);
        return Ok(new { message = "Pong", timestamp = DateTime.UtcNow });
    }

    /// <summary>
    /// POST: api/testing/broadcast-raffles-updated
    /// Manually trigger RafflesUpdated event for testing
    /// </summary>
    [HttpPost("broadcast-raffles-updated")]
    public async Task<IActionResult> BroadcastRafflesUpdated()
    {
        try
        {
            _logger.LogInformation("Broadcasting RafflesUpdated event at {Time}", DateTime.UtcNow);
            await _signalR.NotifyRafflesUpdatedAsync();
            return Ok(new { message = "RafflesUpdated event broadcast successfully", timestamp = DateTime.UtcNow });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting RafflesUpdated");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// POST: api/testing/broadcast-raffle-updated/{raffleId}
    /// Manually trigger RaffleUpdated event for a specific raffle
    /// </summary>
    [HttpPost("broadcast-raffle-updated/{raffleId}")]
    public async Task<IActionResult> BroadcastRaffleUpdated(int raffleId)
    {
        try
        {
            _logger.LogInformation("Broadcasting RaffleUpdated event for raffle {RaffleId} at {Time}", raffleId, DateTime.UtcNow);
            await _signalR.NotifyRaffleUpdatedAsync(raffleId);
            return Ok(new { message = $"RaffleUpdated event broadcast for raffle {raffleId}", timestamp = DateTime.UtcNow });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting RaffleUpdated for raffle {RaffleId}", raffleId);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// POST: api/testing/broadcast-tickets-sold/{raffleId}
    /// Manually trigger TicketsSold event
    /// </summary>
    [HttpPost("broadcast-tickets-sold/{raffleId}")]
    public async Task<IActionResult> BroadcastTicketsSold(int raffleId, [FromQuery] int ticketsSold = 5, [FromQuery] int ticketsAvailable = 95)
    {
        try
        {
            _logger.LogInformation("Broadcasting TicketsSold event for raffle {RaffleId} at {Time}", raffleId, DateTime.UtcNow);
            await _signalR.NotifyTicketsSoldAsync(raffleId, ticketsSold, ticketsAvailable);
            return Ok(new { message = $"TicketsSold event broadcast for raffle {raffleId}", ticketsSold, ticketsAvailable, timestamp = DateTime.UtcNow });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting TicketsSold for raffle {RaffleId}", raffleId);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// POST: api/testing/broadcast-winner-drawn/{raffleId}
    /// Manually trigger WinnerDrawn event
    /// </summary>
    [HttpPost("broadcast-winner-drawn/{raffleId}")]
    public async Task<IActionResult> BroadcastWinnerDrawn(int raffleId)
    {
        try
        {
            _logger.LogInformation("Broadcasting WinnerDrawn event for raffle {RaffleId} at {Time}", raffleId, DateTime.UtcNow);
            await _signalR.NotifyWinnerDrawnAsync(raffleId);
            return Ok(new { message = $"WinnerDrawn event broadcast for raffle {raffleId}", timestamp = DateTime.UtcNow });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting WinnerDrawn for raffle {RaffleId}", raffleId);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// GET: api/testing/signalr-status
    /// Check SignalR health status
    /// </summary>
    [HttpGet("signalr-status")]
    public IActionResult SignalRStatus()
    {
        return Ok(new
        {
            status = "running",
            hubUrl = "/rafflehub",
            timestamp = DateTime.UtcNow,
            message = "SignalR hub is available at /rafflehub"
        });
    }
}
