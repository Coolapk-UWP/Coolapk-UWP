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
            client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
            client.DefaultRequestHeaders.Add("X-Sdk-Int", "28");
            client.DefaultRequestHeaders.Add("X-Sdk-Locale", "zh-CN");
            client.DefaultRequestHeaders.Add("X-App-Id", "com.coolapk.market");
            client.DefaultRequestHeaders.Add("X-App-Version", "9.2.2");
            client.DefaultRequestHeaders.Add("X-App-Code", "1905301");
            client.DefaultRequestHeaders.Add("X-Api-Version", "9");
            client.DefaultRequestHeaders.Add("X-App-Device", Utils.GetMD5(guid));
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

        private static void ReplaceCoolapkCookie(this CookieContainer container, IEnumerable<(string name, string value)> cookies)
        {
            if (cookies == null) { return; }

            var c = container.GetCookies(UriHelper.CoolapkUri);
            foreach (var item in cookies)
            {
                container.SetCookies(UriHelper.BaseUri, $"{item.name}={item.value}");
            }
        }

        private static void BeforeGetOrPost(IEnumerable<(string name, string value)> coolapkCookies)
        {
            clientHandler.CookieContainer.ReplaceCoolapkCookie(coolapkCookies);
            client.DefaultRequestHeaders.ReplaceAppToken();
        }

        public static async Task<string> PostAsync(Uri uri, HttpContent content, IEnumerable<(string name, string value)> coolapkCookies)
        {
            try
            {
                BeforeGetOrPost(coolapkCookies);
                var response = await client.PostAsync(uri, content);
                return await response.Content.ReadAsStringAsync();
            }
            catch { throw; }
        }

        public static async Task<Stream> GetStreamAsync(Uri uri, IEnumerable<(string name, string value)> coolapkCookies)
        {
            try
            {
                BeforeGetOrPost(coolapkCookies);
                return await client.GetStreamAsync(uri);
            }
            catch { throw; }
        }

        public static async Task<string> GetSrtingAsync(Uri uri, IEnumerable<(string name, string value)> coolapkCookies)
        {
            try
            {
                BeforeGetOrPost(coolapkCookies);
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