using CoolapkUWP.Helpers;
using CoolapkUWP.Models;
using Newtonsoft.Json.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace CoolapkUWP.Controls.DataTemplates
{
    public partial class FeedReply : ResourceDictionary
    {
        public FeedReply() => InitializeComponent();

        private void OnTapped(object sender, TappedRoutedEventArgs e)
            => UIHelper.OpenLinkAsync((sender as FrameworkElement).Tag as string);

        private void ReplyRowsItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender == e.OriginalSource || sender.GetType() == typeof(TextBlockEx))
            {
                if (sender is FrameworkElement frameworkElement)
                {
                    UIHelper.NavigateInSplitPane(typeof(Pages.FeedPages.FeedRepliesPage), frameworkElement.Tag);
                }
            }
        }

        private void Image_Tapped(object sender, TappedRoutedEventArgs e)
            => UIHelper.ShowImage((sender as FrameworkElement).Tag as string, ImageType.SmallImage);

        private async void FeedButton_Click(object sender, RoutedEventArgs e)
        {
            void ChangeLikeStatus(ILike f, FrameworkElement button, bool isLike)
            {
                f.Liked = isLike;
                if (button.FindName("like1") is SymbolIcon symbolIcon1)
                    symbolIcon1.Visibility = isLike ? Visibility.Visible : Visibility.Collapsed;
                if (button.FindName("like2") is SymbolIcon symbolIcon2)
                    symbolIcon2.Visibility = isLike ? Visibility.Collapsed : Visibility.Visible;
            }

            FrameworkElement element = sender as FrameworkElement;
            switch (element.Name)
            {
                case "likeButton":
                    var f = element.Tag as ILike;
                    bool isReply = f is FeedReplyModel;
                    bool b = false;
                    JObject o;
                    if (f.Liked)
                        o = (JObject)await DataHelper.GetDataAsync(DataUriType.OperateUnlike, isReply ? "Reply" : string.Empty, f.Id);
                    else
                    {
                        o = (JObject)await DataHelper.GetDataAsync(DataUriType.OperateLike, isReply ? "Reply" : string.Empty, f.Id);
                        b = true;
                    }

                    if (isReply)
                        f.Likenum = o.ToString().Replace("\"", string.Empty);
                    else if (o != null)
                        f.Likenum = o.Value<int>("count").ToString();
                    ChangeLikeStatus(f, element, b);
                    break;
            }
        }
    }
}