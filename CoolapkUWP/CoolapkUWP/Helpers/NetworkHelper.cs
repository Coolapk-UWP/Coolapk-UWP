using CoolapkUWP.Common;
using CoolapkUWP.Models.Exceptions;
using Microsoft.Toolkit.Uwp.Helpers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.System.Profile;
using Windows.Web.Http;
using Windows.Web.Http.Filters;
using HttpClient = System.Net.Http.HttpClient;
using HttpResponseMessage = System.Net.Http.HttpResponseMessage;
using HttpStatusCode = System.Net.HttpStatusCode;

namespace CoolapkUWP.Helpers
{
    public static partial class NetworkHelper
    {
        public static readonly HttpClientHandler ClientHandler;
        public static readonly HttpClient Client;

        private static SemaphoreSlim semaphoreSlim;
        private static TokenCreater token;

        static NetworkHelper()
        {
            semaphoreSlim = new SemaphoreSlim(SettingsHelper.Get<int>(SettingsHelper.SemaphoreSlimCount));
            ThemeHelper.UISettingChanged.Add((arg) => Client?.DefaultRequestHeaders?.ReplaceDarkMode());
            ClientHandler = new HttpClientHandler();
            Client = new HttpClient(ClientHandler);
            SetRequestHeaders();
            SetLoginCookie();
        }

        public static void SetSemaphoreSlim(int initialCount)
        {
            semaphoreSlim.Dispose();
            semaphoreSlim = new SemaphoreSlim(initialCount);
        }

        public static void SetLoginCookie()
        {
            string Uid = SettingsHelper.Get<string>(SettingsHelper.Uid);
            string UserName = SettingsHelper.Get<string>(SettingsHelper.UserName);
            string Token = SettingsHelper.Get<string>(SettingsHelper.Token);

            if (!string.IsNullOrEmpty(Uid) && !string.IsNullOrEmpty(UserName) && !string.IsNullOrEmpty(Token))
            {
                using (HttpBaseProtocolFilter filter = new HttpBaseProtocolFilter())
                {
                    HttpCookieManager cookieManager = filter.CookieManager;
                    HttpCookie uid = new HttpCookie("uid", ".coolapk.com", "/");
                    HttpCookie username = new HttpCookie("username", ".coolapk.com", "/");
                    HttpCookie token = new HttpCookie("token", ".coolapk.com", "/");
                    uid.Value = Uid;
                    username.Value = UserName;
                    token.Value = Token;
                    cookieManager.SetCookie(uid);
                    cookieManager.SetCookie(username);
                    cookieManager.SetCookie(token);
                }
                SettingsHelper.InvokeLoginChanged(Uid, true);
            }
        }

