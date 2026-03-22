namespace MyBlazorApp.Shared.Models;

/// <summary>
/// Raffle DTO for API responses - matches MyBlazorApp.Api.Models.DTOs.RaffleDto
/// </summary>
public class RaffleDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public string FullDescription { get; set; } = string.Empty;
    public decimal TicketPrice { get; set; }
    public int TotalTickets { get; set; }
    public int TicketsSold { get; set; }
    public int TicketsAvailable { get; set; }
    public int MaxTicketsPerUser { get; set; }
    public string? PrimaryImageUrl { get; set; }
    public DateTime SalesStartDate { get; set; }
    public DateTime SalesEndDate { get; set; }
    public DateTime DrawDate { get; set; }
    public int Status { get; set; } = 1; // RaffleStatus enum value
    public bool IsFeatured { get; set; }
    public bool CanPurchase { get; set; }
    public double SoldPercentage { get; set;}
    public List<RafflePrizeDto> Prizes { get; set; } = new();
    public List<RaffleImageDto> Images { get; set; } = new();

    // Computed properties
    public bool IsSoldOut => TicketsAvailable <= 0;
}

/// <summary>
/// Prize DTO
/// </summary>
public class RafflePrizeDto
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = "fas fa-gift";
    public int DisplayOrder { get; set; } = 0;
}

/// <summary>
/// Image DTO
/// </summary>
public class RaffleImageDto
{
    public int Id { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? AltText { get; set; }
    public bool IsPrimary { get; set; }
}