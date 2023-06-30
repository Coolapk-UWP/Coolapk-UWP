using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;
using mtuc = Microsoft.Toolkit.Uwp.Connectivity;
using System.Collections.Generic;
using CoolapkUWP.Models.Upload;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

#if FEATURE2
using System.Net.Http.Headers;
using CoolapkUWP.Common;
using CoolapkUWP.Models.Update;
using Windows.Foundation.Collections;
#else
using System.Linq;
#endif

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
                UIHelper.ShowMessage("加载失败");
                return (false, null);
            }
            if (!token.TryGetValue("data", out JToken data) && token.TryGetValue("message", out JToken message))
            {
                UIHelper.ShowMessage(message.ToString());
                return (false, null);
            }
            else { return (data != null && !string.IsNullOrWhiteSpace(data.ToString()), data); }
        }

        public static async Task<(bool isSucceed, string result)> GetStringAsync(Uri uri, string request = "com.coolapk.market", bool isBackground = false)
        {
            string results = await NetworkHelper.GetStringAsync(uri, NetworkHelper.GetCoolapkCookies(uri), request, isBackground);
            if (string.IsNullOrWhiteSpace(results))
            {
                UIHelper.ShowMessage("加载失败");
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
                UIHelper.ShowMessage("加载失败");
                return (false, null);
            }
            if (!token.TryGetValue("data", out JToken data) && token.TryGetValue("message", out JToken message))
            {
                bool _isSucceed = token.TryGetValue("error", out JToken error) && error.ToObject<int>() == 0;
                UIHelper.ShowMessage(message.ToString());
                return (_isSucceed, token);
            }
            else
            {
                return data != null && !string.IsNullOrWhiteSpace(data.ToString())
                ? ((bool isSucceed, JToken result))(true, data)
                : ((bool isSucceed, JToken result))(token != null && !string.IsNullOrEmpty(token.ToString()), token);
            }
        }

        public static async Task<(bool isSucceed, string result)> PostStringAsync(Uri uri, HttpContent content = null, bool isBackground = false)
        {
            string json = await NetworkHelper.PostAsync(uri, content, NetworkHelper.GetCoolapkCookies(uri), isBackground);
            if (string.IsNullOrEmpty(json))
            {
                UIHelper.ShowMessage("加载失败");
                return (false, null);
            }
            else { return (true, json); }
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

#if FEATURE2
        public static async Task<string[]> UploadImages(IEnumerable<UploadFileFragment> fragments, Extension extension)
        {
            ValueSet message = new ValueSet
            {
                ["UID"] = SettingsHelper.Get<string>(SettingsHelper.Uid),
                ["UserName"] = SettingsHelper.Get<string>(SettingsHelper.UserName),
                ["Token"] = SettingsHelper.Get<string>(SettingsHelper.Token),
                ["TokenVersion"] = (int)SettingsHelper.Get<TokenVersions>(SettingsHelper.TokenVersion),
                ["UserAgent"] = JsonConvert.SerializeObject(UserAgent.Parse(NetworkHelper.Client.DefaultRequestHeaders.UserAgent.ToString())),
                ["APIVersion"] = JsonConvert.SerializeObject(APIVersion.Parse(NetworkHelper.Client.DefaultRequestHeaders.UserAgent.ToString())),
                ["Images"] = JsonConvert.SerializeObject(fragments, new JsonSerializerSettings { ContractResolver = new IgnoreIgnoredContractResolver() })
            };
            return await extension.Invoke(message) as string[];
        }

        public static async Task<(bool isSucceed, string result)> UploadImage(byte[] image, string name)
        {
            using (MultipartFormDataContent content = new MultipartFormDataContent())
            {
                using (ByteArrayContent picFile = new ByteArrayContent(image))
                {
                    picFile.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                    {
                        Name = "\"picFile\"",
                        FileName = $"\"{name}\""
                    };
                    picFile.Headers.ContentType = new MediaTypeHeaderValue("image/png");
                    picFile.Headers.ContentLength = image.Length;

                    content.Add(picFile);

                    (bool isSucceed, JToken result) = await PostDataAsync(UriHelper.GetOldUri(UriType.UploadImage, "feed"), content);

                    if (isSucceed) { return (isSucceed, result.ToString()); }
                }
            }
            return (false, null);
        }

        private class IgnoreIgnoredContractResolver : DefaultContractResolver
        {
            protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
            {
                IList<JsonProperty> list = base.CreateProperties(type, memberSerialization);
                if (list != null)
                {
                    foreach (JsonProperty item in list)
                    {
                        if (item.Ignored)
                        {
                            item.Ignored = false;
                        }
                    }
                }
                return list;
            }
        }
#else
        public static async Task<List<string>> UploadImages(IEnumerable<UploadFileFragment> images)
        {
            List<string> responses = new List<string>();
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
                        UploadPicturePrepareResult data = result.ToObject<UploadPicturePrepareResult>();
                        foreach (UploadFileInfo info in data.FileInfo)
                        {
                            UploadFileFragment image = images.FirstOrDefault((x) => x.MD5 == info.MD5);
                            if (image == null) { continue; }
                            using (Stream stream = image.Bytes.GetStream())
                            {
                                string response = await Task.Run(() => OSSUploadHelper.OssUpload(data.UploadPrepareInfo, info, stream, "image/png"));
                                if (!string.IsNullOrEmpty(response))
                                {
                                    try
                                    {
                                        JObject token = JObject.Parse(response);
                                        if (token.TryGetValue("data", out JToken value)
                                            && ((JObject)value).TryGetValue("url", out JToken url)
                                            && !string.IsNullOrEmpty(url.ToString()))
                                        {
                                            responses.Add(url.ToString());
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        SettingsHelper.LogManager.GetLogger(nameof(RequestHelper)).Error(ex.ExceptionToMessage(), ex);
                                        UIHelper.ShowMessage("上传失败");
                                        continue;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return responses;
        }
#endif

        public static async Task<bool> CheckLogin()
        {
            (bool isSucceed, _) = await GetDataAsync(UriHelper.GetUri(UriType.CheckLoginInfo), true);
            return isSucceed;
        }
    }
}
