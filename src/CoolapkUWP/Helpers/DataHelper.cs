using CoolapkUWP.Core.Helpers;
using CoolapkUWP.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

        public static Task<(bool isSucceed, JToken result)> PostDataAsync(Uri uri, System.Net.Http.HttpContent content)
        {
            return Utils.PostDataAsync(uri, content, GetCoolapkCookies());
        }

        public static Task<(bool isSucceed, JToken result)> GetDataAsync(Uri uri, bool forceRefresh)
        {
            return Utils.GetDataAsync(uri, forceRefresh, GetCoolapkCookies());
        }

        public static Task Refresh(this Core.Providers.CoolapkListProvider provider, int p = 1)
        {
            return provider.Refresh(GetCoolapkCookies(), p);
        }

        public static Task Search(this Core.Providers.SearchListProvider provider, string keyWord)
        {
            return provider.Search(keyWord, GetCoolapkCookies());
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

        public static string ConvertUnixTimeStampToReadable(double time)
        {
            return ConvertUnixTimeStampToReadable(time, DateTime.Now);
        }

        public static string ConvertUnixTimeStampToReadable(double time, DateTime baseTime)
        {
            var (type, obj) = Utils.ConvertUnixTimeStampToReadable(time, baseTime);
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
            var (isSucceed, result) = await GetDataAsync(u, true);
            if (!isSucceed) { return; }

            var o = (JObject)result;

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