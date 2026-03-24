using MyBlazorApp.Api.Data;
using MyBlazorApp.Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace MyBlazorApp.Api.Services;

public class AuthService
{
    private readonly AppDbContext _context;
    private readonly StripeService _stripeService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(AppDbContext context, StripeService stripeService, ILogger<AuthService> logger)
    {
        _context = context;
        _stripeService = stripeService;
        _logger = logger;
    }

    /// <summary>
    /// Register a new user and create customer in Stripe
    /// </summary>
    public async Task<(bool success, string message, User? user)> RegisterAsync(string username, string email, string password, string roleName = "Basic")
    {
        try
        {
            // Validate that the username does not exist
            if (await _context.Users.AnyAsync(u => u.Username == username))
                return (false, "Username already exists", null);

            // Validate that the email does not exist
            if (await _context.Users.AnyAsync(u => u.Email == email))
                return (false, "Email already exists", null);

            // Get the role
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
            if (role == null)
                return (false, "Role not found", null);

            // Hash the password
            var passwordHash = HashPassword(password);

            // Create user
            var user = new User
            {
                Username = username,
                Email = email,
                PasswordHash = passwordHash,
                RoleId = role.Id,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            // Create customer in Stripe
            try
            {
                var stripeCustomer = await _stripeService.CreateCustomerAsync(
                    email: email,
                    name: username,
                    metadata: new Dictionary<string, string>
                    {
                        { "username", username },
                        { "user_role", roleName }
                    }
                );

                user.StripeCustomerId = stripeCustomer.Id;
                user.StripeCustomerCreatedAt = DateTime.UtcNow;

                _logger.LogInformation($"Stripe customer created for user {username}: {stripeCustomer.Id}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating Stripe customer for {username}: {ex.Message}");
                // Continue without Stripe customer - can be added later
            }

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"User registered successfully: {username} (ID: {user.Id})");
            return (true, "User registered successfully", user);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Registration error: {ex.Message}");
            return (false, $"Registration error: {ex.Message}", null);
        }
    }

    /// <summary>
    /// User login
    /// </summary>
    public async Task<(bool success, string message, User? user)> LoginAsync(string username, string password)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Username == username);

        if (user == null || !user.IsActive)
            return (false, "Invalid username or password", null);

        if (!VerifyPassword(password, user.PasswordHash))
            return (false, "Invalid username or password", null);

        return (true, "Login successful", user);
    }

    /// <summary>
    /// Obtener usuario por ID
    /// </summary>
    public async Task<User?> GetUserByIdAsync(int userId)
    {
        return await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);
    }

    /// <summary>
    /// Obtener todos los usuarios
    /// </summary>
    public async Task<List<User>> GetAllUsersAsync()
    {
        return await _context.Users
            .Include(u => u.Role)
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Cambiar contraseña
    /// </summary>
    public async Task<(bool success, string message)> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return (false, "User not found");

        if (!VerifyPassword(currentPassword, user.PasswordHash))
            return (false, "Current password is incorrect");

        user.PasswordHash = HashPassword(newPassword);
        _context.Users.Update(user);
        await _context.SaveChangesAsync();

        return (true, "Password changed successfully");
    }

    /// <summary>
    /// Create or update Stripe Customer for an existing user
    /// </summary>
    public async Task<(bool success, string message, string? customerId)> EnsureStripeCustomerAsync(int userId)
    {
        try
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return (false, "User not found", null);

            // Si ya tiene customer ID, verificar que existe en Stripe
            if (!string.IsNullOrEmpty(user.StripeCustomerId))
            {
                var existingCustomer = await _stripeService.GetCustomerAsync(user.StripeCustomerId);
                if (existingCustomer != null)
                    return (true, "Customer already exists", user.StripeCustomerId);
            }

            // Crear nuevo customer
            var stripeCustomer = await _stripeService.CreateCustomerAsync(
                email: user.Email,
                name: user.Username,
                metadata: new Dictionary<string, string>
                {
                    { "user_id", user.Id.ToString() },
                    { "username", user.Username }
                }
            );

            user.StripeCustomerId = stripeCustomer.Id;
            user.StripeCustomerCreatedAt = DateTime.UtcNow;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Stripe customer created for existing user {user.Username}: {stripeCustomer.Id}");
            return (true, "Stripe customer created successfully", stripeCustomer.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error ensuring Stripe customer: {ex.Message}");
            return (false, $"Error creating Stripe customer: {ex.Message}", null);
        }
    }

    /// <summary>
    /// Obtener usuario por email
    /// </summary>
    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == email && u.IsActive);
    }

    // Métodos helper
    private string HashPassword(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }

    private bool VerifyPassword(string password, string hash)
    {
        var hashOfInput = HashPassword(password);
        return hashOfInput.Equals(hash);
    }
}
