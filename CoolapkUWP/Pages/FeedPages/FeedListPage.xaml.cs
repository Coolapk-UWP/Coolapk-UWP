using CoolapkUWP.Helpers;
using CoolapkUWP.Helpers.Providers;
using CoolapkUWP.Models.Pages.FeedListPageModels;
using CoolapkUWP.ViewModels;
using CoolapkUWP.ViewModels.FeedListPage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace CoolapkUWP.Pages.FeedPages
{
    public sealed partial class FeedListPage : Page
    {
        private ViewModelBase provider;

        public FeedListPage() => this.InitializeComponent();

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            titleBar.ShowProgressRing();

            provider = e.Parameter as ViewModelBase;
            listView.ItemsSource = provider.Models;

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
                    rightComboBox.SelectedIndex = (provider as ICanComboBoxChangeSelectedIndex).ComboBoxSelectedIndex;
                    break;

                case FeedListType.DyhPageList:
                    rightComboBox.Visibility = Visibility.Collapsed;
                    rightComboBox.ItemsSource = new string[]
                    {
                        loader.GetString("all"),
                        loader.GetString("square"),
                    };
                    rightComboBox.SelectedIndex = (provider as ICanComboBoxChangeSelectedIndex).ComboBoxSelectedIndex;
                    break;
            }
            await provider.LoadNextPage();
            await System.Threading.Tasks.Task.Delay(30);

            titleBar.Title = provider.Title;
            scrollViewer.ChangeView(null, provider.VerticalOffsets[0], null, true);
            if (provider.Models.Count > 0)
            {
                if (provider.Models[0] is DyhDetail detail)
                {
                    rightComboBox.Visibility = detail.ShowComboBox ? Visibility.Visible : Visibility.Collapsed;
                }
            }
            rightComboBox.SelectionChanged += FeedTypeComboBox_SelectionChanged;

            titleBar.HideProgressRing();
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            provider.VerticalOffsets[0] = scrollViewer.VerticalOffset;
            provider?.ChangeCopyMode(false);
            rightComboBox.SelectionChanged -= FeedTypeComboBox_SelectionChanged;
            titleBar.Title = string.Empty;

            base.OnNavigatingFrom(e);
        }

        private async void Refresh(int p = -1)
        {
            titleBar.ShowProgressRing();
            scrollViewer.ChangeView(null, 0, null);
            titleBar.Title = provider.Title;
            await provider.Refresh(p);

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
                    UIHelper.NavigateInSplitPane(typeof(UserListPage), new ViewModels.UserListPage.ViewModel(provider.Id, true, provider.Title));
                    break;

                case "fans":
                    UIHelper.NavigateInSplitPane(typeof(UserListPage), new ViewModels.UserListPage.ViewModel(provider.Id, false, provider.Title));
                    break;

                case "FollowUser":
                    var type =
                        (provider.Models[0] as UserDetail).FollowStatus == Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse("FeedListPage").GetString("follow")
                            ? UriType.OperateUnfollow
                            : UriType.OperateFollow;
                    await DataHelper.GetDataAsync(
                        UriProvider.GetObject(type).GetUri(provider.Id),
                        true);
                    Refresh();
                    break;

                case "copy":
                    provider.ChangeCopyMode((sender as ToggleMenuFlyoutItem).IsChecked);
                    break;

                case "report":
                    (provider as UserViewModel).Report();
                    break;

                default:
                    UIHelper.OpenLinkAsync(str);
                    break;
            }
        }

        private async void FeedTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(provider.Id)) { return; }
            var dataProvider = provider as ICanComboBoxChangeSelectedIndex;

            titleBar.ShowProgressRing();
            await dataProvider.SetComboBoxSelectedIndex((sender as ComboBox).SelectedIndex);

            if (provider.Models.Count == 1)
            {
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

        private void titleBar_RefreshButtonClicked(object sender, RoutedEventArgs e)
        {
            Refresh(-2);
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