using CoolapkUWP.Pages;
using CoolapkUWP.Pages.FeedPages;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;
using Windows.System;

namespace CoolapkUWP.Data
{
    static class Tools
    {
        public static RootPage rootPage = null;
        public static MainPage mainPage = null;
        static HttpClient mClient;

        static Tools()
        {
            mClient = new HttpClient();
            mClient.DefaultRequestHeaders.UserAgent.ParseAdd(" +CoolMarket/9.2.2-1905301");
            //mClient.DefaultRequestHeaders.Add("User-Agent", "Dalvik/2.1.0 (Linux; U; Android 9; MI 8 SE MIUI/9.5.9) (#Build; Xiaomi; MI 8 SE; PKQ1.181121.001; 9) +CoolMarket/9.2.2-1905301");
            mClient.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
            mClient.DefaultRequestHeaders.Add("X-Sdk-Int", "28");
            mClient.DefaultRequestHeaders.Add("X-Sdk-Locale", "zh-CN");
            mClient.DefaultRequestHeaders.Add("X-App-Id", "com.coolapk.market");
            mClient.DefaultRequestHeaders.Add("X-App-Token", GetCoolapkAppToken());
            mClient.DefaultRequestHeaders.Add("X-App-Version", "9.2.2");
            mClient.DefaultRequestHeaders.Add("X-App-Code", "1905301");
            mClient.DefaultRequestHeaders.Add("X-Api-Version", "9");
            //mClient.DefaultRequestHeaders.Add("X-App-Device", "QRTBCOgkUTgsTat9WYphFI7kWbvFWaYByO1YjOCdjOxAjOxEkOFJjODlDI7ATNxMjM5MTOxcjMwAjN0AyOxEjNwgDNxITM2kDMzcTOgsTZzkTZlJ2MwUDNhJ2MyYzM");
            //mClient.DefaultRequestHeaders.Add("X-Dark-Mode", "0");
            mClient.DefaultRequestHeaders.Add("Host", "api.coolapk.com");
        }

        public static async void OpenLink(string str)
        {
            if (str.IndexOf("/u/") == 0)
            {
                string u = str.Replace("/u/", string.Empty);
                if (u.Contains('?')) u = u.Substring(0, u.IndexOf('?'));
                if (u.Contains('%')) u = u.Substring(0, u.IndexOf('%'));
                if (int.TryParse(u, out int uu))
                    rootPage.Navigate(typeof(UserPage), u);
                else rootPage.Navigate(typeof(UserPage), await GetUserIDByName(u));
            }
            else if (str.IndexOf("/feed/") == 0)
            {
                string u = str.Replace("/feed/", string.Empty);
                if (u.Contains('?')) u = u.Substring(0, u.IndexOf('?'));
                if (u.Contains('%')) u = u.Substring(0, u.IndexOf('%'));
                rootPage.Navigate(typeof(FeedDetailPage), new object[] { u, rootPage, string.Empty, null });
            }
            else if (str.IndexOf("/t/") == 0)
            {
                string u = str.Replace("/t/", string.Empty);
                if (u.Contains('?')) u = u.Substring(0, u.IndexOf('?'));
                if (u.Contains('%')) u = u.Substring(0, u.IndexOf('%'));
                rootPage.Navigate(typeof(TopicPage), u);
            }
            else if (str.IndexOf("/dyh/") == 0)
            {
                string u = str.Replace("/dyh/", string.Empty);
                if (u.Contains('?')) u = u.Substring(0, u.IndexOf('?'));
                if (u.Contains('%')) u = u.Substring(0, u.IndexOf('%'));
                rootPage.Navigate(typeof(DyhPage), u);
            }

            else if (str.IndexOf("https") == 0)
            {
                if (str.Contains("coolapk.com"))
                    OpenLink(str.Replace("https://www.coolapk.com", string.Empty));
                else await Launcher.LaunchUriAsync(new Uri(str));
            }
            else if (str.IndexOf("http") == 0)
            {
                if (str.Contains("coolapk.com"))
                    OpenLink(str.Replace("http://www.coolapk.com", string.Empty));
                else await Launcher.LaunchUriAsync(new Uri(str));
            }
        }

        //来源：https://blog.csdn.net/lindexi_gd/article/details/48951849
        public static string GetMD5(string inputString)
        {
            CryptographicHash objHash = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Md5).CreateHash();
            objHash.Append(CryptographicBuffer.ConvertStringToBinary(inputString, BinaryStringEncoding.Utf8));
            IBuffer buffHash1 = objHash.GetValueAndReset();
            return CryptographicBuffer.EncodeToHexString(buffHash1);
        }

