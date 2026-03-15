using MyBlazorApp.Data;
using MyBlazorApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace MyBlazorApp.Services;

public class AuthService
{
    private readonly AppDbContext _context;

    public AuthService(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Registrar un nuevo usuario
    /// </summary>
    public async Task<(bool success, string message, User? user)> RegisterAsync(string username, string email, string password, string roleName = "Basic")
    {
        try
        {
            // Validar que el username no exista
            if (await _context.Users.AnyAsync(u => u.Username == username))
                return (false, "Username already exists", null);

            // Obtener el rol Basic (no validar email)
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
            if (role == null)
                return (false, "Role not found", null);

            // Hash de la contraseña
            var passwordHash = HashPassword(password);

            var user = new User
            {
                Username = username,
                Email = email,
                PasswordHash = passwordHash,
                RoleId = role.Id,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return (true, "User registered successfully", user);
        }
        catch (Exception ex)
        {
            return (false, $"Registration error: {ex.Message}", null);
        }
    }

    /// <summary>
    /// Login de usuario
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
