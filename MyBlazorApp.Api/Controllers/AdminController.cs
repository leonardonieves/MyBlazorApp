using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using MyBlazorApp.Api.Data;
using MyBlazorApp.Api.Services;
using MyBlazorApp.Shared.Models;

namespace MyBlazorApp.Api.Controllers;

/// <summary>
/// API controller for admin operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly PaymentService _paymentService;
    private readonly ILogger<AdminController> _logger;

    public AdminController(
        AppDbContext context,
        PaymentService paymentService,
        ILogger<AdminController> logger)
    {
        _context = context;
        _paymentService = paymentService;
        _logger = logger;
    }

    /// <summary>
    /// GET: api/admin/users
    /// Get all users (admin only)
    /// </summary>
    [HttpGet("users")]
    public async Task<ActionResult<List<UserDto>>> GetAllUsers()
    {
        try
        {
            var users = await _context.Users
                .Include(u => u.Role)
                .OrderByDescending(u => u.CreatedAt)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    RoleName = u.Role.Name,
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt
                })
                .ToListAsync();

            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting users");
            return StatusCode(500, new { error = "Error fetching users" });
        }
    }

    /// <summary>
    /// GET: api/admin/payments
    /// Get all payments (admin only)
    /// </summary>
    [HttpGet("payments")]
    public async Task<ActionResult<List<PaymentDto>>> GetAllPayments()
    {
        try
        {
            var payments = await _paymentService.GetAllPaymentsAsync();
            
            var paymentDtos = payments.Select(p => new PaymentDto
            {
                Id = p.Id,
                ProductName = p.ProductName,
                Amount = p.Amount,
                Currency = p.Currency ?? "usd",
                Status = p.Status,
                CreatedAt = p.CreatedAt,
                Quantity = p.Quantity
            }).ToList();

            return Ok(paymentDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payments");
            return StatusCode(500, new { error = "Error fetching payments" });
        }
    }

    /// <summary>
    /// GET: api/admin/stats
    /// Get admin dashboard stats
    /// </summary>
    [HttpGet("stats")]
    public async Task<ActionResult> GetStats()
    {
        try
        {
            var totalUsers = await _context.Users.CountAsync();
            var activeUsers = await _context.Users.CountAsync(u => u.IsActive);
            var totalRaffles = await _context.Raffles.CountAsync();
            var activeRaffles = await _context.Raffles.CountAsync(r => r.Status == Models.RaffleStatus.Active);
            var totalTicketsSold = await _context.Tickets.CountAsync(t => t.Status == Models.TicketStatus.Sold);
            var totalRevenue = await _context.Payments
                .Where(p => p.Status == "completed")
                .SumAsync(p => p.Amount);

            return Ok(new
            {
                TotalUsers = totalUsers,
                ActiveUsers = activeUsers,
                TotalRaffles = totalRaffles,
                ActiveRaffles = activeRaffles,
                TotalTicketsSold = totalTicketsSold,
                TotalRevenue = totalRevenue
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting stats");
            return StatusCode(500, new { error = "Error fetching stats" });
        }
    }

    /// <summary>
    /// PUT: api/admin/users/{id}/toggle-active
    /// Toggle user active status
    /// </summary>
    [HttpPut("users/{id}/toggle-active")]
    public async Task<ActionResult> ToggleUserActive(int id)
    {
        try
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound(new { error = "User not found" });

            user.IsActive = !user.IsActive;
            await _context.SaveChangesAsync();

            return Ok(new { success = true, isActive = user.IsActive });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling user status");
            return StatusCode(500, new { error = "Error updating user" });
        }
    }
}
