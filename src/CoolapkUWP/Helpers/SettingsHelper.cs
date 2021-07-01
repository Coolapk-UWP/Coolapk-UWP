using CoolapkUWP.BackgroundTasks;
using CoolapkUWP.Core.Helpers;
using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;

namespace CoolapkUWP.Helpers
{
    internal static partial class SettingsHelper
    {
        [Obsolete] public const string DefaultFollowPageIndex = "DefaultFollowPageIndex";
        [Obsolete] public const string UserAvatar = "UserAvatar";
        [Obsolete] public const string UserName = "UserName";
        public const string IsNoPicsMode = "IsNoPicsMode";
        public const string IsUseOldEmojiMode = "IsUseOldEmojiMode";
        public const string IsDarkMode = "IsDarkMode";
        public const string CheckUpdateWhenLuanching = "CheckUpdateWhenLuanching";
        public const string IsBackgroundColorFollowSystem = "IsBackgroundColorFollowSystem";
        public const string Uid = "Uid";
        public const string IsDisplayOriginPicture = "IsDisplayOriginPicture";
        public const string ShowOtherException = "ShowOtherException";
        public const string IsFirstRun = "IsFirstRun";

        public static T Get<T>(string key) => (T)localSettings.Values[key];

        public static void Set(string key, object value) => localSettings.Values[key] = value;

        public static void SetDefaultSettings()
        {
#pragma warning disable CS0612 // 类型或成员已过时
            if (localSettings.Values.ContainsKey(DefaultFollowPageIndex))
            {
                _ = localSettings.Values.Remove(DefaultFollowPageIndex);
            }
            if (localSettings.Values.ContainsKey(UserName))
            {
                _ = localSettings.Values.Remove(UserName);
            }
            if (localSettings.Values.ContainsKey(UserAvatar))
            {
                _ = localSettings.Values.Remove(UserAvatar);
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

    internal enum UiSettingChangedType
    {
        LightMode,
        DarkMode,
        NoPicChanged,
    }

    internal static partial class SettingsHelper
    {
        private static readonly ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        public static readonly MetroLog.ILogManager logManager = MetroLog.LogManagerFactory.CreateLogManager();
        public static readonly UISettings uiSettings = new UISettings();
        public static ElementTheme Theme => Get<bool>("IsBackgroundColorFollowSystem") ? ElementTheme.Default : (Get<bool>("IsDarkMode") ? ElementTheme.Dark : ElementTheme.Light);
        public static Core.WeakEvent<UiSettingChangedType> UiSettingChanged { get; } = new Core.WeakEvent<UiSettingChangedType>();

        static SettingsHelper()
        {
            SetDefaultSettings();
            SetBackgroundTheme(uiSettings, null);
            uiSettings.ColorValuesChanged += SetBackgroundTheme;
            UIHelper.CheckTheme();
        }

        private static void SetBackgroundTheme(UISettings o, object _)
        {
            if (Get<bool>(IsBackgroundColorFollowSystem))
            {
                bool value = o.GetColorValue(UIColorType.Background) == Windows.UI.Colors.Black;
                Set(IsDarkMode, value);
                UiSettingChanged.Invoke(value ? UiSettingChangedType.DarkMode : UiSettingChangedType.LightMode);
            }
        }

        public static bool CheckLoginInfo()
        {
            try
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

                            default:
                                break;
                        }
                    }

                    if (string.IsNullOrEmpty(uid) || string.IsNullOrEmpty(token) || string.IsNullOrEmpty(userName))
                    {
                        Logout();
                        return false;
                    }
                    else
                    {
                        Set(Uid, uid);

                        UIHelper.NotificationNums.Initial();
                        LiveTileTask.UpdateTile();

                        return true;
                    }
                }
            }
            catch { throw; }
        }

        public static void Logout()
        {
            using (Windows.Web.Http.Filters.HttpBaseProtocolFilter filter = new Windows.Web.Http.Filters.HttpBaseProtocolFilter())
            {
                Windows.Web.Http.HttpCookieManager cookieManager = filter.CookieManager;
                foreach (Windows.Web.Http.HttpCookie item in cookieManager.GetCookies(UriHelper.BaseUri))
                {
                    cookieManager.DeleteCookie(item);
                }
            }
            Set(Uid, string.Empty);
            UIHelper.NotificationNums.ClearNums();
        }
    }
}