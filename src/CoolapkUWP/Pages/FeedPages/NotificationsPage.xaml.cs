using CoolapkUWP.Helpers;
using CoolapkUWP.Models;
using CoolapkUWP.Models.Pages.NotificationsPageModels;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace CoolapkUWP.Pages.FeedPages
{
    public sealed partial class NotificationsPage : Page
    {
        private ViewModels.NotificationsPage.ViewModel provider;

        public NotificationsPage()
        {
            InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            titleBar.ShowProgressRing();
            provider = e.Parameter as ViewModels.NotificationsPage.ViewModel;

            if (provider.ListType == ViewModels.NotificationsPage.ListType.Comment)
            {
                _ = FindName(nameof(NavigateItems));
            }
            list.ItemsSource = provider.Models;
            await Load(-2);

            await Task.Delay(30);
            titleBar.Title = provider.Title;
            _ = scrollViewer.ChangeView(null, provider.VerticalOffsets[0], null, true);
            titleBar.HideProgressRing();
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            provider.VerticalOffsets[0] = scrollViewer.VerticalOffset;
            titleBar.Title = string.Empty;

            base.OnNavigatingFrom(e);
        }

        private async Task Load(int p = -1)
        {
            titleBar.ShowProgressRing();
            if (p == -2)
            {
                _ = scrollViewer.ChangeView(null, 0, null);
                titleBar.Title = provider.Title;
            }
            await provider?.Refresh(p);
            titleBar.HideProgressRing();
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            switch ((e.ClickedItem as StackPanel).Tag as string)
            {
                case "atMe":
                    _ = Frame.Navigate(typeof(NotificationsPage), new ViewModels.NotificationsPage.ViewModel(ViewModels.NotificationsPage.ListType.AtMe));
                    break;

                case "atCommentMe":
                    _ = Frame.Navigate(typeof(NotificationsPage), new ViewModels.NotificationsPage.ViewModel(ViewModels.NotificationsPage.ListType.AtCommentMe));
                    break;

                case "like":
                    _ = Frame.Navigate(typeof(NotificationsPage), new ViewModels.NotificationsPage.ViewModel(ViewModels.NotificationsPage.ListType.Like));
                    break;

                case "follow":
                    _ = Frame.Navigate(typeof(NotificationsPage), new ViewModels.NotificationsPage.ViewModel(ViewModels.NotificationsPage.ListType.Follow));
                    break;

                case "message":
                    _ = Frame.Navigate(typeof(NotificationsPage), new ViewModels.NotificationsPage.ViewModel(ViewModels.NotificationsPage.ListType.Message));
                    break;
                default:
                    break;
            }
        }

        private static void Ellipse_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (e.OriginalSource is Windows.UI.Xaml.Shapes.Ellipse ||
                (sender is Grid && !(e.OriginalSource is Windows.UI.Xaml.Shapes.Ellipse)))
            { UIHelper.OpenLinkAsync((sender as FrameworkElement).Tag as string); }
        }

        private void titleBar_RefreshButtonClicked(object sender, RoutedEventArgs e) => _ = Load(-2);

        private void TitleBar_BackButtonClick(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }

        private void scrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (!e.IsIntermediate && scrollViewer.VerticalOffset == scrollViewer.ScrollableHeight)
            {
                _ = Load();
            }
        }

        private async void RefreshContainer_RefreshRequested(RefreshContainer sender, RefreshRequestedEventArgs args)
        {
            using (Windows.Foundation.Deferral RefreshCompletionDeferral = args.GetDeferral())
            {
                await Load(-2);
            }
        }
    }

    internal class NotificationsPageTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Feed { get; set; }
        public DataTemplate Reply { get; set; }
        public DataTemplate Like { get; set; }
        public DataTemplate AtCommentMe { get; set; }
        public DataTemplate Message { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            switch (item)
            {
                case FeedModel _: return Feed;
                case LikeNotificationModel _: return Like;
                case AtCommentMeNotificationModel _: return AtCommentMe;
                case MessageNotificationModel _: return Message;
                default: return Reply;
            }
        }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container) => SelectTemplateCore(item);
    }
}