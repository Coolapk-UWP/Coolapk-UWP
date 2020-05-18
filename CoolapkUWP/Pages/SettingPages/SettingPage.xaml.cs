using CoolapkUWP.Helpers;
using System;
using System.Linq;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace CoolapkUWP.Pages.SettingPages
{
    public sealed partial class SettingPage : Page
    {
        public SettingPage() => this.InitializeComponent();

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            IsNoPicsMode.IsOn = SettingsHelper.Get<bool>("IsNoPicsMode");
            IsUseOldEmojiMode.IsOn = SettingsHelper.Get<bool>("IsUseOldEmojiMode");
            IsDarkMode.IsOn = SettingsHelper.Get<bool>("IsDarkMode");
            CheckUpdateWhenLuanching.IsOn = SettingsHelper.Get<bool>("CheckUpdateWhenLuanching");
            IsBackgroundColorFollowSystem.IsOn = SettingsHelper.Get<bool>("IsBackgroundColorFollowSystem");
            IsDarkMode.Visibility = IsBackgroundColorFollowSystem.IsOn ? Visibility.Collapsed : Visibility.Visible;
            VersionTextBlock.Text = $"{Package.Current.Id.Version.Major}.{Package.Current.Id.Version.Minor}.{Package.Current.Id.Version.Build}";
#if DEBUG
            gotoTestPage.Visibility = Visibility.Visible;
#endif
        }

        private void ToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            ToggleSwitch toggle = sender as ToggleSwitch;
            SettingsHelper.Set(toggle.Name, toggle.IsOn);
            switch (toggle.Name as string)
            {
                case "IsDarkMode":
                    SettingsHelper.CheckTheme();
                    break;
                case "IsBackgroundColorFollowSystem":
                    SettingsHelper.Set("IsDarkMode", SettingsHelper.uISettings.GetColorValue(UIColorType.Background).Equals(Colors.Black));
                    SettingsHelper.CheckTheme();
                    IsDarkMode.IsOn = SettingsHelper.Get<bool>("IsDarkMode");
                    IsDarkMode.Visibility = toggle.IsOn ? Visibility.Collapsed : Visibility.Visible;
                    break;
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement button = sender as FrameworkElement;
            switch (button.Tag as string)
            {
                case "gotoTestPage": UIHelper.Navigate(typeof(TestPage), null); break;
                case "checkUpdate": SettingsHelper.CheckUpdate(); break;
                case "reset":
                    bool b = true;
                    if (!string.IsNullOrEmpty(SettingsHelper.Get<string>("Uid")))
                    {
                        MessageDialog dialog = new MessageDialog("进行此操作会同时退出登录。\n你确定吗？", "提示");
                        dialog.Commands.Add(new UICommand("是"));
                        dialog.Commands.Add(new UICommand("否"));
                        if ((await dialog.ShowAsync()).Label == "是")
                            SettingsHelper.Logout();
                        else
                            b = false;
                    }
                    if (b)
                        ApplicationData.Current.LocalSettings.Values.Clear();
                    break;
                case "CleanCache":
                    CleanCacheButton.IsEnabled = CacheContentListView.IsEnabled = false;
                    foreach (var item in CacheContentListView.SelectedItems.Select(i => CacheContentListView.Items.IndexOf(i)))
                        await ImageCacheHelper.CleanCache((ImageType)item);
                    CacheContentListView.SelectedIndex = -1;
                    CacheContentListView.IsEnabled = true;
                    break;
                case "feedback":
                    UIHelper.OpenLink("https://github.com/Tangent-90/Coolapk-UWP/issues");
                    break;
            }
        }

        private void CacheSizeListView_SelectionChanged(object sender, SelectionChangedEventArgs e) => CleanCacheButton.IsEnabled = (sender as ListView).SelectedItems.Count > 0;

        private void TitleBar_BackButtonClick(object sender, RoutedEventArgs e) => Frame.GoBack();
    }
}
