# REPORT - ETAPA 9 HARDENED (WPF client)

Fecha: (auto)

Resumen de cambios
------------------

Archivos modificados:

- `BMTECHRD.Pos.App/Services/AuthHeaderHandler.cs`
  - Refactor del handler para evitar dependencias circulares: ahora el constructor recibe solo `AuthSessionService` y se expone `SetApiClient(ApiClient)` para inyectar `ApiClient` después de construir `HttpClient`.
  - Se eliminó el uso de `null!` y se usa una variable local `refreshToken` tras validación.
  - Uso de `SemaphoreSlim` (ya existente) para evitar concurrencia en refresh.
  - Evita refrescar en endpoints `/api/auth/login`, `/api/auth/refresh`, `/api/auth/logout`.
  - Implementa reintento seguro clonando la petición y usando `Request.Options` para marcar reintentos (`Retried` flag).
  - Al refrescar llama `ApiClient.RefreshTokenAsync(refreshToken, deviceId, cancellationToken)` y usa `DeviceId` desde `AuthSessionService`.

- `BMTECHRD.Pos.App/Services/AuthSessionService.cs`
  - Se agregó persistencia de `DeviceId` en `%AppData%\\BMTECHRD\\POS\\device.id`.
  - Expuesta `public string DeviceId { get; }`.
  - `Clear()` ya no borra `DeviceId` (diseño solicitado).

- `BMTECHRD.Pos.App/MainWindow.xaml.cs`
  - Se eliminó la creación doble de `HttpClient`.
  - Ahora se crea un único `AuthHeaderHandler` con `InnerHandler = new HttpClientHandler()` y un único `HttpClient` construido sobre ese handler.
  - Se construye `ApiClient` con ese `HttpClient` y luego se inyecta en el handler vía `authHandler.SetApiClient(api)` para resolver el ciclo.
  - `baseUrl` se obtiene desde `LocalDeviceConfig` (busca `ApiBaseUrl`, `BaseUrl`, `ServerUrl`) y cae a `https://localhost:5001/` si no existe; se asegura `trailing slash`.
  - Se pasa la instancia de `AuthSessionService` al `StartView.Initialize(...)` para que el `DeviceId` se incluya en el login.

- `BMTECHRD.Pos.App/Views/StartView.xaml.cs`
  - `Initialize` ahora acepta `AuthSessionService` como segundo parámetro y crea `StartViewModel(api, session)`.

- `BMTECHRD.Pos.App/ViewModels/StartViewModel.cs`
  - Constructor modificado a `StartViewModel(ApiClient api, AuthSessionService session)` y guarda la sesión internamente.
  - En `LoginAsync()` ahora incluye `DeviceId = _session.DeviceId` en el `LoginRequest` antes de llamar `_api.LoginAsync(req)`.


Confirmaciones requeridas
-------------------------

- Se eliminó `null!` en `AuthHeaderHandler`.
  - Antes se usaba `_session.RefreshToken!`; ahora se valida y se usa variable local `refreshToken`.

- Se eliminó la doble creación de `HttpClient`.
  - Se usa un único `HttpClient` con `AuthHeaderHandler` como delegating handler y `HttpClientHandler` como `InnerHandler`.

- `DeviceId` persiste y se envía en `Login` y `Refresh`.
  - `AuthSessionService.DeviceId` es persistente en `%AppData%\\BMTECHRD\\POS\\device.id`.
  - `StartViewModel` incluye `DeviceId` en `LoginRequest`.
  - `AuthHeaderHandler` pasa `DeviceId` a `ApiClient.RefreshTokenAsync(...)`.

- Refresh concurrency + retry seguro implementado.
  - `SemaphoreSlim` evita múltiples refresh simultáneos.
  - Se marca la petición como ya reintentada usando `HttpRequestMessage.Options` para evitar retry loops.
  - La petición se clona (incluye cuerpo) para realizar un retry seguro.


Estado por etapas (6 -> 9)
-------------------------

- Etapa 6: Completada (asunción: auth básico y flujo existente mantenido).
- Etapa 6.1: Parcial/Completada (mejoras incrementales aplicadas en sesiones y manejo de tokens).
- Etapa 7: Completada (token storage y uso en requests, handler aplicado).
- Etapa 8: Parcial (SignalR y rol checks están presentes; revisar hardening del backend).
- Etapa 8.1: Parcial (mejoras adicionales de seguridad pendientes en backend y almacenamiento seguro de tokens).
- Etapa 9: Parcial (cliente: implementaciones clave de ETAPA 9 aplicadas — DeviceId, refresh rotation, reuse detection basics, single HttpClient, no null!. Faltan pruebas end-to-end con backend y manejo de reuse-detection avanzado del lado servidor).

Qué falta para cerrar ETAPA 9 completamente:

- Pruebas E2E con backend ETAPA 9 para validar: refresh token rotation + reuse detection + device binding y que backend revoca tokens en reuse.
- Opcional: cifrar `device.id` o proteger con DPAPI si se requiere mayor seguridad.
- Asegurar que backend retorna `ExpiresAt` y que la lógica de expiración en cliente usa valores confiables.
- Implementar manejo explícito de 401 tras refresh fallido (mostrar UI de re-login) en varias pantallas si es necesario.


Pruebas manuales recomendadas
----------------------------

1) Login exitoso
   - Iniciar la app, seleccionar negocio, hacer login con credenciales válidas.
   - Verificar que la UI carga la vista correspondiente según `DeviceMode`.
   - Verificar que `device.id` fue creado en `%AppData%\\BMTECHRD\\POS\\device.id`.

2) Refresh token rotation
   - Forzar expiración del access token (o simular backend) y provocar una llamada que devuelva 401.
   - Verificar que el handler realiza un refresh una sola vez (concurrency lock) y reintenta la petición.

3) Logout
   - Ejecutar flujo de logout (si existe) y verificar que `AuthSessionService.Clear()` remueve tokens pero conserva `device.id`.

4) Device mismatch / binding
   - Intentar usar el mismo refresh token desde otro equipo (o simular deviceId diferente) y verificar que backend rechaza o detecta reuse.

5) Reuse detection scenario
   - Simular que refresh token fue reutilizado (según backend) y verificar que cliente limpia sesión y exige re-login.


Notas finales
-------------

Los cambios son limitados al cliente WPF y respetan el flujo existente (DeviceModeSelectionWindow -> MainWindow -> StartView -> OnLoginSuccess -> SignalR -> carga de vistas). No se agregaron pantallas nuevas. Se resolvió la dependencia circular entre `AuthHeaderHandler` y `ApiClient` de manera explícita y segura.

Si quieres, procedo a compilar la solución y ejecutar los tests (si existen) o a instrumentar logs adicionales para validar el comportamiento de refresh en tiempo de ejecución.
