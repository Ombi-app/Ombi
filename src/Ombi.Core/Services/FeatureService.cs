using Ombi.Core.Settings;
using Ombi.Settings.Settings.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Ombi.Core.Services
{
    public interface IFeatureService
    {
        Task<bool> FeatureEnabled(string featureName);
    }

    public class FeatureService : IFeatureService
    {
        private readonly ISettingsService<FeatureSettings> _featureSettings;

        public FeatureService(ISettingsService<FeatureSettings> featureSettings)
        {
            _featureSettings = featureSettings;
        }

        public async Task<bool> FeatureEnabled(string featureName)
        {
            var settings = await _featureSettings.GetSettingsAsync();
            return settings.Features?.Where(x => x.Name.Equals(featureName, System.StringComparison.InvariantCultureIgnoreCase)).Select(x => x.Enabled)?.FirstOrDefault() ?? false;
        }
    }
}
