using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CoolapkUWP.Core.Helpers
{
    public enum MessageType
    {
        Message,
        NoMore,
        NoMoreReply,
        NoMoreLikeUser,
        NoMoreShare,
        NoMoreHotReply,
    }

    public static partial class Utils
    {
        public static event EventHandler<(MessageType, string)> NeedShowInAppMessageEvent;

        internal static void ShowInAppMessage(MessageType type, string message = null)
        {
            NeedShowInAppMessageEvent?.Invoke(null, (type, message));
        }

        public static string GetMD5(string input)
        {
            using (var md5 = MD5.Create())
            {
                var r1 = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
                var r2 = BitConverter.ToString(r1).ToLowerInvariant();
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

    public static partial class Utils
    {
        private static readonly Dictionary<Uri, (DateTime, string)> responseCache = new Dictionary<Uri, (DateTime, string)>();

        private static readonly object locker = new object();

        private readonly static Timer cleanCacheTimer = new Timer(o =>
        {
            lock (locker)
            {
                var now = DateTime.Now;
                var needDelete = (from i in responseCache
                                  where (now - i.Value.Item1).TotalMinutes > 2
                                  select i.Key).ToArray();
                foreach (var item in needDelete)
                {
                    responseCache.Remove(item);
                }
            }
        }, null, TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(2));

        private static (bool, JToken) GetResult(string json)
        {
            var o = JObject.Parse(json);
            JToken token = null;
            if (!string.IsNullOrEmpty(json) &&
                !o.TryGetValue("data", out token) &&
                o.TryGetValue("message", out JToken message))
            {
                ShowInAppMessage(MessageType.Message, message.ToString());
                return (false, null);
            }
            else { return (true, token); }
        }

        public static async Task<(bool isSucceed, JToken result)> PostDataAsync(Uri uri, System.Net.Http.HttpContent content, IEnumerable<(string, string)> cookies)
        {
            var json = await NetworkHelper.PostAsync(uri, content, cookies);
            return GetResult(json);
        }

        public static async Task<(bool isSucceed, JToken result)> GetDataAsync(Uri uri, bool forceRefresh, IEnumerable<(string, string)> cookies)
        {
            string json;
            if (forceRefresh || !responseCache.ContainsKey(uri))
            {
                json = await NetworkHelper.GetSrtingAsync(uri, cookies);

                lock (locker)
                {
                    if (responseCache.ContainsKey(uri))
                    {
                        responseCache[uri] = (DateTime.Now, json);

                        int i = uri.PathAndQuery.IndexOf("page=", StringComparison.Ordinal);
                        if (i != -1)
                        {
                            string u = uri.PathAndQuery.Substring(i);

                            var needDelete = (from item in responseCache
                                              where item.Key != uri
                                              where item.Key.PathAndQuery.IndexOf(u, StringComparison.Ordinal) == 0
                                              select item).ToArray();
                            foreach (var item in needDelete)
                            {
                                responseCache.Remove(item.Key);
                            }
                        }
                    }
                    else
                    {
                        responseCache.Add(uri, (DateTime.Now, json));
                    }
                }
            }
            else
            {
                lock (locker)
                {
                    json = responseCache[uri].Item2;
                    responseCache[uri] = (DateTime.Now, json);
                }
            }

            return GetResult(json);
        }
    }
}