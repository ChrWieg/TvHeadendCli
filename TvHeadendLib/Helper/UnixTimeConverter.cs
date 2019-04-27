using System;

namespace TvHeadendLib.Helper
{
    public static class UnixTimeConverter
    {
        private static readonly DateTime UnixYear0 = new DateTime(1970, 1, 1);

        public static DateTime GetDateTimeFromUnixTime(long unixTimestamp)
        {
            var unixTimeStampInTicks = unixTimestamp * TimeSpan.TicksPerSecond;
            var result = new DateTime(UnixYear0.Ticks + unixTimeStampInTicks);
            return result.TrimToSeconds();
        }

        public static long GetUnixTimeFromDateTime(DateTime date)
        {
            var result = (date.TrimToSeconds().Ticks - UnixYear0.Ticks) / TimeSpan.TicksPerSecond;
            return result ;
        }

        public static DateTime TrimToSeconds(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second);
        }
    }
}
