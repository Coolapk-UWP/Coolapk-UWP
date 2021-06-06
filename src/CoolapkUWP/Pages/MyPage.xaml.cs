using CoolapkUWP.Core.Helpers;
using CoolapkUWP.Helpers;
using CoolapkUWP.Models;
using CoolapkUWP.Pages.FeedPages;
using CoolapkUWP.ViewModels.AdaptivePage;
using CoolapkUWP.ViewModels.FeedListPage;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace CoolapkUWP.Pages
{
    public sealed partial class MyPage : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        private ViewModels.MyPage.ViewMode provider;

        private double badgeNum;
        private string badgeIconGlyph = "\uED0D";
        private Visibility logoutButtonVisibility;

        public Visibility LogoutButtonVisibility
        {
            get => logoutButtonVisibility;
            set
            {
                logoutButtonVisibility = value;
                RaisePropertyChangedEvent();
            }
        }

        private double BadgeNum
        {
            get => badgeNum;
            set
            {
                badgeNum = value;
                RaisePropertyChangedEvent();
            }
        }

        private string BadgeIconGlyph
        {
            get => badgeIconGlyph;
            set
            {
                badgeIconGlyph = value;
                RaisePropertyChangedEvent();
            }
        }

        public MyPage()
        {
            InitializeComponent();
            NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
            Loaded += (s, e) =>
           {
               LogoutButtonVisibility = string.IsNullOrEmpty(SettingsHelper.Get<string>(SettingsHelper.Uid))
                                                ? Visibility.Collapsed
                                                : Visibility.Visible;
           };
        }

        private async Task Refresh()
        {
            Windows.ApplicationModel.Resources.ResourceLoader loader = Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse();
            if (string.IsNullOrEmpty(SettingsHelper.Get<string>(SettingsHelper.Uid))) { return; }

            ShowProgressRing();

            try
            {
                await provider?.Refresh();
                mainGrid.DataContext = provider?.UserModel;
                repeater.ItemsSource = provider?.provider.Models;

                HideProgressRing();
            }
            catch
            {
                ErrorProgressBar();
                UIHelper.StatusBar_ShowMessage(loader.GetString("IndexPageError"));
            }
        }

        protected override async void OnNavigatedTo(Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            UIHelper.NotificationNums.BadgeNumberChanged += NotificationNums_BadgeNumberChanged;
            await SettingsHelper.CheckLoginInfo();
            ChangeBadgeNum(UIHelper.NotificationNums.BadgeNum);

            provider = (ViewModels.MyPage.ViewMode)e.Parameter;
            await Refresh();
            await Task.Delay(30);
            _ = scrollViewer.ChangeView(null, provider.VerticalOffsets[0], null, true);
        }

        protected override void OnNavigatingFrom(Windows.UI.Xaml.Navigation.NavigatingCancelEventArgs e)
        {
            UIHelper.NotificationNums.BadgeNumberChanged -= NotificationNums_BadgeNumberChanged;
            if (provider != null)
            {
                provider.VerticalOffsets[0] = scrollViewer.VerticalOffset;
            }

            base.OnNavigatingFrom(e);
        }

        private void ShowProgressRing()
        {
            if (mainGrid.Visibility == Visibility)
            {
                ShowProgressBar();
            }
            else
            {
                ring.IsActive = true;
                ring.Visibility = Visibility.Visible;
            }
        }

        private void HideProgressRing()
        {
            if (mainGrid.Visibility == Visibility)
            {
                HideProgressBar();
            }
            else
            {
                ring.Visibility = Visibility.Collapsed;
                ring.IsActive = false;
            }
        }

        #region 进度条相关

        public void ShowProgressBar()
        {
            LevelBar.IsIndeterminate = true;
            LevelBar.ShowError = false;
            LevelBar.ShowPaused = false;
        }

        public void PausedProgressBar()
        {
            LevelBar.IsIndeterminate = true;
            LevelBar.ShowError = false;
            LevelBar.ShowPaused = true;
        }

        public void ErrorProgressBar()
        {
            LevelBar.IsIndeterminate = true;
            LevelBar.ShowPaused = false;
            LevelBar.ShowError = true;
        }

        public void HideProgressBar()
        {
            LevelBar.IsIndeterminate = false;
            LevelBar.ShowError = false;
            LevelBar.ShowPaused = false;
        }

        #endregion

        #region 搜索框相关

        private static async void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                (bool isSucceed, JToken result) = await DataHelper.GetDataAsync(UriHelper.GetUri(UriType.SearchWords, sender.Text), true);
                if (isSucceed && result != null && result is JArray array && array.Count > 0)
                {
                    ObservableCollection<object> observableCollection = new ObservableCollection<object>();
                    sender.ItemsSource = observableCollection;
                    foreach (JToken token in array)
                    {
                        switch (token.Value<string>("entityType"))
                        {
                            case "apk":
                                observableCollection.Add(new AppPageMode(token as JObject));
                                break;
                            case "searchWord":
                            default:
                                observableCollection.Add(new SearchWord (token as JObject));
                                break;
                        }
                    }
                }
            }
        }

        private static void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion is AppPageMode app)
            {
                UIHelper.NavigateInSplitPane(typeof(AppPages.AppPage), "https://www.coolapk.com" + app.Url);
            }
            else if (args.ChosenSuggestion is SearchWord word)
            {
                UIHelper.NavigateInSplitPane(typeof(SearchingPage), new ViewModels.SearchPage.ViewModel(word.Symbol == Symbol.Contact ? 1 : 0, word.GetTitle()));
            }
            else if (args.ChosenSuggestion is null)
            {
                UIHelper.NavigateInSplitPane(typeof(SearchingPage), new ViewModels.SearchPage.ViewModel(0, sender.Text));
            }
        }

        private static void AutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            if (args.SelectedItem is SearchWord searchWord)
            {
                string Text = searchWord.GetTitle();
                sender.Text = Text;
            }
        }

        #endregion 搜索框相关

        private void NotificationNums_BadgeNumberChanged(object sender, System.EventArgs e)
        {
            if (sender is NotificationNums num)
            {
                ChangeBadgeNum(num.BadgeNum);
            }
            else { throw new System.NotImplementedException(); }
        }

        private void ChangeBadgeNum(double num) =>
            _ = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                BadgeNum = num;
                UIHelper.SetBadgeNumber(num.ToString());
                BadgeIconGlyph = num > 0 ? "\uED0C" : "\uED0D";
            });

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as FrameworkElement)?.Tag as string)
            {
                case "Notifications":
                    UIHelper.NavigateInSplitPane(typeof(NotificationsPage), new ViewModels.NotificationsPage.ViewModel(ViewModels.NotificationsPage.ListType.Comment));
                    break;

                case "MyFeed":
                    FeedListPageViewModelBase f = FeedListPageViewModelBase.GetProvider(FeedListType.UserPageList, SettingsHelper.Get<string>(SettingsHelper.Uid));
                    if (f != null) { UIHelper.NavigateInSplitPane(typeof(FeedListPage), f); }
                    break;

                case "feed":
                    string r = SettingsHelper.Get<string>(SettingsHelper.Uid);
                    if (r != null) { UIHelper.NavigateInSplitPane(typeof(AdaptivePage), new ViewModel(r, ListType.UserFeed, "feed")); }
                    break;

                case "follow":
                    _ = Frame.Navigate(typeof(UserListPage), new ViewModels.UserListPage.ViewModel(SettingsHelper.Get<string>(SettingsHelper.Uid), true, "我"));
                    break;

                case "fans":
                    _ = Frame.Navigate(typeof(UserListPage), new ViewModels.UserListPage.ViewModel(SettingsHelper.Get<string>(SettingsHelper.Uid), false, "我"));
                    break;

                default:
                    break;
            }
        }

        private void ListViewItem_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            object tag = (sender as FrameworkElement)?.Tag;

            switch (tag as string)
            {
                case "settings":
                    _ = Frame.Navigate(typeof(SettingPages.SettingPage));
                    break;

                case "login":
                    UIHelper.Navigate(typeof(BrowserPage), new object[] { true, null });
                    break;

                case "logout":
                    SettingsHelper.Logout();
                    LogoutButtonVisibility = Visibility.Collapsed;
                    break;

                case "LvHelp":
                    UIHelper.OpenLinkAsync("/feed/18221454");
                    break;

                default:
                    if (tag is string s)
                    {
                        if (s.Contains("我的常去", System.StringComparison.Ordinal))
                        {
                            UIHelper.NavigateInSplitPane(typeof(HistoryPage), new ViewModels.HistoryPage.ViewModel("我的常去"));
                        }
                        else if (s.Contains("浏览历史", System.StringComparison.Ordinal))
                        {
                            UIHelper.NavigateInSplitPane(typeof(HistoryPage), new ViewModels.HistoryPage.ViewModel("浏览历史"));
                        }
                        else if (s.Contains("我关注的话题", System.StringComparison.Ordinal))
                        {
                            UIHelper.NavigateInSplitPane(typeof(IndexPage), new ViewModels.IndexPage.ViewModel("#/topic/userFollowTagList", true));
                        }
                        else if (s.Contains("我的收藏单", System.StringComparison.Ordinal))
                        {
                        }
                        else if (s.Contains("我的问答", System.StringComparison.Ordinal))
                        {
                            string r = SettingsHelper.Get<string>(SettingsHelper.Uid);
                            if (r != null) { UIHelper.NavigateInSplitPane(typeof(AdaptivePage), new ViewModel(r, ListType.UserFeed, "questionAndAnswer")); }
                        }
                        else { UIHelper.OpenLinkAsync(tag as string); }
                    }
                    break;
            }
        }

        private void ListViewItem_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter || e.Key == Windows.System.VirtualKey.Space)
            {
                ListViewItem_Tapped(sender, null);
            }
        }

        private void MakeFeedControl_MakedFeedSuccessful(object sender, System.EventArgs e)
        {
            _ = Refresh();
        }

        private void TextBlockEx_RichTextBlockLoaded(object sender, System.EventArgs e)
        {
            Controls.TextBlockEx ele = (Controls.TextBlockEx)sender;
            ele.MaxLine = 2;
        }
    }

    public class FirstTemplateSelector : DataTemplateSelector
    {
        public DataTemplate App { get; set; }
        public DataTemplate Others { get; set; }
        public DataTemplate TitleCard { get; set; }
        public DataTemplate TextLinkList { get; set; }
        public DataTemplate ImageTextScrollCard { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            if (item is IndexPageHasEntitiesModel m)
            {
                switch (m.EntitiesType)
                {
                    case EntityType.TextLinks: return TextLinkList;
                    default: return ImageTextScrollCard;
                }
            }
            else if (item is IndexPageOperationCardModel o)
            {
                switch (o.OperationType)
                {
                    case OperationType.ShowTitle: return TitleCard;
                    default: return Others;
                }
            }
            else { return Others; }
        }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container) => SelectTemplateCore(item);
    }

    public class SecondaryTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Null { get; set; }
        public DataTemplate Histroy { get; set; }
        public DataTemplate IconLink { get; set; }
        public DataTemplate TextLink { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            switch ((item as IndexPageModel)?.EntityType)
            {
                case "topic":
                case "recentHistory": return IconLink;
                case "textLink": return TextLink;
                case "collection":
                case "history": return Histroy;
                default: return null;
            }
        }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container) => SelectTemplateCore(item);
    }
}