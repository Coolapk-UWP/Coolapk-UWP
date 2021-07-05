using CoolapkUWP.Data;
using System;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace CoolapkUWP.Pages
{
    public sealed partial class BrowserPage : Page
    {
        private bool isLoginPage;
        private string url;
        public bool IsLoginPage
        {
            get => isLoginPage;
            set
            {
                isLoginPage = value;
                if (value)
                {
                    TryLoginButton.Visibility = Visibility.Visible;
                    GotoSystemBrowserButton.Visibility = Visibility.Collapsed;
                }
                else
                {
                    TryLoginButton.Visibility = Visibility.Collapsed;
                    GotoSystemBrowserButton.Visibility = Visibility.Visible;
                }
            }
        }

        public BrowserPage() => this.InitializeComponent();

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            object[] vs = e.Parameter as object[];
            IsLoginPage = (bool)vs[0];
            if (IsLoginPage)
            { webView.Source = new Uri("https://account.coolapk.com/auth/loginByCoolapk"); }
            else if (!string.IsNullOrEmpty(vs[1] as string))
            {
                url = vs[1] as string;
                LoadUri(url);
            }
        }

        private void LoadUri(string uri)

        {
            EasClientDeviceInformation deviceInfo = new EasClientDeviceInformation();
            using (Windows.Web.Http.HttpRequestMessage httpRequestMessage = new Windows.Web.Http.HttpRequestMessage(Windows.Web.Http.HttpMethod.Get, new Uri(uri)))
            {
                httpRequestMessage.Headers.UserAgent.ParseAdd("Dalvik/2.1.0 (Windows NT 10.0; Win64; x64; WebView/3.0) (#Build; " + deviceInfo.SystemManufacturer + "; " + deviceInfo.SystemProductName + "; CoolapkUWP; " + "10.0)");
                httpRequestMessage.Headers.UserAgent.ParseAdd(" +CoolMarket/9.2.2-1905301-universal");
                webView.NavigateWithHttpRequestMessage(httpRequestMessage);
            }
        }

        private void WebView_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            if (IsLoginPage && args.Uri.AbsoluteUri == "https://www.coolapk.com/") { CheckLogin(); }
            else if (args.Uri.AbsoluteUri == "https://account.coolapk.com/auth/loginByCoolapk")
            { IsLoginPage = true; }
            titleBar.Title = sender.DocumentTitle;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e) => Frame.GoBack();

        private async void CheckLogin()
        {
            if (await SettingHelper.CheckLoginInfo())
            {
                Frame.GoBack();
                UIHelper.ShowMessage("登录成功");
            }
            else
            {
                webView.Navigate(new Uri("https://account.coolapk.com/auth/loginByCoolapk"));
                UIHelper.ShowMessage("没有获取到token，请尝试重新登录");
            }
        }

        private async void GotoSystemBrowserButton_Click(object sender, RoutedEventArgs e)
            => await Windows.System.Launcher.LaunchUriAsync(new Uri(url));
    }
}