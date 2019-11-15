using CoolapkUWP.Control;
using System;
using System.Net.Http;
using Windows.ApplicationModel;
using Windows.Foundation.Metadata;
using Windows.Storage;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace CoolapkUWP.Data
{
    [Bindable]
    static class Settings
    {
        static ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        public static UISettings uISettings = new UISettings();
        public static bool IsMobile = Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile";

        public static double FirstPageTitleHeight
        {
            get
            {
                if (IsMobile) return 96;
                else return 128;
            }
        }

        public static double PageTitleHeight
        {
            get
            {
                if (IsMobile) return 48;
                else return 80;
            }
        }

        public static Thickness titleTextMargin = new Thickness(5, 12, 5, 12);
        public static Thickness stackPanelMargin = new Thickness(0, PageTitleHeight, 0, 2);
        public static VerticalAlignment titleContentVerticalAlignment = VerticalAlignment.Bottom;

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
            if (!localSettings.Values.ContainsKey("UserName"))
                localSettings.Values.Add("UserName", string.Empty);
            if (!localSettings.Values.ContainsKey("Uid"))
                localSettings.Values.Add("Uid", string.Empty);
            if (!localSettings.Values.ContainsKey("UserAvatar"))
                localSettings.Values.Add("UserAvatar", string.Empty);
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
                    GetUpdateContentDialog dialog = new GetUpdateContentDialog(release.HtmlUrl, release.Body) { RequestedTheme = Settings.GetBoolen("IsDarkMode") ? ElementTheme.Dark : ElementTheme.Light };
                    await dialog.ShowAsync();
                }
                else Tools.ShowMessage("当前无可用更新。");
            }
            catch (HttpRequestException ex) { Tools.ShowHttpExceptionMessage(ex); }
        }

        public static void CheckTheme()
        {
            InitializeSettings();
            if (Window.Current?.Content is FrameworkElement frameworkElement)
            {
                if (!GetBoolen("IsDarkMode"))
                {
                    frameworkElement.RequestedTheme = ElementTheme.Light;
                    ChangeColor(true);
                    foreach (var item in Tools.popups)
                        item.RequestedTheme = ElementTheme.Light;
                }
                else
                {
                    frameworkElement.RequestedTheme = ElementTheme.Dark;
                    ChangeColor(false);
                    foreach (var item in Tools.popups)
                        item.RequestedTheme = ElementTheme.Dark;
                }
            }
            void ChangeColor(bool value)//标题栏颜色
            {
                Color BackColor = value ? Color.FromArgb(255, 242, 242, 242) : Color.FromArgb(255, 23, 23, 23),
                      ForeColor = value ? Colors.Black : Colors.White,
                      ButtonForeInactiveColor = value ? Color.FromArgb(255, 50, 50, 50) : Color.FromArgb(255, 200, 200, 200),
                      ButtonBackPressedColor = value ? Color.FromArgb(255, 200, 200, 200) : Color.FromArgb(255, 50, 50, 50);
                Tools.mainPage?.ChangeButtonForeground();
                if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
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
    }
}
