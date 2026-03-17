using Microsoft.Extensions.Configuration;

namespace MyBlazorApp.Services
{
    public class UrlConfigurationService
    {
        private readonly IConfiguration _configuration;

        public UrlConfigurationService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GetBaseUrl()
        {
            var ngrokEnabled = _configuration.GetValue<bool>("AppSettings:Ngrok:Enabled");
            
            if (ngrokEnabled)
            {
                var ngrokUrl = _configuration.GetValue<string>("AppSettings:Ngrok:Url");
                if (!string.IsNullOrEmpty(ngrokUrl))
                {
                    return ngrokUrl;
                }
            }

            // Fallback to localhost with configured port
            var port = _configuration.GetValue<int>("AppSettings:Port");
            return $"http://localhost:{port}";
        }

        public string GetStripeSuccessUrl()
        {
            return $"{GetBaseUrl()}/payment-success";
        }

        public string GetStripeCancelUrl()
        {
            return $"{GetBaseUrl()}/payment-cancel";
        }
    }
}
