using System;
using System.Collections.Generic;
using Ombi.Store.Entities;

namespace Ombi.Core.Models.UI
{
    public class UserViewModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Alias { get; set; }
        public List<ClaimCheckboxes> Claims { get; set; }
        public string EmailAddress { get; set; }
        public string Password { get; set; }
        public DateTime? LastLoggedIn { get; set; }
        public string Language { get; set; }
        public bool HasLoggedIn { get; set; }
        public UserType UserType { get; set; }
        public int MovieRequestLimit { get; set; }
        public int EpisodeRequestLimit { get; set; }
        public string StreamingCountry { get; set; }
        public RequestQuotaCountModel EpisodeRequestQuota { get; set; }
        public RequestQuotaCountModel MovieRequestQuota { get; set; }
        public RequestQuotaCountModel MusicRequestQuota { get; set; }
        public int MusicRequestLimit { get; set; }
        public UserQualityProfiles UserQualityProfiles { get; set; }
    }

    public class ClaimCheckboxes
    {
        public string Value { get; set; }
        public bool Enabled { get; set; }
    }

    public class UserViewModelDropdown
    {
        public string Id { get; set; }
        public string Username { get; set; }
    }
}