using CoolapkUWP.Control;
using CoolapkUWP.Control.ViewModels;
using CoolapkUWP.Data;
using CoolapkUWP.Pages.FeedPages;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace CoolapkUWP.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class UserHubPage : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private ImageSource userAvatar;
        private string userName;
        private double followNum;
        private double fansNum;
        private double feedNum;
        private double levelNum;
        private int page = 0;
        private readonly List<int> pages = new List<int>();
        private string pageUrl;
        private readonly ObservableCollection<Entity> Collection = new ObservableCollection<Entity>();
        private int index;
        private readonly List<string> urls = new List<string>();
        private readonly ObservableCollection<ObservableCollection<Entity>> Feeds2 = new ObservableCollection<ObservableCollection<Entity>>();

        public UserHubPage() => this.InitializeComponent();
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            string uid = SettingHelper.GetString("Uid");
            if (string.IsNullOrEmpty(uid))
            {
                LoginButton.Visibility = Visibility.Visible;
                UserDetailGrid.Visibility = Visibility.Collapsed;
            }
            else
            {
                LoginButton.Visibility = Visibility.Collapsed;
                UserDetailGrid.Visibility = Visibility.Visible;
                JsonObject o = UIHelper.GetJSonObject(await UIHelper.GetJson("/user/profile?uid=" + uid));
                UIHelper.mainPage.UserAvatar = userAvatar = await ImageCache.GetImage(ImageType.BigAvatar, o["userAvatar"].GetString());
                UIHelper.mainPage.UserNames = userName = o["username"].GetString();
                feedNum = o["feed"].GetNumber();
                followNum = o["follow"].GetNumber();
                fansNum = o["fans"].GetNumber();
                levelNum = o["level"].GetNumber();
                index = -1;
                GetUrlPage();
                _ = Task.Run(async () =>
                  {
                      await Task.Delay(1000);
                      await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                      {
                          (VisualTree.FindDescendantByName(listView, "ScrollViewer") as ScrollViewer).ViewChanged += ScrollViewer_ViewChanged;
                      });
                  });
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(userAvatar)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(userName)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(fansNum)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(feedNum)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(followNum)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(levelNum)));
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e) => UIHelper.Navigate(typeof(BrowserPage), new object[] { true, null });

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as FrameworkElement).Tag as string)
            {
                case "feed":
                    UIHelper.Navigate(typeof(FeedListPage), new object[] { FeedListType.UserPageList, SettingHelper.GetString("Uid") });
                    break;
                case "follow":
                    UIHelper.Navigate(typeof(UserListPage), new object[] { SettingHelper.GetString("Uid"), true, userName });
                    break;
                case "fans":
                    UIHelper.Navigate(typeof(UserListPage), new object[] { SettingHelper.GetString("Uid"), false, userName });
                    break;
                case "settings":
                    UIHelper.Navigate(typeof(SettingPages.SettingPage));
                    break;
                default:
                    break;
            }
        }

        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            ScrollViewer VScrollViewer = sender as ScrollViewer;
            if (!e.IsIntermediate)
            {
                if (Collection.Count != 0)
                {
                    if (VScrollViewer.VerticalOffset == VScrollViewer.ScrollableHeight)
                    { GetUrlPage(); }
                }
            }
        }

        private async void GetUrlPage(int p = -1)
        {
            if (index == -1)
            {
                if (!await GetUrlPage(p == -1 ? ++page : p, pageUrl, Collection))
                { page--; }
            }
            else if (p == -1)
            {
                if (!await GetUrlPage(page = p == -1 ? ++pages[index] : p, urls[index], Feeds2[index]))
                { pages[index]--; }
            }
        }

        public void RefreshPage() => GetUrlPage(1);

        private void FeedListViewItem_Tapped(object sender, TappedRoutedEventArgs e) => UIHelper.OpenLink((sender as FrameworkElement).Tag as string);

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
                foreach (Entity item in needDeleteItems)
                { _ = feeds.Remove(item); }
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
                foreach (Entity item in needDeleteItems)
                { _ = feeds.Remove(item); }
                pageUrl = $"/page/dataList?url={model.url}&title={model.title}";
                pageUrl = pageUrl.Replace("#", "%23");
                page = 0;
            }
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

        private async Task<bool> GetUrlPage(int page, string url, ObservableCollection<Entity> FeedsCollection)
        {
            UIHelper.ShowProgressBar();
            string s = await UIHelper.GetJson($"/account/loadConfig?key=my_page_card_config");
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
                        foreach (var item in needDeleteItems)
                        { Collection.Remove(item); }
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

            return false;
        }

        private Entity GetEntity(JsonObject token)
        {
            switch (token["entityType"].GetString())
            {
                case "feed": return new FeedViewModel(token, pageUrl == "/main/indexV8" ? FeedDisplayMode.isFirstPageFeed : FeedDisplayMode.normal);
                case "user": return new UserViewModel(token);
                case "topic": return new TopicViewModel(token);
                case "dyh": return new DyhViewModel(token);
                case "card":
                default: return new IndexPageViewModel(token);
            }
        }

        internal static void TextBlockEx_RichTextBlockLoaded(object sender, EventArgs e)
        {
            MTextBlock b = (MTextBlock)sender;
            b.MaxLine = 2;
        }
    }
}
