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
            Settings.CheckLoginInfo();
            Tools.notifications.BadgeNumberChanged += (sender, e) =>
            {
                if (sender is NotificationsNum num) ChangeBadgeNum(num.BadgeNum);
            };
            ChangeBadgeNum(Tools.notifications.BadgeNum);
            if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Desktop")
                Windows.ApplicationModel.Core.CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            if (Settings.GetBoolen("CheckUpdateWhenLuanching")) Settings.CheckUpdate();
            SystemNavigationManager.GetForCurrentView().BackRequested += (sender, ee) =>
            {
                int i = Settings.HasStatusBar ? Tools.popups.Count - 1 : Tools.popups.Count - 2;
                if (i >= 0)
                {
                    ee.Handled = true;
                    Windows.UI.Xaml.Controls.Primitives.Popup popup = Tools.popups[i];
                    popup.IsOpen = false;
                    Tools.popups.Remove(popup);
                }
                else if (Frame.CanGoBack)
                {
                    ee.Handled = true;
                    Frame.GoBack();
                }
            };
            Tools.mainPage = this;
            navigationView.SelectedItem = navigationView.MenuItems[1];
            RegisterBackgroundTask();
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ApplicationView view = ApplicationView.GetForCurrentView();
            bool isInFullScreenMode = view.IsFullScreenMode;
            if (isInFullScreenMode || view.ViewMode == ApplicationViewMode.CompactOverlay)
                navigationView.Margin = new Thickness(0, 32, 0, 0);
            else navigationView.Margin = new Thickness(0, 0, 0, 0);
        }

        private void NavigationView_SelectionChanged(muxc.NavigationView sender, muxc.NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected)
            {
                _ = navigationViewFrame.Navigate(typeof(SettingPages.SettingPage), true, args.RecommendedNavigationTransitionInfo);
            }
            else if (args.SelectedItemContainer != null)
            {
                var navItemTag = args.SelectedItemContainer.Tag.ToString();
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
                        if (!navItemTag.StartsWith("V"))
                            _ = navigationViewFrame.Navigate(typeof(IndexPage), new object[] { "/page?url=V9_HOME_TAB_FOLLOW&type=" + navItemTag, true }, args.RecommendedNavigationTransitionInfo);
                        else if (navItemTag == "V9_HOME_TAB_HEADLINE")
                            _ = navigationViewFrame.Navigate(typeof(IndexPage), new object[] { "/main/indexV8", true }, args.RecommendedNavigationTransitionInfo);
                        else if (navItemTag == "V11_FIND_DYH")
                            _ = navigationViewFrame.Navigate(typeof(IndexPage), new object[] { "/user/dyhSubscribe", true }, args.RecommendedNavigationTransitionInfo);
                        else _ = navigationViewFrame.Navigate(typeof(IndexPage), new object[] { "/page?url=" + navItemTag, true }, args.RecommendedNavigationTransitionInfo);
                        break;
                }
            }
        }

        #region 搜索框相关
        private async void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                JsonArray array = Tools.GetDataArray(await Tools.GetJson($"/search/suggestSearchWordsNew?searchValue={sender.Text}&type=app"));
                if (array != null && array.Count > 0)
                {
                    ObservableCollection<object> observableCollection = new ObservableCollection<object>();
                    sender.ItemsSource = observableCollection;
                    foreach (var ite in array)
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
                                    Icon = await ImageCache.GetImage(ImageType.Icon, (item["logo"].GetString())),
                                    Size = item["apksize"].GetString(),
                                });
                                break;
                            case "searchWord":
                            default:
                                Symbol s = Symbol.Find;
                                if (item["logo"].GetString().Contains("cube")) s = Symbol.Shop;
                                else if (item["logo"].GetString().Contains("xitongguanli")) s = Symbol.AllApps;
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
                Tools.Navigate(typeof(AppPages.AppPage), "https://www.coolapk.com" + app.Url);
            else if (args.ChosenSuggestion is SearchWord word)
            {
                switch (word.Symbol)
                {
                    case Symbol.Shop:
                        Tools.Navigate(typeof(SearchPage), new object[] { 3, word.GetTitle() });
                        break;
                    case Symbol.Contact:
                        Tools.Navigate(typeof(SearchPage), new object[] { 1, word.GetTitle() });
                        break;
                    case Symbol.Find:
                        Tools.Navigate(typeof(SearchPage), new object[] { 0, word.Title });
                        break;
                }
            }
            else if (args.ChosenSuggestion is null) Tools.Navigate(typeof(SearchPage), new object[] { 0, sender.Text });
        }
        private void AutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            if (args.SelectedItem is ISearchPageViewModel m) sender.Text = m.GetTitle();
        }
        #endregion

        #region toIndexPage
        private void ChangeBadgeNum(double num)
            => Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
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
        #endregion

        private async void RegisterBackgroundTask()
        {
            BackgroundAccessStatus status = await BackgroundExecutionManager.RequestAccessAsync();
            if (status == BackgroundAccessStatus.Unspecified || status == BackgroundAccessStatus.DeniedByUser) return;

            foreach (var item in BackgroundTaskRegistration.AllTasks)
                if (item.Value.Name == "BackgroundTask")
                    item.Value.Unregister(true);
            BackgroundTaskBuilder taskBuilder = new BackgroundTaskBuilder
            {
                Name = "BackgroundTask",
                TaskEntryPoint = typeof(BackgroundTask).FullName
            };
            taskBuilder.AddCondition(new SystemCondition(SystemConditionType.InternetAvailable));
            taskBuilder.SetTrigger(new TimeTrigger(30, false));
            taskBuilder.Register();
        }
    }
}