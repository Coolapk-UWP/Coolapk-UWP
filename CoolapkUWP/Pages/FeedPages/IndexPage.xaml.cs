using CoolapkUWP.Helpers;
using CoolapkUWP.Models;
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

namespace CoolapkUWP.Pages.FeedPages
{
    public sealed partial class IndexPage : Page
    {
        private int page = 0;
        private readonly List<int> pages = new List<int>();
        private string pageUrl;
        private readonly ObservableCollection<Entity> Collection = new ObservableCollection<Entity>();
        private int index;
        private readonly List<string> urls = new List<string>();
        private readonly ObservableCollection<ObservableCollection<Entity>> Feeds2 = new ObservableCollection<ObservableCollection<Entity>>();

        public bool CanLoadMore { get => Collection.Count != 0; }

        public IndexPage() => this.InitializeComponent();

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            object[] vs = e.Parameter as object[];

            pageUrl = vs[0] as string;
            if (pageUrl.Contains("&title=", StringComparison.Ordinal))
            {
                TitleBar.Title = pageUrl.Substring(pageUrl.LastIndexOf("&title=", StringComparison.Ordinal) + 7);
            }
            if (pageUrl.IndexOf("/page", StringComparison.Ordinal) == -1 && pageUrl != "/main/indexV8")
            {
                pageUrl = "/page/dataList?url=" + pageUrl;
            }
            else if (pageUrl.IndexOf("/page", StringComparison.Ordinal) == 0 && !pageUrl.Contains("/page/dataList", StringComparison.Ordinal))
            {
                pageUrl = pageUrl.Replace("/page", "/page/dataList", StringComparison.Ordinal);
            }
            pageUrl = pageUrl.Replace("#", "%23", StringComparison.Ordinal);
            index = -1;
            if ((bool)vs[1])
            {
                TitleBar.Visibility = Visibility.Collapsed;
                listBorder.Padding = new Thickness(0);
            }
            GetUrlPage();
        }

        public async void GetUrlPage(int p = -1)
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

        private async Task<bool> GetUrlPage(int page, string url, ObservableCollection<Entity> FeedsCollection)
        {
            TitleBar.ShowProgressRing();
            var array = (JArray)await DataHelper.GetDataAsync(DataUriType.GetIndexPage, url, url == "/main/indexV8" ? "?" : "&", page);
            bool result = false;
            if (array != null && array.Count > 0)
                if (page == 1)
                {
                    int n = 0;
                    if (FeedsCollection.Count > 0)
                    {
                        var needDeleteItems = (from b in FeedsCollection
                                               from c in array
                                               where b.EntityId == c.Value<string>("entityId").Replace("\"", string.Empty, StringComparison.Ordinal)
                                               select b).ToArray();
                        foreach (var item in needDeleteItems)
                            Collection.Remove(item);
                        n = (from b in FeedsCollection
                             where b.EntityFixed
                             select b).Count();
                    }
                    int k = 0;
                    for (int i = 0; i < array.Count; i++)
                    {
                        JObject jo = array[i] as JObject;
                        if (index == -1 && jo.TryGetValue("entityTemplate", out JToken t) && t?.ToString() == "configCard")
                        {
                            JObject j = JObject.Parse(jo.Value<string>("extraData"));
                            TitleBar.Title = j.Value<string>("pageTitle");
                            continue;
                        }
                        if (jo.TryGetValue("entityTemplate", out JToken tt) && tt?.ToString() == "fabCard") continue;
                        else if (tt?.ToString() == "feedCoolPictureGridCard")
                        {
                            foreach (JObject item in jo.Value<JArray>("entities"))
                            {
                                var entity = GetEntity(item);
                                if (entity != null)
                                {
                                    FeedsCollection.Insert(n + k, entity);
                                    k++;
                                }
                            }
                        }
                        else
                        {
                            var entity = GetEntity(jo);
                            if (entity != null)
                            {
                                FeedsCollection.Insert(n + k, entity);
                                k++;
                            }
                        }
                    }
                    TitleBar.HideProgressRing();
                    result = true;
                }
                else
                {
                    if (array.Count != 0)
                    {
                        foreach (JObject i in array)
                        {
                            if (i.TryGetValue("entityTemplate", out JToken tt) && tt?.ToString() == "feedCoolPictureGridCard")
                            {
                                foreach (JObject item in i.Value<JArray>("entities"))
                                {
                                    var entity = GetEntity(item);
                                    if (entity != null) FeedsCollection.Add(entity);
                                }
                            }
                            else
                            {
                                var entity = GetEntity(i);
                                if (entity != null)
                                    FeedsCollection.Add(entity);
                            }
                        }
                        TitleBar.HideProgressRing();
                        result = true;
                    }
                    else
                    {
                        TitleBar.HideProgressRing();
                    }
                }
            return result;
        }

