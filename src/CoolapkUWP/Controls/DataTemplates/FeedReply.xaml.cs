using CoolapkUWP.Helpers;
using CoolapkUWP.Models;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace CoolapkUWP.Controls.DataTemplates
{
    public partial class FeedReply : ResourceDictionary
    {
        public FeedReply() => InitializeComponent();

        internal static void ReplyRowsItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (e != null && !UIHelper.IsOriginSource(sender, e.OriginalSource)) { return; }

            if (sender is FrameworkElement frameworkElement)
            {
                if ((frameworkElement.Tag as ICanCopy)?.IsCopyEnabled ?? false) { return; }
                UIHelper.NavigateInSplitPane(typeof(Pages.FeedPages.FeedRepliesPage), new ViewModels.FeedRepliesPage.ViewModel((FeedReplyModel)frameworkElement.Tag));
            }
        }

        internal static void Image_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
            UIHelper.ShowImage((sender as FrameworkElement).Tag as ImageModel);
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
                    var item = Microsoft.Toolkit.Uwp.UI.Extensions.VisualTree.FindAscendant<ListViewItem>(element);
                    var ctrl = item.FindName("makeFeed") as MakeFeedControl;
                    ctrl.Visibility = ctrl.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
                    DisabledCopy();
                    break;

                case "likeButton":
                    await DataHelper.MakeLikeAsync(
                        element.Tag as ICanChangeLikModel,
                        element.Dispatcher,
                        (SymbolIcon)element.FindName("like1"),
                        (SymbolIcon)element.FindName("like2"));
                    DisabledCopy();
                    break;

                case "reportButton":
                    DisabledCopy();
                    UIHelper.Navigate(typeof(Pages.BrowserPage), new object[] { false, $"https://m.coolapk.com/mp/do?c=feed&m=report&type=feed_reply&id={element.Tag}" });
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
                    ReplyRowsItem_Tapped(sender, null);
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

        internal static void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var uc = sender as UserControl;
            var bp = uc.FindChildByName("btnsPanel") as StackPanel;
            var width = e is null ? uc.Width : e.NewSize.Width;
            bp.SetValue(Grid.RowProperty, width > 524 ? 0 : 4);
        }

        internal static void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            UserControl_SizeChanged(sender, null);
        }
    }
}