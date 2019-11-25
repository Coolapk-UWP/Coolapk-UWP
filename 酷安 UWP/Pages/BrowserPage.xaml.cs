using CoolapkUWP.Data;
using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace CoolapkUWP.Pages
{
    public sealed partial class BrowserPage : Page
    {
        bool isLoginPage;
        string url;
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
                webView.Source = new Uri("https://account.coolapk.com/auth/loginByCoolapk");
            else if (!string.IsNullOrEmpty(vs[1] as string))
            {
                url = vs[1] as string;
                webView.Source = new Uri(url);
            }
        }

        private void WebView_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            if (IsLoginPage && args.Uri.AbsoluteUri == "https://www.coolapk.com/") CheckLogin();
            else if (args.Uri.AbsoluteUri == "https://account.coolapk.com/auth/loginByCoolapk")
                IsLoginPage = true;
            titleBar.Title = sender.DocumentTitle;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e) => Frame.GoBack();

        async void CheckLogin()
        {
            if (await Settings.CheckLoginInfo())
            {
                Frame.GoBack();
                Tools.ShowMessage("登录成功");
            }
            else
            {
                webView.Navigate(new Uri("https://account.coolapk.com/auth/loginByCoolapk"));
                Tools.ShowMessage("没有获取到token，请尝试重新登录");
            }
        }

        private async void GotoSystemBrowserButton_Click(object sender, RoutedEventArgs e)
            => await Windows.System.Launcher.LaunchUriAsync(new Uri(url));
    }
}