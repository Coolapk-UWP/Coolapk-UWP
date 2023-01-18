using CoolapkUWP.ViewModels.DataSource;
using CoolapkUWP.ViewModels.FeedPages;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using TwoPaneView = Microsoft.UI.Xaml.Controls.TwoPaneView;
using TwoPaneViewMode = Microsoft.UI.Xaml.Controls.TwoPaneViewMode;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace CoolapkUWP.Pages.FeedPages
{
    public enum FeedListType
    {
        TagPageList,
        DyhPageList,
        AppPageList,
        UserPageList,
        DevicePageList,
        ProductPageList,
        CollectionPageList,
    }

    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class FeedListPage : Page
    {
        private FeedListViewModel Provider;

        public FeedListPage() => InitializeComponent();

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is FeedListViewModel ViewModel)
            {
                Provider = ViewModel;
                DataContext = Provider;
                Provider.DataTemplateSelector = DetailTemplateSelector;
                await Refresh(true);
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }

        private async Task Refresh(bool reset = false)
        {
            await Provider.Refresh(reset);
            if (ListView.ItemsSource is EntityItemSourse entities)
            {
                _ = entities.Refresh(true);
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e) => _ = Refresh(true);

        private async void ListView_RefreshRequested(object sender, EventArgs e) => await Refresh(true);

        #region 界面模式切换

        private void TwoPaneView_ModeChanged(TwoPaneView sender, object args)
        {
            // Remove details content from it's parent panel.
            if (HeaderControl.Parent != null)
            {
                (HeaderControl.Parent as Panel).Children.Remove(HeaderControl);
            }
            else
            {
                LeftGrid.Children.Remove(HeaderControl);
                RightGrid.Children.Remove(HeaderControl);
            }

            if (DetailControl.Parent != null)
            {
                (DetailControl.Parent as Panel).Children.Remove(DetailControl);
            }
            else
            {
                Pane1Grid.Children.Remove(DetailControl);
                Pane2Grid.Children.Remove(DetailControl);
            }

            // Single pane
            if (sender.Mode == TwoPaneViewMode.SinglePane)
            {
                ListRefreshButton.Visibility = Visibility.Collapsed;
                HeaderRefreshButton.Visibility = Visibility.Visible;
                // Add the details content to Pane1.
                RightGrid.Children.Add(HeaderControl);
                Pane2Grid.Children.Add(DetailControl);
            }
            // Dual pane.
            else
            {
                ListRefreshButton.Visibility = Visibility.Visible;
                HeaderRefreshButton.Visibility = Visibility.Collapsed;
                // Put details content in Pane2.
                LeftGrid.Children.Add(HeaderControl);
                Pane1Grid.Children.Add(DetailControl);
            }
        }

        private void TwoPaneView_Loaded(object sender, RoutedEventArgs e)
        {
            TwoPaneView_ModeChanged(sender as TwoPaneView, null);
        }

        #endregion 界面模式切换
    }

    internal class DetailTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Others { get; set; }
        public DataTemplate UserDetail { get; set; }
        public DataTemplate TopicDetail { get; set; }
        protected override DataTemplate SelectTemplateCore(object item)
        {
            switch (item.GetType().Name)
            {
                case "UserDetail": return UserDetail;
                case "TopicDetail": return TopicDetail;
                default: return Others;
            }
        }
    }
}
