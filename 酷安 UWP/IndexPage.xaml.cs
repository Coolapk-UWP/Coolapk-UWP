using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
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
        string url;
        ObservableCollection<Feed> FeedsCollection = new ObservableCollection<Feed>();
        int index;
        List<string> urls = new List<string>();
        ObservableCollection<ObservableCollection<Feed>> Feeds2 = new ObservableCollection<ObservableCollection<Feed>>();
        public IndexPage()
        {
            this.InitializeComponent();
            listView.ItemsSource = FeedsCollection;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            object[] vs = e.Parameter as object[];
            mainPage = vs?[0] as MainPage;
            if (string.IsNullOrEmpty(vs[1] as string))
            {
                title.Text = "头条";
                GetIndexPage(++page);
            }
            else
            {
                BackButton.Visibility = Visibility.Visible;
                url = vs[1] as string;
                if (url.Contains("&title="))
                    title.Text = url.Substring(url.LastIndexOf("&title=") + 7);
                if (url.IndexOf("/page") != 0)
                    url = "/page/dataList?url=" + url;
                url = url.Replace("#", "%23");
                index = -1;
                GetUrlPage(++page);
            }
        }

        void GetUrlPage(int page)
        {
            if (index == -1) GetUrlPage(page, url, FeedsCollection);
            else GetUrlPage(page, urls[index], Feeds2[index]);
        }

        async void GetUrlPage(int page, string url, ObservableCollection<Feed> FeedsCollection)
        {
            mainPage.ActiveProgressRing();
            if (page == 1)
            {
                string s = await CoolApkSDK.GetCoolApkMessage($"{url}&page={page}");
                JObject jObject = (JObject)JsonConvert.DeserializeObject(s);
                JArray Root = (JArray)jObject["data"];
                if (FeedsCollection.Count != 0)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        if (FeedsCollection[i].GetValue("entityFixed") == "1")
                            FeedsCollection.RemoveAt(0);
                    }
                    for (int i = 0; i < Root.Count; i++)
                    {
                        if (i >= FeedsCollection.Count) break;
                        if (Root.Contains(FeedsCollection[i].jObject))
                        {
                            FeedsCollection.RemoveAt(i);
                            i--;
                        }
                    }
                }
                for (int i = 0; i < Root.Count; i++)
                {
                    if (index == -1 && (Root[i] as JObject).TryGetValue("entityTemplate", out JToken t) && t.ToString() == "configCard")
                    {
                        JObject j = JObject.Parse(Root[i]["extraData"].ToString());
                        title.Text = j["pageTitle"].ToString();
                        continue;
                    }
                    FeedsCollection.Insert(i, new Feed((JObject)Root[i]));
                }
            }
            else
            {
                JArray Root = JObject.Parse(await CoolApkSDK.GetCoolApkMessage($"{url}?page={page}"))["data"] as JArray;
                if (Root.Count != 0)
                    foreach (JObject i in Root)
                        FeedsCollection.Add(new Feed(i));
                else page--;
            }
            mainPage.DeactiveProgressRing();
        }

        async void GetIndexPage(int page)
        {
            mainPage.ActiveProgressRing();
            if (page == 1)
            {
                timer.Stop();
                timer = new DispatcherTimer();
                JArray Root = await CoolApkSDK.GetIndexList($"{page}");
                if (FeedsCollection.Count != 0)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        if (FeedsCollection[i].GetValue("entityFixed") == "1")
                            FeedsCollection.RemoveAt(0);
                    }
                    for (int i = 0; i < Root.Count; i++)
                    {
                        if (i >= FeedsCollection.Count) break;
                        if (Root.Contains(FeedsCollection[i].jObject))
                        {
                            FeedsCollection.RemoveAt(i);
                            i--;
                        }
                    }
                }
                for (int i = 0; i < Root.Count; i++)
                    FeedsCollection.Insert(i, new Feed((JObject)Root[i]));
                timer.Interval = new TimeSpan(0, 0, 7);
                timer.Tick += (s, e) =>
                {
                    if (flip.SelectedIndex < flip.Items.Count - 1)
                        flip.SelectedIndex++;
                    else
                        flip.SelectedIndex = 0;
                };
                timer.Start();
            }
            else
            {
                JArray Root = await CoolApkSDK.GetIndexList($"{page}");
                if (Root.Count != 0)
                    foreach (JObject i in Root)
                        FeedsCollection.Add(new Feed(i));
                else page--;
            }
            mainPage.DeactiveProgressRing();
        }

        DispatcherTimer timer = new DispatcherTimer();
        FlipView flip;

        private void FlipView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (flip is null) flip = sender as FlipView;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            timer.Stop();
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
                if (FeedsCollection.Count != 0)
                    if (VScrollViewer.VerticalOffset == 0)
                    {
                        if (string.IsNullOrEmpty(url)) GetIndexPage(1);
                        else GetUrlPage(1);
                        VScrollViewer.ChangeView(null, 20, null);
                        refreshText.Visibility = Visibility.Collapsed;
                    }
                    else if (VScrollViewer.VerticalOffset == VScrollViewer.ScrollableHeight)
                        if (string.IsNullOrEmpty(url)) GetIndexPage(++page);
                        else GetUrlPage(++page);
            }
            else refreshText.Visibility = Visibility.Visible;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button i = sender as Button;
            if (i.Tag as string == "Refresh")
            {
                GetIndexPage(1);
                VScrollViewer.ChangeView(null, 0, null);
            }
            else mainPage.Frame.Navigate(typeof(UserPage), new object[] { i.Tag as string, mainPage });
        }

        private async void MarkdownTextBlock_LinkClicked(object sender, Microsoft.Toolkit.Uwp.UI.Controls.LinkClickedEventArgs e)
        {
            if (e.Link.IndexOf("/u/") == 0)
                mainPage.Frame.Navigate(typeof(UserPage), new object[] { await CoolApkSDK.GetUserIDByName(e.Link.Replace("/u/", string.Empty)), mainPage });
            if (e.Link.Replace("mailto:", string.Empty).IndexOf("http://image.coolapk.com") == 0)
                await Launcher.LaunchUriAsync(new Uri(e.Link.Replace("mailto:", string.Empty)));
            if (e.Link.IndexOf("http") == 0)
                await Launcher.LaunchUriAsync(new Uri(e.Link));
        }

        private void ListViewItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            GetIndexPage(1);
            VScrollViewer.ChangeView(null, 0, null);
        }

        private async void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            if(element.Tag is string)
            {
                mainPage.Frame.Navigate(typeof(UserPage), new object[] { element.Tag, mainPage });
                return;
            }
            string s = (element.Tag as Feed).GetValue("url");
            if (s.IndexOf("/feed/") == 0)
            {
                mainPage.Frame.Navigate(typeof(FeedDetailPage), new object[] { s.Replace("/feed/", string.Empty), mainPage, string.Empty, null });
                return;
            }
            s = (element.Tag as Feed).GetValue("extra_url2");
            if (s.IndexOf("/u/") == 0)
                mainPage.Frame.Navigate(typeof(UserPage), new object[] { await CoolApkSDK.GetUserIDByName(s.Replace("/u/", string.Empty)), mainPage });
            if (s.IndexOf("http") == 0)
                await Launcher.LaunchUriAsync(new Uri(s));
        }

        private void PivotItem_Loading(FrameworkElement sender, object args)
        {
            PivotItem element = sender as PivotItem;
            Feed feed = element.Tag as Feed;
            ListView view = element.Content as ListView;
            ObservableCollection<Feed> feeds = new ObservableCollection<Feed>();
            string u = feed.GetValue("url");
            if (u.Contains("#"))
                u = u.Substring(1);
            u = "/page/dataList?url=" + u + $"&title={feed.GetValue("title")}";
            u = Uri.EscapeUriString(u);
            GetUrlPage(1, u, feeds);
            view.ItemsSource = feeds;
            Feeds2.Add(feeds);
            urls.Add(u);
        }

        private async void StackPanel_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            Feed feed = element.Tag as Feed;
            string u = feed.GetValue("url");
            if (u.IndexOf("http") == 0)
            {
                await Launcher.LaunchUriAsync(new Uri(u));
                return;
            }
            u = u.Replace("/page", "/page/dataList");
            u += $"&title={feed.GetValue("title")}";
            mainPage.Frame.Navigate(typeof(IndexPage), new object[] { mainPage, u });
        }

        private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Pivot element = sender as Pivot;
            index = element.SelectedIndex;
            if (element.Items.Count == 1)
            {
                Feed[] feeds = element.Tag as Feed[];
                Style style = new Style(typeof(ListViewItem));
                style.Setters.Add(new Setter(HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch));
                element.Items.Clear();
                foreach (var item in feeds)
                {
                    var i = new PivotItem
                    {
                        Tag = item,
                        Content = new ListView
                        {
                            ItemContainerStyle = style,
                            ItemTemplateSelector = Resources["FTemplateSelector"] as DataTemplateSelector
                        },
                        Header = item.GetValue("title")
                    };
                    i.Loading += PivotItem_Loading;
                    element.Items.Add(i);
                }
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e) => Frame.GoBack();

        private void ListViewItem_Tapped_1(object sender, TappedRoutedEventArgs e)
        {

        }
    }
}