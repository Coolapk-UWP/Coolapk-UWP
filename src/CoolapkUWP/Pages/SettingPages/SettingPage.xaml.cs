using CoolapkUWP.BackgroundTasks;
using CoolapkUWP.Helpers;
using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using System.ComponentModel;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using static CoolapkUWP.Helpers.SettingsHelper;

namespace CoolapkUWP.Pages.SettingPages
{
    public sealed partial class SettingPage : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private Visibility logoutButtonVisibility;

        private Visibility LogoutButtonVisibility
        {
            get => logoutButtonVisibility;
            set
            {
                logoutButtonVisibility = value;
                RaisePropertyChangedEvent();
            }
        }

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        private bool isNoPicsMode = Get<bool>(IsNoPicsMode);
        private bool isUseOldEmojiMode = Get<bool>(IsUseOldEmojiMode);
        private bool isDisplayOriginPicture = Get<bool>(IsDisplayOriginPicture);
        private bool isDarkMode = Get<bool>(IsDarkMode);
        private bool checkUpdateWhenLuanching = Get<bool>(CheckUpdateWhenLuanching);
        private bool isBackgroundColorFollowSystem = Get<bool>(IsBackgroundColorFollowSystem);
        private const string issuePath = "https://github.com/Tangent-90/Coolapk-UWP/issues";
        private bool isCleanCacheButtonEnabled = true;
        private bool isCheckUpdateButtonEnabled = true;
        private bool showOtherException = Get<bool>(ShowOtherException);

        private static string VersionTextBlockText
        {
            get
            {
                string ver = $"{Package.Current.Id.Version.Major}.{Package.Current.Id.Version.Minor}.{Package.Current.Id.Version.Build}";
                ResourceLoader loader = ResourceLoader.GetForViewIndependentUse();
                string name = loader?.GetString("AppName") ?? "CoolapkUWP";
                return $"{name} v{ver}";
            }
        }

        private bool IsNoPicsMode2
        {
            get => isNoPicsMode;
            set
            {
                Set(IsNoPicsMode, value);
                isNoPicsMode = Get<bool>(IsNoPicsMode);
                RaisePropertyChangedEvent();
                UiSettingChanged?.Invoke(UiSettingChangedType.NoPicChanged);
            }
        }

        private bool IsUseOldEmojiMode2
        {
            get => isUseOldEmojiMode;
            set
            {
                Set(IsUseOldEmojiMode, value);
                isUseOldEmojiMode = Get<bool>(IsUseOldEmojiMode);
                RaisePropertyChangedEvent();
            }
        }

        private bool IsDisplayOriginPicture2
        {
            get => isDisplayOriginPicture;
            set
            {
                Set(IsDisplayOriginPicture, value);
                isDisplayOriginPicture = Get<bool>(IsDisplayOriginPicture);
                RaisePropertyChangedEvent();
            }
        }

        private bool IsDarkMode2
        {
            get => isDarkMode;
            set
            {
                Set(IsDarkMode, value);
                isDarkMode = Get<bool>(IsDarkMode);
                UIHelper.CheckTheme();
                RaisePropertyChangedEvent();
            }
        }

        private bool CheckUpdateWhenLuanching2
        {
            get => checkUpdateWhenLuanching;
            set
            {
                Set(CheckUpdateWhenLuanching, value);
                checkUpdateWhenLuanching = Get<bool>(CheckUpdateWhenLuanching);
                RaisePropertyChangedEvent();
            }
        }

        private bool IsBackgroundColorFollowSystem2
        {
            get => isBackgroundColorFollowSystem;
            set
            {
                Set(IsBackgroundColorFollowSystem, value);
                isBackgroundColorFollowSystem = Get<bool>(IsBackgroundColorFollowSystem);
                RaisePropertyChangedEvent();
                IsDarkMode2 = uiSettings.GetColorValue(Windows.UI.ViewManagement.UIColorType.Background).Equals(Windows.UI.Colors.Black);
            }
        }

        private bool IsCleanCacheButtonEnabled
        {
            get => isCleanCacheButtonEnabled;
            set
            {
                isCleanCacheButtonEnabled = value;
                RaisePropertyChangedEvent();
            }
        }

