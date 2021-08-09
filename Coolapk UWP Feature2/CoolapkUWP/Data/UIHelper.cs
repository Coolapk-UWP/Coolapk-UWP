using CoolapkUWP.Control;
using CoolapkUWP.Pages.FeedPages;
using Html2Markdown;
using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Data.Json;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.System.Profile;
using Windows.System.UserProfile;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;

namespace CoolapkUWP.Data
{
    internal static class UIHelper
    {
        public static HttpClient mClient;
        public static NotificationsNum notifications = new NotificationsNum();
        public static Pages.MainPage mainPage = null;
        public static List<Popup> popups = new List<Popup>();
        private static readonly ObservableCollection<string> messageList = new ObservableCollection<string>();
        private static bool isShowingMessage;
        public static bool isShowingProgressBar;
        private static CoreDispatcher shellDispatcher;

        public static CoreDispatcher ShellDispatcher
        {
            get => shellDispatcher;
            set
            {
                if (shellDispatcher == null)
                {
                    shellDispatcher = value;
                }
            }
        }
        static UIHelper()
        {
            CultureInfo Culture = null;
            ulong version = ulong.Parse(AnalyticsInfo.VersionInfo.DeviceFamilyVersion);
            try { Culture = GlobalizationPreferences.Languages.Count > 0 ? new CultureInfo(GlobalizationPreferences.Languages.First()) : null; } catch { }
            EasClientDeviceInformation deviceInfo = new EasClientDeviceInformation();
            mClient = new HttpClient();
            mClient.DefaultRequestHeaders.Add("X-Sdk-Int", "30");
            mClient.DefaultRequestHeaders.Add("X-Sdk-Locale", "zh-CN");
            mClient.DefaultRequestHeaders.Add("X-App-Id", "com.coolapk.market");
            mClient.DefaultRequestHeaders.Add("X-Sdk-Locale", Culture == null ? "zh-CN" : Culture.ToString());
            mClient.DefaultRequestHeaders.Add("X-Dark-Mode", Windows.UI.Xaml.Application.Current.RequestedTheme.ToString() == "Dark" ? "1" : "0");
            mClient.DefaultRequestHeaders.UserAgent.ParseAdd("Dalvik/2.1.0 (Windows NT " + (ushort)((version & 0xFFFF000000000000L) >> 48) + "." + (ushort)((version & 0x0000FFFF00000000L) >> 32) + (Package.Current.Id.Architecture.ToString().Contains("64") ? "; Win64; " : "; Win32; ") + Package.Current.Id.Architecture.ToString().Replace("X", "x") + "; WebView/3.0) (#Build; " + deviceInfo.SystemManufacturer + "; " + deviceInfo.SystemProductName + "; CoolapkUWP " + Package.Current.Id.Version.ToFormattedString() + "; " + (ushort)((version & 0xFFFF000000000000L) >> 48) + "." + (ushort)((version & 0x0000FFFF00000000L) >> 32) + "." + (ushort)((version & 0x00000000FFFF0000L) >> 16) + "." + (ushort)(version & 0x000000000000FFFFL) + ")");
            mClient.DefaultRequestHeaders.UserAgent.ParseAdd(" +CoolMarket/9.2.2-1905301-universal");
            mClient.DefaultRequestHeaders.Add("X-App-Version", "9.2.2");
            mClient.DefaultRequestHeaders.Add("X-App-Code", "1905301");
            mClient.DefaultRequestHeaders.Add("X-Api-Version", "9");
            mClient.DefaultRequestHeaders.Add("X-App-Channel", "coolapk");
            mClient.DefaultRequestHeaders.Add("X-App-Mode", "universal");
            mClient.DefaultRequestHeaders.Add("X-Dark-Mode", SettingsHelper.GetBoolen("IsDarkMode") ? "1" : "0");
            mClient.DefaultRequestHeaders.Add("Cookie", SettingsHelper.cookie);
            Popup popup = new Popup { RequestedTheme = SettingsHelper.GetBoolen("IsDarkMode") ? ElementTheme.Dark : ElementTheme.Light };
            StatusGrid statusGrid2 = new StatusGrid();
            popup.Child = statusGrid2;
            popups.Add(popup);
            popup.IsOpen = true;
        }

