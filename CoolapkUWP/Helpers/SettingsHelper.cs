using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace CoolapkUWP.Helpers
{
    static class SettingsHelper
    {
        static readonly ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        public static UISettings UISettings => new UISettings();
        public static bool HasStatusBar => Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar");
        public static double PageTitleHeight => HasStatusBar ? 48 : 80;
        public static SolidColorBrush SystemAccentColorBrush => Application.Current.Resources.ThemeDictionaries["SystemControlBackgroundAccentBrush"] as SolidColorBrush;
        public static Thickness TitleTextMargin => new Thickness(5, 12, 5, 12);
        public static Thickness StackPanelMargin => new Thickness(0, PageTitleHeight, 0, 0);
        public static Thickness IndexPageStackPanelMargin => new Thickness(0, HasStatusBar ? 0 : 48, 0, 0);
        public static VerticalAlignment TitleContentVerticalAlignment => VerticalAlignment.Bottom;
        public static ElementTheme Theme => Get<bool>("IsBackgroundColorFollowSystem") ? ElementTheme.Default : (Get<bool>("IsDarkMode") ? ElementTheme.Dark : ElementTheme.Light);
        public static T Get<T>(string key) => (T)localSettings.Values[key];
        public static void Set(string key, object value) => localSettings.Values[key] = value;

        static SettingsHelper()
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
            if (!localSettings.Values.ContainsKey("DefaultFollowPageIndex"))
                localSettings.Values.Add("DefaultFollowPageIndex", 0);
            if (localSettings.Values.ContainsKey("UserName"))
            {
                localSettings.Values.Remove("Uid");
                localSettings.Values.Remove("UserName");
                localSettings.Values.Remove("UserAvatar");
            }
            if (!localSettings.Values.ContainsKey("Uid"))
                localSettings.Values.Add("Uid", string.Empty);
            CheckTheme();
        }

        public static async void CheckUpdate()
        {
            try
            {
                UIHelper.ShowProgressBar();
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0;)");
                    var keys = JObject.Parse(await client.GetStringAsync("https://api.github.com/repos/Tangent-90/Coolapk-UWP/releases/latest"));
                    var ver = keys.Value<string>("tag_name").Replace("v", string.Empty).Split('.');
                    if (ushort.Parse(ver[0]) > Package.Current.Id.Version.Major
                        || (ushort.Parse(ver[0]) == Package.Current.Id.Version.Major && ushort.Parse(ver[1]) > Package.Current.Id.Version.Minor)
                        || (ushort.Parse(ver[0]) == Package.Current.Id.Version.Major && ushort.Parse(ver[1]) == Package.Current.Id.Version.Minor && ushort.Parse(ver[2]) > Package.Current.Id.Version.Build))
                    {
                        var dialog = new Controls.GetUpdateContentDialog(keys.Value<string>("html_url"), keys.Value<string>("body")) { RequestedTheme = Theme }; await dialog.ShowAsync();
                    }
                    else UIHelper.ShowMessage("当前无可用更新。");
                }
            }
            catch (HttpRequestException) { UIHelper.ShowMessage("网络异常"); }
            finally { UIHelper.HideProgressBar(); }
        }

        public static async void CheckTheme()
        {
            while (Window.Current?.Content is null)
                await Task.Delay(100);
            if (Window.Current.Content is FrameworkElement frameworkElement)
            {
                frameworkElement.RequestedTheme = Theme;
                foreach (var item in UIHelper.popups)
                    item.RequestedTheme = Theme;

                Color? BackColor, ForeColor, ButtonForeInactiveColor, ButtonBackPressedColor;
                BackColor = ForeColor = ButtonBackPressedColor = ButtonForeInactiveColor = null;
                switch (Theme)
                {
                    case ElementTheme.Light:
                        BackColor = Color.FromArgb(255, 242, 242, 242);
                        ForeColor = Colors.Black;
                        ButtonForeInactiveColor = Color.FromArgb(255, 50, 50, 50);
                        ButtonBackPressedColor = Color.FromArgb(255, 200, 200, 200);
                        break;
                    case ElementTheme.Dark:
                        BackColor = Color.FromArgb(255, 23, 23, 23);
                        ForeColor = Colors.White;
                        ButtonForeInactiveColor = Color.FromArgb(255, 200, 200, 200);
                        ButtonBackPressedColor = Color.FromArgb(255, 50, 50, 50);
                        break;
                }
                if (HasStatusBar)
                {
                    StatusBar statusBar = StatusBar.GetForCurrentView();
                    statusBar.BackgroundOpacity = 0; // 透明度
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
            try
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
                        Set("Uid", uid);
                        var o = (JObject)await DataHelper.GetData(DataUriType.CheckLoginInfo);
                        UIHelper.NotificationNums.Initial((JObject)o["notifyCount"]);
                        UIHelper.MainPageUserAvatar = await ImageCacheHelper.GetImage(ImageType.BigAvatar, o.Value<string>("userAvatar"));
                        return true;
                    }
                    else return false;
                }
            }
            catch { throw; }
        }

        public static void Logout()
        {
            var cookieManager = new Windows.Web.Http.Filters.HttpBaseProtocolFilter().CookieManager;
            foreach (var item in cookieManager.GetCookies(new Uri("http://coolapk.com")))
                cookieManager.DeleteCookie(item);
            Set("Uid", string.Empty);
            UIHelper.MainPageUserAvatar = null;
            UIHelper.NotificationNums.ClearNums();
        }
    }
}
