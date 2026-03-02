# ========================================
# Script de Configuración de Base de Datos
# BMTECHRD.Pos - PostgreSQL Database Setup
# ========================================

param(
    [string]$PostgresHost = "localhost",
    [string]$PostgresPort = "5432",
    [string]$PostgresUser = "postgres",
    [string]$PostgresPassword = "0120",
    [string]$DatabaseName = "bmtechrd_pos"
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "BMTECHRD.Pos - Database Setup Script" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Paso 1: Verificar que PostgreSQL está instalado y corriendo
Write-Host "[1/5] Verificando instalación de PostgreSQL..." -ForegroundColor Yellow

$pgPath = "C:\Program Files\PostgreSQL\17\bin\psql.exe"
if (-not (Test-Path $pgPath)) {
    # Intentar encontrar psql en PATH
    $psqlCommand = Get-Command psql -ErrorAction SilentlyContinue
    if ($psqlCommand) {
        $pgPath = $psqlCommand.Source
        Write-Host "  ✓ PostgreSQL encontrado en: $pgPath" -ForegroundColor Green
    } else {
        Write-Host "  ✗ ERROR: PostgreSQL no encontrado" -ForegroundColor Red
        Write-Host "  Por favor instala PostgreSQL desde: https://www.postgresql.org/download/" -ForegroundColor Red
        exit 1
    }
} else {
    Write-Host "  ✓ PostgreSQL encontrado" -ForegroundColor Green
}

# Paso 2: Verificar conectividad a PostgreSQL
Write-Host "`n[2/5] Verificando conectividad a PostgreSQL..." -ForegroundColor Yellow

$env:PGPASSWORD = $PostgresPassword
$testConnection = & $pgPath -h $PostgresHost -p $PostgresPort -U $PostgresUser -d postgres -c "SELECT version();" 2>&1

if ($LASTEXITCODE -ne 0) {
    Write-Host "  ✗ ERROR: No se puede conectar a PostgreSQL" -ForegroundColor Red
    Write-Host "  Verifica que PostgreSQL esté corriendo en ${PostgresHost}:${PostgresPort}" -ForegroundColor Red
    Write-Host "  Verifica que el usuario '$PostgresUser' y contraseña sean correctos" -ForegroundColor Red
    exit 1
}

Write-Host "  ✓ Conexión exitosa a PostgreSQL" -ForegroundColor Green

# Paso 3: Verificar si la base de datos existe
Write-Host "`n[3/5] Verificando base de datos '$DatabaseName'..." -ForegroundColor Yellow

$checkDbQuery = "SELECT 1 FROM pg_database WHERE datname = '$DatabaseName';"
$dbExists = & $pgPath -h $PostgresHost -p $PostgresPort -U $PostgresUser -d postgres -t -c $checkDbQuery 2>&1

if ($dbExists -match "1") {
    Write-Host "  ✓ Base de datos '$DatabaseName' ya existe" -ForegroundColor Green
    $createDb = $false
} else {
    Write-Host "  ⚠ Base de datos '$DatabaseName' no existe" -ForegroundColor Yellow
    Write-Host "  Creando base de datos..." -ForegroundColor Yellow
    
    # Crear base de datos
    $createDbQuery = "CREATE DATABASE $DatabaseName WITH ENCODING='UTF8' LC_COLLATE='Spanish_Spain.1252' LC_CTYPE='Spanish_Spain.1252' TEMPLATE=template0;"
    & $pgPath -h $PostgresHost -p $PostgresPort -U $PostgresUser -d postgres -c $createDbQuery 2>&1 | Out-Null
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  ✓ Base de datos '$DatabaseName' creada exitosamente" -ForegroundColor Green
        $createDb = $true
    } else {
        Write-Host "  ✗ ERROR: No se pudo crear la base de datos" -ForegroundColor Red
        exit 1
    }
}

# Paso 4: Verificar instalación de dotnet-ef tool
Write-Host "`n[4/5] Verificando herramienta dotnet-ef..." -ForegroundColor Yellow

