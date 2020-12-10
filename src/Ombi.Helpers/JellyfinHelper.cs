namespace Ombi.Helpers
{
    public static class JellyfinHelper
    {
        public static string GetJellyfinMediaUrl(string mediaId, string serverId, string customerServerUrl = null, bool isJellyfin = false)
        {
            //web/index.html#!/details|item
            string path = "item";
            if (isJellyfin)
            {
                path = "details";
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
                return $"https://app.jellyfin.media/web/index.html#!/{path}?id={mediaId}&serverId={serverId}";
            }
        }
    }
}
