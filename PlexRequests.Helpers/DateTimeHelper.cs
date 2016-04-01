using System;

namespace PlexRequests.Helpers
{
    public static class DateTimeHelper
    {
        public static DateTimeOffset OffsetUTCDateTime(DateTime utcDateTime, int minuteOffset)
        {
            TimeSpan ts = TimeSpan.FromMinutes(-minuteOffset);
            return new DateTimeOffset(utcDateTime).ToOffset(ts);
        }

    }
}
