namespace MyBlazorApp.Models.DTOs;

/// <summary>
/// Request to buy raffle tickets
/// </summary>
public class BuyTicketRequest
{
    public int RaffleId { get; set; }
    public int UserId { get; set; }
    public int Quantity { get; set; } = 1;
    public string BuyerEmail { get; set; } = string.Empty;
    public string? BuyerName { get; set; }
    public string? StripeCustomerId { get; set; }
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
/// Raffle summary DTO for API responses
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
    public RaffleStatus Status { get; set; }
    public bool IsFeatured { get; set; }
    public bool CanPurchase { get; set; }
    public double SoldPercentage { get; set; }
    public List<RafflePrizeDto> Prizes { get; set; } = new();
    public List<RaffleImageDto> Images { get; set; } = new();
}

/// <summary>
/// Prize DTO
/// </summary>
public class RafflePrizeDto
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = "fas fa-gift";
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

/// <summary>
/// Ticket DTO for API responses
/// </summary>
public class TicketDto
{
    public int Id { get; set; }
    public string TicketNumber { get; set; } = string.Empty;
    public int RaffleId { get; set; }
    public string RaffleTitle { get; set; } = string.Empty;
    public decimal AmountPaid { get; set; }
    public TicketStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime DrawDate { get; set; }
    public bool IsWinner { get; set; }
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

/// <summary>
/// Request to create a new raffle (Admin)
/// </summary>
public class CreateRaffleRequest
{
    public string Title { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public string FullDescription { get; set; } = string.Empty;
    public decimal TicketPrice { get; set; }
    public int TotalTickets { get; set; }
    public int MaxTicketsPerUser { get; set; } = 10;
    public string? PrimaryImageUrl { get; set; }
    public DateTime SalesStartDate { get; set; }
    public DateTime SalesEndDate { get; set; }
    public DateTime DrawDate { get; set; }
    public bool IsFeatured { get; set; }
    public List<string> Prizes { get; set; } = new();
}

/// <summary>
/// Extension methods for mapping entities to DTOs
/// </summary>
public static class RaffleDtoExtensions
{
    public static RaffleDto ToDto(this Raffle raffle)
    {
        return new RaffleDto
        {
            Id = raffle.Id,
            Title = raffle.Title,
            ShortDescription = raffle.ShortDescription,
            FullDescription = raffle.FullDescription,
            TicketPrice = raffle.TicketPrice,
            TotalTickets = raffle.TotalTickets,
            TicketsSold = raffle.TicketsSold,
            TicketsAvailable = raffle.TicketsAvailable,
            MaxTicketsPerUser = raffle.MaxTicketsPerUser,
            PrimaryImageUrl = raffle.PrimaryImageUrl,
            SalesStartDate = raffle.SalesStartDate,
            SalesEndDate = raffle.SalesEndDate,
            DrawDate = raffle.DrawDate,
            Status = raffle.Status,
            IsFeatured = raffle.IsFeatured,
            CanPurchase = raffle.CanPurchase,
            SoldPercentage = raffle.SoldPercentage,
            Prizes = raffle.Prizes.Select(p => new RafflePrizeDto
            {
                Id = p.Id,
                Description = p.Description,
                Icon = p.Icon
            }).ToList(),
            Images = raffle.Images.Select(i => new RaffleImageDto
            {
                Id = i.Id,
                ImageUrl = i.ImageUrl,
                AltText = i.AltText,
                IsPrimary = i.IsPrimary
            }).ToList()
        };
    }

    public static TicketDto ToDto(this Ticket ticket)
    {
        return new TicketDto
        {
            Id = ticket.Id,
            TicketNumber = ticket.TicketNumber,
            RaffleId = ticket.RaffleId,
            RaffleTitle = ticket.Raffle?.Title ?? "",
            AmountPaid = ticket.AmountPaid,
            Status = ticket.Status,
            CreatedAt = ticket.CreatedAt,
            DrawDate = ticket.Raffle?.DrawDate ?? DateTime.MinValue,
            IsWinner = ticket.Status == TicketStatus.Winner
        };
    }
}

/// <summary>
/// Request to buy specific pre-selected tickets
/// </summary>
public class BuyTicketWithSelectionRequest
{
    public int RaffleId { get; set; }
    public int UserId { get; set; }
    public List<int> TicketIds { get; set; } = new();
    public string BuyerEmail { get; set; } = string.Empty;
    public string? BuyerName { get; set; }
    public string? StripeCustomerId { get; set; }
    public string SuccessUrl { get; set; } = string.Empty;
    public string CancelUrl { get; set; } = string.Empty;
}

/// <summary>
/// DTO for ticket status in the visual selector
/// </summary>
public class TicketStatusDto
{
    public int TicketId { get; set; }
    public int RaffleId { get; set; }
    public string DisplayNumber { get; set; } = string.Empty;
    public string Status { get; set; } = "available"; // available, reserved, sold
    public int? ReservedByUserId { get; set; }
    public DateTime? ExpiresAt { get; set; }
}