        public static string GetMessageText(string s)
        {
            s = s.Replace("\\", "\\\\");
            s = s.Replace("&#039;", "\'");
            s = s.Replace("&nbsp;", "\\&nbsp;");
            s = s.Replace("<sub>", "\\<sub>");
            s = s.Replace("</a>", "</a> ");
            s = s.Replace(">", "\\>");
            s = s.Replace("#", "\\#");
            s = s.Replace("`", "\\`");
            s = s.Replace("*", "\\*");
            s = s.Replace("(", "\\(");
            s = s.Replace("~", "\\~");
            s = s.Replace("^", "\\^");
            s = s.Replace("[", "\\[");
            s = s.Replace("+", "\\+");
            s = s.Replace("-", "\\-");
            s = s.Replace("\\-\\-\\>", "-->");
            s = s.Replace("!\\-\\-", "!--");
            s = s.Replace("*", "\\*");
            s = s.Replace(".", "\\.");
            s = s.Replace(":", "\\:");
            s = s.Replace("\n", "\n\n");
            foreach (var i in Emojis.emojis)
            {
                if (s.Contains(i))
                {
                    if (i.Contains("\\("))
                        s = s.Replace($"\\#\\({i})", $"\n![#{i})](ms-appx:/Assets/Emoji/{i}.png =24)");
                    else if (Settings.GetBoolen("IsUseOldEmojiMode"))
                        if (Emojis.oldEmojis.Contains(i))
                            s = s.Replace($"\\{i}", $"\n![{i}(ms-appx:/Assets/Emoji/{i}2.png =24)");
                        else s = s.Replace($"\\{i}", $"\n![{i}(ms-appx:/Assets/Emoji/{i}.png =24)");
                    else s = s.Replace($"\\{i}", $"\n![{i}(ms-appx:/Assets/Emoji/{i}.png =24)");
                }
            }
            Regex regex = new Regex("<a.*?\\>\\S*"), regex2 = new Regex("href=\".*"), regex3 = new Regex(">.*<");
            while (regex.IsMatch(s))
            {
                var h = regex.Match(s);
                if (!h.Value.Contains("</a\\>"))
                {
                    s = s.Replace(h.Value + " ", h.Value + "&nbsp;");
                    continue;
                }
                string t = regex3.Match(h.Value).Value.Replace(">", string.Empty);
                t = t.Replace("<", string.Empty);
                string tt = regex2.Match(h.Value).Value.Replace("href=", string.Empty);
                tt = tt.Replace("\"", string.Empty);
                tt = tt.Replace($"\\>{t}</a\\>", string.Empty);
                if (t == "查看更多") tt = "get";
                s = s.Replace(h.Value, $"[{t}]({tt})");
            }
            s = s.Replace(" ", "&nbsp;");
            return s;
        }

        public static string ConvertTime(double timestr)
        {
            DateTime time = new DateTime(1970, 1, 1).ToLocalTime().Add(new TimeSpan(Convert.ToInt64(timestr) * 10000000));
            TimeSpan tt = DateTime.Now.Subtract(time);
            if (tt.TotalDays > 30)
                return $"{time.Year}/{time.Month}/{time.Day}";
            else if (tt.Days > 0)
                return $"{tt.Days}天前";
            else if (tt.Hours > 0)
                return $"{tt.Hours}小时前";
            else if (tt.Minutes > 0)
                return $"{tt.Minutes}分钟前";
            else return "刚刚";
        }

