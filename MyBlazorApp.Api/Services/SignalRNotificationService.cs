using Microsoft.AspNetCore.SignalR;
using MyBlazorApp.Api.Hubs;

namespace MyBlazorApp.Api.Services;

/// <summary>
/// Centralized service to broadcast SignalR notifications to connected clients
/// </summary>
public class SignalRNotificationService
{
    private readonly IHubContext<RaffleHub> _hubContext;
    private readonly ILogger<SignalRNotificationService> _logger;

    public SignalRNotificationService(
        IHubContext<RaffleHub> hubContext,
        ILogger<SignalRNotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    /// <summary>
    /// Notify all clients that the raffle list has changed (create, update, activate, close, sync)
    /// </summary>
    public async Task NotifyRafflesUpdatedAsync()
    {
        try
        {
            await _hubContext.Clients.Group(RaffleHub.GlobalGroup)
                .SendAsync(RaffleHub.Events.RafflesUpdated);
            _logger.LogInformation("Broadcast: RafflesUpdated to all clients");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting RafflesUpdated");
        }
    }

    /// <summary>
    /// Notify clients viewing a specific raffle that it has been updated
    /// </summary>
    public async Task NotifyRaffleUpdatedAsync(int raffleId)
    {
        try
        {
            var groupName = RaffleHub.GetRaffleGroupName(raffleId);
            await _hubContext.Clients.Group(groupName)
                .SendAsync(RaffleHub.Events.RaffleUpdated, raffleId);
            _logger.LogInformation("Broadcast: RaffleUpdated for raffle {RaffleId}", raffleId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting RaffleUpdated for raffle {RaffleId}", raffleId);
        }
    }

    /// <summary>
    /// Notify clients that tickets were sold for a raffle
    /// </summary>
    public async Task NotifyTicketsSoldAsync(int raffleId, int ticketsSold, int ticketsAvailable)
    {
        try
        {
            var groupName = RaffleHub.GetRaffleGroupName(raffleId);
            var payload = new { raffleId, ticketsSold, ticketsAvailable };

            // Notify raffle-specific group
            await _hubContext.Clients.Group(groupName)
                .SendAsync(RaffleHub.Events.TicketsSold, payload);

            // Also notify global group so Home page cards update
            await _hubContext.Clients.Group(RaffleHub.GlobalGroup)
                .SendAsync(RaffleHub.Events.TicketsSold, payload);

            _logger.LogInformation("Broadcast: TicketsSold for raffle {RaffleId} ({Sold} sold, {Available} available)",
                raffleId, ticketsSold, ticketsAvailable);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting TicketsSold for raffle {RaffleId}", raffleId);
        }
    }

    /// <summary>
    /// Notify clients that a winner was drawn for a raffle
    /// </summary>
    public async Task NotifyWinnerDrawnAsync(int raffleId)
    {
        try
        {
            var groupName = RaffleHub.GetRaffleGroupName(raffleId);
            await _hubContext.Clients.Group(groupName)
                .SendAsync(RaffleHub.Events.WinnerDrawn, raffleId);

            // Also notify global
            await _hubContext.Clients.Group(RaffleHub.GlobalGroup)
                .SendAsync(RaffleHub.Events.RafflesUpdated);

            _logger.LogInformation("Broadcast: WinnerDrawn for raffle {RaffleId}", raffleId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting WinnerDrawn for raffle {RaffleId}", raffleId);
        }
    }
}
