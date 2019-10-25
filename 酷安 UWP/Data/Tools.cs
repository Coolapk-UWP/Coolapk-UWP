using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System;
using Windows.Web.Http;
using 酷安_UWP.Data;
using 酷安_UWP.FeedsPage;

namespace 酷安_UWP
{
    static class Tools
    {
        public static async void OpenLink(string str, MainPage mainPage)
        {
            if (str.IndexOf("/u/") == 0)
                mainPage.Frame.Navigate(typeof(UserPage), new object[] { await Tools.GetUserIDByName(str.Replace("/u/", string.Empty)), mainPage });
            else if (str.IndexOf("/feed/") == 0) mainPage.Frame.Navigate(typeof(FeedDetailPage), new object[] { str.Replace("/feed/", string.Empty), mainPage, string.Empty, null });
            else if (str.IndexOf("/t/") == 0)
            {
                string u = str.Replace("/t/", string.Empty);
                if (u.Contains('?')) u = u.Substring(0, u.IndexOf('?'));
                if (u.Contains('%')) u = u.Substring(0, u.IndexOf('%'));
                mainPage.Frame.Navigate(typeof(TopicPage), new object[] { u, mainPage });
            }
            else if (str.IndexOf("/dyh/") == 0)
            {
                string u = str.Replace("/dyh/", string.Empty);
                if (u.Contains('?')) u = u.Substring(0, u.IndexOf('?'));
                if (u.Contains('%')) u = u.Substring(0, u.IndexOf('%'));
                mainPage.Frame.Navigate(typeof(DyhPage), new object[] { u, mainPage });
            }

            else if (str.IndexOf("https") == 0)
            {
                if (str.Contains("coolapk.com"))
                    OpenLink(str.Replace("https://www.coolapk.com", string.Empty), mainPage);
                else await Launcher.LaunchUriAsync(new Uri(str));
            }
            else if (str.IndexOf("http") == 0)
            {
                if (str.Contains("coolapk.com"))
                    OpenLink(str.Replace("http://www.coolapk.com", string.Empty), mainPage);
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

        public static string ProcessMessage(string s, ApplicationDataContainer localSettings)
        {
            s = s.Replace("\n", "\n\n");
            s = s.Replace("&#039;", "\'");
            s = s.Replace("</a>", "</a> ");
            foreach (var i in Emojis.emojis)
            {
                if (s.Contains(i))
                {
                    if (i.Contains('('))
                        s = s.Replace($"#{i})", $"\n![#{i})](ms-appx:/Emoji/{i}.png =24)");
                    else if (Convert.ToBoolean(localSettings.Values["IsUseOldEmojiMode"]))
                        if (Emojis.oldEmojis.Contains(s))
                            s = s.Replace(i, $"\n![{i}(ms-appx:/Emoji/{i}2.png =24)");
                        else s = s.Replace(i, $"\n![{i}(ms-appx:/Emoji/{i}.png =24)");
                    else s = s.Replace(i, $"\n![{i}(ms-appx:/Emoji/{i}.png =24)");
                }
            }
            Regex regex = new Regex("<a.*?>\\S*"), regex2 = new Regex("href=\".*"), regex3 = new Regex(">.*<");
            while (regex.IsMatch(s))
            {
                var h = regex.Match(s);
                if (!h.Value.Contains("</a>"))
                {
                    s = s.Replace(h.Value + " ", h.Value);
                    continue;
                }
                string t = regex3.Match(h.Value).Value.Replace(">", string.Empty);
                t = t.Replace("<", string.Empty);
                string tt = regex2.Match(h.Value).Value.Replace("href=", string.Empty);
                tt = tt.Replace("\"", string.Empty);
                tt = tt.Replace($">{t}</a>", string.Empty);
                if (t == "查看更多") tt = "getmore";
                s = s.Replace(h.Value, $"[{t}]({tt})");
            }
            return s;
        }

        public static string ConvertTime(string timestr)
        {
            DateTime time = new DateTime(1970, 1, 1).ToLocalTime().Add(new TimeSpan(Convert.ToInt64(timestr + "0000000")));
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

        //超级感谢！！！👉 https://github.com/ZCKun/CoolapkTokenCrack
        static string GetAppToken()
        {
            string DEVICE_ID = Guid.NewGuid().ToString();
            long UnixDate = (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000;
            string t = UnixDate.ToString();
            string hex_t = "0x" + string.Format("{0:x}", UnixDate);
            // 时间戳加密
            string md5_t = Tools.GetMD5(t);
            string a = "token://com.coolapk.market/c67ef5943784d09750dcfbb31020f0ab?" + md5_t + "$" + DEVICE_ID + "&com.coolapk.market";
            string md5_a = Tools.GetMD5(Convert.ToBase64String(Encoding.UTF8.GetBytes(a)));
            string token = md5_a + DEVICE_ID + hex_t;
            return token;
        }

        public static async Task<string> GetCoolApkMessage(string url)
        {
            //这里感谢https://github.com/bjzhou/Coolapk-kotlin提供的 HTTP 头！！！！！！！！！！！！！

            try
            {
                var mClient = new HttpClient();

                mClient.DefaultRequestHeaders.UserAgent.ParseAdd(" +CoolMarket/9.2.2-1905301");
                //mClient.DefaultRequestHeaders.Add("User-Agent", "Dalvik/2.1.0 (Linux; U; Android 9; MI 8 SE MIUI/9.5.9) (#Build; Xiaomi; MI 8 SE; PKQ1.181121.001; 9) +CoolMarket/9.2.2-1905301");
                mClient.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
                mClient.DefaultRequestHeaders.Add("X-Sdk-Int", "28");
                mClient.DefaultRequestHeaders.Add("X-Sdk-Locale", "zh-CN");
                mClient.DefaultRequestHeaders.Add("X-App-Id", "com.coolapk.market");
                mClient.DefaultRequestHeaders.Add("X-App-Token", GetAppToken());
                mClient.DefaultRequestHeaders.Add("X-App-Version", "9.2.2");
                mClient.DefaultRequestHeaders.Add("X-App-Code", "1905301");
                mClient.DefaultRequestHeaders.Add("X-Api-Version", "9");
                //mClient.DefaultRequestHeaders.Add("X-App-Device", "QRTBCOgkUTgsTat9WYphFI7kWbvFWaYByO1YjOCdjOxAjOxEkOFJjODlDI7ATNxMjM5MTOxcjMwAjN0AyOxEjNwgDNxITM2kDMzcTOgsTZzkTZlJ2MwUDNhJ2MyYzM");
                //mClient.DefaultRequestHeaders.Add("X-Dark-Mode", "0");
                mClient.DefaultRequestHeaders.Add("Host", "api.coolapk.com");
                return await mClient.GetStringAsync(new Uri("https://api.coolapk.com/v6" + url));
            }
            catch
            {
                throw;
            }
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
            catch
            {
                throw;
            }
        }


        public static async Task<JObject> GetUserProfileByID(string uid)
        {
            string result = await GetCoolApkMessage("/user/space?uid=" + uid);
            return (JObject)((JObject)JsonConvert.DeserializeObject(result))["data"];
        }

        /**
            * 根據用戶名獲得用戶信息，失敗返回null
            *
            * @param name 用戶名
            * @return 用戶信息
            *
        public static async Task<JObject> GetUserProfileByName(string name)
        {
            try
            {
                string uid = await GetUserIDByName(name);
                return await GetUserProfileByID(uid);
            }
            catch (Exception)
            {
                return null;
            }
        }*/

        public static async Task<JArray> GetFeedListByID(string uid, string page, string firstItem, string lastItem)
        {
            try
            {
                string str = await GetCoolApkMessage($"/user/feedList?uid={uid}&page={page}&firstItem={firstItem}&lastItem={lastItem}");
                JObject jo = (JObject)JsonConvert.DeserializeObject(str);
                return jo.HasValues ? (JArray)jo["data"] : new JArray();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static async Task<JArray> GetIndexList(string page)
        {
            try
            {
                string str = await GetCoolApkMessage("/main/indexV8?page=" + page);
                JObject jo = (JObject)JsonConvert.DeserializeObject(str);
                return (JArray)jo["data"];
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static async Task<JObject> GetFeedDetailById(string feedId)
        {
            try
            {
                string result = await GetCoolApkMessage("/feed/detail?id=" + feedId);
                JObject jo = (JObject)JsonConvert.DeserializeObject(result);
                return (JObject)jo["data"];
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static async Task<JArray> GetAnswerListById(string feedId, string sortType, string page, string firstItem, string lastItem)
        {
            try
            {
                string result = await GetCoolApkMessage($"/question/answerList?id={feedId}&sort={sortType}&page={page}&firstItem={firstItem}&lastItem={lastItem}");
                JObject jo = (JObject)JsonConvert.DeserializeObject(result);
                return (JArray)jo["data"];
            }
            catch (Exception)
            {
                throw;
            }
        }
        //回复
        public static async Task<JArray> GetFeedReplyListById(string feedId, string listType, string page, string fromFeedAuthor, string firstItem, string lastItem)
        {
            try
            {
                string result = await GetCoolApkMessage($"/feed/replyList?id={feedId}&listType={listType}&page={page}&firstItem={firstItem}&lastItem={lastItem}&discussMode=1&feedType=feed&blockStatus=0&fromFeedAuthor={fromFeedAuthor}");
                JObject jo = (JObject)JsonConvert.DeserializeObject(result);
                return (JArray)jo["data"];
            }
            catch (Exception)
            {
                throw;
            }
        }
        //回复的回复
        public static async Task<JArray> GetReplyListById(string feedId, string page, string lastItem)
        {
            try
            {
                string result = await GetCoolApkMessage($"/feed/replyList?id={feedId}&listType=&page={page}&lastItem={lastItem}&discussMode=0&feedType=feed_reply&blockStatus=0&fromFeedAuthor=0");
                JObject jo = (JObject)JsonConvert.DeserializeObject(result);
                return (JArray)jo["data"];
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static async Task<JArray> GetFeedLikeUsersListById(string feedId, string page, string firstItem, string lastItem)
        {
            try
            {
                string result = await GetCoolApkMessage($"/feed/likeList?id={feedId}&listType=lastupdate_desc&page={page}&firstItem={firstItem}&lastItem={lastItem}");
                JObject jo = (JObject)JsonConvert.DeserializeObject(result);
                return (JArray)jo["data"];
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static async Task<JArray> GetForwardListById(string feedId, string page)
        {
            try
            {
                string result = await GetCoolApkMessage($"/feed/forwardList?id={feedId}&type=feed&page={page}");
                JObject jo = (JObject)JsonConvert.DeserializeObject(result);
                return (JArray)jo["data"];
            }
            catch (Exception)
            {
                throw;
            }
        }

        /*
        public static async Task<string> GetCoolApkUserFaceUri(string NameOrID)
        {
            String body = await Web.GetHttp("https://www.coolapk.com/u/" + NameOrID);
            body = Regex.Split(body, @"<div class=""msg_box"">")[1];
            body = Regex.Split(body, @"src=""")[1];
            return Regex.Split(body, @"""")[0];
        }
        */
    }
}