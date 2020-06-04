using CoolapkUWP.Helpers;
using CoolapkUWP.Pages.FeedPages.ViewModels;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace CoolapkUWP.Pages.FeedPages
{
    public sealed partial class FeedListPage : Page
    {
        FeedListDataProvider provider;
        ScrollViewer VScrollViewer;
        bool isLoading = true;
        public FeedListPage() => this.InitializeComponent();

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            provider = e.Parameter as FeedListDataProvider;
            listView.ItemsSource = provider.itemCollection;
            switch (provider.ListType)
            {
                case FeedListType.UserPageList:
                    titleBar.ComboBoxVisibility = Visibility.Collapsed;
                    break;
                case FeedListType.TagPageList:
                    titleBar.ComboBoxVisibility = Visibility.Visible;
                    titleBar.ComboBoxItemsSource = new string[] { "最近回复", "按时间排序", "按热度排序" };
                    titleBar.ComboBoxSelectedIndex = (provider as ICanChangeSelectedIndex).SelectedIndex;
                    break;
                case FeedListType.DyhPageList:
                    titleBar.ComboBoxVisibility = Visibility.Collapsed;
                    titleBar.ComboBoxItemsSource = new string[] { "精选", "广场" };
                    break;
            }
            Refresh();

            if (VScrollViewer is null)
            {
                Task.Run(async () =>
                {
                    await Task.Delay(300);
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        VScrollViewer = VisualTree.FindDescendantByName(listView, "ScrollViewer") as ScrollViewer;
                        VScrollViewer.ViewChanged += async (s, ee) =>
                        {
                            if (!ee.IsIntermediate && VScrollViewer.VerticalOffset == VScrollViewer.ScrollableHeight)
                            {
                                UIHelper.ShowProgressBar();
                                await provider.LoadNextPage();
                                UIHelper.HideProgressBar();
                            }
                        };
                    });
                });
            }
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            provider.ObjectToJsonString();
            if (!isLoading)
                titleBar.ComboBoxSelectionChange -= FeedTypeComboBox_SelectionChanged;

            base.OnNavigatedFrom(e);
        }
        async void Refresh()
        {
            UIHelper.ShowProgressBar();
            if (provider.JsonStringToObject()) await provider.Refresh();

            if (provider.itemCollection.Count > 0)
            {
                if (provider.itemCollection[0] is DyhDetail detail)
                {
                    titleBar.ComboBoxSelectedIndex = detail.SelectedIndex;
                    titleBar.ComboBoxVisibility = detail.ShowComboBox ? Visibility.Visible : Visibility.Collapsed;
                }
                titleBar.Title = provider.GetTitleBarText();
                if (isLoading)
                {
                    titleBar.ComboBoxSelectionChange += FeedTypeComboBox_SelectionChanged;
                    isLoading = false;
                }
            }

            UIHelper.HideProgressBar();
        }
        private void TitleBar_BackButtonClick(object sender, RoutedEventArgs e) => Frame.GoBack();

        private void UserDetailBorder_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender is FrameworkElement fe)
            {
                if (fe != e.OriginalSource) return;
                if (fe.Tag is string s)
                    if (s == (provider.itemCollection[0] as UserDetail).BackgroundUrl)
                        UIHelper.ShowImage(s, ImageType.OriginImage);
                    else UIHelper.ShowImage(s, ImageType.SmallAvatar);
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            string str = (sender as Button).Tag as string;
            switch (str)
            {
                case "follow":
                    UIHelper.Navigate(typeof(UserListPage), new object[] { provider.Id, true, titleBar.Title });
                    break;
                case "fans":
                    UIHelper.Navigate(typeof(UserListPage), new object[] { provider.Id, false, titleBar.Title });
                    break;
                case "FollowUser":
                    switch ((provider.itemCollection[0] as UserDetail).FollowStatus)
                    {
                        case "关注":
                            await DataHelper.GetData(DataUriType.OperateFollow, provider.Id);
                            break;
                        case "取消关注":
                            await DataHelper.GetData(DataUriType.OperateUnfollow, provider.Id);
                            break;
                    }
                    Refresh();
                    break;
                default:
                    UIHelper.OpenLink(str);
                    break;
            }
        }

        private void FeedTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(provider.Id)) return;
            ICanChangeSelectedIndex dataProvider = provider as ICanChangeSelectedIndex;
            dataProvider.SelectedIndex = (sender as ComboBox).SelectedIndex;
            dataProvider.Reset();
            if (provider.itemCollection.Count > 1)
            {
                for (int i = provider.itemCollection.Count - 1; i > 0; i--)
                    provider.itemCollection.RemoveAt(i);
                Refresh();
            }
        }
    }

    class UserDetail
    {
        public string UserFaceUrl;
        public ImageSource UserFace;
        public string UserName;
        public double FollowNum;
        public double FansNum;
        public double FeedNum;
        public double Level;
        public string Bio;
        public string BackgroundUrl;
        public ImageBrush Background;
        public string Verify_title;
        public string Gender;
        public string City;
        public string Astro;
        public string Logintime;
        public string FollowStatus;
        public bool ShowFollowStatus { get => !string.IsNullOrEmpty(FollowStatus); }
        public bool Has_bio { get => !string.IsNullOrEmpty(Bio); }
        public bool Has_verify_title { get => !string.IsNullOrEmpty(Verify_title); }
        public bool Has_Astro { get => !string.IsNullOrEmpty(Astro); }
        public bool Has_City { get => !string.IsNullOrWhiteSpace(City) && !string.IsNullOrEmpty(City); }
        public bool Has_Gender { get => !string.IsNullOrEmpty(Gender); }
    }
    class TopicDetail
    {
        public ImageSource Logo { get; set; }
        public string Title { get; set; }
        public double FollowNum { get; set; }
        public double CommentNum { get; set; }
        public string Description { get; set; }
        public int SelectedIndex { get; set; }
    }
    class DyhDetail
    {
        public ImageSource Logo { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public double FollowNum { get; set; }
        public bool ShowUserButton { get; set; }
        public string Url { get; set; }
        public string UserName { get; set; }
        public ImageSource UserAvatar { get; set; }
        public int SelectedIndex { get; set; }
        public bool ShowComboBox { get; set; }
    }
    class FeedListPageTemplateSelector : DataTemplateSelector
    {
        public DataTemplate UserHeader { get; set; }
        public DataTemplate Feed { get; set; }
        public DataTemplate TopicHeader { get; set; }
        public DataTemplate DyhHeader { get; set; }
        protected override DataTemplate SelectTemplateCore(object item)
        {
            if (item is UserDetail) return UserHeader;
            else if (item is TopicDetail) return TopicHeader;
            else if (item is DyhDetail) return DyhHeader;
            else return Feed;
        }
        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container) => SelectTemplateCore(item);
    }
}
