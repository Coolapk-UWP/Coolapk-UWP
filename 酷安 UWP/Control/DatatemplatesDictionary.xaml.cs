using CoolapkUWP.Data;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using System;
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
            => Tools.OpenLink((sender as FrameworkElement).Tag as string);

        private void PicA_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender is GridView view)
            {
                if (view.SelectedIndex > -1 && view.Tag is System.Collections.Generic.List<string> ss)
                    Tools.ShowImages(ss.ToArray(), view.SelectedIndex);
                view.SelectedIndex = -1;
            }
        }

        private void Image_Tapped(object sender, TappedRoutedEventArgs e)
            => Tools.ShowImage((sender as FrameworkElement).Tag as string, ImageType.SmallImage);

        private void replyRowsItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender is FrameworkElement frameworkElement)
            {
                var popup = new Windows.UI.Xaml.Controls.Primitives.Popup();
                popup.Child = new ReplyDialogPresenter(frameworkElement.Tag, popup);
                Tools.ShowPopup(popup);
            }
        }

        private async void FeedButton_Click(object sender, RoutedEventArgs e)
        {
            void ChangeLikeStatus(ViewModels.ILike f, FrameworkElement button, bool isLike)
            {
                f.liked = isLike;
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
                    if (f.liked)
                    {
                        string s = await Tools.GetJson($"/feed/unlike{(isReply ? "Reply" : string.Empty)}?id={f.id}&detail=0");
                        if (isReply)
                        {
                            f.likenum = Tools.GetObjectStrigInJson(s);
                            ChangeLikeStatus(f, element, false);
                        }
                        else
                        {
                            JsonObject o = Tools.GetJSonObject(s);
                            if (o != null)
                            {
                                f.likenum = o["count"].GetNumber().ToString();
                                ChangeLikeStatus(f, element, false);
                            }
                        }
                    }
                    else
                    {
                        string s = await Tools.GetJson($"/feed/like{(isReply ? "Reply" : string.Empty)}?id={f.id}&detail=0");
                        if (isReply)
                        {
                            f.likenum = Tools.GetObjectStrigInJson(s);
                            ChangeLikeStatus(f, element, true);
                        }
                        else
                        {
                            JsonObject o = Tools.GetJSonObject(s);
                            if (o != null)
                            {
                                f.likenum = o["count"].GetNumber().ToString();
                                ChangeLikeStatus(f, element, true);
                            }
                        }
                    }
                    break;
            }
        }

        private void repRL_ItemClick(object sender, ItemClickEventArgs e)
            => Tools.Navigate(typeof(Pages.FeedPages.FeedDetailPage), (e.ClickedItem as FrameworkElement).Tag.ToString());
    }
}
