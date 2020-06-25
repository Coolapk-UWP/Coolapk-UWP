using CoolapkUWP.Helpers;
using CoolapkUWP.Pages.FeedPages;
using CoolapkUWP.ViewModels.FeedListDataProvider;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace CoolapkUWP.Pages.SettingPages
{
    public sealed partial class TestPage : Page
    {
        public TestPage()
        {
            this.InitializeComponent();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var f = FeedListDataProvider.GetProvider(FeedListType.UserPageList, await NetworkHelper.GetUserIDByName(uid.Text));
            if (f != null)
                UIHelper.Navigate(typeof(FeedListPage), f);
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
                Frame.GoBack();
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            UIHelper.ShowMessage(message.Text);
        }

        private async void Button_Click_5(object sender, RoutedEventArgs e)
        {
            //Tools.Navigate(typeof(FeedPages.FeedListPage), new object[] { FeedPages.FeedListType.DYHPageList, "1324" });
            string s = await NetworkHelper.GetJson(url.Text);
            System.Diagnostics.Debug.WriteLine(s);
            await new MessageDialog(s).ShowAsync();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            UIHelper.Navigate(typeof(Pages.BrowserPage), new object[] { false, "https://m.coolapk.com/mp/do?c=userDevice&m=myDevice" });
        }

        private void Hyperlink_Click(Windows.UI.Xaml.Documents.Hyperlink sender, Windows.UI.Xaml.Documents.HyperlinkClickEventArgs args)
        {
            Application.Current.Exit();
        }

        private async void Button_Click_2(object sender, RoutedEventArgs e)
        {
            pic.Source = await ImageCacheHelper.GetImageAsync(ImageType.OriginImage, picUri.Text, true);
        }
    }
}