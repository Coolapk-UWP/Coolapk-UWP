using CoolapkUWP.Helpers;
using Windows.UI.Xaml;

namespace CoolapkUWP.Controls.DataTemplates
{
    public partial class Pic : ResourceDictionary
    {
        public Pic() => InitializeComponent();

        private static void Image_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            UIHelper.ShowImage((sender as FrameworkElement).Tag as Models.ImageModel);
        }

        private static void ListViewItem_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter || e.Key == Windows.System.VirtualKey.Space)
            {
                Image_Tapped(sender, null);
            }
        }
    }
}