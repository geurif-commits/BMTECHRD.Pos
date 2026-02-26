using System;
using System.IO;

namespace BMTECHRD.Pos.App.Services;

/// <summary>
/// Mantiene la sesión de autenticación del usuario actual (tokens, datos).
/// ETAPA 9 HARDENED:
/// - DeviceId persistente por equipo (para Device Binding en refresh token).
/// - Almacenamiento en memoria (tokens) + DeviceId persistido en disco.
/// </summary>
public sealed class AuthSessionService
{
    private string? _accessToken;
    private string? _refreshToken;
    private Guid _userId;
    private Guid _businessId;
    private string? _username;
    private string? _role;
    private DateTime _expiresAt;

    private readonly object _lockObj = new();

    // DeviceId persistente (por equipo)
    private readonly string _deviceIdFilePath;
    private string _deviceId;

    public AuthSessionService()
    {
        _deviceIdFilePath = GetDeviceIdFilePath();
        _deviceId = LoadOrCreateDeviceId(_deviceIdFilePath);
    }

    public string? AccessToken => _accessToken;
    public string? RefreshToken => _refreshToken;
    public Guid UserId => _userId;
    public Guid BusinessId => _businessId;
    public string? Username => _username;
    public string? Role => _role;
    public DateTime ExpiresAt => _expiresAt;

    /// <summary>
    /// Identificador único del dispositivo (persistente).
    /// Se envía en login y refresh para habilitar Device Binding.
    /// </summary>
    public string DeviceId => _deviceId;

    /// <summary>
    /// Indica si el usuario está autenticado actualmente.
    /// </summary>
    public bool IsAuthenticated => !string.IsNullOrWhiteSpace(_accessToken) && _userId != Guid.Empty;

    /// <summary>
    /// Indica si el access token está próximo a expirar.
    /// Nota: para seguridad real, ExpiresAt debe venir del access token o del backend.
    /// Aquí se mantiene tu comportamiento actual.
    /// </summary>
    public bool IsAccessTokenExpiring => DateTime.UtcNow.AddMinutes(5) >= _expiresAt;

    /// <summary>
    /// Establece la sesión con los datos de login.
    /// </summary>
    public void SetSession(
        string accessToken,
        string refreshToken,
        Guid userId,
        Guid businessId,
        string username,
        string role,
        DateTime expiresAt)
    {
        lock (_lockObj)
        {
            _accessToken = accessToken;
            _refreshToken = refreshToken;
            _userId = userId;
            _businessId = businessId;
            _username = username;
            _role = role;
            _expiresAt = expiresAt;
        }
    }

    /// <summary>
    /// Actualiza el access token.
    /// </summary>
    public void UpdateAccessToken(string newAccessToken, DateTime newExpiresAt)
    {
        lock (_lockObj)
        {
            _accessToken = newAccessToken;
            _expiresAt = newExpiresAt;
        }
    }

    /// <summary>
    /// Actualiza ambos tokens (refresh token rotation).
    /// </summary>
    public void UpdateTokens(string newAccessToken, string newRefreshToken, DateTime newExpiresAt)
    {
        lock (_lockObj)
        {
            _accessToken = newAccessToken;
            _refreshToken = newRefreshToken;
            _expiresAt = newExpiresAt;
        }
    }

    /// <summary>
    /// Limpia la sesión (logout). Mantiene DeviceId (por diseño).
    /// </summary>
    public void Clear()
    {
        lock (_lockObj)
        {
            _accessToken = null;
            _refreshToken = null;
            _userId = Guid.Empty;
            _businessId = Guid.Empty;
            _username = null;
            _role = null;
            _expiresAt = DateTime.MinValue;
        }
        // Notificar que la sesión expiró/limpió
        try
        {
            SessionExpired?.Invoke(this, EventArgs.Empty);
        }
        catch
        {
            // ignore
        }
    }

    public event EventHandler? SessionExpired;

    // ------------------------------
    // DeviceId persistence
    // ------------------------------

    private static string GetDeviceIdFilePath()
    {
        var baseDir = BMTECHRD.Pos.App.Core.AppPaths.Root;
        Directory.CreateDirectory(baseDir);
        return Path.Combine(baseDir, "device.id");
    }

    private static string LoadOrCreateDeviceId(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                var existing = File.ReadAllText(filePath).Trim();
                if (!string.IsNullOrWhiteSpace(existing) && existing.Length >= 16)
                    return existing;
            }
        }
        catch
        {
            // si falla lectura, generamos nuevo
        }

        var newId = Guid.NewGuid().ToString("N"); // 32 chars
        try
        {
            File.WriteAllText(filePath, newId);
        }
        catch
        {
            // si falla escritura, igual devolvemos el id en memoria
        }

        return newId;
    }
}