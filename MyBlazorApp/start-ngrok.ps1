# Start ngrok tunnel for Stripe testing
# This script starts ngrok on port 7184 and keeps it running

param(
    [int]$Port = 7184
)

Write-Host "Starting ngrok tunnel on port $Port..." -ForegroundColor Green
Write-Host "Monitor ngrok at: http://127.0.0.1:4040" -ForegroundColor Cyan
Write-Host ""
Write-Host "Once ngrok starts, copy the Forwarding URL and update appsettings.json:" -ForegroundColor Yellow
Write-Host "  1. Set Ngrok:Enabled to true" -ForegroundColor Yellow
Write-Host "  2. Set Ngrok:Url to the https URL from below" -ForegroundColor Yellow
Write-Host ""
Write-Host "Press Ctrl+C to stop ngrok" -ForegroundColor Cyan

ngrok http $Port --bind-tls=true
