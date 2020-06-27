using CoolapkUWP.Helpers;
using System;
using Windows.ApplicationModel.Resources;
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
                    if(openInSystemBrowserButton != null)
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
            IsLoginPage = (bool)vs[0];
            if (IsLoginPage)
                webView.Source = new Uri(loginUri);
            else if (!string.IsNullOrEmpty(vs[1] as string))
            {
                uri = vs[1] as string;
                webView.Source = new Uri(uri);
            }
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
        }

        private void BackButton_Click(object sender, RoutedEventArgs e) => Frame.GoBack();

        private async void CheckLogin()
        {
            var loader = ResourceLoader.GetForCurrentView("BrowserPage");
            if (await SettingsHelper.CheckLoginInfo())
            {
                if (Frame.CanGoBack)
                    Frame.GoBack();
                UIHelper.NavigateInSplitPane(typeof(MyPage));
                UIHelper.ShowMessage(loader.GetString("LoginSuccessfully"));
            }
            else
            {
                webView.Navigate(new Uri(loginUri));
                UIHelper.ShowMessage(loader.GetString("CannotGetToken"));
            }
        }

        private async void GotoSystemBrowserButton_Click(object sender, RoutedEventArgs e)
            => await Windows.System.Launcher.LaunchUriAsync(new Uri(uri));
    }
}