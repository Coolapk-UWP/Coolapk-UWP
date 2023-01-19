using CoolapkUWP.Helpers;
using CoolapkUWP.Models;
using CoolapkUWP.Pages.BrowserPages;
using CoolapkUWP.ViewModels.FeedPages;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using TwoPaneView = Microsoft.UI.Xaml.Controls.TwoPaneView;
using TwoPaneViewMode = Microsoft.UI.Xaml.Controls.TwoPaneViewMode;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace CoolapkUWP.Pages.FeedPages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class FeedShellPage : Page
    {
        private FeedShellViewModel Provider;

        public FeedShellPage() => InitializeComponent();

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is FeedShellViewModel ViewModel)
            {
                Provider = ViewModel;
                DataContext = Provider;
                await Provider.Refresh(true);
                if (Provider.FeedDetail != null)
                {
                    SetLayout();
                }
            }
            await Task.Delay(30);
        }

        private void SetLayout()
        {
            TwoPaneView.MinWideModeWidth = Provider.FeedDetail?.IsFeedArticle ?? false ? 876 : 804;
            TwoPaneView.Pane1Length = new GridLength(Provider.FeedDetail?.IsFeedArticle ?? false ? 520 : 420);
        }

        private async void FeedButton_Click(object sender, RoutedEventArgs _)
        {
            void DisabledCopy()
            {
                if ((sender as FrameworkElement).DataContext is ICanCopy i)
                {
                    i.IsCopyEnabled = false;
                }
            }

            FrameworkElement element = sender as FrameworkElement;
            switch (element.Name)
            {
                case "MakeReplyButton":
                    DisabledCopy();
                    //ListViewItem item = element.FindAscendant<ListViewItem>();
                    //MakeFeedControl ctrl = item.FindName("makeFeed") as MakeFeedControl;
                    //ctrl.Visibility = ctrl.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
                    break;

                case "LikeButton":
                    DisabledCopy();
                    await RequestHelper.ChangeLikeAsync(element.Tag as ICanLike, element.Dispatcher);
                    break;

                case "ReportButton":
                    DisabledCopy();
                    UIHelper.Navigate(typeof(BrowserPage), new object[] { false, $"https://m.coolapk.com/mp/do?c=feed&m=report&type=feed&id={element.Tag}" });
                    break;

                case "ShareButton":
                    DisabledCopy();
                    break;

                default:
                    DisabledCopy();
                    UIHelper.OpenLinkAsync((sender as FrameworkElement).Tag as string);
                    break;
            }
        }

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

            if (BtnsPanel.Parent != null)
            {
                (BtnsPanel.Parent as Panel).Children.Remove(BtnsPanel);
            }
            else
            {
                LeftGrid.Children.Remove(BtnsPanel);
                RightGrid.Children.Remove(BtnsPanel);
            }

            // Single pane
            if (sender.Mode == TwoPaneViewMode.SinglePane)
            {
                ListControl.RefreshButtonVisibility = Visibility.Collapsed;
                // Add the details content to Pane1.
                RightGrid.Children.Add(HeaderControl);
                RightGrid.Children.Add(BtnsPanel);
                Pane2Grid.Children.Add(DetailControl);
            }
            // Dual pane.
            else
            {
                ListControl.RefreshButtonVisibility = Visibility.Visible;
                // Put details content in Pane2.
                LeftGrid.Children.Add(HeaderControl);
                LeftGrid.Children.Add(BtnsPanel);
                Pane1Grid.Children.Add(DetailControl);
            }
        }

        private void TwoPaneView_Loaded(object sender, RoutedEventArgs e)
        {
            TwoPaneView_ModeChanged(sender as TwoPaneView, null);
        }

        #endregion 界面模式切换
    }
}
