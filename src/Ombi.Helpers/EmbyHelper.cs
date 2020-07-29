namespace Ombi.Helpers
{
    public class EmbyHelper
    {
        public static string GetEmbyMediaUrl(string mediaId, string customerServerUrl = null, bool isJellyfin = false)
        {
            string path = "item";
            if (isJellyfin)
            {
                path = "details";
            }
            if (customerServerUrl.HasValue())
            {
                if (isJellyfin)
                {
                    return $"{customerServerUrl}#!/{path}?id={mediaId}";
                }
                else {
                    if (!customerServerUrl.EndsWith("/"))
                    {
                        return $"{customerServerUrl}/#!/{path}?id={mediaId}";
                    }
                    return $"{customerServerUrl}#!/{path}?id={mediaId}";
                }
            }
            else
            {
                return $"https://app.emby.media/#!/{path}?id={mediaId}";
            }
        }
    }
}
