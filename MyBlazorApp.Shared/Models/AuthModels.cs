namespace MyBlazorApp.Shared.Models;

public class LoginRequest
{
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
}

public class RegisterRequest
{
    public string Username { get; set; } = "";
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public string ConfirmPassword { get; set; } = "";
}

public class AuthResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public AuthUserData? User { get; set; }
    public string? Token { get; set; }
    public DateTime TokenExpiration { get; set; }
}

public class AuthUserData
{
    public int Id { get; set; }
    public required string Username { get; set; }
    public required string Email { get; set; }
    public required string RoleName { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? StripeCustomerId { get; set; }
}

public class BuyTicketRequest
{
    public int RaffleId { get; set; }
    public int Quantity { get; set; }
    public int UserId { get; set; }
    public string BuyerEmail { get; set; } = "";
    public string BuyerName { get; set; } = "";
    public string? StripeCustomerId { get; set; }
    public string? SuccessUrl { get; set; }
    public string? CancelUrl { get; set; }
}

public class BuyTicketResponse
{
    public bool Success { get; set; }
    public string? CheckoutUrl { get; set; }
    public string? ErrorMessage { get; set; }
    public string Message { get; set; } = "";
}

public class SyncResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = "";
    public int SyncedCount { get; set; }
}