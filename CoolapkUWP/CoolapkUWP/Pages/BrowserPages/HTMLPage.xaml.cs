using CoolapkUWP.Controls;
using CoolapkUWP.Helpers;
using CoolapkUWP.ViewModels.BrowserPages;
using Microsoft.Toolkit.Uwp.UI;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace CoolapkUWP.Pages.BrowserPages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class HTMLPage : Page
    {
        private HTMLViewModel Provider;

        public HTMLPage() => InitializeComponent();

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            UIHelper.MainPage.FindDescendant<WebViewContentControl>().IsWebView = true;
            if (e.Parameter is HTMLViewModel ViewModel)
            {
                Provider = ViewModel;
                DataContext = Provider;
                await Refresh(true);
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            UIHelper.MainPage.FindDescendant<WebViewContentControl>().IsWebView = false;
        }

        private void WebView_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            if (args.Uri != null)
            {
                args.Cancel = true;
                UIHelper.OpenLinkAsync(args.Uri.AbsoluteUri);
            }
        }

        public async Task Refresh(bool reset = false) => await Provider.Refresh(reset);

        private void RefreshButton_Click(object sender, RoutedEventArgs e) => _ = Refresh(true);
    }
}
