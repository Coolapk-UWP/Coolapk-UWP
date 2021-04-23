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
                    FindName(nameof(tryLoginButton));
                    if (openInSystemBrowserButton != null)
                    {
                        UnloadObject(openInSystemBrowserButton);
                    }
                }
                else
                {
                    FindName(nameof(openInSystemBrowserButton));
                    if (tryLoginButton != null)
                    {
                        UnloadObject(tryLoginButton);
                    }
                }
            }
        }

        public BrowserPage() => this.InitializeComponent();

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            object[] vs = e.Parameter as object[];
            titleBar.ShowProgressRing();
            IsLoginPage = (bool)vs[0];
            if (IsLoginPage)
            {
                LoadUri(loginUri);
            }
            else if (!string.IsNullOrEmpty(vs[1] as string))
            {
                uri = vs[1] as string;
                LoadUri(uri);
            }
        }

        private void LoadUri(String uri)

        {
            EasClientDeviceInformation deviceInfo = new EasClientDeviceInformation();
            var httpRequestMessage = new Windows.Web.Http.HttpRequestMessage(Windows.Web.Http.HttpMethod.Get, new Uri(uri));
            httpRequestMessage.Headers.UserAgent.ParseAdd("(#Build; " + deviceInfo.SystemManufacturer + "; " + deviceInfo.SystemProductName + "; ; " + "10.0) +CoolMarket / 11.1.2 - 2104021 - universal");
            webView.NavigateWithHttpRequestMessage(httpRequestMessage);
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            _ = SettingsHelper.CheckLoginInfo();
            base.OnNavigatingFrom(e);
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

        private async void CheckLogin()
        {
            var loader = ResourceLoader.GetForCurrentView("BrowserPage");
            if (await SettingsHelper.CheckLoginInfo())
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
            await Windows.System.Launcher.LaunchUriAsync(new Uri(uri));
        }

        private async void Refresh_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
            Frame.Navigate(typeof(BrowserPage), new object[] { false, uri });
        }

        private void tryLoginButton_Click(object sender, RoutedEventArgs e)
        {
            CheckLogin();
        }
    }
}