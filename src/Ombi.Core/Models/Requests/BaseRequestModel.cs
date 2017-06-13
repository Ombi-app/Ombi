using Newtonsoft.Json;
using Ombi.Store.Entities;
using System;

namespace Ombi.Core.Models.Requests
{
    public class BaseRequestModel : Entity
    {
        public int ProviderId { get; set; }
        public string Overview { get; set; }
        public string Title { get; set; }
        public string PosterPath { get; set; }
        public DateTime ReleaseDate { get; set; }
        public RequestType Type { get; set; }
        public string Status { get; set; }
        public bool Approved { get; set; }
        public bool Admin { get; set; }
        public DateTime RequestedDate { get; set; }
        public bool Available { get; set; }
        public IssueState Issues { get; set; }
        public string OtherMessage { get; set; }
        public string AdminNote { get; set; }
        public string RequestedUser { get; set; }
        public int IssueId { get; set; }
        public bool Denied { get; set; }
        public string DeniedReason { get; set; }

        [JsonIgnore]
        public bool Released => DateTime.UtcNow > ReleaseDate;


        [JsonIgnore]
        public bool CanApprove => !Approved && !Available;
        
        public bool UserHasRequested(string username)
        {
            return RequestedUser.Equals(username, StringComparison.OrdinalIgnoreCase);
        }
    }
}