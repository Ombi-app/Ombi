using System;

namespace Ombi.Helpers
{
    public static class UriHelper
    {
        private const string Https = "Https";
        private const string Http = "Http";

        public static Uri ReturnUri(this string val)
        {
            if (val == null)
            {
                throw new ApplicationSettingsException("The URI is null, please check your settings to make sure you have configured the applications correctly.");
            }
            var uri = new UriBuilder();

            if (val.StartsWith("http://", StringComparison.Ordinal))
            {
                uri = new UriBuilder(val);
            }
            else if (val.StartsWith("https://", StringComparison.Ordinal))
            {
                uri = new UriBuilder(val);
            }
            else if (val.Contains(":"))
            {
                var split = val.Split(':', '/');
                int port;
                int.TryParse(split[1], out port);

                uri = split.Length == 3
                    ? new UriBuilder(Http, split[0], port, "/" + split[2])
                    : new UriBuilder(Http, split[0], port);
            }
            else
            {
                if(val.EndsWith("/"))
                {
                    // Remove a trailing slash, since the URIBuilder adds one
                    val = val.Remove(val.Length - 1, 1);
                }
                uri = new UriBuilder(Http, val);
            }

            return uri.Uri;
        }

        /// <summary>
        /// Returns the URI.
        /// </summary>
        /// <param name="val">The value.</param>
        /// <param name="port">The port.</param>
        /// <param name="ssl">if set to <c>true</c> [SSL].</param>
        /// <param name="subdir">The subdir.</param>
        /// <returns></returns>
        /// <exception cref="ApplicationSettingsException">The URI is null, please check your settings to make sure you have configured the applications correctly.</exception>
        /// <exception cref="System.Exception"></exception>
        public static Uri ReturnUri(this string val, int port, bool ssl = default(bool))
        {
            if (val == null)
            {
                throw new ApplicationSettingsException("The URI is null, please check your settings to make sure you have configured the applications correctly.");
            }
            var uri = new UriBuilder();

            if (val.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase))
            {
                var split = val.Split('/');
                uri = split.Length >= 4 ? new UriBuilder(Http, split[2], port, "/" + split[3]) : new UriBuilder(new Uri($"{val}:{port}"));
            }
            else if (val.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase))
            {
                var split = val.Split('/');
                uri = split.Length >= 4
                    ? new UriBuilder(Https, split[2], port, "/" + split[3])
                    : new UriBuilder(Https, split[2], port);
            }
            else if ((ssl || port == 443) && port != 80)
            {
                if (val.EndsWith("/"))
                {
                    // Remove a trailing slash, since the URIBuilder adds one
                    val = val.Remove(val.Length - 1, 1);
                }
                uri = new UriBuilder(Https, val, port);
            }
            else
            {
                if (val.EndsWith("/"))
                {
                    // Remove a trailing slash, since the URIBuilder adds one
                    val = val.Remove(val.Length - 1, 1);
                }
                uri = new UriBuilder(Http, val, port);
            }

            return uri.Uri;
        }

        public static Uri ReturnUriWithSubDir(this string val, int port, bool ssl, string subDir)
        {
            var uriBuilder = new UriBuilder(val);
            if (ssl)
            {
                uriBuilder.Scheme = Https;
            }
            if (!string.IsNullOrEmpty(subDir))
            {
                uriBuilder.Path = subDir;
            }
            uriBuilder.Port = port;

            return uriBuilder.Uri;
        }

    }

    public class ApplicationSettingsException : Exception
    {
        public ApplicationSettingsException(string s) : base(s)
        {
        }
    }
}