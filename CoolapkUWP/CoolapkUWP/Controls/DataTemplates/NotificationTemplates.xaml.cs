using CoolapkUWP.Helpers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Shapes;
using PersonPicture = Microsoft.UI.Xaml.Controls.PersonPicture;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace CoolapkUWP.Controls.DataTemplates
{
    public sealed partial class NotificationTemplates : ResourceDictionary
    {
        public NotificationTemplates() => InitializeComponent();

        private void OnTapped(object sender, TappedRoutedEventArgs e)
        {
            if (e == null || sender is PersonPicture || (sender is Grid && !(e.OriginalSource is Ellipse)))
            { _ = UIHelper.OpenLinkAsync((sender as FrameworkElement).Tag as string); }
        }

        private void OnKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter || e.Key == Windows.System.VirtualKey.Space)
            {
                OnTapped(sender, null);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e) => OnTapped(sender, null);
    }
}
