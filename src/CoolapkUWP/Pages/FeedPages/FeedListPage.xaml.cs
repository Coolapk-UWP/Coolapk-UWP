using CoolapkUWP.Core.Helpers;
using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Pages.FeedListPageModels;
using CoolapkUWP.ViewModels;
using CoolapkUWP.ViewModels.FeedListPage;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace CoolapkUWP.Pages.FeedPages
{
    public sealed partial class FeedListPage : Page
    {
        private FeedListPageViewModelBase provider;

        public FeedListPage() => this.InitializeComponent();

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            titleBar.ShowProgressRing();
            provider = e.Parameter as FeedListPageViewModelBase;
            listView.ItemsSource = provider.Models;
            provider.TitleUpdate += Provider_TitleUpdate;

            var loader = Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse("FeedListPage");
            switch (provider.ListType)
            {
                case FeedListType.UserPageList:
                    rightComboBox.Visibility = Visibility.Visible;
                    rightComboBox.ItemsSource = new string[]
                    {
                        loader.GetString("feed"),
                        loader.GetString("htmlFeed"),
                        loader.GetString("questionAndAnswer"),
                    };
                    rightComboBox.SelectedIndex = (provider as ICanComboBoxChangeSelectedIndex).ComboBoxSelectedIndex;
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

                case FeedListType.CollectionPageList:
                    rightComboBox.Visibility = Visibility.Visible;
                    rightComboBox.ItemsSource = ((CollectionViewModel)provider).ComboBoxItems;
                    rightComboBox.SelectedIndex = (provider as ICanComboBoxChangeSelectedIndex).ComboBoxSelectedIndex;
                    break;

                case FeedListType.ProductPageList:
                    rightComboBox.Visibility = Visibility.Visible;
                    rightComboBox.ItemsSource = new string[]
                    {
                        loader.GetString("feed"),
                        loader.GetString("answer"),
                        loader.GetString("article"),
                    };
                    rightComboBox.SelectedIndex = (provider as ICanComboBoxChangeSelectedIndex).ComboBoxSelectedIndex;
                    break;

                case FeedListType.AppPageList:
                    rightComboBox.Visibility = Visibility.Collapsed;
                    FindName(nameof(reportButton));
                    break;
            }
            _ = Refresh();
            titleBar.ShowProgressRing();
            await Task.Delay(30);

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

        private void Provider_TitleUpdate(object sender, System.EventArgs e)
        {
            titleBar.Title = provider.Title;
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            provider.VerticalOffsets[0] = scrollViewer.VerticalOffset;
            provider?.ChangeCopyMode(false);
            provider.TitleUpdate -= Provider_TitleUpdate;
            rightComboBox.SelectionChanged -= FeedTypeComboBox_SelectionChanged;
            titleBar.Title = string.Empty;

            base.OnNavigatingFrom(e);
        }

        private async Task Refresh(int p = -1)
        {
            titleBar.ShowProgressRing();
            if (p == -2)
            {
                scrollViewer.ChangeView(null, 0, null);
            }
            titleBar.Title = provider.Title;
            await provider.Refresh(p);

            titleBar.HideProgressRing();
        }

        private void TitleBar_BackButtonClick(object sender, RoutedEventArgs e) => Frame.GoBack();

        private static void UserDetailBorder_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (!(e == null || UIHelper.IsOriginSource(sender, e.OriginalSource))) { return; }
            if (e.OriginalSource.GetType() == typeof(Windows.UI.Xaml.Shapes.Ellipse)) { return; }
            if (sender is ListViewItem l && l.Tag is Models.IndexPageModel i)
            {
                UIHelper.OpenLinkAsync(i.Url);
            }
            else { return; }

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
                        UriHelper.GetUri(type, provider.Id),
                        true);
                    _ = Refresh();
                    break;

                case "copy":
                    provider.ChangeCopyMode((sender as ToggleMenuFlyoutItem).IsChecked);
                    break;

                case "report":
                    switch (provider.ListType)
                    {
                        case FeedListType.UserPageList:
                            (provider as UserViewModel).Report();
                            break;
                            //case FeedListType.AppPageList:
                            //    (provider as AppViewModel).Report();
                            //    break;
                    }
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
                _ = Refresh();
            }
        }

        private void scrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (!e.IsIntermediate && scrollViewer.VerticalOffset == scrollViewer.ScrollableHeight)
            {
                _ = Refresh();
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
            _ = Refresh(-2);
        }

        internal static void UserDetailBorder_Loaded(object sender, RoutedEventArgs e)
        {
            var b = sender as Border;
            b.Height = b.ActualWidth;
        }

        internal static void UserDetailBorder_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var b = sender as Border;
            b.Height = e.NewSize.Width;
        }
    }

    internal enum FeedListType
    {
        UserPageList,
        TagPageList,
        DyhPageList,
        CollectionPageList,
        ProductPageList,
        AppPageList,
    }

    internal class FeedListPageTemplateSelector : DataTemplateSelector
    {
        public DataTemplate UserHeader { get; set; }
        public DataTemplate Feed { get; set; }
        public DataTemplate TopicHeader { get; set; }
        public DataTemplate DyhHeader { get; set; }
        public DataTemplate CollectionHeader { get; set; }
        public DataTemplate ProductHeader { get; set; }
        public DataTemplate AppHeader { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            switch (item)
            {
                case UserDetail _: return UserHeader;
                case TopicDetail _: return TopicHeader;
                case DyhDetail _: return DyhHeader;
                case CollectionDetail _: return CollectionHeader;
                case ProductDetail _: return ProductHeader;
                case AppDetail _: return AppHeader;
                default: return Feed;
            }
        }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container) => SelectTemplateCore(item);
    }
}