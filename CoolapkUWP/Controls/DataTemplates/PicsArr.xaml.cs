using CoolapkUWP.Helpers;
using Windows.UI.Xaml;

namespace CoolapkUWP.Controls.DataTemplates
{
    public partial class PicsArr : ResourceDictionary
    {
        public PicsArr() => InitializeComponent();

        private void Image_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
             => UIHelper.ShowImage((sender as FrameworkElement).Tag as string, ImageType.SmallImage);

    }
}