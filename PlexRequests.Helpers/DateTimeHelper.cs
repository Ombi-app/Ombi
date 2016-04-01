using System;
using System.Linq;

namespace PlexRequests.Helpers
{
    public static class DateTimeHelper
    {
        public static DateTimeOffset OffsetUTCDateTime(DateTime utcDateTime, int minuteOffset)
        {
            //TimeSpan ts = TimeSpan.FromMinutes(-minuteOffset);
            //return new DateTimeOffset(utcDateTime).ToOffset(ts);

            // this is a workaround below to work with MONO
            var tzi = FindTimeZoneFromOffset(minuteOffset);
            var utcOffset = tzi.GetUtcOffset(utcDateTime);
            var newDate = utcDateTime + utcOffset;
            return new DateTimeOffset(newDate.Ticks, utcOffset);
        }

        private static TimeZoneInfo FindTimeZoneFromOffset(int minuteOffset)
        {
            var tzc = TimeZoneInfo.GetSystemTimeZones();
            return tzc.FirstOrDefault(x => x.BaseUtcOffset.TotalMinutes == -minuteOffset);
        }
    }
}
