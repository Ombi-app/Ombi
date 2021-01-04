namespace Ombi.Helpers
{
    public static class EmbyHelper
    {
        public static string GetEmbyMediaUrl(string mediaId, string serverId, string customerServerUrl = null)
        {
            //web/index.html#!/details|item
            string path = "item";
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
