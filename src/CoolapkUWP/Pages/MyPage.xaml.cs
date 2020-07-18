using CoolapkUWP.Core.Helpers;
using CoolapkUWP.Helpers;
using CoolapkUWP.Models;
using CoolapkUWP.Pages.FeedPages;
using CoolapkUWP.ViewModels.FeedListPage;
using Newtonsoft.Json.Linq;
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
            if (name != null)
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private ViewModels.MyPage.ViewMode provider;

        private double badgeNum;
        private string badgeIconGlyph = "\uED0D";
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
            this.InitializeComponent();
            Loaded += (s, e) =>
           {
               LogoutButtonVisibility = string.IsNullOrEmpty(SettingsHelper.Get<string>(SettingsHelper.Uid))
                                                ? Visibility.Collapsed
                                                : Visibility.Visible;
           };
        }

        private async Task Refresh()
        {
            if (string.IsNullOrEmpty(SettingsHelper.Get<string>(SettingsHelper.Uid))) { return; }

            ring.IsActive = true;
            ring.Visibility = Visibility.Visible;

            await provider?.Refresh();
            mainGrid.DataContext = provider?.UserModel;
            repeater.ItemsSource = provider?.provider.Models;

            ring.IsActive = false;
            ring.Visibility = Visibility.Collapsed;
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
            scrollViewer.ChangeView(null, provider.VerticalOffsets[0], null, true);
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

        #region 搜索框相关

        private async void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                var (isSucceed, result) = await DataHelper.GetDataAsync(UriHelper.GetUri(UriType.SearchWords, sender.Text), true);
                sender.ItemsSource =
                    isSucceed &&
                    result is JArray array &&
                    array.Count > 0
                        ? (from i in array select new SearchWord(i as JObject))
                        : null;
            }
        }

        private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion is SearchWord word)
            {
                UIHelper.NavigateInSplitPane(typeof(SearchingPage), new ViewModels.SearchPage.ViewModel(word.Symbol == Symbol.Contact ? 1 : 0, word.GetTitle()));
            }
            else if (args.ChosenSuggestion is null)
            {
                UIHelper.NavigateInSplitPane(typeof(SearchingPage), new ViewModels.SearchPage.ViewModel(0, sender.Text));
            }
        }

        private void AutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            sender.Text = ((SearchWord)args.SelectedItem).GetTitle();
        }

        #endregion 搜索框相关

        private void NotificationNums_BadgeNumberChanged(object sender, System.EventArgs e)
        {
            if (sender is NotificationNums num)
                ChangeBadgeNum(num.BadgeNum);
            throw new System.NotImplementedException();
        }

        private void ChangeBadgeNum(double num) =>
            _ = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                BadgeNum = num;
                BadgeIconGlyph = num > 0 ? "\uED0C" : "\uED0D";
            });

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as FrameworkElement)?.Tag as string)
            {
                case "Notifications":
                    UIHelper.NavigateInSplitPane(typeof(NotificationsPage), new ViewModels.NotificationsPage.ViewModel(ViewModels.NotificationsPage.ListType.Comment));
                    break;

                case "feed":
                    var f = ViewModelBase.GetProvider(FeedListType.UserPageList, SettingsHelper.Get<string>(SettingsHelper.Uid));
                    if (f != null)
                        UIHelper.NavigateInSplitPane(typeof(FeedListPage), f);
                    break;

                case "follow":
                    Frame.Navigate(typeof(UserListPage), new ViewModels.UserListPage.ViewModel(SettingsHelper.Get<string>(SettingsHelper.Uid), true, "我"));
                    break;

                case "fans":
                    Frame.Navigate(typeof(UserListPage), new ViewModels.UserListPage.ViewModel(SettingsHelper.Get<string>(SettingsHelper.Uid), false, "我"));
                    break;
            }
        }

        private void ListViewItem_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            var tag = (sender as FrameworkElement)?.Tag;

            switch (tag as string)
            {
                case "settings":
                    Frame.Navigate(typeof(SettingPages.SettingPage));
                    break;

                case "login":
                    UIHelper.Navigate(typeof(BrowserPage), new object[] { true, null });
                    break;

                case "logout":
                    SettingsHelper.Logout();
                    LogoutButtonVisibility = Visibility.Collapsed;
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
                            UIHelper.NavigateInSplitPane(typeof(IndexPage), new ViewModels.IndexPage.ViewModel("#/topic/userFollowTagList&title=我关注的话题", true));
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
            Refresh();
        }

        private void TextBlockEx_RichTextBlockLoaded(object sender, System.EventArgs e)
        {
            var ele = (Controls.TextBlockEx)sender;
            ele.MaxLine = 2;
        }
    }

    public class FirstTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Others { get; set; }
        public DataTemplate TitleCard { get; set; }
        public DataTemplate TextLinkList { get; set; }
        public DataTemplate DataTemplate2 { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            if (item is IndexPageHasEntitiesModel m)
            {
                switch (m.EntitiesType)
                {
                    case EntityType.TextLinks: return TextLinkList;
                    default: return DataTemplate2;
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
            else return Others;
        }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container) => SelectTemplateCore(item);
    }

    public class SecondaryTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Null { get; set; }
        public DataTemplate Histroy { get; set; }
        public DataTemplate HistroyIcon { get; set; }
        public DataTemplate TextLink { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            switch ((item as IndexPageModel)?.EntityType)
            {
                case "topic":
                case "recentHistory": return HistroyIcon;
                case "textLink": return TextLink;
                case "collection":
                case "history": return Histroy;
                default: return null;
            }
        }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container) => SelectTemplateCore(item);
    }
}