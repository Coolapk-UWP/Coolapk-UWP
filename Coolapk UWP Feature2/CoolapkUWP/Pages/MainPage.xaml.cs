using CoolapkUWP.Control;
using CoolapkUWP.Control.ViewModels;
using CoolapkUWP.Data;
using CoolapkUWP.Pages.FeedPages;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Windows.ApplicationModel.Background;
using Windows.Data.Json;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using muxc = Microsoft.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace CoolapkUWP.Pages
{
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private ImageSource userAvatar;
        public ImageSource UserAvatar
        {
            get => userAvatar;
            set
            {
                userAvatar = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UserAvatar)));
            }
        }

        private string userNames;
        public string UserNames
        {
            get => userNames;
            set
            {
                if (value == null) { value = "个人中心"; }
                userNames = value;
                UserName.Text = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UserNames)));
            }
        }

        public MainPage()
        {
            InitializeComponent();
            _ = SettingsHelper.CheckLoginInfo();
            UIHelper.notifications.BadgeNumberChanged += (sender, e) =>
            {
                if (sender is NotificationsNum num) { ChangeBadgeNum(num.BadgeNum); }
            };
            ChangeBadgeNum(UIHelper.notifications.BadgeNum);
            if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Desktop")
            { Windows.ApplicationModel.Core.CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true; }
            if (SettingsHelper.GetBoolen("CheckUpdateWhenLuanching")) { _ = SettingsHelper.CheckUpdate(); }
            SystemNavigationManager.GetForCurrentView().BackRequested += (sender, ee) =>
            {
                int i = SettingsHelper.HasStatusBar ? UIHelper.popups.Count - 1 : UIHelper.popups.Count - 2;
                if (i >= 0)
                {
                    ee.Handled = true;
                    Windows.UI.Xaml.Controls.Primitives.Popup popup = UIHelper.popups[i];
                    popup.IsOpen = false;
                    _ = UIHelper.popups.Remove(popup);
                }
                else if (Frame.CanGoBack)
                {
                    ee.Handled = true;
                    Frame.GoBack();
                }
            };
            UIHelper.mainPage = this;
            navigationView.SelectedItem = navigationView.MenuItems[1];
            RegisterBackgroundTask();
            LiveTileControl.UpdateTile();
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ApplicationView view = ApplicationView.GetForCurrentView();
            bool isInFullScreenMode = view.IsFullScreenMode;
            navigationView.Margin = !(navigationView.PaneDisplayMode == muxc.NavigationViewPaneDisplayMode.Top) && (isInFullScreenMode || view.ViewMode == ApplicationViewMode.CompactOverlay)
                ? new Thickness(0, 32, 0, 0)
                : new Thickness(0, 0, 0, 0);
        }

        private void NavigationView_SelectionChanged(muxc.NavigationView sender, muxc.NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected)
            {
                _ = navigationViewFrame.Navigate(typeof(SettingPages.SettingPage), true, args.RecommendedNavigationTransitionInfo);
            }
            else if (args.SelectedItemContainer != null)
            {
                string navItemTag = args.SelectedItemContainer.Tag.ToString();
                switch (navItemTag)
                {
                    case "MakeFeed":
                        _ = navigationViewFrame.Navigate(typeof(MakeFeedPage), new object[] { MakeFeedMode.Feed }, args.RecommendedNavigationTransitionInfo);
                        break;
                    case "Notification":
                        _ = navigationViewFrame.Navigate(typeof(NotificationsPage), NotificationPageType.Comment, args.RecommendedNavigationTransitionInfo);
                        break;
                    case "UserHub":
                        _ = navigationViewFrame.Navigate(typeof(UserHubPage), args.RecommendedNavigationTransitionInfo);
                        break;
                    default:
                        _ = !navItemTag.StartsWith("V")
                            ? navigationViewFrame.Navigate(typeof(IndexPage), new object[] { "/page?url=V9_HOME_TAB_FOLLOW&type=" + navItemTag, true }, args.RecommendedNavigationTransitionInfo)
                            : navItemTag == "V9_HOME_TAB_HEADLINE"
                            ? navigationViewFrame.Navigate(typeof(IndexPage), new object[] { "/main/indexV8", true }, args.RecommendedNavigationTransitionInfo)
                            : navItemTag == "V11_FIND_DYH"
                            ? navigationViewFrame.Navigate(typeof(IndexPage), new object[] { "/user/dyhSubscribe", true }, args.RecommendedNavigationTransitionInfo)
                            : navigationViewFrame.Navigate(typeof(IndexPage), new object[] { "/page?url=" + navItemTag, true }, args.RecommendedNavigationTransitionInfo);
                        break;
                }
            }
            try
            {
                if (args.SelectedItemContainer.Content != null)
                {
                    navigationView.Header = args.SelectedItemContainer.Content;
                    navigationView.PaneTitle = args.SelectedItemContainer.Content.ToString();
                }
                else
                {
                    navigationView.Header = "酷安UWP";
                    navigationView.PaneTitle = " ";
                }
            }
            catch
            {
                navigationView.Header = "酷安UWP";
                navigationView.PaneTitle = " ";
            }
        }

        #region 搜索框相关
        private async void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                JsonArray array = UIHelper.GetDataArray(await UIHelper.GetJson($"/search/suggestSearchWordsNew?searchValue={sender.Text}&type=app"));
                if (array != null && array.Count > 0)
                {
                    ObservableCollection<object> observableCollection = new ObservableCollection<object>();
                    sender.ItemsSource = observableCollection;
                    foreach (IJsonValue ite in array)
                    {
                        JsonObject item = ite.GetObject();
                        switch (item["entityType"].GetString())
                        {
                            case "apk":
                                observableCollection.Add(new AppViewModel
                                {
                                    AppName = item["title"].GetString(),
                                    DownloadNum = $"{item["score"].GetString()}分 {item["downCount"].ToString().Replace("\"", string.Empty)}下载",
                                    Url = item["url"].GetString(),
                                    Icon = await ImageCache.GetImage(ImageType.Icon, item["logo"].GetString()),
                                    Size = item["apksize"].GetString(),
                                });
                                break;
                            case "searchWord":
                            default:
                                Symbol s = Symbol.Find;
                                if (item["logo"].GetString().Contains("cube")) { s = Symbol.Shop; }
                                else if (item["logo"].GetString().Contains("xitongguanli")) { s = Symbol.AllApps; }
                                observableCollection.Add(new SearchWord { Symbol = s, Title = item["title"].GetString() });
                                break;
                        }
                    }
                }
            }
        }
        private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion is AppViewModel app)
            { UIHelper.Navigate(typeof(AppPages.AppPage), "https://www.coolapk.com" + app.Url); }
            else if (args.ChosenSuggestion is SearchWord word)
            {
                switch (word.Symbol)
                {
                    case Symbol.Shop:
                        UIHelper.Navigate(typeof(SearchPage), new object[] { 3, word.GetTitle() });
                        break;
                    case Symbol.Contact:
                        UIHelper.Navigate(typeof(SearchPage), new object[] { 1, word.GetTitle() });
                        break;
                    case Symbol.Find:
                        UIHelper.Navigate(typeof(SearchPage), new object[] { 0, word.Title });
                        break;
                    default:
                        break;
                }
            }
            else if (args.ChosenSuggestion is null) { UIHelper.Navigate(typeof(SearchPage), new object[] { 0, sender.Text }); }
        }
        private void AutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            if (args.SelectedItem is ISearchPageViewModel m) { sender.Text = m.GetTitle(); }
        }
        #endregion

        #region toIndexPage
        private void ChangeBadgeNum(double num)
        {
            _ = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (num > 0)
                {
                    NotifyStatus.Glyph = "";// ED0C
                    Notify.Content = num.ToString() + "个通知";
                }
                else
                {
                    NotifyStatus.Glyph = "";// ED0D
                    Notify.Content = "通知";
                }
            });
        }
        #endregion

        private async void RegisterBackgroundTask()
        {
            BackgroundAccessStatus status = await BackgroundExecutionManager.RequestAccessAsync();
            if (status == BackgroundAccessStatus.Unspecified || status == BackgroundAccessStatus.DeniedByUser) { return; }

            foreach (System.Collections.Generic.KeyValuePair<Guid, IBackgroundTaskRegistration> item in BackgroundTaskRegistration.AllTasks)
            {
                if (item.Value.Name == "BackgroundTask")
                { item.Value.Unregister(true); }
            }
            BackgroundTaskBuilder taskBuilder = new BackgroundTaskBuilder
            {
                Name = "BackgroundTask",
                TaskEntryPoint = typeof(BackgroundTask).FullName
            };
            taskBuilder.AddCondition(new SystemCondition(SystemConditionType.InternetAvailable));
            taskBuilder.SetTrigger(new TimeTrigger(30, false));
            _ = taskBuilder.Register();
        }
    }
}