        //https://github.com/ZCKun/CoolapkTokenCrack
        static string GetCoolapkAppToken()
        {
            string DEVICE_ID = Guid.NewGuid().ToString();
            long UnixDate = (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000;
            string t = UnixDate.ToString();
            string hex_t = "0x" + string.Format("{0:x}", UnixDate);
            // 时间戳加密
            string md5_t = GetMD5(t);
            string a = "token://com.coolapk.market/c67ef5943784d09750dcfbb31020f0ab?" + md5_t + "$" + DEVICE_ID + "&com.coolapk.market";
            string md5_a = GetMD5(Convert.ToBase64String(Encoding.UTF8.GetBytes(a)));
            string token = md5_a + DEVICE_ID + hex_t;
            return token;
        }


        public static async Task<string> GetJson(string url)
        {
            try
            {
                return await mClient.GetStringAsync(new Uri("https://api.coolapk.com/v6" + url));
            }
            catch (HttpRequestException e)
            {
                rootPage.ShowHttpExceptionMessage(e);
                return string.Empty;
            }
            catch { throw; }
        }

        public static JsonObject GetJSonObject(string json)
        {
            try
            {
                if (string.IsNullOrEmpty(json)) return null;
                return JsonObject.Parse(json)["data"].GetObject();
            }
            catch
            {
                if (JsonObject.Parse(json).TryGetValue("message", out IJsonValue value))
                    rootPage.ShowMessage($"{value.GetString()}");
                return null;
            }
        }

        public static JsonArray GetDataArray(string json)
        {
            try
            {
                if (string.IsNullOrEmpty(json)) return null;
                return JsonObject.Parse(json)["data"].GetArray();
            }
            catch
            {
                if (JsonObject.Parse(json).TryGetValue("message", out IJsonValue value))
                    rootPage.ShowMessage($"{value.GetString()}");
                return null;
            }
        }

        public static string ReplaceHtml(string str)
        {
            //换行和段落
            string s = str.Replace("<br>", "\n").Replace("<br>", "\n").Replace("<br/>", "\n").Replace("<br />", "\n").Replace("<p>", "").Replace("</p>", "\n").Replace("&nbsp;", " ");
            //链接彻底删除！
            while (s.IndexOf("<a") > 0)
            {
                s = s.Replace(@"<a href=""" + Regex.Split(Regex.Split(s, @"<a href=""")[1], @""">")[0] + @""">", "");
                s = s.Replace("</a>", "");
            }
            return s;
        }

        public static async Task<string> GetUserIDByName(string name)
        {
            try
            {
                string uid = await new HttpClient().GetStringAsync(new Uri($"https://www.coolapk.com/n/{name}"));
                uid = uid.Split(new string[] { "coolmarket://www.coolapk.com/u/" }, StringSplitOptions.RemoveEmptyEntries)[1];
                uid = uid.Split(new string[] { @"""" }, StringSplitOptions.RemoveEmptyEntries)[0];
                return uid;
            }
            catch (HttpRequestException e)
            {
                //if (e.Message.Contains("404")) rootPage.ShowMessage("用户不存在");
                //else
                rootPage.ShowHttpExceptionMessage(e);
                return "0";
            }
            catch { throw; }
        }

        public static async Task<JsonObject> GetFeedDetailById(string feedId)
        {
            string result = await GetJson("/feed/detail?id=" + feedId);
            JsonObject jo = JsonObject.Parse(result);
            return jo["data"].GetObject();
        }
        public static async Task<JsonArray> GetAnswerListById(string feedId, string sortType, string page, string firstItem, string lastItem)
        {
            string result = await GetJson($"/question/answerList?id={feedId}&sort={sortType}&page={page}&firstItem={firstItem}&lastItem={lastItem}");
            JsonObject jo = JsonObject.Parse(result);
            return jo["data"].GetArray();
        }
        //回复
        public static async Task<JsonArray> GetFeedReplyListById(string feedId, string listType, string page, string fromFeedAuthor, string firstItem, string lastItem)
        {
            string result = await GetJson($"/feed/replyList?id={feedId}&listType={listType}&page={page}&firstItem={firstItem}&lastItem={lastItem}&discussMode=1&feedType=feed&blockStatus=0&fromFeedAuthor={fromFeedAuthor}");
            JsonObject jo = JsonObject.Parse(result);
            return jo["data"].GetArray();
        }
        //回复的回复
        public static async Task<JsonArray> GetReplyListById(string feedId, string page, string lastItem)
        {
            string result = await GetJson($"/feed/replyList?id={feedId}&listType=&page={page}&lastItem={lastItem}&discussMode=0&feedType=feed_reply&blockStatus=0&fromFeedAuthor=0");
            JsonObject jo = JsonObject.Parse(result);
            return jo["data"].GetArray();
        }

        public static async Task<JsonArray> GetFeedLikeUsersListById(string feedId, string page, string firstItem, string lastItem)
        {
            string result = await GetJson($"/feed/likeList?id={feedId}&listType=lastupdate_desc&page={page}&firstItem={firstItem}&lastItem={lastItem}");
            JsonObject jo = JsonObject.Parse(result);
            return jo["data"].GetArray();
        }
        public static async Task<JsonArray> GetForwardListById(string feedId, string page)
        {
            string result = await GetJson($"/feed/forwardList?id={feedId}&type=feed&page={page}");
            JsonObject jo = JsonObject.Parse(result);
            return jo["data"].GetArray();
        }
    }
    /*
    public static async Task<string> GetCoolApkUserFaceUri(string NameOrID)
    {
        String body = await new HttpClient().GetStringAsync("https://www.coolapk.com/u/" + NameOrID);
        body = Regex.Split(body, @"<div class=""msg_box"">")[1];
        body = Regex.Split(body, @"src=""")[1];
        return Regex.Split(body, @"""")[0];
    }
    */
}