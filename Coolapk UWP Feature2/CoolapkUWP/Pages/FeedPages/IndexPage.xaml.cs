using CoolapkUWP.Control.ViewModels;
using CoolapkUWP.Data;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace CoolapkUWP.Pages.FeedPages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class IndexPage : Page
    {
        private int page = 0;
        private readonly List<int> pages = new List<int>();
        private string pageUrl;
        private readonly ObservableCollection<Entity> Collection = new ObservableCollection<Entity>();
        private int index;
        private readonly List<string> urls = new List<string>();
        private readonly ObservableCollection<ObservableCollection<Entity>> Feeds2 = new ObservableCollection<ObservableCollection<Entity>>();

        public IndexPage() => this.InitializeComponent();

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            object[] vs = e.Parameter as object[];
            if (!(bool)vs[1])
            {
                TitleBar.Visibility = Visibility.Visible;
                listView.Padding = SettingsHelper.stackPanelMargin;
            }
            pageUrl = vs[0] as string;
            TitleBar.BackButtonVisibility = Visibility.Visible;
            pageUrl = GetUri(pageUrl);
            index = -1;
            GetUrlPage();
            _ = Task.Run(async () =>
              {
                  await Task.Delay(1000);
                  await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                  {
                      try { (VisualTree.FindDescendantByName(listView, "ScrollViewer") as ScrollViewer).ViewChanged += ScrollViewer_ViewChanged; } catch { }
                  });
              });
        }

        private string GetUri(string uri)
        {
            if (uri.Contains("&title="))
            {
                const string Value = "&title=";
                TitleBar.Title = uri.Substring(uri.LastIndexOf(Value, StringComparison.Ordinal) + Value.Length);
            }

            if (uri.IndexOf("/page", StringComparison.Ordinal) == -1 && (uri.StartsWith("#", StringComparison.Ordinal) || (!uri.Contains("/main/") && !uri.Contains("/user/") && !uri.Contains("/apk/") && !uri.Contains("/appForum/") && !uri.Contains("/picture/") && !uri.Contains("/topic/") && !uri.Contains("/discovery/"))))
            {
                uri = "/page/dataList?url=" + uri;
            }
            else if (uri.IndexOf("/page", StringComparison.Ordinal) == 0 && !uri.Contains("/page/dataList"))
            {
                uri = uri.Replace("/page", "/page/dataList");
            }

            return uri.Replace("#", "%23");
        }

        private async void GetUrlPage(int p = -1)
        {
            try
            {
                if (index == -1)
                {
                    if (!await GetUrlPage(p == -1 ? ++page : p, pageUrl, Collection)) { page--; }
                }
                else if (p == -1)
                {
                    if (!await GetUrlPage(page = p == -1 ? ++pages[index] : p, urls[index], Feeds2[index])) { pages[index]--; }
                }
            }
            catch
            {
                UIHelper.ErrorProgressBar();
            }
        }

        private async Task<bool> GetUrlPage(int page, string url, ObservableCollection<Entity> FeedsCollection)
        {
            UIHelper.ShowProgressBar();
            string s = await UIHelper.GetJson($"{url}{(url.Contains("?") ? "&" : "?")}page={page}");
            if (string.IsNullOrEmpty(s))
            {
                UIHelper.ErrorProgressBar();
                return true;
            }
            else
            {
                JsonArray Root = UIHelper.GetDataArray(s);
                if (Root != null && Root.Count > 0)
                {
                    if (page == 1)
                    {
                        int n = 0;
                        if (FeedsCollection.Count > 0)
                        {
                            Entity[] needDeleteItems = (from b in FeedsCollection
                                                        from c in Root
                                                        where b.entityId == c.GetObject()["entityId"].ToString().Replace("\"", string.Empty)
                                                        select b).ToArray();
                            foreach (Entity item in needDeleteItems) { Collection.Remove(item); }
                            n = (from b in FeedsCollection
                                 where b.entityFixed
                                 select b).Count();
                        }
                        int k = 0;
                        for (int i = 0; i < Root.Count; i++)
                        {
                            JsonObject jo = Root[i].GetObject();
                            if (index == -1 && jo.TryGetValue("entityTemplate", out IJsonValue t) && t?.GetString() == "configCard")
                            {
                                JsonObject j = JsonObject.Parse(jo["extraData"].GetString());
                                TitleBar.Title = j["pageTitle"].GetString();
                                continue;
                            }
                            if (jo.TryGetValue("entityTemplate", out IJsonValue tt) && tt.GetString() == "fabCard") { continue; }
                            FeedsCollection.Insert(n + k, GetEntity(jo));
                            k++;
                        }
                        UIHelper.HideProgressBar();
                        return true;
                    }
                    else
                    {
                        if (Root.Count != 0)
                        {
                            foreach (IJsonValue i in Root) { FeedsCollection.Add(GetEntity(i.GetObject())); }
                            UIHelper.HideProgressBar();
                            return true;
                        }
                        else
                        {
                            UIHelper.HideProgressBar();
                            return false;
                        }
                    }
                }
                else
                {
                    return false;
                }
            }
        }

        private Entity GetEntity(JsonObject token)
        {
            switch (token["entityType"].GetString())
            {
                case "dyh": return new DyhViewModel(token);
                case "feed": return new FeedViewModel(token, pageUrl == "/main/indexV8" ? FeedDisplayMode.isFirstPageFeed : FeedDisplayMode.normal);
                case "user": return new UserViewModel(token);
                case "topic": return new TopicViewModel(token);
                case "product": return new ProductViewModel(token);
                case "card":
                default: return new IndexPageViewModel(token);
            }
        }

        private void FeedListViewItem_Tapped(object sender, TappedRoutedEventArgs e) => UIHelper.OpenLink((sender as FrameworkElement).Tag as string);

        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            ScrollViewer VScrollViewer = sender as ScrollViewer;
            if (!e.IsIntermediate)
            {
                if (Collection.Count != 0)
                {
                    if (VScrollViewer.VerticalOffset >= VScrollViewer.ScrollableHeight)
                    {
                        //if (string.IsNullOrEmpty(pageUrl)) GetIndexPage(++page);
                        GetUrlPage();
                    }
                }
            }
        }

        internal static void FlipView_Loaded(object sender, RoutedEventArgs _)
        {
            FlipView view = sender as FlipView;
            view.MaxHeight = view.ActualWidth / 3;
            DispatcherTimer timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(20)
            };
            timer.Tick += (o, a) =>
            {
                if (view.SelectedIndex + 1 >= view.Items.Count())
                {
                    while (view.SelectedIndex > 0)
                    {
                        view.SelectedIndex -= 1;
                    }
                }
                else
                {
                    view.SelectedIndex += 1;
                }
            };

            timer.Start();
        }

        internal static void FlipView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            FlipView view = sender as FlipView;
            view.MaxHeight = e.NewSize.Width / 3;
        }

        public void RefreshPage() => GetUrlPage(1);

        private void Button_Click(object sender, RoutedEventArgs e)
            => UIHelper.Navigate(typeof(FeedListPage), new object[] { FeedListType.UserPageList, (sender as FrameworkElement).Tag as string });
        private void TitleBar_BackButtonClick(object sender, RoutedEventArgs e) => Frame.GoBack();

        private void ListViewItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            GetUrlPage(1);
        }

        private void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            if (element.Tag is string s) { UIHelper.OpenLink(s); }
            else if (element.Tag is IndexPageViewModel m)
            {
                if (string.IsNullOrEmpty(m.url)) { return; }
                string str = m.url;
                if (str.IndexOf("/page") == 0)
                {
                    str = str.Replace("/page", "/page/dataList");
                    str += $"&title={m.title}";
                    UIHelper.Navigate(typeof(IndexPage), new object[] { str, false, null });
                }
                else if (str.IndexOf('#') == 0) { UIHelper.Navigate(typeof(IndexPage), new object[] { $"{str}&title={m.title}", false, null }); }
                else { UIHelper.OpenLink(str); }
            }
        }

        private void ListViewItem_Tapped_1(object sender, TappedRoutedEventArgs e)
        {
            IndexPageViewModel model = (sender as ListViewItem).DataContext as IndexPageViewModel;
            if (Feeds2.Count > 0)
            {
                ObservableCollection<Entity> feeds = Feeds2[0];
                Entity[] needDeleteItems = (from b in feeds
                                            where b.entityType == "feed"
                                            select b).ToArray();
                foreach (Entity item in needDeleteItems) { feeds.Remove(item); }
                urls[0] = $"/page/dataList?url={model.url}&title={model.title}";
                urls[0] = urls[0].Replace("#", "%23");
                pages[0] = 0;

            }
            else
            {
                ObservableCollection<Entity> feeds = Collection;
                Entity[] needDeleteItems = (from b in feeds
                                            where b.entityType == "topic"
                                            select b).ToArray();
                foreach (Entity item in needDeleteItems) { feeds.Remove(item); }
                pageUrl = $"/page/dataList?url={model.url}&title={model.title}";
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

        private void Pivot_Loaded(object sender, RoutedEventArgs e)
        {
            Pivot element = sender as Pivot;
            index = element.SelectedIndex;
            if (element.Items.Count == 0)
            {
                Entity[] f = element.Tag as Entity[];
                Style style = new Style(typeof(ListViewItem));
                style.Setters.Add(new Setter(TemplateProperty, Windows.UI.Xaml.Application.Current.Resources["ListViewItemTemplate1"] as ControlTemplate));
                for (int j = 0; j < f.Length; j++)
                {
                    IndexPageViewModel model = f[j] as IndexPageViewModel;
                    ObservableCollection<Entity> ff = new ObservableCollection<Entity>();
                    ListView l = new ListView
                    {
                        Style = Windows.UI.Xaml.Application.Current.Resources["ListViewStyle"] as Style,
                        ItemContainerStyle = style,
                        ItemTemplateSelector = Resources["FTemplateSelector"] as DataTemplateSelector,
                        ItemsSource = ff,
                        ItemsPanel = Windows.UI.Xaml.Markup.XamlReader.Load("<ItemsPanelTemplate xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" xmlns:c=\"using:CoolapkUWP.Control\"><c:GridPanel DesiredColumnWidth=\"384\" CubeInSameHeight=\"False\"/></ItemsPanelTemplate>") as ItemsPanelTemplate,
                        SelectionMode = ListViewSelectionMode.None
                    };
                    l.SetValue(ScrollViewer.VerticalScrollModeProperty, ScrollMode.Disabled);
                    PivotItem i = new PivotItem
                    {
                        Tag = f[j],
                        Content = l,
                        Header = model.title
                    };
                    element.Items.Add(i);
                    pages.Add(1);
                    Feeds2.Add(ff);
                    urls.Add("/page/dataList?url=" + model.url.Replace("#", "%23") + $"&title={model.title}");
                    if (j == 0) { load(element, i); }
                }
                return;
            }
        }

        private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e) => load(sender as Pivot);

        private void load(Pivot element, PivotItem i = null)
        {
            PivotItem item = i is null ? element.SelectedItem as PivotItem : i;
            IndexPageViewModel model = item.Tag as IndexPageViewModel;
            ListView view = item.Content as ListView;
            ObservableCollection<Entity> feeds = view.ItemsSource as ObservableCollection<Entity>;
            string u = model.url;
            u = u.Replace("#", "%23");
            u = "/page/dataList?url=" + u + $"&title={model.title}";
            _ = GetUrlPage(1, u, feeds);
        }
    }
}