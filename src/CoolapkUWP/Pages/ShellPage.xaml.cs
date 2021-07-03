using CoolapkUWP.BackgroundTasks;
using CoolapkUWP.Controls;
using CoolapkUWP.Helpers;
using System;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Navigation;

namespace CoolapkUWP.Pages
{
    public sealed partial class ShellPage : Page
    {
        public ShellPage()
        {
            InitializeComponent();
            Windows.ApplicationModel.Core.CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            if (SettingsHelper.Get<bool>(SettingsHelper.CheckUpdateWhenLuanching))
            {
                _ = CheckUpdate.CheckUpdateAsync(false, false);
            }

            SystemNavigationManager.GetForCurrentView().BackRequested += (sender, ee) =>
            {
                if (splitView.IsPaneOpen)
                {
                    ee.Handled = true;
                    splitView.IsPaneOpen = false;
                    UIHelper.IsSplitViewPaneOpen = splitView.IsPaneOpen;
                }

                if (shellFrame.CanGoBack)
                {
                    ee.Handled = true;
                    shellFrame.GoBack();
                }
            };

            Windows.UI.Notifications.TileUpdateManager.CreateTileUpdaterForApplication().Clear();

            UIHelper.IsSplitViewPaneOpenedChanged += (s, e) =>
            {
                splitView.IsPaneOpen = e;
                UIHelper.IsSplitViewPaneOpen = splitView.IsPaneOpen;
            };
#pragma warning disable 0612
            _ = ImageCacheHelper.CleanOldVersionImageCacheAsync();
#pragma warning restore 0612

            Popup popup = new Popup { RequestedTheme = SettingsHelper.Get<bool>("IsBackgroundColorFollowSystem") ? ElementTheme.Default : (SettingsHelper.Get<bool>("IsDarkMode") ? ElementTheme.Dark : ElementTheme.Light) };
            StatusGrid statusGrid2 = new StatusGrid();
            popup.Child = statusGrid2;
            UIHelper.popups.Add(popup);
            popup.IsOpen = true;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            string Version = "V9";
            base.OnNavigatedTo(e);
            if (ApplicationData.Current.LocalSettings.Values["Version"] != null)
            { Version = ApplicationData.Current.LocalSettings.Values["Version"].ToString(); }
            switch (Version)
            {
                case "V6":
                    _ = shellFrame.Navigate(typeof(MainPageV7));
                    break;
                case "V7":
                    _ = shellFrame.Navigate(typeof(MainPageV7));
                    break;
                default:
                    _ = shellFrame.Navigate(typeof(MainPage));
                    break;
            }
            _ = paneFrame.Navigate(typeof(MyPage), new ViewModels.MyPage.ViewMode());
            UIHelper.MainFrame = shellFrame;
            UIHelper.PaneFrame = paneFrame;
            UIHelper.InAppNotification = AppNotification;
            UIHelper.ShellDispatcher = Frame.Dispatcher;
            if (SettingsHelper.Get<bool>(SettingsHelper.IsFirstRun))
            {
                AboutDialog dialog = new AboutDialog();
                await dialog.ShowAsync();
                SettingsHelper.Set(SettingsHelper.IsFirstRun, false);
            }
            splitView.IsPaneOpen = Window.Current.Bounds.Width >= 960;
            UIHelper.IsSplitViewPaneOpen = splitView.IsPaneOpen;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            switch (((Button)sender).Tag as string)
            {
                case "panel":
                    splitView.IsPaneOpen = !splitView.IsPaneOpen;
                    UIHelper.IsSplitViewPaneOpen = splitView.IsPaneOpen;
                    break;

                case "home":
                    _ = paneFrame.Navigate(typeof(MyPage), new ViewModels.MyPage.ViewMode());
                    break;

                default:
                    break;
            }
        }

        private void paneFrame_Navigated(object sender, NavigationEventArgs e)
        {
            goHomeButton.Visibility = e.SourcePageType == typeof(MyPage) ? Visibility.Collapsed : Visibility.Visible;
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            bool canOpen = splitView.IsPaneOpen && (e?.NewSize.Width ?? Window.Current.Bounds.Width) >= 960;

            splitView.IsPaneOpen = canOpen;
            splitView.OpenPaneLength = Window.Current.Bounds.Width <= 660 ? Window.Current.Bounds.Width : 400;
            UIHelper.IsSplitViewPaneOverlay = splitView.DisplayMode == SplitViewDisplayMode.Overlay;
            UIHelper.IsSplitViewPaneOpen = splitView.IsPaneOpen;
        }
    }
}