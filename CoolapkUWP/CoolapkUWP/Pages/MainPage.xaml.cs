using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Resources;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using muxc = Microsoft.UI.Xaml.Controls;
using Windows.Phone.UI.Input;
using CoolapkUWP.Helpers;
using CoolapkUWP.Pages.FeedPages;
using CoolapkUWP.ViewModels;
using CoolapkUWP.ViewModels.FeedPages;
using CoolapkUWP.Pages.SettingsPages;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace CoolapkUWP.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public Action NavigationViewLoaded { get; set; }
        
        private readonly List<(string Tag, Type Page)> _pages = new List<(string Tag, Type Page)>
        {
            ("Home", typeof(IndexPage)),
            ("Circle", typeof(CirclePage)),
            ("Settings", typeof(SettingsPage)),
        };

        public MainPage()
        {
            InitializeComponent();
            UIHelper.MainPage = this;
            //LiveTileTask.UpdateTile();
            UIHelper.ShellDispatcher = Dispatcher;
            //if (SettingsHelper.Get<bool>(SettingsHelper.CheckUpdateWhenLuanching)) { _ = CheckUpdate.CheckUpdateAsync(false, false); }
            if (ApiInformation.IsMethodPresent("Windows.UI.Composition.Compositor", "TryCreateBlurredWallpaperBackdropBrush")) { BackdropMaterial.SetApplyToRootOrPageBackground(this, true); }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            Window.Current.SetTitleBar(AppTitleBar);
            SystemNavigationManager.GetForCurrentView().BackRequested += System_BackRequested;
            if (ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
            {
                HardwareButtons.BackPressed += System_BackPressed;
            }
            CoreApplication.GetCurrentView().TitleBar.LayoutMetricsChanged += (s, _) => UpdateAppTitle(s);
        }

        public string GetAppTitleFromSystem => Package.Current.DisplayName;

        private void NavigationView_Loaded(object sender, RoutedEventArgs e)
        {
            // Add handler for ContentFrame navigation.
            NavigationViewFrame.Navigated += On_Navigated;
            NavigationView.SelectedItem = NavigationView.MenuItems[0];
            // Delay necessary to ensure NavigationView visual state can match navigation
            _ = Task.Delay(500).ContinueWith(_ => NavigationViewLoaded?.Invoke(), TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void NavigationView_Navigate(string NavItemTag, NavigationTransitionInfo TransitionInfo, object vs = null)
        {
            Type _page = null;

            (string Tag, Type Page) item = _pages.FirstOrDefault(p => p.Tag.Equals(NavItemTag, StringComparison.Ordinal));
            _page = item.Page;
            // Get the page type before navigation so you can prevent duplicate
            // entries in the backstack.
            Type PreNavPageType = NavigationViewFrame.CurrentSourcePageType;

            // Only navigate if the selected page isn't currently loaded.
            if (_page != null && !Equals(PreNavPageType, _page))
            {
                _ = NavigationViewFrame.Navigate(_page, vs, TransitionInfo);
            }
        }

        private void NavigationView_BackRequested(muxc.NavigationView sender, muxc.NavigationViewBackRequestedEventArgs args) => _ = TryGoBack();

        private void NavigationView_SelectionChanged(muxc.NavigationView sender, muxc.NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItemContainer != null)
            {
                string NavItemTag = args.SelectedItemContainer.Tag.ToString();
                NavigationView_Navigate(NavItemTag, args.RecommendedNavigationTransitionInfo);
            }
        }

        private bool TryGoBack()
        {
            if (!NavigationViewFrame.CanGoBack)
            { return false; }

            // Don't go back if the nav pane is overlayed.
            if (NavigationView.IsPaneOpen &&
                (NavigationView.DisplayMode == muxc.NavigationViewDisplayMode.Compact ||
                 NavigationView.DisplayMode == muxc.NavigationViewDisplayMode.Minimal))
            { return false; }

            NavigationViewFrame.GoBack();
            return true;
        }

        private void On_Navigated(object _, NavigationEventArgs e)
        {
            NavigationView.IsBackEnabled = NavigationViewFrame.CanGoBack;
            if (NavigationViewFrame.SourcePageType != null)
            {
                (string Tag, Type Page) item = _pages.FirstOrDefault(p => p.Page == e.SourcePageType);
                try
                {
                    NavigationView.SelectedItem = NavigationView.MenuItems
                        .OfType<muxc.NavigationViewItem>()
                        .First(n => n.Tag.Equals(item.Tag));
                }
                catch
                {
                    try
                    {
                        NavigationView.SelectedItem = NavigationView.FooterMenuItems
                            .OfType<muxc.NavigationViewItem>()
                            .First(n => n.Tag.Equals(item.Tag));
                    }
                    catch { }
                }
            }
            UIHelper.HideProgressBar();
        }

        private void NavigationViewControl_DisplayModeChanged(muxc.NavigationView sender, muxc.NavigationViewDisplayModeChangedEventArgs args)
        {
            if (ApiInformation.IsPropertyPresent("Windows.UI.Xaml.UIElement", "TranslationTransition"))
            {
                AppTitleBar.TranslationTransition = new Vector3Transition();

                AppTitleBar.Translation = sender.DisplayMode == muxc.NavigationViewDisplayMode.Minimal &&
                    sender.IsBackButtonVisible != muxc.NavigationViewBackButtonVisible.Collapsed &&
                    sender.PaneDisplayMode != muxc.NavigationViewPaneDisplayMode.Top
                    ? new System.Numerics.Vector3(((float)48 * 2) - 8, 0, 0)
                    : new System.Numerics.Vector3(48, 0, 0);
            }
            else
            {
                Thickness currMargin = AppTitleBar.Margin;

                AppTitleBar.Margin = sender.DisplayMode == muxc.NavigationViewDisplayMode.Minimal &&
                    sender.IsBackButtonVisible != muxc.NavigationViewBackButtonVisible.Collapsed &&
                    sender.PaneDisplayMode != muxc.NavigationViewPaneDisplayMode.Top
                    ? new Thickness((48 * 2) - 8, currMargin.Top, currMargin.Right, currMargin.Bottom)
                    : new Thickness(48, currMargin.Top, currMargin.Right, currMargin.Bottom);
            }
        }

        private void UpdateAppTitle(CoreApplicationViewTitleBar coreTitleBar)
        {
            //ensure the custom title bar does not overlap window caption controls
            Thickness currMargin = AppTitleBar.Margin;
            AppTitleBar.Margin = new Thickness(currMargin.Left, currMargin.Top, coreTitleBar.SystemOverlayRightInset, currMargin.Bottom);
        }

        private void System_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if (!e.Handled)
            {
                e.Handled = TryGoBack();
            }
        }

        private void System_BackPressed(object sender, BackPressedEventArgs e)
        {
            if (!e.Handled)
            {
                e.Handled = TryGoBack();
            }
        }

        #region 状态栏

        public void ShowProgressBar()
        {
            ProgressBar.Visibility = Visibility.Visible;
            ProgressBar.IsIndeterminate = true;
            ProgressBar.ShowError = false;
            ProgressBar.ShowPaused = false;
        }

        public void ShowProgressBar(double value)
        {
            ProgressBar.Visibility = Visibility.Visible;
            ProgressBar.IsIndeterminate = false;
            ProgressBar.ShowError = false;
            ProgressBar.ShowPaused = false;
            ProgressBar.Value = value;
        }

        public void PausedProgressBar()
        {
            ProgressBar.Visibility = Visibility.Visible;
            ProgressBar.IsIndeterminate = true;
            ProgressBar.ShowError = false;
            ProgressBar.ShowPaused = true;
        }

        public void ErrorProgressBar()
        {
            ProgressBar.Visibility = Visibility.Visible;
            ProgressBar.IsIndeterminate = true;
            ProgressBar.ShowPaused = false;
            ProgressBar.ShowError = true;
        }

        public void HideProgressBar()
        {
            ProgressBar.Visibility = Visibility.Collapsed;
            ProgressBar.IsIndeterminate = false;
            ProgressBar.ShowError = false;
            ProgressBar.ShowPaused = false;
            ProgressBar.Value = 0;
        }

        public void ShowMessage(string message = null)
        {
            if (CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar)
            {
                AppTitleText.Text = message ?? ResourceLoader.GetForViewIndependentUse().GetString("AppName") ?? "酷安";
            }
            else
            {
                ApplicationView.GetForCurrentView().Title = message ?? string.Empty;
            }
        }

        #endregion
    }
}
