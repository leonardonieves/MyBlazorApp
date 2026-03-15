namespace MyBlazorApp.Models;

public class Payment
{
    public int Id { get; set; }
    public string StripeSessionId { get; set; }
    public string ProductName { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } // pending, completed, failed
    public DateTime CreatedAt { get; set; }
    public string? CustomerEmail { get; set; }
}
