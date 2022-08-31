using Newtonsoft.Json.Linq;
using ReverseMarkdown;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
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

        public static string GetMD5(this string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] r1 = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
                string r2 = BitConverter.ToString(r1).ToLowerInvariant();
                return r2.Replace("-", "");
            }
        }

        public static string GetBase64(this string input, bool israw = false)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            string result = Convert.ToBase64String(bytes);
            if (israw) { result = result.Replace("=", ""); }
            return result;
        }

        public static string Reverse(this string text)
        {
            char[] charArray = text.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
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
                TimeIntervalType type = temp.Days > 0
                    ? TimeIntervalType.DaysAgo
                    : temp.Hours > 0 ? TimeIntervalType.HoursAgo : temp.Minutes > 0 ? TimeIntervalType.MinutesAgo : TimeIntervalType.JustNow;
                return (type, temp);
            }
        }

        public static double ConvertDateTimeToUnixTimeStamp(this DateTime time)
        {
            return Math.Round(
                time.ToUniversalTime()
                    .Subtract(unixDateBase)
                    .TotalSeconds);
        }

        public static string GetSizeString(double size)
        {
            int index = 0;
            while (index <= 11)
            {
                index++;
                size /= 1024;
                if (size > 0.7 && size < 716.8) { break; }
                else if (size >= 716.8) { continue; }
                else if (size <= 0.7)
                {
                    size *= 1024;
                    index--;
                    break;
                }
            }
            string str = string.Empty;
            switch (index)
            {
                case 0: str = "B"; break;
                case 1: str = "KB"; break;
                case 2: str = "MB"; break;
                case 3: str = "GB"; break;
                case 4: str = "TB"; break;
                case 5: str = "PB"; break;
                case 6: str = "EB"; break;
                case 7: str = "ZB"; break;
                case 8: str = "YB"; break;
                case 9: str = "BB"; break;
                case 10: str = "NB"; break;
                case 11: str = "DB"; break;
                default:
                    break;
            }
            return $"{size:N2}{str}";
        }

        public static string GetNumString(double num)
        {
            string str = string.Empty;
            if (num < 1000) { }
            else if (num < 10000)
            {
                str = "k";
                num /= 1000;
            }
            else if (num < 10000000)
            {
                str = "w";
                num /= 10000;
            }
            else
            {
                str = "kw";
                num /= 10000000;
            }
            return $"{num:N2}{str}";
        }

        public static string CSStoMarkDown(string text)
        {
            try
            {
                Converter converter = new Converter();
                return converter.Convert(text);
            }
            catch
            {
                Regex h1 = new Regex(@"<h1.*?>", RegexOptions.IgnoreCase);
                Regex h2 = new Regex(@"<h2.*?>", RegexOptions.IgnoreCase);
                Regex h3 = new Regex(@"<h3.*?>", RegexOptions.IgnoreCase);
                Regex h4 = new Regex(@"<h4.*?>\n", RegexOptions.IgnoreCase);
                Regex div = new Regex(@"<div.*?>", RegexOptions.IgnoreCase);
                Regex p = new Regex(@"<p.*?>", RegexOptions.IgnoreCase);
                Regex ul = new Regex(@"<ul.*?>", RegexOptions.IgnoreCase);
                Regex li = new Regex(@"<li.*?>", RegexOptions.IgnoreCase);
                Regex span = new Regex(@"<span.*?>", RegexOptions.IgnoreCase);

                text = text.Replace("</h1>", "");
                text = text.Replace("</h2>", "");
                text = text.Replace("</h3>", "");
                text = text.Replace("</h4>", "");
                text = text.Replace("</div>", "");
                text = text.Replace("<p>", "");
                text = text.Replace("</p>", "");
                text = text.Replace("</ul>", "");
                text = text.Replace("</li>", "");
                text = text.Replace("</span>", "**");
                text = text.Replace("</strong>", "**");

                text = h1.Replace(text, "#");
                text = h2.Replace(text, "##");
                text = h3.Replace(text, "###");
                text = h4.Replace(text, "####");
                text = text.Replace("<br/>", "  \n");
                text = text.Replace("<br />", "  \n");
                text = div.Replace(text, "");
                text = p.Replace(text, "");
                text = ul.Replace(text, "");
                text = li.Replace(text, " - ");
                text = span.Replace(text, "**");
                text = text.Replace("<strong>", "**");

                for (int i = 0; i < 20; i++) { text = text.Replace("(" + i.ToString() + ") ", " 1. "); }

                return text;
            }
        }

        public static string CSStoString(string str)
        {
            //换行和段落
            string s = str.Replace("<br>", "\n").Replace("<br>", "\n").Replace("<br/>", "\n").Replace("<br/>", "\n").Replace("<p>", "").Replace("</p>", "\n").Replace("&nbsp;", " ").Replace("<br />", "").Replace("<br />", "");
            //链接彻底删除！
            while (s.IndexOf("<a", StringComparison.Ordinal) > 0)
            {
                s = s.Replace(@"<a href=""" + Regex.Split(Regex.Split(s, @"<a href=""")[1], @""">")[0] + @""">", "");
                s = s.Replace("</a>", "");
            }
            return s;
        }
    }

    public static partial class Utils
    {
        private static readonly Dictionary<Uri, (DateTime, string)> responseCache = new Dictionary<Uri, (DateTime, string)>();

        private static readonly object locker = new object();

        private static readonly Timer cleanCacheTimer = new Timer(o =>
        {
            lock (locker)
            {
                DateTime now = DateTime.Now;
                Uri[] needDelete = (from i in responseCache
                                    where (now - i.Value.Item1).TotalMinutes > 2
                                    select i.Key).ToArray();
                foreach (Uri item in needDelete)
                {
                    _ = responseCache.Remove(item);
                }
            }
        }, null, TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(2));

        private static (bool, JToken) GetResult(string json)
        {
            JObject o = JObject.Parse(json);
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

        public static async Task<(bool isSucceed, string result)> PostHtmlAsync(Uri uri, System.Net.Http.HttpContent content, IEnumerable<(string, string)> cookies)
        {
            string json = await NetworkHelper.PostAsync(uri, content, cookies);
            if (string.IsNullOrEmpty(json))
            {
                ShowInAppMessage(MessageType.Message, "获取失败");
                return (false, null);
            }
            else { return (true, json); }
        }

        public static async Task<(bool isSucceed, JToken result)> PostDataAsync(Uri uri, System.Net.Http.HttpContent content, IEnumerable<(string, string)> cookies)
        {
            string json = await NetworkHelper.PostAsync(uri, content, cookies);
            return GetResult(json);
        }

        public static async Task<(bool isSucceed, string result)> GetHtmlAsync(Uri uri, IEnumerable<(string, string)> cookies, string request)
        {
            string json = await NetworkHelper.GetHtmlAsync(uri, cookies, request);
            if (string.IsNullOrEmpty(json))
            {
                ShowInAppMessage(MessageType.Message, "获取失败");
                return (false, null);
            }
            else { return (true, json); }
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

                            KeyValuePair<Uri, (DateTime, string)>[] needDelete = (from item in responseCache
                                                                                  where item.Key != uri
                                                                                  where item.Key.PathAndQuery.IndexOf(u, StringComparison.Ordinal) == 0
                                                                                  select item).ToArray();
                            foreach (KeyValuePair<Uri, (DateTime, string)> item in needDelete)
                            {
                                _ = responseCache.Remove(item.Key);
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