# Development startup script
# This script starts both the Blazor app and ngrok tunnel in separate terminals

param(
    [int]$Port = 7184,
    [switch]$WithNgrok
)

$projectPath = Split-Path -Parent $PSCommandPath

Write-Host "=" * 60
Write-Host "MyBlazorApp Development Startup" -ForegroundColor Cyan
Write-Host "=" * 60
Write-Host ""

# Start the Blazor app
Write-Host "Starting Blazor app on port $Port..." -ForegroundColor Green
$appProcess = Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$projectPath'; dotnet run" -PassThru

Write-Host "Blazor app started with PID: $($appProcess.Id)" -ForegroundColor Green
Write-Host "App URL: http://localhost:$Port" -ForegroundColor Cyan
Write-Host ""

if ($WithNgrok) {
    Write-Host "Waiting 3 seconds before starting ngrok..." -ForegroundColor Yellow
    Start-Sleep -Seconds 3
    
    Write-Host "Starting ngrok tunnel..." -ForegroundColor Green
    $ngrokProcess = Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$projectPath'; powershell .\start-ngrok.ps1 -Port $Port" -PassThru
    
    Write-Host "ngrok started with PID: $($ngrokProcess.Id)" -ForegroundColor Green
    Write-Host "Monitor ngrok at: http://127.0.0.1:4040" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Remember to update appsettings.json after ngrok starts!" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Development environment ready." -ForegroundColor Green
Write-Host "Press Ctrl+C in this window to view options for stopping services." -ForegroundColor Cyan
