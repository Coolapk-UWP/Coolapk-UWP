using CoolapkUWP.Data;
using Windows.Data.Json;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace CoolapkUWP.Control
{
    public partial class DatatemplatesDictionary
    {
        public DatatemplatesDictionary() => InitializeComponent();

        private void OnTapped(object sender, TappedRoutedEventArgs e)
            => UIHelper.OpenLink((sender as FrameworkElement).Tag as string);

        private void PicA_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender is GridView view)
            {
                if (view.SelectedIndex > -1 && view.Tag is System.Collections.Generic.List<string> ss)
                { UIHelper.ShowImages(ss.ToArray(), view.SelectedIndex); }
                view.SelectedIndex = -1;
            }
        }

        private void Image_Tapped(object sender, TappedRoutedEventArgs e)
            => UIHelper.ShowImage((sender as FrameworkElement).Tag as string, ImageType.SmallImage);

        private void replyRowsItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender is FrameworkElement frameworkElement)
            {
                Windows.UI.Xaml.Controls.Primitives.Popup popup = new Windows.UI.Xaml.Controls.Primitives.Popup();
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
                { symbolIcon1.Visibility = isLike ? Visibility.Visible : Visibility.Collapsed; }
                if (button.FindName("like2") is SymbolIcon symbolIcon2)
                { symbolIcon2.Visibility = isLike ? Visibility.Collapsed : Visibility.Visible; }
            }

            FrameworkElement element = sender as FrameworkElement;
            switch (element.Name)
            {
                case "shareButton":
                    break;

                case "likeButton":
                    ViewModels.ILike f = element.Tag as ViewModels.ILike;
                    bool isReply = f is ViewModels.FeedReplyViewModel;
                    if (f.Liked)
                    {
                        string s = await UIHelper.GetJson($"/feed/unlike{(isReply ? "Reply" : string.Empty)}?id={f.id}&detail=0");
                        if (isReply)
                        {
                            f.Likenum = UIHelper.GetObjectStrigInJson(s);
                            ChangeLikeStatus(f, element, false);
                        }
                        else
                        {
                            JsonObject o = UIHelper.GetJSonObject(s);
                            if (o != null)
                            {
                                f.Likenum = o["count"].GetNumber().ToString();
                                ChangeLikeStatus(f, element, false);
                            }
                        }
                    }
                    else
                    {
                        string s = await UIHelper.GetJson($"/feed/like{(isReply ? "Reply" : string.Empty)}?id={f.id}&detail=0");
                        if (isReply)
                        {
                            f.Likenum = UIHelper.GetObjectStrigInJson(s);
                            ChangeLikeStatus(f, element, true);
                        }
                        else
                        {
                            JsonObject o = UIHelper.GetJSonObject(s);
                            if (o != null)
                            {
                                f.Likenum = o["count"].GetNumber().ToString();
                                ChangeLikeStatus(f, element, true);
                            }
                        }
                    }
                    break;
            }
        }

        private void Flyout_Opened(object sender, object e)
        {
            Flyout flyout = sender as Flyout;
            FrameworkElement element = flyout.Target as FrameworkElement;
            Frame replyFlyoutFrame = flyout.Content as Frame;
            if (replyFlyoutFrame.Content is null)
            { _ = replyFlyoutFrame.Navigate(typeof(Pages.FeedPages.MakeFeedPage), new object[] { Pages.FeedPages.MakeFeedMode.Reply, element.Tag, flyout }); }
        }

        private void Flyout_Opened_1(object sender, object e)
        {
            Flyout flyout = sender as Flyout;
            FrameworkElement element = flyout.Target as FrameworkElement;
            Frame replyFlyoutFrame = flyout.Content as Frame;
            if (replyFlyoutFrame.Content is null)
            { replyFlyoutFrame.Navigate(typeof(Pages.FeedPages.MakeFeedPage), new object[] { Pages.FeedPages.MakeFeedMode.ReplyReply, ((double)element.Tag).ToString(), flyout }); }
        }

        private void QRCode_Opened(object sender, object _)
        {
            Flyout flyout = (Flyout)sender;
            if (flyout.Content == null)
            {
                flyout.Content = new ShowQRCodeControl
                {
                    QRCodeText = (string)flyout.Target.Tag
                };
            }
        }

        private void repRL_ItemClick(object sender, ItemClickEventArgs e)
            => UIHelper.Navigate(typeof(Pages.FeedPages.FeedDetailPage), (e.ClickedItem as FrameworkElement).Tag.ToString());
    }
}
