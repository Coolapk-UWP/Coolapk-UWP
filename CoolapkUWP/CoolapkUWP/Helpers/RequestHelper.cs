using CoolapkUWP.Models;
using CoolapkUWP.Models.Feeds;
using CoolapkUWP.Models.Upload;
using Microsoft.Toolkit.Uwp.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml.Media.Imaging;
using mtuc = Microsoft.Toolkit.Uwp.Connectivity;

namespace CoolapkUWP.Helpers
{
    public static class RequestHelper
    {
        private static bool IsInternetAvailable => mtuc.NetworkHelper.Instance.ConnectionInformation.IsInternetAvailable;
        private static readonly object locker = new object();

        public static async Task<(bool isSucceed, JToken result)> GetDataAsync(Uri uri, bool isBackground = false)
        {
            string results = await NetworkHelper.GetStringAsync(uri, NetworkHelper.GetCoolapkCookies(uri), "XMLHttpRequest", isBackground);
            if (string.IsNullOrEmpty(results)) { return (false, null); }
            JObject token;
            try { token = JObject.Parse(results); }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.GetLogger(nameof(RequestHelper)).Error(ex.ExceptionToMessage(), ex);
                UIHelper.ShowInAppMessage(MessageType.Message, "加载失败");
                return (false, null);
            }
            if (!token.TryGetValue("data", out JToken data) && token.TryGetValue("message", out JToken message))
            {
                UIHelper.ShowInAppMessage(MessageType.Message, message.ToString());
                return (false, null);
            }
            else { return (data != null && !string.IsNullOrWhiteSpace(data.ToString()), data); }
        }

        public static async Task<(bool isSucceed, string result)> GetStringAsync(Uri uri, string request = "com.coolapk.market", bool isBackground = false)
        {
            string results = await NetworkHelper.GetStringAsync(uri, NetworkHelper.GetCoolapkCookies(uri), request, isBackground);
            if (string.IsNullOrWhiteSpace(results))
            {
                UIHelper.ShowInAppMessage(MessageType.Message, "加载失败");
                return (false, results);
            }
            else { return (true, results); }
        }

        public static async Task<(bool isSucceed, JToken result)> PostDataAsync(Uri uri, HttpContent content = null, bool isBackground = false)
        {
            string json = await NetworkHelper.PostAsync(uri, content, NetworkHelper.GetCoolapkCookies(uri), isBackground);
            if (string.IsNullOrEmpty(json)) { return (false, null); }
            JObject token;
            try { token = JObject.Parse(json); }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.GetLogger(nameof(RequestHelper)).Error(ex.ExceptionToMessage(), ex);
                UIHelper.ShowInAppMessage(MessageType.Message, "加载失败");
                return (false, null);
            }
            if (!token.TryGetValue("data", out JToken data) && token.TryGetValue("message", out JToken message))
            {
                bool _isSucceed = token.TryGetValue("error", out JToken error) && error.ToObject<int>() == 0;
                UIHelper.ShowInAppMessage(MessageType.Message, message.ToString());
                return (_isSucceed, token);
            }
            else if (data != null && !string.IsNullOrWhiteSpace(data.ToString())) { return (true, data); }
            else { return (token != null && !string.IsNullOrEmpty(token.ToString()), token); }
        }

        public static async Task<(bool isSucceed, string result)> PostStringAsync(Uri uri, HttpContent content = null, bool isBackground = false)
        {
            string json = await NetworkHelper.PostAsync(uri, content, NetworkHelper.GetCoolapkCookies(uri), isBackground);
            if (string.IsNullOrEmpty(json))
            {
                UIHelper.ShowInAppMessage(MessageType.Message, "加载失败");
                return (false, null);
            }
            else { return (true, json); }
        }

        private static (int page, Uri uri) GetPage(this Uri uri)
        {
            Regex pageregex = new Regex(@"([&|?])page=(\d+)(\??)");
            if (pageregex.IsMatch(uri.ToString()))
            {
                int pagenum = Convert.ToInt32(pageregex.Match(uri.ToString()).Groups[2].Value);
                Uri baseuri = new Uri(pageregex.Match(uri.ToString()).Groups[3].Value == "?" ? pageregex.Replace(uri.ToString(), pageregex.Match(uri.ToString()).Groups[1].Value) : pageregex.Replace(uri.ToString(), string.Empty));
                return (pagenum, baseuri);
            }
            else
            {
                return (0, uri);
            }
        }

        public static string GetId(JToken token, string _idName)
        {
            return token == null
                ? string.Empty
                : (token as JObject).TryGetValue(_idName, out JToken jToken)
                    ? jToken.ToString()
                    : (token as JObject).TryGetValue("entityId", out JToken v1)
                        ? v1.ToString()
                        : (token as JObject).TryGetValue("id", out JToken v2)
                            ? v2.ToString()
                            : throw new ArgumentException(nameof(_idName));
        }

