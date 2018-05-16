﻿using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Ombi.Helpers;
using Ombi.Store.Entities;
using Ombi.Store.Entities.Requests;

namespace Ombi.Store.Context
{
    public sealed class OmbiContext : IdentityDbContext<OmbiUser>, IOmbiContext
    {
        private static bool _created;
        public OmbiContext()
        {
            if (_created) return;

            _created = true;
            Database.Migrate();
        }

        public DbSet<NotificationTemplates> NotificationTemplates { get; set; }
        public DbSet<GlobalSettings> Settings { get; set; }
        public DbSet<PlexServerContent> PlexServerContent { get; set; }
        public DbSet<PlexEpisode> PlexEpisode { get; set; }
        public DbSet<RadarrCache> RadarrCache { get; set; }
        public DbSet<CouchPotatoCache> CouchPotatoCache { get; set; }
        public DbSet<EmbyContent> EmbyContent { get; set; }
        public DbSet<EmbyEpisode> EmbyEpisode { get; set; }

        public DbSet<MovieRequests> MovieRequests { get; set; }
        public DbSet<TvRequests> TvRequests { get; set; }
        public DbSet<ChildRequests> ChildRequests { get; set; }

        public DbSet<Issues> Issues { get; set; }
        public DbSet<IssueCategory> IssueCategories { get; set; }
        public DbSet<IssueComments> IssueComments { get; set; }
        public DbSet<RequestLog> RequestLogs { get; set; }
        public DbSet<RecentlyAddedLog> RecentlyAddedLogs { get; set; }


        public DbSet<Audit> Audit { get; set; }
        public DbSet<Tokens> Tokens { get; set; }
        public DbSet<SonarrCache> SonarrCache { get; set; }
        public DbSet<SonarrEpisodeCache> SonarrEpisodeCache { get; set; }
        public DbSet<SickRageCache> SickRageCache { get; set; }
        public DbSet<SickRageEpisodeCache> SickRageEpisodeCache { get; set; }
        public DbSet<RequestSubscription> RequestSubscription { get; set; }

        public DbSet<ApplicationConfiguration> ApplicationConfigurations { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var i = StoragePathSingleton.Instance;
            if (string.IsNullOrEmpty(i.StoragePath))
            {
                i.StoragePath = string.Empty;
            }
            optionsBuilder.UseSqlite($"Data Source={Path.Combine(i.StoragePath, "Ombi.db")}");
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<PlexServerContent>().HasMany(x => x.Episodes)
                .WithOne(x => x.Series)
                .HasPrincipalKey(x => x.Key)
                .HasForeignKey(x => x.GrandparentKey);

            builder.Entity<EmbyEpisode>()
                .HasOne(p => p.Series)
                .WithMany(b => b.Episodes)
                .HasPrincipalKey(x => x.EmbyId)
                .HasForeignKey(p => p.ParentId);

            base.OnModelCreating(builder);
        }


