# Validación final E2E (PowerShell)

Se agregó un script único para ejecutar un flujo completo de validación:

1. `GET /api/business/public`
2. `POST /api/auth/login`
3. `POST /api/license/activate` (opcional con `-SkipActivate`)
4. `GET /api/license/status` y `GET /api/license/alerts`
5. `GET /api/auth/me`
6. Preparación de catálogo (categoría/producto)
7. Apertura de mesa y creación de comanda
8. Verificación de colas cocina/bar (y transición a `DONE`)
9. Apertura de turno y cobro en caja
10. Cierre de mesa

## Ejecutar

```powershell
pwsh -File .\scripts\validate-e2e.ps1
```

Opcional (si la licencia ya está activa):

```powershell
pwsh -File .\scripts\validate-e2e.ps1 -SkipActivate
```

Opcional (ambiente custom):

```powershell
pwsh -File .\scripts\validate-e2e.ps1 -BaseUrl "http://localhost:5139" -Username "admin" -Password "admin" -ActivationKey "BMT-DEMO-00000"
```
