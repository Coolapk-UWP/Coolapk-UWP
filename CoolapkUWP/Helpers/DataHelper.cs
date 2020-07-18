using CoolapkUWP.Core.Helpers;
using CoolapkUWP.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;
using static CoolapkUWP.Core.Helpers.NetworkHelper;
using SymbolIcon = Windows.UI.Xaml.Controls.SymbolIcon;
using Visibility = Windows.UI.Xaml.Visibility;

namespace CoolapkUWP.Helpers
{
    [DebuggerStepThrough]
    internal static class DataHelper
    {
        private static readonly Dictionary<Uri, string> responseCache = new Dictionary<Uri, string>();

        private static IEnumerable<(string name, string value)> GetCoolapkCookies()
        {
            using (var filter = new Windows.Web.Http.Filters.HttpBaseProtocolFilter())
            {
                var cookieManager = filter.CookieManager;
                foreach (var item in cookieManager.GetCookies(UriHelper.BaseUri))
                {
                    if (item.Name == "uid" ||
                        item.Name == "username" ||
                        item.Name == "token")
                    {
                        yield return (item.Name, item.Value);
                    }
                }
            }
        }

        public static async Task<BitmapImage> GetImageAsync(string uri)
        {
            var folder = await ImageCacheHelper.GetFolderAsync(ImageType.Captcha);
            var file = await folder.CreateFileAsync(Utils.GetMD5(uri));

            var s = await GetStreamAsync(new Uri(uri), GetCoolapkCookies());

            using (var ss = await file.OpenStreamForWriteAsync())
            {
                await s.CopyToAsync(ss);
            }

            return new BitmapImage(new Uri(file.Path));
        }

        public static async Task<JToken> PostDataAsync(Uri uri, System.Net.Http.HttpContent content)
        {
            var json = await PostAsync(uri, content, GetCoolapkCookies());

            var o = JObject.Parse(json);
            JToken token = null;
            if (!string.IsNullOrEmpty(json) &&
                !o.TryGetValue("data", out token) &&
                o.TryGetValue("message", out _))
            {
                throw new Core.Exceptions.CoolapkMessageException(o);
            }
            else return token;
        }

        public static async Task<JToken> GetDataAsync(Uri uri, bool forceRefresh)
        {
            string json;
            if (forceRefresh || !responseCache.ContainsKey(uri))
            {
                json = await GetJsonAsync(uri, GetCoolapkCookies());

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
                throw new Core.Exceptions.CoolapkMessageException(o);
            else return token;
        }

        public static Task<string> GetUserIDByNameAsync(string name)
        {
            return NetworkHelper.GetUserIDByNameAsync(name);
        }

        public static string ConvertUnixTimeStampToReadable(double time)
        {
            return ConvertUnixTimeStampToReadable(time, DateTime.Now);
        }

        public static string ConvertUnixTimeStampToReadable(double time, DateTime baseTime)
        {
            var (type, obj) = Utils.ConvertUnixTimeStampToReadable(time, DateTime.Now);
            switch (type)
            {
                case Utils.TimeIntervalType.MonthsAgo:
                    return ((DateTime)obj).ToLongDateString();
                case Utils.TimeIntervalType.DaysAgo:
                    return $"{((TimeSpan)obj).Days}天前";
                case Utils.TimeIntervalType.HoursAgo:
                    return $"{((TimeSpan)obj).Hours}小时前";
                case Utils.TimeIntervalType.MinutesAgo:
                    return $"{((TimeSpan)obj).Minutes}分钟前";
                case Utils.TimeIntervalType.JustNow:
                    return "刚刚";
                default:
                    return string.Empty;
            }
        }

        public static async Task MakeLikeAsync(ICanChangeLikModel model, Windows.UI.Core.CoreDispatcher dispatcher, SymbolIcon like, SymbolIcon coloredLike)
        {
            if (model == null) { return; }

            bool isReply = model is FeedReplyModel;
            var u = UriHelper.GetUri(
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