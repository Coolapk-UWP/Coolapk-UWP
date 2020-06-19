using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace CoolapkUWP.Helpers
{
    internal static class SettingsHelper
    {
        [Obsolete]
        public const string DefaultFollowPageIndex        = "DefaultFollowPageIndex";
        public const string IsNoPicsMode                  = "IsNoPicsMode";
        public const string IsUseOldEmojiMode             = "IsUseOldEmojiMode";
        public const string IsDarkMode                    = "IsDarkMode";
        public const string CheckUpdateWhenLuanching      = "CheckUpdateWhenLuanching";
        public const string IsBackgroundColorFollowSystem = "IsBackgroundColorFollowSystem";
        public const string UserName                      = "UserName";
        public const string Uid                           = "Uid";
        public const string UserAvatar                    = "UserAvatar";
        public const string IsDisplayOriginPicture        = "IsDisplayOriginPicture";

        private static readonly ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        public static readonly MetroLog.ILogManager logManager         = MetroLog.LogManagerFactory.CreateLogManager();
        public static readonly UISettings uiSettings                   = new UISettings();
        public static ElementTheme Theme                               => Get<bool>("IsBackgroundColorFollowSystem") ? ElementTheme.Default : (Get<bool>("IsDarkMode") ? ElementTheme.Dark : ElementTheme.Light);


        public static T Get<T>(string key) => (T)localSettings.Values[key];

        public static void Set(string key, object value) => localSettings.Values[key] = value;

        static SettingsHelper()
        {
            SetDefaultSettings();
            CheckTheme();
        }

        public static void SetDefaultSettings()
        {
#pragma warning disable CS0612 // 类型或成员已过时
            if (localSettings.Values.ContainsKey(DefaultFollowPageIndex))
                localSettings.Values.Remove(DefaultFollowPageIndex);
#pragma warning restore CS0612

            if (!localSettings.Values.ContainsKey(IsNoPicsMode))
                localSettings.Values.Add(IsNoPicsMode, false);
            if (!localSettings.Values.ContainsKey(IsUseOldEmojiMode))
                localSettings.Values.Add(IsUseOldEmojiMode, false);
            if (!localSettings.Values.ContainsKey(IsDarkMode))
                localSettings.Values.Add(IsDarkMode, false);
            if (!localSettings.Values.ContainsKey(CheckUpdateWhenLuanching))
                localSettings.Values.Add(CheckUpdateWhenLuanching, true);
            if (!localSettings.Values.ContainsKey(IsBackgroundColorFollowSystem))
                localSettings.Values.Add(IsBackgroundColorFollowSystem, true);
            if (!localSettings.Values.ContainsKey(IsDisplayOriginPicture))
                localSettings.Values.Add(IsDisplayOriginPicture, false);
            if (localSettings.Values.ContainsKey(UserName))
            {
                localSettings.Values.Remove(Uid);
                localSettings.Values.Remove(UserName);
                localSettings.Values.Remove(UserAvatar);
            }
            if (!localSettings.Values.ContainsKey(Uid))
                localSettings.Values.Add(Uid, string.Empty);

        }

        public static async Task CheckUpdate()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0;)");
                    var keys = JObject.Parse(await client.GetStringAsync("https://api.github.com/repos/Tangent-90/Coolapk-UWP/releases/latest"));
                    var ver = keys.Value<string>("tag_name").Replace("v", string.Empty).Split('.');
                    if (ushort.Parse(ver[0]) > Package.Current.Id.Version.Major
                        || (ushort.Parse(ver[0]) == Package.Current.Id.Version.Major && ushort.Parse(ver[1]) > Package.Current.Id.Version.Minor)
                        || (ushort.Parse(ver[0]) == Package.Current.Id.Version.Major && ushort.Parse(ver[1]) == Package.Current.Id.Version.Minor && ushort.Parse(ver[2]) > Package.Current.Id.Version.Build))
                    {
                        var grid = new Grid();
                        var textBlock = new TextBlock 
                        { 
                            Text = $"程序更新了({Package.Current.Id.Version.Major}.{Package.Current.Id.Version.Minor}.{Package.Current.Id.Version.Build} -> {keys.Value<string>("tag_name")})。" ,
                            HorizontalAlignment = HorizontalAlignment.Left,
                            VerticalAlignment = VerticalAlignment.Center
                        };
                        var button = new Button
                        {
                            Content = "前往Github",
                            HorizontalAlignment = HorizontalAlignment.Right,
                            VerticalAlignment = VerticalAlignment.Center
                        };
                        button.Click += async (_, __) =>
                        {
                            await Windows.System.Launcher.LaunchUriAsync(new Uri(keys.Value<string>("html_url")));
                        };
                        grid.Children.Add(textBlock);
                        grid.Children.Add(button);
                        UIHelper.InAppNotification.Show(grid, 12000);
                    }
                    else UIHelper.ShowMessage("当前无可用更新。");
                }
            }
            catch (HttpRequestException) { UIHelper.ShowMessage("网络异常"); }
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

                var view = ApplicationView.GetForCurrentView().TitleBar;
                view.ButtonBackgroundColor = view.InactiveBackgroundColor = view.ButtonInactiveBackgroundColor = Colors.Transparent;
                view.ForegroundColor = view.ButtonForegroundColor = view.ButtonHoverForegroundColor = view.ButtonPressedForegroundColor = ForeColor;
                view.InactiveForegroundColor = view.ButtonInactiveForegroundColor = ButtonForeInactiveColor;
                view.ButtonHoverBackgroundColor = BackColor;
                view.ButtonPressedBackgroundColor = ButtonBackPressedColor;
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
                        Set(Uid, uid);
                        Set(UserName, userName);
                        var o = (JObject)await DataHelper.GetDataAsync(DataUriType.CheckLoginInfo);
                        UIHelper.NotificationNums.Initial((JObject)o["notifyCount"]);
                        UIHelper.RaiseUserAvatarChangedEvent(null, await ImageCacheHelper.GetImageAsync(ImageType.BigAvatar, o.Value<string>("userAvatar")));
                        Set(UserAvatar, (JObject)(await DataHelper.GetDataAsync(DataUriType.GetUserSpace, uid)).Value<string>("userAvatar"));
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
            Set(Uid, string.Empty);
            Set(UserName, string.Empty);
            Set(UserAvatar, string.Empty);
            UIHelper.RaiseUserAvatarChangedEvent(null, null);
            UIHelper.NotificationNums.ClearNums();
        }
    }
}