using CoolapkUWP.BackgroundTasks;
using CoolapkUWP.Core.Helpers;
using Newtonsoft.Json.Linq;
using System;
using Windows.Storage;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.Web.Http;
using Windows.Web.Http.Filters;

namespace CoolapkUWP.Helpers
{
    internal static partial class SettingsHelper
    {
        [Obsolete] public const string DefaultFollowPageIndex = "DefaultFollowPageIndex";
        [Obsolete] public const string UserAvatar = "UserAvatar";
        public const string IsNoPicsMode = "IsNoPicsMode";
        public const string IsUseOldEmojiMode = "IsUseOldEmojiMode";
        public const string IsDarkMode = "IsDarkMode";
        public const string IsUseAPI2 = "IsUseAPI2";
        public const string TokenVersion = "TokenVersion";
        public const string CheckUpdateWhenLuanching = "CheckUpdateWhenLuanching";
        public const string IsBackgroundColorFollowSystem = "IsBackgroundColorFollowSystem";
        public const string Uid = "Uid";
        public const string UserName = "UserName";
        public const string Token = "Token";
        public const string IsDisplayOriginPicture = "IsDisplayOriginPicture";
        public const string ShowOtherException = "ShowOtherException";
        public const string IsFirstRun = "IsFirstRun";

        public static Type Get<Type>(string key) => (Type)localSettings.Values[key];

        public static void Set(string key, object value) => localSettings.Values[key] = value;

        public static void SetDefaultSettings()
        {
#pragma warning disable CS0612 // 类型或成员已过时
            if (localSettings.Values.ContainsKey(DefaultFollowPageIndex))
            {
                _ = localSettings.Values.Remove(DefaultFollowPageIndex);
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
            if (!localSettings.Values.ContainsKey(IsUseAPI2))
            {
                localSettings.Values.Add(IsUseAPI2, true);
            }
            if (!localSettings.Values.ContainsKey(TokenVersion))
            {
                localSettings.Values.Add(TokenVersion, (int)Core.Helpers.TokenVersion.TokenV2);
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
            if (!localSettings.Values.ContainsKey(UserName))
            {
                localSettings.Values.Add(UserName, string.Empty);
            }
            if (!localSettings.Values.ContainsKey(Token))
            {
                localSettings.Values.Add(Token, string.Empty);
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

        public static bool LoginIn() => LoginIn(Get<string>(Uid), Get<string>(UserName), Get<string>(Token));

        public static bool LoginIn(string Uid, string UserName, string Token)
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
                var Expires = DateTime.UtcNow.AddDays(365);
                uid.Expires = username.Expires = token.Expires = Expires;
                cookieManager.SetCookie(uid);
                cookieManager.SetCookie(username);
                cookieManager.SetCookie(token);
                return CheckLoginInfo();
            }
        }

        public static bool CheckLoginInfo()
        {
            using (HttpBaseProtocolFilter filter = new HttpBaseProtocolFilter())
            {
                HttpCookieManager cookieManager = filter.CookieManager;
                string uid = string.Empty, token = string.Empty, userName = string.Empty;
                foreach (HttpCookie item in cookieManager.GetCookies(UriHelper.CoolapkUri))
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
                    Set(UserName, userName);
                    Set(Token, token);

                    UIHelper.NotificationNums.Initial();
                    LiveTileTask.UpdateTile();

                    return true;
                }
            }
        }

        public static void Logout()
        {
            using (HttpBaseProtocolFilter filter = new HttpBaseProtocolFilter())
            {
                HttpCookieManager cookieManager = filter.CookieManager;
                foreach (HttpCookie item in cookieManager.GetCookies(UriHelper.BaseUri))
                {
                    cookieManager.DeleteCookie(item);
                }
            }
            Set(Uid, string.Empty);
            Set(UserName, string.Empty);
            UIHelper.NotificationNums.ClearNums();
        }
    }
}