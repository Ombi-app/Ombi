#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: PlexHelper.cs
//    Created By: Jamie Rees
//   
//    Permission is hereby granted, free of charge, to any person obtaining
//    a copy of this software and associated documentation files (the
//    "Software"), to deal in the Software without restriction, including
//    without limitation the rights to use, copy, modify, merge, publish,
//    distribute, sublicense, and/or sell copies of the Software, and to
//    permit persons to whom the Software is furnished to do so, subject to
//    the following conditions:
//   
//    The above copyright notice and this permission notice shall be
//    included in all copies or substantial portions of the Software.
//   
//    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
//    EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//    MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
//    NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
//    LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
//    OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
//    WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//  ************************************************************************/
#endregion

using System;
using NLog;

namespace PlexRequests.Helpers
{
    public class PlexHelper
    {

        private static Logger Log = LogManager.GetCurrentClassLogger();
        public static string GetProviderIdFromPlexGuid(string guid)
        {
            if (string.IsNullOrEmpty(guid))
                return guid;

            var guidSplit = guid.Split(new[] { '/', '?' }, StringSplitOptions.RemoveEmptyEntries);
            if (guidSplit.Length > 1)
            {
                return guidSplit[1];
            }
            return string.Empty;
        }

        public static EpisodeModelHelper GetSeasonsAndEpisodesFromPlexGuid(string guid)
        {
            var ep = new EpisodeModelHelper();
            //guid="com.plexapp.agents.thetvdb://269586/2/8?lang=en"
            if (string.IsNullOrEmpty(guid))
                return null;
            try
            {
                var guidSplit = guid.Split(new[] { '/', '?' }, StringSplitOptions.RemoveEmptyEntries);
                if (guidSplit.Length > 2)
                {
                    ep.ProviderId = guidSplit[1];
                    ep.SeasonNumber = int.Parse(guidSplit[2]);
                    ep.EpisodeNumber = int.Parse(guidSplit[3]);
                }
                return ep;

            }
            catch (Exception e)
            {
                Log.Error(e);
                Log.Error(guid);
                return ep;
            }
        }

        public static int GetSeasonNumberFromTitle(string title)
        {
            if (string.IsNullOrEmpty(title))
            {
                return 0;
            }

            var split = title.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length < 2)
            {
                // Cannot get the season number, it's not in the usual format
                return 0;
            }

            int season;
            if (int.TryParse(split[1], out season))
            {
                return season;
            }

            return 0;
        }

        public static string GetPlexMediaUrl(string machineId, string mediaId)
        {
            var url =
                $"https://app.plex.tv/web/app#!/server/{machineId}/details/%2Flibrary%2Fmetadata%2F{mediaId}";
            return url;
        }

        public static string FormatGenres(string tags)
        {
            var split = tags.Split(new[] {'|'}, StringSplitOptions.RemoveEmptyEntries);
            return string.Join(", ", split);
        }
    }

    public class EpisodeModelHelper
    {
        public string ProviderId { get; set; }
        public int SeasonNumber { get; set; }
        public int EpisodeNumber { get; set; }
    }
}