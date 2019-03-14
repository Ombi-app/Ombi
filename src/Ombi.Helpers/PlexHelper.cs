#region Copyright
// /************************************************************************
//    Copyright (c) 2017 Jamie Rees
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
using System.Globalization;
using System.Text.RegularExpressions;

namespace Ombi.Helpers
{
    public class PlexHelper
    {
        private  const string ImdbMatchExpression = "tt([0-9]{1,10})";
        private  const string TvDbIdMatchExpression = "//[0-9]+/([0-9]{1,3})/([0-9]{1,3})";

        public static ProviderId GetProviderIdFromPlexGuid(string guid)
        {
            //com.plexapp.agents.thetvdb://269586/2/8?lang=en
            //com.plexapp.agents.themoviedb://390043?lang=en
            //com.plexapp.agents.imdb://tt2543164?lang=en
            if (string.IsNullOrEmpty(guid))
            {
                return new ProviderId();
            }

            var guidSplit = guid.Split(new[] { '/', '?' }, StringSplitOptions.RemoveEmptyEntries);
            if (guidSplit.Length > 1)
            {
                if (guid.Contains("thetvdb", CompareOptions.IgnoreCase))
                {
                    return new ProviderId
                    {
                        TheTvDb = guidSplit[1]
                    };
                } else
                if (guid.Contains("themoviedb", CompareOptions.IgnoreCase))
                {
                    return new ProviderId
                    {
                        TheMovieDb = guidSplit[1]
                    };
                }
                else
                if (guid.Contains("imdb", CompareOptions.IgnoreCase))
                {
                    return new ProviderId
                    {
                        ImdbId = guidSplit[1]
                    };
                }
                else
                {
                    var imdbRegex = new Regex(ImdbMatchExpression, RegexOptions.Compiled);
                    var tvdbRegex = new Regex(TvDbIdMatchExpression, RegexOptions.Compiled);
                    var imdbMatch = imdbRegex.IsMatch(guid);
                    if (imdbMatch)
                    {
                        return new ProviderId
                        {
                            ImdbId = guidSplit[1]
                        };
                    }
                    else
                    {
                        // Check if it matches the TvDb pattern
                        var tvdbMatch = tvdbRegex.IsMatch(guid);
                        if (tvdbMatch)
                        {
                            return new ProviderId
                            {
                                TheTvDb = guidSplit[1]
                            };
                        }
                    }
                }
            }
            return new ProviderId();
        }

        public static EpisodeModelHelper GetSeasonsAndEpisodesFromPlexGuid(string guid)
        {
            var ep = new EpisodeModelHelper();
            //com.plexapp.agents.thetvdb://269586/2/8?lang=en
            //com.plexapp.agents.themoviedb://390043?lang=en
            //com.plexapp.agents.imdb://tt2543164?lang=en
            if (string.IsNullOrEmpty(guid))
                return null;
            try
            {
                var guidSplit = guid.Split(new[] { '/', '?' }, StringSplitOptions.RemoveEmptyEntries);
                if (guidSplit.Length > 2)
                {
                    if (guid.Contains("thetvdb", CompareOptions.IgnoreCase))
                    {
                        ep.ProviderId = new ProviderId {TheTvDb = guidSplit[1]};
                    }
                    if (guid.Contains("themoviedb", CompareOptions.IgnoreCase))
                    {
                        ep.ProviderId = new ProviderId { TheMovieDb = guidSplit[1] };

                    }
                    if (guid.Contains("imdb", CompareOptions.IgnoreCase))
                    {
                        ep.ProviderId = new ProviderId { ImdbId = guidSplit[1] };

                    }
                    ep.SeasonNumber = int.Parse(guidSplit[2]);
                    ep.EpisodeNumber = int.Parse(guidSplit[3]);
                }
                return ep;

            }
            catch (Exception)
            {
                return ep;
            }
        }

        public static string GetPlexMediaUrl(string machineId, int mediaId)
        {
            var url =
                $"https://app.plex.tv/web/app#!/server/{machineId}/details?key=library%2Fmetadata%2F{mediaId}";
            return url;
        }
    }

    public class EpisodeModelHelper
    {
        public ProviderId ProviderId { get; set; }
        public int SeasonNumber { get; set; }
        public int EpisodeNumber { get; set; }
    }

    public class ProviderId
    {
        public string TheTvDb { get; set; }
        public string TheMovieDb { get; set; }
        public string ImdbId { get; set; }

        public ProviderType Type
        {
            get
            {
                if (ImdbId.HasValue())
                {
                    return ProviderType.ImdbId;
                }
                if (TheMovieDb.HasValue())
                {
                    return ProviderType.TheMovieDbId;
                }
                if (TheTvDb.HasValue())
                {
                    return ProviderType.TvDbId;
                }
                return ProviderType.ImdbId;
            }
        }
    }

    public enum ProviderType
    {
        ImdbId,
        TheMovieDbId,
        TvDbId
    }
}