        private Entity GetEntity(JObject jo)
        {
            switch (jo.Value<string>("entityType"))
            {
                case "feed": return new FeedModel(jo, pageUrl == "/main/indexV8" ? FeedDisplayMode.isFirstPageFeed : FeedDisplayMode.normal);
                case "user": return new UserModel(jo);
                case "topic": return new TopicModel(jo);
                case "dyh": return new DyhModel(jo);
                case "card":
                default:
                    if (jo.TryGetValue("entityTemplate", out JToken v1))
                    {
                        switch (v1.Value<string>())
                        {
                            case "imageTextGridCard":
                            case "imageSquareScrollCard":
                            case "iconScrollCard":
                            case "iconGridCard":
                            case "feedScrollCard":
                            case "imageTextScrollCard":
                            case "iconMiniLinkGridCard":
                            case "iconMiniGridCard": return new IndexPageHasEntitiesModel(jo, EntitiesType.Others);
                            case "headCard":
                            case "imageCarouselCard_1": //return new IndexPageHasEntitiesViewModel(jo, EntitiesType.Image_1);
                            case "imageCard": return new IndexPageHasEntitiesModel(jo, EntitiesType.Image);
                            case "iconLinkGridCard": return new IndexPageHasEntitiesModel(jo, EntitiesType.IconLink);
                            case "feedGroupListCard":
                            case "textLinkListCard": return new IndexPageHasEntitiesModel(jo, EntitiesType.TextLink);
                            case "textCard":
                            case "messageCard": return new IndexPageMessageCardModel(jo);
                            case "refreshCard": return new IndexPageOperationCardModel(jo, OperationType.Refresh);
                            case "unLoginCard": return new IndexPageOperationCardModel(jo, OperationType.Login);
                            case "titleCard": return new IndexPageOperationCardModel(jo, OperationType.ShowTitle);
                            case "iconTabLinkGridCard": return new IndexPageHasEntitiesModel(jo, EntitiesType.TabLink);
                            case "selectorLinkCard": return new IndexPageHasEntitiesModel(jo, EntitiesType.SelectorLink);
                            default: return null;
                        }
                    }
                    else return null;
            }
        }

        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (!e.IsIntermediate && scrollViewer.VerticalOffset == scrollViewer.ScrollableHeight && CanLoadMore)
            {
                GetUrlPage();
            }
        }

        public void RefreshPage() => GetUrlPage(1);

        private void TitleBar_BackButtonClick(object sender, RoutedEventArgs e) => Frame.GoBack();

        private void OnTapped(object tag)
        {
            if (tag is string s)
            {
                if (!string.IsNullOrEmpty(s))
                {
                    UIHelper.OpenLinkAsync(s);
                }
            }
            else if (tag is IHasUriAndTitle u)
            {
                if (string.IsNullOrEmpty(u.Url) || u.Url == "/topic/quickList?quickType=list") { return; }
                string str = u.Url;
                if (str == "Refresh") { RefreshPage(); }
                else if (str == "Login")
                {
                    UIHelper.Navigate(typeof(BrowserPage), new object[] { true, null });
                }
                else if (str.IndexOf("/page", StringComparison.Ordinal) == 0)
                {
                    str = str.Replace("/page", "/page/dataList", StringComparison.Ordinal);
                    str += $"&title={u.Title}";
                    UIHelper.Navigate(typeof(IndexPage), new object[] { str, false });
                }
                else if (str.IndexOf('#') == 0)
                {
                    UIHelper.Navigate(typeof(IndexPage), new object[] { $"{str}&title={u.Title}", false });
                }
                else { UIHelper.OpenLinkAsync(str); }
            }
            else if (tag is IndexPageModel i)
            {
                if (!string.IsNullOrEmpty(i.Url))
                {
                    UIHelper.OpenLinkAsync(i.Url);
                }
            }
        }

        private void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (e != null && !UIHelper.IsOriginSource(sender, e.OriginalSource)) { return; }

            OnTapped((sender as FrameworkElement).Tag);
        }

        private void ListViewItem_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter || e.Key == Windows.System.VirtualKey.Space)
            {
                OnTapped((sender as FrameworkElement).Tag);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OnTapped((sender as FrameworkElement).Tag);
        }

        private void ListViewItem_Tapped_1(object sender, TappedRoutedEventArgs e)
        {
            IndexPageModel model = (sender as FrameworkElement).DataContext as IndexPageModel;
            if (Feeds2.Count > 0)
            {
                ObservableCollection<Entity> feeds = Feeds2[0];
                var needDeleteItems = (from b in feeds
                                       where b.EntityType == "feed"
                                       select b).ToArray();
                foreach (var item in needDeleteItems)
                    feeds.Remove(item);
                urls[0] = $"/page/dataList?url={model.Url}&title={model.Title}";
                urls[0] = urls[0].Replace("#", "%23", StringComparison.Ordinal);
                pages[0] = 0;
            }
            else
            {
                ObservableCollection<Entity> feeds = Collection;
                var needDeleteItems = (from b in feeds
                                       where b.EntityType == "topic"
                                       select b).ToArray();
                foreach (var item in needDeleteItems)
                    feeds.Remove(item);
                pageUrl = $"/page/dataList?url={model.Url}&title={model.Title}";
                pageUrl = pageUrl.Replace("#", "%23", StringComparison.Ordinal);
                page = 0;
            }
            GetUrlPage();
        }

        private void FlipView_Loaded(object sender, RoutedEventArgs e)
        {
            var view = sender as FlipView;
            var timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(4)
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

        public void ChangeTabView(string u)
        {
            pageUrl = u;
            page = 0;
            Collection.Clear();
            GetUrlPage();
        }

        private void Pivot_Loaded(object sender, RoutedEventArgs e)
        {
            var element = sender as Pivot;
            index = element.SelectedIndex;
            if (element.Items.Count == 0)
            {
                var f = element.Tag as Entity[];
                for (int j = 0; j < f.Length; j++)
                {
                    var ff = new ObservableCollection<Entity>();

                    var l = new Microsoft.UI.Xaml.Controls.ItemsRepeater
                    {
                        ItemTemplate = Resources["FTemplateSelector"] as DataTemplateSelector,
                        ItemsSource = ff,
                    };

                    var model = f[j] as IndexPageModel;
                    var i = new PivotItem
                    {
                        Tag = f[j],
                        Content = l,
                        Header = model.Title
                    };
                    element.Items.Add(i);
                    pages.Add(1);
                    Feeds2.Add(ff);
                    urls.Add("/page/dataList?url=" + model.Url.Replace("#", "%23", StringComparison.Ordinal) + $"&title={model.Title}");
                    if (j == 0) LoadPivotItem(element, i);
                }
                return;
            }
        }

        private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadPivotItem(sender as Pivot);
        }

        private void LoadPivotItem(Pivot element, PivotItem i = null)
        {
            var item = i is null ? element.SelectedItem as PivotItem : i;
            var view = item.Content as Microsoft.UI.Xaml.Controls.ItemsRepeater;
            var feeds = view.ItemsSource as ObservableCollection<Entity>;
            if (feeds.Count < 1)
            {
                var model = item.Tag as IndexPageModel;
                string u = model.Url;
                if (model.Url.IndexOf("/page", StringComparison.Ordinal) != 0)
                {
                    u = u.Replace("#", "%23", StringComparison.Ordinal);
                    u = "/page/dataList?url=" + u + $"&title={model.Title}";
                }
                _ = GetUrlPage(1, u, feeds);
            }
        }
    }
}