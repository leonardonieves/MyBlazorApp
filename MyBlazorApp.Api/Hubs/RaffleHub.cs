using Microsoft.AspNetCore.SignalR;

namespace MyBlazorApp.Api.Hubs;

/// <summary>
/// SignalR Hub for real-time raffle ticket updates
/// </summary>
public class RaffleHub : Hub
{
    private readonly ILogger<RaffleHub> _logger;

    public const string GlobalGroup = "all_raffles";

    public RaffleHub(ILogger<RaffleHub> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Called when a client connects - automatically join global group
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, GlobalGroup);
        _logger.LogInformation("Client {ConnectionId} connected and joined global group", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Join a raffle room to receive updates for that specific raffle
    /// </summary>
    public async Task JoinRaffle(int raffleId)
    {
        var groupName = GetRaffleGroupName(raffleId);
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation("Client {ConnectionId} joined raffle {RaffleId}", Context.ConnectionId, raffleId);
    }

    /// <summary>
    /// Leave a raffle room
    /// </summary>
    public async Task LeaveRaffle(int raffleId)
    {
        var groupName = GetRaffleGroupName(raffleId);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation("Client {ConnectionId} left raffle {RaffleId}", Context.ConnectionId, raffleId);
    }

    /// <summary>
    /// Called when a client disconnects
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Client {ConnectionId} disconnected", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Get the group name for a raffle
    /// </summary>
    public static string GetRaffleGroupName(int raffleId) => $"raffle_{raffleId}";

    // Event name constants
    public static class Events
    {
        public const string RafflesUpdated = "RafflesUpdated";
        public const string RaffleUpdated = "RaffleUpdated";
        public const string TicketsSold = "TicketsSold";
        public const string WinnerDrawn = "WinnerDrawn";
    }
}

/// <summary>
/// DTO for ticket status updates sent via SignalR
/// </summary>
public class TicketStatusUpdate
{
    public int TicketId { get; set; }
    public int RaffleId { get; set; }
    public string DisplayNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // "available", "reserved", "sold"
    public int? ReservedByUserId { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

/// <summary>
/// DTO for bulk ticket updates
/// </summary>
public class BulkTicketUpdate
{
    public int RaffleId { get; set; }
    public List<TicketStatusUpdate> Tickets { get; set; } = new();
}
