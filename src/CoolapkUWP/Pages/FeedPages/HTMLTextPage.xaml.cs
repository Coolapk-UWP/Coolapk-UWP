using CoolapkUWP.Helpers;
using Newtonsoft.Json.Linq;
using System;
using System.Text.RegularExpressions;
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

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            TitleBar.ShowProgressRing();
            uri = new Uri(e.Parameter.ToString());
            Load_HTML(uri);
        }

        private async void Load_HTML(Uri uri)
        {
            (bool isSucceed, string result) = await DataHelper.GetHtmlAsync(uri, "XMLHttpRequest");
            if (isSucceed)
            {
                JObject o;
                try
                {
                    o = JObject.Parse(result);
                    MarkdownText.Text = CSStoMarkDown(o.TryGetValue("html", out JToken token) ? token.ToString() : o.TryGetValue("description", out JToken desc) ? desc.ToString() : "#网络错误");
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

        private static string CSStoMarkDown(string text)
        {
            Regex h1 = new Regex("<h1 style.*?>", RegexOptions.IgnoreCase);
            Regex h2 = new Regex("<h2 style.*?>", RegexOptions.IgnoreCase);
            Regex h3 = new Regex("<h3 style.*?>", RegexOptions.IgnoreCase);
            Regex h4 = new Regex("<h4 style.*?>\n", RegexOptions.IgnoreCase);
            Regex div = new Regex("<div style.*?>", RegexOptions.IgnoreCase);
            Regex p = new Regex("<p style.*?>", RegexOptions.IgnoreCase);
            Regex ul = new Regex("<ul style.*?>", RegexOptions.IgnoreCase);
            Regex li = new Regex("<li style.*?>", RegexOptions.IgnoreCase);
            Regex span = new Regex("<span style.*?>", RegexOptions.IgnoreCase);

            text = text.Replace("</h1>", "");
            text = text.Replace("</h2>", "");
            text = text.Replace("</h3>", "");
            text = text.Replace("</h4>", "");
            text = text.Replace("</div>", "");
            text = text.Replace("</p>", "");
            text = text.Replace("</ul>", "");
            text = text.Replace("</li>", "");
            text = text.Replace("</span>", "**");

            text = h1.Replace(text, "#");
            text = h2.Replace(text, "##");
            text = h3.Replace(text, "###");
            text = h4.Replace(text, "####");
            text = text.Replace("<br />", "  \n");
            text = div.Replace(text, "");
            text = p.Replace(text, "");
            text = ul.Replace(text, "");
            text = li.Replace(text, " - ");
            text = span.Replace(text, "**");

            for (int i = 0; i < 20; i++) { text = text.Replace("(" + i.ToString() + ") ", " 1. "); }

            return text;
        }

        private void TitleBar_BackButtonClick(object sender, RoutedEventArgs e) => Frame.GoBack();

        private void TitleBar_RefreshButtonClicked(object sender, RoutedEventArgs e)
        {
            TitleBar.ShowProgressRing();
            Load_HTML(uri);
        }
    }
}
