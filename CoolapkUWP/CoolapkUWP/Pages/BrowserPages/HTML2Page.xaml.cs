using CoolapkUWP.Helpers;
using CoolapkUWP.ViewModels.BrowserPages;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using System;
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

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is HTMLViewModel ViewModel)
            {
                Provider = ViewModel;
                DataContext = Provider;
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            WebView.Close();
        }

        private async void WebView_Loaded(object sender, RoutedEventArgs e)
        {
            UIHelper.ShowProgressBar();
            WebView2 WebView2 = sender as WebView2;
            await WebView2.EnsureCoreWebView2Async();
            WebView2.CoreWebView2?.SetVirtualHostNameToFolderMapping(
                "coolapkuwp.app", "Assets/WebView",
                CoreWebView2HostResourceAccessKind.Allow);
            UIHelper.HideProgressBar();
            await Refresh(true);
        }

        public async Task Refresh(bool reset = false) => await Provider.Refresh(reset);

        private void RefreshButton_Click(object sender, RoutedEventArgs e) => _ = Refresh(true);
    }
}
