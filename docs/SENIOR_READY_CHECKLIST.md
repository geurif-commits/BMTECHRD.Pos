# Senior-ready checklist (ejecución)

## 1) Arquitectura de casos de uso
- [x] Extraer caso de uso de creación de órdenes por lote a servicio dedicado (`OrderBatchService`).
- [x] Extraer reportes a servicio dedicado (`ReportsService`).
- [x] Extraer turnos a servicio dedicado (`ShiftService`).
- [x] Extraer inventario a servicio dedicado (`InventoryService`).
- [x] Extraer caja/pagos a servicio dedicado (`CashierService`).
- [x] Extraer users/products/tables a servicios dedicados.
- [ ] Extraer categorías/licencias/negocio público/resto administrativo a casos de uso dedicados.

## 2) Error handling estándar
- [x] Middleware global en formato `ProblemDetails`.
- [x] Códigos de error de negocio iniciales para órdenes, reportes, turnos, inventario y caja.
- [x] Helper de claims (`bid`/`sub`) con errores consistentes (`ApiProblemException`).
- [ ] Unificar todos los controladores restantes al mismo contrato de error (categorías/licencias/negocio público y restantes).

## 3) Pruebas
- [x] Tests de auth flow existentes.
- [x] Nuevos unit tests de servicios (`OrderBatchService`, `ReportsService`, `ShiftService`, `InventoryService`, `CashierService`).
- [x] Integration tests API+DB iniciales (smoke endpoint API con `WebApplicationFactory`).

## 4) Documentación operativa
- [x] README base con setup/build/run/test.
- [x] Runbooks operativos (`LOCAL_DEV`, `INCIDENT_RESPONSE`, `DEPLOY_ROLLBACK`).
- [x] Runbook de performance baseline (`PERFORMANCE_BASELINE`).
- [x] Bitácora integral del proyecto (`PROJECT_BITACORA_SENIOR_READY`).
- [x] Definition of Done (`DEFINITION_OF_DONE`).

## 5) Cliente WPF
- [x] Extraída política de roles por modo de dispositivo (`DeviceRolePolicy`) fuera de `MainWindow`.
- [ ] Seguir reduciendo orquestación de sesión/navegación/SignalR de `MainWindow`.

## 6) CI/CD quality gates
- [x] Pipeline con restore/build/test de solución en Release.
- [x] Recolección y publicación de cobertura.
- [x] Umbral mínimo de cobertura (60%) en CI.

## 7) Resiliencia/performance
- [x] Idempotencia básica en pagos de caja vía `Idempotency-Key`.
- [x] Prueba de carga base inicial (`k6_cashier_smoke.js`).
- [ ] Endurecer idempotencia para más operaciones críticas (cierres/órdenes).
