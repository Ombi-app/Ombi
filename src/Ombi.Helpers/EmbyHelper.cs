using System;

namespace Ombi.Helpers
{
    public static class EmbyHelper
    {
        public static string GetEmbyMediaUrl(string mediaId, string serverId, string customerServerUrl = null)
        {
            // app.emby.media only supports #!/item format, not #!/details or #!/itemdetails
            string path = "item";

            // Check if targeting app.emby.media specifically
            bool isAppEmbyMedia = !string.IsNullOrEmpty(customerServerUrl) &&
                                  customerServerUrl.Contains("app.emby.media", StringComparison.OrdinalIgnoreCase);

            if (customerServerUrl.HasValue())
            {
                // app.emby.media doesn't use /web/index.html in URLs
                if (isAppEmbyMedia)
                {
                    if (!customerServerUrl.EndsWith("/"))
                    {
                        return $"{customerServerUrl}/#!/{path}?id={mediaId}&serverId={serverId}";
                    }
                    return $"{customerServerUrl}#!/{path}?id={mediaId}&serverId={serverId}";
                }

                // Custom Emby servers use /web/index.html
                if (!customerServerUrl.EndsWith("/"))
                {
                    return $"{customerServerUrl}/web/index.html#!/{path}?id={mediaId}&serverId={serverId}";
                }
                return $"{customerServerUrl}web/index.html#!/{path}?id={mediaId}&serverId={serverId}";
            }
            else
            {
                // Default (no custom server) uses app.emby.media WITHOUT /web/index.html (causes 404)
                return $"https://app.emby.media/#!/{path}?id={mediaId}&serverId={serverId}";
            }
        }
    }
}
