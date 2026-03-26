using MyBlazorApp.Shared.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace MyBlazorApp.Web.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public ApiService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _baseUrl = configuration["ApiBaseUrl"] ?? "https://localhost:7133/api/";
        // Asegurar que BaseAddress termine en / para que funcione correctamente con rutas relativas
        var baseAddress = _baseUrl.EndsWith("/") ? _baseUrl : _baseUrl + "/";
        _httpClient.BaseAddress = new Uri(baseAddress);
    }

    // Auth endpoints
    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("auth/login", request);
        return await response.Content.ReadFromJsonAsync<AuthResponse>() ?? new AuthResponse { Success = false, Message = "Error de conexión" };
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("auth/register", request);
        return await response.Content.ReadFromJsonAsync<AuthResponse>() ?? new AuthResponse { Success = false, Message = "Error de conexión" };
    }

    // Raffle endpoints
    public async Task<List<RaffleDto>> GetRafflesAsync()
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<List<RaffleDto>>("raffles");
            Console.WriteLine($"GetRafflesAsync: Got {result?.Count ?? 0} raffles from API");
            return result ?? new List<RaffleDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"GetRafflesAsync ERROR: {ex.Message}");
            return new List<RaffleDto>();
        }
    }

    // Debug: Get ALL raffles without filters
    public async Task<List<RaffleDto>> GetAllRafflesAsync()
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<List<RaffleDto>>("raffles/all");
            Console.WriteLine($"GetAllRafflesAsync: Got {result?.Count ?? 0} raffles from API");
            return result ?? new List<RaffleDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"GetAllRafflesAsync ERROR: {ex.Message}");
            return new List<RaffleDto>();
        }
    }

    public async Task<RaffleDto?> GetRaffleAsync(int id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<RaffleDto>($"raffles/{id}");
        }
        catch
        {
            return null;
        }
    }

    public async Task<RaffleDto?> GetRaffleByIdAsync(int id) => await GetRaffleAsync(id);

    // Ticket endpoints
    public async Task<List<TicketDto>> GetMyTicketsAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<TicketDto>>("tickets/my") ?? new List<TicketDto>();
        }
        catch
        {
            return new List<TicketDto>();
        }
    }

    public async Task<List<TicketDto>> GetUserTicketsAsync(int userId)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<TicketDto>>($"tickets/user/{userId}") ?? new List<TicketDto>();
        }
        catch
        {
            return new List<TicketDto>();
        }
    }

    public async Task<BuyTicketResponse> BuyTicketsAsync(BuyTicketRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("tickets/buy", request);
            return await response.Content.ReadFromJsonAsync<BuyTicketResponse>() ?? new BuyTicketResponse { Success = false, ErrorMessage = "Connection error" };
        }
        catch
        {
            return new BuyTicketResponse { Success = false, ErrorMessage = "Connection error" };
        }
    }

    // Admin endpoints
    public async Task<SyncResponse> SyncFromStripeAsync()
    {
        try
        {
            var response = await _httpClient.PostAsync("sync/stripe", null);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<SyncResponse>() 
                    ?? new SyncResponse { Success = false, Message = "Error parsing response" };
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            return new SyncResponse 
            { 
                Success = false, 
                Message = $"Error: {response.StatusCode} - {errorContent}" 
            };
        }
        catch (Exception ex)
        {
            return new SyncResponse { Success = false, Message = $"Error de conexión: {ex.Message}" };
        }
    }

    /// <summary>
    /// Get sync status from the API
    /// </summary>
    public async Task<SyncStatusResponse?> GetSyncStatusAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<SyncStatusResponse>("sync/status");
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Get Stripe products for debugging
    /// </summary>
    public async Task<List<object>?> GetStripeProductsAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<object>>("sync/stripe-products");
        }
        catch
        {
            return null;
        }
    }

    public void SetAuthToken(string token)
    {
        _httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    public void ClearAuthToken()
    {
        _httpClient.DefaultRequestHeaders.Authorization = null;
    }

    // Payment endpoints
    public async Task<List<PaymentDto>> GetMyPaymentsAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<PaymentDto>>("payments/my") ?? new List<PaymentDto>();
        }
        catch
        {
            return new List<PaymentDto>();
        }
    }

    // Raffle Tickets Visual Selector
    public async Task<List<TicketStatusDto>> GetRaffleTicketStatusesAsync(int raffleId)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<TicketStatusDto>>($"tickets/status/{raffleId}") ?? new List<TicketStatusDto>();
        }
        catch
        {
            return new List<TicketStatusDto>();
        }
    }

    // Reserve tickets
    public async Task<ReserveTicketsResponse> ReserveTicketsAsync(int raffleId, int userId, List<int> ticketIds)
    {
        try
        {
            var request = new { RaffleId = raffleId, UserId = userId, TicketIds = ticketIds };
            var response = await _httpClient.PostAsJsonAsync("tickets/reserve", request);
            return await response.Content.ReadFromJsonAsync<ReserveTicketsResponse>() 
                ?? new ReserveTicketsResponse { Success = false, Message = "Error parsing response" };
        }
        catch (Exception ex)
        {
            return new ReserveTicketsResponse { Success = false, Message = ex.Message };
        }
    }

    // Release reservation
    public async Task<bool> ReleaseReservationAsync(int raffleId, int userId, List<int>? ticketIds = null)
    {
        try
        {
            var request = new { RaffleId = raffleId, UserId = userId, TicketIds = ticketIds };
            var response = await _httpClient.PostAsJsonAsync("tickets/release", request);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    // Buy specific tickets (with selection)
    public async Task<BuyTicketResponse> BuySelectedTicketsAsync(int raffleId, int userId, List<int> ticketIds, string buyerEmail, string? buyerName, string successUrl, string cancelUrl)
    {
        try
        {
            var request = new 
            { 
                RaffleId = raffleId, 
                UserId = userId, 
                TicketIds = ticketIds,
                BuyerEmail = buyerEmail,
                BuyerName = buyerName,
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl
            };
            var response = await _httpClient.PostAsJsonAsync("tickets/buy-selected", request);
            return await response.Content.ReadFromJsonAsync<BuyTicketResponse>() 
                ?? new BuyTicketResponse { Success = false, ErrorMessage = "Error parsing response" };
        }
        catch (Exception ex)
        {
            return new BuyTicketResponse { Success = false, ErrorMessage = ex.Message };
        }
    }

    // Get featured raffles
    public async Task<List<RaffleDto>> GetFeaturedRafflesAsync(int count = 3)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<RaffleDto>>($"raffles/featured?count={count}") ?? new List<RaffleDto>();
        }
        catch
        {
            return new List<RaffleDto>();
        }
    }

    // Admin: Get all users
    public async Task<List<UserDto>> GetAllUsersAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<UserDto>>("admin/users") ?? new List<UserDto>();
        }
        catch
        {
            return new List<UserDto>();
        }
    }

    // Admin: Get all payments
    public async Task<List<PaymentDto>> GetAllPaymentsAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<PaymentDto>>("admin/payments") ?? new List<PaymentDto>();
        }
        catch
        {
            return new List<PaymentDto>();
        }
    }

    // Verify a Stripe payment session and process tickets if needed
    public async Task<VerifyPaymentResponse> VerifyPaymentAsync(string sessionId)
    {
        try
        {
            var request = new { SessionId = sessionId };
            var response = await _httpClient.PostAsJsonAsync("tickets/verify-payment", request);
            return await response.Content.ReadFromJsonAsync<VerifyPaymentResponse>()
                ?? new VerifyPaymentResponse { Success = false, Message = "Error parsing response" };
        }
        catch (Exception ex)
        {
            return new VerifyPaymentResponse { Success = false, Message = ex.Message };
        }
    }

    /// <summary>
    /// Generic method to POST and get JSON response (useful for testing)
    /// </summary>
    public async Task<T?> GetFromJsonAsync<T>(string endpoint, string method = "GET")
    {
        try
        {
            HttpResponseMessage response;
            if (method.ToUpper() == "POST")
            {
                response = await _httpClient.PostAsync(endpoint, null);
            }
            else
            {
                response = await _httpClient.GetAsync(endpoint);
            }

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<T>();
            }
            return default;
        }
        catch
        {
            return default;
        }
    }

    /// <summary>
    /// Generic method to make HTTP requests without specifying response type
    /// </summary>
    public async Task<bool> SendRequestAsync(string endpoint, string method = "GET")
    {
        try
        {
            HttpResponseMessage response;
            if (method.ToUpper() == "POST")
            {
                response = await _httpClient.PostAsync(endpoint, null);
            }
            else
            {
                response = await _httpClient.GetAsync(endpoint);
            }

            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
