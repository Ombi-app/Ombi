using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
using System.Text;

namespace Ombi.Helpers
{
    public static class StringHelper
    {
        public static bool Contains(this string paragraph, string word, CompareOptions opts)
        {
            if (string.IsNullOrEmpty(paragraph))
            {
                return false;
            }
            return CultureInfo.CurrentUICulture.CompareInfo.IndexOf(paragraph, word, CompareOptions.IgnoreCase) >= 0;
        }

        public static string CalcuateMd5Hash(this string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }
            using (var md5 = MD5.Create())
            {
                var inputBytes = Encoding.UTF8.GetBytes(input);
                var hash = md5.ComputeHash(inputBytes);

                var sb = new StringBuilder();

                foreach (var t in hash)
                {
                    sb.Append(t.ToString("x2"));
                }

                return sb.ToString();
            }
        }

        public static string GetSha1Hash(this string input)
        {
            var sha1 = SHA1.Create();
            return string.Join("", (sha1.ComputeHash(Encoding.UTF8.GetBytes(input))).Select(x => x.ToString("x2")).ToArray());
        }


        public static bool IsNullOrEmpty(this string s) => string.IsNullOrEmpty(s);
        public static bool HasValue(this string s) => !IsNullOrEmpty(s);

        public static SecureString ToSecureString(this string password)
        {
            if (password == null)
                throw new ArgumentNullException(nameof(password));

            var securePassword = new SecureString();

            foreach (var c in password)
            {
                securePassword.AppendChar(c);
            }

            securePassword.MakeReadOnly();
            return securePassword;
        }

        public static int IntParseLinq(string stringIn)
        {
            if (int.TryParse(stringIn, out var result))
            {
                return result;
            }

            return -1;
        }

        public static string BuildEpisodeList(IEnumerable<(int EpisodeNumber, DateTime? Aired)> orderedEpisodes)
        {
            var epSb = new StringBuilder();
            var previousEpisodes = new List<(int EpisodeNumber, DateTime? Aired)>();
            var previousEpisode = -1;
            foreach (var ep in orderedEpisodes)
            {
                if (ep.EpisodeNumber - 1 == previousEpisode)
                {
                    // This is the next one
                    previousEpisodes.Add(ep);
                }
                else
                {
                    if (previousEpisodes.Count > 1)
                    {
                        // End it
                        (int EpisodeNumber, DateTime? Aired) first = previousEpisodes.First();
                        (int EpisodeNumber, DateTime? Aired) last = previousEpisodes.Last();
                        
                        epSb.Append($"{first.EpisodeNumber}-{last.EpisodeNumber}{(last.Aired.HasValue ? $" {last.Aired.Value:MM/yyyy}" : string.Empty)}, ");
                    }
                    else if (previousEpisodes.Count == 1)
                    {
                        (int EpisodeNumber, DateTime? Aired) = previousEpisodes.FirstOrDefault();
                        epSb.Append($"{EpisodeNumber}{(Aired.HasValue ? $" {Aired.Value:MM/yyyy}" : string.Empty)}, ");
                    }
                    // New one
                    previousEpisodes.Clear();
                    previousEpisodes.Add(ep);
                }
                previousEpisode = ep.EpisodeNumber;
            }

            if (previousEpisodes.Count > 1)
            {
                // Got some left over
                (int EpisodeNumber, DateTime? Aired) first = previousEpisodes.First();
                (int EpisodeNumber, DateTime? Aired) last = previousEpisodes.Last();
                epSb.Append($"{first.EpisodeNumber}-{last.EpisodeNumber}{(last.Aired.HasValue ? $" {last.Aired.Value:MM/yyyy}" : string.Empty)}");
            }
            else if (previousEpisodes.Count == 1)
            {
                (int EpisodeNumber, DateTime? Aired) = previousEpisodes.FirstOrDefault();
                // string.Empty used instead of "" as extra space was being added at the end
                epSb.Append($"{EpisodeNumber}{(Aired.HasValue ? $" {Aired.Value:d}" : string.Empty)}");
            }

            return epSb.ToString();
        }

        public static string BuildEpisodeList(IEnumerable<int> orderedEpisodes)
        {
            var epSb = new StringBuilder();
            var previousEpisodes = new List<int>();
            var previousEpisode = -1;
            foreach (var ep in orderedEpisodes)
            {
                if (ep - 1 == previousEpisode)
                {
                    // This is the next one
                    previousEpisodes.Add(ep);
                }
                else
                {
                    if (previousEpisodes.Count > 1)
                    {
                        // End it
                        epSb.Append($"{previousEpisodes.First()}-{previousEpisodes.Last()}, ");
                    }
                    else if (previousEpisodes.Count == 1)
                    {
                        epSb.Append($"{previousEpisodes.FirstOrDefault()}, ");
                    }
                    // New one
                    previousEpisodes.Clear();
                    previousEpisodes.Add(ep);
                }
                previousEpisode = ep;
            }

            if (previousEpisodes.Count > 1)
            {
                // Got some left over
                epSb.Append($"{previousEpisodes.First()}-{previousEpisodes.Last()}");
            }
            else if (previousEpisodes.Count == 1)
            {
                epSb.Append(previousEpisodes.FirstOrDefault());
            }

            return epSb.ToString();
        }

        public static string RemoveSpaces(this string str)
        {
            return str.Replace(" ", "");
        }
        public static string StripCharacters(this string str, params char[] chars)
        {
            return string.Concat(str.Where(c => !chars.Contains(c)));
        }

        public static string ToHttpsUrl(this string currentUrl)
        {
            return currentUrl.Replace("http://", "https://");
        }


    }
}