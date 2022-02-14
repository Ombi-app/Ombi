using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ombi.Attributes;
using Ombi.Core.Settings;
using Ombi.Settings.Settings.Models;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Ombi.Controllers.V2
{
    public class FeaturesController : V2Controller
    {
        private readonly ISettingsService<FeatureSettings> _features;

        public FeaturesController(ISettingsService<FeatureSettings> features) => _features = features;

        [HttpGet]
        [AllowAnonymous]
        public async Task<List<FeatureEnablement>> GetFeatures()
        {
            var features = await _features.GetSettingsAsync();
            return PopulateFeatures(features?.Features ?? new List<FeatureEnablement>()); 
        }

        [HttpPost("enable")]
        [Admin]
        public async Task<List<FeatureEnablement>> Enable([FromBody] FeatureEnablement feature)
        {
            var featureSettings = await _features.GetSettingsAsync();
            var features = PopulateFeatures(featureSettings?.Features ?? new List<FeatureEnablement>());
            var featureToUpdate = features.First(x => x.Name.Equals(feature.Name));
            featureToUpdate.Enabled = true;

            featureSettings.Features = features;

            await _features.SaveSettingsAsync(featureSettings);

            return PopulateFeatures(featureSettings?.Features ?? new List<FeatureEnablement>());
        }

        [HttpPost("disable")]
        [Admin]
        public async Task<List<FeatureEnablement>> Disable([FromBody] FeatureEnablement feature)
        {
            var featureSettings = await _features.GetSettingsAsync();
            var features = PopulateFeatures(featureSettings?.Features ?? new List<FeatureEnablement>());
            var featureToUpdate = features.First(x => x.Name.Equals(feature.Name));
            featureToUpdate.Enabled = false;

            featureSettings.Features = features;

            await _features.SaveSettingsAsync(featureSettings);

            return PopulateFeatures(featureSettings?.Features ?? new List<FeatureEnablement>());
        }


        private List<FeatureEnablement> PopulateFeatures(List<FeatureEnablement> existingFeatures)
        {
            var supported = GetSupportedFeatures().ToList();
            if (supported.Count == existingFeatures.Count)
            {
                return existingFeatures;
            }
            var diff = supported.Except(existingFeatures.Select(x => x.Name));

            foreach (var feature in diff)
            {
                existingFeatures.Add(new FeatureEnablement
                {
                    Name = feature
                });
            }
            return existingFeatures;
        }

        private IEnumerable<string> GetSupportedFeatures()
        {
            FieldInfo[] fieldInfos = typeof(FeatureNames).GetFields(BindingFlags.Public |
                 BindingFlags.Static | BindingFlags.FlattenHierarchy);

            return fieldInfos.Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(string)).Select(x => (string)x.GetValue(x));
        }
    }
}