        public static void SetRequestHeaders()
        {
            EasClientDeviceInformation deviceInfo = new EasClientDeviceInformation();
            ulong OSVersion = ulong.Parse(AnalyticsInfo.VersionInfo.DeviceFamilyVersion);
            APIVersion APIVersion = SettingsHelper.Get<APIVersion>(SettingsHelper.APIVersion);
            TokenVersion TokenVersion = SettingsHelper.Get<TokenVersion>(SettingsHelper.TokenVersion);
            string Culture = LanguageHelper.GetPrimaryLanguage();

            token = new TokenCreater(TokenVersion);
            Client.DefaultRequestHeaders.Clear();
            Client.DefaultRequestHeaders.Add("X-Sdk-Int", "30");
            Client.DefaultRequestHeaders.Add("X-Sdk-Locale", Culture);
            Client.DefaultRequestHeaders.Add("X-App-Mode", "universal");
            Client.DefaultRequestHeaders.Add("X-App-Channel", "coolapk");
            Client.DefaultRequestHeaders.Add("X-App-Id", "com.coolapk.market");
            Client.DefaultRequestHeaders.Add("X-App-Device", TokenCreater.DeviceCode);
            Client.DefaultRequestHeaders.Add("X-Dark-Mode", ThemeHelper.IsDarkTheme() ? "1" : "0");
            Client.DefaultRequestHeaders.UserAgent.ParseAdd($"Dalvik/2.1.0 (Windows NT {(ushort)((OSVersion & 0xFFFF000000000000L) >> 48)}.{(ushort)((OSVersion & 0x0000FFFF00000000L) >> 32)}; Win{(Package.Current.Id.Architecture.ToString().Contains("64") ? "64" : "32")}; {Package.Current.Id.Architecture.ToString().Replace("X", "x")}; WebView/3.0) (#Build; {deviceInfo.SystemManufacturer}; {deviceInfo.SystemProductName}; {deviceInfo.SystemProductName}_{deviceInfo.SystemSku}; {(ushort)((OSVersion & 0xFFFF000000000000L) >> 48)}.{(ushort)((OSVersion & 0x0000FFFF00000000L) >> 32)}.{(ushort)((OSVersion & 0x00000000FFFF0000L) >> 16)}.{(ushort)(OSVersion & 0x000000000000FFFFL)})");

            switch (APIVersion)
            {
                case APIVersion.V6:
                    Client.DefaultRequestHeaders.UserAgent.ParseAdd(" +CoolMarket/6.10.6-1608291-universal");
                    Client.DefaultRequestHeaders.Add("X-App-Version", "6.10.6");
                    Client.DefaultRequestHeaders.Add("X-App-Code", "1608291");
                    break;
                case APIVersion.V7:
                    Client.DefaultRequestHeaders.UserAgent.ParseAdd(" +CoolMarket/7.9.6_S-1710201-universal");
                    Client.DefaultRequestHeaders.Add("X-App-Version", "7.9.6_S");
                    Client.DefaultRequestHeaders.Add("X-App-Code", "1710201");
                    Client.DefaultRequestHeaders.Add("X-Api-Version", "7");
                    break;
                case APIVersion.V8:
                    Client.DefaultRequestHeaders.UserAgent.ParseAdd(" +CoolMarket/8.7-1809041-universal");
                    Client.DefaultRequestHeaders.Add("X-App-Version", "8.7");
                    Client.DefaultRequestHeaders.Add("X-App-Code", "1809041");
                    Client.DefaultRequestHeaders.Add("X-Api-Version", "8");
                    break;
                case APIVersion.V9:
                    Client.DefaultRequestHeaders.UserAgent.ParseAdd(" +CoolMarket/9.6.3-1910291-universal");
                    Client.DefaultRequestHeaders.Add("X-App-Version", "9.6.3");
                    Client.DefaultRequestHeaders.Add("X-App-Code", "1910291");
                    Client.DefaultRequestHeaders.Add("X-Api-Version", "9");
                    break;
                case APIVersion.小程序:
                    Client.DefaultRequestHeaders.UserAgent.ParseAdd(" +CoolMarket/1.0-1902250-universal");
                    Client.DefaultRequestHeaders.Add("X-App-Version", "1.0");
                    Client.DefaultRequestHeaders.Add("X-App-Code", "1902250");
                    Client.DefaultRequestHeaders.Add("X-Api-Version", "9");
                    break;
                case APIVersion.V10:
                    Client.DefaultRequestHeaders.UserAgent.ParseAdd(" +CoolMarket/10.5.3-2009271-universal");
                    Client.DefaultRequestHeaders.Add("X-App-Version", "10.5.3");
                    Client.DefaultRequestHeaders.Add("X-App-Code", "2009271");
                    Client.DefaultRequestHeaders.Add("X-Api-Version", "10");
                    break;
                case APIVersion.V11:
                    Client.DefaultRequestHeaders.UserAgent.ParseAdd(" +CoolMarket/11.4.7-2112231-universal");
                    Client.DefaultRequestHeaders.Add("X-App-Version", "11.4.7");
                    Client.DefaultRequestHeaders.Add("X-App-Code", "2112231");
                    Client.DefaultRequestHeaders.Add("X-Api-Version", "11");
                    break;
                case APIVersion.V12:
                    Client.DefaultRequestHeaders.UserAgent.ParseAdd(" +CoolMarket/12.5.4-2212261-universal");
                    Client.DefaultRequestHeaders.Add("X-App-Version", "12.5.4");
                    Client.DefaultRequestHeaders.Add("X-Api-Supported", "2212261");
                    Client.DefaultRequestHeaders.Add("X-App-Code", "2212261");
                    Client.DefaultRequestHeaders.Add("X-Api-Version", "12");
                    break;
                case APIVersion.V13:
                default:
                    Client.DefaultRequestHeaders.UserAgent.ParseAdd(" +CoolMarket/13.1.2-2304171-universal");
                    Client.DefaultRequestHeaders.Add("X-App-Version", "13.1.2");
                    Client.DefaultRequestHeaders.Add("X-Api-Supported", "2304171");
                    Client.DefaultRequestHeaders.Add("X-App-Code", "2304171");
                    Client.DefaultRequestHeaders.Add("X-Api-Version", "13");
                    break;
            }
        }

