using CoolapkUWP.Helpers.Providers;
using CoolapkUWP.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;
using static CoolapkUWP.Helpers.NetworkHelper;
using SymbolIcon = Windows.UI.Xaml.Controls.SymbolIcon;
using Visibility = Windows.UI.Xaml.Visibility;

namespace CoolapkUWP.Helpers
{
    /// <summary> 提供数据处理的方法。 </summary>
    internal static class DataHelper
    {
        private static readonly Dictionary<Uri, string> responseCache = new Dictionary<Uri, string>();

        [DebuggerStepThrough]
        public static async Task<BitmapImage> GetImageAsync(string uri)
        {
            var folder = await ImageCacheHelper.GetFolderAsync(ImageType.Captcha);
            var file = await folder.CreateFileAsync(Core.Helpers.DataHelper.GetMD5(uri));
            return await DownloadImageAsync(new Uri(uri), file);
        }

        [DebuggerStepThrough]
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

        /// <summary> 从服务器中获取数据。 </summary>
        /// <param name="type"> 要获取的数据的类型。 </param>
        /// <param name="args"> 一个参数数组，其中的内容用于替换格式符号。 </param>
        [DebuggerStepThrough]
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

        public static async Task MakeLikeAsync(ICanChangeLikModel model, Windows.UI.Core.CoreDispatcher dispatcher, SymbolIcon like, SymbolIcon coloredLike)
        {
            if (model == null) { return; }

            bool isReply = model is FeedReplyModel;
            var u = UriProvider.GetUri(
                model.Liked ? UriType.OperateUnlike : UriType.OperateLike,
                isReply ? "Reply" : string.Empty, model.Id);
            var o = (JObject)await GetDataAsync(u, true);

            await dispatcher?.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                model.Liked = !model.Liked;
                if (isReply)
                {
                    model.Likenum = o.ToString().Replace("\"", string.Empty);
                }
                else if (o != null)
                {
                    model.Likenum = $"{o.Value<int>("count")}";
                }

                if (like != null)
                {
                    like.Visibility = model.Liked ? Visibility.Visible : Visibility.Collapsed;
                }
                if (coloredLike != null)
                {
                    coloredLike.Visibility = model.Liked ? Visibility.Collapsed : Visibility.Visible;
                }
            });
        }
    }
}