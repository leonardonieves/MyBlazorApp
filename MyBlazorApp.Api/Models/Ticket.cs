using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyBlazorApp.Api.Models;

/// <summary>
/// Ticket status enum - for pre-generated ticket system
/// Maintains backward compatibility with legacy names
/// </summary>
public enum TicketStatus
{
    Available = 0,      // Ticket is available for selection
    Pending = 1,        // Legacy: same as Reserved
    Reserved = 1,       // Temporarily reserved (user in checkout) - expires after timeout
    Confirmed = 2,      // Legacy: same as Sold  
    Sold = 2,           // Payment confirmed, ticket sold
    Refunded = 3,       // Payment refunded, ticket back to available
    Cancelled = 3,      // Legacy: same as Refunded
    Winner = 4          // This ticket won the raffle
}

/// <summary>
/// Represents a pre-generated raffle ticket
/// </summary>
public class Ticket
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int RaffleId { get; set; }

    /// <summary>
    /// Display number shown to users (e.g., "001", "002", etc.)
    /// </summary>
    [Required]
    [MaxLength(10)]
    public string DisplayNumber { get; set; } = string.Empty;

    /// <summary>
    /// Unique ticket number/code for verification
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string TicketNumber { get; set; } = string.Empty;

    /// <summary>
    /// Current status of the ticket
    /// </summary>
    public TicketStatus Status { get; set; } = TicketStatus.Available;

    /// <summary>
    /// User who owns/reserved this ticket (null if available)
    /// </summary>
    public int? UserId { get; set; }

    /// <summary>
    /// Email of the buyer (set when sold)
    /// </summary>
    [MaxLength(255)]
    public string? BuyerEmail { get; set; }

    /// <summary>
    /// Name of the buyer (set when sold)
    /// </summary>
    [MaxLength(100)]
    public string? BuyerName { get; set; }

    /// <summary>
    /// When the ticket was reserved (for timeout calculation)
    /// </summary>
    public DateTime? ReservedAt { get; set; }

    /// <summary>
    /// When the reservation expires (typically ReservedAt + 10 minutes)
    /// </summary>
    public DateTime? ReservationExpiresAt { get; set; }

    /// <summary>
    /// When the ticket was sold/confirmed
    /// </summary>
    public DateTime? SoldAt { get; set; }

    /// <summary>
    /// Stripe Payment Intent ID (set when sold)
    /// </summary>
    [MaxLength(255)]
    public string? StripePaymentIntentId { get; set; }

    /// <summary>
    /// Stripe Session ID (set when sold)
    /// </summary>
    [MaxLength(255)]
    public string? StripeSessionId { get; set; }

    /// <summary>
    /// Stripe Customer ID (set when sold)
    /// </summary>
    [MaxLength(100)]
    public string? StripeCustomerId { get; set; }

    /// <summary>
    /// Amount paid for this ticket
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal AmountPaid { get; set; }

    /// <summary>
    /// When the ticket was created (pre-generated)
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Computed properties
    [NotMapped]
    public bool IsAvailable => Status == TicketStatus.Available;

    [NotMapped]
    public bool IsReserved => Status == TicketStatus.Reserved && ReservationExpiresAt > DateTime.UtcNow;

    [NotMapped]
    public bool IsSold => Status == TicketStatus.Sold;

    [NotMapped]
    public bool IsExpiredReservation => Status == TicketStatus.Reserved && ReservationExpiresAt <= DateTime.UtcNow;

    // Navigation properties
    [ForeignKey(nameof(RaffleId))]
    public Raffle Raffle { get; set; } = null!;

    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }

    public Winner? Winner { get; set; }
}
