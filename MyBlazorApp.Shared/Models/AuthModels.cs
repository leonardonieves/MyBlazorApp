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
    public List<SyncedRaffleInfo> Raffles { get; set; } = new();
}

/// <summary>
/// Info about a synced raffle from Stripe
/// </summary>
public class SyncedRaffleInfo
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? StripeProductId { get; set; }
    public int TotalTickets { get; set; }
    public decimal TicketPrice { get; set; }
}

/// <summary>
/// Sync status response
/// </summary>
public class SyncStatusResponse
{
    public int TotalRaffles { get; set; }
    public int ActiveRaffles { get; set; }
    public int TotalTicketsGenerated { get; set; }
    public int TotalTicketsSold { get; set; }
    public DateTime LastChecked { get; set; }
}

/// <summary>
/// Payment DTO for API responses
/// </summary>
public class PaymentDto
{
    public int Id { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "usd";
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int Quantity { get; set; }
}

/// <summary>
/// Ticket status DTO for visual selector
/// </summary>
public class TicketStatusDto
{
    public int TicketId { get; set; }
    public int RaffleId { get; set; }
    public string DisplayNumber { get; set; } = string.Empty;
    public string Status { get; set; } = "available"; // "available", "reserved", "sold"
    public int? ReservedByUserId { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

/// <summary>
/// Response for ticket reservation
/// </summary>
public class ReserveTicketsResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<int> ReservedTicketIds { get; set; } = new();
    public List<int> UnavailableTicketIds { get; set; } = new();
    public DateTime? ExpiresAt { get; set; }
}

/// <summary>
/// User DTO for admin panel
/// </summary>
public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}