        public static IEnumerable<(string name, string value)> GetCoolapkCookies(Uri uri)
        {
            using (HttpBaseProtocolFilter filter = new HttpBaseProtocolFilter())
            {
                HttpCookieManager cookieManager = filter.CookieManager;
                foreach (HttpCookie item in cookieManager.GetCookies(GetHost(uri)))
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

        private static void ReplaceDarkMode(this HttpRequestHeaders headers)
        {
            const string name = "X-Dark-Mode";
            _ = headers.Remove(name);
            headers.Add(name, ThemeHelper.IsDarkTheme() ? "1" : "0");
        }

        private static void ReplaceAppToken(this HttpRequestHeaders headers)
        {
            const string name = "X-App-Token";
            _ = headers.Remove(name);
            headers.Add(name, token.GetToken());
        }

        private static void ReplaceRequested(this HttpRequestHeaders headers, string request)
        {
            const string name = "X-Requested-With";
            _ = headers.Remove(name);
            if (request != null) { headers.Add(name, request); }
        }

        private static void ReplaceCoolapkCookie(this CookieContainer container, IEnumerable<(string name, string value)> cookies, Uri uri)
        {
            if (cookies == null) { return; }

            foreach ((string name, string value) in cookies)
            {
                container.SetCookies(GetHost(uri), $"{name}={value}");
            }
        }

        private static void BeforeGetOrPost(IEnumerable<(string name, string value)> coolapkCookies, Uri uri, string request)
        {
            ClientHandler.CookieContainer.ReplaceCoolapkCookie(coolapkCookies, uri);
            Client.DefaultRequestHeaders.ReplaceAppToken();
            Client.DefaultRequestHeaders.ReplaceRequested(request);
        }

    }

    public static partial class NetworkHelper
    {
        public static async Task<string> PostAsync(Uri uri, HttpContent content, IEnumerable<(string name, string value)> coolapkCookies, bool isBackground)
        {
            try
            {
                HttpResponseMessage response;
                BeforeGetOrPost(coolapkCookies, uri, "XMLHttpRequest");
                response = await Client.PostAsync(uri, content);
                return await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException e)
            {
                SettingsHelper.LogManager.GetLogger(nameof(ImageCacheHelper)).Error(e.ExceptionToMessage(), e);
                if (!isBackground) { UIHelper.ShowHttpExceptionMessage(e); }
                return null;
            }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.GetLogger(nameof(NetworkHelper)).Error(ex.ExceptionToMessage(), ex);
                return null;
            }
        }

        public static async Task<Stream> GetStreamAsync(Uri uri, IEnumerable<(string name, string value)> coolapkCookies, string request = "XMLHttpRequest", bool isBackground = false)
        {
            try
            {
                BeforeGetOrPost(coolapkCookies, uri, request);
                return await Client.GetStreamAsync(uri);
            }
            catch (HttpRequestException e)
            {
                SettingsHelper.LogManager.GetLogger(nameof(NetworkHelper)).Error(e.ExceptionToMessage(), e);
                if (!isBackground) { UIHelper.ShowHttpExceptionMessage(e); }
                return null;
            }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.GetLogger(nameof(NetworkHelper)).Error(ex.ExceptionToMessage(), ex);
                return null;
            }
        }

