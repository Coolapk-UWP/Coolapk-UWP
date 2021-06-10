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
        private static IEnumerable<(string name, string value)> GetCoolapkCookies(Uri uri)
        {
            using (Windows.Web.Http.Filters.HttpBaseProtocolFilter filter = new Windows.Web.Http.Filters.HttpBaseProtocolFilter())
            {
                Windows.Web.Http.HttpCookieManager cookieManager = filter.CookieManager;
                foreach (Windows.Web.Http.HttpCookie item in cookieManager.GetCookies(GetHost(uri)))
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

        public static Task<(bool isSucceed, string result)> PostHtmlAsync(Uri uri, System.Net.Http.HttpContent content)
        {
            return Utils.PostHtmlAsync(uri, content, GetCoolapkCookies(uri));
        }

        public static Task<(bool isSucceed, JToken result)> PostDataAsync(Uri uri, System.Net.Http.HttpContent content)
        {
            return Utils.PostDataAsync(uri, content, GetCoolapkCookies(uri));
        }

        public static Task<(bool isSucceed, string result)> GetHtmlAsync(Uri uri, string request)
        {
            return Utils.GetHtmlAsync(uri, GetCoolapkCookies(uri), request);
        }

        public static Task<(bool isSucceed, JToken result)> GetDataAsync(Uri uri, bool forceRefresh)
        {
            return Utils.GetDataAsync(uri, forceRefresh, GetCoolapkCookies(uri));
        }

        public static Task Refresh(this Core.Providers.CoolapkListProvider provider, int p = 1)
        {
            return provider.Refresh(GetCoolapkCookies(UriHelper.BaseUri), p);
        }

        public static Task Search(this Core.Providers.SearchListProvider provider, string keyWord)
        {
            return provider.Search(keyWord, GetCoolapkCookies(UriHelper.BaseUri));
        }

#pragma warning disable 0612
        public static async Task<BitmapImage> GetImageAsync(string uri)
        {
            Windows.Storage.StorageFolder folder = await ImageCacheHelper.GetFolderAsync(ImageType.Captcha);
            Windows.Storage.StorageFile file = await folder.CreateFileAsync(Utils.GetMD5(uri));

            Stream s = await GetStreamAsync(new Uri(uri), GetCoolapkCookies(new Uri(uri)));

            using (Stream ss = await file.OpenStreamForWriteAsync())
            {
                await s.CopyToAsync(ss);
            }

            return new BitmapImage(new Uri(file.Path));
        }
#pragma warning restore 0612

        public static string ConvertUnixTimeStampToReadable(double time)
        {
            return ConvertUnixTimeStampToReadable(time, DateTime.Now);
        }

        public static string ConvertUnixTimeStampToReadable(double time, DateTime baseTime)
        {
            (Utils.TimeIntervalType type, object obj) = Utils.ConvertUnixTimeStampToReadable(time, baseTime);
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
            Uri u = UriHelper.GetUri(
                model.Liked ? UriType.OperateUnlike : UriType.OperateLike,
                isReply ? "Reply" : string.Empty,
                model.Id);
            (bool isSucceed, JToken result) = await GetDataAsync(u, true);
            if (!isSucceed) { return; }

            JObject o = result as JObject;
            await dispatcher?.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                model.Liked = !model.Liked;
                if (isReply)
                {
                    model.Likenum = o.ToString().Replace("\"", string.Empty, StringComparison.OrdinalIgnoreCase);
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