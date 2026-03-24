using Microsoft.AspNetCore.Mvc;
using MyBlazorApp.Api.Models;
using MyBlazorApp.Api.Models.DTOs;
using MyBlazorApp.Api.Services;
using SharedModels = MyBlazorApp.Shared.Models;

namespace MyBlazorApp.Api.Controllers;

/// <summary>
/// API controller for ticket operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TicketsController : ControllerBase
{
    private readonly RaffleService _raffleService;
    private readonly StripeService _stripeService;
    private readonly StripeSyncService _syncService;
    private readonly ILogger<TicketsController> _logger;
    private readonly IConfiguration _configuration;

    public TicketsController(
        RaffleService raffleService,
        StripeService stripeService,
        StripeSyncService syncService,
        ILogger<TicketsController> logger,
        IConfiguration configuration)
    {
        _raffleService = raffleService;
        _stripeService = stripeService;
        _syncService = syncService;
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

            if (!raffle.CanPurchase)
                return BadRequest(new BuyTicketResponse
                {
                    Success = false,
                    ErrorMessage = "Raffle is not available for purchase"
                });

            // Check user purchase limits if logged in
            if (request.UserId > 0)
            {
                var (canPurchase, remainingAllowed) = await _raffleService.CanUserPurchaseAsync(
                    request.RaffleId, request.UserId, request.Quantity);

                if (!canPurchase)
                    return BadRequest(new BuyTicketResponse
                    {
                        Success = false,
                        ErrorMessage = remainingAllowed <= 0
                            ? $"You have reached the maximum of {raffle.MaxTicketsPerUser} tickets for this raffle"
                            : "Not enough tickets available"
                    });
            }
            else
            {
                var hasAvailable = await _raffleService.HasAvailableTicketsAsync(request.RaffleId, request.Quantity);
                if (!hasAvailable)
                    return BadRequest(new BuyTicketResponse
                    {
                        Success = false,
                        ErrorMessage = "Not enough tickets available"
                    });
            }

            // Create Stripe checkout session
            var successUrl = string.IsNullOrEmpty(request.SuccessUrl)
                ? $"{Request.Scheme}://{Request.Host}/payment-success?raffleId={raffle.Id}"
                : request.SuccessUrl;

            var cancelUrl = string.IsNullOrEmpty(request.CancelUrl)
                ? $"{Request.Scheme}://{Request.Host}/payment-cancel?raffleId={raffle.Id}"
                : request.CancelUrl;

            var checkoutUrl = await _stripeService.CreateRaffleCheckoutSessionAsync(
                raffleId: raffle.Id,
                userId: request.UserId,
                quantity: request.Quantity,
                ticketPrice: raffle.TicketPrice,
                raffleName: raffle.Title,
                buyerEmail: request.BuyerEmail,
                buyerName: request.BuyerName,
                stripeCustomerId: request.StripeCustomerId,
                stripePriceId: raffle.StripePriceId,
                successUrl: successUrl,
                cancelUrl: cancelUrl
            );

            _logger.LogInformation("Created checkout for raffle {RaffleId}: {Quantity} tickets for {BuyerEmail}, User {UserId}",
                raffle.Id, request.Quantity, request.BuyerEmail, request.UserId);

            return Ok(new BuyTicketResponse
            {
                Success = true,
                CheckoutUrl = checkoutUrl
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error buying ticket");
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
    public async Task<ActionResult<List<TicketDto>>> GetMyTickets([FromQuery] string email)
    {
        try
        {
            if (string.IsNullOrEmpty(email))
                return BadRequest(new { error = "Email is required" });

            var tickets = await _raffleService.GetTicketsByEmailAsync(email);
            return Ok(tickets.Select(t => t.ToDto()).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tickets for {Email}", email);
            return StatusCode(500, new { error = "Error fetching tickets" });
        }
    }

    /// <summary>
    /// GET: api/tickets/user/{userId}
    /// Get tickets by user ID
    /// </summary>
    [HttpGet("user/{userId}")]
    public async Task<ActionResult<List<TicketDto>>> GetTicketsByUserId(int userId)
    {
        try
        {
            var tickets = await _raffleService.GetTicketsByUserIdAsync(userId);
            return Ok(tickets.Select(t => t.ToDto()).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tickets for user {UserId}", userId);
            return StatusCode(500, new { error = "Error fetching tickets" });
        }
    }

    /// <summary>
    /// GET: api/tickets/raffle/{raffleId}
    /// Get confirmed tickets for a raffle (for draw)
    /// </summary>
    [HttpGet("raffle/{raffleId}")]
    public async Task<ActionResult<List<TicketDto>>> GetTicketsForRaffle(int raffleId)
    {
        try
        {
            var tickets = await _raffleService.GetConfirmedTicketsAsync(raffleId);
            return Ok(tickets.Select(t => t.ToDto()).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tickets for raffle {RaffleId}", raffleId);
            return StatusCode(500, new { error = "Error fetching tickets" });
        }
    }

    /// <summary>
    /// GET: api/tickets/status/{raffleId}
    /// Get ticket statuses for the visual selector
    /// </summary>
    [HttpGet("status/{raffleId}")]
    public async Task<ActionResult<List<TicketStatusDto>>> GetTicketStatuses(int raffleId)
    {
        try
        {
            var statuses = await _syncService.GetTicketStatusesAsync(raffleId);
            return Ok(statuses.Select(s => new TicketStatusDto
            {
                TicketId = s.TicketId,
                RaffleId = s.RaffleId,
                DisplayNumber = s.DisplayNumber,
                Status = s.Status,
                ReservedByUserId = s.ReservedByUserId,
                ExpiresAt = s.ExpiresAt
            }).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting ticket statuses for raffle {RaffleId}", raffleId);
            return StatusCode(500, new { error = "Error fetching ticket statuses" });
        }
    }

    /// <summary>
    /// POST: api/tickets/reserve
    /// Reserve selected tickets for checkout
    /// </summary>
    [HttpPost("reserve")]
    public async Task<ActionResult<ReserveTicketsResult>> ReserveTickets([FromBody] ReserveTicketsRequest request)
    {
        try
        {
            var result = await _syncService.ReserveTicketsAsync(
                request.RaffleId, 
                request.UserId, 
                request.TicketIds);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reserving tickets");
            return StatusCode(500, new ReserveTicketsResult
            {
                Success = false,
                Message = "Error reserving tickets"
            });
        }
    }

    /// <summary>
    /// POST: api/tickets/release
    /// Release reserved tickets
    /// </summary>
    [HttpPost("release")]
    public async Task<ActionResult> ReleaseTickets([FromBody] ReleaseTicketsRequest request)
    {
        try
        {
            await _syncService.ReleaseReservationAsync(
                request.RaffleId, 
                request.UserId, 
                request.TicketIds);
            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error releasing tickets");
            return StatusCode(500, new { error = "Error releasing tickets" });
        }
    }

    /// <summary>
    /// POST: api/tickets/buy-selected
    /// Buy specific pre-selected tickets
    /// </summary>
    [HttpPost("buy-selected")]
    public async Task<ActionResult<BuyTicketResponse>> BuySelectedTickets([FromBody] BuyTicketWithSelectionRequest request)
    {
        try
        {
            // Validate request
            if (request.RaffleId <= 0 || !request.TicketIds.Any())
                return BadRequest(new BuyTicketResponse
                {
                    Success = false,
                    ErrorMessage = "Invalid raffle ID or no tickets selected"
                });

            if (string.IsNullOrEmpty(request.BuyerEmail))
                return BadRequest(new BuyTicketResponse
                {
                    Success = false,
                    ErrorMessage = "Buyer email is required"
                });

            // Get raffle
            var raffle = await _raffleService.GetRaffleByIdAsync(request.RaffleId);
            if (raffle == null)
                return NotFound(new BuyTicketResponse
                {
                    Success = false,
                    ErrorMessage = "Raffle not found"
                });

            if (!raffle.CanPurchase)
                return BadRequest(new BuyTicketResponse
                {
                    Success = false,
                    ErrorMessage = "Raffle is not available for purchase"
                });

            // Check user purchase limits
            if (request.UserId > 0 && raffle.MaxTicketsPerUser > 0)
            {
                var (canPurchase, remainingAllowed) = await _raffleService.CanUserPurchaseAsync(
                    request.RaffleId, request.UserId, request.TicketIds.Count);

                if (!canPurchase)
                    return BadRequest(new BuyTicketResponse
                    {
                        Success = false,
                        ErrorMessage = remainingAllowed <= 0
                            ? $"You have reached the maximum of {raffle.MaxTicketsPerUser} tickets for this raffle"
                            : "Cannot purchase that many tickets"
                    });
            }

            // Reserve the tickets first (if not already reserved)
            var reserveResult = await _syncService.ReserveTicketsAsync(
                request.RaffleId, 
                request.UserId, 
                request.TicketIds);

            if (!reserveResult.Success)
            {
                return BadRequest(new BuyTicketResponse
                {
                    Success = false,
                    ErrorMessage = reserveResult.Message
                });
            }

            // Calculate total
            var totalAmount = raffle.TicketPrice * request.TicketIds.Count;

            // Create Stripe checkout session with ticket IDs in metadata
            var successUrl = string.IsNullOrEmpty(request.SuccessUrl)
                ? $"{Request.Scheme}://{Request.Host}/payment-success?raffleId={raffle.Id}"
                : request.SuccessUrl;

            var cancelUrl = string.IsNullOrEmpty(request.CancelUrl)
                ? $"{Request.Scheme}://{Request.Host}/payment-cancel?raffleId={raffle.Id}"
                : request.CancelUrl;

            var checkoutUrl = await _stripeService.CreateRaffleCheckoutSessionWithTicketsAsync(
                raffleId: raffle.Id,
                userId: request.UserId,
                ticketIds: request.TicketIds,
                ticketPrice: raffle.TicketPrice,
                raffleName: raffle.Title,
                buyerEmail: request.BuyerEmail,
                buyerName: request.BuyerName,
                stripeCustomerId: request.StripeCustomerId,
                stripePriceId: raffle.StripePriceId,
                successUrl: successUrl,
                cancelUrl: cancelUrl
            );

            _logger.LogInformation(
                "Created checkout for raffle {RaffleId}: tickets [{TicketIds}] for {BuyerEmail}, User {UserId}",
                raffle.Id, string.Join(",", request.TicketIds), request.BuyerEmail, request.UserId);

            return Ok(new BuyTicketResponse
            {
                Success = true,
                CheckoutUrl = checkoutUrl
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error buying selected tickets");
            return StatusCode(500, new BuyTicketResponse
            {
                Success = false,
                ErrorMessage = $"Error processing purchase: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// POST: api/tickets/verify-payment
    /// Verify a Stripe checkout session and process tickets if payment is complete.
    /// Safety net when webhooks are delayed or missed.
    /// </summary>
    [HttpPost("verify-payment")]
    public async Task<ActionResult<VerifyPaymentResult>> VerifyPayment([FromBody] VerifyPaymentRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.SessionId))
                return BadRequest(new VerifyPaymentResult { Success = false, Message = "Session ID is required" });

            _logger.LogInformation("Verifying payment for session {SessionId}", request.SessionId);

            var result = await _syncService.VerifyAndProcessPaymentAsync(request.SessionId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying payment for session {SessionId}", request.SessionId);
            return StatusCode(500, new VerifyPaymentResult
            {
                Success = false,
                Message = "Error verifying payment"
            });
        }
    }
}

/// <summary>
/// Request to reserve tickets
/// </summary>
public class ReserveTicketsRequest
{
    public int RaffleId { get; set; }
    public int UserId { get; set; }
    public List<int> TicketIds { get; set; } = new();
}

/// <summary>
/// Request to release tickets
/// </summary>
public class ReleaseTicketsRequest
{
    public int RaffleId { get; set; }
    public int UserId { get; set; }
    public List<int>? TicketIds { get; set; }
}

/// <summary>
/// Request to verify a Stripe payment session
/// </summary>
public class VerifyPaymentRequest
{
    public string SessionId { get; set; } = string.Empty;
}