#pragma warning disable 0612
        public static async Task<BitmapImage> GetImageAsync(string uri, bool isBackground = false)
        {
            StorageFolder folder = await ImageCacheHelper.GetFolderAsync(ImageType.Captcha);
            StorageFile file = await folder.CreateFileAsync(DataHelper.GetMD5(uri));

            Stream s = await NetworkHelper.GetStreamAsync(new Uri(uri), NetworkHelper.GetCoolapkCookies(new Uri(uri)), "XMLHttpRequest", isBackground);

            using (Stream ss = await file.OpenStreamForWriteAsync())
            {
                await s.CopyToAsync(ss);
            }

            return new BitmapImage(new Uri(file.Path));
        }
#pragma warning restore 0612

        public static async Task ChangeLikeAsync(this ICanLike model, CoreDispatcher dispatcher)
        {
            if (model == null) { return; }

            bool isReply = model is FeedReplyModel;
            Uri u = UriHelper.GetOldUri(
                model.Liked ? UriType.PostFeedUnlike : UriType.PostFeedLike,
                isReply ? "Reply" : string.Empty,
                model.ID);
            (bool isSucceed, JToken result) = await PostDataAsync(u, null, true);
            if (!isSucceed) { return; }

            int LikeNum = 0;
            if (isReply)
            {
                LikeNum = Convert.ToInt32(result.ToString().Replace("\"", string.Empty));
            }
            else
            {
                JObject json = result as JObject;
                if (json.TryGetValue("count", out JToken count))
                {
                    LikeNum = count.ToObject<int>();
                }
            }
            await dispatcher.AwaitableRunAsync(() =>
            {
                model.Liked = !model.Liked;
                model.LikeNum = LikeNum;
            });
        }

        public static async void UploadImagePrepare(IList<UploadFileFragment> images)
        {
            using (MultipartFormDataContent content = new MultipartFormDataContent())
            {
                string json = JsonConvert.SerializeObject(images, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                using (StringContent uploadBucket = new StringContent("image"))
                using (StringContent uploadDir = new StringContent("feed"))
                using (StringContent is_anonymous = new StringContent("0"))
                using (StringContent uploadFileList = new StringContent(json))
                {
                    content.Add(uploadBucket, "uploadBucket");
                    content.Add(uploadDir, "uploadDir");
                    content.Add(is_anonymous, "is_anonymous");
                    content.Add(uploadFileList, "uploadFileList");
                    (bool isSucceed, JToken result) = await PostDataAsync(UriHelper.GetUri(UriType.OOSUploadPrepare), content);
                    if (isSucceed)
                    {
                    }
                }
            }
        }

        public static async Task<(bool isSucceed, string result)> UploadImage(byte[] image, string name)
        {
            using (MultipartFormDataContent content = new MultipartFormDataContent())
            {
                using (ByteArrayContent picFile = new ByteArrayContent(image))
                {
                    picFile.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data");
                    picFile.Headers.ContentDisposition.Name = "\"picFile\"";
                    picFile.Headers.ContentDisposition.FileName = $"\"{name}\"";
                    picFile.Headers.ContentType = new MediaTypeHeaderValue("image/png");
                    picFile.Headers.ContentLength = image.Length;

                    content.Add(picFile);

                    (bool isSucceed, JToken result) = await PostDataAsync(UriHelper.GetOldUri(UriType.UploadImage, "feed"), content);

                    if (isSucceed) { return (isSucceed, result.ToString()); }
                }
            }
            return (false, null);
        }

        public static async Task<bool> CheckLogin()
        {
            (bool isSucceed, _) = await GetDataAsync(UriHelper.GetUri(UriType.CheckLoginInfo), true);
            return isSucceed;
        }
    }
}
