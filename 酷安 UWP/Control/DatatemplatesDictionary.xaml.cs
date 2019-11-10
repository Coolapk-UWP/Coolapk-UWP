using CoolapkUWP.Data;
using CoolapkUWP.Pages.FeedPages;
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

        private void MarkdownTextBlock_LinkClicked(object sender, Microsoft.Toolkit.Uwp.UI.Controls.LinkClickedEventArgs e)
        {
            if (e.Link.Replace("mailto:", string.Empty).IndexOf("http://image.coolapk.com") == 0) Tools.rootPage.ShowImage(e.Link.Replace("mailto:", string.Empty));
            else Tools.OpenLink(e.Link);
        }

        private void PicA_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender is GridView view)
            {
                if (view.SelectedIndex > -1 && view.Tag is string[] ss)
                    Tools.rootPage.ShowImage(ss[view.SelectedIndex].Remove(ss[view.SelectedIndex].Length - 6));
                view.SelectedIndex = -1;
            }
        }

        private void ListViewItem_Tapped_1(object sender, TappedRoutedEventArgs e)
            => Tools.rootPage.Navigate(typeof(FeedDetailPage), new object[] { (sender as FrameworkElement).Tag.ToString(), string.Empty });

    }
}
