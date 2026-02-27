# Bitácora técnica integral — BMTECHRD.Pos

## 1) Propósito de este documento
Esta bitácora consolida el estado real del proyecto, las decisiones ya aplicadas y la ruta técnica pendiente para finalizar el sistema con estándar **senior-ready**.

Está diseñada para que cualquier IA o desarrollador nuevo pueda:
- entender rápidamente la arquitectura actual,
- saber qué ya está resuelto,
- identificar qué falta y en qué orden,
- continuar sin perder consistencia de lógica ni estilo de implementación.

---

## 2) Objetivo final del sistema
Entregar un POS robusto y mantenible con:
1. arquitectura por capas + casos de uso desacoplados,
2. contratos de error uniformes y seguros,
3. pruebas automatizadas suficientes y confiables,
4. CI/CD con quality gates obligatorios,
5. documentación operativa completa,
6. cliente WPF con menor acoplamiento,
7. resiliencia para operaciones críticas (idempotencia y control de reintentos).

---

## 3) Estado actual por componentes

### 3.1 Backend API
**Avances aplicados**
- Refactor de órdenes por lote a `OrderBatchService`.
- Refactor de reportes a `ReportsService`.
- Refactor de turnos a `ShiftService`.
- Refactor de ajuste de inventario a `InventoryService`.
- Refactor de caja/pagos a `CashierService` con idempotencia básica por `Idempotency-Key`.
- Refactor de users/products/tables a servicios (`UsersService`, `ProductsService`, `TablesService`).
- Refactor de categorías/licencias/negocio público a servicios (`CategoriesService`, `LicenseActivationService`, `BusinessService`).
- Controladores críticos quedaron más delgados y delegan en servicios.

**Error handling**
- Excepción de negocio centralizada: `ApiProblemException`.
- Middleware global responde `ProblemDetails` con `traceId` y `code`.
- Helpers de claims: `GetRequiredBusinessId` y `GetRequiredUserId`.

### 3.2 Dominio y persistencia
- Entidades principales de POS presentes (negocio, usuarios, mesas, órdenes, ítems, pagos, turnos, inventario, licencias, auditoría).
- Configuraciones EF Core y migraciones versionadas.
- Corrección de warnings de ocultamiento de `CreatedAt` (uso consistente de `AuditableEntity`).

### 3.3 Cliente WPF
- Flujo auth endurecido en etapas previas (DeviceId, refresh flow).
- Extracción de política de rol por modo de dispositivo a `IDeviceRolePolicy` / `DeviceRolePolicy`.
- Se redujo orquestación de `MainWindow` al extraer coordinación post-login a `IMainWindowSessionOrchestrator`; queda mejora incremental de navegación avanzada.

### 3.4 Pruebas
- AuthHarness existente para refresh/reintentos/concurrencia.
- Nuevo proyecto `BMTECHRD.Pos.Api.Tests` con tests de servicios e integración:
  - `OrderBatchServiceTests`
  - `ReportsServiceTests`
  - `InventoryServiceTests`
  - `ShiftServiceTests`
  - `CashierServiceTests`
  - `HealthEndpointIntegrationTests`
  - `UsersServiceTests`
  - `ProductsServiceTests`
  - `TablesServiceTests`
  - `CategoriesServiceTests`
  - `LicenseActivationServiceTests`
  - `BusinessServiceTests`
  - `InventoryMovementsServiceTests`
  - `ShiftIdempotencyTests`
  - `OrderBatchIdempotencyTests`

### 3.5 CI/CD
- Pipeline CI en GitHub Actions con:
  - restore + build release,
  - test de solución,
  - cobertura y publicación de artefactos,
  - umbral mínimo de cobertura (60%).

### 3.6 Documentación
- README operativo actualizado.
- Runbooks de desarrollo local, incident response, performance baseline y deploy/rollback.
- Checklist senior-ready vivo para seguimiento.

---

