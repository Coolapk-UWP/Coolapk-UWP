using CoolapkUWP.ViewModels.FeedRepliesPage;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace CoolapkUWP.Pages.FeedPages
{
    public sealed partial class FeedRepliesPage : Page
    {
        private ViewModel provider;

        public FeedRepliesPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            titleBar.ShowProgressRing();
            provider = e.Parameter as ViewModel;

            FeedReplyList.ItemsSource = provider.Models;
            await GetReplys(false);

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

        private void VScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (!e.IsIntermediate && scrollViewer.VerticalOffset == scrollViewer.ScrollableHeight)
            {
                _ = GetReplys(false);
            }
        }

        private async Task GetReplys(bool isRefresh)
        {
            titleBar.ShowProgressRing();
            if (isRefresh)
            {
                scrollViewer?.ChangeView(null, 0, null);
                titleBar.Title = provider.Title;
            }
            await provider.Refresh(isRefresh ? 1 : -1);
            titleBar.HideProgressRing();
        }

        private void TitleBar_BackButtonClick(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }

        private void TitleBar_RefreshEvent(object sender, RoutedEventArgs e) => _ = GetReplys(true);
    }
}