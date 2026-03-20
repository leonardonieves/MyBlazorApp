# Script para ejecutar API y Web simultáneamente
# Uso: .\run-both.ps1

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  🏆 World Cup Sweepstakes - Launcher   " -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Verificar que estamos en el directorio correcto
if (-not (Test-Path "MyBlazorApp.Api")) {
    Write-Host "ERROR: No se encontró el directorio MyBlazorApp.Api" -ForegroundColor Red
    Write-Host "Ejecuta este script desde la raíz del proyecto." -ForegroundColor Yellow
    exit 1
}

Write-Host "Iniciando API y Web..." -ForegroundColor Green
Write-Host ""

# Inicia API en una nueva ventana
Write-Host "📌 Lanzando API..." -ForegroundColor Yellow
Write-Host "   Puerto: 7100" -ForegroundColor Gray
Write-Host "   URL: https://localhost:7100" -ForegroundColor Gray
Write-Host "   Swagger: https://localhost:7100/swagger" -ForegroundColor Gray

$apiWindow = Start-Process powershell -ArgumentList `
    "-NoExit", `
    "-Command", `
    "cd '$PSScriptRoot\MyBlazorApp.Api'; Write-Host ''; Write-Host 'API iniciando...' -ForegroundColor Green; dotnet run" `
    -PassThru

# Espera a que API inicie
Write-Host ""
Start-Sleep -Seconds 4

# Inicia Web en otra ventana
Write-Host "📌 Lanzando Web (Blazor WebAssembly)..." -ForegroundColor Yellow
Write-Host "   Puerto: 7200" -ForegroundColor Gray
Write-Host "   URL: https://localhost:7200" -ForegroundColor Gray
Write-Host ""

$webWindow = Start-Process powershell -ArgumentList `
    "-NoExit", `
    "-Command", `
    "cd '$PSScriptRoot\MyBlazorApp.Web'; Write-Host ''; Write-Host 'Web iniciando...' -ForegroundColor Green; dotnet run" `
    -PassThru

# Espera un poco más
Start-Sleep -Seconds 2

Write-Host "========================================" -ForegroundColor Green
Write-Host "✅ Ambos proyectos iniciados!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "📊 API Info:" -ForegroundColor Cyan
Write-Host "   - URL: https://localhost:7100" -ForegroundColor White
Write-Host "   - Swagger: https://localhost:7100/swagger" -ForegroundColor White
Write-Host ""
Write-Host "🌐 Web Info:" -ForegroundColor Cyan
Write-Host "   - URL: https://localhost:7200" -ForegroundColor White
Write-Host "   - Usuario Demo: admin / admin123" -ForegroundColor White
Write-Host ""
Write-Host "🗄️ Base de Datos:" -ForegroundColor Cyan
Write-Host "   - Servidor: localhost:3306" -ForegroundColor White
Write-Host "   - Usuario: root" -ForegroundColor White
Write-Host ""
Write-Host "⏹️  Para detener, cierra ambas ventanas de PowerShell" -ForegroundColor Yellow
Write-Host ""
