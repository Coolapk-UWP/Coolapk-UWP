using System;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace CoolapkUWP.Data
{
    [Bindable]
    static class Settings
    {
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
                await System.Threading.Tasks.Task.Delay(100);
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
    }
}
