using System;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using Ombi.Helpers;

namespace Ombi.Settings.Settings.Models
{
    public class CustomizationSettings : Settings
    {
        public string ApplicationName { get; set; }
        public string ApplicationUrl { get; set; }
        public string CustomCssLink { get; set; }
        public string Logo { get; set; }

        public string PresetThemeName { get; set; }
        public string PresetThemeContent { get; set; }

        [NotMapped]
        public string PresetThemeVersion
        {
            get
            {
                if (HasPresetTheme)
                {
                    var parts = PresetThemeName.Split('-');
                    return parts[3].Replace(".css", string.Empty);
                }
                return string.Empty;
            }
        }

        [NotMapped]
        public string PresetThemeDisplayName
        {
            get
            {
                if (HasPresetTheme)
                {
                    var parts = PresetThemeName.Split('-');
                    return parts[1];
                }
                return string.Empty;
            }
        }

        [NotMapped]
        public bool HasPresetTheme => PresetThemeName.HasValue() || PresetThemeContent.HasValue();

        public void AddToUrl(string part)
        {
            if (string.IsNullOrEmpty(ApplicationUrl))
            {
                ApplicationUrl = part;
            }

            if (ApplicationUrl.EndsWith("/"))
            {
                ApplicationUrl.Remove(ApplicationUrl.Length - 1);
            }
            if (!part.StartsWith("/"))
            {
                part = "/" + part;
            }
            ApplicationUrl = ApplicationUrl + part;
        }
    }
}