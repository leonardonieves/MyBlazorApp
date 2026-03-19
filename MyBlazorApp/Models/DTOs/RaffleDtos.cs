namespace MyBlazorApp.Models.DTOs;

/// <summary>
/// Request to buy raffle tickets
/// </summary>
public class BuyTicketRequest
{
    public int RaffleId { get; set; }
    public int Quantity { get; set; } = 1;
    public string BuyerEmail { get; set; } = string.Empty;
    public string? BuyerName { get; set; }
    public string SuccessUrl { get; set; } = string.Empty;
    public string CancelUrl { get; set; } = string.Empty;
}

/// <summary>
/// Response for buy ticket request
/// </summary>
public class BuyTicketResponse
{
    public bool Success { get; set; }
    public string? CheckoutUrl { get; set; }
    public string? SessionId { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Raffle summary DTO
/// </summary>
public class RaffleDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string PrizeDetails { get; set; } = string.Empty;
    public decimal TicketPrice { get; set; }
    public int TotalTickets { get; set; }
    public int TicketsSold { get; set; }
    public int TicketsAvailable { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime DrawDate { get; set; }
    public bool IsActive { get; set; }
    public bool IsDrawn { get; set; }
}

/// <summary>
/// Winner information DTO
/// </summary>
public class WinnerDto
{
    public int Id { get; set; }
    public int RaffleId { get; set; }
    public string RaffleTitle { get; set; } = string.Empty;
    public string TicketNumber { get; set; } = string.Empty;
    public string WinnerEmail { get; set; } = string.Empty;
    public string? WinnerName { get; set; }
    public DateTime AnnouncedAt { get; set; }
}
