using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace 酷安_UWP
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class IndexPage : Page
    {
        MainPage mainPage;
        int page = 0;
        List<int> pages = new List<int>();
        string pageUrl;
        ObservableCollection<Feed> Collection = new ObservableCollection<Feed>();
        int index;
        List<string> urls = new List<string>();
        ObservableCollection<ObservableCollection<Feed>> Feeds2 = new ObservableCollection<ObservableCollection<Feed>>();
        Style listviewStyle { get; set; }
        public IndexPage()
        {
            this.InitializeComponent();
            listView.ItemsSource = Collection;
            if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile") listviewStyle = Application.Current.Resources["ListViewStyle2Mobile"] as Style;
            else listviewStyle = Application.Current.Resources["ListViewStyle2Desktop"] as Style;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            object[] vs = e.Parameter as object[];
            mainPage = vs?[0] as MainPage;
            if ((bool)vs[2]) TitleBar.Visibility = Visibility.Collapsed;
            if (string.IsNullOrEmpty(vs[1] as string))
            {
                if (vs.Count() == 3)
                {
                    title.Text = "头条";
                    GetIndexPage(++page);
                }
            }
            else
            {
                pageUrl = vs[1] as string;
                BackButton.Visibility = Visibility.Visible;
                if (pageUrl.Contains("&title=")) title.Text = pageUrl.Substring(pageUrl.LastIndexOf("&title=") + 7);
                if (pageUrl.IndexOf("/page") == -1) pageUrl = "/page/dataList?url=" + pageUrl;
                else if (pageUrl.IndexOf("/page") == 0 && !pageUrl.Contains("/page/dataList")) pageUrl = pageUrl.Replace("/page", "/page/dataList");
                pageUrl = pageUrl.Replace("#", "%23");
                index = -1;
                GetUrlPage();
            }
        }

        async void GetUrlPage(int p = -1)
        {
            if (index == -1)
            {
                if (!await GetUrlPage(p == -1 ? ++page : p, pageUrl, Collection))
                    page--;
            }
            else if (p == -1)
            {
                if (!await GetUrlPage(page = p == -1 ? ++pages[index] : p, urls[index], Feeds2[index]))
                    pages[index]--;
            }
        }

        async Task<bool> GetUrlPage(int page, string url, ObservableCollection<Feed> FeedsCollection)
        {
            mainPage.ActiveProgressRing();
            if (page == 1)
            {
                string s = await Tools.GetCoolApkMessage($"{url}&page={page}");
                JObject jObject = (JObject)JsonConvert.DeserializeObject(s);
                JArray Root = (JArray)jObject["data"];
                int n = 0;
                if (FeedsCollection.Count > 0)
                {
                    var needDeleteItems = (from b in FeedsCollection
                                           from c in Root
                                           where b.GetValue("entityId") == c["entityId"].ToString()
                                           select b).ToArray();
                    foreach (var item in needDeleteItems)
                        Collection.Remove(item);
                    n = (from b in FeedsCollection
                         where b.GetValue("entityFixed") == "1"
                         select b).Count();
                }
                int k = 0;
                for (int i = 0; i < Root.Count; i++)
                {
                    if (index == -1 && (Root[i] as JObject).TryGetValue("entityTemplate", out JToken t) && t.ToString() == "configCard")
                    {
                        JObject j = JObject.Parse(Root[i]["extraData"].ToString());
                        title.Text = j["pageTitle"].ToString();
                        continue;
                    }
                    if ((Root[i] as JObject).TryGetValue("entityTemplate", out JToken tt) && tt.ToString() == "fabCard") continue;
                    FeedsCollection.Insert(n + k, new Feed((JObject)Root[i]));
                    k++;
                }
                mainPage.DeactiveProgressRing();
                return true;
            }
            else
            {
                string r = await Tools.GetCoolApkMessage($"{url}&page={page}");
                JArray Root = JObject.Parse(r)["data"] as JArray;
                if (!(Root is null) && Root.Count != 0)
                {
                    foreach (JObject i in Root) FeedsCollection.Add(new Feed(i));
                    mainPage.DeactiveProgressRing();
                    return true;
                }
                else
                {
                    mainPage.DeactiveProgressRing();
                    return false;
                }
            }
        }

        async void GetIndexPage(int page)
        {
            mainPage.ActiveProgressRing();
            if (page == 1)
            {
                JArray Root = await Tools.GetIndexList($"{page}");
                if (Collection.Count > 0)
                {
                    var needDeleteItems = (from b in Collection
                                           from c in Root
                                           where b.GetValue("entityId") == c["entityId"].ToString()
                                           select b).ToArray();
                    foreach (var item in needDeleteItems)
                        Collection.Remove(item);
                }
                int k = 0;
                for (int i = 0; i < Root.Count; i++)
                {
                    if ((Root[i] as JObject).TryGetValue("entityTemplate", out JToken tt) && tt.ToString() == "listCard") continue;
                    Collection.Insert(k, new Feed((JObject)Root[i]));
                    k++;
                }
            }
            else
            {
                JArray Root = await Tools.GetIndexList($"{page}");
                if (Root.Count != 0)
                    foreach (JObject i in Root)
                        Collection.Add(new Feed(i));
                else this.page--;
            }
            mainPage.DeactiveProgressRing();
        }

        private void FeedListViewItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if ((sender as FrameworkElement).Tag is Feed)
                mainPage.Frame.Navigate(typeof(FeedDetailPage), new object[] { ((sender as FrameworkElement).Tag as Feed).GetValue("id"), mainPage, string.Empty, null });
            else if ((sender as FrameworkElement).Tag is Feed[])
            {
                var f = (sender as FrameworkElement).Tag as Feed[];
                if (!string.IsNullOrEmpty(f[0].jObject.ToString()))
                    mainPage.Frame.Navigate(typeof(FeedDetailPage), new object[] { f[0].GetValue("id"), mainPage, string.Empty, null });
            }
        }

        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (!e.IsIntermediate)
            {
                if (Collection.Count != 0)
                    if (VScrollViewer.VerticalOffset == 0)
                    {
                        if (string.IsNullOrEmpty(pageUrl)) GetIndexPage(1);
                        else GetUrlPage(1);
                        VScrollViewer.ChangeView(null, 20, null);
                        refreshText.Visibility = Visibility.Collapsed;
                    }
                    else if (VScrollViewer.VerticalOffset == VScrollViewer.ScrollableHeight)
                        if (string.IsNullOrEmpty(pageUrl)) GetIndexPage(++page);
                        else GetUrlPage();
            }
            else refreshText.Visibility = Visibility.Visible;
        }

        public void RefreshPage()
        {
            if (string.IsNullOrEmpty(pageUrl)) GetIndexPage(1);
            else GetUrlPage(1);
            VScrollViewer.ChangeView(null, 0, null);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as FrameworkElement).Tag as string)
            {
                case "Refresh":
                    RefreshPage();
                    break;
                case "back":
                    Frame.GoBack();
                    break;
                default:
                    mainPage.Frame.Navigate(typeof(UserPage), new object[] { (sender as FrameworkElement).Tag as string, mainPage });
                    break;
            }
        }

        private void MarkdownTextBlock_LinkClicked(object sender, Microsoft.Toolkit.Uwp.UI.Controls.LinkClickedEventArgs e)
        {
            if (e.Link.Replace("mailto:", string.Empty).IndexOf("http://image.coolapk.com") == 0) ShowImageControl.ShowImage(e.Link.Replace("mailto:", string.Empty));
            else Tools.OpenLink(e.Link, mainPage);
        }

        private void ListViewItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(pageUrl)) GetIndexPage(1);
            else GetUrlPage(1);
            VScrollViewer.ChangeView(null, 0, null);
        }

        private void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            if (element.Tag is string s) Tools.OpenLink(s, mainPage);
            else if (element.Tag is Feed feed)
            {
                string str = string.IsNullOrEmpty((element.Tag as Feed).GetValue("url")) ? (element.Tag as Feed).GetValue("extra_url2") : (element.Tag as Feed).GetValue("url");
                if (str.IndexOf("/page") == 0)
                {
                    str = str.Replace("/page", "/page/dataList");
                    str += $"&title={feed.GetValue("title")}";
                    mainPage.Frame.Navigate(typeof(IndexPage), new object[] { mainPage, str, false });
                }
                else if (str.IndexOf('#') == 0) mainPage.Frame.Navigate(typeof(IndexPage), new object[] { mainPage, $"{str}&title={feed.GetValue("title")}", false });
                else Tools.OpenLink(str, mainPage);
            }
        }

        private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Pivot element = sender as Pivot;
            index = element.SelectedIndex;
            if (element.Items.Count == 1)
            {
                Feed[] f = element.Tag as Feed[];
                Style style = new Style(typeof(ListViewItem));
                style.Setters.Add(new Setter(HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch));
                element.Items.Clear();
                for (int j = 0; j < f.Count(); j++)
                {
                    var ff = new ObservableCollection<Feed>();
                    var i = new PivotItem
                    {
                        Tag = f[j],
                        Content = new ListView
                        {
                            Style = listviewStyle,
                            ItemContainerStyle = style,
                            ItemTemplateSelector = Resources["FTemplateSelector"] as DataTemplateSelector,
                            ItemsSource = ff
                        },
                        Header = f[j].GetValue("title")
                    };
                    element.Items.Add(i);
                    pages.Add(1);
                    Feeds2.Add(ff);
                    urls.Add("/page/dataList?url=" + f[j].GetValue("url").Replace("#", "%23") + $"&title={f[j].GetValue("title")}");
                    if (j == 0) load(i);
                }
                return;
            }
            load();
            void load(PivotItem i = null)
            {
                PivotItem item = i is null ? element.SelectedItem as PivotItem : i;
                Feed feed = item.Tag as Feed;
                ListView view = item.Content as ListView;
                ObservableCollection<Feed> feeds = view.ItemsSource as ObservableCollection<Feed>;
                string u = feed.GetValue("url");
                u = u.Replace("#", "%23");
                u = "/page/dataList?url=" + u + $"&title={feed.GetValue("title")}";
                GetUrlPage(1, u, feeds);
            }
        }

        private void ListViewItem_Tapped_1(object sender, TappedRoutedEventArgs e)
        {
            Feed feed = (sender as ListViewItem).DataContext as Feed;
            if (Feeds2.Count > 0)
            {
                ObservableCollection<Feed> feeds = Feeds2[0];
                var needDeleteItems = (from b in feeds
                                       where b.GetValue("entityType") == "feed"
                                       select b).ToArray();
                foreach (var item in needDeleteItems)
                    feeds.Remove(item);
                urls[0] = $"/page/dataList?url={feed.GetValue("url")}&title={feed.GetValue("title")}";
                urls[0] = urls[0].Replace("#", "%23");
                pages[0] = 0;

            }
            else
            {
                ObservableCollection<Feed> feeds = Collection;
                var needDeleteItems = (from b in feeds
                                       where b.GetValue("entityType") == "topic"
                                       select b).ToArray();
                foreach (var item in needDeleteItems)
                    feeds.Remove(item);
                pageUrl = $"/page/dataList?url={feed.GetValue("url")}&title={feed.GetValue("title")}";
                pageUrl = pageUrl.Replace("#", "%23");
                page = 0;
            }
            GetUrlPage();
        }

        public void ChangeTabView(string u)
        {
            pageUrl = u;
            page = 0;
            Collection.Clear();
            GetUrlPage();
        }

        private void PicA_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GridView view = sender as GridView;
            if (view.SelectedIndex > -1 && view.Tag is string[] ss)
                ShowImageControl.ShowImage(ss[view.SelectedIndex].Remove(ss[view.SelectedIndex].Length - 6));
            view.SelectedIndex = -1;
        }
    }
}