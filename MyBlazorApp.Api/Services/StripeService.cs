using Stripe;
using Stripe.Checkout;

namespace MyBlazorApp.Api.Services;

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
                        Price = priceId,
                        Quantity = quantity,
                    },
                },
                Mode = "payment",
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl,
            };

            var service = new Stripe.Checkout.SessionService();
            var session = await service.CreateAsync(options);

            Console.WriteLine($"Session Created: {session.Id}");
            Console.WriteLine($"Session Status: {session.Status}");
            Console.WriteLine($"Session URL: {session.Url}");
            Console.WriteLine($"================================");

            // Return the Stripe-generated URL, not manually constructed one
            return session.Url;
        }
        catch (StripeException ex) 
        {
            Console.WriteLine($"STRIPE ERROR: {ex.Message}");
            Console.WriteLine($"Error Code: {ex.StripeError?.Code}");
            Console.WriteLine($"Error Type: {ex.StripeError?.Type}");
            Console.WriteLine($"Error Param: {ex.StripeError?.Param}");
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
    /// Create a Stripe Checkout session for raffle tickets with metadata
    /// </summary>
    public async Task<string> CreateRaffleCheckoutSessionAsync(
        int raffleId,
        int userId,
        int quantity,
        decimal ticketPrice,
        string raffleName,
        string buyerEmail,
        string? buyerName,
        string? stripeCustomerId,
        string? stripePriceId,
        string successUrl,
        string cancelUrl)
    {
        try
        {
            var lineItems = new List<SessionLineItemOptions>();

            // Use existing Stripe Price if available, otherwise create inline price
            if (!string.IsNullOrEmpty(stripePriceId))
            {
                lineItems.Add(new SessionLineItemOptions
                {
                    Price = stripePriceId,
                    Quantity = quantity,
                });
            }
            else
            {
                lineItems.Add(new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(ticketPrice * 100), // Convert to cents
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = $"{raffleName} - Raffle Ticket",
                            Description = $"Ticket(s) for {raffleName}",
                        },
                    },
                    Quantity = quantity,
                });
            }

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = lineItems,
                Mode = "payment",
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl,
                Metadata = new Dictionary<string, string>
                {
                    { "raffle_id", raffleId.ToString() },
                    { "user_id", userId.ToString() },
                    { "quantity", quantity.ToString() },
                    { "buyer_email", buyerEmail },
                    { "buyer_name", buyerName ?? "" }
                }
            };

            // Use existing Stripe Customer if available
            if (!string.IsNullOrEmpty(stripeCustomerId))
            {
                options.Customer = stripeCustomerId;
            }
            else
            {
                options.CustomerEmail = buyerEmail;
            }

            var service = new Stripe.Checkout.SessionService();
            var session = await service.CreateAsync(options);

            Console.WriteLine($"Raffle Session Created: {session.Id}");
            Console.WriteLine($"Raffle ID: {raffleId}, User ID: {userId}, Quantity: {quantity}");
            Console.WriteLine($"Session URL: {session.Url}");
            Console.WriteLine($"================================");

            return session.Url;
        }
        catch (StripeException ex)
        {
            Console.WriteLine($"STRIPE ERROR: {ex.Message}");
            throw new InvalidOperationException($"Error creating raffle checkout session: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Create a Stripe Product for a raffle
    /// </summary>
    public async Task<Product> CreateRaffleProductAsync(int raffleId, string raffleName, string description, string? imageUrl = null)
    {
        try
        {
            var options = new ProductCreateOptions
            {
                Name = $"{raffleName} - Raffle Ticket",
                Description = description,
                Metadata = new Dictionary<string, string>
                {
                    { "raffle_id", raffleId.ToString() },
                    { "type", "raffle_ticket" }
                }
            };

            if (!string.IsNullOrEmpty(imageUrl))
            {
                options.Images = new List<string> { imageUrl };
            }

            var service = new ProductService();
            var product = await service.CreateAsync(options);

            Console.WriteLine($"Stripe Product Created: {product.Id} for Raffle {raffleId}");
            return product;
        }
        catch (StripeException ex)
        {
            throw new InvalidOperationException($"Error creating Stripe product: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Create a Stripe Price for a raffle ticket
    /// </summary>
    public async Task<Price> CreateRafflePriceAsync(string productId, decimal ticketPrice)
    {
        try
        {
            var options = new PriceCreateOptions
            {
                Product = productId,
                UnitAmount = (long)(ticketPrice * 100), // Convert to cents
                Currency = "usd",
            };

            var service = new PriceService();
            var price = await service.CreateAsync(options);

            Console.WriteLine($"Stripe Price Created: {price.Id} for Product {productId}");
            return price;
        }
        catch (StripeException ex)
        {
            throw new InvalidOperationException($"Error creating Stripe price: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Archive a Stripe Product (when raffle is completed)
    /// </summary>
    public async Task<bool> ArchiveProductAsync(string productId)
    {
        try
        {
            var options = new ProductUpdateOptions
            {
                Active = false
            };

            var service = new ProductService();
            await service.UpdateAsync(productId, options);
            return true;
        }
        catch (StripeException ex)
        {
            Console.WriteLine($"Error archiving product: {ex.Message}");
            return false;
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

    /// <summary>
    /// Create a Stripe Customer
    /// </summary>
    public async Task<Customer> CreateCustomerAsync(string email, string name, Dictionary<string, string>? metadata = null)
    {
        try
        {
            var options = new CustomerCreateOptions
            {
                Email = email,
                Name = name,
                Metadata = metadata ?? new Dictionary<string, string>()
            };

            var service = new CustomerService();
            var customer = await service.CreateAsync(options);

            Console.WriteLine($"Stripe Customer Created: {customer.Id}");
            Console.WriteLine($"Email: {email}, Name: {name}");
            Console.WriteLine($"================================");

            return customer;
        }
        catch (StripeException ex)
        {
            Console.WriteLine($"STRIPE ERROR creating customer: {ex.Message}");
            throw new InvalidOperationException($"Error creating Stripe customer: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Get a Stripe Customer by ID
    /// </summary>
    public async Task<Customer?> GetCustomerAsync(string customerId)
    {
        try
        {
            var service = new CustomerService();
            return await service.GetAsync(customerId);
        }
        catch (StripeException ex)
        {
            Console.WriteLine($"Error getting customer: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Update a Stripe Customer
    /// </summary>
    public async Task<Customer> UpdateCustomerAsync(string customerId, string? email = null, string? name = null, Dictionary<string, string>? metadata = null)
    {
        try
        {
            var options = new CustomerUpdateOptions();

            if (!string.IsNullOrEmpty(email))
                options.Email = email;

            if (!string.IsNullOrEmpty(name))
                options.Name = name;

            if (metadata != null)
                options.Metadata = metadata;

            var service = new CustomerService();
            return await service.UpdateAsync(customerId, options);
        }
        catch (StripeException ex)
        {
            throw new InvalidOperationException($"Error updating Stripe customer: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Delete a Stripe Customer
    /// </summary>
    public async Task<bool> DeleteCustomerAsync(string customerId)
    {
        try
        {
            var service = new CustomerService();
            await service.DeleteAsync(customerId);
            return true;
        }
        catch (StripeException ex)
        {
            Console.WriteLine($"Error deleting customer: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Create a Stripe Checkout session for specific pre-selected raffle tickets
    /// </summary>
    public async Task<string> CreateRaffleCheckoutSessionWithTicketsAsync(
        int raffleId,
        int userId,
        List<int> ticketIds,
        decimal ticketPrice,
        string raffleName,
        string buyerEmail,
        string? buyerName,
        string? stripeCustomerId,
        string? stripePriceId,
        string successUrl,
        string cancelUrl)
    {
        try
        {
            var quantity = ticketIds.Count;
            var lineItems = new List<SessionLineItemOptions>();

            // Use existing Stripe Price if available, otherwise create inline price
            if (!string.IsNullOrEmpty(stripePriceId))
            {
                lineItems.Add(new SessionLineItemOptions
                {
                    Price = stripePriceId,
                    Quantity = quantity,
                });
            }
            else
            {
                lineItems.Add(new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(ticketPrice * 100), // Convert to cents
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = $"{raffleName} - Raffle Ticket",
                            Description = $"Ticket(s) for {raffleName}",
                        },
                    },
                    Quantity = quantity,
                });
            }

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = lineItems,
                Mode = "payment",
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl,
                Metadata = new Dictionary<string, string>
                {
                    { "raffle_id", raffleId.ToString() },
                    { "user_id", userId.ToString() },
                    { "ticket_ids", string.Join(",", ticketIds) }, // Store specific ticket IDs
                    { "quantity", quantity.ToString() },
                    { "buyer_email", buyerEmail },
                    { "buyer_name", buyerName ?? "" },
                    { "purchase_type", "selected_tickets" } // Identify this as pre-selected tickets
                }
            };

            // Use existing Stripe Customer if available
            if (!string.IsNullOrEmpty(stripeCustomerId))
            {
                options.Customer = stripeCustomerId;
            }
            else
            {
                options.CustomerEmail = buyerEmail;
            }

            var service = new Stripe.Checkout.SessionService();
            var session = await service.CreateAsync(options);

            Console.WriteLine($"Raffle Session Created (Selected Tickets): {session.Id}");
            Console.WriteLine($"Raffle ID: {raffleId}, User ID: {userId}, Tickets: [{string.Join(",", ticketIds)}]");
            Console.WriteLine($"Session URL: {session.Url}");
            Console.WriteLine($"================================");

            return session.Url;
        }
        catch (StripeException ex)
        {
            Console.WriteLine($"STRIPE ERROR: {ex.Message}");
            throw new InvalidOperationException($"Error creating raffle checkout session: {ex.Message}", ex);
        }
    }
}
