using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MyBlazorApp.Api.Data;
using MyBlazorApp.Api.Services;
using MyBlazorApp.Api.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add MySQL Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// Add CORS for Blazor WebAssembly
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient", policy =>
    {
        policy.WithOrigins(
                "https://localhost:7267",  // Blazor WASM actual
                "http://localhost:5031",   // Blazor WASM http
                "https://localhost:7200",
                "http://localhost:5200",
                "https://localhost:5001",
                "http://localhost:5000"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Add JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "DefaultSecretKeyThatIsAtLeast32Chars!!";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

// Add controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add HttpClient for services that need it
builder.Services.AddHttpClient();

// Add SignalR
builder.Services.AddSignalR();

// Add custom services
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<RaffleService>();
builder.Services.AddScoped<StripeService>();
builder.Services.AddScoped<StripeSyncService>();
builder.Services.AddScoped<StripeWebhookService>();
builder.Services.AddScoped<PaymentService>();

// Add hosted service for expired reservation cleanup
builder.Services.AddHostedService<ReservationCleanupService>();

// Configure Stripe
Stripe.StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

var app = builder.Build();

// Apply migrations and seed data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        // Check if database exists and tables are created
        // If using existing database with tables, don't run migrations
        var pendingMigrations = await db.Database.GetPendingMigrationsAsync();
        var appliedMigrations = await db.Database.GetAppliedMigrationsAsync();

        if (!appliedMigrations.Any() && await db.Database.CanConnectAsync())
        {
            // Database exists but no migrations recorded - this is an existing database
            // Just ensure we can connect, don't try to migrate
            logger.LogInformation("Database exists with tables. Skipping migrations.");
        }
        else if (pendingMigrations.Any())
        {
            // There are pending migrations to apply
            logger.LogInformation("Applying {Count} pending migrations...", pendingMigrations.Count());
            await db.Database.MigrateAsync();
        }

        // Seed demo users if they don't exist
        await SeedDemoUsers(db);
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Database migration skipped. Tables may already exist.");

        // Still try to seed users
        try
        {
            await SeedDemoUsers(db);
        }
        catch (Exception seedEx)
        {
            logger.LogWarning(seedEx, "User seeding skipped.");
        }
    }
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowBlazorClient");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<RaffleHub>("/rafflehub");

app.Run();

// Helper method to seed demo users
async Task SeedDemoUsers(AppDbContext db)
{
    try
    {
        // Check if admin user already exists
        if (await db.Users.AnyAsync(u => u.Username == "admin"))
            return;

        // Ensure roles exist
        var adminRole = await db.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
        var basicRole = await db.Roles.FirstOrDefaultAsync(r => r.Name == "Basic");

        if (adminRole == null || basicRole == null)
        {
            if (adminRole == null)
                adminRole = new MyBlazorApp.Api.Models.Role { Name = "Admin" };
            if (basicRole == null)
                basicRole = new MyBlazorApp.Api.Models.Role { Name = "Basic" };

            db.Roles.AddRange(adminRole, basicRole);
            await db.SaveChangesAsync();
        }

        // Create demo users
        var adminUser = new MyBlazorApp.Api.Models.User
        {
            Username = "admin",
            Email = "admin@example.com",
            PasswordHash = HashPasswordForSeed("admin123"),
            RoleId = adminRole.Id,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var basicUser = new MyBlazorApp.Api.Models.User
        {
            Username = "user",
            Email = "user@example.com",
            PasswordHash = HashPasswordForSeed("user123"),
            RoleId = basicRole.Id,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        db.Users.AddRange(adminUser, basicUser);
        await db.SaveChangesAsync();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error seeding users: {ex.Message}");
    }
}

// Helper method to hash passwords
string HashPasswordForSeed(string password)
{
    using (var sha256 = System.Security.Cryptography.SHA256.Create())
    {
        var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }
}
