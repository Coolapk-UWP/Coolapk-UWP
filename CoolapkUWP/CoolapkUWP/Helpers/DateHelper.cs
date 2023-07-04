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

        private static readonly DateTime UnixDateBase = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static string ConvertUnixTimeStampToReadable(this long time) => ConvertUnixTimeStampToReadable(time, DateTime.Now);

        public static string ConvertUnixTimeStampToReadable(this long time, DateTime? baseTime)
        {
            object obj;
            TimeIntervalType type;

            TimeSpan ttime = new TimeSpan(time * 1000_0000);
            DateTime tdate = UnixDateBase.Add(ttime);

            if (baseTime == null)
            {
                type = TimeIntervalType.MonthsAgo;
                obj = tdate;
            }
            else
            {
                TimeSpan temp = baseTime.Value.ToUniversalTime().Subtract(tdate);

                if (temp.TotalDays > 30)
                {
                    type = TimeIntervalType.MonthsAgo;
                    obj = tdate;
                }
                else
                {
                    type =
                        temp.Days > 0
                            ? TimeIntervalType.DaysAgo
                            : temp.Hours > 0
                                ? TimeIntervalType.HoursAgo
                                : temp.Minutes > 0
                                    ? TimeIntervalType.MinutesAgo
                                    : TimeIntervalType.JustNow;
                    obj = temp;
                }
            }

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

        public static DateTime ConvertUnixTimeStampToDateTime(this long time) => UnixDateBase.Add(new TimeSpan(time * 1000_0000));

        public static double ConvertDateTimeToUnixTimeStamp(this DateTime time) => Math.Round(time.ToUniversalTime().Subtract(UnixDateBase).TotalSeconds);
    }
}
