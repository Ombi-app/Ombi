using Microsoft.Extensions.Logging;

namespace Ombi.Helpers
{
    public class LoggingEvents
    {
        public static EventId Authentication => new EventId(500);

        public static EventId Api => new EventId(1000);
        public static EventId RadarrApi => new EventId(1001);
        public static EventId CouchPotatoApi => new EventId(1002);
        
        public static EventId Cacher => new EventId(2000);
        public static EventId RadarrCacher => new EventId(2001);
        public static EventId PlexEpisodeCacher => new EventId(2002);
        public static EventId EmbyContentCacher => new EventId(2003);
        public static EventId JellyfinContentCacher => new EventId(2012);
        public static EventId PlexUserImporter => new EventId(2004);
        public static EventId EmbyUserImporter => new EventId(2005);
        public static EventId JellyfinUserImporter => new EventId(2013);
        public static EventId SonarrCacher => new EventId(2006);
        public static EventId CouchPotatoCacher => new EventId(2007);
        public static EventId PlexContentCacher => new EventId(2008);
        public static EventId SickRageCacher => new EventId(2009);
        public static EventId LidarrArtistCache => new EventId(2010);
        public static EventId MediaReferesh => new EventId(2011);
        
        public static EventId MovieSender => new EventId(3000);

        public static EventId Notification => new EventId(4000);
        public static EventId DiscordNotification => new EventId(4001);
        public static EventId PushbulletNotification => new EventId(4002);
        public static EventId SlackNotification => new EventId(4003);
        public static EventId MattermostNotification => new EventId(4004);
        public static EventId PushoverNotification => new EventId(4005);
        public static EventId TelegramNotifcation => new EventId(4006);
        public static EventId GotifyNotification => new EventId(4007);
        public static EventId WhatsApp => new EventId(4008);
        public static EventId WebhookNotification => new EventId(4009);

        public static EventId TvSender => new EventId(5000);
        public static EventId SonarrSender => new EventId(5001);


        public static EventId Updater => new EventId(6000);

    }
}
