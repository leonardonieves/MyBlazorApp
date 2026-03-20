namespace MyBlazorApp.Shared.Models;

public class RaffleDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string Title => Name; // Alias for Name
    public required string Description { get; set; }
    public decimal TicketPrice { get; set; }
    public int TotalTickets { get; set; }
    public int AvailableTickets { get; set; }
    public int TicketsAvailable => AvailableTickets; // Alias for AvailableTickets
    public int TicketsSold => TotalTickets - AvailableTickets;
    public double SoldPercentage => TotalTickets > 0 ? (TicketsSold / (double)TotalTickets) * 100 : 0;
    public bool IsSoldOut => AvailableTickets <= 0;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime DrawDate => EndDate; // Alias for EndDate
    public string? ImageUrl { get; set; }
    public string? PrimaryImageUrl => ImageUrl; // Alias for ImageUrl
    public bool IsActive { get; set; }
    public bool IsFeatured { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<PrizeDto> Prizes { get; set; } = new();
}

public class PrizeDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public string? ImageUrl { get; set; }
    public decimal Value { get; set; }
    public int Position { get; set; }
    public string Icon { get; set; } = "🏆"; // Default icon
}