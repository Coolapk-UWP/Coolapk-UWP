using CoolapkUWP.Common;
using CoolapkUWP.Models.Exceptions;
using CoolapkUWP.Models.Update;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
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
        private static readonly TimeSpan timeout = TimeSpan.FromTicks(863970000000 / 2);

        private static DateTimeOffset lastUpdate;

        public const string XMLHttpRequest = "XMLHttpRequest";

        public static readonly HttpClientHandler ClientHandler;
        public static readonly HttpClient Client;

        public static TokenCreator TokenCreator;

        static NetworkHelper()
        {
            ClientHandler = new HttpClientHandler { MaxConnectionsPerServer = 20 };
            Client = new HttpClient(ClientHandler);
            ThemeHelper.UISettingChanged += arg => Client.DefaultRequestHeaders.ReplaceDarkMode(arg);
            SettingsHelper.LoginChanged += (sender, arg) => ClientHandler.CookieContainer.ReplaceCoolapkCookie();
            SetRequestHeaders();
        }

        public static void SetRequestHeaders()
        {
            TokenCreator = new TokenCreator(SettingsHelper.Get<TokenVersion>(SettingsHelper.TokenVersion));
            SetRequestHeaders(Client, ClientHandler);
            Client.DefaultRequestHeaders.ReplaceAppToken(true);
        }

        public static void SetRequestHeaders(HttpClient client, HttpClientHandler handler = null)
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

            handler?.CookieContainer.ReplaceCoolapkCookie();
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

        private static HttpCookieCollection GetCoolapkCookies(Uri uri)
        {
            using (HttpBaseProtocolFilter filter = new HttpBaseProtocolFilter())
            {
                HttpCookieManager cookieManager = filter.CookieManager;
                return cookieManager.GetCookies(uri);
            }
        }

        private static void ReplaceDarkMode(this HttpRequestHeaders headers, ApplicationTheme theme)
        {
            const string name = "X-Dark-Mode";
            _ = headers.Remove(name);
            headers.Add(name, theme == ApplicationTheme.Dark ? "1" : "0");
        }

        private static void ReplaceAppToken(this HttpRequestHeaders headers, bool forces = false)
        {
            lock (appTokenLock)
            {
                DateTimeOffset now = DateTimeOffset.UtcNow;
                if (forces || now - lastUpdate > timeout)
                {
                    lastUpdate = now;
                    const string name = "X-App-Token";
                    _ = headers.Remove(name);
                    headers.Add(name, TokenCreator.GetToken());
                }
            }
        }

        private static void ReplaceRequested(this HttpRequestHeaders headers, string request)
        {
            const string name = "X-Requested-With";
            _ = headers.Remove(name);
            if (request != null) { headers.Add(name, request); }
        }

        private static void ReplaceCoolapkCookie(this CookieContainer container)
        {
            Uri host = new Uri("http://coolapk.com");
            foreach (Cookie cookie in container.GetCookies(host).OfType<Cookie>())
            {
                cookie.Expired = true;
            }
            HttpCookieCollection cookies = GetCoolapkCookies(host);
            foreach (HttpCookie cookie in cookies)
            {
                container.Add(
#if FEATURE2
                    host,
#endif
                    new Cookie(
                        cookie.Name,
                        cookie.Value,
                        cookie.Path,
                        cookie.Domain));
            }
        }
    }

    public static partial class NetworkHelper
    {
        private static readonly object requestedLock = new object();

        public static async Task<string> PostAsync(Uri uri, HttpContent content, bool isBackground)
        {
            try
            {
                HttpRequestHeaders headers = Client.DefaultRequestHeaders;
                headers.ReplaceAppToken();
                Task<HttpResponseMessage> task;
                lock (requestedLock)
                {
                    headers.ReplaceRequested(XMLHttpRequest);
                    task = Client.PostAsync(uri, content);
                }
                HttpResponseMessage response = await task.ConfigureAwait(false);
                return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
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

        public static async Task<HttpResponseMessage> GetAsync(Uri uri, string request = XMLHttpRequest, bool isBackground = false)
        {
            try
            {
                HttpRequestHeaders headers = Client.DefaultRequestHeaders;
                headers.ReplaceAppToken();
                Task<HttpResponseMessage> task;
                lock (requestedLock)
                {
                    headers.ReplaceRequested(request);
                    task = Client.GetAsync(uri);
                }
                return await task.ConfigureAwait(false);
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

        public static async Task<Stream> GetStreamAsync(Uri uri, string request = XMLHttpRequest, bool isBackground = false)
        {
            try
            {
                HttpRequestHeaders headers = Client.DefaultRequestHeaders;
                headers.ReplaceAppToken();
                Task<Stream> task;
                lock (requestedLock)
                {
                    headers.ReplaceRequested(request);
                    task = Client.GetStreamAsync(uri);
                }
                return await task.ConfigureAwait(false);
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

        public static async Task<string> GetStringAsync(Uri uri, string request = XMLHttpRequest, bool isBackground = false)
        {
            try
            {
                HttpRequestHeaders headers = Client.DefaultRequestHeaders;
                headers.ReplaceAppToken();
                Task<string> task;
                lock (requestedLock)
                {
                    headers.ReplaceRequested(request);
                    task = Client.GetStringAsync(uri);
                }
                return await task.ConfigureAwait(false);
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
