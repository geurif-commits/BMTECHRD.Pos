using System;
using System.IO;
using System.Text.Json;
using BMTECHRD.Pos.App.Core;

namespace BMTECHRD.Pos.App.Services;

public class DeviceConfig
{
    public DeviceMode Mode { get; set; }
}

public sealed class LocalDeviceConfigService
{
    private readonly string _configPath;
    private readonly string _configFolder;

    public LocalDeviceConfigService()
    {
        _configFolder = Path.Combine(BMTECHRD.Pos.App.Core.AppPaths.Root);
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

    public DeviceConfig? Load()
    {
        try
        {
            if (!File.Exists(_configPath))
                return null;

            var json = File.ReadAllText(_configPath);
            var config = JsonSerializer.Deserialize<DeviceConfig>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return config;
        }
        catch (JsonException)
        {
            // Archivo corrupto: renombrarlo a .bak con timestamp
            BackupCorruptedFile();
            return null;
        }
        catch
        {
            return null;
        }
    }

    public void Save(DeviceMode mode)
    {
        try
        {
            var config = new DeviceConfig { Mode = mode };
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

            // Opcionalmente, limpiar backups antiguos
            CleanupOldBackups();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error reseteando configuración: {ex.Message}", ex);
        }
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
            // Ignorar errores al hacer backup
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
                        // Ignorar errores al limpiar
                    }
                }
            }
        }
        catch
        {
            // Ignorar errores
        }
    }
}
