using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace CoolapkUWP.Data
{
    static class Settings
    {
        public static string cookie = string.Empty;
        static ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        public static UISettings uISettings => new UISettings();
        public static bool HasStatusBar => Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar");
        public static double PageTitleHeight => HasStatusBar ? 48 : 80;
        public static SolidColorBrush SystemAccentColorBrush => Application.Current.Resources.ThemeDictionaries["SystemControlBackgroundAccentBrush"] as SolidColorBrush;
        public static Thickness titleTextMargin => new Thickness(5, 12, 5, 12);
        public static Thickness stackPanelMargin => new Thickness(0, PageTitleHeight, 0, 2);
        public static VerticalAlignment titleContentVerticalAlignment => VerticalAlignment.Bottom;
        public static bool GetBoolen(string key) => (bool)localSettings.Values[key];
        public static string GetString(string key) => localSettings.Values[key] as string;
        public static void Set(string key, object value) => localSettings.Values[key] = value;

        public static void InitializeSettings()
        {
            if (!localSettings.Values.ContainsKey("IsNoPicsMode"))
                localSettings.Values.Add("IsNoPicsMode", false);
            if (!localSettings.Values.ContainsKey("IsUseOldEmojiMode"))
                localSettings.Values.Add("IsUseOldEmojiMode", false);
            if (!localSettings.Values.ContainsKey("IsDarkMode"))
                localSettings.Values.Add("IsDarkMode", false);
            if (!localSettings.Values.ContainsKey("CheckUpdateWhenLuanching"))
                localSettings.Values.Add("CheckUpdateWhenLuanching", true);
            if (!localSettings.Values.ContainsKey("IsBackgroundColorFollowSystem"))
                localSettings.Values.Add("IsBackgroundColorFollowSystem", true);
            if (localSettings.Values.ContainsKey("UserName"))
            {
                localSettings.Values.Remove("Uid");
                localSettings.Values.Remove("UserName");
                localSettings.Values.Remove("UserAvatar");
            }
            if (!localSettings.Values.ContainsKey("Uid"))
                localSettings.Values.Add("Uid", string.Empty);
        }

        public static async void CheckUpdate()
        {
            try
            {
                Octokit.GitHubClient client = new Octokit.GitHubClient(new Octokit.ProductHeaderValue("Coolapk-UWP"));
                var release = await client.Repository.Release.GetLatest("Tangent-90", "Coolapk-UWP");
                var ver = release.TagName.Replace("v", string.Empty).Split('.');
                if (ushort.Parse(ver[0]) > Package.Current.Id.Version.Major
                    || (ushort.Parse(ver[0]) == Package.Current.Id.Version.Major && ushort.Parse(ver[1]) > Package.Current.Id.Version.Minor)
                    || (ushort.Parse(ver[0]) == Package.Current.Id.Version.Major && ushort.Parse(ver[1]) == Package.Current.Id.Version.Minor && ushort.Parse(ver[2]) > Package.Current.Id.Version.Build))
                {
                    var dialog = new Control.GetUpdateContentDialog(release.HtmlUrl, release.Body) { RequestedTheme = GetBoolen("IsDarkMode") ? ElementTheme.Dark : ElementTheme.Light };
                    await dialog.ShowAsync();
                }
                else Tools.ShowMessage("当前无可用更新。");
            }
            catch (System.Net.Http.HttpRequestException ex) { Tools.ShowHttpExceptionMessage(ex); }
        }

        public static async void CheckTheme()
        {
            InitializeSettings();
            while (Window.Current?.Content is null)
                await Task.Delay(100);
            if (Window.Current?.Content is FrameworkElement frameworkElement)
            {
                ElementTheme elementTheme = GetBoolen("IsDarkMode") ? ElementTheme.Dark : ElementTheme.Light;
                frameworkElement.RequestedTheme = elementTheme;
                ChangeColor(!GetBoolen("IsDarkMode"));
                foreach (var item in Tools.popups)
                    item.RequestedTheme = elementTheme;
            }
            void ChangeColor(bool value)//标题栏颜色
            {
                Color BackColor = value ? Color.FromArgb(255, 242, 242, 242) : Color.FromArgb(255, 23, 23, 23),
                      ForeColor = value ? Colors.Black : Colors.White,
                      ButtonForeInactiveColor = value ? Color.FromArgb(255, 50, 50, 50) : Color.FromArgb(255, 200, 200, 200),
                      ButtonBackPressedColor = value ? Color.FromArgb(255, 200, 200, 200) : Color.FromArgb(255, 50, 50, 50);
                if (HasStatusBar)
                {
                    StatusBar statusBar = StatusBar.GetForCurrentView();
                    statusBar.BackgroundOpacity = 1; // 透明度
                    statusBar.BackgroundColor = BackColor;
                    statusBar.ForegroundColor = ForeColor;
                }
                else
                {
                    var view = ApplicationView.GetForCurrentView().TitleBar;
                    view.ButtonBackgroundColor = view.InactiveBackgroundColor = view.ButtonInactiveBackgroundColor = Colors.Transparent;
                    view.ForegroundColor = view.ButtonForegroundColor = view.ButtonHoverForegroundColor = view.ButtonPressedForegroundColor = ForeColor;
                    view.InactiveForegroundColor = view.ButtonInactiveForegroundColor = ButtonForeInactiveColor;
                    view.ButtonHoverBackgroundColor = BackColor;
                    view.ButtonPressedBackgroundColor = ButtonBackPressedColor;
                }
            }
        }

        public static async Task<bool> CheckLoginInfo()
        {
            using (var filter = new Windows.Web.Http.Filters.HttpBaseProtocolFilter())
            {
                var cookieManager = filter.CookieManager;
                string uid = string.Empty, token = string.Empty, userName = string.Empty;
                foreach (var item in cookieManager.GetCookies(new Uri("http://coolapk.com")))
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
                if (!string.IsNullOrEmpty(uid) && !string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(userName))
                {
                    cookie = $"uid={uid}; username={userName}; token={token}";
                    Set("Uid", uid);
                    var o = Tools.GetJSonObject(await Tools.GetJson("/account/checkLoginInfo"));
                    Tools.notifications.Initial(o["notifyCount"].GetObject());
                    Tools.mainPage.UserAvatar = await ImageCache.GetImage(ImageType.BigAvatar, o["userAvatar"].GetString());
                    return true;
                }
                else return false;
            }
        }

        public static void Logout()
        {
            var cookieManager = new Windows.Web.Http.Filters.HttpBaseProtocolFilter().CookieManager;
            foreach (var item in cookieManager.GetCookies(new Uri("http://coolapk.com")))
                cookieManager.DeleteCookie(item);
            cookie = string.Empty;
            Set("Uid", string.Empty);
            Tools.mainPage.UserAvatar = null;
        }
    }
}
