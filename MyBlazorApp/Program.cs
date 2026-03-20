using MyBlazorApp.Components;
using MyBlazorApp.Services;
using MyBlazorApp.Data;
using MyBlazorApp.Hubs;
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

// Add SignalR for real-time updates
builder.Services.AddSignalR();

// Add custom services
builder.Services.AddScoped<StripeService>();
builder.Services.AddScoped<StripeWebhookService>(); // Webhook service
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<PaymentService>();
builder.Services.AddScoped<RaffleService>(); // Raffle service
builder.Services.AddScoped<StripeSyncService>(); // Stripe sync service
builder.Services.AddScoped<UrlConfigurationService>(); // For dynamic URL configuration
builder.Services.AddScoped<SessionService>(); // Scoped - one per circuit/connection
builder.Services.AddSingleton<SessionStore>(); // Singleton - persists sessions across circuits
builder.Services.AddSingleton<AppAuthStateProvider>(); // Singleton - global auth state

// Add hosted service for expired reservation cleanup
builder.Services.AddHostedService<ReservationCleanupService>();

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
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseSession();
app.UseAntiforgery();

// Map API controllers (webhooks)
app.MapControllers();

// Map SignalR hub
app.MapHub<RaffleHub>("/rafflehub");

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
