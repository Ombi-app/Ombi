using System;
using System.Globalization;
using System.Collections.Generic;
using System.Text;

namespace Ombi.Helpers
{
    public class EmbyHelper
    {
        public static string GetEmbyMediaUrl(string mediaId)
        {
            var url =
                $"http://app.emby.media/itemdetails.html?id={mediaId}";
            return url;
        }
    }
}
