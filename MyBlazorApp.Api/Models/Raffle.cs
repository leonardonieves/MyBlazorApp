using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyBlazorApp.Api.Models;

/// <summary>
/// Raffle status enum
/// </summary>
public enum RaffleStatus
{
    Draft = 0,          // Borrador - no visible al público
    Active = 1,         // Activo - aceptando compras
    SalesClosed = 2,    // Ventas cerradas - esperando sorteo
    Drawing = 3,        // En proceso de sorteo
    Completed = 4,      // Completado - ganador anunciado
    Cancelled = 5       // Cancelado
}

/// <summary>
/// Represents a raffle/sweepstakes event
/// </summary>
public class Raffle
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string ShortDescription { get; set; } = string.Empty;

    [Required]
    public string FullDescription { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal TicketPrice { get; set; }

    [Required]
    public int TotalTickets { get; set; }

    public int TicketsSold { get; set; } = 0;

    /// <summary>
    /// Maximum tickets per user (0 = unlimited)
    /// </summary>
    public int MaxTicketsPerUser { get; set; } = 10;

    [MaxLength(500)]
    public string? PrimaryImageUrl { get; set; }

    public DateTime SalesStartDate { get; set; }
    public DateTime SalesEndDate { get; set; }
    public DateTime DrawDate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public RaffleStatus Status { get; set; } = RaffleStatus.Draft;

    /// <summary>
    /// Stripe Product ID for this raffle
    /// </summary>
    [MaxLength(100)]
    public string? StripeProductId { get; set; }

    /// <summary>
    /// Stripe Price ID for ticket purchase
    /// </summary>
    [MaxLength(100)]
    public string? StripePriceId { get; set; }

    /// <summary>
    /// Featured raffle (shown first)
    /// </summary>
    public bool IsFeatured { get; set; } = false;

    /// <summary>
    /// User who created this raffle
    /// </summary>
    public int? CreatedByUserId { get; set; }

    // Navigation properties
    public ICollection<RafflePrize> Prizes { get; set; } = new List<RafflePrize>();
    public ICollection<RaffleImage> Images { get; set; } = new List<RaffleImage>();
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    public Winner? Winner { get; set; }

    // Computed properties
    [NotMapped]
    public int TicketsAvailable => TotalTickets - TicketsSold;

    [NotMapped]
    public bool IsSoldOut => TicketsSold >= TotalTickets;

    [NotMapped]
    public bool CanPurchase => Status == RaffleStatus.Active && 
                               !IsSoldOut && 
                               DateTime.UtcNow >= SalesStartDate && 
                               DateTime.UtcNow <= SalesEndDate;

    [NotMapped]
    public double SoldPercentage => TotalTickets > 0 ? (double)TicketsSold / TotalTickets * 100 : 0;
}

/// <summary>
/// Prize details for a raffle
/// </summary>
public class RafflePrize
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int RaffleId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Icon { get; set; } = "fas fa-gift";

    public int DisplayOrder { get; set; } = 0;

    [ForeignKey(nameof(RaffleId))]
    public Raffle Raffle { get; set; } = null!;
}

/// <summary>
/// Additional images for a raffle
/// </summary>
public class RaffleImage
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int RaffleId { get; set; }

    [Required]
    [MaxLength(500)]
    public string ImageUrl { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? AltText { get; set; }

    public bool IsPrimary { get; set; } = false;

    public int DisplayOrder { get; set; } = 0;

    [ForeignKey(nameof(RaffleId))]
    public Raffle Raffle { get; set; } = null!;
}
