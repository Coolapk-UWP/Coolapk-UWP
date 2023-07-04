using CoolapkUWP.Helpers;
using CoolapkUWP.Pages.BrowserPages;
using CoolapkUWP.ViewModels.BrowserPages;
using CoolapkUWP.ViewModels.SettingsPages;
using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Storage;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace CoolapkUWP.Controls
{
    public sealed partial class SettingsFlyoutControl : SettingsFlyout
    {
        private Action<UISettingChangedType> UISettingChanged;

        internal SettingsViewModel Provider;

        public SettingsFlyoutControl() => InitializeComponent();

        private void SettingsFlyout_Loaded(object sender, RoutedEventArgs e)
        {
            UISettingChanged = (mode) =>
            {
                switch (mode)
                {
                    case UISettingChangedType.LightMode:
                        RequestedTheme = ElementTheme.Light;
                        break;
                    case UISettingChangedType.DarkMode:
                        RequestedTheme = ElementTheme.Dark;
                        break;
                    default:
                        break;
                }
            };
            Provider = SettingsViewModel.Caches ?? new SettingsViewModel(Dispatcher);
            ThemeHelper.UISettingChanged.Add(UISettingChanged);
            DataContext = Provider;
        }

        private void SettingsFlyout_Unloaded(object sender, RoutedEventArgs e)
        {
            ThemeHelper.UISettingChanged.Remove(UISettingChanged);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as FrameworkElement).Tag.ToString())
            {
                case "Reset":
                    ApplicationData.Current.LocalSettings.Values.Clear();
                    SettingsHelper.SetDefaultSettings();
                    if (Reset.Flyout is Flyout flyout_reset)
                    {
                        flyout_reset.Hide();
                    }
                    break;
                case "CleanCache":
                    Provider.CleanCache();
                    break;
                case "CheckUpdate":
                    Provider.CheckUpdate();
                    break;
                case "AccountLogout":
                    SettingsHelper.Logout();
                    if (AccountLogout.Flyout is Flyout flyout_logout)
                    {
                        flyout_logout.Hide();
                    }
                    Provider.IsLogin = false;
                    break;
                case "AccountSetting":
                    _ = this.NavigateAsync(typeof(BrowserPage), new BrowserViewModel("https://account.coolapk.com/account/settings"));
                    break;
                default:
                    break;
            }
        }

        private async void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as FrameworkElement).Tag.ToString())
            {
                case "OpenLogFile":
                    StorageFolder folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("MetroLogs", CreationCollisionOption.OpenIfExists);
                    IReadOnlyList<StorageFile> files = await folder.GetFilesAsync();
                    StorageFile file = files.FirstOrDefault();
                    if (file != null) { _ = Launcher.LaunchFileAsync(file); }
                    break;
                default:
                    break;
            }
        }

        private async void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as FrameworkElement).Tag.ToString())
            {
                case "LogFolder":
                    _ = Launcher.LaunchFolderAsync(await ApplicationData.Current.LocalFolder.CreateFolderAsync("MetroLogs", CreationCollisionOption.OpenIfExists));
                    break;
                case "WindowsColor":
                    _ = Launcher.LaunchUriAsync(new Uri("ms-settings:colors"));
                    break;
                default:
                    break;
            }
        }

        private void MarkdownText_LinkClicked(object sender, LinkClickedEventArgs e) => _ = this.OpenLinkAsync(e.Link);

        private void GotoUpdate_Click(object sender, RoutedEventArgs e) => _ = Launcher.LaunchUriAsync(new Uri((sender as FrameworkElement).Tag.ToString()));
    }
}
