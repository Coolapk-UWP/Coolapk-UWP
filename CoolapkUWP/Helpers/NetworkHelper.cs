using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Web.Http;

namespace CoolapkUWP.Helpers
{
    /// <summary> 提供与网络相关的方法。 </summary>
    internal static class NetworkHelper
    {
        private static readonly HttpClient mClient = new HttpClient();
        private static readonly string guid = Guid.NewGuid().ToString();

        static NetworkHelper()
        {
            mClient.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
            mClient.DefaultRequestHeaders.Add("X-Sdk-Int", "28");
            mClient.DefaultRequestHeaders.Add("X-Sdk-Locale", "zh-CN");
            mClient.DefaultRequestHeaders.Add("X-App-Id", "com.coolapk.market");
            mClient.DefaultRequestHeaders.Add("X-App-Version", "9.2.2");
            mClient.DefaultRequestHeaders.Add("X-App-Code", "1905301");
            mClient.DefaultRequestHeaders.Add("X-Api-Version", "9");
            mClient.DefaultRequestHeaders.Add("X-App-Device", Core.Helpers.DataHelper.GetMD5(guid));
        }

        [DebuggerStepThrough]
        private static string GetCoolapkAppToken()
        {
            var t = Core.Helpers.DataHelper.ConvertTimeToUnix(DateTime.Now);
            var hex_t = "0x" + Convert.ToString((int)t, 16);
            // 时间戳加密
            var md5_t = Core.Helpers.DataHelper.GetMD5($"{t}");
            var a = $"token://com.coolapk.market/c67ef5943784d09750dcfbb31020f0ab?{md5_t}${guid}&com.coolapk.market";
            var md5_a = Core.Helpers.DataHelper.GetMD5(Convert.ToBase64String(Encoding.UTF8.GetBytes(a)));
            var token = md5_a + guid + hex_t;
            return token;
        }

        [DebuggerStepThrough]
        public static async Task<string> PostAsync(Uri uri, IHttpContent content)
        {
            try
            {
                mClient.DefaultRequestHeaders.Remove("X-App-Token");
                mClient.DefaultRequestHeaders.Add("X-App-Token", GetCoolapkAppToken());
                var a = await mClient.PostAsync(uri, content);
                return await a.Content.ReadAsStringAsync();
            }
            catch { throw; }
        }

        [DebuggerStepThrough]
        public static async Task<BitmapImage> DownloadImageAsync(Uri uri, StorageFile file)
        {
            mClient.DefaultRequestHeaders.Remove("X-App-Token");
            mClient.DefaultRequestHeaders.Add("X-App-Token", GetCoolapkAppToken());

            var s = await mClient.GetInputStreamAsync(uri);
            using (var ss = await file.OpenStreamForWriteAsync())
            {
                await s.AsStreamForRead().CopyToAsync(ss);
            }

            return new BitmapImage(new Uri(file.Path));
        }

        /// <summary> 从指定URI中获取Json文本。 </summary>
        /// <param name="uri"> 数据在酷安服务器中的位置。 </param>
        [DebuggerStepThrough]
        public static async Task<string> GetJson(Uri uri)
        {
            try
            {
                //if (url != "/notification/checkCount") UIHelper.notifications?.RefreshNotificationsNum();
                mClient.DefaultRequestHeaders.Remove("X-App-Token");
                mClient.DefaultRequestHeaders.Add("X-App-Token", GetCoolapkAppToken());
                return await mClient.GetStringAsync(uri);
            }
            catch { throw; }
        }

        /// <summary> 通过用户名获取UID。 </summary>
        /// <param name="name"> 要获取UID的用户名。 </param>
        [DebuggerStepThrough]
        public static async Task<string> GetUserIDByNameAsync(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                var s = Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse().GetString("UserNameError");
                throw new CoolapkMessageException(s);
            }

            var str = string.Empty;
            try
            {
                str = await mClient.GetStringAsync(new Uri("https://www.coolapk.com/n/" + name));
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