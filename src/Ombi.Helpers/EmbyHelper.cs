using System;
using System.Globalization;
using System.Collections.Generic;
using System.Text;

namespace Ombi.Helpers
{
    public class EmbyHelper
    {
        public static string GetEmbyMediaUrl(string mediaId, string customerServerUrl = null)
        {
            if (customerServerUrl.HasValue())
            {
                return $"{customerServerUrl}#!/itemdetails.html?id={mediaId}";
            }
            else
            {
                return $"https://app.emby.media/#!/itemdetails.html?id={mediaId}";
            }
        }
    }
}
