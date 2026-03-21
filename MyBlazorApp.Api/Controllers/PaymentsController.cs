using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MyBlazorApp.Api.Services;
using MyBlazorApp.Shared.Models;
using System.Security.Claims;

namespace MyBlazorApp.Api.Controllers;

/// <summary>
/// API controller for payment operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly PaymentService _paymentService;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(
        PaymentService paymentService,
        ILogger<PaymentsController> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }

    /// <summary>
    /// GET: api/payments/my
    /// Get current user's payments
    /// </summary>
    [HttpGet("my")]
    public async Task<ActionResult<List<PaymentDto>>> GetMyPayments()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { error = "Invalid user" });
            }

            var payments = await _paymentService.GetUserPaymentsAsync(userId);
            
            var paymentDtos = payments.Select(p => new PaymentDto
            {
                Id = p.Id,
                ProductName = p.ProductName,
                Amount = p.Amount,
                Currency = p.Currency ?? "usd",
                Status = p.Status,
                CreatedAt = p.CreatedAt,
                Quantity = p.Quantity
            }).ToList();

            return Ok(paymentDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user payments");
            return StatusCode(500, new { error = "Error fetching payments" });
        }
    }

    /// <summary>
    /// GET: api/payments/{id}
    /// Get payment details by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<PaymentDto>> GetPayment(int id)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { error = "Invalid user" });
            }

            var payments = await _paymentService.GetUserPaymentsAsync(userId);
            var payment = payments.FirstOrDefault(p => p.Id == id);
            
            if (payment == null)
                return NotFound(new { error = "Payment not found" });

            return Ok(new PaymentDto
            {
                Id = payment.Id,
                ProductName = payment.ProductName,
                Amount = payment.Amount,
                Currency = payment.Currency ?? "usd",
                Status = payment.Status,
                CreatedAt = payment.CreatedAt,
                Quantity = payment.Quantity
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment {PaymentId}", id);
            return StatusCode(500, new { error = "Error fetching payment" });
        }
    }
}
