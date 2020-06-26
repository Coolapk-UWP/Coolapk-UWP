using CoolapkUWP.Helpers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace CoolapkUWP.Controls.DataTemplates
{
    public partial class DataTemplates : ResourceDictionary
    {
        public DataTemplates()
        {
            this.InitializeComponent();
        }

        private void OnTapped(object sender, TappedRoutedEventArgs e)
        {
            UIHelper.OpenLinkAsync((sender as FrameworkElement).Tag as string);
        }

        private void ListViewItem_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if(e.Key == Windows.System.VirtualKey.Enter || e.Key == Windows.System.VirtualKey.Space)
            {
                OnTapped(sender, null);
            }
        }
    }
}