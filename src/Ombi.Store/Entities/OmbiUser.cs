using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;

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

        public string Language { get; set; }

        /// <summary>
        /// Used to get the Streaming information for media
        /// </summary>
        [Required]
        public string StreamingCountry { get; set; }

        public int? MovieRequestLimit { get; set; }
        public int? EpisodeRequestLimit { get; set; }
        public int? MusicRequestLimit { get; set; }

        public string UserAccessToken { get; set; }

        public List<NotificationUserId> NotificationUserIds { get; set; }
        public List<UserNotificationPreferences> UserNotificationPreferences { get; set; }

        [NotMapped]
        public bool IsEmbyConnect => UserType == UserType.EmbyConnectUser;

        [NotMapped]
        public virtual string UserAlias => string.IsNullOrEmpty(Alias) ? UserName : Alias;

        [NotMapped]
        public bool EmailLogin { get; set; }

        [NotMapped] public bool IsSystemUser => UserType == UserType.SystemUser;

        [JsonIgnore]
        public override string PasswordHash
        {
            get => base.PasswordHash;
            set => base.PasswordHash = value;
        }

        [JsonIgnore]
        public override string SecurityStamp
        {
            get => base.SecurityStamp;
            set => base.SecurityStamp = value;
        }

        [JsonIgnore]
        public override string ConcurrencyStamp
        {
            get => base.ConcurrencyStamp;
            set => base.ConcurrencyStamp = value;
        }

    }
}