## 4) Hitos aplicados (timeline resumido)
1. Diagnóstico de nivel y roadmap inicial.
2. Corrección de errores de compilación y warnings de entidades.
3. Extracción de Orders a service layer + ProblemDetails.
4. Extracción de Reports + claims helpers.
5. Implementación de pruebas unitarias de servicios.
6. Gating de cobertura en CI.
7. Extracción de Shift/Inventory a servicios.
8. Consolidación de bitácora y actualización de checklist.
9. Extracción de caja a servicio + idempotencia básica y baseline de performance.
10. Integración API inicial con `WebApplicationFactory` (health smoke).
11. Extracción adicional de users/products/tables a servicios + nuevos unit tests.
12. Extracción de categorías/licencias/negocio público + reducción adicional de orquestación WPF (`IMainWindowSessionOrchestrator`).
13. Endurecimiento de idempotencia en órdenes y turnos.
14. Extracción de categorías/licencias/negocio público/inventory-movements a servicios + estandarización de errores.
15. Reducción adicional de orquestación WPF con `IMainWindowSessionOrchestrator`.
16. Consolidación de almacenamiento de llaves de idempotencia con `IIdempotencyKeyStore` (TTL y compactación).

---

## 5) Principios de implementación adoptados
1. **Controlador delgado, servicio rico en negocio**.
2. **Errores de negocio explícitos** (código estable + mensaje controlado).
3. **Persistencia con transacciones en operaciones críticas**.
4. **Eventos de actualización (SignalR) después de persistir cambios**.
5. **Auditoría en acciones de reporte y autenticación**.
6. **DI explícita de servicios por bounded context funcional**.

---

## 6) Qué falta para cerrar “senior-ready”

### 6.1 Arquitectura
- Mantener vigilancia sobre nuevos endpoints administrativos para asegurar patrón de servicio desde su creación.
- Definir convención estable de carpetas por feature (`Services/<Feature>` + contratos).

### 6.2 Contratos de error
- El contrato de error ya está estandarizado en controladores críticos; validar continuamente endpoints nuevos para no romper consistencia.
- Definir catálogo de códigos de error por dominio (AUTH, ORDER, SHIFT, INV, CASH, REPORT, USER, TABLE).

### 6.3 Testing
- Expandir tests de integración (actualmente hay smoke inicial) a flujos críticos: órdenes, pagos/caja, turnos, inventario, users/products/tables, auth refresh edge-cases.
- Subir cobertura efectiva más allá del mínimo (meta sugerida: >= 75%).

### 6.4 WPF
- Separar coordinación de sesión y SignalR de `MainWindow` hacia un orquestador (`ISessionOrchestrator` / `INavCoordinator`).
- Mejorar testabilidad de navegación.

### 6.5 Resiliencia
- Idempotencia básica extendida a pagos, órdenes por lote y aperturas/cierres de turno.
- Política robusta aplicada con `IIdempotencyKeyStore` persistido en auditoría con TTL y compactación periódica.

### 6.6 Operación y gobernanza
- Runbook de despliegue/rollback ya creado; falta institucionalizar su uso operativo.
- Definition of Done por PR ya creado; falta institucionalizarlo en revisiones obligatorias.

---

## 7) Criterio de “Done” para cerrar el proyecto
Se considera completo cuando:
1. Controladores críticos estén desacoplados a servicios.
2. Error contract sea uniforme en toda API.
3. CI esté verde con cobertura y tests de integración para flujos core.
4. Operaciones críticas tengan idempotencia.
5. Cliente WPF reduzca acoplamiento principal en `MainWindow`.
6. Documentación permita onboarding y operación sin conocimiento tribal.

---

## 8) Instrucciones de continuidad para IA/desarrollador
1. Leer primero `docs/SENIOR_READY_CHECKLIST.md`.
2. Tomar un módulo pendiente (ej. Cashier) y aplicar patrón ya usado:
   - interfaz + implementación de servicio,
   - controlador delgado,
   - `ApiProblemException` con códigos claros,
   - tests unitarios del servicio,
   - actualización de checklist + bitácora.
3. Nunca dejar cambios a medias: todo endpoint tocado debe compilar, tener manejo de error consistente y pruebas mínimas.
4. Mantener naming y estilo ya adoptados en servicios recientes.

---

## 9) Estado de madurez actual
- **Arquitectura**: Mid+/Avanzando a Senior (por extracción progresiva a servicios).
- **Seguridad auth**: Avanzado.
- **Calidad/CI**: Mid+ con gates iniciales.
- **Producto completo senior-ready**: en progreso activo, con base sólida.
