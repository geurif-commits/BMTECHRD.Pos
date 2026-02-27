# Runbook: Local Development

## Objetivo
Levantar API + base de datos local y validar un ciclo básico de trabajo.

## Prerrequisitos
- .NET SDK 8+
- PostgreSQL 15+

## Pasos
1. Configurar `BMTECHRD.Pos.Api/appsettings.Development.json` con:
   - `ConnectionStrings:DefaultConnection`
   - Bloque `Jwt`
2. Restaurar paquetes:
   - `dotnet restore BMTECHRD.Pos.slnx`
3. Compilar:
   - `dotnet build BMTECHRD.Pos.slnx -c Release --no-restore`
4. Ejecutar API:
   - `dotnet run --project BMTECHRD.Pos.Api`
5. Validar `/swagger` y endpoint `/api/health`.

## Diagnóstico rápido
- Error DB: validar host/puerto/credenciales de PostgreSQL.
- Error JWT: validar `SigningKey`, `Issuer`, `Audience`.
- Error WPF build: revisar referencias/imports del proyecto `BMTECHRD.Pos.App`.
