using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace CoolapkUWP.Pages.FeedPages
{
    public sealed partial class UserListPage : Page
    {
        private ViewModels.UserListPage.ViewModel provider;

        public UserListPage() => this.InitializeComponent();

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            titleBar.ShowProgressRing();
            provider = e.Parameter as ViewModels.UserListPage.ViewModel;

            UserList.ItemsSource = provider.Models;
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

        private void TitleBar_RefreshButtonClick(object sender, RoutedEventArgs e) => _ = LoadList(-2);

        private void TitleBar_BackButtonClick(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }
    }
}