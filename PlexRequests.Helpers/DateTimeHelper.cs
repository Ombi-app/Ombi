using System;
using System.Globalization;
using System.Linq;

namespace PlexRequests.Helpers
{
    public static class DateTimeHelper
    {
        public static DateTimeOffset OffsetUTCDateTime(DateTime utcDateTime, int minuteOffset)
        {
            TimeSpan ts = TimeSpan.FromMinutes(-minuteOffset);
            return new DateTimeOffset(utcDateTime).ToOffset(ts);

            // this is a workaround below to work with MONO
            //var tzi = FindTimeZoneFromOffset(minuteOffset);
            //var utcOffset = tzi.GetUtcOffset(utcDateTime);
            //var newDate = utcDateTime + utcOffset;
            //return new DateTimeOffset(newDate.Ticks, utcOffset);
        }

        public static void CustomParse(string date, out DateTime dt)
        {
            // Try and parse it
            if (DateTime.TryParse(date, out dt))
            {
                return;
            }

            // Maybe it's only a year?
            if (DateTime.TryParseExact(date, "yyyy", CultureInfo.CurrentCulture, DateTimeStyles.None, out dt))
            {
                return;
            }           
        }

        private static TimeZoneInfo FindTimeZoneFromOffset(int minuteOffset)
        {
            var tzc = TimeZoneInfo.GetSystemTimeZones();
            return tzc.FirstOrDefault(x => x.BaseUtcOffset.TotalMinutes == -minuteOffset);
        }

        public static DateTime UnixTimeStampToDateTime(this int unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }
    }
}
