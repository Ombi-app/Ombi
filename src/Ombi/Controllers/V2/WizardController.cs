using System;
using System.Threading;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ombi.Attributes;
using Ombi.Core.Settings;
using Ombi.Helpers;
using Ombi.Models.V2;
using Ombi.Settings.Settings.Models;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using Npgsql;
using Ombi.Core.Services;

namespace Ombi.Controllers.V2
{
    [ServiceFilter(typeof(WizardActionFilter))]
    [AllowAnonymous]
    public class WizardController : V2Controller
    {
        private readonly ISettingsService<OmbiSettings> _ombiSettings;
        private readonly IDatabaseConfigurationService _databaseConfigurationService;
        private readonly ILogger _logger;
        private ISettingsService<CustomizationSettings> _customizationSettings { get; }

        public WizardController(
            ISettingsService<CustomizationSettings> customizationSettings, 
            ISettingsService<OmbiSettings> ombiSettings,
            IDatabaseConfigurationService databaseConfigurationService,
            ILogger<WizardController> logger)
        {
            _ombiSettings = ombiSettings;
            _databaseConfigurationService = databaseConfigurationService;
            _logger = logger;
            _customizationSettings = customizationSettings;
        }

        [HttpPost("config")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> OmbiConfig([FromBody] OmbiConfigModel config)
        {
            if (config == null)
            {
                return BadRequest();
            }

            var ombiSettings = await _ombiSettings.GetSettingsAsync();
            if (ombiSettings.Wizard)
            {
                _logger.LogError("Wizard has already been completed");
                return BadRequest();
            }

            var settings = await _customizationSettings.GetSettingsAsync();

            if (config.ApplicationName.HasValue())
            {
                settings.ApplicationName = config.ApplicationName;
            }

            if(config.ApplicationUrl.HasValue())
            {
                settings.ApplicationUrl = config.ApplicationUrl;
            }

            if(config.Logo.HasValue())
            {
                settings.Logo = config.Logo;
            }

            await _customizationSettings.SaveSettingsAsync(settings);

            return new OkObjectResult(settings);
        }

        [HttpPost("database")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> DatabaseConfig([FromBody] WizardDatabaseConfiguration config, CancellationToken token)
        {
            if (config == null)
            {
                return BadRequest();
            }            
            
            var ombiSettings = await _ombiSettings.GetSettingsAsync();
            if (ombiSettings.Wizard)
            {
                _logger.LogError("Wizard has already been completed");
                return BadRequest();
            }

            var sanitizedType = config.Type.Replace(Environment.NewLine, "").Replace("\n", "").Replace("\r", "");
            _logger.LogInformation("Setting up database type: {0}", sanitizedType);

            var connectionString = string.Empty;
            if (config.Type == IDatabaseConfigurationService.MySqlDatabase)
            {
                _logger.LogInformation("Building MySQL connectionstring");
                var builder = new MySqlConnectionStringBuilder
                {
                    Database = config.Name,
                    Port = Convert.ToUInt32(config.Port),
                    Server = config.Host,
                    UserID = config.User,
                    Password = config.Password
                };

                connectionString = builder.ToString();
            }

            if (config.Type == IDatabaseConfigurationService.PostgresDatabase)
            {
                _logger.LogInformation("Building Postgres connectionstring");
                var builder = new NpgsqlConnectionStringBuilder
                {
                    Host = config.Host,
                    Port = config.Port,
                    Database = config.Name,
                    Username = config.User,
                    Password = config.Password
                };
                connectionString = builder.ToString();
            }

            var result = await _databaseConfigurationService.ConfigureDatabase(config.Type, connectionString, token);
            
            if (!result)
            {
                return BadRequest(new DatabaseConfigurationResult(false, "Could not configure the database, please check the logs"));
            }
            
            return Ok(new DatabaseConfigurationResult(true, "Database configured successfully"));
        }

        public record DatabaseConfigurationResult(bool Success, string Message);

    }
}
