namespace Ombi.Helpers
{
    public static class JellyfinHelper
    {
        public static string GetJellyfinMediaUrl(string mediaId, string serverId, string customerServerUrl = null)
        {
            //web/index.html#!/details|item
            string path = "details";
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
                return $"http://localhost:8096/web/index.html#!/{path}?id={mediaId}&serverId={serverId}";
            }
        }
    }
}
