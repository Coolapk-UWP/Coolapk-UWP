using CoolapkUWP.Helpers;
using CoolapkUWP.Models;
using CoolapkUWP.Pages.FeedPages;
using CoolapkUWP.ViewModels.FeedListPage;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace CoolapkUWP.Controls.DataTemplates
{
    public partial class Feed : ResourceDictionary
    {
        public Feed() => InitializeComponent();

        internal static void OnTapped(object sender, TappedRoutedEventArgs e)
        {
            var s = sender as FrameworkElement;
            if (e != null && !UIHelper.IsOriginSource(sender, e.OriginalSource)) { return; }
            if ((s.DataContext as ICanCopy)?.IsCopyEnabled ?? false) { return; }

            if (e != null) { e.Handled = true; }

            UIHelper.OpenLinkAsync(s.Tag as string);
        }

        internal static async void FeedButton_Click(object sender, RoutedEventArgs e)
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
                case "makeReplyButton":
                    DisabledCopy();
                    var item = element.FindAscendant<ListViewItem>();
                    var ctrl = item.FindName("makeFeed") as MakeFeedControl;
                    ctrl.Visibility = ctrl.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
                    break;

                case "likeButton":
                    DisabledCopy();
                    await DataHelper.MakeLikeAsync(
                        element.Tag as ICanChangeLikModel,
                        element.Dispatcher,
                        (SymbolIcon)element.FindName("like1"),
                        (SymbolIcon)element.FindName("like2"));
                    break;

                case "reportButton":
                    DisabledCopy();
                    UIHelper.Navigate(typeof(Pages.BrowserPage), new object[] { false, $"https://m.coolapk.com/mp/do?c=feed&m=report&type=feed&id={element.Tag}" });
                    break;

                case "shareButton":
                    DisabledCopy();
                    break;

                case "deviceButton":
                    var f = FeedListPageViewModelBase.GetProvider(FeedListType.DevicePageList, (sender as FrameworkElement).Tag as string);
                    if (f != null)
                    {
                        UIHelper.NavigateInSplitPane(typeof(FeedListPage), f);
                    }
                    break;

                case "changeButton":
                    DisabledCopy();
                    UIHelper.NavigateInSplitPane(typeof(FeedOnlyPage), new ViewModels.FeedOnlyPage.ViewModel((sender as FrameworkElement).Tag as string, ViewModels.FeedOnlyPage.ListType.FeedInfo, "changeHistory"));
                    break;

                default:
                    DisabledCopy();
                    UIHelper.OpenLinkAsync((sender as FrameworkElement).Tag as string);
                    break;
            }
        }

        internal static void ListViewItem_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (UIHelper.IsOriginSource(sender, e.OriginalSource))
            {
                if (e.Key == Windows.System.VirtualKey.Enter || e.Key == Windows.System.VirtualKey.Space)
                {
                    OnTapped(sender, null);
                }
                else if (e.Key == Windows.System.VirtualKey.Menu)
                {
                    ListViewItem_RightTapped(sender, null);
                }
            }
        }

        internal static void makeFeed_MakedFeedSuccessful(object sender, System.EventArgs e)
        {
            if (((FrameworkElement)sender).Tag is ICanChangeReplyNum m)
            {
                m.Replynum = (int.Parse(m.Replynum) + 1).ToString();
            }
        }

        internal static void ListViewItem_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            FrameworkElement s = (FrameworkElement)sender;
            var b = s.FindName("moreButton") as Button;
            b.Flyout.ShowAt(s);
        }

        internal static void relaRLis_ItemClick(object sender, ItemClickEventArgs e)
        {
            UIHelper.OpenLinkAsync(((Models.RelationRowsItem)e.ClickedItem).Url);
        }

        internal static void Flyout_Opened(object sender, object e)
        {
            var flyout = (Flyout)sender;
            if (flyout.Content == null)
            {
                flyout.Content = new ShowQRCodeControl
                {
                    QRCodeText = (string)flyout.Target.Tag
                };
            }
        }

        internal static void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var uc = sender as UserControl;
            var bp = uc.FindChildByName("btnsPanel") as StackPanel;
            var width = e is null ? uc.Width : e.NewSize.Width;
            bp.SetValue(Grid.RowProperty, width > 600 ? 1 : 10);
        }

        internal static void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            UserControl_SizeChanged(sender, null);
        }
    }
}