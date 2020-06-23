using CoolapkUWP.Helpers;
using System;
using System.ComponentModel;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using static CoolapkUWP.Helpers.SettingsHelper;

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

        private bool isNoPicsMode = Get<bool>(SettingsHelper.IsNoPicsMode);
        private bool isUseOldEmojiMode = Get<bool>(SettingsHelper.IsUseOldEmojiMode);
        private bool isDisplayOriginPicture = Get<bool>(SettingsHelper.IsDisplayOriginPicture);
        private bool isDarkMode = Get<bool>(SettingsHelper.IsDarkMode);
        private bool checkUpdateWhenLuanching = Get<bool>(SettingsHelper.CheckUpdateWhenLuanching);
        private bool isBackgroundColorFollowSystem = Get<bool>(SettingsHelper.IsBackgroundColorFollowSystem);
        private const string issuePath = "https://github.com/Tangent-90/Coolapk-UWP/issues";
        private bool isCleanCacheButtonEnabled = true;
        private bool isCheckUpdateButtonEnabled = true;
        private bool showOtherException = Get<bool>(SettingsHelper.ShowOtherException);

        private string VersionTextBlockText
        {
            get
            {
                var ver = $"{Package.Current.Id.Version.Major}.{Package.Current.Id.Version.Minor}.{Package.Current.Id.Version.Build}";
                var loader = Windows.UI.Core.CoreWindow.GetForCurrentThread() == null ? null : ResourceLoader.GetForCurrentView();
                string name = loader?.GetString("AppName") ?? "CoolapkUWP";
                return $"{name} V{ver}";
            }
        }

        private bool IsNoPicsMode
        {
            get => isNoPicsMode;
            set
            {
                Set(SettingsHelper.IsNoPicsMode, value);
                isNoPicsMode = Get<bool>(SettingsHelper.IsNoPicsMode);
                RaisePropertyChangedEvent();
            }
        }

        private bool IsUseOldEmojiMode
        {
            get => isUseOldEmojiMode;
            set
            {
                Set(SettingsHelper.IsUseOldEmojiMode, value);
                isUseOldEmojiMode = Get<bool>(SettingsHelper.IsUseOldEmojiMode);
                RaisePropertyChangedEvent();
            }
        }

        private bool IsDisplayOriginPicture
        {
            get => isDisplayOriginPicture;
            set
            {
                Set(SettingsHelper.IsDisplayOriginPicture, value);
                isDisplayOriginPicture = Get<bool>(SettingsHelper.IsDisplayOriginPicture);
                RaisePropertyChangedEvent();
            }
        }

        private bool IsDarkMode
        {
            get => isDarkMode;
            set
            {
                Set(SettingsHelper.IsDarkMode, value);
                isDarkMode = Get<bool>(SettingsHelper.IsDarkMode);
                UIHelper.CheckTheme();
                RaisePropertyChangedEvent();
            }
        }

        private bool CheckUpdateWhenLuanching
        {
            get => checkUpdateWhenLuanching;
            set
            {
                Set(SettingsHelper.CheckUpdateWhenLuanching, value);
                checkUpdateWhenLuanching = Get<bool>(SettingsHelper.CheckUpdateWhenLuanching);
                RaisePropertyChangedEvent();
            }
        }

        private bool IsBackgroundColorFollowSystem
        {
            get => isBackgroundColorFollowSystem;
            set
            {
                Set(SettingsHelper.IsBackgroundColorFollowSystem, value);
                isBackgroundColorFollowSystem = Get<bool>(SettingsHelper.IsBackgroundColorFollowSystem);
                RaisePropertyChangedEvent();
                IsDarkMode = SettingsHelper.uiSettings.GetColorValue(Windows.UI.ViewManagement.UIColorType.Background).Equals(Windows.UI.Colors.Black);
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

        private bool ShowOtherException
        {
            get => showOtherException;
            set
            {
                Set(SettingsHelper.ShowOtherException, value);
                showOtherException = Get<bool>(SettingsHelper.ShowOtherException);
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
                case "gotoTestPage": Frame.Navigate(typeof(TestPage)); break;

                case "checkUpdate":
                    IsCheckUpdateButtonEnabled = false;
                    await SettingsHelper.CheckUpdateAsync();
                    IsCheckUpdateButtonEnabled = true;
                    break;

                case "reset":
                    bool b = true;
                    if (!string.IsNullOrEmpty(Get<string>(SettingsHelper.Uid)))
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
                        SetDefaultSettings();
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