$efInstalled = dotnet tool list -g | Select-String "dotnet-ef"

if (-not $efInstalled) {
    Write-Host "  ⚠ dotnet-ef no está instalado globalmente" -ForegroundColor Yellow
    Write-Host "  Instalando dotnet-ef..." -ForegroundColor Yellow
    dotnet tool install --global dotnet-ef
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  ✓ dotnet-ef instalado exitosamente" -ForegroundColor Green
    } else {
        Write-Host "  ✗ ERROR: No se pudo instalar dotnet-ef" -ForegroundColor Red
        exit 1
    }
} else {
    Write-Host "  ✓ dotnet-ef ya está instalado" -ForegroundColor Green
}

# Paso 5: Aplicar migraciones de Entity Framework
Write-Host "`n[5/5] Aplicando migraciones de Entity Framework..." -ForegroundColor Yellow

# Navegar al directorio del proyecto API
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $scriptPath

# Construir cadena de conexión
$connectionString = "Host=$PostgresHost;Port=$PostgresPort;Database=$DatabaseName;Username=$PostgresUser;Password=$PostgresPassword"

# Aplicar migraciones
Write-Host "  Ejecutando: dotnet ef database update..." -ForegroundColor Cyan

$env:ConnectionStrings__DefaultConnection = $connectionString
dotnet ef database update --project "../BMTECHRD.Pos.Infrastructure/BMTECHRD.Pos.Infrastructure.csproj" --startup-project "BMTECHRD.Pos.Api.csproj"

if ($LASTEXITCODE -eq 0) {
    Write-Host "  ✓ Migraciones aplicadas exitosamente" -ForegroundColor Green
} else {
    Write-Host "  ✗ ERROR: No se pudieron aplicar las migraciones" -ForegroundColor Red
    Write-Host "  Verifica los logs arriba para más detalles" -ForegroundColor Red
    exit 1
}

# Paso 6: Verificar tablas creadas
Write-Host "`n[6/6] Verificando tablas creadas..." -ForegroundColor Yellow

$checkTablesQuery = @"
SELECT COUNT(*) as table_count 
FROM information_schema.tables 
WHERE table_schema = 'public' 
AND table_type = 'BASE TABLE';
"@

$tableCount = & $pgPath -h $PostgresHost -p $PostgresPort -U $PostgresUser -d $DatabaseName -t -c $checkTablesQuery 2>&1

if ($tableCount -match "\d+") {
    $count = [int]($tableCount -replace '\s+', '')
    if ($count -gt 0) {
        Write-Host "  ✓ Base de datos configurada con $count tablas" -ForegroundColor Green
        
        # Listar tablas
        Write-Host "`n  Tablas creadas:" -ForegroundColor Cyan
        $listTablesQuery = "SELECT tablename FROM pg_tables WHERE schemaname = 'public' ORDER BY tablename;"
        & $pgPath -h $PostgresHost -p $PostgresPort -U $PostgresUser -d $DatabaseName -c $listTablesQuery
    } else {
        Write-Host "  ⚠ ADVERTENCIA: No se encontraron tablas en la base de datos" -ForegroundColor Yellow
    }
}

# Resumen final
Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "RESUMEN" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "✓ PostgreSQL: Funcionando" -ForegroundColor Green
Write-Host "✓ Base de datos: $DatabaseName" -ForegroundColor Green
Write-Host "✓ Host: ${PostgresHost}:${PostgresPort}" -ForegroundColor Green
Write-Host "✓ Usuario: $PostgresUser" -ForegroundColor Green
Write-Host "✓ Migraciones: Aplicadas" -ForegroundColor Green
Write-Host "`n¡Base de datos lista para usar!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan

# Limpiar variable de entorno
Remove-Item Env:\PGPASSWORD -ErrorAction SilentlyContinue
Remove-Item Env:\ConnectionStrings__DefaultConnection -ErrorAction SilentlyContinue
