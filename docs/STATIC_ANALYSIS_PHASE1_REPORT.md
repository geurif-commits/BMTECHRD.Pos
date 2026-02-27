# Plan de Análisis Estático — Fase 1 (Top 20)

## Objetivo
Cerrar la primera iteración de análisis estático con:
1. inventario priorizado de 20 hallazgos,
2. clasificación por riesgo,
3. ejecución de lotes de corrección iniciales.

## Nota de entorno
En este entorno no está disponible `dotnet`, por lo que el inventario se construyó con barridos estáticos (`rg`) orientados a los diagnósticos baseline definidos en `.editorconfig` y a code smells de alto riesgo de runtime/maintainability.

## Top 20 hallazgos priorizados

| # | Riesgo | Tipo | Ubicación | Estado Fase 1 |
|---|---|---|---|---|
| 1 | Alto | Posible `CS8602` (null-forgiving) | `UsersService` (`actor!`) | ✅ Corregido |
| 2 | Alto | Posible `CS8602` (`First()` sin validar) | `AuthFlowIntegrationTests` | ✅ Corregido |
| 3 | Alto | Excepción silenciada | `TablesMapViewModel` (`catch {}`) | ✅ Corregido |
| 4 | Alto | Excepción silenciada | `CashierViewModel` (`catch {}`) | ✅ Corregido |
| 5 | Medio | Doble suscripción evento (riesgo refresh duplicado) | `CashierViewModel` | ✅ Corregido |
| 6 | Medio | Variable no usada | `CashierViewModel.CloseShiftAsync` | ✅ Corregido |
| 7 | Medio | `ToString()` en enums (revisión CA1305/estilo) | `UsersService` | 🔄 Pendiente lote B |
| 8 | Medio | `ToString()` en enums | `AuthService` | 🔄 Pendiente lote B |
| 9 | Medio | `ToString()` en enums | `CashierService` | 🔄 Pendiente lote B |
| 10 | Medio | `ToString()` en enums | `ProductionQueueService` | 🔄 Pendiente lote B |
| 11 | Medio | `InvalidOperationException` con mensajes de config | `Program.cs` | 🔄 Pendiente lote C |
| 12 | Medio | Captura genérica + rethrow wrapped | `LocalDeviceConfigService` | 🔄 Pendiente lote C |
| 13 | Bajo | Capturas genéricas en UI (diagnóstico) | `LoginViewModel` | 🔄 Pendiente lote C |
| 14 | Bajo | Capturas genéricas en UI (diagnóstico) | `ReportsAdminViewModel` | 🔄 Pendiente lote C |
| 15 | Bajo | Capturas genéricas en UI (diagnóstico) | `ShiftsAdminViewModel` | 🔄 Pendiente lote C |
| 16 | Bajo | `Task.Run` en UI orchestration (revisión) | `MainWindow.xaml.cs` | 🔄 Pendiente lote C |
| 17 | Bajo | String interpolation en errores de IO/config | `LocalDeviceConfigService` | 🔄 Pendiente lote C |
| 18 | Bajo | Validación hardcoded strings estado | `CashierViewModel` | 🔄 Pendiente lote B |
| 19 | Bajo | Revisión de nulabilidad DTOs integración | `AuthFlowIntegrationTests` y similares | 🔄 Parcial (1/3) |
| 20 | Bajo | Revisión de consistencia `CancellationToken` UI/API boundary | Varios | 🔄 Pendiente lote B |

## Lotes de corrección

### Lote A (riesgo alto) — ✅ Ejecutado en Fase 1
- Null-safety en `UsersService`.
- Guard para colección vacía en integración auth.
- Eliminación de catches silenciosos en ViewModels críticos de operación.

### Lote B (riesgo medio) — Próxima iteración
- Homogeneizar serialización/string conversion y revisar avisos CA1305 donde aplique.
- Revisión de consistencia de strings de estado y boundary token/cancelación.

### Lote C (riesgo bajo / mantenibilidad)
- Endurecer manejo de excepciones UI/config con errores de dominio más explícitos.
- Reducir puntos de captura genérica en WPF y revisar `Task.Run` en UI thread orchestration.

## Resultado de Fase 1
- Inventario top 20 definido.
- Priorización por riesgo definida.
- Lote A implementado y documentado.
- Proyecto listo para ejecutar Fase 2 al habilitar corrida completa de analyzers en entorno con `dotnet`.
