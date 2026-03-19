using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyBlazorApp.Models;

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
    public string Description { get; set; } = string.Empty;

    [Required]
    public string PrizeDetails { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal TicketPrice { get; set; }

    [Required]
    public int TotalTickets { get; set; }

    public int TicketsSold { get; set; } = 0;

    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    public DateTime DrawDate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    public bool IsDrawn { get; set; } = false;

    [MaxLength(100)]
    public string? StripePriceId { get; set; }

    // Navigation properties
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    public Winner? Winner { get; set; }
}
