using MetroLog;
using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace CoolapkUWP.Helpers
{
    internal static partial class SettingsHelper
    {
        public const string Uid = "Uid";
        public const string Token = "Token";
        public const string UserName = "UserName";
        public const string IsUseAPI2 = "IsUseAPI2";
        public const string IsFirstRun = "IsFirstRun";
        public const string APIVersion = "APIVersion";
        public const string UpdateDate = "UpdateDate";
        public const string IsNoPicsMode = "IsNoPicsMode";
        public const string TokenVersion = "TokenVersion";
        public const string SelectedAppTheme = "SelectedAppTheme";
        public const string ShowOtherException = "ShowOtherException";
        public const string IsDisplayOriginPicture = "IsDisplayOriginPicture";
        public const string CheckUpdateWhenLuanching = "CheckUpdateWhenLuanching";

        public static Type Get<Type>(string key) => LocalObject.Read<Type>(key);
        public static void Set<Type>(string key, Type value) => LocalObject.Save(key, value);
        public static void SetFile<Type>(string key, Type value) => LocalObject.SaveFileAsync(key, value);
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
            if (!LocalObject.KeyExists(APIVersion))
            {
                LocalObject.Save(APIVersion, "V13");
            }
            if (!LocalObject.KeyExists(UpdateDate))
            {
                LocalObject.Save(UpdateDate, new DateTime());
            }
            if (!LocalObject.KeyExists(IsNoPicsMode))
            {
                LocalObject.Save(IsNoPicsMode, false);
            }
            if (!LocalObject.KeyExists(TokenVersion))
            {
                LocalObject.Save(TokenVersion, Common.TokenVersion.TokenV2);
            }
            if (!LocalObject.KeyExists(SelectedAppTheme))
            {
                LocalObject.Save(SelectedAppTheme, ElementTheme.Default);
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
        }
    }

    internal static partial class SettingsHelper
    {
        private static readonly LocalObjectStorageHelper LocalObject = new LocalObjectStorageHelper();
        public static readonly ILogManager LogManager = LogManagerFactory.CreateLogManager();

        static SettingsHelper()
        {
            SetDefaultSettings();
        }
    }
}
