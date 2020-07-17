using CoolapkUWP.Helpers.Providers;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace CoolapkUWP.Helpers
{
    internal static class SettingsHelper
    {
        [Obsolete]
        public const string DefaultFollowPageIndex = "DefaultFollowPageIndex";
        [Obsolete]
        public const string UserAvatar = "UserAvatar";
        [Obsolete]
        public const string UserName = "UserName";
        public const string IsNoPicsMode = "IsNoPicsMode";
        public const string IsUseOldEmojiMode = "IsUseOldEmojiMode";
        public const string IsDarkMode = "IsDarkMode";
        public const string CheckUpdateWhenLuanching = "CheckUpdateWhenLuanching";
        public const string IsBackgroundColorFollowSystem = "IsBackgroundColorFollowSystem";
        public const string Uid = "Uid";
        public const string IsDisplayOriginPicture = "IsDisplayOriginPicture";
        public const string ShowOtherException = "ShowOtherException";
        public const string IsFirstRun = "IsFirstRun";

        private static readonly ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        public static readonly MetroLog.ILogManager logManager = MetroLog.LogManagerFactory.CreateLogManager();
        public static readonly UISettings uiSettings = new UISettings();
        public static ElementTheme Theme => Get<bool>("IsBackgroundColorFollowSystem") ? ElementTheme.Default : (Get<bool>("IsDarkMode") ? ElementTheme.Dark : ElementTheme.Light);

        public static T Get<T>(string key) => (T)localSettings.Values[key];

        public static void Set(string key, object value) => localSettings.Values[key] = value;

        static SettingsHelper()
        {
            SetDefaultSettings();
            SetBackgroundTheme(uiSettings, null);
            uiSettings.ColorValuesChanged += SetBackgroundTheme;
            UIHelper.CheckTheme();
        }

        private static void SetBackgroundTheme(UISettings o,object _)
        {
            if (Get<bool>(IsBackgroundColorFollowSystem))
            {
                Set(IsDarkMode, o.GetColorValue(UIColorType.Background) == Windows.UI.Colors.Black);
            }
        }

        public static async Task CheckUpdateAsync()
        {
            var loader = Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse();
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
                            Text = string.Format(
                                        loader.GetString("HasUpdate"),
                                        Package.Current.Id.Version.Major,
                                        Package.Current.Id.Version.Minor,
                                        Package.Current.Id.Version.Build,
                                        keys.Value<string>("tag_name")),
                            HorizontalAlignment = HorizontalAlignment.Left,
                            VerticalAlignment = VerticalAlignment.Center
                        };
                        var button = new Button
                        {
                            Content = loader.GetString("GotoGithub"),
                            HorizontalAlignment = HorizontalAlignment.Right,
                            VerticalAlignment = VerticalAlignment.Center
                        };
                        button.Click += async (_, __) =>
                        {
                            await Windows.System.Launcher.LaunchUriAsync(new Uri(keys.Value<string>("html_url")));
                        };
                        grid.Children.Add(textBlock);
                        grid.Children.Add(button);
                        UIHelper.InAppNotification.Show(grid, 6000);
                    }
                    else UIHelper.ShowMessage(loader.GetString("NoUpdate"));
                }
            }
            catch (HttpRequestException) { UIHelper.ShowMessage(loader.GetString("NetworkError")); }
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
                        var o = (JObject)await DataHelper.GetDataAsync(UriProvider.GetUri(UriType.CheckLoginInfo), true);
                        UIHelper.NotificationNums.Initial((JObject)o["notifyCount"]);
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
            UIHelper.NotificationNums.ClearNums();
        }

        public static void SetDefaultSettings()
        {
#pragma warning disable CS0612 // 类型或成员已过时
            if (localSettings.Values.ContainsKey(DefaultFollowPageIndex))
            {
                localSettings.Values.Remove(DefaultFollowPageIndex);
            }
            if (localSettings.Values.ContainsKey(UserName))
            {
                localSettings.Values.Remove(UserName);
            }
            if (localSettings.Values.ContainsKey(UserAvatar))
            {
                localSettings.Values.Remove(UserAvatar);
            }
#pragma warning restore CS0612

            if (!localSettings.Values.ContainsKey(ShowOtherException))
            {
                localSettings.Values.Add(ShowOtherException, true);
            }
            if (!localSettings.Values.ContainsKey(IsNoPicsMode))
            {
                localSettings.Values.Add(IsNoPicsMode, false);
            }
            if (!localSettings.Values.ContainsKey(IsUseOldEmojiMode))
            {
                localSettings.Values.Add(IsUseOldEmojiMode, false);
            }
            if (!localSettings.Values.ContainsKey(IsDarkMode))
            {
                localSettings.Values.Add(IsDarkMode, false);
            }
            if (!localSettings.Values.ContainsKey(CheckUpdateWhenLuanching))
            {
                localSettings.Values.Add(CheckUpdateWhenLuanching, true);
            }
            if (!localSettings.Values.ContainsKey(IsBackgroundColorFollowSystem))
            {
                localSettings.Values.Add(IsBackgroundColorFollowSystem, true);
            }
            if (!localSettings.Values.ContainsKey(IsDisplayOriginPicture))
            {
                localSettings.Values.Add(IsDisplayOriginPicture, false);
            }
            if (!localSettings.Values.ContainsKey(IsFirstRun))
            {
                localSettings.Values.Add(IsFirstRun, true);
            }
            if (!localSettings.Values.ContainsKey(Uid))
            {
                localSettings.Values.Add(Uid, string.Empty);
            }
        }
    }
}