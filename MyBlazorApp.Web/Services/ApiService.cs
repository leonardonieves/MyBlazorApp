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
        _baseUrl = configuration["ApiBaseUrl"] ?? "https://localhost:7267/api";
        _httpClient.BaseAddress = new Uri(_baseUrl);
    }

    // Auth endpoints
    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("/auth/login", request);
        return await response.Content.ReadFromJsonAsync<AuthResponse>() ?? new AuthResponse { Success = false, Message = "Error de conexión" };
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("/auth/register", request);
        return await response.Content.ReadFromJsonAsync<AuthResponse>() ?? new AuthResponse { Success = false, Message = "Error de conexión" };
    }

    // Raffle endpoints
    public async Task<List<RaffleDto>> GetRafflesAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<RaffleDto>>("/raffles") ?? new List<RaffleDto>();
        }
        catch
        {
            return new List<RaffleDto>();
        }
    }

    public async Task<RaffleDto?> GetRaffleAsync(int id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<RaffleDto>($"/raffles/{id}");
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
            return await _httpClient.GetFromJsonAsync<List<TicketDto>>("/tickets/my") ?? new List<TicketDto>();
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
            return await _httpClient.GetFromJsonAsync<List<TicketDto>>($"/tickets/user/{userId}") ?? new List<TicketDto>();
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
            var response = await _httpClient.PostAsJsonAsync("/tickets/buy", request);
            return await response.Content.ReadFromJsonAsync<BuyTicketResponse>() ?? new BuyTicketResponse { Success = false, ErrorMessage = "Error de conexión" };
        }
        catch
        {
            return new BuyTicketResponse { Success = false, ErrorMessage = "Error de conexión" };
        }
    }

    // Admin endpoints
    public async Task<SyncResponse> SyncFromStripeAsync()
    {
        try
        {
            var response = await _httpClient.PostAsync("/admin/sync-stripe", null);
            return await response.Content.ReadFromJsonAsync<SyncResponse>() ?? new SyncResponse { Success = false, Message = "Error de conexión" };
        }
        catch
        {
            return new SyncResponse { Success = false, Message = "Error de conexión" };
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
}