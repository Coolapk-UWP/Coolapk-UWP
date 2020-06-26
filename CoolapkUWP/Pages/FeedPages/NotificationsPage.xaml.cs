using CoolapkUWP.Helpers;
using CoolapkUWP.Models;
using CoolapkUWP.Models.Pages.NotificationsPageModels;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace CoolapkUWP.Pages.FeedPages
{
    public sealed partial class NotificationsPage : Page
    {
        public NotificationsPage() => this.InitializeComponent();

        private ViewModels.NotificationsPage.ViewModel provider;

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            titleBar.ShowProgressRing();
            provider = e.Parameter as ViewModels.NotificationsPage.ViewModel;

            if (provider.ListType == ViewModels.NotificationsPage.ListType.Comment)
            {
                FindName(nameof(NavigateItems));
            }
            list.ItemsSource = provider.Models;
            await Load();

            await Task.Delay(30);
            titleBar.Title = provider.Title;
            scrollViewer.ChangeView(null, provider.VerticalOffsets[0], null, true);
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
            if (p == 1)
            {
                scrollViewer.ChangeView(null, 0, null);
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
                    Frame.Navigate(typeof(NotificationsPage), new ViewModels.NotificationsPage.ViewModel(ViewModels.NotificationsPage.ListType.AtMe));
                    break;

                case "atCommentMe":
                    Frame.Navigate(typeof(NotificationsPage), new ViewModels.NotificationsPage.ViewModel(ViewModels.NotificationsPage.ListType.AtCommentMe));
                    break;

                case "like":
                    Frame.Navigate(typeof(NotificationsPage), new ViewModels.NotificationsPage.ViewModel(ViewModels.NotificationsPage.ListType.Like));
                    break;

                case "follow":
                    Frame.Navigate(typeof(NotificationsPage), new ViewModels.NotificationsPage.ViewModel(ViewModels.NotificationsPage.ListType.Follow));
                    break;
            }
        }

        private void Ellipse_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (e.OriginalSource is Windows.UI.Xaml.Shapes.Ellipse
                || (sender is Grid && !(e.OriginalSource is Windows.UI.Xaml.Shapes.Ellipse)))
                UIHelper.OpenLinkAsync((sender as FrameworkElement).Tag as string);
        }

        private void titleBar_RefreshButtonClicked(object sender, RoutedEventArgs e) => _ = Load(1);

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
    }

    internal class NotificationsPageTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Feed { get; set; }
        public DataTemplate Reply { get; set; }
        public DataTemplate Like { get; set; }
        public DataTemplate AtCommentMe { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            switch (item)
            {
                case FeedModel _: return Feed;
                case LikeNotificationModel _: return Like;
                case AtCommentMeNotificationModel _: return AtCommentMe;
                default: return Reply;
            }
        }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container) => SelectTemplateCore(item);
    }
}