using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Ombi.Core.Helpers;
using Ombi.Core.Models;
using Ombi.Helpers;

namespace Ombi.Core.Services;

public class DatabaseConfigurationService : IDatabaseConfigurationService
{

    private readonly ILogger _logger;
    private readonly IFileSystem _fileSystem;

    public DatabaseConfigurationService(
        ILogger<DatabaseConfigurationService> logger,
        IFileSystem fileSystem)
    {
        _logger = logger;
        _fileSystem = fileSystem;
    }

    public async Task<bool> ConfigureDatabase(string databaseType, string connectionString, CancellationToken token)
    {
        var i = StartupSingleton.Instance;
        if (string.IsNullOrEmpty(i.StoragePath))
        {
            i.StoragePath = string.Empty;
        }

        var databaseFileLocation = Path.Combine(i.StoragePath, "database.json");
        if (_fileSystem.FileExists(databaseFileLocation))
        {
            var error = $"The database file at '{databaseFileLocation}' already exists";
            _logger.LogError(error);
            return false;
        }

        var configuration = new DatabaseConfiguration
        {
            ExternalDatabase = new PerDatabaseConfiguration(databaseType, connectionString),
            OmbiDatabase = new PerDatabaseConfiguration(databaseType, connectionString),
            SettingsDatabase = new PerDatabaseConfiguration(databaseType, connectionString)
        };

        var json = JsonConvert.SerializeObject(configuration, Formatting.Indented);

        _logger.LogInformation("Writing database configuration to file");

        try
        {
            await File.WriteAllTextAsync(databaseFileLocation, json, token);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to write database configuration to file");
            return false;
        }

        _logger.LogInformation("Database configuration written to file");

      
        return true;
    }
}