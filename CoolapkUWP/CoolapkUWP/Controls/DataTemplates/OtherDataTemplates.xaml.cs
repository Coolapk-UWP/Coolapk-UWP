using CoolapkUWP.Helpers;
using Microsoft.Toolkit.Uwp.UI;
using Windows.UI.Xaml;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace CoolapkUWP.Controls.DataTemplates
{
    public sealed partial class OtherDataTemplates : ResourceDictionary
    {
        public OtherDataTemplates() => InitializeComponent();

        private void OnTapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            if (((FrameworkElement)sender).FindAscendant("searchPivot") == null)
            {
                _ = UIHelper.OpenLinkAsync((sender as FrameworkElement).Tag as string);
            }
        }

        private void OnKeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter || e.Key == Windows.System.VirtualKey.Space)
            {
                OnTapped(sender, null);
            }
        }
    }
}
