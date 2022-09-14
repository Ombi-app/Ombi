using Ombi.Store.Entities;
using System;

namespace Ombi.Core.Models.Requests
{
    public class RecentlyRequestedModel
    {
        public int RequestId { get; set; }
        public RequestType Type { get; set; }
        public string UserId { get; set; }
        public string Username { get; set; }
        public bool Available { get; set; }
        public bool TvPartiallyAvailable { get; set; }
        public DateTime RequestDate { get; set; }
        public string Title { get; set; }
        public string Overview { get; set; }
        public DateTime ReleaseDate { get; set; }
        public bool Approved { get; set; }
        public string MediaId { get; set; }

        public string PosterPath { get; set; }
        public string Background { get; set; }
    }
}
