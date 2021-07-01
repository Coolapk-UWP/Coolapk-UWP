using CoolapkUWP.Helpers;
using System;
using Windows.ApplicationModel.Resources;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace CoolapkUWP.Pages
{
    public sealed partial class BrowserPage : Page
    {
        private bool isLoginPage;
        private string uri;

        private const string loginUri = "https://account.coolapk.com/auth/loginByCoolapk";

        public bool IsLoginPage
        {
            get => isLoginPage;
            set
            {
                isLoginPage = value;
                if (value)
                {
                    _ = FindName(nameof(tryLoginButton));
                    if (openInSystemBrowserButton != null)
                    {
                        UnloadObject(openInSystemBrowserButton);
                    }
                }
                else
                {
                    _ = FindName(nameof(openInSystemBrowserButton));
                    if (tryLoginButton != null)
                    {
                        UnloadObject(tryLoginButton);
                    }
                }
            }
        }

        public BrowserPage() => InitializeComponent();

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            object[] vs = e.Parameter as object[];
            titleBar.ShowProgressRing();
            IsLoginPage = (bool)vs[0];
            if (IsLoginPage)
            {
                webView.Navigate(new Uri(loginUri));
            }
            else if (!string.IsNullOrEmpty(vs[1] as string))
            {
                uri = vs[1] as string;
                webView.Navigate(new Uri(uri));
            }
        }

        private void LoadUri(Uri uri)

        {
            EasClientDeviceInformation deviceInfo = new EasClientDeviceInformation();
            using (Windows.Web.Http.HttpRequestMessage httpRequestMessage = new Windows.Web.Http.HttpRequestMessage(Windows.Web.Http.HttpMethod.Get, uri))
            {
                httpRequestMessage.Headers.UserAgent.ParseAdd("Dalvik/2.1.0 (Windows NT 10.0; Win64; x64; WebView/3.0) (#Build; " + deviceInfo.SystemManufacturer + "; " + deviceInfo.SystemProductName + "; CoolapkUWP; " + "10.0)" + " +CoolMarket/11.2-2105201-universal");
                webView.NavigateWithHttpRequestMessage(httpRequestMessage);
                webView.NavigationStarting += WebView_NavigationStarting;
            }
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            _ = SettingsHelper.CheckLoginInfo();
            base.OnNavigatingFrom(e);
        }

        private void WebView_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            webView.NavigationStarting -= WebView_NavigationStarting;
            args.Cancel = true;
            LoadUri(args.Uri);
        }

        private void WebView_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            if (IsLoginPage && args.Uri.AbsoluteUri == "https://www.coolapk.com/")
            {
                CheckLogin();
            }
            else if (args.Uri.AbsoluteUri == loginUri)
            {
                IsLoginPage = true;
            }

            titleBar.Title = sender.DocumentTitle;
            titleBar.HideProgressRing();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e) => Frame.GoBack();

        private void CheckLogin()
        {
            ResourceLoader loader = ResourceLoader.GetForCurrentView("BrowserPage");
            if (SettingsHelper.CheckLoginInfo())
            {
                if (Frame.CanGoBack) { Frame.GoBack(); }
                UIHelper.NavigateInSplitPane(typeof(MyPage), new ViewModels.MyPage.ViewMode());
                UIHelper.ShowMessage(loader.GetString("LoginSuccessfully"));
            }
            else
            {
                webView.Navigate(new Uri(loginUri));
                UIHelper.ShowMessage(loader.GetString("CannotGetToken"));
            }
        }

        private async void GotoSystemBrowserButton_Click(object sender, RoutedEventArgs e)
        {
            _ = await Windows.System.Launcher.LaunchUriAsync(new Uri(uri));
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            titleBar.ShowProgressRing();
            webView.Refresh();
            titleBar.HideProgressRing();
        }

        private void tryLoginButton_Click(object sender, RoutedEventArgs e)
        {
            CheckLogin();
        }
    }
}