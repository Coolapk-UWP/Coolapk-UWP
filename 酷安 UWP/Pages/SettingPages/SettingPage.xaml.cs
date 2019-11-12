using CoolapkUWP.Data;
using System;
using Windows.ApplicationModel;
using Windows.Data.Json;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace CoolapkUWP.Pages.SettingPages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SettingPage : Page
    {
        public SettingPage()
        {
            this.InitializeComponent();
            Tools.mainPage.ResetRowHeight();

            title.Height = Settings.FirstPageTitleHeight;
            stackP.Margin = new Thickness(0, Settings.FirstPageTitleHeight, 0, 50);

            //IsNoPicsMode.IsOn = Settings.GetBoolen("IsNoPicsMode");
            IsUseOldEmojiMode.IsOn = Settings.GetBoolen("IsUseOldEmojiMode");
            IsDarkMode.IsOn = Settings.GetBoolen("IsDarkMode");
            CheckUpdateWhenLuanching.IsOn = Settings.GetBoolen("CheckUpdateWhenLuanching");
            IsBackgroundColorFollowSystem.IsOn = Settings.GetBoolen("IsBackgroundColorFollowSystem");
            IsDarkMode.Visibility = IsBackgroundColorFollowSystem.IsOn ? Visibility.Collapsed : Visibility.Visible;
            VersionTextBlock.Text = $"{Package.Current.Id.Version.Major}.{Package.Current.Id.Version.Minor}.{Package.Current.Id.Version.Build}";
            uidTextBox.Text = Settings.GetString("UserName");
#if DEBUG
            gotoTestPage.Visibility = Visibility.Visible;
#endif
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
            Settings.Set(toggle.Name, toggle.IsOn);
            switch (toggle.Name as string)
            {
                case "IsDarkMode":
                    Settings.CheckTheme();
                    break;
                case "IsBackgroundColorFollowSystem":
                    Settings.Set("IsDarkMode", new UISettings().GetColorValue(UIColorType.Background).Equals(Colors.Black) ? true : false);
                    Settings.CheckTheme();
                    IsDarkMode.IsOn = Settings.GetBoolen("IsDarkMode");
                    IsDarkMode.Visibility = toggle.IsOn ? Visibility.Collapsed : Visibility.Visible;
                    break;
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            switch (button.Tag as string)
            {
                case "gotoTestPage":
                    Tools.Navigate(typeof(TestPage), null);
                    break;
                case "checkUpdate":
                    Settings.CheckUpdate();
                    break;
                case "fakeLogin":
                    try
                    {
                        string userName, uid, userAvatar;
                        userName = uid = userAvatar = string.Empty;
                        if (!string.IsNullOrEmpty(uidTextBox.Text))
                        {
                            uid = await Tools.GetUserIDByName(uidTextBox.Text);
                            JsonObject r = Tools.GetJSonObject(await Tools.GetJson("/user/space?uid=" + uid));
                            if (r != null)
                            {
                                userName = r["username"].GetString();
                                userAvatar = r["userSmallAvatar"].GetString();
                            }
                            else uid = string.Empty;
                        }
                        Settings.Set("UserName", userName);
                        Settings.Set("Uid", uid);
                        Settings.Set("UserAvatar", userAvatar);
                        Tools.mainPage.UpdateUserInfo();
                    }
                    catch (System.Net.Http.HttpRequestException ex) { Tools.ShowHttpExceptionMessage(ex); }
                    catch (Exception ex) { await new MessageDialog($"出现错误，可能是用户名不正确。\n{ex}").ShowAsync(); }
                    break;
            }
        }

        private void ScrollViewer_ViewChanging(object sender, ScrollViewerViewChangingEventArgs e)
        {
            double height = (sender as ScrollViewer).VerticalOffset - e.FinalView.VerticalOffset;
            Tools.mainPage.ChangeRowHeight(height);
            ChangeTitleHeight(height);
        }

        private void ScrollViewer_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            double height = e.Delta.Translation.Y;
            Tools.mainPage.ChangeRowHeight(height);
            ChangeTitleHeight(height);
        }
        void ChangeTitleHeight(double height)
        {
            if (height < 0 && height < Settings.FirstPageTitleHeight - 48 - title.ActualHeight) height = Settings.FirstPageTitleHeight - 48 - title.ActualHeight;
            if ((title.ActualHeight + height > Settings.FirstPageTitleHeight) ||
                (title.ActualHeight + height < Settings.FirstPageTitleHeight - 48)) return;
            title.Height += height;
            stackP.Margin = new Thickness(0, title.Height, 0, 50);
        }
    }
}
