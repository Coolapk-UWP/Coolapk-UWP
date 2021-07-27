using CoolapkUWP.Helpers;
using Newtonsoft.Json.Linq;
using ReverseMarkdown;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace CoolapkUWP.Pages.FeedPages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class HTMLTextPage : Page
    {
        private Uri uri;

        public HTMLTextPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            TitleBar.ShowProgressRing();
            uri = new Uri(e.Parameter.ToString());
            _ = Load_HTML(uri);
        }

        private async Task Load_HTML(Uri uri)
        {
            (bool isSucceed, string result) = await DataHelper.GetHtmlAsync(uri, "XMLHttpRequest");
            if (isSucceed)
            {
                JObject o;
                try
                {
                    o = JObject.Parse(result);
                    MarkdownText.Text = UIHelper.CSStoMarkDown(o.TryGetValue("html", out JToken token) ? token.ToString() : o.TryGetValue("description", out JToken desc) ? desc.ToString() : "#网络错误");
                    TitleBar.Title = o.TryGetValue("title", out JToken Title) ? Title.ToString() : null;
                }
                catch
                {
                    Frame.GoBack();
                    UIHelper.OpenLinkAsync(uri.ToString());
                }
            }
            else { Frame.GoBack(); }
            TitleBar.HideProgressRing();
        }

        private async Task Refresh(int p = -1)
        {
            TitleBar.ShowProgressRing();
            if (p == -2)
            {
                _ = (scrollViewer?.ChangeView(null, 0, null));
            }
            await Load_HTML(uri);
        }

        private void TitleBar_BackButtonClick(object sender, RoutedEventArgs e) => Frame.GoBack();

        private void TitleBar_RefreshButtonClicked(object sender, RoutedEventArgs e)
        {
            _ = Refresh(-2);
        }

        private async void RefreshContainer_RefreshRequested(RefreshContainer _, RefreshRequestedEventArgs args)
        {
            using (Windows.Foundation.Deferral RefreshCompletionDeferral = args.GetDeferral())
            {
                await Refresh(-2);
            }
        }
    }
}
