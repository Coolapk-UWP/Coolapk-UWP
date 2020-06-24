using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Pages.FeedListPageModels;
using CoolapkUWP.ViewModels.FeedListDataProvider;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace CoolapkUWP.Pages.FeedPages
{
    public sealed partial class FeedListPage : Page
    {
        private FeedListDataProvider provider;
        private bool isLoading = true;

        public FeedListPage() => this.InitializeComponent();

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            provider = e.Parameter as FeedListDataProvider;
            listView.ItemsSource = provider.itemCollection;
            var loader = Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse("FeedListPage");
            switch (provider.ListType)
            {
                case FeedListType.UserPageList:
                    rightComboBox.Visibility = Visibility.Collapsed;
                    FindName(nameof(reportButton));
                    break;

                case FeedListType.TagPageList:
                    rightComboBox.Visibility = Visibility.Visible;
                    rightComboBox.ItemsSource = new string[]
                    {
                        loader.GetString("lastupdate_desc"),
                        loader.GetString("dateline_desc"),
                        loader.GetString("popular"),
                    };
                    rightComboBox.SelectedIndex = (provider as ICanChangeSelectedIndex).SelectedIndex;
                    break;

                case FeedListType.DyhPageList:
                    rightComboBox.Visibility = Visibility.Collapsed;
                    rightComboBox.ItemsSource = new string[]
                    {
                        loader.GetString("all"),
                        loader.GetString("square"),
                    };
                    break;
            }
            Refresh();
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (!isLoading)
            {
                rightComboBox.SelectionChanged -= FeedTypeComboBox_SelectionChanged;
            }

            provider.ChangeCopyMode(false);

            base.OnNavigatingFrom(e);
        }

        private async void Refresh()
        {
            titleBar.ShowProgressRing();
            scrollViewer.ChangeView(null, 0, null);
            await provider.Refresh();
            if (provider.itemCollection.Count > 0)
            {
                if (provider.itemCollection[0] is DyhDetail detail)
                {
                    rightComboBox.Visibility = detail.ShowComboBox ? Visibility.Visible : Visibility.Collapsed;
                }
                titleBar.Title = provider.Title;
                if (isLoading)
                {
                    rightComboBox.SelectionChanged += FeedTypeComboBox_SelectionChanged;
                    isLoading = false;
                }
            }

            titleBar.HideProgressRing();
        }

        private void TitleBar_BackButtonClick(object sender, RoutedEventArgs e) => Frame.GoBack();

        private void UserDetailBorder_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (!(e == null || UIHelper.IsOriginSource(sender, e.OriginalSource))) { return; }
            if (e.OriginalSource.GetType() == typeof(Windows.UI.Xaml.Shapes.Ellipse) && sender.GetType() == typeof(ListViewItem)) { return; }
            UIHelper.ShowImage((sender as FrameworkElement)?.Tag as Models.ImageModel);
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            string str = (sender as FrameworkElement).Tag as string;
            switch (str)
            {
                case "follow":
                    UIHelper.NavigateInSplitPane(typeof(UserListPage), new object[] { provider.Id, true, titleBar.Title });
                    break;

                case "fans":
                    UIHelper.NavigateInSplitPane(typeof(UserListPage), new object[] { provider.Id, false, titleBar.Title });
                    break;

                case "FollowUser":
                    await DataHelper.GetDataAsync(
                        (provider.itemCollection[0] as UserDetail).FollowStatus == Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse("FeedListPage").GetString("follow")
                            ? DataUriType.OperateUnfollow
                            : DataUriType.OperateFollow,
                        provider.Id);
                    Refresh();
                    break;

                case "copy":
                    var b = sender as ToggleMenuFlyoutItem;
                    provider.ChangeCopyMode(b.IsChecked);
                    break;

                case "report":
                    (provider as UserPageDataProvider).Report();
                    break;

                default:
                    UIHelper.OpenLinkAsync(str);
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

        private async void scrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (!e.IsIntermediate && scrollViewer.VerticalOffset == scrollViewer.ScrollableHeight)
            {
                titleBar.ShowProgressRing();
                await provider.LoadNextPage();
                titleBar.HideProgressRing();
            }
        }

        private void ListViewItem_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter || e.Key == Windows.System.VirtualKey.Space)
            {
                UserDetailBorder_Tapped(sender, null);
            }
        }
    }

    internal enum FeedListType
    {
        UserPageList,
        TagPageList,
        DyhPageList
    }

    internal class FeedListPageTemplateSelector : DataTemplateSelector
    {
        public DataTemplate UserHeader { get; set; }
        public DataTemplate Feed { get; set; }
        public DataTemplate TopicHeader { get; set; }
        public DataTemplate DyhHeader { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            switch (item)
            {
                case UserDetail _: return UserHeader;
                case TopicDetail _: return TopicHeader;
                case DyhDetail _: return DyhHeader;
                default: return Feed;
            }
        }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container) => SelectTemplateCore(item);
    }
}