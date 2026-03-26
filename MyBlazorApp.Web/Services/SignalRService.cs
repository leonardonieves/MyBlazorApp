using Microsoft.AspNetCore.SignalR.Client;

namespace MyBlazorApp.Web.Services;

/// <summary>
/// Manages the SignalR connection to the API hub for real-time updates
/// </summary>
public class SignalRService : IAsyncDisposable
{
    private readonly HubConnection _hubConnection;
    private readonly ILogger<SignalRService> _logger;
    private bool _started;

    /// <summary>
    /// Fires when the raffle list has changed (new raffle, status change, sync, etc.)
    /// </summary>
    public event Func<Task>? OnRafflesUpdated;

    /// <summary>
    /// Fires when a specific raffle is updated. Provides the raffle ID.
    /// </summary>
    public event Func<int, Task>? OnRaffleUpdated;

    /// <summary>
    /// Fires when tickets are sold. Provides raffleId, ticketsSold, ticketsAvailable.
    /// </summary>
    public event Func<int, int, int, Task>? OnTicketsSold;

    /// <summary>
    /// Fires when a winner is drawn for a raffle. Provides the raffle ID.
    /// </summary>
    public event Func<int, Task>? OnWinnerDrawn;

    public bool IsConnected => _hubConnection.State == HubConnectionState.Connected;

    public SignalRService(IConfiguration configuration, ILogger<SignalRService> logger)
    {
        _logger = logger;

        var apiBaseUrl = configuration["ApiBaseUrl"] ?? "https://localhost:7133/api/";
        // Build hub URL from API base - strip the /api/ suffix
        var hubUrl = apiBaseUrl.TrimEnd('/');
        if (hubUrl.EndsWith("/api"))
            hubUrl = hubUrl[..^4];
        hubUrl += "/rafflehub";

        _logger.LogInformation("SignalRService initializing with hub URL: {HubUrl}", hubUrl);

        _hubConnection = new HubConnectionBuilder()
            .WithUrl(hubUrl)
            .WithAutomaticReconnect(new[] { TimeSpan.Zero, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(30) })
            .Build();

        RegisterHandlers();
        _logger.LogInformation("SignalRService initialized successfully");
    }

    private void RegisterHandlers()
    {
        _hubConnection.On("RafflesUpdated", async () =>
        {
            _logger.LogInformation("SignalR: RafflesUpdated received");
            if (OnRafflesUpdated != null)
                await OnRafflesUpdated.Invoke();
        });

        _hubConnection.On<int>("RaffleUpdated", async (raffleId) =>
        {
            _logger.LogInformation("SignalR: RaffleUpdated received for raffle {RaffleId}", raffleId);
            if (OnRaffleUpdated != null)
                await OnRaffleUpdated.Invoke(raffleId);
        });

        _hubConnection.On<TicketsSoldPayload>("TicketsSold", async (payload) =>
        {
            _logger.LogInformation("SignalR: TicketsSold received for raffle {RaffleId}", payload.RaffleId);
            if (OnTicketsSold != null)
                await OnTicketsSold.Invoke(payload.RaffleId, payload.TicketsSold, payload.TicketsAvailable);
        });

        _hubConnection.On<int>("WinnerDrawn", async (raffleId) =>
        {
            _logger.LogInformation("SignalR: WinnerDrawn received for raffle {RaffleId}", raffleId);
            if (OnWinnerDrawn != null)
                await OnWinnerDrawn.Invoke(raffleId);
        });

        _hubConnection.Reconnecting += (ex) =>
        {
            _logger.LogWarning("SignalR reconnecting: {Error}", ex?.Message);
            return Task.CompletedTask;
        };

        _hubConnection.Reconnected += (connectionId) =>
        {
            _logger.LogInformation("SignalR reconnected with ID: {ConnectionId}", connectionId);
            return Task.CompletedTask;
        };

        _hubConnection.Closed += (ex) =>
        {
            _logger.LogWarning("SignalR connection closed: {Error}", ex?.Message);
            return Task.CompletedTask;
        };
    }

    /// <summary>
    /// Start the SignalR connection
    /// </summary>
    public async Task StartAsync()
    {
        if (_started)
        {
            _logger.LogWarning("SignalR already started");
            return;
        }

        try
        {
            _logger.LogInformation("Starting SignalR connection...");
            await _hubConnection.StartAsync();
            _started = true;
            _logger.LogInformation("SignalR connected successfully. State: {State}", _hubConnection.State);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting SignalR connection. Exception: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Join a raffle-specific group to receive targeted updates
    /// </summary>
    public async Task JoinRaffleAsync(int raffleId)
    {
        if (!IsConnected) return;

        try
        {
            await _hubConnection.InvokeAsync("JoinRaffle", raffleId);
            _logger.LogInformation("Joined raffle group {RaffleId}", raffleId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error joining raffle group {RaffleId}", raffleId);
        }
    }

    /// <summary>
    /// Leave a raffle-specific group
    /// </summary>
    public async Task LeaveRaffleAsync(int raffleId)
    {
        if (!IsConnected) return;

        try
        {
            await _hubConnection.InvokeAsync("LeaveRaffle", raffleId);
            _logger.LogInformation("Left raffle group {RaffleId}", raffleId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error leaving raffle group {RaffleId}", raffleId);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_hubConnection is not null)
        {
            await _hubConnection.DisposeAsync();
        }
    }

    private record TicketsSoldPayload(int RaffleId, int TicketsSold, int TicketsAvailable);
}
