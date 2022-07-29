using CoolapkUWP.Helpers;
using Microsoft.Toolkit.Uwp.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;

namespace CoolapkUWP.Controls.DataTemplates
{
    public partial class OtherDataTemplates : ResourceDictionary
    {
        public OtherDataTemplates()
        {
            InitializeComponent();
        }

        internal static void OnTapped(object sender, TappedRoutedEventArgs e)
        {
            if (((FrameworkElement)sender).FindAscendant("searchPivot") == null)
            {
                UIHelper.OpenLinkAsync((sender as FrameworkElement).Tag as string);
            }
        }

        internal static void ListViewItem_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter || e.Key == Windows.System.VirtualKey.Space)
            {
                OnTapped(sender, null);
            }
        }
    }
}