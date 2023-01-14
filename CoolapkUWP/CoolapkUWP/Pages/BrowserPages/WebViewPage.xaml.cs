using CoolapkUWP.Helpers;
using CoolapkUWP.ViewModels.BrowserPages;
using System;
using Windows.ApplicationModel.Resources;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace CoolapkUWP.Pages.BrowserPages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class BrowserPage : Page
    {
        private BrowserViewModel Provider;

        public BrowserPage() => InitializeComponent();

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is BrowserViewModel ViewModel)
            {
                Provider = ViewModel;
                DataContext = Provider;
                WebView.Navigate(Provider.Uri);
            }
        }

        private void LoadUri(Uri uri)
        {
            using (HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, uri))
            {
                httpRequestMessage.Headers.UserAgent.ParseAdd(NetworkHelper.Client.DefaultRequestHeaders.UserAgent.ToString());
                WebView.NavigateWithHttpRequestMessage(httpRequestMessage);
                WebView.NavigationStarting += WebView_NavigationStarting;
            }
        }

        private void WebView_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            UIHelper.ShowProgressBar();
            if (args.Uri.Host.Contains("coolapk"))
            {
                WebView.NavigationStarting -= WebView_NavigationStarting;
                args.Cancel = true;
                LoadUri(args.Uri);
            }
        }

        private void WebView_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            if (Provider.IsLoginPage && args.Uri.AbsoluteUri == "https://www.coolapk.com/")
            {
                CheckLogin();
            }
            else if (args.Uri.AbsoluteUri == UriHelper.LoginUri)
            {
                Provider.IsLoginPage = true;
            }
            Provider.Title = sender.DocumentTitle;
            UIHelper.HideProgressBar();
        }

        private async void CheckLogin()
        {
            ResourceLoader loader = ResourceLoader.GetForCurrentView("BrowserPage");
            if (await SettingsHelper.Login())
            {
                if (Frame.CanGoBack) { Frame.GoBack(); }
                UIHelper.ShowMessage(loader.GetString("LoginSuccessfully"));
            }
            else
            {
                WebView.Navigate(new Uri(UriHelper.LoginUri));
                UIHelper.ShowMessage(loader.GetString("CannotGetToken"));
            }
        }

        private void GotoSystemBrowserButton_Click(object sender, RoutedEventArgs e) => _ = Launcher.LaunchUriAsync(WebView.Source);

        private void TryLoginButton_Click(object sender, RoutedEventArgs e) => CheckLogin();

        private void RefreshButton_Click(object sender, RoutedEventArgs e) => WebView.Refresh();
    }
}
