using CoolapkUWP.Core.Exceptions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.Storage;

namespace CoolapkUWP.Core.Helpers
{
    [DebuggerStepThrough]

    public static class NetworkHelper
    {
        private static readonly HttpClientHandler clientHandler = new HttpClientHandler();
        private static readonly HttpClient client = new HttpClient(clientHandler);
        private static readonly string guid = Guid.NewGuid().ToString();

        static NetworkHelper()
        {
            string Version = "V11";
            EasClientDeviceInformation deviceInfo = new EasClientDeviceInformation();
            //client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
            client.DefaultRequestHeaders.Add("X-Sdk-Int", "30");
            client.DefaultRequestHeaders.Add("X-Sdk-Locale", "zh-CN");
            client.DefaultRequestHeaders.Add("X-App-Id", "com.coolapk.market");
            if (ApplicationData.Current.LocalSettings.Values["Version"] != null)
                Version = ApplicationData.Current.LocalSettings.Values["Version"].ToString();
            switch (Version)
            {
                case "V6":
                    client.DefaultRequestHeaders.Add("X-App-Version", "6.10.6");
                    client.DefaultRequestHeaders.Add("X-App-Code", "1608291");
                    break;
                case "V7":
                    client.DefaultRequestHeaders.Add("X-App-Version", "7.9.6_S");
                    client.DefaultRequestHeaders.Add("X-App-Code", "1710201");
                    client.DefaultRequestHeaders.Add("X-Api-Version", "7");
                    break;
                case "V8":
                    client.DefaultRequestHeaders.Add("X-App-Version", "8.4.1");
                    client.DefaultRequestHeaders.Add("X-App-Code", "1806141");
                    client.DefaultRequestHeaders.Add("X-Api-Version", "8");
                    break;
                case "V9":
                    client.DefaultRequestHeaders.Add("X-App-Version", "9.2.2");
                    client.DefaultRequestHeaders.Add("X-App-Code", "1905301");
                    client.DefaultRequestHeaders.Add("X-Api-Version", "9");
                    break;
                case "V10":
                    client.DefaultRequestHeaders.Add("X-App-Version", "10.5.3");
                    client.DefaultRequestHeaders.Add("X-App-Code", "2009271");
                    client.DefaultRequestHeaders.Add("X-Api-Version", "10");
                    break;
                case "V11":
                    client.DefaultRequestHeaders.Add("X-App-Version", "11.1.5.1");
                    client.DefaultRequestHeaders.Add("X-App-Code", "2104291");
                    client.DefaultRequestHeaders.Add("X-Api-Version", "11");
                    break;
                default:
                    client.DefaultRequestHeaders.Add("X-App-Version", "9.2.2");
                    client.DefaultRequestHeaders.Add("X-App-Code", "1905301");
                    client.DefaultRequestHeaders.Add("X-Api-Version", "9");
                    break;
            }
            client.DefaultRequestHeaders.Add("X-App-Device", Utils.GetMD5(guid));
            //client.DefaultRequestHeaders.UserAgent.ParseAdd("Dalvik/2.1.0 (Linux; U; Android 11; GM1910 Build/RKQ1.201022.002) (#Build; OnePlus; GM1910; GM1910_21_210317; 11) +CoolMarket/11.1.2-2104021-universal");
            client.DefaultRequestHeaders.UserAgent.ParseAdd("(#Build; " + deviceInfo.SystemManufacturer + "; " + deviceInfo.SystemProductName + "; ; " + "10.0) +CoolMarket/11.1.2-2104021-universal");
        }

        private static string GetCoolapkAppToken()
        {
            var t = Utils.ConvertDateTimeToUnixTimeStamp(DateTime.Now);
            var hex_t = "0x" + Convert.ToString((int)t, 16);
            // 时间戳加密
            var md5_t = Utils.GetMD5($"{t}");
            var a = $"token://com.coolapk.market/c67ef5943784d09750dcfbb31020f0ab?{md5_t}${guid}&com.coolapk.market";
            var md5_a = Utils.GetMD5(Convert.ToBase64String(Encoding.UTF8.GetBytes(a)));
            var token = md5_a + guid + hex_t;
            return token;
        }

        private static void ReplaceAppToken(this System.Net.Http.Headers.HttpRequestHeaders headers)
        {
            const string name = "X-App-Token";
            headers.Remove(name);
            headers.Add(name, GetCoolapkAppToken());
        }

        private static void ReplaceRequested(this System.Net.Http.Headers.HttpRequestHeaders headers, string request)
        {
            const string name = "X-Requested-With";
            headers.Remove(name);
            if (request != null)
                headers.Add(name, request);
        }

        private static void ReplaceCoolapkCookie(this CookieContainer container, IEnumerable<(string name, string value)> cookies)
        {
            if (cookies == null) { return; }

            var c = container.GetCookies(UriHelper.CoolapkUri);
            foreach (var item in cookies)
            {
                container.SetCookies(UriHelper.BaseUri, $"{item.name}={item.value}");
            }
        }

        private static void BeforeGetOrPost(IEnumerable<(string name, string value)> coolapkCookies, string request)
        {
            clientHandler.CookieContainer.ReplaceCoolapkCookie(coolapkCookies);
            client.DefaultRequestHeaders.ReplaceAppToken();
            client.DefaultRequestHeaders.ReplaceRequested(request);
        }

        public static async Task<string> PostAsync(Uri uri, HttpContent content, IEnumerable<(string name, string value)> coolapkCookies)
        {
            try
            {
                BeforeGetOrPost(coolapkCookies, "XMLHttpRequest");
                var response = await client.PostAsync(uri, content);
                return await response.Content.ReadAsStringAsync();
            }
            catch { throw; }
        }

        public static async Task<Stream> GetStreamAsync(Uri uri, IEnumerable<(string name, string value)> coolapkCookies)
        {
            try
            {
                BeforeGetOrPost(coolapkCookies, "XMLHttpRequest");
                return await client.GetStreamAsync(uri);
            }
            catch { throw; }
        }

        public static async Task<string> GetSrtingAsync(Uri uri, IEnumerable<(string name, string value)> coolapkCookies)
        {
            try
            {
                BeforeGetOrPost(coolapkCookies, "XMLHttpRequest");
                return await client.GetStringAsync(uri);
            }
            catch { throw; }
        }

        public static async Task<string> GetHtmlAsync(Uri uri, IEnumerable<(string name, string value)> coolapkCookies , string request)
        {
            try
            {
                BeforeGetOrPost(coolapkCookies, request);

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

            var str = string.Empty;
            try
            {
                str = await client.GetStringAsync(new Uri("https://www.coolapk.com/n/" + name));
                return $"{JObject.Parse(str)["dataRow"].Value<int>("uid")}";
            }
            catch
            {
                var o = JObject.Parse(str);
                if (o == null) { throw; }
                else
                {
                    throw new CoolapkMessageException(o);
                }
            }
        }
    }
}