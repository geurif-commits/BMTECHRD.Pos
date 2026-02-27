using System;
using System.IO;

namespace BMTECHRD.Pos.Auth.Core.Services;

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
    private readonly string _deviceId;

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

    public string DeviceId => _deviceId;

    public bool IsAuthenticated => !string.IsNullOrWhiteSpace(_accessToken) && _userId != Guid.Empty;

    public bool IsAccessTokenExpiring => _expiresAt != DateTime.MinValue && DateTime.UtcNow.AddMinutes(5) >= _expiresAt;

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

    public void UpdateAccessToken(string newAccessToken, DateTime newExpiresAt)
    {
        lock (_lockObj)
        {
            _accessToken = newAccessToken;
            _expiresAt = newExpiresAt;
        }
    }

    public void UpdateTokens(string newAccessToken, string newRefreshToken, DateTime newExpiresAt)
    {
        lock (_lockObj)
        {
            _accessToken = newAccessToken;
            _refreshToken = newRefreshToken;
            _expiresAt = newExpiresAt;
        }
    }

    public void Clear()
    {
        lock (_lockObj)
        {
            if (string.IsNullOrWhiteSpace(_accessToken) && _userId == Guid.Empty)
            {
                return;
            }

            _accessToken = null;
            _refreshToken = null;
            _userId = Guid.Empty;
            _businessId = Guid.Empty;
            _username = null;
            _role = null;
            _expiresAt = DateTime.MinValue;
        }

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

    private static string GetDeviceIdFilePath()
    {
        var baseDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BMTECHRD", "POS");
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
        }

        var newId = Guid.NewGuid().ToString("N");
        try
        {
            File.WriteAllText(filePath, newId);
        }
        catch
        {
        }

        return newId;
    }
}