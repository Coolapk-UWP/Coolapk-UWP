using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace CoolapkUWP.Pages.FeedPages
{
    public sealed partial class HistoryPage : Page
    {
        private ViewModels.HistoryPage.ViewModel provider;

        public HistoryPage()
        {
            InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            titleBar.ShowProgressRing();
            provider = e.Parameter as ViewModels.HistoryPage.ViewModel;

            list.ItemsSource = provider.Models;
            await LoadList(-2);
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

        private async Task LoadList(int p = -1)
        {
            titleBar.ShowProgressRing();
            if (p == -2)
            {
                scrollViewer?.ChangeView(null, 0, null);
                titleBar.Title = provider.Title;
            }
            await provider.Refresh(p);
            titleBar.HideProgressRing();
        }

        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (!e.IsIntermediate && scrollViewer.VerticalOffset == scrollViewer.ScrollableHeight)
            {
                _ = LoadList();
            }
        }

        private void TitleBar_RefreshButtonClick(object sender, RoutedEventArgs e)
        {
            _ = LoadList(-2);
        }

        private void TitleBar_BackButtonClick(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }

        private static void ListViewItem_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs _)
        {
            Helpers.UIHelper.OpenLinkAsync((sender as FrameworkElement)?.Tag as string);
        }

        private static void ListViewItem_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter || e.Key == Windows.System.VirtualKey.Space)
            {
                ListViewItem_Tapped(sender, null);
            }
        }
    }
}