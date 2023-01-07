using CoolapkUWP.Common;
using CoolapkUWP.Models.Exceptions;
using Microsoft.Toolkit.Uwp.Helpers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.System.Profile;
using Windows.System.UserProfile;
using Windows.Web.Http;
using Windows.Web.Http.Filters;
using HttpClient = System.Net.Http.HttpClient;
using HttpResponseMessage = System.Net.Http.HttpResponseMessage;
using HttpStatusCode = System.Net.HttpStatusCode;

namespace CoolapkUWP.Helpers
{
    public static partial class NetworkHelper
    {
        public static readonly HttpClientHandler ClientHandler = new HttpClientHandler();
        public static readonly HttpClient Client = new HttpClient(ClientHandler);
        private static TokenCreater token;

        static NetworkHelper()
        {
            SetLoginCookie();
            SetRequestHeaders();
            ThemeHelper.UISettingChanged.Add((arg) => Client.DefaultRequestHeaders.ReplaceDarkMode());
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
            }
        }

        public static void SetRequestHeaders()
        {
            EasClientDeviceInformation deviceInfo = new EasClientDeviceInformation();
            string APIVersion = SettingsHelper.Get<string>(SettingsHelper.APIVersion);
            ulong OSVersion = ulong.Parse(AnalyticsInfo.VersionInfo.DeviceFamilyVersion);
            TokenVersion TokenVersion = SettingsHelper.Get<TokenVersion>(SettingsHelper.TokenVersion);
            string Culture = GlobalizationPreferences.Languages.Any() ? new CultureInfo(GlobalizationPreferences.Languages.FirstOrDefault()).ToString() : "zh-CN";

            token = new TokenCreater(TokenVersion);
            Client.DefaultRequestHeaders.Clear();
            Client.DefaultRequestHeaders.Add("X-Sdk-Int", "30");
            Client.DefaultRequestHeaders.Add("X-Sdk-Locale", Culture);
            Client.DefaultRequestHeaders.Add("X-App-Mode", "universal");
            Client.DefaultRequestHeaders.Add("X-App-Channel", "coolapk");
            Client.DefaultRequestHeaders.Add("X-App-Id", "com.coolapk.market");
            Client.DefaultRequestHeaders.Add("X-App-Device", TokenCreater.DeviceCode);
            Client.DefaultRequestHeaders.Add("X-Dark-Mode", ThemeHelper.IsDarkTheme() ? "1" : "0");
            Client.DefaultRequestHeaders.UserAgent.ParseAdd($"Dalvik/2.1.0 (Windows NT {(ushort)((OSVersion & 0xFFFF000000000000L) >> 48)}.{(ushort)((OSVersion & 0x0000FFFF00000000L) >> 32)}; Win32; {Package.Current.Id.Architecture.ToString().Replace("X", "x")}; WebView/3.0) (#Build; {deviceInfo.SystemManufacturer}; {deviceInfo.SystemProductName}; CoolapkUWP {Package.Current.Id.Version.ToFormattedString()}; {(ushort)((OSVersion & 0xFFFF000000000000L) >> 48)}.{(ushort)((OSVersion & 0x0000FFFF00000000L) >> 32)}.{(ushort)((OSVersion & 0x00000000FFFF0000L) >> 16)}.{(ushort)(OSVersion & 0x000000000000FFFFL)})");

            switch (APIVersion)
            {
                case "V6":
                    Client.DefaultRequestHeaders.UserAgent.ParseAdd(" +CoolMarket/6.10.6-1608291-universal");
                    Client.DefaultRequestHeaders.Add("X-App-Version", "6.10.6");
                    Client.DefaultRequestHeaders.Add("X-App-Code", "1608291");
                    break;
                case "V7":
                    Client.DefaultRequestHeaders.UserAgent.ParseAdd(" +CoolMarket/7.9.6_S-1710201-universal");
                    Client.DefaultRequestHeaders.Add("X-App-Version", "7.9.6_S");
                    Client.DefaultRequestHeaders.Add("X-App-Code", "1710201");
                    Client.DefaultRequestHeaders.Add("X-Api-Version", "7");
                    break;
                case "V8":
                    Client.DefaultRequestHeaders.UserAgent.ParseAdd(" +CoolMarket/8.7-1809041-universal");
                    Client.DefaultRequestHeaders.Add("X-App-Version", "8.7");
                    Client.DefaultRequestHeaders.Add("X-App-Code", "1809041");
                    Client.DefaultRequestHeaders.Add("X-Api-Version", "8");
                    break;
                case "V9":
                    Client.DefaultRequestHeaders.UserAgent.ParseAdd(" +CoolMarket/9.6.3-1910291-universal");
                    Client.DefaultRequestHeaders.Add("X-App-Version", "9.6.3");
                    Client.DefaultRequestHeaders.Add("X-App-Code", "1910291");
                    Client.DefaultRequestHeaders.Add("X-Api-Version", "9");
                    break;
                case "小程序":
                    Client.DefaultRequestHeaders.UserAgent.ParseAdd(" +CoolMarket/1.0-1902250-universal");
                    Client.DefaultRequestHeaders.Add("X-App-Version", "1.0");
                    Client.DefaultRequestHeaders.Add("X-App-Code", "1902250");
                    Client.DefaultRequestHeaders.Add("X-Api-Version", "9");
                    break;
                case "V10":
                    Client.DefaultRequestHeaders.UserAgent.ParseAdd(" +CoolMarket/10.5.3-2009271-universal");
                    Client.DefaultRequestHeaders.Add("X-App-Version", "10.5.3");
                    Client.DefaultRequestHeaders.Add("X-App-Code", "2009271");
                    Client.DefaultRequestHeaders.Add("X-Api-Version", "10");
                    break;
                case "V11":
                    Client.DefaultRequestHeaders.UserAgent.ParseAdd(" +CoolMarket/11.4.7-2112231-universal");
                    Client.DefaultRequestHeaders.Add("X-App-Version", "11.4.7");
                    Client.DefaultRequestHeaders.Add("X-App-Code", "2112231");
                    Client.DefaultRequestHeaders.Add("X-Api-Version", "11");
                    break;
                case "V12":
                    Client.DefaultRequestHeaders.UserAgent.ParseAdd(" +CoolMarket/12.5.4-2212261-universal");
                    Client.DefaultRequestHeaders.Add("X-App-Version", "12.5.4");
                    Client.DefaultRequestHeaders.Add("X-Api-Supported", "2212261");
                    Client.DefaultRequestHeaders.Add("X-App-Code", "2212261");
                    Client.DefaultRequestHeaders.Add("X-Api-Version", "12");
                    break;
                case "V13":
                default:
                    Client.DefaultRequestHeaders.UserAgent.ParseAdd(" +CoolMarket/13.0-beta4-2301042-universal");
                    Client.DefaultRequestHeaders.Add("X-App-Version", "13.0-beta4");
                    Client.DefaultRequestHeaders.Add("X-Api-Supported", "2301042");
                    Client.DefaultRequestHeaders.Add("X-App-Code", "2301042");
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

        private static void ReplaceDarkMode(this System.Net.Http.Headers.HttpRequestHeaders headers)
        {
            const string name = "X-Dark-Mode";
            _ = headers.Remove(name);
            headers.Add(name, ThemeHelper.IsDarkTheme() ? "1" : "0");
        }

        private static void ReplaceAppToken(this System.Net.Http.Headers.HttpRequestHeaders headers)
        {
            const string name = "X-App-Token";
            _ = headers.Remove(name);
            headers.Add(name, token.GetToken());
        }

        private static void ReplaceRequested(this System.Net.Http.Headers.HttpRequestHeaders headers, string request)
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
                if (!isBackground) { UIHelper.ShowHttpExceptionMessage(e); }
                return null;
            }
            catch
            {
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
                if (!isBackground) { UIHelper.ShowHttpExceptionMessage(e); }
                return null;
            }
            catch
            {
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
                if (!isBackground) { UIHelper.ShowHttpExceptionMessage(e); }
                return null;
            }
            catch
            {
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
        /// <returns>用户信息</returns>
        public static async Task<(string UID, string UserName, string UserAvatar)> GetUserInfoByNameAsync(string name)
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
                UIHelper.ShowHttpExceptionMessage(e);
                return result;
            }
            catch (Exception ex)
            {
                if (string.IsNullOrWhiteSpace(str)) { throw ex; }
                JObject o = JObject.Parse(str);
                if (o == null) { throw ex; }
                else { throw new CoolapkMessageException(o); }
            }
        }

        public static Uri GetHost(Uri uri)
        {
            return new Uri("https://" + uri.Host);
        }

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

        public static Uri ValidateAndGetUri(this string uriString)
        {
            Uri uri = null;
            try
            {
                uri = new Uri(uriString);
            }
            catch (FormatException)
            {
            }
            return uri;
        }
    }
}
