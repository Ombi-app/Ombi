using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ombi.Helpers;
using Ombi.Store.Entities;

namespace Ombi.Store.Context
{
    public sealed class OmbiContext : DbContext, IOmbiContext
    {
        private static bool _created;
        public OmbiContext()
        {
            if (_created) return;

            _created = true;
            Database.EnsureCreated();
            Database.Migrate();

#if DEBUG
            var location = System.Reflection.Assembly.GetEntryAssembly().Location;
            var directory = System.IO.Path.GetDirectoryName(location);
            var file = File.ReadAllText(Path.Combine(directory,"SqlTables.sql"));
#else

            var file = File.ReadAllText("SqlTables.sql");
#endif
            // Run Script

            Database.ExecuteSqlCommand(file, 0);
            
            // Add the notifcation templates
            AddAllTemplates();
        }

        public DbSet<RequestBlobs> Requests { get; set; }
        public DbSet<GlobalSettings> Settings { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<PlexContent> PlexContent { get; set; }
        public DbSet<RadarrCache> RadarrCache { get; set; }
        public DbSet<NotificationTemplates> NotificationTemplates { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=Ombi.db");
        }


        private void AddAllTemplates()
        {
            // Check if templates exist
            var templates = NotificationTemplates.ToList();
            if (templates.Any())
            {
                return;
            }

            var allAgents = Enum.GetValues(typeof(NotificationAgent)).Cast<NotificationAgent>().ToList();
            var allTypes = Enum.GetValues(typeof(NotificationType)).Cast<NotificationType>().ToList();

            foreach (var agent in allAgents)
            {
                foreach (var notificationType in allTypes)
                {
                    var notificationToAdd = new NotificationTemplates();
                    switch (notificationType)
                    {
                        case NotificationType.NewRequest:
                            notificationToAdd = new NotificationTemplates
                            {
                                NotificationType = notificationType,
                                Message = "Hello! The user '{requestedUser}' has requested the {Type} '{Title}'! Please log in to approve this request. Request Date: {RequestedDate}",
                                Subject = "Ombi: New {Type} request for {Title}!",
                                Agent = agent,
                            };
                            break;
                        case NotificationType.Issue:
                            notificationToAdd = new NotificationTemplates
                            {
                                NotificationType = notificationType,
                                Message = "Hello! The user '{requestedUser}' has reported a new issue for the title {Title}! </br> {Issue}",
                                Subject = "Ombi: New issue for {Title}!",
                                Agent = agent,
                            };
                            break;
                        case NotificationType.RequestAvailable:
                            notificationToAdd = new NotificationTemplates
                            {
                                NotificationType = notificationType,
                                Message = "Hello! You requested {Title} on Ombi! This is now available! :)",
                                Subject = "Ombi: {Title} is now available!",
                                Agent = agent,
                            };
                            break;
                        case NotificationType.RequestApproved:
                            notificationToAdd = new NotificationTemplates
                            {
                                NotificationType = notificationType,
                                Message = "Hello! Your request for {Title} has been approved!",
                                Subject = "Ombi: your request has been approved",
                                Agent = agent,
                            };
                            break;
                        case NotificationType.AdminNote:
                            continue;
                        case NotificationType.Test:
                            continue;
                        case NotificationType.RequestDeclined:
                            notificationToAdd = new NotificationTemplates
                            {
                                //"Ombi: Your request has been declined",
                                //$"Hello! Your request for {model.Title} has been declined, Sorry!",
                                NotificationType = notificationType,
                                Message = "Hello! Your request for {Title} has been declined, Sorry!",
                                Subject = "Ombi: your request has been declined",
                                Agent = agent,
                            };
                            break;
                        case NotificationType.ItemAddedToFaultQueue:
                            continue;
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