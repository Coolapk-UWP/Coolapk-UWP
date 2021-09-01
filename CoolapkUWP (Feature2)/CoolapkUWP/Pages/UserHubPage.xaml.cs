using CoolapkUWP.Control;
using CoolapkUWP.Control.ViewModels;
using CoolapkUWP.Data;
using CoolapkUWP.Pages.FeedPages;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Storage;
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
        private double nextLevelExperience;
        private double nextLevelPercentage;
        private string nextLevelNowExperience;
        private string levelTodayMessage;
        private string leveldetailurl;
        private bool IsAuthor;
        private bool IsSpecial;
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
            string uid = SettingsHelper.GetString("Uid");
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
                if (o != null)
                {
                    if (o.TryGetValue("userAvatar", out IJsonValue userAvatarurl) && !string.IsNullOrEmpty(userAvatarurl.GetString()))
                    {
                        UIHelper.mainPage.UserAvatar = userAvatar = ImageCache.defaultNoAvatarUrl.Contains(userAvatarurl.GetString()) ? ImageCache.NoPic : await ImageCache.GetImage(ImageType.BigAvatar, userAvatarurl.GetString());
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(userAvatar)));
                    }
                    if (o.TryGetValue("username", out IJsonValue username) && !string.IsNullOrEmpty(username.GetString()))
                    {
                        UIHelper.mainPage.UserNames = userName = username.GetString();
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(userName)));
                    }
                    if (o.TryGetValue("entityId", out IJsonValue entityId))
                    {
                        IsAuthor = authors.Contains(entityId.GetNumber());
                        ApplicationData.Current.LocalSettings.Values["IsAuthor"] = IsAuthor;
                        IsSpecial = specials.Contains(entityId.GetNumber());
                        ApplicationData.Current.LocalSettings.Values["IsSpecial"] = IsSpecial;
                    }
                    if (o.TryGetValue("feed", out IJsonValue feed) && !string.IsNullOrEmpty(feed.GetNumber().ToString()))
                    {
                        feedNum = feed.GetNumber();
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(feedNum)));
                    }
                    if (o.TryGetValue("follow", out IJsonValue follow) && !string.IsNullOrEmpty(follow.GetNumber().ToString()))
                    {
                        followNum = follow.GetNumber();
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(followNum)));
                    }
                    if (o.TryGetValue("fans", out IJsonValue fans) && !string.IsNullOrEmpty(fans.GetNumber().ToString()))
                    {
                        fansNum = fans.GetNumber();
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(fansNum)));
                    }
                    if (o.TryGetValue("level", out IJsonValue level) && !string.IsNullOrEmpty(level.GetNumber().ToString()))
                    {
                        levelNum = level.GetNumber();
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(levelNum)));
                    }
                    if (o.TryGetValue("next_level_experience", out IJsonValue next_level_experience) && !string.IsNullOrEmpty(next_level_experience.GetNumber().ToString()))
                    {
                        nextLevelExperience = next_level_experience.GetNumber();
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(nextLevelExperience)));
                    }
                    if (o.TryGetValue("next_level_percentage", out IJsonValue next_level_percentage) && !string.IsNullOrEmpty(next_level_percentage.GetString()))
                    {
                        nextLevelPercentage = double.Parse(next_level_percentage.GetString());
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(nextLevelPercentage)));
                    }
                    if (o.TryGetValue("level_today_message", out IJsonValue level_today_message) && !string.IsNullOrEmpty(level_today_message.GetString()))
                    {
                        levelTodayMessage = level_today_message.GetString();
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(levelTodayMessage)));
                    }
                    if (o.TryGetValue("level_detail_url", out IJsonValue level_detail_url) && !string.IsNullOrEmpty(level_detail_url.GetString()))
                    {
                        leveldetailurl = level_detail_url.GetString();
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(leveldetailurl)));
                    }
                    nextLevelNowExperience = $"{nextLevelPercentage / 100 * nextLevelExperience:F0}/{nextLevelExperience}";
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(nextLevelNowExperience)));
                }
                else
                {
                    UIHelper.mainPage.UserAvatar = userAvatar = ImageCache.NoPic;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(userAvatar)));
                    UIHelper.mainPage.UserNames = userName = "网络错误";
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(userName)));

                }
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
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e) => UIHelper.Navigate(typeof(BrowserPage), new object[] { true, null });

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as FrameworkElement).Tag as string)
            {
                case "MyFeed":
                case "feed":
                    UIHelper.Navigate(typeof(FeedListPage), new object[] { FeedListType.UserPageList, SettingsHelper.GetString("Uid") });
                    break;
                case "follow":
                    UIHelper.Navigate(typeof(UserListPage), new object[] { SettingsHelper.GetString("Uid"), true, userName });
                    break;
                case "fans":
                    UIHelper.Navigate(typeof(UserListPage), new object[] { SettingsHelper.GetString("Uid"), false, userName });
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
                                            where b.EntityType == "feed"
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
                                            where b.EntityType == "topic"
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
            string s = await UIHelper.GetJson("/account/loadConfig?key=my_page_card_config");
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
                                                        where b.EntityId == c.GetObject()["entityId"].ToString().Replace("\"", string.Empty)
                                                        select b).ToArray();
                            foreach (Entity item in needDeleteItems)
                            { _ = Collection.Remove(item); }
                            n = (from b in FeedsCollection
                                 where b.EntityFixed
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
                case "card":
                default: return new IndexPageViewModel(token);
            }
        }

        internal static void TextBlockEx_RichTextBlockLoaded(object sender, EventArgs e)
        {
            MTextBlock b = (MTextBlock)sender;
            b.MaxLine = 2;
        }

        #region
        private static readonly ImmutableArray<double> authors = new double[]
        {
            536381,//wherewhere
            695942,//一块小板子
        }.ToImmutableArray();

        private static readonly ImmutableArray<double> specials = new double[]
        {
            1222543,
            1893913,
            2134270,
            1494629,
            3327704,
            3591060,
        }.ToImmutableArray();
        #endregion

        private void LvHelp_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            string Url = string.IsNullOrEmpty(element.Tag.ToString()) ? "/feed/18221454" : element.Tag.ToString();
            UIHelper.OpenLink(Url);
        }
    }
}
