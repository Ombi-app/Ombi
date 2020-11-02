namespace Ombi.Helpers
{
    public class EmbyHelper
    {
        public static string GetEmbyMediaUrl(string mediaId, string serverId, string customerServerUrl = null, bool isJellyfin = false)
        {
            string path = "item";
            if (isJellyfin)
            {
                path = "itemdetails.html";
            }
            if (customerServerUrl.HasValue())
            {
                if (!customerServerUrl.EndsWith("/"))
                {
                    return $"{customerServerUrl}/#!/{path}?id={mediaId}&serverId={serverId}";
                }
                    return $"{customerServerUrl}#!/{path}?id={mediaId}&serverId={serverId}";
            }
            else
            {
                return $"https://app.emby.media/#!/{path}?id={mediaId}&serverId={serverId}";
            }
        }
    }
}
