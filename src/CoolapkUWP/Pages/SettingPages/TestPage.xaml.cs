using CoolapkUWP.Helpers;
using CoolapkUWP.Pages.FeedPages;
using CoolapkUWP.ViewModels.FeedListPage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace CoolapkUWP.Pages.SettingPages
{
    public sealed partial class TestPage : Page
    {
        public TestPage()
        {
            this.InitializeComponent();
            //var loader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView("EmojiId");
            //System.Diagnostics.Debug.WriteLine(
            //loader.GetString("?")
            //    );
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var f = FeedListPageViewModelBase.GetProvider(FeedListType.UserPageList, await Core.Helpers.NetworkHelper.GetUserIDByNameAsync(uid.Text));
            if (f != null)
                UIHelper.NavigateInSplitPane(typeof(FeedListPage), f);
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

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            UIHelper.Navigate(typeof(BrowserPage), new object[] { false, "http://www.all-tool.cn/Tools/ua/" });
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            UIHelper.Navigate(typeof(Pages.BrowserPage), new object[] { false, "https://m.coolapk.com/mp/do?c=userDevice&m=myDevice" });
        }

        private void TitleBar_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}