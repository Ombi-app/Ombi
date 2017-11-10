﻿using System;
using System.IO;
using System.Linq;
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
        public DbSet<MovieIssues> MovieIssues { get; set; }
        public DbSet<TvIssues> TvIssues { get; set; }

        public DbSet<Audit> Audit { get; set; }
        public DbSet<Tokens> Tokens { get; set; }
        public DbSet<SonarrCache> SonarrCache { get; set; }
        public DbSet<SonarrEpisodeCache> SonarrEpisodeCache { get; set; }

        public DbSet<ApplicationConfiguration> ApplicationConfigurations { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var i = StoragePathSingleton.Instance;
            if (string.IsNullOrEmpty(i.StoragePath))
            {
                i.StoragePath = string.Empty;
            }
            optionsBuilder.UseSqlite($"Data Source={Path.Combine(i.StoragePath,"Ombi.db")}");
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

            //Check if templates exist
            var templates = NotificationTemplates.ToList();
            //if (templates.Any())
            //{
            //    return;
            //}

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
                                Message = "Hello! The user '{RequestedUser}' has requested the {Type} '{Title}'! Please log in to approve this request. Request Date: {RequestedDate}",
                                Subject = "{ApplicationName}: New {Type} request for {Title}!",
                                Agent = agent,
                                Enabled = true,
                            };
                            break;
                        case NotificationType.Issue:
                            notificationToAdd = new NotificationTemplates
                            {
                                NotificationType = notificationType,
                                Message = "Hello! The user '{RequestedUser}' has reported a new issue for the title {Title}! </br> {Issue}",
                                Subject = "{ApplicationName}: New issue for {Title}!",
                                Agent = agent,
                                Enabled = true,
                            };
                            break;
                        case NotificationType.RequestAvailable:
                            notificationToAdd = new NotificationTemplates
                            {
                                NotificationType = notificationType,
                                Message = "Hello! You requested {Title} on {ApplicationName}! This is now available! :)",
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
                        case NotificationType.AdminNote:
                            continue;
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