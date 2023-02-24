using CoolapkUWP.Helpers;
using CoolapkUWP.Models;
using CoolapkUWP.Pages.BrowserPages;
using CoolapkUWP.ViewModels.BrowserPages;
using CoolapkUWP.ViewModels.DataSource;
using CoolapkUWP.ViewModels.FeedPages;
using Microsoft.Toolkit.Uwp.UI;
using System;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
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
            if (e.Parameter is FeedListViewModel ViewModel
                && (Provider == null
                    || Provider.ListType != ViewModel.ListType
                    || Provider.ID != ViewModel.ID))
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

        private void FlipView_SizeChanged(object sender, SizeChangedEventArgs e) => (sender as FrameworkElement).MaxHeight = e.NewSize.Width / 2;

        private void FlipView_Loaded(object sender, RoutedEventArgs e)
        {
            if (SettingsHelper.Get<bool>(SettingsHelper.IsNoPicsMode))
            {
                if ((sender as FrameworkElement).Parent is FrameworkElement parent)
                { parent.Visibility = Visibility.Collapsed; }
            }
            else
            {
                FlipView view = sender as FlipView;
                view.MaxHeight = view.ActualWidth / 3;
                DispatcherTimer timer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(20)
                };
                timer.Tick += (o, a) =>
                {
                    if (view.SelectedIndex != -1)
                    {
                        if (view.SelectedIndex + 1 >= view.Items.Count)
                        {
                            while (view.SelectedIndex > 0)
                            {
                                view.SelectedIndex -= 1;
                            }
                        }
                        else
                        {
                            view.SelectedIndex += 1;
                        }
                    }
                };
                view.Unloaded += (_, __) => timer.Stop();
                timer.Start();
            }
        }

        private void Button_Loaded(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            ContentPresenter content = element.FindDescendant<ContentPresenter>();
            if (content != null)
            {
                switch (element.Name)
                {
                    case "OnlyOwnerButton":
                        content.CornerRadius = new CornerRadius(4, 0, 0, 4);
                        break;
                    case "SeeAllButton":
                        content.CornerRadius = new CornerRadius(0, 4, 4, 0);
                        break;
                    default:
                        break;
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            switch (element.Name)
            {
                case "FansButton":
                    UIHelper.Navigate(typeof(AdaptivePage), AdaptiveViewModel.GetUserListProvider(Provider.ID, false, Provider.Title));
                    break;
                case "ReportButton":
                    UIHelper.Navigate(typeof(BrowserPage), new BrowserViewModel(element.Tag.ToString()));
                    break;
                case "FollowButton":
                    _ = (element.Tag as ICanFollow).ChangeFollow();
                    break;
                case "PinTileButton":
                    _ = Provider.PinSecondaryTile(element.Tag as Entity);
                    break;
                case "FollowsButton":
                    UIHelper.Navigate(typeof(AdaptivePage), AdaptiveViewModel.GetUserListProvider(Provider.ID, true, Provider.Title));
                    break;
                default:
                    break;
            }
        }

        private void On_Tapped(object sender, TappedRoutedEventArgs e) => UIHelper.OpenLinkAsync((sender as FrameworkElement).Tag.ToString());

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
        public DataTemplate DyhDetail { get; set; }
        public DataTemplate UserDetail { get; set; }
        public DataTemplate TopicDetail { get; set; }
        public DataTemplate ProductDetail { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            switch (item.GetType().Name)
            {
                case "DyhDetail": return DyhDetail;
                case "UserDetail": return UserDetail;
                case "TopicDetail": return TopicDetail;
                case "ProductDetail": return ProductDetail;
                default: return Others;
            }
        }
    }
}
