using CoolapkUWP.Data;
using System;
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
            UIHelper.ShowProgressBar();
            if (IsLoginPage)
            {
                webView.Navigate(new Uri("https://account.coolapk.com/auth/loginByCoolapk"));
            }
            else if (!string.IsNullOrEmpty(vs[1] as string))
            {
                url = vs[1] as string;
                webView.Navigate(new Uri(url));
            }
        }

        private void LoadUri(Uri uri)
        {
            using (Windows.Web.Http.HttpRequestMessage httpRequestMessage = new Windows.Web.Http.HttpRequestMessage(Windows.Web.Http.HttpMethod.Get, uri))
            {
                httpRequestMessage.Headers.UserAgent.ParseAdd(UIHelper.mClient.DefaultRequestHeaders.UserAgent.ToString());
                webView.NavigateWithHttpRequestMessage(httpRequestMessage);
                webView.NavigationStarting += WebView_NavigationStarting;
            }
        }

        private void WebView_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            if (IsLoginPage && args.Uri.AbsoluteUri == "https://www.coolapk.com/") { CheckLogin(); }
            else if (args.Uri.AbsoluteUri == "https://account.coolapk.com/auth/loginByCoolapk")
            { IsLoginPage = true; }
            titleBar.Title = sender.DocumentTitle;
            UIHelper.HideProgressBar();
        }

        private void WebView_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            webView.NavigationStarting -= WebView_NavigationStarting;
            args.Cancel = true;
            LoadUri(args.Uri);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e) => Frame.GoBack();

        private async void CheckLogin()
        {
            if (await SettingsHelper.CheckLoginInfo())
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

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            UIHelper.ShowProgressBar();
            webView.Refresh();
            UIHelper.HideProgressBar();
        }

        private async void GotoSystemBrowserButton_Click(object sender, RoutedEventArgs e)
            => await Windows.System.Launcher.LaunchUriAsync(new Uri(url));
    }
}