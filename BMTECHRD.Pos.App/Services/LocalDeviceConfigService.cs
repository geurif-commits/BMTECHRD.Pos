using System;
using System.Globalization;
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
            return JsonSerializer.Deserialize<DeviceConfig>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (JsonException)
        {
            // Archivo corrupto: renombrarlo a .bak con timestamp
            BackupCorruptedFile();
            return null;
        }
        catch (IOException)
        {
            return null;
        }
        catch (UnauthorizedAccessException)
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
        catch (IOException ex)
        {
            throw new InvalidOperationException("Error guardando configuración del dispositivo.", ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new InvalidOperationException("Error guardando configuración del dispositivo.", ex);
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
        catch (IOException ex)
        {
            throw new InvalidOperationException("Error reseteando configuración del dispositivo.", ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new InvalidOperationException("Error reseteando configuración del dispositivo.", ex);
        }
    }

    private void BackupCorruptedFile()
    {
        try
        {
            if (File.Exists(_configPath))
            {
                var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
                var backupPath = $"{_configPath}.{timestamp}.bak";
                File.Move(_configPath, backupPath, overwrite: false);
            }
        }
        catch (IOException)
        {
            // Ignorar errores al hacer backup
        }
        catch (UnauthorizedAccessException)
        {
            // Ignorar errores al hacer backup
        }
    }

    private void CleanupOldBackups()
    {
        try
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-10);
            var backupFiles = Directory.GetFiles(_configFolder, "device.config.json.*.bak");

            foreach (var backupFile in backupFiles)
            {
                var fileInfo = new FileInfo(backupFile);
                if (fileInfo.LastWriteTimeUtc < cutoffDate)
                {
                    try
                    {
                        File.Delete(backupFile);
                    }
                    catch (IOException)
                    {
                        // Ignorar errores al limpiar
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // Ignorar errores al limpiar
                    }
                }
            }
        }
        catch (IOException)
        {
            // Ignorar errores
        }
        catch (UnauthorizedAccessException)
        {
            // Ignorar errores
        }
    }
}
