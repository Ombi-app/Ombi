using System;

using PlexRequests.Helpers.Exceptions;

namespace PlexRequests.Helpers
{
    public static class UriHelper
    {

        public static Uri ReturnUri(this string val)
        {
            if (val == null)
            {
                throw new ApplicationSettingsException("The URI is null, please check your settings to make sure you have configured the applications correctly.");
            }
            try
            {
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
                        ? new UriBuilder(Uri.UriSchemeHttp, split[0], port, "/" + split[2])
                        : new UriBuilder(Uri.UriSchemeHttp, split[0], port);
                }
                else
                {
                    uri = new UriBuilder(Uri.UriSchemeHttp, val);
                }

                return uri.Uri;
            }
            catch (Exception exception)
            {
                throw new Exception(exception.Message, exception);
            }
        }



        /// <summary>
        /// Returns the URI.
        /// </summary>
        /// <param name="val">The value.</param>
        /// <param name="port">The port.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception"></exception>
        public static Uri ReturnUri(this string val, int port)
        {
            if (val == null)
            {
                throw new ApplicationSettingsException("The URI is null, please check your settings to make sure you have configured the applications correctly.");
            }
            try
            {
                var uri = new UriBuilder();

                if (val.StartsWith("http://", StringComparison.Ordinal))
                {
                    var split = val.Split('/');
                    uri = split.Length >= 4 ? new UriBuilder(Uri.UriSchemeHttp, split[2], port, "/" + split[3]) : new UriBuilder(new Uri($"{val}:{port}"));
                }
                else if (val.StartsWith("https://", StringComparison.Ordinal))
                {
                    var split = val.Split('/');
                    uri = split.Length >= 4 ? new UriBuilder(Uri.UriSchemeHttps, split[2], port, "/" + split[3]) : new UriBuilder(Uri.UriSchemeHttps, split[2], port);
                }
                else
                {
                    uri = new UriBuilder(Uri.UriSchemeHttp, val, port);
                }

                return uri.Uri;
            }
            catch (Exception exception)
            {
                throw new Exception(exception.Message, exception);
            }
        }
    }
}