using Microsoft.AspNetCore.Mvc;
using MyBlazorApp.Models;
using MyBlazorApp.Models.DTOs;
using MyBlazorApp.Services;

namespace MyBlazorApp.Controllers;

/// <summary>
/// API controller for ticket operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TicketsController : ControllerBase
{
    private readonly RaffleService _raffleService;
    private readonly StripeService _stripeService;
    private readonly ILogger<TicketsController> _logger;
    private readonly IConfiguration _configuration;

    public TicketsController(
        RaffleService raffleService,
        StripeService stripeService,
        ILogger<TicketsController> logger,
        IConfiguration configuration)
    {
        _raffleService = raffleService;
        _stripeService = stripeService;
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// POST: api/tickets/buy
    /// Create a purchase (creates Stripe checkout session)
    /// </summary>
    [HttpPost("buy")]
    public async Task<ActionResult<BuyTicketResponse>> BuyTicket([FromBody] BuyTicketRequest request)
    {
        try
        {
            // Validate request
            if (request.RaffleId <= 0 || request.Quantity <= 0)
                return BadRequest(new BuyTicketResponse
                {
                    Success = false,
                    ErrorMessage = "Invalid raffle ID or quantity"
                });

            if (string.IsNullOrEmpty(request.BuyerEmail))
                return BadRequest(new BuyTicketResponse
                {
                    Success = false,
                    ErrorMessage = "Buyer email is required"
                });

            // Check if raffle exists and has available tickets
            var raffle = await _raffleService.GetRaffleByIdAsync(request.RaffleId);
            if (raffle == null)
                return NotFound(new BuyTicketResponse
                {
                    Success = false,
                    ErrorMessage = "Raffle not found"
                });

            if (!raffle.IsActive || raffle.IsDrawn)
                return BadRequest(new BuyTicketResponse
                {
                    Success = false,
                    ErrorMessage = "Raffle is not active"
                });

            var hasAvailable = await _raffleService.HasAvailableTicketsAsync(request.RaffleId, request.Quantity);
            if (!hasAvailable)
                return BadRequest(new BuyTicketResponse
                {
                    Success = false,
                    ErrorMessage = "Not enough tickets available"
                });

            // Create Stripe checkout session
            var successUrl = string.IsNullOrEmpty(request.SuccessUrl)
                ? $"{Request.Scheme}://{Request.Host}/payment-success?raffleId={raffle.Id}"
                : request.SuccessUrl;

            var cancelUrl = string.IsNullOrEmpty(request.CancelUrl)
                ? $"{Request.Scheme}://{Request.Host}/payment-cancel?raffleId={raffle.Id}"
                : request.CancelUrl;

            var checkoutUrl = await _stripeService.CreateRaffleCheckoutSessionAsync(
                raffleId: raffle.Id,
                quantity: request.Quantity,
                ticketPrice: raffle.TicketPrice,
                raffleName: raffle.Title,
                buyerEmail: request.BuyerEmail,
                buyerName: request.BuyerName,
                successUrl: successUrl,
                cancelUrl: cancelUrl
            );

            _logger.LogInformation($"Created checkout for raffle {raffle.Id}: {request.Quantity} tickets for {request.BuyerEmail}");

            return Ok(new BuyTicketResponse
            {
                Success = true,
                CheckoutUrl = checkoutUrl
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error buying ticket: {ex.Message}");
            return StatusCode(500, new BuyTicketResponse
            {
                Success = false,
                ErrorMessage = $"Error processing purchase: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// GET: api/tickets/my-tickets?email={email}
    /// Get tickets by email
    /// </summary>
    [HttpGet("my-tickets")]
    public async Task<ActionResult<List<Ticket>>> GetMyTickets([FromQuery] string email)
    {
        try
        {
            if (string.IsNullOrEmpty(email))
                return BadRequest(new { error = "Email is required" });

            var tickets = await _raffleService.GetTicketsByEmailAsync(email);
            return Ok(tickets);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting tickets for {email}: {ex.Message}");
            return StatusCode(500, new { error = "Error fetching tickets" });
        }
    }
}
