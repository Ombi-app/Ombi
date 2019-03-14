using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;

namespace Ombi.Api
{
    public class Request
    {
        public Request()
        {

        }

        public Request(string endpoint, string baseUrl, HttpMethod http, ContentType contentType = ContentType.Json)
        {
            Endpoint = endpoint;
            BaseUrl = baseUrl;
            HttpMethod = http;
            ContentType = contentType;
        }

        public ContentType ContentType { get; }
        public string Endpoint { get; }
        public string BaseUrl { get; }
        public HttpMethod HttpMethod { get; }
        public bool IgnoreErrors { get; set; }
        public bool Retry { get; set; }
        public List<HttpStatusCode> StatusCodeToRetry { get; set; } = new List<HttpStatusCode>();
        public bool IgnoreBaseUrlAppend { get; set; }

        public Action<string> OnBeforeDeserialization { get; set; }

        private string FullUrl
        {
            get
            {
                var sb = new StringBuilder();
                if (!string.IsNullOrEmpty(BaseUrl))
                {
                    sb.Append(!BaseUrl.EndsWith("/") && !IgnoreBaseUrlAppend ? string.Format("{0}/", BaseUrl) : BaseUrl);
                }
                sb.Append(Endpoint.StartsWith("/") ? Endpoint.Remove(0, 1) : Endpoint);
                return sb.ToString();
            }
        }

        private Uri _modified;

        public Uri FullUri
        {
            get => _modified != null ? _modified : new Uri(FullUrl);
            set => _modified = value;
        }

        public List<KeyValuePair<string, string>> Headers { get; } = new List<KeyValuePair<string, string>>();
        public List<KeyValuePair<string, string>> ContentHeaders { get; } = new List<KeyValuePair<string, string>>();

        public object JsonBody { get; private set; }

        public bool IsValidUrl
        {
            get
            {
                try
                {
                    // ReSharper disable once ObjectCreationAsStatement
                    new Uri(FullUrl);
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public void AddHeader(string key, string value)
        {
            Headers.Add(new KeyValuePair<string, string>(key, value));
        }
        public void AddContentHeader(string key, string value)
        {
            ContentHeaders.Add(new KeyValuePair<string, string>(key, value));
        }

        public void ApplicationJsonContentType()
        {
            AddContentHeader("Content-Type", "application/json");
        }

        public void AddQueryString(string key, string value)
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value)) return;

            var builder = new UriBuilder(FullUri);
            var startingTag = string.Empty;
            var hasQuery = false;
            if (string.IsNullOrEmpty(builder.Query))
            {
                startingTag = "?";
            }
            else
            {
                hasQuery = true;
                startingTag = builder.Query.Contains("?") ? "&" : "?";
            }
            builder.Query = hasQuery
                ? $"{builder.Query}{startingTag}{key}={value}"
                : $"{startingTag}{key}={value}";

            _modified = builder.Uri;
        }

        public void AddJsonBody(object obj)
        {
            JsonBody = obj;
        }
    }

}