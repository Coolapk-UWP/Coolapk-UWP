using CoolapkUWP.Helpers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace CoolapkUWP.Controls.DataTemplates
{
    public partial class NotificationTemplates : ResourceDictionary
    {
        public NotificationTemplates()
        {
            InitializeComponent();
        }

        public static void Ellipse_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (e.OriginalSource is Windows.UI.Xaml.Shapes.Ellipse ||
                (sender is Grid && !(e.OriginalSource is Windows.UI.Xaml.Shapes.Ellipse)))
            { UIHelper.OpenLinkAsync((sender as FrameworkElement).Tag as string); }
        }
    }
}