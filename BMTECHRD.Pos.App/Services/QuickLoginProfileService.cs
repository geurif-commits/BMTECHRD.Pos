using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace BMTECHRD.Pos.App.Services;

public sealed class QuickLoginProfileService
{
    private readonly string _filePath;

    public QuickLoginProfileService()
    {
        var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BMTECHRD", "POS");
        Directory.CreateDirectory(dir);
        _filePath = Path.Combine(dir, "quick-login.json");
    }

    public QuickLoginProfile? Load()
    {
        try
        {
            if (!File.Exists(_filePath)) return null;
            var payload = File.ReadAllText(_filePath);
            var json = Unprotect(payload);
            return JsonSerializer.Deserialize<QuickLoginProfile>(json);
        }
        catch
        {
            return null;
        }
    }

    public void Save(QuickLoginProfile profile)
    {
        try
        {
            var json = JsonSerializer.Serialize(profile);
            var payload = Protect(json);
            File.WriteAllText(_filePath, payload);
        }
        catch
        {
            // ignore persistence errors
        }
    }

    public void Clear()
    {
        try
        {
            if (File.Exists(_filePath)) File.Delete(_filePath);
        }
        catch
        {
            // ignore
        }
    }

    private static string Protect(string plain)
    {
        try
        {
            var bytes = Encoding.UTF8.GetBytes(plain);
            var protectedBytes = ProtectedData.Protect(bytes, null, DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(protectedBytes);
        }
        catch
        {
            return plain;
        }
    }

    private static string Unprotect(string payload)
    {
        try
        {
            var protectedBytes = Convert.FromBase64String(payload);
            var bytes = ProtectedData.Unprotect(protectedBytes, null, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(bytes);
        }
        catch
        {
            return payload;
        }
    }
}

public sealed class QuickLoginProfile
{
    public Guid BusinessId { get; set; }
    public string BusinessName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
}
