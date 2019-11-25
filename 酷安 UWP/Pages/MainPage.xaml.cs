using CoolapkUWP.Control.ViewModels;
using CoolapkUWP.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Windows.ApplicationModel.Background;
using Windows.Data.Json;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace CoolapkUWP.Pages
{
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        ImageSource userAvatar;
        public ImageSource UserAvatar
        {
            get => userAvatar;
            set
            {
                userAvatar = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UserAvatar)));
            }
        }
        bool a = true;
        static int seletedItem =
#if DEBUG
            0;
#else
        1;
#endif
        public MainPage()
        {
            this.InitializeComponent();
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
            RegisterBackgroundTask();
            GetIndexPageItems();
        }

        private void NavigateInVFrame(int index)
        {
            switch (index)
            {
                case 0:
                    VFrame.Navigate(typeof(UserHubPage));
                    seletedItem = 0;
                    SetNavItemBorder(2);
                    break;
                case 1:
                    TopNavListView.SelectedIndex = 0;
                    seletedItem = 1;
                    break;
                case 2:
                    TopNavListView.SelectedIndex = TopNavListView.Items.Count - 3;
                    seletedItem = 2;
                    break;
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
        List<string> IndexPageUrls = new List<string>();
        private Visibility indexPageNavButtonVisibility = Visibility.Collapsed;
        public Visibility IndexPageNavButtonVisibility
        {
            get => indexPageNavButtonVisibility;
            set
            {
                indexPageNavButtonVisibility = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IndexPageNavButtonVisibility)));
            }
        }
        private Visibility indexPageFollowNavButtonVisibility = Visibility.Collapsed;
        public Visibility IndexPageFollowNavButtonVisibility
        {
            get => indexPageFollowNavButtonVisibility;
            set
            {
                indexPageFollowNavButtonVisibility = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IndexPageFollowNavButtonVisibility)));
            }
        }
        private Visibility appPageFollowNavButtonVisibility = Visibility.Collapsed;
        public Visibility AppPageNavButtonVisibility
        {
            get => appPageFollowNavButtonVisibility;
            set
            {
                appPageFollowNavButtonVisibility = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AppPageNavButtonVisibility)));
            }
        }
        int followItemNum;
        async void GetIndexPageItems()
        {
            await Settings.CheckLoginInfo();
            Tools.notifications.BadgeNumberChanged += (sender, e) =>
            {
                if (sender is NotificationsNum num) ChangeBadgeNum(num.BadgeNum);
            };
            ChangeBadgeNum(Tools.notifications.BadgeNum);
            JsonArray array = Tools.GetDataArray(await Tools.GetJson("/main/init"));
            if (array != null & array.Count > 0)
            {
                int i = 1;
                foreach (var a in array)
                    if (a.GetObject()["entityTemplate"].GetString() == "configCard")
                        foreach (var b in a.GetObject()["entities"].GetArray())
                        {
                            JsonObject IndexPageNavItem = b.GetObject();
                            switch (IndexPageNavItem["title"].GetString())
                            {
                                case "酷品":
                                case "看看号":
                                case "直播": continue;
                            }
                            ListViewItem listViewItem = new ListViewItem { Content = new TextBlock { Text = IndexPageNavItem["title"].GetString() } };
                            if (IndexPageNavItem["title"].GetString() != "关注")
                                listViewItem.SetBinding(VisibilityProperty, new Windows.UI.Xaml.Data.Binding
                                {
                                    Source = this,
                                    Path = new PropertyPath("IndexPageNavButtonVisibility")
                                });
                            else listViewItem.Visibility = Visibility.Collapsed;
                            IndexPageUrls.Add(IndexPageNavItem["title"].GetString() == "头条" ? "/main/indexV8" : $"{IndexPageNavItem["url"].GetString()}&title={IndexPageNavItem["title"].GetString()}");
                            TopNavListView.Items.Insert(i++, listViewItem);
                            if (IndexPageNavItem["title"].GetString() == "关注")
                            {
                                foreach (var t in IndexPageNavItem["entities"].GetArray())
                                {
                                    followItemNum++;
                                    JsonObject followNavItem = t.GetObject();
                                    if (followNavItem["entityType"].GetString() == "page")
                                    {
                                        ListViewItem listViewItem2 = new ListViewItem { Content = new TextBlock { Text = followNavItem["title"].GetString() } };
                                        listViewItem2.SetBinding(VisibilityProperty, new Windows.UI.Xaml.Data.Binding
                                        {
                                            Source = this,
                                            Path = new PropertyPath("IndexPageFollowNavButtonVisibility")
                                        });
                                        TopNavListView.Items.Insert(i++, listViewItem2);
                                        IndexPageUrls.Add($"{followNavItem["url"].GetString()}&title={followNavItem["title"].GetString()}");
                                    }
                                }
                            }
                        }
            }
            if (a)
            {
                a = false;
                NavigateInVFrame(seletedItem);
            }
        }

        private void ChangeBadgeNum(double num)
            => Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                                                                 {
                                                                     if (num > 0)
                                                                     {
                                                                         NotifyStatus.Text = "";// ED0C
                                                                         NotifyNumber.Visibility = Visibility.Visible;
                                                                         NotifyNumber.Text = num.ToString();
                                                                     }
                                                                     else
                                                                     {
                                                                         NotifyStatus.Text = "";// ED0D
                                                                         NotifyNumber.Visibility = Visibility.Collapsed;
                                                                     }
                                                                 });

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TopNavListView.SelectedIndex == -1) return;
            else if (TopNavListView.SelectedIndex == 0)
            {
                (TopNavListView.Items[0] as FrameworkElement).Visibility = IndexPageFollowNavButtonVisibility = Visibility.Collapsed;
                (TopNavListView.Items[1] as FrameworkElement).Visibility = IndexPageNavButtonVisibility = Visibility.Visible;
                TopNavListView.SelectedIndex = 8;
                (TopNavListView.Items[TopNavListView.Items.Count - 4] as FrameworkElement).Visibility = Visibility.Visible;
                AppPageNavButtonVisibility = Visibility.Collapsed;
            }
            else if (TopNavListView.SelectedIndex == 1)
            {
                IndexPageFollowNavButtonVisibility = Visibility.Visible;
                (TopNavListView.Items[1] as FrameworkElement).Visibility = Visibility.Collapsed;
                TopNavListView.SelectedIndex = 2;
                (TopNavListView.Items[TopNavListView.Items.Count - 4] as FrameworkElement).Visibility = Visibility.Visible;
                AppPageNavButtonVisibility = Visibility.Collapsed;
            }
            else if (TopNavListView.SelectedIndex == TopNavListView.Items.Count - 5) return;
            else if (TopNavListView.SelectedIndex == TopNavListView.Items.Count - 4)
            {
                (TopNavListView.Items[0] as FrameworkElement).Visibility = Visibility.Visible;
                (TopNavListView.Items[1] as FrameworkElement).Visibility = IndexPageFollowNavButtonVisibility = IndexPageNavButtonVisibility = Visibility.Collapsed;
                (TopNavListView.Items[TopNavListView.Items.Count - 4] as FrameworkElement).Visibility = Visibility.Collapsed;
                AppPageNavButtonVisibility = Visibility.Visible;
                TopNavListView.SelectedIndex = TopNavListView.Items.Count - 3;
            }
            else
            {
                void gotoAppRecommendPage(int i)
                {
                    AppPageNavButtonVisibility = Visibility.Visible;
                    IndexPageFollowNavButtonVisibility = (TopNavListView.Items[1] as FrameworkElement).Visibility = (TopNavListView.Items[TopNavListView.Items.Count - 4] as FrameworkElement).Visibility = Visibility.Collapsed;
                    VFrame.Navigate(typeof(AppPages.AppRecommendPage), i);
                }
                if (TopNavListView.SelectedIndex == TopNavListView.Items.Count - 3) gotoAppRecommendPage(0);
                else if (TopNavListView.SelectedIndex == TopNavListView.Items.Count - 2) gotoAppRecommendPage(1);
                else if (TopNavListView.SelectedIndex == TopNavListView.Items.Count - 1) gotoAppRecommendPage(2);
                else if (TopNavListView.SelectedIndex > followItemNum)
                {
                    IndexPageFollowNavButtonVisibility = AppPageNavButtonVisibility = Visibility.Collapsed;
                    (TopNavListView.Items[1] as FrameworkElement).Visibility = (TopNavListView.Items[TopNavListView.Items.Count - 4] as FrameworkElement).Visibility = Visibility.Visible;
                    VFrame.Navigate(typeof(FeedPages.IndexPage), new object[] { IndexPageUrls[TopNavListView.SelectedIndex - 1], true, null });
                }
                else
                {
                    (TopNavListView.Items[1] as FrameworkElement).Visibility = AppPageNavButtonVisibility = Visibility.Collapsed;
                    IndexPageFollowNavButtonVisibility = (TopNavListView.Items[TopNavListView.Items.Count - 4] as FrameworkElement).Visibility = Visibility.Visible;
                    VFrame.Navigate(typeof(FeedPages.IndexPage), new object[] { IndexPageUrls[TopNavListView.SelectedIndex - 1], true, null });
                }
            }
            SetNavItemBorder(0);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            (TopNavListView.Items[1] as FrameworkElement).Visibility = AppPageNavButtonVisibility = IndexPageFollowNavButtonVisibility = IndexPageNavButtonVisibility = Visibility.Collapsed;
            (TopNavListView.Items[0] as FrameworkElement).Visibility = (TopNavListView.Items[TopNavListView.Items.Count - 4] as FrameworkElement).Visibility = Visibility.Visible;
            if (sender == UserButton)
            {
                VFrame.Navigate(typeof(UserHubPage));
                SetNavItemBorder(2);
            }
            else if (sender == NotifiesCenterButton)
            {
                VFrame.Navigate(typeof(NotificationsPage), NotificationPageType.Comment);
                SetNavItemBorder(1);
            }
            else if (sender == MakeFeedButton)
            {
                VFrame.Navigate(typeof(FeedPages.MakeFeedPage), new object[] { FeedPages.MakeFeedMode.Feed });
                SetNavItemBorder(3);
            }
        }

        void SetNavItemBorder(int mode)
        {
            Thickness thickness = new Thickness(0);
            foreach (object item in TopNavListView.Items)
            {
                ListViewItem listViewItem = item as ListViewItem;
                if (listViewItem is null) continue;
                listViewItem.BorderThickness = thickness;
            }
            MakeFeedButton.BorderThickness = NotifiesCenterButton.BorderThickness = UserButton.BorderThickness = thickness;
            switch (mode)
            {
                case 0:
                    (TopNavListView.Items[TopNavListView.SelectedIndex] as ListViewItem).BorderThickness = new Thickness(0, 0, 0, 2);
                    break;
                case 1:
                    NotifiesCenterButton.BorderThickness = new Thickness(0, 0, 0, 2);
                    break;
                case 2:
                    UserButton.BorderThickness = new Thickness(0, 0, 0, 2);
                    break;
                case 3:
                    MakeFeedButton.BorderThickness = new Thickness(0, 0, 0, 2);
                    break;
            }
        }
        #endregion
        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width >= 768)
            {
                SearchBox.Visibility = Visibility.Visible;
                SearchButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                SearchBox.Visibility = Visibility.Collapsed;
                SearchButton.Visibility = Visibility.Visible;
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            SearchBox.Visibility = Visibility.Visible;
            SearchButton.Visibility = Visibility.Collapsed;
            SearchBox.Focus(FocusState.Keyboard);
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (Window.Current.Bounds.Width < 768)
            {
                SearchBox.Visibility = Visibility.Collapsed;
                SearchButton.Visibility = Visibility.Visible;
            }
        }

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