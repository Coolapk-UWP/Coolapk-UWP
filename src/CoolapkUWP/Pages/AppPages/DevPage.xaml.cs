using CoolapkUWP.Helpers;
using System;
using System.Net.Http;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace CoolapkUWP.Pages.AppPages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class DevPage : Page
    {
        public DevPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Uri uri = new Uri("https://api.coolapk.com/v6/feed/changeHistoryList?id=26935758");
            var (isSucceed, result) = await DataHelper.GetHtmlAsync(uri,"XMLHttpRequest");
            if (!isSucceed) { }
            else main.Text = result;
        }

        #region Task：任务

        private void GetDevMyList(string str)
        {
            main.Text = str;
        }


        #endregion

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
                Frame.GoBack();
        }

        private void TitleBar_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
