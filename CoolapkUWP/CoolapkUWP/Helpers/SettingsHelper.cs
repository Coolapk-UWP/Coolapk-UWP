using CoolapkUWP.BackgroundTasks;
using CoolapkUWP.Core.Helpers;
using Microsoft.Toolkit.Uwp.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.Web.Http;
using Windows.Web.Http.Filters;

namespace CoolapkUWP.Helpers
{
    internal static partial class SettingsHelper
    {
        public const string Uid = "Uid";
        public const string Token = "Token";
        public const string UserName = "UserName";
        public const string IsUseAPI2 = "IsUseAPI2";
        public const string IsDarkMode = "IsDarkMode";
        public const string IsFirstRun = "IsFirstRun";
        public const string IsNoPicsMode = "IsNoPicsMode";
        public const string TokenVersion = "TokenVersion";
        public const string IsUseOldEmojiMode = "IsUseOldEmojiMode";
        public const string ShowOtherException = "ShowOtherException";
        public const string IsDisplayOriginPicture = "IsDisplayOriginPicture";
        public const string CheckUpdateWhenLuanching = "CheckUpdateWhenLuanching";
        public const string IsBackgroundColorFollowSystem = "IsBackgroundColorFollowSystem";

        public static Type Get<Type>(string key) => LocalObject.Read<Type>(key);
        public static void Set<Type>(string key, Type value) => LocalObject.Save(key, value);
        public static void SetFile<Type>(string key, Type value) => LocalObject.CreateFileAsync(key, value);
        public static async Task<Type> GetFile<Type>(string key) => await LocalObject.ReadFileAsync<Type>(key);

        public static void SetDefaultSettings()
        {
            if (!LocalObject.KeyExists(Uid))
            {
                LocalObject.Save(Uid, string.Empty);
            }
            if (!LocalObject.KeyExists(Token))
            {
                LocalObject.Save(Token, string.Empty);
            }
            if (!LocalObject.KeyExists(UserName))
            {
                LocalObject.Save(UserName, string.Empty);
            }
            if (!LocalObject.KeyExists(IsUseAPI2))
            {
                LocalObject.Save(IsUseAPI2, true);
            }
            if (!LocalObject.KeyExists(IsFirstRun))
            {
                LocalObject.Save(IsFirstRun, true);
            }
            if (!LocalObject.KeyExists(IsDarkMode))
            {
                LocalObject.Save(IsDarkMode, false);
            }
            if (!LocalObject.KeyExists(IsNoPicsMode))
            {
                LocalObject.Save(IsNoPicsMode, false);
            }
            if (!LocalObject.KeyExists(TokenVersion))
            {
                LocalObject.Save(TokenVersion, Core.Helpers.TokenVersion.TokenV2);
            }
            if (!LocalObject.KeyExists(IsUseOldEmojiMode))
            {
                LocalObject.Save(IsUseOldEmojiMode, false);
            }
            if (!LocalObject.KeyExists(ShowOtherException))
            {
                LocalObject.Save(ShowOtherException, true);
            }
            if (!LocalObject.KeyExists(IsDisplayOriginPicture))
            {
                LocalObject.Save(IsDisplayOriginPicture, false);
            }
            if (!LocalObject.KeyExists(CheckUpdateWhenLuanching))
            {
                LocalObject.Save(CheckUpdateWhenLuanching, true);
            }
            if (!LocalObject.KeyExists(IsBackgroundColorFollowSystem))
            {
                LocalObject.Save(IsBackgroundColorFollowSystem, true);
            }
        }
    }

    internal enum UISettingChangedType
    {
        LightMode,
        DarkMode,
        NoPicChanged,
    }

    internal static partial class SettingsHelper
    {
        private static readonly ApplicationDataStorageHelper LocalObject = ApplicationDataStorageHelper.GetCurrent(new SystemTextJsonObjectSerializer());
        public static readonly MetroLog.ILogManager logManager = MetroLog.LogManagerFactory.CreateLogManager();
        public static readonly UISettings UISettings = new UISettings();
        public static ElementTheme Theme => Get<bool>("IsBackgroundColorFollowSystem") ? ElementTheme.Default : (Get<bool>("IsDarkMode") ? ElementTheme.Dark : ElementTheme.Light);
        public static Core.WeakEvent<UISettingChangedType> UISettingChanged { get; } = new Core.WeakEvent<UISettingChangedType>();

        static SettingsHelper()
        {
            SetDefaultSettings();
            SetBackgroundTheme(UISettings, null);
            UISettings.ColorValuesChanged += SetBackgroundTheme;
            UIHelper.CheckTheme();
        }

        private static void SetBackgroundTheme(UISettings o, object _)
        {
            if (Get<bool>(IsBackgroundColorFollowSystem))
            {
                bool value = o.GetColorValue(UIColorType.Background) == Windows.UI.Colors.Black;
                Set(IsDarkMode, value);
                UISettingChanged.Invoke(value ? UISettingChangedType.DarkMode : UISettingChangedType.LightMode);
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

    public class SystemTextJsonObjectSerializer : Microsoft.Toolkit.Helpers.IObjectSerializer
    {
        // Specify your serialization settings
        private readonly JsonSerializerSettings settings = new JsonSerializerSettings();

        string Microsoft.Toolkit.Helpers.IObjectSerializer.Serialize<T>(T value) => JsonConvert.SerializeObject(value, typeof(T), Formatting.Indented, settings);

        public T Deserialize<T>(string value) => JsonConvert.DeserializeObject<T>(value, settings);
    }
}