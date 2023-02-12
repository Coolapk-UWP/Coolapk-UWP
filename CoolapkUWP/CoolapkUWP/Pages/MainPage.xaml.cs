﻿using CoolapkUWP.BackgroundTasks;
using CoolapkUWP.Controls;
using CoolapkUWP.Helpers;
using CoolapkUWP.Models;
using CoolapkUWP.Pages.BrowserPages;
using CoolapkUWP.Pages.FeedPages;
using CoolapkUWP.Pages.SettingsPages;
using CoolapkUWP.ViewModels.BrowserPages;
using CoolapkUWP.ViewModels.FeedPages;
using Microsoft.Toolkit.Uwp.Helpers;
using Microsoft.UI.Xaml.Controls;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Xml.Linq;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Resources;
using Windows.Foundation.Metadata;
using Windows.Phone.UI.Input;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using muxc = Microsoft.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace CoolapkUWP.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        private string _userName;
        public string UserName
        {
            get => _userName;
            set
            {
                if (_userName != value)
                {
                    _userName = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private ImageSource _userAvatar;
        public ImageSource UserAvatar
        {
            get => _userAvatar;
            set
            {
                if (_userAvatar != value)
                {
                    _userAvatar = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private NotificationsTask _notificationsTask;
        public NotificationsTask NotificationsTask
        {
            get => _notificationsTask;
            set
            {
                if (_notificationsTask != value)
                {
                    _notificationsTask = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private readonly List<(string Tag, Type Page)> _pages = new List<(string Tag, Type Page)>
        {
            ("Find", typeof(FindPage)),
            ("Home", typeof(IndexPage)),
            ("Circle", typeof(CirclePage)),
            ("Settings", typeof(SettingsPage)),
            ("Notifications", typeof(NotificationsPage))
        };

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        public MainPage()
        {
            InitializeComponent();
            UIHelper.MainPage = this;
            LiveTileTask.Instance?.UpdateTile();
            UIHelper.ShellDispatcher = Dispatcher;
            NotificationsTask.Instance?.GetNums();
            NotificationsTask = NotificationsTask.Instance;
            SearchBoxHolder.RegisterPropertyChangedCallback(Slot.IsStretchProperty, new DependencyPropertyChangedCallback(OnIsStretchProperty));
            NavigationView.RegisterPropertyChangedCallback(muxc.NavigationView.IsBackButtonVisibleProperty, new DependencyPropertyChangedCallback(OnIsBackButtonVisibleChanged));
            if (ApiInformation.IsMethodPresent("Windows.UI.Composition.Compositor", "TryCreateBlurredWallpaperBackdropBrush")) { BackdropMaterial.SetApplyToRootOrPageBackground(this, true); }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            OnLoginChanged(string.Empty, true);
            Window.Current?.SetTitleBar(DragRegion);
            SettingsHelper.LoginChanged += OnLoginChanged;
            if (ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
            {
                HardwareButtons.BackPressed += System_BackPressed;
            }
            SystemNavigationManager.GetForCurrentView().BackRequested += System_BackRequested;
            AppTitleText.Text = ResourceLoader.GetForViewIndependentUse().GetString("AppName") ?? "酷安";
            CoreApplication.GetCurrentView().TitleBar.LayoutMetricsChanged += (s, _) => UpdateAppTitle(s);
            if (e.Parameter is IActivatedEventArgs ActivatedEventArgs) { OpenActivatedEventArgs(ActivatedEventArgs); }
        }

        public string GetAppTitleFromSystem => Package.Current.DisplayName;

        private void OpenActivatedEventArgs(IActivatedEventArgs args)
        {
            switch (args.Kind)
            {
                case ActivationKind.Launch:
                    LaunchActivatedEventArgs LaunchActivatedEventArgs = (LaunchActivatedEventArgs)args;
                    if (LaunchActivatedEventArgs.Arguments != null)
                    {
                        switch (LaunchActivatedEventArgs.Arguments)
                        {
                            case "settings":
                                UIHelper.Navigate(typeof(SettingsPage));
                                break;
                            case "flags":
                                UIHelper.Navigate(typeof(TestPage));
                                break;
                            default:
                                NavigationView.SelectedItem = NavigationView.MenuItems[0];
                                break;
                        }
                    }
                    else if (LaunchActivatedEventArgs.TileActivatedInfo != null)
                    {
                        if (LaunchActivatedEventArgs.TileActivatedInfo.RecentlyShownNotifications.Any())
                        {
                            UIHelper.OpenLinkAsync(LaunchActivatedEventArgs.TileActivatedInfo.RecentlyShownNotifications.FirstOrDefault().Arguments);
                        }
                        else
                        {
                            NavigationView.SelectedItem = NavigationView.MenuItems[0];
                        }
                    }
                    else
                    {
                        NavigationView.SelectedItem = NavigationView.MenuItems[0];
                    }
                    break;
                case ActivationKind.Protocol:
                    IProtocolActivatedEventArgs ProtocolActivatedEventArgs = (IProtocolActivatedEventArgs)args;
                    switch (ProtocolActivatedEventArgs.Uri.Host)
                    {
                        case "www.coolapk.com":
                        case "coolapk.com":
                        case "www.coolmarket.com":
                        case "coolmarket.com":
                            UIHelper.OpenLinkAsync(ProtocolActivatedEventArgs.Uri.AbsolutePath);
                            break;
                        case "http":
                        case "https":
                            UIHelper.OpenLinkAsync($"{ProtocolActivatedEventArgs.Uri.Host}:{ProtocolActivatedEventArgs.Uri.AbsolutePath}");
                            break;
                        case "settings":
                            UIHelper.Navigate(typeof(SettingsPage));
                            break;
                        case "flags":
                            UIHelper.Navigate(typeof(TestPage));
                            break;
                        default:
                            UIHelper.OpenLinkAsync($"{(ProtocolActivatedEventArgs.Uri.AbsolutePath[0] == '/' ? string.Empty : "/")}{ProtocolActivatedEventArgs.Uri.AbsolutePath}");
                            break;
                    }
                    break;
                default:
                    NavigationView.SelectedItem = NavigationView.MenuItems[0];
                    break;
            }
        }

        private void OnIsStretchProperty(DependencyObject sender, DependencyProperty dp)
        {
            if (sender is Slot)
            {
                UpdateAppTitleIcon();
            }
        }

        private void OnIsBackButtonVisibleChanged(DependencyObject sender, DependencyProperty dp)
        {
            UpdateLeftPaddingColumn();
            UpdateAppTitleIcon();
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
            NavigationView.IsBackButtonVisible = NavigationViewFrame.CanGoBack
                ? muxc.NavigationViewBackButtonVisible.Visible
                : muxc.NavigationViewBackButtonVisible.Collapsed;
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

        private void NavigationViewControl_PaneClosing(muxc.NavigationView sender, muxc.NavigationViewPaneClosingEventArgs args)
        {
            UpdateLeftPaddingColumn();
        }

        private void NavigationViewControl_PaneOpening(muxc.NavigationView sender, object args)
        {
            UpdateLeftPaddingColumn();
        }

        private void UpdateLeftPaddingColumn()
        {
            LeftPaddingColumn.Width = NavigationView.PaneDisplayMode == muxc.NavigationViewPaneDisplayMode.Top
                ? NavigationView.IsBackButtonVisible != muxc.NavigationViewBackButtonVisible.Collapsed
                    ? new GridLength(48) : new GridLength(0)
                    : NavigationView.DisplayMode == muxc.NavigationViewDisplayMode.Minimal
                        ? NavigationView.IsPaneOpen ? new GridLength(72)
                        : NavigationView.IsPaneToggleButtonVisible
                            ? NavigationView.IsBackButtonVisible != muxc.NavigationViewBackButtonVisible.Collapsed
                            ? new GridLength(88) : new GridLength(48)
                                : NavigationView.IsBackButtonVisible != muxc.NavigationViewBackButtonVisible.Collapsed
                                ? new GridLength(48) : new GridLength(0)
                                    : NavigationView.IsBackButtonVisible != muxc.NavigationViewBackButtonVisible.Collapsed
                                    ? new GridLength(48) : new GridLength(0);
        }

        private void NavigationViewControl_DisplayModeChanged(muxc.NavigationView sender, muxc.NavigationViewDisplayModeChangedEventArgs args)
        {
            UpdateLeftPaddingColumn();
            UpdateAppTitleIcon();
        }

        private void UpdateAppTitleIcon()
        {
            AppTitleIcon.Margin = SearchBoxHolder.IsStretch
                && NavigationView.PaneDisplayMode != muxc.NavigationViewPaneDisplayMode.Top
                && NavigationView.DisplayMode != muxc.NavigationViewDisplayMode.Minimal
                    ? NavigationView.IsBackButtonVisible == muxc.NavigationViewBackButtonVisible.Visible
                        ? new Thickness(0, 0, 16, 0)
                        : new Thickness(24.5, 0, 24, 0)
                    : NavigationView.IsBackButtonVisible == muxc.NavigationViewBackButtonVisible.Visible
                        || NavigationView.IsPaneToggleButtonVisible
                        ? new Thickness(0, 0, 16, 0)
                        : new Thickness(16, 0, 16, 0);
        }

        private void OnLoginChanged(string sender, bool args) => _ = Dispatcher.AwaitableRunAsync(() => SetUserAvatar(args));

        private async void SetUserAvatar(bool isLogin)
        {
            if (isLogin && await SettingsHelper.CheckLoginAsync())
            {
                string UID = SettingsHelper.Get<string>(SettingsHelper.Uid);
                if (!string.IsNullOrEmpty(UID))
                {
                    (string UID, string UserName, string UserAvatar) results = await NetworkHelper.GetUserInfoByNameAsync(UID);
                    if (results.UID != UID) { return; }
                    UserName = results.UserName;
                    UserAvatar = new BitmapImage(new Uri(results.UserAvatar));
                }
            }
            else
            {
                UserName = null;
                UserAvatar = null;
            }
        }

        private void UpdateAppTitle(CoreApplicationViewTitleBar coreTitleBar)
        {
            //ensure the custom title bar does not overlap window caption controls
            RightPaddingColumn.Width = new GridLength(coreTitleBar.SystemOverlayRightInset);
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

        private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as FrameworkElement).Tag.ToString())
            {
                case "Login":
                    NavigationViewFrame.Navigate(typeof(BrowserPage), new BrowserViewModel(UriHelper.LoginUri));
                    break;
                case "Logout":
                    SettingsHelper.Logout();
                    break;
                case "Settings":
                    NavigationView.SelectedItem = NavigationView.FooterMenuItems.LastOrDefault();
                    break;
                case "CreateFeed":
                    new CreateFeedControl
                    {
                        FeedType = CreateFeedType.Feed,
                        PopupTransitions = new TransitionCollection
                        {
                            new EdgeUIThemeTransition
                            {
                                Edge = EdgeTransitionLocation.Bottom
                            }
                        }
                    }.Show();
                    break;
            }
        }

        #region 搜索框

        private async void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                (bool isSucceed, JToken result) = await RequestHelper.GetDataAsync(UriHelper.GetUri(UriType.SearchWords, sender.Text), true);
                if (isSucceed && result != null && result is JArray array && array.Count > 0)
                {
                    ObservableCollection<object> observableCollection = new ObservableCollection<object>();
                    sender.ItemsSource = observableCollection;
                    foreach (JToken token in array)
                    {
                        switch (token.Value<string>("entityType"))
                        {
                            case "apk":
                                observableCollection.Add(new SearchWord(token as JObject));
                                break;
                            case "searchWord":
                            default:
                                observableCollection.Add(new SearchWord(token as JObject));
                                break;
                        }
                    }
                }
            }
        }

        private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            //if (args.ChosenSuggestion is AppModel app)
            //{
            //    UIHelper.NavigateInSplitPane(typeof(AppPages.AppPage), "https://www.coolapk.com" + app.Url);
            //}
            //else
            if (args.ChosenSuggestion is SearchWord word)
            {
                NavigationViewFrame.Navigate(typeof(SearchingPage), new SearchingViewModel(word.ToString(), word.Glyph == "\uE77B" ? 1 : -1));
            }
            else if (args.ChosenSuggestion is null && !string.IsNullOrEmpty(sender.Text))
            {
                NavigationViewFrame.Navigate(typeof(SearchingPage), new SearchingViewModel(sender.Text));
            }
        }

        #endregion

        #region 状态栏

        public void ShowProgressBar()
        {
            _ = Dispatcher.AwaitableRunAsync(() =>
            {
                ProgressBar.Visibility = Visibility.Visible;
                ProgressBar.IsIndeterminate = true;
                ProgressBar.ShowError = false;
                ProgressBar.ShowPaused = false;
            });
        }

        public void ShowProgressBar(double value)
        {
            _ = Dispatcher.AwaitableRunAsync(() =>
            {
                ProgressBar.Visibility = Visibility.Visible;
                ProgressBar.IsIndeterminate = false;
                ProgressBar.ShowError = false;
                ProgressBar.ShowPaused = false;
                ProgressBar.Value = value;
            });
        }

        public void PausedProgressBar()
        {
            _ = Dispatcher.AwaitableRunAsync(() =>
            {
                ProgressBar.Visibility = Visibility.Visible;
                ProgressBar.IsIndeterminate = true;
                ProgressBar.ShowError = false;
                ProgressBar.ShowPaused = true;
            });
        }

        public void ErrorProgressBar()
        {
            _ = Dispatcher.AwaitableRunAsync(() =>
            {
                ProgressBar.Visibility = Visibility.Visible;
                ProgressBar.IsIndeterminate = true;
                ProgressBar.ShowPaused = false;
                ProgressBar.ShowError = true;
            });
        }

        public void HideProgressBar()
        {
            _ = Dispatcher.AwaitableRunAsync(() =>
            {
                ProgressBar.Visibility = Visibility.Collapsed;
                ProgressBar.IsIndeterminate = false;
                ProgressBar.ShowError = false;
                ProgressBar.ShowPaused = false;
                ProgressBar.Value = 0;
            });
        }

        public void ShowMessage(string message = null)
        {
            _ = Dispatcher.AwaitableRunAsync(() =>
            {
                if (CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar)
                {
                    AppTitleText.Text = message ?? ResourceLoader.GetForViewIndependentUse().GetString("AppName") ?? "酷安";
                }
                ApplicationView.GetForCurrentView().Title = message ?? string.Empty;
            });
        }

        #endregion
    }
}
