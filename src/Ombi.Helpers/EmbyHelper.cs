namespace Ombi.Helpers
{
    public class EmbyHelper
    {
        public static string GetEmbyMediaUrl(string mediaId, string customerServerUrl = null)
        {
            if (customerServerUrl.HasValue())
            {
                if(!customerServerUrl.EndsWith("/"))
                {
                    return $"{customerServerUrl}/#!/itemdetails.html?id={mediaId}";
                } 
                return $"{customerServerUrl}#!/itemdetails.html?id={mediaId}";
            }
            else
            {
                return $"https://app.emby.media/#!/itemdetails.html?id={mediaId}";
            }
        }
    }
}
