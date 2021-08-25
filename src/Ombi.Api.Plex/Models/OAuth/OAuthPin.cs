using System;
using System.Collections.Generic;

namespace Ombi.Api.Plex.Models.OAuth
{
    public class OAuthContainer
    {
        public OAuthPin Result { get; set; }
        public OAuthErrorsContainer Errors { get; set; }
    }

    public class OAuthPin
    {
        public int id { get; set; }
        public string code { get; set; }
        public bool trusted { get; set; }
        public string clientIdentifier { get; set; }
        public Location location { get; set; }
        public int expiresIn { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime expiresAt { get; set; }
        public string authToken { get; set; }
    }

    public class Location
    {
        public string code { get; set; }
        public string country { get; set; }
        public string city { get; set; }
        public string subdivisions { get; set; }
        public string coordinates { get; set; }
    }

    public class OAuthErrorsContainer
    {
        public List<OAuthErrors> errors { get; set; }
    }

    public class OAuthErrors
    {
        public int code { get; set; }
        public string message { get; set; }
    }

}