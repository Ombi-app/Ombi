using Microsoft.Extensions.Logging;

namespace Ombi.Helpers
{
    public class LoggingEvents
    {
        public static EventId Authentication => new EventId(500);

        public static EventId Api => new EventId(1000);
        public static EventId RadarrApi => new EventId(1001);
        
        public static EventId Cacher => new EventId(2000);
        public static EventId RadarrCacher => new EventId(2001);
        public static EventId PlexEpisodeCacher => new EventId(2002);
        public static EventId EmbyContentCacher => new EventId(2003);
        public static EventId PlexUserImporter => new EventId(2004);
        public static EventId EmbyUserImporter => new EventId(2005);
        public static EventId SonarrCacher => new EventId(2006);
        
        public static EventId MovieSender => new EventId(3000);

        public static EventId Notification => new EventId(4000);
        public static EventId DiscordNotification => new EventId(4001);
        public static EventId PushbulletNotification => new EventId(4002);
        public static EventId SlackNotification => new EventId(4003);
        public static EventId MattermostNotification => new EventId(4004);
        public static EventId PushoverNotification => new EventId(4005);

        public static EventId TvSender => new EventId(5000);
        public static EventId SonarrSender => new EventId(5001);


        public static EventId Updater => new EventId(6000);

    }
}