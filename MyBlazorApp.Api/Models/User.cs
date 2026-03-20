namespace MyBlazorApp.Api.Models;

public class Role
{
    public int Id { get; set; }
    public string Name { get; set; } // "Admin", "Basic"

    public ICollection<User> Users { get; set; } = new List<User>();
}

public class User
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public int RoleId { get; set; }
    public Role Role { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; } = true;

    // Stripe integration
    public string? StripeCustomerId { get; set; }
    public DateTime? StripeCustomerCreatedAt { get; set; }
}
