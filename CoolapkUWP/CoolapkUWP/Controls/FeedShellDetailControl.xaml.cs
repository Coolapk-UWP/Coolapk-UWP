using CoolapkUWP.Helpers;
using CoolapkUWP.Models;
using CoolapkUWP.Models.Images;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace CoolapkUWP.Controls
{
    public sealed partial class FeedShellDetailControl : UserControl
    {
        public FeedShellDetailControl() => InitializeComponent();

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            switch (element.Name)
            {
                case "FollowButton":
                    _ = (element.Tag as ICanFollow).ChangeFollow();
                    break;
                default:
                    UIHelper.OpenLinkAsync((sender as FrameworkElement).Tag as string);
                    break;
            }
        }

        private void CopyMenuItem_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            DataPackage dp = new DataPackage();
            dp.SetText(element.Tag.ToString());
            Clipboard.SetContent(dp);
        }

        private void Flyout_Opened(object sender, object _)
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

        private void OnTapped(object sender, TappedRoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            if ((element.DataContext as ICanCopy)?.IsCopyEnabled ?? false) { return; }

            if (e != null) { e.Handled = true; }

            if (element.Tag is ImageModel image)
            {
                UIHelper.ShowImage(image);
            }
            else
            {
                UIHelper.OpenLinkAsync(element.Tag.ToString());
            }
        }

        private void UrlButton_Click(object sender, RoutedEventArgs e) => UIHelper.OpenLinkAsync((sender as FrameworkElement).Tag.ToString());

        private void GridView_SelectionChanged(object sender, SelectionChangedEventArgs e) => (sender as GridView).SelectedIndex = -1;
    }
}
