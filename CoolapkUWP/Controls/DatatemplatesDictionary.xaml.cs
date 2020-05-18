using CoolapkUWP.Helpers;
using Newtonsoft.Json.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace CoolapkUWP.Controls
{
    public partial class DataTemplatesDictionary
    {
        public DataTemplatesDictionary() => InitializeComponent();

        private void OnTapped(object sender, TappedRoutedEventArgs e)
            => UIHelper.OpenLink((sender as FrameworkElement).Tag as string);

        private void PicA_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender is GridView view)
            {
                if (view.SelectedIndex > -1 && view.Tag is System.Collections.Generic.List<string> ss)
                    UIHelper.ShowImages(ss.ToArray(), view.SelectedIndex);
                view.SelectedIndex = -1;
            }
        }

        private void Image_Tapped(object sender, TappedRoutedEventArgs e)
            => UIHelper.ShowImage((sender as FrameworkElement).Tag as string, ImageType.SmallImage);

        private void ReplyRowsItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender is FrameworkElement frameworkElement)
            {
                var popup = new Windows.UI.Xaml.Controls.Primitives.Popup();
                popup.Child = new ReplyDialogPresenter(frameworkElement.Tag, popup);
                UIHelper.ShowPopup(popup);
            }
        }

        private async void FeedButton_Click(object sender, RoutedEventArgs e)
        {
            void ChangeLikeStatus(ViewModels.ILike f, FrameworkElement button, bool isLike)
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
                    var f = element.Tag as ViewModels.ILike;
                    bool isReply = f is ViewModels.FeedReplyViewModel;
                    bool b = false;
                    JObject o;
                    if (f.Liked)
                        o = (JObject)await DataHelper.GetData(DataType.OperateUnlike, isReply ? "Reply" : string.Empty, f.Id);
                    else
                    {
                        o = (JObject)await DataHelper.GetData(DataType.OperateLike, isReply ? "Reply" : string.Empty, f.Id);
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
