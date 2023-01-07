using System;

namespace CoolapkUWP.Helpers
{
    public static class DateHelper
    {
        public enum TimeIntervalType
        {
            MonthsAgo,
            DaysAgo,
            HoursAgo,
            MinutesAgo,
            JustNow,
        }

        private static readonly DateTime unixDateBase = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static string ConvertUnixTimeStampToReadable(this double time)
        {
            return ConvertUnixTimeStampToReadable(time, DateTime.Now);
        }

        public static string ConvertUnixTimeStampToReadable(this double time, DateTime baseTime)
        {
            (TimeIntervalType type, object time) ConvertUnixTimeStampToReadable(double times, DateTime baseTimes)
            {
                TimeSpan ttime = new TimeSpan((long)times * 1000_0000);
                DateTime tdate = unixDateBase.Add(ttime);
                TimeSpan temp = baseTimes.ToUniversalTime()
                                        .Subtract(tdate);

                if (temp.TotalDays > 30)
                {
                    return (TimeIntervalType.MonthsAgo, tdate);
                }
                else
                {
                    TimeIntervalType timetype = temp.Days > 0
                        ? TimeIntervalType.DaysAgo
                        : temp.Hours > 0 ? TimeIntervalType.HoursAgo : temp.Minutes > 0 ? TimeIntervalType.MinutesAgo : TimeIntervalType.JustNow;
                    return (timetype, temp);
                }
            }

            (TimeIntervalType type, object obj) = ConvertUnixTimeStampToReadable(time, baseTime);
            switch (type)
            {
                case TimeIntervalType.MonthsAgo:
                    return $"{((DateTime)obj).Year}/{((DateTime)obj).Month}/{((DateTime)obj).Day}";

                case TimeIntervalType.DaysAgo:
                    return $"{((TimeSpan)obj).Days}天前";

                case TimeIntervalType.HoursAgo:
                    return $"{((TimeSpan)obj).Hours}小时前";

                case TimeIntervalType.MinutesAgo:
                    return $"{((TimeSpan)obj).Minutes}分钟前";

                case TimeIntervalType.JustNow:
                    return "刚刚";

                default:
                    return string.Empty;
            }
        }

        public static DateTime ConvertUnixTimeStampToDateTime(this double time) => unixDateBase.Add(new TimeSpan((long)time * 1000_0000));

        public static double ConvertDateTimeToUnixTimeStamp(this DateTime time) => Math.Round(time.ToUniversalTime().Subtract(unixDateBase).TotalSeconds);
    }
}
