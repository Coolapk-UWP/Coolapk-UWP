using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace CoolapkUWP.New.Core.Services
{
    // This class provides a wrapper around common functionality for HTTP actions.
    // Learn more at https://docs.microsoft.com/windows/uwp/networking/httpclient
    public static class HttpDataService
    {
        private static readonly Dictionary<string, object> responseCache;
        private static readonly HttpClient client;

        static HttpDataService()
        {
            client = new HttpClient();
            client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
            client.DefaultRequestHeaders.Add("X-Sdk-Int", "28");
            client.DefaultRequestHeaders.Add("X-Sdk-Locale", "zh-CN");
            client.DefaultRequestHeaders.Add("X-App-Id", "com.coolapk.market");
            client.DefaultRequestHeaders.Add("X-App-Version", "9.2.2");
            client.DefaultRequestHeaders.Add("X-App-Code", "1905301");
            client.DefaultRequestHeaders.Add("X-Api-Version", "9");
            string s = Guid.NewGuid().ToString();
            client.DefaultRequestHeaders.Add("X-App-Device", Helpers.DataHelper.GetMD5(s + s + s) + "ady6r8"); //随便弄的

            responseCache = new Dictionary<string, object>();
        }

        private static string GetCoolapkAppToken()
        {
            string DEVICE_ID = Guid.NewGuid().ToString();
            long UnixDate = (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000;
            string t = UnixDate.ToString();
            string hex_t = "0x" + string.Format("{0:x}", UnixDate);
            // 时间戳加密
            string md5_t = Helpers.DataHelper.GetMD5(t);
            string a = "token://com.coolapk.market/c67ef5943784d09750dcfbb31020f0ab?" + md5_t + "$" + DEVICE_ID + "&com.coolapk.market";
            string md5_a = Helpers.DataHelper.GetMD5(Convert.ToBase64String(Encoding.UTF8.GetBytes(a)));
            string token = md5_a + DEVICE_ID + hex_t;
            return token;
        }

        public static async Task<string> GetUidByNameAsync(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new Exception("Coolapk message:\n请输入用户名");
            }
            string str = string.Empty;
            try
            {
                RemoveAuthorizationHeader();
                str = await client.GetStringAsync(new Uri("https://www.coolapk.com/n/" + name));
                return Newtonsoft.Json.Linq.JObject.Parse(str)["dataRow"]["uid"].ToString();
            }
            catch
            {
                var o = Newtonsoft.Json.Linq.JObject.Parse(str);
                if (o != null)
                {
                    throw new Exception($"Coolapk message:\n{o["message"]}");
                }
                else throw;
            }
        }

        public static async Task<T> GetAsync<T>(Helpers.DataUriType dataUriType, string accessToken, bool forceRefresh = false, params object[] args)
        {
            T result = default;
            var uri = "https://api.coolapk.com/v6" + string.Format(Helpers.DataHelper.GetUriStringTemplate(dataUriType), args);

            // The responseCache is a simple store of past responses to avoid unnecessary requests for the same resource.
            // Feel free to remove it or extend this request logic as appropraite for your app.
            if (forceRefresh || !responseCache.ContainsKey(uri))
            {
                RemoveAuthorizationHeader();
                AddAuthorizationHeader(accessToken);
                var json = await client.GetStringAsync(uri);
                result = await Task.Run(() => JsonConvert.DeserializeObject<T>(json, new JsonSerializerSettings
                {
                    Error = (_, e) =>
                    {
                        if (!string.IsNullOrEmpty(json))
                        {
                            var o = JsonConvert.DeserializeObject<Models.MessageModel.Rootobject>(json, new JsonSerializerSettings { Error = (__, ee) => ee.ErrorContext.Handled = true });
                            if (o != null)
                                throw new Exception($"Coolapk message:\n{o.message}");
                        }
                        e.ErrorContext.Handled = true;
                    }
                }));
                if (responseCache.ContainsKey(uri))
                {
                    responseCache[uri] = result;
                }
                else
                {
                    responseCache.Add(uri, result);
                }
            }
            else
            {
                result = (T)responseCache[uri];
            }

            return result;
        }

        public static async Task<bool> PostAsync<T>(string uri, T item)
        {
            if (item == null)
            {
                return false;
            }

            var serializedItem = JsonConvert.SerializeObject(item);
            var buffer = Encoding.UTF8.GetBytes(serializedItem);
            var byteContent = new ByteArrayContent(buffer);

            var response = await client.PostAsync(uri, byteContent);

            return response.IsSuccessStatusCode;
        }

        public static async Task<bool> PostAsJsonAsync<T>(string uri, T item)
        {
            if (item == null)
            {
                return false;
            }

            var serializedItem = JsonConvert.SerializeObject(item);

            var response = await client.PostAsync(uri, new StringContent(serializedItem, Encoding.UTF8, "application/json"));

            return response.IsSuccessStatusCode;
        }

        public static async Task<bool> PutAsync<T>(string uri, T item)
        {
            if (item == null)
            {
                return false;
            }

            var serializedItem = JsonConvert.SerializeObject(item);
            var buffer = Encoding.UTF8.GetBytes(serializedItem);
            var byteContent = new ByteArrayContent(buffer);

            var response = await client.PutAsync(uri, byteContent);

            return response.IsSuccessStatusCode;
        }

        public static async Task<bool> PutAsJsonAsync<T>(string uri, T item)
        {
            if (item == null)
            {
                return false;
            }

            var serializedItem = JsonConvert.SerializeObject(item);

            var response = await client.PutAsync(uri, new StringContent(serializedItem, Encoding.UTF8, "application/json"));

            return response.IsSuccessStatusCode;
        }

        public static async Task<bool> DeleteAsync(string uri)
        {
            var response = await client.DeleteAsync(uri);

            return response.IsSuccessStatusCode;
        }

        // Add this to all public methods
        private static void AddAuthorizationHeader(string token)
        {
            client.DefaultRequestHeaders.Add("X-App-Token", GetCoolapkAppToken());
            client.DefaultRequestHeaders.Add("Cookie", token);
        }

        private static void RemoveAuthorizationHeader()
        {
            client.DefaultRequestHeaders.Remove("X-App-Token");
            client.DefaultRequestHeaders.Remove("Cookie");
        }
    }
}
