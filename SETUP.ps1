#!/usr/bin/env powershell

Write-Host "🚀 MyBlazorApp - Setup Completo" -ForegroundColor Cyan

# Paso 1: Cambiar a carpeta MyBlazorApp
Write-Host "`n[1/5] Navegando a carpeta del proyecto..." -ForegroundColor Yellow
cd MyBlazorApp
Write-Host "✅ En: $(Get-Location)" -ForegroundColor Green

# Paso 2: Limpiar y compilar
Write-Host "`n[2/5] Limpiando y compilando proyecto..." -ForegroundColor Yellow
dotnet clean
dotnet build
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Error en compilación" -ForegroundColor Red
    exit 1
}
Write-Host "✅ Compilación exitosa" -ForegroundColor Green

# Paso 3: Aplicar migraciones
Write-Host "`n[3/5] Aplicando migraciones de base de datos..." -ForegroundColor Yellow
dotnet ef database drop --force
dotnet ef database update
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Error en migraciones" -ForegroundColor Red
    exit 1
}
Write-Host "✅ Base de datos actualizada" -ForegroundColor Green

# Paso 4: Crear usuarios demo
Write-Host "`n[4/5] Creando usuarios de demo..." -ForegroundColor Yellow
$seedScript = @"
USE MyBlazorAppDb;
DELETE FROM Users WHERE Username IN ('admin', 'user');
INSERT INTO Users (Username, Email, PasswordHash, RoleId, IsActive, CreatedAt)
VALUES 
  ('admin', 'admin@example.com', 'CxT1DSymjYMi0MOzQPLFO5oEofP/H6B+mOhufw+ieuM=', 1, 1, NOW()),
  ('user', 'user@example.com', 'pmWkWSBCL50Hfkh+79xPuKBKHzz/H6B+mOhufw+ieuM=', 2, 1, NOW());
SELECT * FROM Users;
"@

$seedScript | mysql -u root -p root
Write-Host "✅ Usuarios de demo creados" -ForegroundColor Green

# Paso 5: Ejecutar
Write-Host "`n[5/5] Iniciando aplicación..." -ForegroundColor Yellow
Write-Host "Abre: http://localhost:5091" -ForegroundColor Cyan
Write-Host "Credenciales: admin / admin123" -ForegroundColor Cyan
dotnet run

