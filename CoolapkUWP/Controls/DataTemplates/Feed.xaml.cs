using CoolapkUWP.Helpers;
using CoolapkUWP.Models;
using Newtonsoft.Json.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace CoolapkUWP.Controls.DataTemplates
{
    public partial class Feed : ResourceDictionary
    {
        public Feed() => InitializeComponent();

        private void OnTapped(object sender, TappedRoutedEventArgs e)
        {
            if (e != null && !UIHelper.IsOriginSource(sender, e.OriginalSource)) { return; }

            UIHelper.OpenLinkAsync((sender as FrameworkElement).Tag as string);
        }

        private async void FeedButton_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            switch (element.Name)
            {
                case "makeReplyButton":
                    var item = Microsoft.Toolkit.Uwp.UI.Extensions.VisualTree.FindAscendant<ListViewItem>(element);
                    var ctrl = item.FindName("makeFeed") as MakeFeedControl;
                    ctrl.Visibility = ctrl.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
                    break;

                case "likeButton":
                    await DataHelper.MakeLikeAsync( element.Tag as ICanChangeLike,
                                                    element.Dispatcher,
                                                    (SymbolIcon)element.FindName("like1"),
                                                    (SymbolIcon)element.FindName("like2"));
                    break;
                default:
                    UIHelper.OpenLinkAsync((sender as FrameworkElement).Tag as string);
                    break;
            }
        }

        private void ListViewItem_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (UIHelper.IsOriginSource(sender, e.OriginalSource))
            {
                if (e.Key == Windows.System.VirtualKey.Enter || e.Key == Windows.System.VirtualKey.Space)
                {
                    OnTapped(sender, null);
                }
            }
        }

        private void makeFeed_MakedFeedSuccessful(object sender, System.EventArgs e)
        {
            if (((FrameworkElement)sender).Tag is ICanChangeReplyNum m)
            {
                m.Replynum = (int.Parse(m.Replynum) + 1).ToString();
            }
        }
    }
}