        #region UI相关
        public static void ShowPopup(Popup popup)
        {
            popup.RequestedTheme = SettingsHelper.IsDarkTheme() ? ElementTheme.Dark : ElementTheme.Light;
            if (AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Desktop")
            { popups.Insert(popups.Count - 1, popup); }
            else { popups.Add(popup); }
            popup.IsOpen = true;
            popups.Last().IsOpen = false;
            popups.Last().IsOpen = true;
        }
        public static void Hide(this Popup popup)
        {
            popup.IsOpen = false;
            if (popups.Contains(popup)) { popups.Remove(popup); }
        }
        public static async void ShowProgressBar()
        {
            isShowingProgressBar = true;
            if (SettingsHelper.HasStatusBar)
            {
                StatusBar.GetForCurrentView().ProgressIndicator.ProgressValue = null;
                await StatusBar.GetForCurrentView().ProgressIndicator.ShowAsync();
            }
            else if (popups.Last().Child is StatusGrid statusGrid)
            { await statusGrid.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => statusGrid.ShowProgressBar()); }
        }
        public static async void PausedProgressBar()
        {
            if (!SettingsHelper.HasStatusBar && popups.Last().Child is StatusGrid statusGrid)
            { await statusGrid.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => statusGrid.PausedProgressBar()); }
        }
        public static async void ErrorProgressBar()
        {
            if (!SettingsHelper.HasStatusBar && popups.Last().Child is StatusGrid statusGrid)
            { await statusGrid.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => statusGrid.ErrorProgressBar()); }
        }
        public static async void HideProgressBar()
        {
            isShowingProgressBar = false;
            if (SettingsHelper.HasStatusBar && !isShowingMessage) { await StatusBar.GetForCurrentView().ProgressIndicator.HideAsync(); }
            else if (popups.Last().Child is StatusGrid statusGrid) { statusGrid.HideProgressBar(); }
        }
        public static async void ShowMessage(string message)
        {
            messageList.Add(message);
            if (!isShowingMessage)
            {
                isShowingMessage = true;
                while (messageList.Count > 0)
                {
                    string s = $"[{messageList.Count}]{messageList[0]}";
                    if (SettingsHelper.HasStatusBar)
                    {
                        StatusBar statusBar = StatusBar.GetForCurrentView();
                        statusBar.ProgressIndicator.Text = s;
                        if (isShowingProgressBar) { statusBar.ProgressIndicator.ProgressValue = null; }
                        else { statusBar.ProgressIndicator.ProgressValue = 0; }
                        await statusBar.ProgressIndicator.ShowAsync();
                        await Task.Delay(3000);
                        if (messageList.Count == 0 && !isShowingProgressBar) { await statusBar.ProgressIndicator.HideAsync(); }
                        statusBar.ProgressIndicator.Text = string.Empty;
                        messageList.RemoveAt(0);
                    }
                    else if (popups.Last().Child is StatusGrid statusGrid)
                    {
                        await statusGrid.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => statusGrid.ShowMessage(s));
                        await Task.Delay(3000);
                        messageList.RemoveAt(0);
                        await statusGrid.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            if (messageList.Count == 0) { statusGrid.Rectangle_PointerExited(); }
                            if (!isShowingProgressBar) { HideProgressBar(); }
                        });
                    }
                }
                isShowingMessage = false;
            }
        }

        public static void ShowHttpExceptionMessage(HttpRequestException e)
        {
            if (e.Message.IndexOfAny(new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' }) != -1)
            { ShowMessage($"服务器错误： {e.Message.Replace("Response status code does not indicate success: ", string.Empty)}"); }
            else if (e.Message == "An error occurred while sending the request.") { ShowMessage("无法连接网络。"); }
            else { ShowMessage($"请检查网络连接。 {e.Message}"); }
        }

        public static void ShowImage(string url, ImageType type)
        {
            Popup popup = new Popup();
            ShowImageControl control = new ShowImageControl(popup);
            control.ShowImage(url, type);
            popup.Child = control;
            ShowPopup(popup);
        }

        public static void ShowImages(string[] urls, int index)
        {
            Popup popup = new Popup();
            ShowImageControl control = new ShowImageControl(popup);
            control.ShowImages(urls, ImageType.SmallImage, index);
            popup.Child = control;
            ShowPopup(popup);
        }

        public static void Navigate(Type pageType, object e = null)
        {
            mainPage?.Frame.Navigate(pageType, e);
            HideProgressBar();
        }

        public static async void OpenLink(string str)
        {
            if (string.IsNullOrWhiteSpace(str)) { return; }
            if (str == "/contacts/fans")
            {
                Navigate(typeof(UserListPage), new object[] { SettingsHelper.GetString("Uid"), false, "我" });
                return;
            }
            if (str.Contains('?')) { str = str.Substring(0, str.IndexOf('?')); }
            if (str.Contains('%')) { str = str.Substring(0, str.IndexOf('%')); }
            if (str.Contains('&')) { str = str.Substring(0, str.IndexOf('&')); }
            if (str == "https://m.coolapk.com/mp/user/communitySpecification")
            {
                Navigate(typeof(Pages.BrowserPage), new object[] { false, str });
            }
            else if (str.IndexOf("/u/") == 0)
            {
                string u = str.Replace("/u/", string.Empty);
                if (int.TryParse(u, out _)) { Navigate(typeof(FeedListPage), new object[] { FeedListType.UserPageList, u }); }
                else { Navigate(typeof(FeedListPage), new object[] { FeedListType.UserPageList, await GetUserIDByName(u) }); }
                return;
            }
            else if (str.IndexOf("/feed/") == 0)
            {
                string u = str.Replace("/feed/", string.Empty);
                Navigate(typeof(FeedDetailPage), u);
                return;
            }
            else if (str.IndexOf("/t/") == 0)
            {
                string u = str.Replace("/t/", string.Empty);
                Navigate(typeof(FeedListPage), new object[] { FeedListType.TagPageList, u });
                return;
            }
            else if (str.IndexOf("/dyh/") == 0)
            {
                string u = str.Replace("/dyh/", string.Empty);
                Navigate(typeof(FeedListPage), new object[] { FeedListType.DYHPageList, u });
                return;
            }
            else if (str.IndexOf("/product/") == 0)
            {
                string u = str.Replace("/product/", string.Empty);
                Navigate(typeof(FeedListPage), new object[] { FeedListType.ProductPageList, u });
                return;
            }
            else if (str.IndexOf("/apk/") == 0 || str.IndexOf("/game/") == 0)
            {
                string u = "http://www.coolapk.com" + str;
                Navigate(typeof(Pages.AppPages.AppPage), u);
                return;
            }
            else if (str.IndexOf("https") == 0)
            {
                if (str.Contains("coolapk.com")) { OpenLink(str.Replace("https://www.coolapk.com", string.Empty)); }
                else { Navigate(typeof(Pages.BrowserPage), new object[] { false, str }); }
                return;
            }
            else if (str.IndexOf("http") == 0)
            {
                if (str.Contains("coolapk.com")) { OpenLink(str.Replace("http://www.coolapk.com", string.Empty)); }
                else { Navigate(typeof(Pages.BrowserPage), new object[] { false, str }); }
                return;
            }
        }
        #endregion

        //来源：https://blog.csdn.net/lindexi_gd/article/details/48951849
        public static string GetMD5(string inputString)
        {
            CryptographicHash objHash = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Md5).CreateHash();
            objHash.Append(CryptographicBuffer.ConvertStringToBinary(inputString, BinaryStringEncoding.Utf8));
            Windows.Storage.Streams.IBuffer buffHash1 = objHash.GetValueAndReset();
            return CryptographicBuffer.EncodeToHexString(buffHash1);
        }

        //https://github.com/ZCKun/CoolapkTokenCrack
        private static string GetCoolapkAppToken()
        {
            string DEVICE_ID = Guid.NewGuid().ToString();
            long UnixDate = (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000;
            string t = UnixDate.ToString();
            string hex_t = "0x" + string.Format("{0:x}", UnixDate);
            // 时间戳加密
            string md5_t = GetMD5(t);
            string a = "token://com.coolapk.market/c67ef5943784d09750dcfbb31020f0ab?" + md5_t + "$" + DEVICE_ID + "&com.coolapk.market";
            string md5_a = GetMD5(Convert.ToBase64String(Encoding.UTF8.GetBytes(a)));
            string token = md5_a + DEVICE_ID + hex_t;
            return token;
        }

        public static async Task<string> GetJson(string url, bool isBackground = false)
        {
            try
            {
                if (url != "/notification/checkCount") { _ = (notifications?.RefreshNotificationsNum()); }
                _ = mClient.DefaultRequestHeaders.Remove("X-App-Token");
                mClient.DefaultRequestHeaders.Add("X-App-Token", GetCoolapkAppToken());
                _ = mClient.DefaultRequestHeaders.Remove("Cookie");
                mClient.DefaultRequestHeaders.Add("Cookie", SettingsHelper.cookie);
                _ = mClient.DefaultRequestHeaders.Remove("X-Requested-With");
                mClient.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
                return await mClient.GetStringAsync(new Uri("https://api.coolapk.com/v6" + url));
            }
            catch (HttpRequestException e)
            {
                if (!isBackground) { ShowHttpExceptionMessage(e); }
                { return string.Empty; }
            }
            catch
            {
                if (isBackground) { return string.Empty; }
                else { throw; }
            }
        }

        public static async Task<string> GetHTML(string url, string Requested, bool isBackground = false)
        {
            try
            {
                if (url != "/notification/checkCount") { _ = (notifications?.RefreshNotificationsNum()); }
                _ = mClient.DefaultRequestHeaders.Remove("X-App-Token");
                mClient.DefaultRequestHeaders.Add("X-App-Token", GetCoolapkAppToken());
                _ = mClient.DefaultRequestHeaders.Remove("Cookie");
                mClient.DefaultRequestHeaders.Add("Cookie", SettingsHelper.cookie);
                _ = mClient.DefaultRequestHeaders.Remove("X-Requested-With");
                mClient.DefaultRequestHeaders.Add("X-Requested-With", Requested);
                return await mClient.GetStringAsync(new Uri(url));
            }
            catch (HttpRequestException e)
            {
                if (!isBackground) { ShowHttpExceptionMessage(e); }
                return string.Empty;
            }
            catch
            {
                if (isBackground) { return string.Empty; }
                else { throw; }
            }
        }

        public static async Task<bool> Post(string url, HttpContent content)
        {
            try
            {
                if (url != "/notification/checkCount") { notifications?.RefreshNotificationsNum(); }
                _ = mClient.DefaultRequestHeaders.Remove("X-App-Token");
                mClient.DefaultRequestHeaders.Add("X-App-Token", GetCoolapkAppToken());
                _ = mClient.DefaultRequestHeaders.Remove("Cookie");
                mClient.DefaultRequestHeaders.Add("Cookie", SettingsHelper.cookie);
                _ = mClient.DefaultRequestHeaders.Remove("X-Requested-With");
                mClient.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
                HttpResponseMessage a = await mClient.PostAsync(new Uri("https://api.coolapk.com/v6" + url), content);
                return !(GetJSonObject(await a.Content.ReadAsStringAsync()) is null);
            }
            catch (HttpRequestException e)
            {
                ShowHttpExceptionMessage(e);
                return false;
            }
            catch { throw; }

        }

        public static JsonObject GetJSonObject(string json)
        {
            try
            {
                return string.IsNullOrEmpty(json) ? null : JsonObject.Parse(json)["data"].GetObject();
            }
            catch
            {
                if (JsonObject.Parse(json).TryGetValue("message", out IJsonValue value))
                { ShowMessage($"{value.GetString()}"); }
                return null;
            }
        }

        public static string GetObjectStrigInJson(string json)
        {
            try
            {
                return string.IsNullOrEmpty(json) ? null : JsonObject.Parse(json)["data"].ToString().Replace("\"", string.Empty);
            }
            catch
            {
                if (JsonObject.Parse(json).TryGetValue("message", out IJsonValue value))
                { ShowMessage($"{value.GetString()}"); }
                return string.Empty;
            }
        }

        public static JsonArray GetDataArray(string json)
        {
            try
            {
                return string.IsNullOrEmpty(json) ? null : JsonObject.Parse(json)["data"].GetArray();
            }
            catch
            {
                if (JsonObject.Parse(json).TryGetValue("message", out IJsonValue value))
                { ShowMessage($"{value.GetString()}"); }
                return null;
            }
        }


        public static string ConvertTime(double timestr)
        {
            DateTime time = new DateTime(1970, 1, 1).ToLocalTime().Add(new TimeSpan(Convert.ToInt64(timestr) * 10000000));
            TimeSpan temptime = DateTime.Now.Subtract(time);
            return temptime.TotalDays > 30
                ? $"{time.Year}/{time.Month}/{time.Day}"
                : temptime.Days > 0
                ? $"{temptime.Days}天前"
                : temptime.Hours > 0 ? $"{temptime.Hours}小时前" : temptime.Minutes > 0 ? $"{temptime.Minutes}分钟前" : "刚刚";
        }

        public static string ReplaceHtml(string str)
        {
            //换行和段落
            string s = str.Replace("<br>", "\n").Replace("<br>", "\n").Replace("<br/>", "\n").Replace("<br/>", "\n").Replace("<p>", "").Replace("</p>", "\n").Replace("&nbsp;", " ").Replace("<br />", "").Replace("<br />", "");
            //链接彻底删除！
            Regex a = new Regex("<a.*?>", RegexOptions.IgnoreCase);
            Regex a1 = new Regex("<a.*?/>", RegexOptions.IgnoreCase);
            Regex a2 = new Regex("</a.*?>", RegexOptions.IgnoreCase);
            s = a.Replace(s, "");
            s = a1.Replace(s, "");
            s = a2.Replace(s, "");
            return s;
        }

        public static async Task<string> GetUserIDByName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                ShowMessage("请输入用户名");
                return "0";
            }

            try
            {
                string uid = await new HttpClient().GetStringAsync("https://www.coolapk.com/n/" + name);
                uid = uid.Split(new string[] { "coolmarket://www.coolapk.com/u/" }, StringSplitOptions.RemoveEmptyEntries)[1];
                uid = uid.Split(new string[] { @"""" }, StringSplitOptions.RemoveEmptyEntries)[0];
                return uid;
            }
            catch (HttpRequestException e)
            {
                if (e.Message.Contains("404")) { ShowMessage("用户不存在"); }
                else { ShowHttpExceptionMessage(e); }
                return "0";
            }
        }

        public static string GetSizeString(double size)
        {
            int index = 0;
            while (true)
            {
                index++;
                size /= 1024;
                if (size > 0.7 && size < 716.8) { break; }
                else if (size >= 716.8) { continue; }
                else if (size <= 0.7)
                {
                    size *= 1024;
                    index--;
                    break;
                }
            }
            string str = string.Empty;
            switch (index)
            {
                case 0: str = "B"; break;
                case 1: str = "KB"; break;
                case 2: str = "MB"; break;
                case 3: str = "GB"; break;
                case 4: str = "TB"; break;
                default:
                    break;
            }
            return $"{size:N2}{str}";
        }

        public static string GetNumString(double num)
        {
            string str = string.Empty;
            if (num < 1000) { }
            else if (num < 10000)
            {
                str = "k";
                num /= 1000;
            }
            else if (num < 10000000)
            {
                str = "w";
                num /= 10000;
            }
            else
            {
                str = "kw";
                num /= 10000000;
            }
            return $"{num:N2}{str}";
        }


        public static string GetValue(IJsonValue json)
        {
            string str = json.ToString();
            if (str.StartsWith("\""))
            {
                str = str.Substring(1);
            }
            if (str.EndsWith("\""))
            {
                str = str.Remove(str.Length - 1);
            }
            return str;
        }

        public static string CSStoMarkDown(string text)
        {
            try
            {
                Converter converter = new Converter();
                return converter.Convert(text);
            }
            catch
            {
                Regex h1 = new Regex(@"<h1.*?>", RegexOptions.IgnoreCase);
                Regex h2 = new Regex(@"<h2.*?>", RegexOptions.IgnoreCase);
                Regex h3 = new Regex(@"<h3.*?>", RegexOptions.IgnoreCase);
                Regex h4 = new Regex(@"<h4.*?>\n", RegexOptions.IgnoreCase);
                Regex div = new Regex(@"<div.*?>", RegexOptions.IgnoreCase);
                Regex p = new Regex(@"<p.*?>", RegexOptions.IgnoreCase);
                Regex ul = new Regex(@"<ul.*?>", RegexOptions.IgnoreCase);
                Regex li = new Regex(@"<li.*?>", RegexOptions.IgnoreCase);
                Regex span = new Regex(@"<span.*?>", RegexOptions.IgnoreCase);

                text = text.Replace("</h1>", "");
                text = text.Replace("</h2>", "");
                text = text.Replace("</h3>", "");
                text = text.Replace("</h4>", "");
                text = text.Replace("</div>", "");
                text = text.Replace("<p>", "");
                text = text.Replace("</p>", "");
                text = text.Replace("</ul>", "");
                text = text.Replace("</li>", "");
                text = text.Replace("</span>", "**");
                text = text.Replace("</strong>", "**");

                text = h1.Replace(text, "#");
                text = h2.Replace(text, "##");
                text = h3.Replace(text, "###");
                text = h4.Replace(text, "####");
                text = text.Replace("<br/>", "  \n");
                text = text.Replace("<br />", "  \n");
                text = div.Replace(text, "");
                text = p.Replace(text, "");
                text = ul.Replace(text, "");
                text = li.Replace(text, " - ");
                text = span.Replace(text, "**");
                text = text.Replace("<strong>", "**");

                for (int i = 0; i < 20; i++) { text = text.Replace("(" + i.ToString() + ") ", " 1. "); }

                return text;
            }
        }
    }
}