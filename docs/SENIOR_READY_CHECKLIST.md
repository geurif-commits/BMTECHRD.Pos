# Senior-ready checklist (ejecución)

## 1) Arquitectura de casos de uso
- [x] Extraer caso de uso de creación de órdenes por lote a servicio dedicado (`OrderBatchService`).
- [x] Extraer reportes a servicio dedicado (`ReportsService`).
- [x] Extraer turnos a servicio dedicado (`ShiftService`).
- [x] Extraer inventario a servicio dedicado (`InventoryService`).
- [x] Extraer caja/pagos a servicio dedicado (`CashierService`).
- [x] Extraer users/products/tables a servicios dedicados.
- [x] Extraer categorías/licencias/negocio público a servicios dedicados.
- [x] Extraer inventory-movements a servicio dedicado.
- [x] Extraídos módulos administrativos residuales detectados en auditoría (`Auth`, `Kitchen`, `Bar`) a servicios dedicados.

## 2) Error handling estándar
- [x] Middleware global en formato `ProblemDetails`.
- [x] Códigos de error de negocio iniciales para órdenes, reportes, turnos, inventario y caja.
- [x] Helper de claims (`bid`/`sub`) con errores consistentes (`ApiProblemException`).
- [x] Unificar contrato de error para controladores críticos ya migrados (incluye categorías/licencias/negocio público).
- [x] Cerrados endpoints residuales relevantes al contrato (`Auth`, `Kitchen`, `Bar`) con `ApiProblemException`.

## 3) Pruebas
- [x] Tests de auth flow existentes.
- [x] Nuevos unit tests de servicios (`OrderBatchService`, `ReportsService`, `ShiftService`, `InventoryService`, `CashierService`, `UsersService`, `ProductsService`, `TablesService`, `CategoriesService`, `LicenseActivationService`, `BusinessService`, `InventoryMovementsService`, `OrderBatchIdempotency`, `ShiftIdempotency`).
- [x] Integration tests API+DB expandidos con `WebApplicationFactory` (health + auth flow + activación de licencia).

## 4) Documentación operativa
- [x] README base con setup/build/run/test.
- [x] Runbooks operativos (`LOCAL_DEV`, `INCIDENT_RESPONSE`, `DEPLOY_ROLLBACK`).
- [x] Runbook de performance baseline (`PERFORMANCE_BASELINE`).
- [x] Bitácora integral del proyecto (`PROJECT_BITACORA_SENIOR_READY`).
- [x] Definition of Done (`DEFINITION_OF_DONE`).

## 5) Cliente WPF
- [x] Extraída política de roles por modo de dispositivo (`DeviceRolePolicy`) fuera de `MainWindow`.
- [x] Reducida orquestación de `MainWindow` con `IMainWindowSessionOrchestrator`.
- [x] Desacople adicional aplicado con `IMainWindowNavigationCoordinator` para navegación/sesión en `MainWindow`.

## 6) CI/CD quality gates
- [x] Pipeline con restore/build/test de solución en Release.
- [x] Recolección y publicación de cobertura.
- [x] Umbral mínimo de cobertura (60%) en CI.

## 7) Resiliencia/performance
- [x] Idempotencia básica en pagos de caja vía `Idempotency-Key`.
- [x] Prueba de carga base inicial (`k6_cashier_smoke.js`).
- [x] Endurecida idempotencia en operaciones críticas adicionales (`Orders batch`, `Shifts open/close`, `Cashier payments`).
- [x] Consolidado almacenamiento/expiración robusta de llaves de idempotencia (store dedicado con TTL y compactación).
