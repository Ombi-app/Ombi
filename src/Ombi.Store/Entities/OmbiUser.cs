using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Ombi.Helpers;

namespace Ombi.Store.Entities
{
    public class OmbiUser : IdentityUser
    {
        public string Alias { get; set; }
        public UserType UserType { get; set; }

        /// <summary>
        /// This will be the unique Plex/Emby user id reference
        /// </summary>
        public string ProviderUserId { get; set; } 

        public DateTime? LastLoggedIn { get; set; }

        public string EmbyConnectUserId { get; set; }

        public int? MovieRequestLimit { get; set; }
        public int? EpisodeRequestLimit { get; set; }

        public string UserAccessToken { get; set; }

        public List<NotificationUserId> NotificationUserIds { get; set; }

        [NotMapped]
        public bool IsEmbyConnect => UserType == UserType.EmbyUser && EmbyConnectUserId.HasValue();

        [NotMapped]
        public string UserAlias => string.IsNullOrEmpty(Alias) ? UserName : Alias;
    }
}