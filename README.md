# BMTECHRD.Pos

Sistema POS compuesto por API ASP.NET Core, cliente WPF, capa de dominio/aplicación/infraestructura y pruebas de flujo de autenticación.

## Estructura

- `BMTECHRD.Pos.Api`: API REST + SignalR + middleware.
- `BMTECHRD.Pos.App`: cliente WPF.
- `BMTECHRD.Pos.Domain`: entidades y enums de negocio.
- `BMTECHRD.Pos.Application`: DTOs, contratos y utilidades de aplicación.
- `BMTECHRD.Pos.Infrastructure`: EF Core, auth, persistence.
- `BMTECHRD.Pos.Auth.Core`: sesión auth cliente, refresh handler.
- `BMTECHRD.Pos.AuthHarness.Tests`: pruebas del flujo de refresh/auth.

## Requisitos

- .NET SDK 8.0+
- PostgreSQL 15+

## Configuración rápida

1. Configura la conexión en `BMTECHRD.Pos.Api/appsettings.Development.json`:
   - `ConnectionStrings:DefaultConnection`
2. Configura JWT en el mismo archivo:
   - `Jwt:SigningKey` (mínimo 32 chars)
   - `Jwt:Issuer`
   - `Jwt:Audience`
   - `Jwt:AccessTokenMinutes`
   - `Jwt:RefreshTokenDays`

## Comandos útiles

```bash
# Restaurar paquetes
dotnet restore BMTECHRD.Pos.slnx

# Build release
dotnet build BMTECHRD.Pos.slnx -c Release --no-restore

# Ejecutar API
dotnet run --project BMTECHRD.Pos.Api

# Ejecutar tests
dotnet test BMTECHRD.Pos.slnx --no-build
```

## Migraciones (EF Core)

```bash
# Crear migración (ejemplo)
dotnet ef migrations add NombreMigracion --project BMTECHRD.Pos.Infrastructure --startup-project BMTECHRD.Pos.Api

# Aplicar migraciones al arrancar API
# (ya se ejecuta Database.Migrate() en Development)
```

## Estado actual y próximos pasos

- Seguridad auth endurecida (JWT + refresh rotation + device binding + reuse detection).
- Refactor inicial aplicado para mover lógica de órdenes fuera de `OrdersController` hacia `OrderBatchService`.
- Manejo global de errores con `ProblemDetails` y códigos de error.

## Troubleshooting

- Si falla build por WPF en CI, valida imports y referencias de `BMTECHRD.Pos.App`.
- Si falla auth, revisa sincronía de `Issuer/Audience/SigningKey` entre API y cliente.
- Si falla conexión DB, valida cadena `DefaultConnection` y permisos del usuario PostgreSQL.
