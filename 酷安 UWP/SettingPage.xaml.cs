using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace 酷安_UWP
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SettingPage : Page
    {
        MainPage mainPage;
        public SettingPage()
        {
            this.InitializeComponent();
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            IsNoPicsMode.IsOn = Convert.ToBoolean(localSettings.Values["IsNoPicsMode"]);
            IsUseOldEmojiMode.IsOn = Convert.ToBoolean(localSettings.Values["IsUseOldEmojiMode"]);
            IsDarkMode.IsOn = Convert.ToBoolean(localSettings.Values["IsDarkMode"]);
            CheckUpdateWhenLuanching.IsOn = Convert.ToBoolean(localSettings.Values["CheckUpdateWhenLuanching"]);
            //IsAccentColorFollowSystem.IsOn = Convert.ToBoolean(localSettings.Values["IsAccentColorFollowSystem"]);
            IsBackgroundColorFollowSystem.IsOn = Convert.ToBoolean(localSettings.Values["IsBackgroundColorFollowSystem"]);
            IsDarkMode.Visibility = IsBackgroundColorFollowSystem.IsOn ? Visibility.Collapsed : Visibility.Visible;
            VersionTextBlock.Text = $"{Package.Current.Id.Version.Major}.{Package.Current.Id.Version.Minor}.{Package.Current.Id.Version.Build}";
            uidTextBox.Text = localSettings.Values["UserName"] as string;
#if DEBUG
            gotoTestPage.Visibility = Visibility.Visible;
#endif
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            mainPage = e.Parameter as MainPage;
        }

        static public void InitializeSettings(ApplicationDataContainer localSettings)
        {
            if (!localSettings.Values.ContainsKey("IsNoPicsMode"))
                localSettings.Values.Add("IsNoPicsMode", false);
            if (!localSettings.Values.ContainsKey("IsUseOldEmojiMode"))
                localSettings.Values.Add("IsUseOldEmojiMode", false);
            if (!localSettings.Values.ContainsKey("IsDarkMode"))
                localSettings.Values.Add("IsDarkMode", false);
            if (!localSettings.Values.ContainsKey("CheckUpdateWhenLuanching"))
                localSettings.Values.Add("CheckUpdateWhenLuanching", true);
            //if (!localSettings.Values.ContainsKey("IsAccentColorFollowSystem"))
            //    localSettings.Values.Add("IsAccentColorFollowSystem", false);
            if (!localSettings.Values.ContainsKey("IsBackgroundColorFollowSystem"))
                localSettings.Values.Add("IsBackgroundColorFollowSystem", true);
            if (!localSettings.Values.ContainsKey("UserName"))
                localSettings.Values.Add("UserName", string.Empty);
            if (!localSettings.Values.ContainsKey("Uid"))
                localSettings.Values.Add("Uid", string.Empty);
            if (!localSettings.Values.ContainsKey("UserAvatar"))
                localSettings.Values.Add("UserAvatar", string.Empty);
        }

        public static void CheckTheme()
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            InitializeSettings(localSettings);
            if (Window.Current.Content is FrameworkElement frameworkElement)
                if (Convert.ToBoolean(localSettings.Values["IsDarkMode"]) == false)
                {
                    frameworkElement.RequestedTheme = ElementTheme.Light;
                    ChangeTitleBarColor(true);
                }
                else
                {
                    frameworkElement.RequestedTheme = ElementTheme.Dark;
                    ChangeTitleBarColor(false);
                }

            void ChangeTitleBarColor(bool value)//标题栏颜色
            {
                Color BackColor = value ? Color.FromArgb(255, 242, 242, 242) : Color.FromArgb(255, 43, 43, 43),
                      ForeColor = value ? Colors.Black : Colors.White,
                      CoolForeInactiveColor = value ? Color.FromArgb(255, 50, 50, 50) : Color.FromArgb(255, 200, 200, 200),
                      CoolBackPressedColor = value ? Color.FromArgb(255, 200, 200, 200) : Color.FromArgb(255, 50, 50, 50);
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
                    view.BackgroundColor = view.ButtonBackgroundColor = view.InactiveBackgroundColor = view.ButtonInactiveBackgroundColor = BackColor;
                    view.ForegroundColor = view.ButtonForegroundColor = view.ButtonHoverForegroundColor = view.ButtonPressedForegroundColor = ForeColor;
                    view.InactiveForegroundColor = view.ButtonInactiveForegroundColor = CoolForeInactiveColor;
                    view.ButtonHoverBackgroundColor = BackColor;
                    view.ButtonPressedBackgroundColor = CoolBackPressedColor;
                }
            }
        }

        private void CleanDataButton_Click(object sender, RoutedEventArgs e)
        {
            ApplicationData.Current.LocalSettings.Values.Clear();
            var cookieManager = new Windows.Web.Http.Filters.HttpBaseProtocolFilter().CookieManager;
            foreach (var item in cookieManager.GetCookies(new Uri("http://account.coolapk.com")))
                cookieManager.DeleteCookie(item);
        }

        private void ToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            ToggleSwitch toggle = sender as ToggleSwitch;
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            UISettings uiSettings = new UISettings();
            switch (toggle.Tag as string)
            {
                case "0":
                case "1":
                case "3":
                    localSettings.Values[toggle.Name] = toggle.IsOn;
                    break;
                case "2":
                    localSettings.Values[toggle.Name] = toggle.IsOn;
                    CheckTheme();
                    break;
                //case "4":
                //    localSettings.Values[toggle.Name] = toggle.IsOn;
                //    if (toggle.IsOn) Application.Current.Resources["SystemAccentColor"] = uiSettings.GetColorValue(UIColorType.Accent);
                //    else Application.Current.Resources["SystemAccentColor"] = Color.FromArgb(255, 76, 175, 80);
                //    Window.Current.Content.UpdateLayout();
                //    CheckTheme();
                //    break;
                case "5":
                    localSettings.Values[toggle.Name] = toggle.IsOn;
                    localSettings.Values["IsDarkMode"] = uiSettings.GetColorValue(UIColorType.Background).Equals(Colors.Black) ? true : false;
                    CheckTheme();
                    IsDarkMode.IsOn = Convert.ToBoolean(localSettings.Values["IsDarkMode"]);
                    IsDarkMode.Visibility = toggle.IsOn ? Visibility.Collapsed : Visibility.Visible;
                    break;
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            switch (button.Tag as string)
            {
                case "0":
                    Frame.Navigate(typeof(TestPage), mainPage);
                    break;
                case "1":
                    MainPage.CheckUpdate(true);
                    break;
                case "fakeLogin":
                    try
                    {
                        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
                        if (string.IsNullOrEmpty(uidTextBox.Text))
                            localSettings.Values["UserName"] = localSettings.Values["Uid"] = localSettings.Values["UserAvatar"] = string.Empty;
                        else
                        {
                            string uid = await Tools.GetUserIDByName(uidTextBox.Text);
                            JObject r = await Tools.GetUserProfileByID(uid);
                            localSettings.Values["UserName"] = r["username"].ToString();
                            localSettings.Values["Uid"] = uid;
                            localSettings.Values["UserAvatar"] = r["userAvatar"].ToString();
                        }
                        mainPage.UpdateUserInfo(localSettings);
                    }
                    catch (Exception ex) { await new MessageDialog($"出现错误，可能是用户名不正确。\n{ex}").ShowAsync(); }
                    break;
            }
        }
    }
}
