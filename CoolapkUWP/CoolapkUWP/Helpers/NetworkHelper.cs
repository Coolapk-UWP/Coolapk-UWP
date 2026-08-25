using CoolapkUWP.Common;
using CoolapkUWP.Models.Exceptions;
using CoolapkUWP.Models.Update;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.Web.Http;
using Windows.Web.Http.Filters;
using Windows.Web.Http.Headers;
using HttpClient = System.Net.Http.HttpClient;
using HttpResponseMessage = System.Net.Http.HttpResponseMessage;
using HttpStatusCode = System.Net.HttpStatusCode;

namespace CoolapkUWP.Helpers
{
    public static partial class NetworkHelper
    {
        private static readonly object appTokenLock = new object();
        private static readonly object darkModeLock = new object();
        private static readonly object requestedLock = new object();

        public const string XMLHttpRequest = "XMLHttpRequest";

        public static readonly HttpClientHandler ClientHandler;
        public static readonly HttpClient Client;

        private static SemaphoreSlim semaphoreSlim;
        public static TokenCreator TokenCreator;

        static NetworkHelper()
        {
            semaphoreSlim = new SemaphoreSlim(SettingsHelper.Get<int>(SettingsHelper.SemaphoreSlimCount));
            ClientHandler = new HttpClientHandler { MaxConnectionsPerServer = 20 };
            Client = new HttpClient(ClientHandler);
            ThemeHelper.UISettingChanged += arg => Client.DefaultRequestHeaders.ReplaceDarkMode(arg);
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
            TokenCreator = new TokenCreator(SettingsHelper.Get<TokenVersion>(SettingsHelper.TokenVersion));
            SetRequestHeaders(Client);
        }

        public static void SetRequestHeaders(HttpClient client)
        {
            HttpRequestHeaders headers = client.DefaultRequestHeaders;

            headers.Clear();
            headers.Add("X-Sdk-Int", "36");
            headers.Add("X-Sdk-Locale", LanguageHelper.GetPrimaryLanguage());
            headers.Add("X-App-Mode", "universal");
            headers.Add("X-App-Channel", "coolapk");
            headers.Add("X-App-Id", "com.coolapk.market");
            headers.Add("X-App-Device", TokenCreator.DeviceCode);
            if (Window.Current != null)
            {
                headers.Add("X-Dark-Mode", ThemeHelper.IsDarkTheme() ? "1" : "0");
            }

            bool isCustomUA = SettingsHelper.Get<bool>(SettingsHelper.IsCustomUA);
            headers.UserAgent.ParseAdd((isCustomUA ? SettingsHelper.Get<UserAgent>(SettingsHelper.CustomUA) : UserAgent.Default).ToString());

            APIVersion version = TokenCreator.APIVersion;
            headers.UserAgent.ParseAdd($" {version}");
            headers.Add("X-App-Version", version.Version);
            headers.Add("X-Api-Supported", version.VersionCode.ToString());
            headers.Add("X-App-Code", version.VersionCode.ToString());
            headers.Add("X-Api-Version", version.MajorVersion);
        }

        public static void SetRequestHeaders(Windows.Web.Http.HttpClient client)
        {
            HttpRequestHeaderCollection headers = client.DefaultRequestHeaders;

            headers.Clear();
            headers.Add("X-Sdk-Int", "33");
            headers.Add("X-Sdk-Locale", LanguageHelper.GetPrimaryLanguage());
            headers.Add("X-App-Mode", "universal");
            headers.Add("X-App-Channel", "coolapk");
            headers.Add("X-App-Id", "com.coolapk.market");
            headers.Add("X-App-Device", TokenCreator.DeviceCode);
            if (Window.Current != null)
            {
                headers.Add("X-Dark-Mode", ThemeHelper.IsDarkTheme() ? "1" : "0");
            }

            bool isCustomUA = SettingsHelper.Get<bool>(SettingsHelper.IsCustomUA);
            headers.UserAgent.ParseAdd((isCustomUA ? SettingsHelper.Get<UserAgent>(SettingsHelper.CustomUA) : UserAgent.Default).ToString());

            APIVersion version = TokenCreator.APIVersion;
            headers.UserAgent.ParseAdd($" {version}");
            headers.Add("X-App-Version", version.Version);
            headers.Add("X-Api-Supported", version.VersionCode.ToString());
            headers.Add("X-App-Code", version.VersionCode.ToString());
            headers.Add("X-Api-Version", version.MajorVersion);
        }

        private static void ReplaceDarkMode(this HttpRequestHeaders headers, ApplicationTheme theme)
        {
            lock (darkModeLock)
            {
                const string name = "X-Dark-Mode";
                _ = headers.Remove(name);
                headers.Add(name, theme == ApplicationTheme.Dark ? "1" : "0");
            }
        }

        private static void ReplaceAppToken(this HttpRequestHeaders headers)
        {
            lock (appTokenLock)
            {
                const string name = "X-App-Token";
                _ = headers.Remove(name);
                headers.Add(name, TokenCreator.GetToken());
            }
        }

        private static void ReplaceRequested(this HttpRequestHeaders headers, string request)
        {
            lock (requestedLock)
            {
                const string name = "X-Requested-With";
                _ = headers.Remove(name);
                if (request != null) { headers.Add(name, request); }
            }
        }

        private static void ReplaceCoolapkCookie(this CookieContainer container, HttpCookieCollection cookies, Uri uri)
        {
            if (cookies == null) { return; }
            Uri host = GetHost(uri);
            foreach (HttpCookie cookie in cookies)
            {
                container.SetCookies(host, $"{cookie.Name}={cookie.Value}");
            }
        }

        private static void BeforeGetOrPost(HttpCookieCollection coolapkCookies, Uri uri, string request)
        {
            ClientHandler.CookieContainer.ReplaceCoolapkCookie(coolapkCookies, uri);
            HttpRequestHeaders headers = Client.DefaultRequestHeaders;
            headers.ReplaceAppToken();
            headers.ReplaceRequested(request);
        }
    }

    public static partial class NetworkHelper
    {
        public static async Task<string> PostAsync(Uri uri, HttpContent content, HttpCookieCollection coolapkCookies, bool isBackground)
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

        public static async Task<Stream> GetStreamAsync(Uri uri, HttpCookieCollection coolapkCookies, string request = "XMLHttpRequest", bool isBackground = false)
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

        public static async Task<string> GetStringAsync(Uri uri, HttpCookieCollection coolapkCookies, string request = "XMLHttpRequest", bool isBackground = false)
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
