using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Ombi.Helpers;
using Ombi.Store.Entities;
using Ombi.Store.Entities.Requests;
using Ombi.Store.Repository.Requests;

namespace Ombi.Store.Context
{
    public abstract class OmbiContext : IdentityDbContext<OmbiUser>
    {
        protected OmbiContext(DbContextOptions<OmbiContext> options) : base(options)
        {

        }


        /// <summary>
        /// This allows a sub class to call the base class 'DbContext' non typed constructor
        /// This is need because instances of the subclasses will use a specific typed DbContextOptions
        /// which can not be converted into the parameter in the above constructor
        /// </summary>
        /// <param name="options"></param>
        protected OmbiContext(DbContextOptions options)
            : base(options)
        {
        }


        public DbSet<NotificationTemplates> NotificationTemplates { get; set; }

        public DbSet<MovieRequests> MovieRequests { get; set; }
        public DbSet<AlbumRequest> AlbumRequests { get; set; }
        public DbSet<TvRequests> TvRequests { get; set; }
        public DbSet<ChildRequests> ChildRequests { get; set; }
        public DbSet<EpisodeRequests> EpisodeRequests { get; set; }

        public DbSet<Issues> Issues { get; set; }
        public DbSet<IssueCategory> IssueCategories { get; set; }
        public DbSet<IssueComments> IssueComments { get; set; }
        public DbSet<RequestLog> RequestLogs { get; set; }
        public DbSet<RecentlyAddedLog> RecentlyAddedLogs { get; set; }
        public DbSet<Votes> Votes { get; set; }


        public DbSet<Audit> Audit { get; set; }
        public DbSet<Tokens> Tokens { get; set; }
        public DbSet<RequestSubscription> RequestSubscription { get; set; }
        public DbSet<UserNotificationPreferences> UserNotificationPreferences { get; set; }
        public DbSet<MobileDevices> MobileDevices { get; set; }
        public DbSet<UserQualityProfiles> UserQualityProfileses { get; set; }
        public DbSet<RequestQueue> RequestQueue { get; set; }

        public void Seed()
        {
            var strat = Database.CreateExecutionStrategy();
            strat.Execute(() =>
            {
                using (var tran = Database.BeginTransaction())
                {
                    // Make sure we have the API User
                    var apiUserExists = Users.ToList().Any(x => x.NormalizedUserName == "API");
                    if (!apiUserExists)
                    {
                        Users.Add(new OmbiUser
                        {
                            UserName = "Api",
                            UserType = UserType.SystemUser,
                            NormalizedUserName = "API",
                            StreamingCountry = "US"
                        });
                        SaveChanges();
                        tran.Commit();
                    }
                }
            });

            //Check if templates exist
            var templates = NotificationTemplates.ToList();

            var allAgents = Enum.GetValues(typeof(NotificationAgent)).Cast<NotificationAgent>().ToList();
            var allTypes = Enum.GetValues(typeof(NotificationType)).Cast<NotificationType>().ToList();

            var needToSave = false;
            foreach (var agent in allAgents)
            {
                foreach (var notificationType in allTypes)
                {
                    if (templates.Any(x => x.Agent == agent && x.NotificationType == notificationType))
                    {
                        // We already have this
                        continue;
                    }

                    needToSave = true;
                    NotificationTemplates notificationToAdd = null;
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
                                Message = "Hello! Your request for {Title} on {ApplicationName} is now available! :)",
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
                            notificationToAdd = new NotificationTemplates
                            {
                                NotificationType = notificationType,
                                Message = "Hello! The user '{UserName}' has requested {Title} but it could not be added. This has been added into the requests queue and will keep retrying",
                                Subject = "Item Added To Retry Queue",
                                Agent = agent,
                                Enabled = true,
                            };
                            break;
                        case NotificationType.WelcomeEmail:
                            if (agent == NotificationAgent.Email)
                            {
                                notificationToAdd = new NotificationTemplates
                                {
                                    NotificationType = notificationType,
                                    Message = "Hello! You have been invited to use {ApplicationName}! You can login here: {ApplicationUrl}",
                                    Subject = "Invite to {ApplicationName}",
                                    Agent = agent,
                                    Enabled = true,
                                };
                            }
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
                    if (notificationToAdd != null)
                    {
                        NotificationTemplates.Add(notificationToAdd);
                    }
                }
            }

            if (needToSave)
            {
                strat.Execute(() =>
                {
                    using (var tran = Database.BeginTransaction())
                    {
                        SaveChanges();
                        tran.Commit();
                    }
                });
            }
        }
    }
}
