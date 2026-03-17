using MyBlazorApp.Components;
using MyBlazorApp.Services;
using MyBlazorApp.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add MySQL Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpClient();

// Add controllers for API endpoints (webhooks)
builder.Services.AddControllers();

// Add custom services
builder.Services.AddScoped<StripeService>();
builder.Services.AddScoped<StripeWebhookService>(); // Webhook service
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<PaymentService>();
builder.Services.AddScoped<UrlConfigurationService>(); // For dynamic URL configuration
builder.Services.AddSingleton<SessionService>(); // Singleton to maintain sessions across circuits

// Add distributed cache for sessions
builder.Services.AddDistributedMemoryCache();

// Add session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
});

var app = builder.Build();

// Create database if it doesn't exist
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseSession();
app.UseAntiforgery();

// Map API controllers (webhooks)
app.MapControllers();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
