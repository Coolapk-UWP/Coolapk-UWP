using System;
using System.Security.Cryptography;
using System.Text;

namespace CoolapkUWP.Core.Helpers
{
    public static class DataHelper
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

        private static readonly DateTime unixDateBase = new DateTime(1970, 1, 1);
        public static string ConvertUnixTimeToReadable(double beijingTime)
        {
            TimeSpan ttime = new TimeSpan(Convert.ToInt64(beijingTime) * 1000_0000);
            DateTime tdate = unixDateBase.Add(ttime);
            TimeSpan temp = DateTime.Now.ToUniversalTime().Subtract(tdate);

            if (temp.TotalDays > 30)
            {
                return $"{tdate.Year}/{tdate.Month}/{tdate.Day}";
            }
            else if (temp.Days > 0)
            {
                return $"{temp.Days}天前";
            }
            else if (temp.Hours > 0)
            {
                return $"{temp.Hours}小时前";
            }
            else if (temp.Minutes > 0)
            {
                return $"{temp.Minutes}分钟前";
            }
            else
            {
                return "刚刚";
            }
        }

        public static double ConvertTimeToUnix(DateTime time)
        {
            var r = time.ToUniversalTime().Subtract(unixDateBase).TotalSeconds;
            return Math.Round(r);
        }
    }
}