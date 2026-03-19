using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyBlazorApp.Models;

/// <summary>
/// Represents a raffle ticket purchased by a user
/// </summary>
public class Ticket
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int RaffleId { get; set; }

    [Required]
    [MaxLength(255)]
    [EmailAddress]
    public string BuyerEmail { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? BuyerName { get; set; }

    [Required]
    [MaxLength(50)]
    public string TicketNumber { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string StripePaymentIntentId { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? StripeSessionId { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal AmountPaid { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(50)]
    public string Status { get; set; } = "pending"; // pending, confirmed, refunded

    // Navigation properties
    [ForeignKey(nameof(RaffleId))]
    public Raffle Raffle { get; set; } = null!;

    public Winner? Winner { get; set; }
}
