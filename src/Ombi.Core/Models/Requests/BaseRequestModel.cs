using Newtonsoft.Json;
using Ombi.Store.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

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
        public List<string> RequestedUsers { get; set; } = new List<string>();
        public int IssueId { get; set; }
        public bool Denied { get; set; }
        public string DeniedReason { get; set; }

        [JsonIgnore]
        public bool Released => DateTime.UtcNow > ReleaseDate;

        [JsonIgnore]
        public IEnumerable<string> AllUsers
        {
            get
            {
                var u = new List<string>();
                if (RequestedUsers != null && RequestedUsers.Any())
                    u.AddRange(RequestedUsers);
                return u;
            }
        }

        [JsonIgnore]
        public bool CanApprove => !Approved && !Available;

        public bool UserHasRequested(string username)
        {
            return AllUsers.Any(x => x.Equals(username, StringComparison.OrdinalIgnoreCase));
        }
    }
}