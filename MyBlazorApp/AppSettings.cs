namespace MyBlazorApp;

/// <summary>
/// Configuration for Stripe and environment-specific settings
/// This class is kept for backward compatibility. Use UrlConfigurationService instead.
/// </summary>
[Obsolete("Use UrlConfigurationService instead")]
public static class AppSettings
{
    /// <summary>
    /// Get the base URL for callbacks (localhost for dev, ngrok for local testing, production URL for prod)
    /// </summary>
    public static string GetBaseUrlForStripe()
    {
        // Get ngrok URL from environment variable
        var ngrokUrl = Environment.GetEnvironmentVariable("NGROK_URL");
        if (!string.IsNullOrEmpty(ngrokUrl))
        {
            return ngrokUrl.TrimEnd('/');
        }

        // Get environment
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        if (environment == "Production")
        {
            return "https://yourdomain.com"; // Update to your actual domain
        }

        // Local development fallback
        return "http://localhost:7184";
    }
}
