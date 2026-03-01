using System;
using System.IO;
using System.Text.Json;
using BMTECHRD.Pos.App.Core;

namespace BMTECHRD.Pos.App.Services;

public sealed class LocalDeviceConfig
{
    public DeviceMode Mode { get; set; }
    public string ApiBaseUrl { get; set; } = LocalDeviceConfigService.DefaultApiBaseUrl;
}

public sealed class LocalDeviceConfigService
{
    public const string DefaultApiBaseUrl = "http://localhost:5139/";

    private readonly string _configPath;
    private readonly string _configFolder;

    public LocalDeviceConfigService()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        _configFolder = Path.Combine(appData, "BMTECHRD.POS");
        Directory.CreateDirectory(_configFolder);
        _configPath = Path.Combine(_configFolder, "device.config.json");
    }

    public bool HasConfig()
    {
        return File.Exists(_configPath) && Load() != null;
    }

    public string GetConfigPath()
    {
        return _configPath;
    }

    public LocalDeviceConfig? Load()
    {
        try
        {
            if (!File.Exists(_configPath))
                return null;

            var json = File.ReadAllText(_configPath);
            var config = JsonSerializer.Deserialize<LocalDeviceConfig>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (config == null) return null;

            config.ApiBaseUrl = NormalizeBaseUrl(config.ApiBaseUrl) ?? DefaultApiBaseUrl;
            return config;
        }
        catch (JsonException)
        {
            BackupCorruptedFile();
            return null;
        }
        catch
        {
            return null;
        }
    }

    public void Save(DeviceMode mode, string? apiBaseUrl = null)
    {
        try
        {
            var config = new LocalDeviceConfig
            {
                Mode = mode,
                ApiBaseUrl = NormalizeBaseUrl(apiBaseUrl) ?? DefaultApiBaseUrl
            };

            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_configPath, json);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error guardando configuración del dispositivo: {ex.Message}", ex);
        }
    }

    public void Reset()
    {
        try
        {
            if (File.Exists(_configPath))
            {
                File.Delete(_configPath);
            }

            CleanupOldBackups();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error reseteando configuración: {ex.Message}", ex);
        }
    }

    private static string? NormalizeBaseUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return null;

        url = url.Trim();
        if (!url.EndsWith('/')) url += "/";

        return url;
    }

    private void BackupCorruptedFile()
    {
        try
        {
            if (File.Exists(_configPath))
            {
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var backupPath = $"{_configPath}.{timestamp}.bak";
                File.Move(_configPath, backupPath, overwrite: false);
            }
        }
        catch
        {
        }
    }

    private void CleanupOldBackups()
    {
        try
        {
            var cutoffDate = DateTime.Now.AddDays(-10);
            var backupFiles = Directory.GetFiles(_configFolder, "device.config.json.*.bak");

            foreach (var backupFile in backupFiles)
            {
                var fileInfo = new FileInfo(backupFile);
                if (fileInfo.LastWriteTime < cutoffDate)
                {
                    try
                    {
                        File.Delete(backupFile);
                    }
                    catch
                    {
                    }
                }
            }
        }
        catch
        {
        }
    }
}
