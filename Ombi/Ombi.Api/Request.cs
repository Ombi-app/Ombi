using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

namespace Ombi.Api
{
    public class Request
    {
        public Request(string endpoint, string baseUrl, HttpMethod http)
        {
            Endpoint = endpoint;
            BaseUrl = baseUrl;
            HttpMethod = http;
        }

        public string Endpoint { get; }
        public string BaseUrl { get; }
        public HttpMethod HttpMethod { get; }

        private string FullUrl
        {
            get
            {
                var sb = new StringBuilder();
                sb.Append(!BaseUrl.EndsWith("/") ? string.Format("{0}/", BaseUrl) : BaseUrl);
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

        public object JsonBody { get; set; }

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

        public void AddJsonBody(object obj)
        {
            JsonBody = obj;
        }
    }
}