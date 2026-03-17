using Stripe;
using Stripe.Checkout;

namespace MyBlazorApp.Services;

/// <summary>
/// Service to handle Stripe operations
/// </summary>
public class StripeService
{
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;

    public StripeService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;

        // Configure Stripe secret key
        var secretKey = _configuration["Stripe:SecretKey"];
        StripeConfiguration.ApiKey = secretKey;
    }

    /// <summary>
    /// Get Stripe public key
    /// </summary>
    public string GetPublishableKey()
    {
        return _configuration["Stripe:PublishableKey"] ?? string.Empty;
    }

    /// <summary>
    /// Create a Stripe Checkout session using Price ID
    /// </summary>
    public async Task<string> CreateCheckoutSessionAsync(string priceId, int quantity, string successUrl, string cancelUrl)
    {
        try
        {
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        Price = priceId,  // Use existing Price ID from Stripe
                        Quantity = quantity,
                    },
                },
                Mode = "payment",
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl,
            };

            var service = new Stripe.Checkout.SessionService();
            var session = await service.CreateAsync(options);
            return session.Id;
        }
        catch (StripeException ex) 
        {
            throw new InvalidOperationException($"Error creating Stripe session: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Get all products from Stripe
    /// </summary>
    public async Task<List<Product>> GetProductsAsync()
    {
        try
        {
            var options = new ProductListOptions { Limit = 100, Active = true };
            var service = new ProductService();
            var products = await service.ListAsync(options);
            return products.Data.ToList();
        }
        catch (StripeException ex)
        {
            throw new InvalidOperationException($"Error fetching products: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Get prices for a specific product
    /// </summary>
    public async Task<List<Price>> GetPricesForProductAsync(string productId)
    {
        try
        {
            var options = new PriceListOptions 
            { 
                Product = productId,
                Limit = 100,
                Active = true
            };
            var service = new PriceService();
            var prices = await service.ListAsync(options);
            return prices.Data.ToList();
        }
        catch (StripeException ex)
        {
            throw new InvalidOperationException($"Error fetching prices: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Create a Stripe Checkout session with amount (legacy method)
    /// </summary>
    public async Task<string> CreateCheckoutSessionWithAmountAsync(decimal amount, string productName, string successUrl, string cancelUrl)
    {
        try
        {
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(amount * 100),
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = productName,
                            },
                        },
                        Quantity = 1,
                    },
                },
                Mode = "payment",
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl,
            };

            var service = new Stripe.Checkout.SessionService();
            var session = await service.CreateAsync(options);
            return session.Id;
        }
        catch (StripeException ex) 
        {
            throw new InvalidOperationException($"Error creating Stripe session: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Retrieve a Stripe session by ID
    /// </summary>
    public async Task<Session> GetSessionAsync(string sessionId)
    {
        try
        {
            var service = new Stripe.Checkout.SessionService();
            return await service.GetAsync(sessionId);
        }
        catch (StripeException ex)
        {
            throw new InvalidOperationException($"Error retrieving Stripe session: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Create a Payment Intent for custom payments
    /// </summary>
    public async Task<PaymentIntent> CreatePaymentIntentAsync(decimal amount, string description = "")
    {
        try
        {
            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(amount * 100), // Convert to cents
                Currency = "usd",
                Description = description,
                PaymentMethodTypes = new List<string> { "card" },
            };

            var service = new PaymentIntentService();
            return await service.CreateAsync(options);
        }
        catch (StripeException ex)
        {
            throw new InvalidOperationException($"Error creating PaymentIntent: {ex.Message}", ex);
        }
    }
}
