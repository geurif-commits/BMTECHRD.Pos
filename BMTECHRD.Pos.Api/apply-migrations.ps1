# ========================================
# Script Simplificado - Solo Migraciones
# BMTECHRD.Pos - Apply EF Migrations
# ========================================

Write-Host "Aplicando migraciones de Entity Framework..." -ForegroundColor Cyan

# Navegar al directorio del proyecto
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $scriptPath

# Verificar que dotnet-ef está instalado
$efInstalled = dotnet tool list -g | Select-String "dotnet-ef"

if (-not $efInstalled) {
    Write-Host "Instalando dotnet-ef..." -ForegroundColor Yellow
    dotnet tool install --global dotnet-ef
}

# Aplicar migraciones
Write-Host "`nEjecutando migraciones..." -ForegroundColor Yellow
dotnet ef database update `
    --project "../BMTECHRD.Pos.Infrastructure/BMTECHRD.Pos.Infrastructure.csproj" `
    --startup-project "BMTECHRD.Pos.Api.csproj" `
    --verbose

if ($LASTEXITCODE -eq 0) {
    Write-Host "`n✓ Migraciones aplicadas exitosamente!" -ForegroundColor Green
} else {
    Write-Host "`n✗ Error al aplicar migraciones" -ForegroundColor Red
    exit 1
}
