using CoolapkUWP.Helpers;
using Windows.UI.Xaml;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace CoolapkUWP.Controls.DataTemplates
{
    public sealed partial class RelationRowsTemplates : ResourceDictionary
    {
        public RelationRowsTemplates() => InitializeComponent();

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            UIHelper.OpenLinkAsync((sender as FrameworkElement).Tag.ToString());
        }
    }
}
