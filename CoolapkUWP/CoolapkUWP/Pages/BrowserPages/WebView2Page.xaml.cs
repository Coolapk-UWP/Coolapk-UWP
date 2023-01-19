using CoolapkUWP.Helpers;
using CoolapkUWP.ViewModels.BrowserPages;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;
using Windows.Web.Http.Filters;

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
                WebView.Source = Provider.Uri;
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            WebView.Close();
        }

        private void WebView_NavigationStarting(WebView2 sender, CoreWebView2NavigationStartingEventArgs args)
        {
            UIHelper.ShowProgressBar();
        }

        private void WebView_NavigationCompleted(WebView2 sender, CoreWebView2NavigationCompletedEventArgs args)
        {
            if (Provider.IsLoginPage && sender.Source.AbsoluteUri == "https://www.coolapk.com/")
            {
                CheckLogin();
            }
            else if (sender.Source.AbsoluteUri == UriHelper.LoginUri)
            {
                Provider.IsLoginPage = true;
            }
            Provider.Title = sender.CoreWebView2.DocumentTitle;
            UIHelper.HideProgressBar();
        }

        private async void CheckLogin()
        {
            ResourceLoader loader = ResourceLoader.GetForCurrentView("BrowserPage");
            if (await SetLoginCookie() && await SettingsHelper.Login())
            {
                if (Frame.CanGoBack) { Frame.GoBack(); }
                UIHelper.ShowMessage(loader.GetString("LoginSuccessfully"));
            }
            else
            {
                WebView.Source = new Uri(UriHelper.LoginUri);
                UIHelper.ShowMessage(loader.GetString("CannotGetToken"));
            }
        }

        public async Task<bool> SetLoginCookie()
        {
            string Uid = string.Empty, Token = string.Empty, UserName = string.Empty;
            foreach (CoreWebView2Cookie item in await WebView.CoreWebView2.CookieManager.GetCookiesAsync("https://coolapk.com"))
            {
                switch (item.Name)
                {
                    case "uid":
                        Uid = item.Value;
                        break;
                    case "username":
                        UserName = item.Value;
                        break;
                    case "token":
                        Token = item.Value;
                        break;
                    default:
                        break;
                }
            }
            if (!string.IsNullOrEmpty(Uid) && !string.IsNullOrEmpty(UserName) && !string.IsNullOrEmpty(Token))
            {
                using (HttpBaseProtocolFilter filter = new HttpBaseProtocolFilter())
                {
                    HttpCookieManager cookieManager = filter.CookieManager;
                    HttpCookie uid = new HttpCookie("uid", ".coolapk.com", "/");
                    HttpCookie username = new HttpCookie("username", ".coolapk.com", "/");
                    HttpCookie token = new HttpCookie("token", ".coolapk.com", "/");
                    uid.Value = Uid;
                    username.Value = UserName;
                    token.Value = Token;
                    cookieManager.SetCookie(uid);
                    cookieManager.SetCookie(username);
                    cookieManager.SetCookie(token);
                }
                return true;
            }
            return false;
        }

        private void GotoSystemBrowserButton_Click(object sender, RoutedEventArgs e) => _ = Launcher.LaunchUriAsync(WebView.Source);

        private void TryLoginButton_Click(object sender, RoutedEventArgs e) => CheckLogin();

        private void RefreshButton_Click(object sender, RoutedEventArgs e) => WebView.Reload();
    }
}
