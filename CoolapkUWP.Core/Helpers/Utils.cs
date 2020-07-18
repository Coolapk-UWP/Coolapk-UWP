using System;
using System.Security.Cryptography;
using System.Text;

namespace CoolapkUWP.Core.Helpers
{
    public static class Utils
    {
        public static string GetMD5(string input)
        {
            using (var md5 = MD5.Create())
            {
                var r1 = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
                var r2 = BitConverter.ToString(r1).ToLower();
                return r2.Replace("-", "");
            }
        }

        public enum TimeIntervalType
        {
            MonthsAgo,
            DaysAgo,
            HoursAgo,
            MinutesAgo,
            JustNow,
        }

        private static readonly DateTime unixDateBase = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        public static (TimeIntervalType type, object time) ConvertUnixTimeStampToReadable(double time, DateTime baseTime)
        {
            TimeSpan ttime = new TimeSpan((long)time * 1000_0000);
            DateTime tdate = unixDateBase.Add(ttime);
            TimeSpan temp = baseTime.ToUniversalTime()
                                    .Subtract(tdate);

            if (temp.TotalDays > 30)
            {
                return (TimeIntervalType.MonthsAgo, tdate);
            }
            else
            {
                TimeIntervalType type;
                if (temp.Days > 0) { type = TimeIntervalType.DaysAgo; }
                else if (temp.Hours > 0) { type = TimeIntervalType.HoursAgo; }
                else if (temp.Minutes > 0) { type = TimeIntervalType.MinutesAgo; }
                else { type = TimeIntervalType.JustNow; }

                return (type, temp);
            }
        }

        public static double ConvertDateTimeToUnixTimeStamp(DateTime time)
        {
            return Math.Round(
                time.ToUniversalTime()
                    .Subtract(unixDateBase)
                    .TotalSeconds);
        }
    }
}