namespace Ombi.Helpers
{
    public class EmbyHelper
    {
        public static string GetEmbyMediaUrl(string mediaId, string customerServerUrl = null, bool isJellyfin = false)
        {
            string path = "item/item";
            if (isJellyfin)
            {
                path = "itemdetails";
            }
            if (customerServerUrl.HasValue())
            {
                if (!customerServerUrl.EndsWith("/"))
                {
                    return $"{customerServerUrl}/#!/{path}.html?id={mediaId}";
                }
                    return $"{customerServerUrl}#!/{path}.html?id={mediaId}";
            }
            else
            {
                return $"https://app.emby.media/#!/{path}.html?id={mediaId}";
            }
        }
    }
}
