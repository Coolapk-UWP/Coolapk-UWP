using CoolapkUWP.Helpers;
using System;
using System.ComponentModel;
using System.Linq;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace CoolapkUWP.Pages.SettingPages
{
    public sealed partial class SettingPage : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null)
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private bool isNoPicsMode = SettingsHelper.Get<bool>(SettingsHelper.IsNoPicsMode);
        private bool isUseOldEmojiMode = SettingsHelper.Get<bool>(SettingsHelper.IsUseOldEmojiMode);
        private bool isDisplayOriginPicture = SettingsHelper.Get<bool>(SettingsHelper.IsDisplayOriginPicture);
        private bool isDarkMode = SettingsHelper.Get<bool>(SettingsHelper.IsDarkMode);
        private bool checkUpdateWhenLuanching = SettingsHelper.Get<bool>(SettingsHelper.CheckUpdateWhenLuanching);
        private bool isBackgroundColorFollowSystem = SettingsHelper.Get<bool>(SettingsHelper.IsBackgroundColorFollowSystem);
        private Visibility isDarkModeSwitchVisibility = SettingsHelper.Get<bool>(SettingsHelper.IsBackgroundColorFollowSystem) ? Visibility.Collapsed : Visibility.Visible;
        private readonly string versionTextBlockText = $"{Package.Current.Id.Version.Major}.{Package.Current.Id.Version.Minor}.{Package.Current.Id.Version.Build}";
        private const string issuePath = "https://github.com/Tangent-90/Coolapk-UWP/issues";
        private bool isCleanCacheButtonEnabled = true;
        private bool isCheckUpdateButtonEnabled = true;

        public bool IsNoPicsMode
        {
            get => isNoPicsMode;
            set
            {
                SettingsHelper.Set(SettingsHelper.IsNoPicsMode, value);
                isNoPicsMode = SettingsHelper.Get<bool>(SettingsHelper.IsNoPicsMode);
                RaisePropertyChangedEvent();
            }
        }

        public bool IsUseOldEmojiMode
        {
            get => isUseOldEmojiMode;
            set
            {
                SettingsHelper.Set(SettingsHelper.IsUseOldEmojiMode, value);
                isUseOldEmojiMode = SettingsHelper.Get<bool>(SettingsHelper.IsUseOldEmojiMode);
                RaisePropertyChangedEvent();
            }
        }

        public bool IsDisplayOriginPicture
        {
            get => isDisplayOriginPicture;
            set
            {
                SettingsHelper.Set(SettingsHelper.IsDisplayOriginPicture, value);
                isDisplayOriginPicture = SettingsHelper.Get<bool>(SettingsHelper.IsDisplayOriginPicture);
                RaisePropertyChangedEvent();
            }
        }

        public bool IsDarkMode
        {
            get => isDarkMode;
            set
            {
                SettingsHelper.Set(SettingsHelper.IsDarkMode, value);
                isDarkMode = SettingsHelper.Get<bool>(SettingsHelper.IsDarkMode);
                SettingsHelper.CheckTheme();
                RaisePropertyChangedEvent();
            }
        }

        public bool CheckUpdateWhenLuanching
        {
            get => checkUpdateWhenLuanching;
            set
            {
                SettingsHelper.Set(SettingsHelper.CheckUpdateWhenLuanching, value);
                checkUpdateWhenLuanching = SettingsHelper.Get<bool>(SettingsHelper.CheckUpdateWhenLuanching);
                RaisePropertyChangedEvent();
            }
        }

        public bool IsBackgroundColorFollowSystem
        {
            get => isBackgroundColorFollowSystem;
            set
            {
                SettingsHelper.Set(SettingsHelper.IsBackgroundColorFollowSystem, value);
                isBackgroundColorFollowSystem = SettingsHelper.Get<bool>(SettingsHelper.IsBackgroundColorFollowSystem);
                RaisePropertyChangedEvent();
                IsDarkModeSwitchVisibility = isBackgroundColorFollowSystem ? Visibility.Collapsed : Visibility.Visible;
                IsDarkMode = SettingsHelper.uiSettings.GetColorValue(Windows.UI.ViewManagement.UIColorType.Background).Equals(Windows.UI.Colors.Black);
            }
        }

        public Visibility IsDarkModeSwitchVisibility
        {
            get => isDarkModeSwitchVisibility;
            set
            {
                isDarkModeSwitchVisibility = value;
                RaisePropertyChangedEvent();
            }
        }

        public bool IsCleanCacheButtonEnabled
        {
            get => isCleanCacheButtonEnabled;
            set
            {
                isCleanCacheButtonEnabled = value;
                RaisePropertyChangedEvent();
            }
        }

        public bool IsCheckUpdateButtonEnabled
        {
            get => isCheckUpdateButtonEnabled;
            set
            {
                isCheckUpdateButtonEnabled = value;
                RaisePropertyChangedEvent();
            }
        }

        public SettingPage() => this.InitializeComponent();

        protected override void OnNavigatedTo(Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
#if DEBUG
            gotoTestPage.Visibility = Visibility.Visible;
#endif
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as FrameworkElement).Tag as string)
            {
                case "gotoTestPage": UIHelper.Navigate(typeof(TestPage)); break;
                case "checkUpdate":
                    IsCheckUpdateButtonEnabled = false;
                    await SettingsHelper.CheckUpdate();
                    IsCheckUpdateButtonEnabled = true;
                    break;
                case "reset":
                    bool b = true;
                    if (!string.IsNullOrEmpty(SettingsHelper.Get<string>(SettingsHelper.Uid)))
                    {
                        var loader = ResourceLoader.GetForCurrentView();
                        MessageDialog dialog = new MessageDialog(loader.GetString("SettingPageMessageDialogContent"), loader.GetString("SettingPageMessageDialogTitle"));
                        dialog.Commands.Add(new UICommand(loader.GetString("Yes")));
                        dialog.Commands.Add(new UICommand(loader.GetString("No")));
                        if ((await dialog.ShowAsync()).Label == loader.GetString("Yes"))
                            SettingsHelper.Logout();
                        else
                            b = false;
                    }
                    if (b)
                    {
                        ApplicationData.Current.LocalSettings.Values.Clear();
                        SettingsHelper.SetDefaultSettings();
                    }
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
            }
        }

        private void TitleBar_BackButtonClick(object sender, RoutedEventArgs e) => Frame.GoBack();
    }
}