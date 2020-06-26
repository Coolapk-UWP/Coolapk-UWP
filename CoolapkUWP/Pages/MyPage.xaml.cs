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

        private readonly Models.Controls.UserHubModel model = new Models.Controls.UserHubModel();
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
               if (string.IsNullOrEmpty(SettingsHelper.Get<string>(SettingsHelper.Uid)))
               {
                   LogoutButtonVisibility = Visibility.Collapsed;
               }
               else
               {
                   LogoutButtonVisibility = Visibility.Visible;
                   Refresh();
               }
           };
        }

        private async void Refresh()
        {
            var o = (JObject)await DataHelper.GetDataAsync(DataUriType.GetUserProfile, SettingsHelper.Get<string>(SettingsHelper.Uid));
            string url = o.Value<string>("userAvatar");
            var bitmapImage = await ImageCacheHelper.GetImageAsync(ImageType.BigAvatar, url);
            model.Initialize(o, bitmapImage);
        }

        protected override async void OnNavigatedTo(Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            UIHelper.NotificationNums.BadgeNumberChanged += NotificationNums_BadgeNumberChanged;
            await SettingsHelper.CheckLoginInfo();
            ChangeBadgeNum(UIHelper.NotificationNums.BadgeNum);
        }

        #region 搜索框相关

        private async void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                if (await DataHelper.GetDataAsync(DataUriType.SearchWords, sender.Text) is JArray array && array.Count > 0)
                    sender.ItemsSource = from i in array select new SearchWord(i as JObject);
                else
                    sender.ItemsSource = null;
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

        protected override void OnNavigatedFrom(Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            UIHelper.NotificationNums.BadgeNumberChanged -= NotificationNums_BadgeNumberChanged;
            base.OnNavigatedFrom(e);
        }

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
            switch ((sender as FrameworkElement)?.Tag as string)
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
    }
}