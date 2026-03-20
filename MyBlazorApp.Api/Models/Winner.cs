using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyBlazorApp.Api.Models;

/// <summary>
/// Represents a raffle winner
/// </summary>
public class Winner
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int RaffleId { get; set; }

    [Required]
    public int TicketId { get; set; }

    public DateTime AnnouncedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(500)]
    public string? Notes { get; set; }

    public bool ContactedWinner { get; set; } = false;

    public bool PrizeDelivered { get; set; } = false;

    // Navigation properties
    [ForeignKey(nameof(RaffleId))]
    public Raffle Raffle { get; set; } = null!;

    [ForeignKey(nameof(TicketId))]
    public Ticket Ticket { get; set; } = null!;
}
