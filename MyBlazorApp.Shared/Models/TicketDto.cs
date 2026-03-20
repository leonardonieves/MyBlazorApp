namespace MyBlazorApp.Shared.Models;

public class TicketDto
{
    public int Id { get; set; }
    public int RaffleId { get; set; }
    public int TicketNumber { get; set; }
    public int? UserId { get; set; }
    public string? UserName { get; set; }
    public string Status { get; set; } = "Available";
    public DateTime? PurchaseDate { get; set; }
    public DateTime? PurchasedAt => PurchaseDate; // Alias for PurchaseDate
    public DateTime? ReservationDate { get; set; }
    public DateTime? ReservationExpiry { get; set; }
    public decimal Price { get; set; }
    public string? PaymentId { get; set; }
    public string? PaymentStatus { get; set; }
    public bool IsWinner { get; set; } = false;
}