using System;

namespace Ombi.Helpers
{
    public static class EmbyHelper
    {
        public static string GetEmbyMediaUrl(string mediaId, string serverId, string customerServerUrl = null)
        {
            // app.emby.media only supports #!/item format, not #!/details or #!/itemdetails
            string path = "item";
            
            // Check if targeting app.emby.media and use correct format
            if (!string.IsNullOrEmpty(customerServerUrl) && customerServerUrl.Contains("app.emby.media", StringComparison.OrdinalIgnoreCase))
            {
                path = "item";  // app.emby.media uses #!/item
            }
            
            if (customerServerUrl.HasValue())
            {
                if (!customerServerUrl.EndsWith("/"))
                {
                    return $"{customerServerUrl}/web/index.html#!/{path}?id={mediaId}&serverId={serverId}";
                }
                    return $"{customerServerUrl}web/index.html#!/{path}?id={mediaId}&serverId={serverId}";
            }
            else
            {
                return $"https://app.emby.media/web/index.html#!/{path}?id={mediaId}&serverId={serverId}";
            }
        }
    }
}
