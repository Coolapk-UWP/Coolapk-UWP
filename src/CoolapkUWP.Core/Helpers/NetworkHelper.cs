using CoolapkUWP.Core.Exceptions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.Storage;
using Windows.System.Profile;
using Windows.System.UserProfile;
using Windows.UI.Xaml;

namespace CoolapkUWP.Core.Helpers
{
    [DebuggerStepThrough]

    public static class NetworkHelper
    {
        public static readonly HttpClientHandler clientHandler = new HttpClientHandler();
        private static readonly HttpClient client = new HttpClient(clientHandler);
        private static readonly string guid = Guid.NewGuid().ToString();

        static NetworkHelper()
        {
            CultureInfo Culture = null;
            string Version = "V11";
            ulong version = ulong.Parse(AnalyticsInfo.VersionInfo.DeviceFamilyVersion);
            try { Culture = GlobalizationPreferences.Languages.Count > 0 ? new CultureInfo(GlobalizationPreferences.Languages.First()) : null; } catch { }
            EasClientDeviceInformation deviceInfo = new EasClientDeviceInformation();
            client.DefaultRequestHeaders.Add("X-Sdk-Int", "30");
            client.DefaultRequestHeaders.Add("X-App-Mode", "universal");
            client.DefaultRequestHeaders.Add("X-App-Channel", "coolapk");
            client.DefaultRequestHeaders.Add("X-App-Id", "com.coolapk.market");
            client.DefaultRequestHeaders.Add("X-Sdk-Locale", Culture == null ? "zh-CN" : Culture.ToString());
            client.DefaultRequestHeaders.Add("X-Dark-Mode", Application.Current.RequestedTheme.ToString() == "Dark" ? "1" : "0");
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Dalvik/2.1.0 (Windows NT " + (ushort)((version & 0xFFFF000000000000L) >> 48) + "." + (ushort)((version & 0x0000FFFF00000000L) >> 32) + (Package.Current.Id.Architecture.ToString().Contains("64") ? "; Win64; " : "; Win32; ") + Package.Current.Id.Architecture.ToString().Replace("X", "x") + "; WebView/3.0) (#Build; " + deviceInfo.SystemManufacturer + "; " + deviceInfo.SystemProductName + "; CoolapkUWP; " + (ushort)((version & 0xFFFF000000000000L) >> 48) + "." + (ushort)((version & 0x0000FFFF00000000L) >> 32) + "." + (ushort)((version & 0x00000000FFFF0000L) >> 16) + "." + (ushort)(version & 0x000000000000FFFFL) + ")");
            if (ApplicationData.Current.LocalSettings.Values["Version"] != null)
            { Version = ApplicationData.Current.LocalSettings.Values["Version"].ToString(); }
            switch (Version)
            {
                case "V6":
                    client.DefaultRequestHeaders.UserAgent.ParseAdd(" +CoolMarket/6.10.6-1608291-universal");
                    client.DefaultRequestHeaders.Add("X-App-Version", "6.10.6");
                    client.DefaultRequestHeaders.Add("X-App-Code", "1608291");
                    break;
                case "V7":
                    client.DefaultRequestHeaders.UserAgent.ParseAdd(" +CoolMarket/7.9.6_S-1710201-universal");
                    client.DefaultRequestHeaders.Add("X-App-Version", "7.9.6_S");
                    client.DefaultRequestHeaders.Add("X-App-Code", "1710201");
                    client.DefaultRequestHeaders.Add("X-Api-Version", "7");
                    break;
                case "V8":
                    client.DefaultRequestHeaders.UserAgent.ParseAdd(" +CoolMarket/8.4.1-1806141-universal");
                    client.DefaultRequestHeaders.Add("X-App-Version", "8.4.1");
                    client.DefaultRequestHeaders.Add("X-App-Code", "1806141");
                    client.DefaultRequestHeaders.Add("X-Api-Version", "8");
                    break;
                case "V9":
                    client.DefaultRequestHeaders.UserAgent.ParseAdd(" +CoolMarket/9.2.2-1905301-universal");
                    client.DefaultRequestHeaders.Add("X-App-Version", "9.2.2");
                    client.DefaultRequestHeaders.Add("X-App-Code", "1905301");
                    client.DefaultRequestHeaders.Add("X-Api-Version", "9");
                    break;
                case "小程序":
                    client.DefaultRequestHeaders.UserAgent.ParseAdd(" +CoolMarket/1.0-1902250-universal");
                    client.DefaultRequestHeaders.Add("X-App-Version", "1.0");
                    client.DefaultRequestHeaders.Add("X-App-Code", "1902250");
                    client.DefaultRequestHeaders.Add("X-Api-Version", "9");
                    break;
                case "V10":
                    client.DefaultRequestHeaders.UserAgent.ParseAdd(" +CoolMarket/10.5.3-2009271-universal");
                    client.DefaultRequestHeaders.Add("X-App-Version", "10.5.3");
                    client.DefaultRequestHeaders.Add("X-App-Code", "2009271");
                    client.DefaultRequestHeaders.Add("X-Api-Version", "10");
                    break;
                case "V11":
                    client.DefaultRequestHeaders.UserAgent.ParseAdd(" +CoolMarket/11.2-2105201-universal");
                    client.DefaultRequestHeaders.Add("X-App-Version", "11.2");
                    client.DefaultRequestHeaders.Add("X-App-Code", "2105201");
                    client.DefaultRequestHeaders.Add("X-Api-Version", "11");
                    break;
                default:
                    client.DefaultRequestHeaders.UserAgent.ParseAdd(" +CoolMarket/9.2.2-1905301-universal");
                    client.DefaultRequestHeaders.Add("X-App-Version", "9.2.2");
                    client.DefaultRequestHeaders.Add("X-App-Code", "1905301");
                    client.DefaultRequestHeaders.Add("X-Api-Version", "9");
                    break;
            }
            client.DefaultRequestHeaders.Add("X-App-Device", GetCoolapkDeviceID());
        }

        private static string GetCoolapkDeviceID()
        {
            Guid easId = new EasClientDeviceInformation().Id;
            string md5_easID = Utils.GetMD5(easId.ToString());
            string base64 = md5_easID;
            for (int i = 0; i < 5; i++)
            {
                base64 = Utils.GetBase64(base64);
            }
            string token = base64.Replace("=", "");
            return token;
        }

        private static string GetCoolapkAppToken()
        {
            double t = Utils.ConvertDateTimeToUnixTimeStamp(DateTime.Now);
            string hex_t = "0x" + Convert.ToString((int)t, 16);
            // 时间戳加密
            string md5_t = Utils.GetMD5($"{t}");
            string a = $"token://com.coolapk.market/c67ef5943784d09750dcfbb31020f0ab?{md5_t}${guid}&com.coolapk.market";
            string md5_a = Utils.GetMD5(Utils.GetBase64(a));
            string token = md5_a + guid + hex_t;
            return token;
        }

        private static void ReplaceAppToken(this System.Net.Http.Headers.HttpRequestHeaders headers)
        {
            const string name = "X-App-Token";
            _ = headers.Remove(name);
            headers.Add(name, GetCoolapkAppToken());
        }

        private static void ReplaceRequested(this System.Net.Http.Headers.HttpRequestHeaders headers, string request)
        {
            const string name = "X-Requested-With";
            _ = headers.Remove(name);
            if (request != null) { headers.Add(name, request); }
        }

        private static void ReplaceCoolapkCookie(this CookieContainer container, IEnumerable<(string name, string value)> cookies, Uri uri)
        {
            if (cookies == null) { return; }

            //var c = container.GetCookies(UriHelper.CoolapkUri);
            foreach ((string name, string value) in cookies)
            {
                container.SetCookies(GetHost(uri), $"{name}={value}");
            }
        }

        public static Uri GetHost(Uri uri)
        {
            return new Uri("https://" + uri.Host);
        }

        private static void BeforeGetOrPost(IEnumerable<(string name, string value)> coolapkCookies, Uri uri, string request)
        {
            clientHandler.CookieContainer.ReplaceCoolapkCookie(coolapkCookies, uri);
            client.DefaultRequestHeaders.ReplaceAppToken();
            client.DefaultRequestHeaders.ReplaceRequested(request);
        }

        public static async Task<string> PostAsync(Uri uri, HttpContent content, IEnumerable<(string name, string value)> coolapkCookies)
        {
            try
            {
                BeforeGetOrPost(coolapkCookies, uri, "XMLHttpRequest");
                _ = client.DefaultRequestHeaders.Remove("X-App-Device");
                HttpResponseMessage response = await client.PostAsync(uri, content);
                client.DefaultRequestHeaders.Add("X-App-Device", GetCoolapkDeviceID());
                return await response.Content.ReadAsStringAsync();
            }
            catch { throw; }
        }

        public static async Task<Stream> GetStreamAsync(Uri uri, IEnumerable<(string name, string value)> coolapkCookies)
        {
            try
            {
                BeforeGetOrPost(coolapkCookies, uri, "XMLHttpRequest");
                return await client.GetStreamAsync(uri);
            }
            catch { throw; }
        }

        public static async Task<string> GetSrtingAsync(Uri uri, IEnumerable<(string name, string value)> coolapkCookies)
        {
            try
            {
                BeforeGetOrPost(coolapkCookies, uri, "XMLHttpRequest");
                return await client.GetStringAsync(uri);
            }
            catch { throw; }
        }

        public static async Task<string> GetHtmlAsync(Uri uri, IEnumerable<(string name, string value)> coolapkCookies, string request)
        {
            try
            {
                BeforeGetOrPost(coolapkCookies, uri, request);
                return await client.GetStringAsync(uri);
            }
            catch { throw; }
        }

        /// <summary> 通过用户名获取UID。 </summary>
        /// <param name="name"> 要获取UID的用户名。 </param>
        public static async Task<string> GetUserIDByNameAsync(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new UserNameErrorException();
            }

            string str = string.Empty;
            try
            {
                str = await client.GetStringAsync(new Uri("https://www.coolapk.com/n/" + name));
                return $"{JObject.Parse(str)["dataRow"].Value<int>("uid")}";
            }
            catch
            {
                JObject o = JObject.Parse(str);
                if (o == null) { throw; }
                else
                {
                    throw new CoolapkMessageException(o);
                }
            }
        }

        public static string ExpandShortUrl(Uri ShortUrl)
        {
            string NativeUrl = null;
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(ShortUrl);
            req.AllowAutoRedirect = false;
            try { _ = (HttpWebResponse)req.GetResponse(); }
            catch (WebException ex)
            {
                HttpWebResponse res = ex.Response as HttpWebResponse;
                if (res.StatusCode == HttpStatusCode.Found)
                { NativeUrl = res.Headers["Location"]; }
            }
            return NativeUrl ?? ShortUrl.ToString();
        }
    }
}