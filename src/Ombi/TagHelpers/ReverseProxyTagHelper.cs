using Microsoft.AspNetCore.Razor.TagHelpers;
using Ombi.Core.Settings;
using Ombi.Settings.Settings.Models;

namespace Ombi.TagHelpers
{
    public class ReverseProxyTagHelper : TagHelper
    {
        public ReverseProxyTagHelper(ISettingsService<OmbiSettings> c)
        {
            _ctx = c;
        }

        private readonly ISettingsService<OmbiSettings> _ctx;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "base";
            var s = _ctx.GetSettings();
            var baseUrl = string.IsNullOrEmpty(s.BaseUrl) ? "/" : s.BaseUrl;
            output.Attributes.SetAttribute("href", baseUrl);
        }
    }
}
