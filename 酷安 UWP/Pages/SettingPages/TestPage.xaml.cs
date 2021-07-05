using CoolapkUWP.Data;
using System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace CoolapkUWP.Pages.SettingPages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class TestPage : Page
    {
        public TestPage()
        {
            InitializeComponent();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            UIHelper.Navigate(typeof(FeedPages.FeedListPage), new object[] { FeedPages.FeedListType.UserPageList, await UIHelper.GetUserIDByName(uid.Text) });
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
            { Frame.GoBack(); }
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            UIHelper.ShowMessage(message.Text);
        }

        private async void Button_Click_5(object sender, RoutedEventArgs e)
        {
            //UIHelper.Navigate(typeof(FeedPages.FeedListPage), new object[] { FeedPages.FeedListType.DYHPageList, "1324" });
            string s = await UIHelper.GetJson(url.Text);
            System.Diagnostics.Debug.WriteLine(s);
            _ = await new MessageDialog(s).ShowAsync();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            UIHelper.Navigate(typeof(BrowserPage), new object[] { false, "https://m.coolapk.com/mp/do?c=userDevice&m=myDevice" });

        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {

        }

        private void Hyperlink_Click(Windows.UI.Xaml.Documents.Hyperlink sender, Windows.UI.Xaml.Documents.HyperlinkClickEventArgs args)
        {
            Windows.UI.Xaml.Application.Current.Exit();
        }
    }
}