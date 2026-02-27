# Senior-ready checklist (ejecución)

## 1) Arquitectura de casos de uso
- [x] Extraer caso de uso de creación de órdenes por lote a servicio dedicado (`OrderBatchService`).
- [x] Extraer reportes a servicio dedicado (`ReportsService`).
- [x] Extraer turnos a servicio dedicado (`ShiftService`).
- [x] Extraer inventario a servicio dedicado (`InventoryService`).
- [ ] Extraer pagos/caja y administración restante a casos de uso dedicados.

## 2) Error handling estándar
- [x] Middleware global en formato `ProblemDetails`.
- [x] Códigos de error de negocio iniciales para órdenes, reportes, turnos e inventario.
- [x] Helper de claims (`bid`/`sub`) con errores consistentes (`ApiProblemException`).
- [ ] Unificar todos los controladores restantes al mismo contrato de error.

## 3) Pruebas
- [x] Tests de auth flow existentes.
- [x] Nuevos unit tests de servicios (`OrderBatchService`, `ReportsService`, `ShiftService`, `InventoryService`).
- [ ] Integration tests API+DB para endpoints críticos.

## 4) Documentación operativa
- [x] README base con setup/build/run/test.
- [x] Runbooks operativos (`LOCAL_DEV`, `INCIDENT_RESPONSE`).
- [x] Bitácora integral del proyecto (`PROJECT_BITACORA_SENIOR_READY`).

## 5) Cliente WPF
- [x] Extraída política de roles por modo de dispositivo (`DeviceRolePolicy`) fuera de `MainWindow`.
- [ ] Seguir reduciendo orquestación de sesión/navegación/SignalR de `MainWindow`.

## 6) CI/CD quality gates
- [x] Pipeline con restore/build/test de solución en Release.
- [x] Recolección y publicación de cobertura.
- [x] Umbral mínimo de cobertura (60%) en CI.

## 7) Resiliencia/performance
- [ ] Idempotencia y reglas anti-reintentos duplicados para operaciones críticas.
- [ ] Pruebas de carga base para endpoints clave.