        private bool IsCheckUpdateButtonEnabled
        {
            get => isCheckUpdateButtonEnabled;
            set
            {
                isCheckUpdateButtonEnabled = value;
                RaisePropertyChangedEvent();
            }
        }

        private bool ShowOtherException2
        {
            get => showOtherException;
            set
            {
                Set(ShowOtherException, value);
                showOtherException = Get<bool>(ShowOtherException);
                RaisePropertyChangedEvent();
            }
        }

        public SettingPage()
        {
            InitializeComponent();
            LogoutButtonVisibility = string.IsNullOrEmpty(Get<string>(Uid))
                                             ? Visibility.Collapsed
                                             : Visibility.Visible;
        }

        protected override void OnNavigatedTo(Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
#if !DEBUG
            if (UIHelper.IsAuthor)
#endif
            gotoTestPage.Visibility = Visibility.Visible;

            ThemeMode.SelectedIndex = IsBackgroundColorFollowSystem2 ? 2 : IsDarkMode2 ? 1 : 0;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as FrameworkElement).Tag as string)
            {
                case "gotoTestPage": _ = Frame.Navigate(typeof(TestPage)); break;

                case "checkUpdate":
                    IsCheckUpdateButtonEnabled = false;
                    await CheckUpdate.CheckUpdateAsync(true, false);
                    IsCheckUpdateButtonEnabled = true;
                    break;

                case "reset":
                    Logout();
                    ApplicationData.Current.LocalSettings.Values.Clear();
                    SetDefaultSettings();
                    if (reset.Flyout is Flyout flyout_reset)
                    {
                        flyout_reset.Hide();
                    }
                    _ = Frame.Navigate(typeof(SettingPage));
                    Frame.GoBack();
                    break;

                case "CleanCache":
                    IsCleanCacheButtonEnabled = false;
                    await ImageCacheHelper.CleanCacheAsync();
                    IsCleanCacheButtonEnabled = true;
                    break;

                case "feedback":
                    UIHelper.OpenLinkAsync(issuePath);
                    break;

                case "logFolder":
                    await Windows.System.Launcher.LaunchFolderAsync(await ApplicationData.Current.LocalFolder.CreateFolderAsync("MetroLogs", CreationCollisionOption.OpenIfExists));
                    break;

                case "AccountSetting":
                    UIHelper.Navigate(typeof(BrowserPage), new object[] { false, "https://account.coolapk.com/account/settings" });
                    break;

                case "AccountLogout":
                    Logout();
                    LogoutButtonVisibility = Visibility.Collapsed;
                    if (AccountLogout.Flyout is Flyout flyout_logout)
                    {
                        flyout_logout.Hide();
                    }
                    _ = Frame.Navigate(typeof(SettingPage));
                    Frame.GoBack();
                    break;

                case "MyDevice":
                    UIHelper.Navigate(typeof(BrowserPage), new object[] { false, "https://m.coolapk.com/mp/do?c=userDevice&m=myDevice" });
                    break;
                default:
                    break;
            }
        }

        private void TitleBar_BackButtonClick(object sender, RoutedEventArgs e) => Frame.GoBack();

        private void RadioButtons_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (((Microsoft.UI.Xaml.Controls.RadioButtons)sender).SelectedIndex)
            {
                case 0:
                    IsBackgroundColorFollowSystem2 = false;
                    IsDarkMode2 = false;
                    break;
                case 1:
                    IsBackgroundColorFollowSystem2 = false;
                    IsDarkMode2 = true;
                    break;
                case 2:
                    IsBackgroundColorFollowSystem2 = true;
                    UiSettingChanged?.Invoke(IsDarkMode2 ? UiSettingChangedType.DarkMode : UiSettingChangedType.LightMode);
                    break;
                default:
                    break;
            }
        }

        private async void MarkdownText_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            if (Uri.TryCreate(e.Link, UriKind.Absolute, out Uri link))
            {
                string str = link.ToString();
                if (str.Contains("m.coolapk.com/mp")) { UIHelper.NavigateInSplitPane(typeof(FeedPages.HTMLTextPage), str); }
                else { UIHelper.Navigate(typeof(BrowserPage), new object[] { false, str }); }
            }
        }
    }
}