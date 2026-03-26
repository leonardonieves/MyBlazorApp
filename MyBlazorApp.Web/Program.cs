using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Blazored.LocalStorage;
using MyBlazorApp.Web;
using MyBlazorApp.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Read API URL from configuration
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7100";

// Add HttpClient configured for API
builder.Services.AddScoped(sp => new HttpClient 
{ 
    BaseAddress = new Uri(apiBaseUrl) 
});

// Add Blazored LocalStorage for token persistence
builder.Services.AddBlazoredLocalStorage();

// Add custom services
builder.Services.AddScoped<AuthStateService>();
builder.Services.AddScoped<ApiService>();
builder.Services.AddSingleton<SignalRService>();

var host = builder.Build();

// Start SignalR connection
var signalR = host.Services.GetRequiredService<SignalRService>();
await signalR.StartAsync();

await host.RunAsync();