        public void Seed()
        {

            // Add the tokens
            var fanArt = ApplicationConfigurations.FirstOrDefault(x => x.Type == ConfigurationTypes.FanartTv);
            if (fanArt == null)
            {
                ApplicationConfigurations.Add(new ApplicationConfiguration
                {
                    Type = ConfigurationTypes.FanartTv,
                    Value = "4b6d983efa54d8f45c68432521335f15"
                });
                SaveChanges();
            }
            var movieDb = ApplicationConfigurations.FirstOrDefault(x => x.Type == ConfigurationTypes.FanartTv);
            if (movieDb == null)
            {
                ApplicationConfigurations.Add(new ApplicationConfiguration
                {
                    Type = ConfigurationTypes.TheMovieDb,
                    Value = "b8eabaf5608b88d0298aa189dd90bf00"
                });
                SaveChanges();
            }
            var notification = ApplicationConfigurations.FirstOrDefault(x => x.Type == ConfigurationTypes.Notification);
            if (notification == null)
            {
                ApplicationConfigurations.Add(new ApplicationConfiguration
                {
                    Type = ConfigurationTypes.Notification,
                    Value = "4f0260c4-9c3d-41ab-8d68-27cb5a593f0e"
                });
                SaveChanges();
            }

            // VACUUM;
            Database.ExecuteSqlCommand("VACUUM;");

            // Make sure we have the roles
            var roles = Roles.Where(x => x.Name == OmbiRoles.RecievesNewsletter);
            if (!roles.Any())
            {
                Roles.Add(new IdentityRole(OmbiRoles.RecievesNewsletter)
                {
                    NormalizedName = OmbiRoles.RecievesNewsletter.ToUpper()
                });
                SaveChanges();
            }

            // Make sure we have the API User
            var apiUserExists = Users.Any(x => x.UserName.Equals("Api", StringComparison.CurrentCultureIgnoreCase));
            if (!apiUserExists)
            {
                Users.Add(new OmbiUser
                {
                    UserName = "Api",
                    UserType = UserType.SystemUser,
                    NormalizedUserName = "API",

                });
                SaveChanges();
            }

            //Check if templates exist
            var templates = NotificationTemplates.ToList();

            var allAgents = Enum.GetValues(typeof(NotificationAgent)).Cast<NotificationAgent>().ToList();
            var allTypes = Enum.GetValues(typeof(NotificationType)).Cast<NotificationType>().ToList();

            foreach (var agent in allAgents)
            {
                foreach (var notificationType in allTypes)
                {
                    if (templates.Any(x => x.Agent == agent && x.NotificationType == notificationType))
                    {
                        // We already have this
                        continue;
                    }
                    NotificationTemplates notificationToAdd;
                    switch (notificationType)
                    {
                        case NotificationType.NewRequest:
                            notificationToAdd = new NotificationTemplates
                            {
                                NotificationType = notificationType,
                                Message = "Hello! The user '{UserName}' has requested the {Type} '{Title}'! Please log in to approve this request. Request Date: {RequestedDate}",
                                Subject = "{ApplicationName}: New {Type} request for {Title}!",
                                Agent = agent,
                                Enabled = true,
                            };
                            break;
                        case NotificationType.Issue:
                            notificationToAdd = new NotificationTemplates
                            {
                                NotificationType = notificationType,
                                Message = "Hello! The user '{UserName}' has reported a new issue for the title {Title}! </br> {IssueCategory} - {IssueSubject} : {IssueDescription}",
                                Subject = "{ApplicationName}: New issue for {Title}!",
                                Agent = agent,
                                Enabled = true,
                            };
                            break;
                        case NotificationType.RequestAvailable:
                            notificationToAdd = new NotificationTemplates
                            {
                                NotificationType = notificationType,
                                Message = "Hello! You   {Title} on {ApplicationName}! This is now available! :)",
                                Subject = "{ApplicationName}: {Title} is now available!",
                                Agent = agent,
                                Enabled = true,
                            };
                            break;
                        case NotificationType.RequestApproved:
                            notificationToAdd = new NotificationTemplates
                            {
                                NotificationType = notificationType,
                                Message = "Hello! Your request for {Title} has been approved!",
                                Subject = "{ApplicationName}: your request has been approved",
                                Agent = agent,
                                Enabled = true,
                            };
                            break;
                        case NotificationType.Test:
                            continue;
                        case NotificationType.RequestDeclined:
                            notificationToAdd = new NotificationTemplates
                            {
                                NotificationType = notificationType,
                                Message = "Hello! Your request for {Title} has been declined, Sorry!",
                                Subject = "{ApplicationName}: your request has been declined",
                                Agent = agent,
                                Enabled = true,
                            };
                            break;
                        case NotificationType.ItemAddedToFaultQueue:
                            continue;
                        case NotificationType.WelcomeEmail:
                            notificationToAdd = new NotificationTemplates
                            {
                                NotificationType = notificationType,
                                Message = "Hello! You have been invited to use {ApplicationName}! You can login here: {ApplicationUrl}",
                                Subject = "Invite to {ApplicationName}",
                                Agent = agent,
                                Enabled = true,
                            };
                            break;
                        case NotificationType.IssueResolved:
                            notificationToAdd = new NotificationTemplates
                            {
                                NotificationType = notificationType,
                                Message = "Hello {UserName} Your issue for {Title} has now been resolved.",
                                Subject = "{ApplicationName}: Issue has been resolved for {Title}!",
                                Agent = agent,
                                Enabled = true,
                            };
                            break;

                        case NotificationType.IssueComment:
                            notificationToAdd = new NotificationTemplates
                            {
                                NotificationType = notificationType,
                                Message = "Hello, There is a new comment on your issue {IssueSubject}, The comment is: {NewIssueComment}",
                                Subject = "{ApplicationName}: New comment on issue {IssueSubject}!",
                                Agent = agent,
                                Enabled = true,
                            };
                            break;
                        case NotificationType.AdminNote:
                            continue;
                        case NotificationType.Newsletter:
                            notificationToAdd = new NotificationTemplates
                            {
                                NotificationType = notificationType,
                                Message = "Here is a list of Movies and TV Shows that have recently been added!",
                                Subject = "{ApplicationName}: Recently Added Content!",
                                Agent = agent,
                                Enabled = true,
                            };
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    NotificationTemplates.Add(notificationToAdd);
                }
            }
            SaveChanges();
        }
    }
}