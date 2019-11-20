using CoolapkUWP.Data;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;

namespace CoolapkUWP.Control
{
    public partial class DatatemplatesDictionary
    {
        public DatatemplatesDictionary() => InitializeComponent();

        private void OnTapped(object sender, TappedRoutedEventArgs e)
            => Tools.OpenLink((sender as FrameworkElement).Tag as string);

        private void MarkdownTextBlock_LinkClicked(object sender, Microsoft.Toolkit.Uwp.UI.Controls.LinkClickedEventArgs e)
        {
            if (e.Link.Replace("mailto:", string.Empty).IndexOf("http://image.coolapk.com") == 0)
                Tools.ShowImage(e.Link.Replace("mailto:", string.Empty), ImageType.SmallImage);
            else Tools.OpenLink(e.Link);
        }

        private void PicA_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender is Windows.UI.Xaml.Controls.GridView view)
            {
                if (view.SelectedIndex > -1 && view.Tag is System.Collections.Generic.List<string> ss)
                    Tools.ShowImages(ss.ToArray(), view.SelectedIndex);
                view.SelectedIndex = -1;
            }
        }

        private void ListViewItem_Tapped_1(object sender, TappedRoutedEventArgs e)
            => Tools.Navigate(typeof(Pages.FeedPages.FeedDetailPage), (sender as FrameworkElement).Tag.ToString());

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

        private void MarkdownTextBlock_ImageResolving(object sender, Microsoft.Toolkit.Uwp.UI.Controls.ImageResolvingEventArgs e) => Tools.SetEmojiPadding(sender);
    }
}
