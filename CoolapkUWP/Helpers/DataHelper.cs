using CoolapkUWP.Helpers.Providers;
using CoolapkUWP.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.UI.Xaml.Media.Imaging;
using static CoolapkUWP.Helpers.NetworkHelper;
using SymbolIcon = Windows.UI.Xaml.Controls.SymbolIcon;
using Visibility = Windows.UI.Xaml.Visibility;

namespace CoolapkUWP.Helpers
{
    /// <summary>
    ///     提供数据处理的方法。
    /// </summary>
    internal static class DataHelper
    {
        private static readonly Dictionary<Uri, object> responseCache = new Dictionary<Uri, object>();

        public static string GetMD5(string inputString)
        {
            CryptographicHash objHash = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Md5).CreateHash();
            objHash.Append(CryptographicBuffer.ConvertStringToBinary(inputString, BinaryStringEncoding.Utf8));
            Windows.Storage.Streams.IBuffer buffHash1 = objHash.GetValueAndReset();
            return CryptographicBuffer.EncodeToHexString(buffHash1);
        }

        public static async Task<BitmapImage> GetImageAsync(string uri)
        {
            var folder = await ImageCacheHelper.GetFolderAsync(ImageType.Captcha);
            var file = await folder.CreateFileAsync(GetMD5(uri));
            return await DownloadImageAsync(new Uri(uri), file);
        }

        public static async Task<JToken> PostDataAsync(Uri uri, Windows.Web.Http.IHttpContent content)
        {
            var json = await PostAsync(uri, content);
            var o = JObject.Parse(json);
            JToken token = null;
            if (!string.IsNullOrEmpty(json) &&
                !o.TryGetValue("data", out token) &&
                o.TryGetValue("message", out _))
            {
                throw new CoolapkMessageException(o);
            }
            else return token;
        }

        /// <summary>
        ///     从服务器中获取数据。
        /// </summary>
        /// <param name="type">
        ///     要获取的数据的类型。
        /// </param>
        /// <param name="args">
        ///     一个参数数组，其中的内容用于替换格式符号。
        /// </param>
        public static async Task<JToken> GetDataAsync(Uri uri, bool forceRefresh)
        {
            string json;
            if (forceRefresh || !responseCache.ContainsKey(uri))
            {
                json = await GetJson(uri);
                if (responseCache.ContainsKey(uri))
                {
                    responseCache[uri] = json;

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
                    responseCache.Add(uri, json);
                }
            }
            else
            {
                json = responseCache[uri] as string;
            }
            var o = JObject.Parse(json);
            JToken token = null;
            if (!string.IsNullOrEmpty(json) &&
                !o.TryGetValue("data", out token) &&
                o.TryGetValue("message", out _))
                throw new CoolapkMessageException(o);
            else return token;
        }

        public static async Task<T> GetDataAsync<T>(Uri uri, bool forceRefresh = false)
        {
            string json = string.Empty;
            T result = default;

            if (forceRefresh || !responseCache.ContainsKey(uri))
            {
                json = await GetJson(uri);
                result = await Task.Run(() => JsonConvert.DeserializeObject<T>(json, new JsonSerializerSettings
                {
                    Error = (_, e) =>
                    {
                        if (!string.IsNullOrEmpty(json))
                        {
                            var o = JsonConvert.DeserializeObject<Models.Json.MessageModel.Rootobject>(json, new JsonSerializerSettings { Error = (__, ee) => ee.ErrorContext.Handled = true });
                            if (o != null)
                                throw new CoolapkMessageException(o.Message);
                        }
                        e.ErrorContext.Handled = true;
                    }
                }));
                if (responseCache.ContainsKey(uri))
                {
                    responseCache[uri] = result;

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
                    responseCache.Add(uri, result);
                }
            }
            else
            {
                result = (T)responseCache[uri];
            }

            return result;
        }

        static readonly DateTime unixDateBase = new DateTime(1970, 1, 1).ToLocalTime();

        /// <summary>
        ///     转换Unix时间戳至可读时间。
        /// </summary>
        /// <param name="timeD">
        ///     Unix时间戳。
        /// </param>
        public static string ConvertUnixTimeToReadable(double timeD)
        {
            DateTime time = unixDateBase.Add(new TimeSpan(Convert.ToInt64(timeD) * 1000_0000));
            TimeSpan temp = DateTime.Now.Subtract(time);

            if (temp.TotalDays > 30)
            {
                return $"{time.Year}/{time.Month}/{time.Day}";
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

        public static string ConvertTimeToUnix(DateTime time) => $"{time.Subtract(unixDateBase).TotalSeconds:F0}";

        public static async Task MakeLikeAsync(ICanChangeLikModel model, Windows.UI.Core.CoreDispatcher dispatcher, SymbolIcon like, SymbolIcon coloredLike)
        {
            if (model == null) { return; }

            bool isReply = model is FeedReplyModel;
            var s = UriProvider.GetObject(model.Liked ? UriType.OperateUnlike : UriType.OperateLike);
            var u = s.GetUri(isReply ? "Reply" : string.Empty, model.Id);
            JObject o = (JObject)await GetDataAsync(u, true);

            await dispatcher?.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                model.Liked = !model.Liked;
                if (isReply)
                {
                    model.Likenum = o.ToString().Replace("\"", string.Empty);
                }
                else if (o != null)
                {
                    model.Likenum = o.Value<int>("count").ToString();
                }

                if (like != null)
                {
                    like.Visibility = model.Liked ? Visibility.Collapsed : Visibility.Visible;
                }
                if (coloredLike != null)
                {
                    coloredLike.Visibility = model.Liked ? Visibility.Visible : Visibility.Collapsed;
                }
            });
        }
    }
}