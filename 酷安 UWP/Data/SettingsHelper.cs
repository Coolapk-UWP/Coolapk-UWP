using CoolapkUWP.Control;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace CoolapkUWP.Data
{
    internal static class SettingsHelper
    {
        public static string cookie = string.Empty;
        private static readonly ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        public static UISettings uISettings => new UISettings();
        public static bool HasStatusBar => Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar");
        public static double PageTitleHeight => HasStatusBar ? 48 : 80;
        public static SolidColorBrush SystemAccentColorBrush => Windows.UI.Xaml.Application.Current.Resources.ThemeDictionaries["SystemControlBackgroundAccentBrush"] as SolidColorBrush;
        public static Thickness stackPanelMargin => new Thickness(0, PageTitleHeight, 0, 2);
        public static Thickness ButtonMargin => new Thickness(0, PageTitleHeight - 48, 0, 2);
        public static VerticalAlignment titleContentVerticalAlignment => VerticalAlignment.Bottom;
        public static ElementTheme theme => GetBoolen("IsBackgroundColorFollowSystem") ? ElementTheme.Default : (GetBoolen("IsDarkMode") ? ElementTheme.Dark : ElementTheme.Light);
        public static bool GetBoolen(string key) => (bool)localSettings.Values[key];
        public static string GetString(string key) => localSettings.Values[key] as string;
        public static void Set(string key, object value) => localSettings.Values[key] = value;
        public static bool IsAuthor => ApplicationData.Current.LocalSettings.Values["IsAuthor"] != null && (bool)ApplicationData.Current.LocalSettings.Values["IsAuthor"];
        public static bool IsSpecialUser => (ApplicationData.Current.LocalSettings.Values["IsAuthor"] != null && (bool)ApplicationData.Current.LocalSettings.Values["IsAuthor"]) || (ApplicationData.Current.LocalSettings.Values["IsSpecial"] != null && (bool)ApplicationData.Current.LocalSettings.Values["IsSpecial"]);

        static SettingsHelper()
        {
            if (!localSettings.Values.ContainsKey("IsNoPicsMode"))
            { localSettings.Values.Add("IsNoPicsMode", false); }
            if (!localSettings.Values.ContainsKey("IsUseOldEmojiMode"))
            { localSettings.Values.Add("IsUseOldEmojiMode", false); }
            if (!localSettings.Values.ContainsKey("IsDarkMode"))
            { localSettings.Values.Add("IsDarkMode", false); }
            if (!localSettings.Values.ContainsKey("CheckUpdateWhenLuanching"))
            { localSettings.Values.Add("CheckUpdateWhenLuanching", true); }
            if (!localSettings.Values.ContainsKey("IsBackgroundColorFollowSystem"))
            { localSettings.Values.Add("IsBackgroundColorFollowSystem", true); }
            if (localSettings.Values.ContainsKey("UserName"))
            {
                _ = localSettings.Values.Remove("Uid");
                _ = localSettings.Values.Remove("UserName");
                _ = localSettings.Values.Remove("UserAvatar");
            }
            if (!localSettings.Values.ContainsKey("Uid"))
            { localSettings.Values.Add("Uid", string.Empty); }
            CheckTheme();
        }

        private static bool IsDarkTheme()
        {
            if (theme == ElementTheme.Default)
            {
                return Application.Current.RequestedTheme == ApplicationTheme.Dark;
            }
            return theme == ElementTheme.Dark;
        }

        public static async void CheckUpdate()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0;)");
                    Windows.Data.Json.JsonObject keys;
                    try { keys = Windows.Data.Json.JsonObject.Parse(await client.GetStringAsync("https://api.github.com/repos/Tangent-90/Coolapk-UWP/releases/latest")); }
                    catch { keys = Windows.Data.Json.JsonObject.Parse(await client.GetStringAsync("https://v2.kkpp.cc/repos/Tangent-90/Coolapk-UWP/releases/latest")); }
                    string[] ver = keys["tag_name"].GetString().Replace("v", string.Empty).Split('.');
                    if (ushort.Parse(ver[0]) > Package.Current.Id.Version.Major
                        || (ushort.Parse(ver[0]) == Package.Current.Id.Version.Major && ushort.Parse(ver[1]) > Package.Current.Id.Version.Minor)
                        || (ushort.Parse(ver[0]) == Package.Current.Id.Version.Major && ushort.Parse(ver[1]) == Package.Current.Id.Version.Minor && ushort.Parse(ver[2]) > Package.Current.Id.Version.Build))
                    {
                        Control.GetUpdateContentDialog dialog = new Control.GetUpdateContentDialog(keys["html_url"].GetString(), keys["body"].GetString()) { RequestedTheme = theme };
                        _ = await dialog.ShowAsync();
                    }
                    else { UIHelper.ShowMessage("当前无可用更新。"); }
                }
            }
            catch (HttpRequestException ex) { UIHelper.ShowHttpExceptionMessage(ex); }
        }

        public static async void CheckTheme()
        {
            while (Window.Current?.Content is null)
            { await Task.Delay(100); }
            if (Window.Current.Content is FrameworkElement frameworkElement)
            {
                frameworkElement.RequestedTheme = theme;
                foreach (Windows.UI.Xaml.Controls.Primitives.Popup item in UIHelper.popups)
                { item.RequestedTheme = theme; }

                bool IsDark = IsDarkTheme();
                SolidColorBrush AccentColor = (SolidColorBrush)Windows.UI.Xaml.Application.Current.Resources["SystemControlBackgroundAccentBrush"];

                if (HasStatusBar)
                {
                    if (IsDark)
                    {
                        StatusBar statusBar = StatusBar.GetForCurrentView();
                        statusBar.BackgroundColor = AccentColor.Color;
                        statusBar.ForegroundColor = Colors.White;
                        statusBar.BackgroundOpacity = 0; // 透明度
                    }
                    else
                    {
                        StatusBar statusBar = StatusBar.GetForCurrentView();
                        statusBar.BackgroundColor = AccentColor.Color;
                        statusBar.ForegroundColor = Colors.Black;
                        statusBar.BackgroundOpacity = 0; // 透明度
                    }
                }
                else if (IsDark)
                {
                    ApplicationViewTitleBar view = ApplicationView.GetForCurrentView().TitleBar;
                    view.ButtonBackgroundColor = view.InactiveBackgroundColor = view.ButtonInactiveBackgroundColor = Colors.Transparent;
                    view.ButtonForegroundColor = Colors.White;
                }
                else
                {
                    ApplicationViewTitleBar view = ApplicationView.GetForCurrentView().TitleBar;
                    view.ButtonBackgroundColor = view.InactiveBackgroundColor = view.ButtonInactiveBackgroundColor = Colors.Transparent;
                    view.ButtonForegroundColor = Colors.Black;
                }
            }
        }

        public static async Task<bool> CheckLoginInfo()
        {
            using (Windows.Web.Http.Filters.HttpBaseProtocolFilter filter = new Windows.Web.Http.Filters.HttpBaseProtocolFilter())
            {
                Windows.Web.Http.HttpCookieManager cookieManager = filter.CookieManager;
                string uid = string.Empty, token = string.Empty, userName = string.Empty;
                foreach (Windows.Web.Http.HttpCookie item in cookieManager.GetCookies(new Uri("http://coolapk.com")))
                {
                    switch (item.Name)
                    {
                        case "uid":
                            uid = item.Value;
                            break;
                        case "username":
                            userName = item.Value;
                            break;
                        case "token":
                            token = item.Value;
                            break;
                    }
                }
                if (!string.IsNullOrEmpty(uid) && !string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(userName))
                {
                    cookie = $"uid={uid}; username={userName}; token={token}";
                    Set("Uid", uid);
                    Windows.Data.Json.JsonObject o = UIHelper.GetJSonObject(await UIHelper.GetJson("/account/checkLoginInfo"));
                    UIHelper.notifications.Initial(o);
                    UIHelper.mainPage.UserAvatar = await ImageCache.GetImage(ImageType.BigAvatar, o["userAvatar"].GetString());
                    UIHelper.mainPage.UserNames = o["username"].GetString();
                    return true;
                }
                else { return false; }
            }
        }

        public static void Logout()
        {
            Windows.Web.Http.HttpCookieManager cookieManager = new Windows.Web.Http.Filters.HttpBaseProtocolFilter().CookieManager;
            foreach (Windows.Web.Http.HttpCookie item in cookieManager.GetCookies(new Uri("http://coolapk.com")))
            { cookieManager.DeleteCookie(item); }
            cookie = string.Empty;
            Set("Uid", string.Empty);
            UIHelper.mainPage.UserAvatar = null;
            UIHelper.mainPage.UserNames = "登录";
        }
    }
}