        public static async Task<string> GetStringAsync(Uri uri, IEnumerable<(string name, string value)> coolapkCookies, string request = "XMLHttpRequest", bool isBackground = false)
        {
            try
            {
                BeforeGetOrPost(coolapkCookies, uri, request);
                return await Client.GetStringAsync(uri);
            }
            catch (HttpRequestException e)
            {
                SettingsHelper.LogManager.GetLogger(nameof(NetworkHelper)).Error(e.ExceptionToMessage(), e);
                if (!isBackground) { UIHelper.ShowHttpExceptionMessage(e); }
                return null;
            }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.GetLogger(nameof(NetworkHelper)).Error(ex.ExceptionToMessage(), ex);
                return null;
            }
        }
    }

    public static partial class NetworkHelper
    {
        /// <summary>
        /// 通过用户名或 UID 获取用户信息。
        /// </summary>
        /// <param name="name">要获取信息的用户名或 UID 。</param>
        /// <param name="isBackground">是否通知错误。</param>
        /// <returns>用户信息</returns>
        public static async Task<(string UID, string UserName, string UserAvatar)> GetUserInfoByNameAsync(string name, bool isBackground = false)
        {
            (string UID, string UserName, string UserAvatar) result = (string.Empty, string.Empty, string.Empty);

            if (string.IsNullOrEmpty(name))
            {
                throw new UserNameErrorException();
            }

            string str = string.Empty;
            try
            {
                str = await Client.GetStringAsync(new Uri($"https://www.coolapk.com/n/{name}"));

                JObject token = JObject.Parse(str);
                if (token.TryGetValue("dataRow", out JToken v1))
                {
                    JObject dataRow = (JObject)v1;

                    if (dataRow.TryGetValue("uid", out JToken uid))
                    {
                        result.UID = uid.ToString();
                    }

                    if (dataRow.TryGetValue("username", out JToken username))
                    {
                        result.UserName = username.ToString();
                    }

                    if (dataRow.TryGetValue("userAvatar", out JToken userAvatar))
                    {
                        result.UserAvatar = userAvatar.ToString();
                    }

                    return result;
                }

                throw new Exception();
            }
            catch (HttpRequestException e)
            {
                SettingsHelper.LogManager.GetLogger(nameof(NetworkHelper)).Error(e.ExceptionToMessage(), e);
                if (!isBackground) { UIHelper.ShowHttpExceptionMessage(e); }
                return result;
            }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.GetLogger(nameof(NetworkHelper)).Error(ex.ExceptionToMessage(), ex);
                if (string.IsNullOrWhiteSpace(str)) { throw ex; }
                JObject o = JObject.Parse(str);
                if (o == null) { throw ex; }
                else { throw new CoolapkMessageException(o); }
            }
        }

        public static Uri GetHost(Uri uri) => new Uri("https://" + uri.Host);

        public static string ExpandShortUrl(this Uri ShortUrl)
        {
            string NativeUrl = null;
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(ShortUrl);
            try { _ = req.HaveResponse; }
            catch (WebException ex)
            {
                HttpWebResponse res = ex.Response as HttpWebResponse;
                if (res.StatusCode == HttpStatusCode.Found)
                { NativeUrl = res.Headers["Location"]; }
            }
            return NativeUrl ?? ShortUrl.ToString();
        }

        public static Uri ValidateAndGetUri(this string url)
        {
            if (string.IsNullOrWhiteSpace(url)) { return null; }
            Uri uri = null;
            try
            {
                uri = url.Contains("://") ? new Uri(url)
                    : url[0] == '/' ? new Uri(UriHelper.CoolapkUri, url)
                    : new Uri($"https://{url}");
            }
            catch (FormatException ex)
            {
                SettingsHelper.LogManager.GetLogger(nameof(NetworkHelper)).Warn(ex.ExceptionToMessage(), ex);
            }
            return uri;
        }
    }
}
