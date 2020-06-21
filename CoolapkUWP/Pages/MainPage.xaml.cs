using CoolapkUWP.Helpers;
using CoolapkUWP.Models;
using CoolapkUWP.Pages.FeedPages;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using ImageSource = Windows.UI.Xaml.Media.ImageSource;
using MenuItem = CoolapkUWP.Models.Json.IndexPageHeaderItemModel.Item;

namespace CoolapkUWP.Pages
{
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        private MenuItem[] allMenuItems;
        private ImageSource userAvatar;
        private double badgeNum;
        private string badgeIconGlyph = ""; //"&#xED0D;"
        private MenuItem[] menuItems;

        public ImageSource UserAvatar
        {
            get => userAvatar;
            set
            {
                userAvatar = value;
                RaisePropertyChangedEvent();
            }
        }

        public double BadgeNum
        {
            get => badgeNum;
            private set
            {
                badgeNum = value;
                RaisePropertyChangedEvent();
            }
        }

        public string BadgeIconGlyph
        {
            get => badgeIconGlyph;
            private set
            {
                badgeIconGlyph = value;
                RaisePropertyChangedEvent();
            }
        }

        public MenuItem[] MenuItems
        {
            get => menuItems;
            private set
            {
                menuItems = value;
                RaisePropertyChangedEvent();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null)
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public MainPage()
        {
            this.InitializeComponent();
            GetIndexPageItems();
        }

        protected override async void OnNavigatedTo(Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            UserAvatar = await ImageCacheHelper.GetImageAsync(ImageType.BigAvatar, SettingsHelper.Get<string>(SettingsHelper.UserAvatar));
            UIHelper.NotificationNums.BadgeNumberChanged += NotificationNums_BadgeNumberChanged;
            UIHelper.UserAvatarChanged += UIHelper_UserAvatarChanged;
            await SettingsHelper.CheckLoginInfo();
            ChangeBadgeNum(UIHelper.NotificationNums.BadgeNum);
        }

        protected override void OnNavigatedFrom(Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            UIHelper.NotificationNums.BadgeNumberChanged -= NotificationNums_BadgeNumberChanged;
            UIHelper.UserAvatarChanged -= UIHelper_UserAvatarChanged;
            base.OnNavigatedFrom(e);
        }

        private void UIHelper_UserAvatarChanged(object sender, ImageSource e) => UserAvatar = e;

        private void NotificationNums_BadgeNumberChanged(object sender, EventArgs e)
        {
            if (sender is NotificationNums num)
                ChangeBadgeNum(num.BadgeNum);
        }

        private async void ChangeBadgeNum(double num) =>
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                BadgeNum = num;
                BadgeIconGlyph = num > 0 ? "" /*"&#xED0C;"*/ : ""; //"&#xED0D;"
            });

        #region 搜索框相关

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

        private async void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                if (await DataHelper.GetDataAsync(DataUriType.SearchWords, args: sender.Text) is JArray array && array.Count > 0)
                    sender.ItemsSource = from i in array select new SearchWord(i as JObject);
                else
                    sender.ItemsSource = null;
            }
        }

        private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion is SearchWord word)
            {
                if (word.Symbol == Symbol.Contact)
                    UIHelper.Navigate(typeof(SearchingPage), new object[] { 1, word.GetTitle() });
                else
                    UIHelper.Navigate(typeof(SearchingPage), new object[] { 0, word.Title });
            }
            else if (args.ChosenSuggestion is null)
                UIHelper.Navigate(typeof(SearchingPage), new object[] { 0, sender.Text });
        }

        private void AutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            if (args.SelectedItem is SearchWord m)
                sender.Text = m.GetTitle();
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (Window.Current.Bounds.Width < 768)
            {
                SearchBox.Visibility = Visibility.Collapsed;
                SearchButton.Visibility = Visibility.Visible;
            }
        }

        #endregion 搜索框相关

        private async void GetIndexPageItems()
        {
            var temp = await DataHelper.GetDataAsync<Models.Json.IndexPageHeaderItemModel.Rootobject>(DataUriType.GetIndexPageNames);
            MenuItems = await Task.Run(() =>
                (from t in temp.Data
                 where t.EntityTemplate == "configCard"
                 from ta in t.Entities
                 where ta.Title != "酷品"
                 where ta.Title != "看看号"
                 where ta.Title != "直播"
                 where ta.Title != "视频"
                 select new MenuItem(ta)).ToArray()
            );
            await Task.Run(() =>
                allMenuItems = menuItems.Concat(from i in menuItems
                                                where i.Entities != null
                                                from j in i.Entities
                                                select j).ToArray()
            );
            await Task.Delay(200);
            navigationView.SelectedItem = menuItems.First(i => i.Title == "头条");
            navigationViewFrame.Navigate(typeof(IndexPage), new object[] { "/main/indexV8", true });
        }

        private void NavigationView_ItemInvoked(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewItemInvokedEventArgs args)
        {
            var title = args.InvokedItem as string;
            if (title == "关注") return;
            var item = allMenuItems.First(i => i.Title == title);
            var uri = title == "头条" ? "/main/indexV8" : $"{item.Uri}&title={title}";
            navigationViewFrame.Navigate(typeof(IndexPage), new object[] { uri, true }, args.RecommendedNavigationTransitionInfo);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            switch (((FrameworkElement)sender).Tag as string)
            {
                case "UserPage":
                    UIHelper.NavigateInSplitPane(typeof(UserPage));
                    break;
                case "ShowSearchBar":
                    SearchBox.Visibility = Visibility.Visible;
                    SearchButton.Visibility = Visibility.Collapsed;
                    SearchBox.Focus(FocusState.Keyboard);
                    break;
                case "Notifications":
                    UIHelper.NavigateInSplitPane(typeof(NotificationsPage), new object[] { NotificationPageType.Comment });
                    break;
            }
        }
    }
}