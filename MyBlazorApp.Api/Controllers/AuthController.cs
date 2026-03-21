using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MyBlazorApp.Api.Services;
using MyBlazorApp.Shared.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MyBlazorApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        AuthService authService,
        IConfiguration configuration,
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        try
        {
            _logger.LogInformation($"Login attempt for user: {request?.Username}");

            if (request == null || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new AuthResponse
                {
                    Success = false,
                    Message = "Username and password are required"
                });
            }

            var (success, message, user) = await _authService.LoginAsync(request.Username, request.Password);

            if (!success || user == null)
            {
                _logger.LogWarning($"Failed login for: {request.Username} - {message}");
                return Unauthorized(new AuthResponse
                {
                    Success = false,
                    Message = "Invalid username or password"
                });
            }

            // Reload user with role included
            var userWithRole = await _authService.GetUserByIdAsync(user.Id);
            if (userWithRole == null)
            {
                _logger.LogError($"User loaded but then not found: {user.Id}");
                return StatusCode(500, new AuthResponse
                {
                    Success = false,
                    Message = "An error occurred"
                });
            }

            var token = GenerateJwtToken(userWithRole);

            _logger.LogInformation($"User logged in: {userWithRole.Username}");

            return Ok(new AuthResponse
            {
                Success = true,
                Message = "Login successful",
                User = MapUserToAuthUserData(userWithRole),
                Token = token,
                TokenExpiration = DateTime.UtcNow.AddHours(24)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return StatusCode(500, new AuthResponse
            {
                Success = false,
                Message = "Server error during login"
            });
        }
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        try
        {
            _logger.LogInformation($"Register attempt for user: {request?.Username}");

            if (request == null || string.IsNullOrWhiteSpace(request.Username) || 
                string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new AuthResponse
                {
                    Success = false,
                    Message = "Username, email, and password are required"
                });
            }

            if (request.Password != request.ConfirmPassword)
            {
                return BadRequest(new AuthResponse
                {
                    Success = false,
                    Message = "Passwords do not match"
                });
            }

            if (request.Password.Length < 6)
            {
                return BadRequest(new AuthResponse
                {
                    Success = false,
                    Message = "Password must be at least 6 characters"
                });
            }

            var (success, message, user) = await _authService.RegisterAsync(
                request.Username,
                request.Email,
                request.Password,
                "Basic"
            );

            if (!success || user == null)
            {
                _logger.LogWarning($"Failed registration: {request.Username} - {message}");
                return BadRequest(new AuthResponse
                {
                    Success = false,
                    Message = message ?? "Registration failed"
                });
            }

            // Reload user with role included
            var userWithRole = await _authService.GetUserByIdAsync(user.Id);
            if (userWithRole == null)
            {
                _logger.LogError($"User created but then not found: {user.Id}");
                return StatusCode(500, new AuthResponse
                {
                    Success = false,
                    Message = "An error occurred"
                });
            }

            var token = GenerateJwtToken(userWithRole);

            _logger.LogInformation($"User registered: {userWithRole.Username}");

            return Ok(new AuthResponse
            {
                Success = true,
                Message = "Registration successful",
                User = MapUserToAuthUserData(userWithRole),
                Token = token,
                TokenExpiration = DateTime.UtcNow.AddHours(24)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration");
            return StatusCode(500, new AuthResponse
            {
                Success = false,
                Message = "Server error during registration"
            });
        }
    }

    private string GenerateJwtToken(MyBlazorApp.Api.Models.User user)
    {
        var jwtKey = _configuration["Jwt:Key"] ?? "DefaultSecretKeyThatIsAtLeast32Chars!!";
        var jwtIssuer = _configuration["Jwt:Issuer"] ?? "MyBlazorApp";
        var jwtAudience = _configuration["Jwt:Audience"] ?? "MyBlazorAppClient";

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var roleName = user.Role?.Name ?? "Basic";

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, roleName),
            new Claim("RoleName", roleName),
            new Claim("StripeCustomerId", user.StripeCustomerId ?? "")
        };

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private AuthUserData MapUserToAuthUserData(MyBlazorApp.Api.Models.User user)
    {
        return new AuthUserData
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            RoleName = user.Role?.Name ?? "Basic",
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            StripeCustomerId = user.StripeCustomerId
        };
    }
}
