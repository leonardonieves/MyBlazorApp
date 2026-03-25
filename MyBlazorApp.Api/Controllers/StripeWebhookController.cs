using Microsoft.AspNetCore.Mvc;
using MyBlazorApp.Api.Services;

namespace MyBlazorApp.Api.Controllers;

/// <summary>
/// Webhook endpoint for Stripe events
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class StripeWebhookController : ControllerBase
{
    private readonly StripeWebhookService _webhookService;
    private readonly ILogger<StripeWebhookController> _logger;

    public StripeWebhookController(
        StripeWebhookService webhookService,
        ILogger<StripeWebhookController> logger)
    {
        _webhookService = webhookService;
        _logger = logger;
    }

    /// <summary>
    /// Receive and process Stripe webhook events
    /// POST: api/stripewebhook
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> HandleWebhook()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        var signatureHeader = Request.Headers["Stripe-Signature"];

        _logger.LogInformation($"Received Stripe webhook event");

        var success = await _webhookService.ProcessWebhookEventAsync(json, signatureHeader);

        if (success)
        {
            _logger.LogInformation("Webhook processed successfully");
            return Ok(new { received = true });
        }

        _logger.LogWarning("Webhook processing failed");
        return BadRequest(new { received = false });
    }
}
