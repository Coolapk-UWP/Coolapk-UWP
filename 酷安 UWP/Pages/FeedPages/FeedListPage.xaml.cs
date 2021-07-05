using CoolapkUWP.Control.ViewModels;
using CoolapkUWP.Data;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace CoolapkUWP.Pages.FeedPages
{
    internal enum FeedListType
    {
        UserPageList,
        TagPageList,
        DYHPageList
    }
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class FeedListPage : Page
    {
        private interface IFeedListDataProvider
        {
            string Id { get; }
            FeedListType ListType { get; }
            Task<object> GetDetail();
            Task<List<FeedViewModel>> GetFeeds(int p = -1);
            string GetTitleBarText(object o);
        }

        private interface ICanChangeSelectedIndex : IFeedListDataProvider
        {
            int SelectedIndex { get; set; }
            void Reset();
        }

        private class UserPageDataProvider : IFeedListDataProvider
        {
            public string Id { get; private set; }

            private int page;
            private double firstItem, lastItem;
            public FeedListType ListType { get => FeedListType.UserPageList; }
            public UserPageDataProvider(string uid) => Id = uid;

            public async Task<object> GetDetail()
            {
                JsonObject detail = UIHelper.GetJSonObject(await UIHelper.GetJson("/user/space?uid=" + Id));
                return detail != null
                    ? new UserDetail
                    {
                        FollowStatus = detail["uid"].GetNumber().ToString() == SettingHelper.GetString("uid") ? string.Empty
                                                                                                         : detail["isFollow"].GetNumber() == 0 ? "关注" : "取消关注",
                        UserFaceUrl = detail["userAvatar"].GetString(),
                        UserName = detail["username"].GetString(),
                        FollowNum = detail["follow"].GetNumber(),
                        FansNum = detail["fans"].GetNumber(),
                        Level = detail["level"].GetNumber(),
                        Bio = detail["bio"].GetString(),
                        BackgroundUrl = detail["cover"].GetString(),
                        Verify_title = detail["verify_title"].GetString(),
                        Gender = detail["gender"].GetNumber() == 1 ? "♂" : (detail["gender"].GetNumber() == 0 ? "♀" : string.Empty),
                        City = $"{detail["province"].GetString()} {detail["city"].GetString()}",
                        Astro = detail["astro"].GetString(),
                        Logintime = $"{UIHelper.ConvertTime(detail["logintime"].GetNumber())}活跃",
                        FeedNum = detail["feed"].GetNumber(),
                        UserFace = await ImageCache.GetImage(ImageType.SmallAvatar, detail["userSmallAvatar"].GetString()),
                        Background = new BackgroundImageViewModel(detail["cover"].GetString(), ImageType.OriginImage),
                    }
                    : null;
            }

            public async Task<List<FeedViewModel>> GetFeeds(int p = -1)
            {
                if (p == 1 && page == 0) { page = 1; }
                JsonArray Root = UIHelper.GetDataArray(await UIHelper.GetJson($"/user/feedList?uid={Id}&page={(p == -1 ? ++page : p)}{(firstItem == 0 ? string.Empty : $"&firstItem={firstItem}")}{(lastItem == 0 ? string.Empty : $"&lastItem={lastItem}")}"));
                if (!(Root is null) && Root.Count != 0)
                {
                    if (page == 1 || p == 1)
                    { firstItem = Root.First()?.GetObject()["id"].GetNumber() ?? firstItem; }
                    lastItem = Root.Last().GetObject()["id"].GetNumber();
                    List<FeedViewModel> FeedsCollection = new List<FeedViewModel>();
                    foreach (IJsonValue i in Root) { FeedsCollection.Add(new FeedViewModel(i)); }
                    return FeedsCollection;
                }
                else
                {
                    page--;
                    return null;
                }
            }

            public string GetTitleBarText(object o) => (o as UserDetail).UserName;
        }

        private class TagPageDataProvider : ICanChangeSelectedIndex
        {
            public string Id { get; private set; }

            private int page, _selectedIndex;
            public int SelectedIndex
            {
                get => _selectedIndex;
                set
                {
                    if (value > -1)
                        _selectedIndex = value;
                }
            }

            private double firstItem, lastItem;
            public FeedListType ListType { get => FeedListType.TagPageList; }
            public TagPageDataProvider(string tag) => Id = tag;

            public void Reset() => firstItem = lastItem = page = 0;

            public async Task<object> GetDetail()
            {
                JsonObject detail = UIHelper.GetJSonObject(await UIHelper.GetJson("/topic/newTagDetail?tag=" + Id));
                return detail != null
                    ? new TopicDetail
                    {
                        Logo = await ImageCache.GetImage(ImageType.Icon, detail["logo"].GetString()),
                        Title = detail["title"].GetString(),
                        FollowNum = detail.TryGetValue("follownum", out IJsonValue t) ? t.GetNumber() : detail["follow_num"].GetNumber(),
                        CommentNum = detail.TryGetValue("commentnum", out IJsonValue tt) ? tt.GetNumber() : detail["rating_total_num"].GetNumber(),
                        Description = detail["description"].GetString(),
                        SelectedIndex = SelectedIndex
                    }
                    : null;
            }

            public async Task<List<FeedViewModel>> GetFeeds(int p = -1)
            {
                string sortType = string.Empty;
                switch (SelectedIndex)
                {
                    case 0:
                        sortType = "lastupdate_desc";
                        break;
                    case 1:
                        sortType = "dateline_desc";
                        break;
                    case 2:
                        sortType = "popular";
                        break;
                    default:
                        break;
                }
                if (p == 1 && page == 0) { page = 1; }
                JsonArray Root = UIHelper.GetDataArray(await UIHelper.GetJson($"/topic/tagFeedList?tag={Id}&page={(p == -1 ? ++page : p)}{(firstItem == 0 ? string.Empty : $"&firstItem={firstItem}")}{(lastItem == 0 ? string.Empty : $"&lastItem={lastItem}")}&listType={sortType}&blockStatus=0"));
                if (!(Root is null) && Root.Count != 0)
                {
                    if (page == 1 || p == 1)
                    { firstItem = Root.First()?.GetObject()["id"].GetNumber() ?? firstItem; }
                    lastItem = Root.Last()?.GetObject()["id"].GetNumber() ?? lastItem;
                    List<FeedViewModel> FeedsCollection = new List<FeedViewModel>();
                    foreach (IJsonValue i in Root) { FeedsCollection.Add(new FeedViewModel(i)); }
                    return FeedsCollection;
                }
                else
                {
                    page--;
                    return null;
                }
            }

            public string GetTitleBarText(object o) => (o as TopicDetail).Title;
        }

        private class DYHPageDataProvider : ICanChangeSelectedIndex
        {
            public string Id { get; private set; }

            private int page, _selectedIndex;
            public int SelectedIndex
            {
                get => _selectedIndex;
                set
                {
                    if (value > -1)
                        _selectedIndex = value;
                }
            }

            private double firstItem, lastItem;
            public FeedListType ListType { get => FeedListType.DYHPageList; }
            public DYHPageDataProvider(string id) => Id = id;

            public void Reset() => firstItem = lastItem = page = 0;

            public async Task<object> GetDetail()
            {
                JsonObject detail = UIHelper.GetJSonObject(await UIHelper.GetJson("/dyh/detail?dyhId=" + Id));
                if (detail != null)
                {
                    bool showUserButton = detail["uid"].GetNumber() != 0;
                    return new DYHDetail
                    {
                        Logo = await ImageCache.GetImage(ImageType.Icon, detail["logo"].GetString()),
                        Title = detail["title"].GetString(),
                        Description = detail["description"].GetString(),
                        FollowNum = detail["follownum"].GetNumber(),
                        ShowUserButton = showUserButton,
                        Url = showUserButton ? detail["userInfo"].GetObject()["url"].GetString() : string.Empty,
                        UserName = showUserButton ? detail["userInfo"].GetObject()["username"].GetString() : string.Empty,
                        UserAvatar = showUserButton ? await ImageCache.GetImage(ImageType.SmallAvatar, detail["userInfo"].GetObject()["userSmallAvatar"].ToString().Replace("\"", string.Empty)) : null,
                        SelectedIndex = SelectedIndex,
                        ShowComboBox = detail["is_open_discuss"].GetNumber() == 1
                    };
                }
                else { return null; }
            }

            public async Task<List<FeedViewModel>> GetFeeds(int p = -1)
            {
                if (p == 1 && page == 0) { page = 1; }
                JsonArray Root = UIHelper.GetDataArray(await UIHelper.GetJson($"/dyhArticle/list?dyhId={Id}&type={(SelectedIndex == 0 ? "all" : "square")}&page={(p == -1 ? ++page : p)}{(firstItem == 0 ? string.Empty : $"&firstItem={firstItem}")}{((lastItem == 0) ? string.Empty : $"&lastItem={lastItem}")}"));
                if (!(Root is null) && Root.Count != 0)
                {
                    if (page == 1 || p == 1)
                    { firstItem = Root.First()?.GetObject()["id"].GetNumber() ?? firstItem; }
                    lastItem = Root.Last()?.GetObject()["id"].GetNumber() ?? lastItem;
                    List<FeedViewModel> FeedsCollection = new List<FeedViewModel>();
                    foreach (IJsonValue i in Root) { FeedsCollection.Add(new FeedViewModel(i)); }
                    return FeedsCollection;
                }
                else
                {
                    page--;
                    return null;
                }
            }

            public string GetTitleBarText(object o) => (o as DYHDetail).Title;
        }

        IFeedListDataProvider provider;
        ScrollViewer VScrollViewer;
        ObservableCollection<object> itemCollection = new ObservableCollection<object>();

        public FeedListPage() => InitializeComponent();
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            object[] vs = e.Parameter as object[];
            string str = vs[1] as string;
            FeedListType feedListType = (FeedListType)vs[0];
            if (!string.IsNullOrEmpty(str))
            {
                if (feedListType != provider?.ListType || (feedListType == provider?.ListType && str != provider.Id))
                {
                    if (itemCollection.Count > 0) itemCollection.Clear();
                    switch (feedListType)
                    {
                        case FeedListType.UserPageList:
                            if (str == "0") { Frame.GoBack(); }
                            else { provider = new UserPageDataProvider(str); }
                            titleBar.ComboBoxVisibility = Visibility.Collapsed;
                            break;
                        case FeedListType.TagPageList:
                            provider = new TagPageDataProvider(str);
                            titleBar.ComboBoxVisibility = Visibility.Visible;
                            titleBar.ComboBoxItemsSource = new string[] { "最近回复", "按时间排序", "按热度排序" };
                            titleBar.ComboBoxSelectedIndex = 0;
                            break;
                        case FeedListType.DYHPageList:
                            provider = new DYHPageDataProvider(str);
                            titleBar.ComboBoxVisibility = Visibility.Collapsed;
                            titleBar.ComboBoxItemsSource = new string[] { "精选", "广场" };
                            break;
                    }
                    Refresh();
                }
            }
            else { Frame.GoBack(); }
            if (VScrollViewer is null)
            {
                _ = Task.Run(async () =>
                  {
                      await Task.Delay(300);
                      await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                      {
                          VScrollViewer = VisualTree.FindDescendantByName(listView, "ScrollViewer") as ScrollViewer;
                          VScrollViewer.ViewChanged += async (s, ee) =>
                          {
                              if (!ee.IsIntermediate && VScrollViewer.VerticalOffset == VScrollViewer.ScrollableHeight)
                              {
                                  UIHelper.ShowProgressBar();
                                  List<FeedViewModel> feeds = await provider.GetFeeds();
                                  if (feeds != null)
                                  {
                                      foreach (FeedViewModel item in feeds)
                                      { itemCollection.Add(item); }
                                  }

                                  UIHelper.HideProgressBar();
                              }
                          };
                      });
                  });
            }
        }

        private async void Refresh()
        {
            UIHelper.ShowProgressBar();
            if (itemCollection.Count > 0) { itemCollection.RemoveAt(0); }
            itemCollection.Insert(0, await provider.GetDetail());
            if (itemCollection[0] is DYHDetail detail)
            {
                titleBar.ComboBoxSelectedIndex = detail.SelectedIndex;
                titleBar.ComboBoxVisibility = detail.ShowComboBox ? Visibility.Visible : Visibility.Collapsed;
            }

            List<FeedViewModel> feeds = await provider.GetFeeds(1);
            if (feeds != null)
            {
                for (int i = 0; i < feeds.Count; i++)
                { itemCollection.Insert(i + 1, feeds[i]); }
            }

            titleBar.Title = provider.GetTitleBarText(itemCollection[0]);
            UIHelper.HideProgressBar();
        }
        private void TitleBar_BackButtonClick(object sender, RoutedEventArgs e) => Frame.GoBack();

        private void UserDetailBorder_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender is FrameworkElement fe)
            {
                if (fe != e.OriginalSource) { return; }
                if (fe.Tag is string s)
                {
                    if (s == (itemCollection[0] as UserDetail).BackgroundUrl)
                    { UIHelper.ShowImage(s, ImageType.OriginImage); }
                    else { UIHelper.ShowImage(s, ImageType.SmallAvatar); }
                }
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            switch (button.Tag as string)
            {
                case "follow":
                    UIHelper.Navigate(typeof(UserListPage), new object[] { provider.Id, true, titleBar.Title });
                    break;
                case "fans":
                    UIHelper.Navigate(typeof(UserListPage), new object[] { provider.Id, false, titleBar.Title });
                    break;
                case "FollowUser":
                    JsonObject o = null;
                    switch ((itemCollection[0] as UserDetail).FollowStatus)
                    {
                        case "关注":
                            o = JsonObject.Parse(await UIHelper.GetJson($"/user/follow?uid={provider.Id}"));
                            break;
                        case "取消关注":
                            o = JsonObject.Parse(await UIHelper.GetJson($"/user/unfollow?uid={provider.Id}"));
                            break;
                        default:
                            break;
                    }
                    if (o != null)
                    {
                        if (o.TryGetValue("message", out IJsonValue value))
                            UIHelper.ShowMessage($"{value.GetString()}");
                        else
                        {
                            itemCollection.RemoveAt(0);
                            itemCollection.Insert(0, await provider.GetDetail());
                        }
                    }
                    break;
                default:
                    UIHelper.OpenLink(button.Tag as string);
                    break;
            }
        }

        private void FeedTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(provider.Id)) { return; }
            ICanChangeSelectedIndex dataProvider = provider as ICanChangeSelectedIndex;
            dataProvider.SelectedIndex = (sender as ComboBox).SelectedIndex;
            dataProvider.Reset();
            if (itemCollection.Count > 1)
            {
                for (int i = itemCollection.Count - 1; i > 0; i--)
                { itemCollection.RemoveAt(i); }
                Refresh();
            }
        }

        internal static void UserDetailBorder_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Border b = sender as Border;
            b.Height = e.NewSize.Width <= 400 ? e.NewSize.Width : 400;
        }
    }

    internal class UserDetail
    {
        public string UserFaceUrl;
        public ImageSource UserFace;
        public string UserName;
        public double FollowNum;
        public double FansNum;
        public double FeedNum;
        public double Level;
        public string Bio;
        public string BackgroundUrl;
        public BackgroundImageViewModel Background;
        public string Verify_title;
        public string Gender;
        public string City;
        public string Astro;
        public string Logintime;
        public string FollowStatus;
        public bool ShowFollowStatus { get => !string.IsNullOrEmpty(FollowStatus); }
        public bool Has_bio { get => !string.IsNullOrEmpty(Bio); }
        public bool Has_verify_title { get => !string.IsNullOrEmpty(Verify_title); }
        public bool Has_Astro { get => !string.IsNullOrEmpty(Astro); }
        public bool Has_City { get => !string.IsNullOrWhiteSpace(City) && !string.IsNullOrEmpty(City); }
        public bool Has_Gender { get => !string.IsNullOrEmpty(Gender); }
    }

    internal class TopicDetail
    {
        public ImageSource Logo { get; set; }
        public string Title { get; set; }
        public double FollowNum { get; set; }
        public double CommentNum { get; set; }
        public string Description { get; set; }
        public int SelectedIndex { get; set; }
    }

    internal class DYHDetail
    {
        public ImageSource Logo { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public double FollowNum { get; set; }
        public bool ShowUserButton { get; set; }
        public string Url { get; set; }
        public string UserName { get; set; }
        public ImageSource UserAvatar { get; set; }
        public int SelectedIndex { get; set; }
        public bool ShowComboBox { get; set; }
    }

    internal class TemplateSelector : DataTemplateSelector
    {
        public DataTemplate DataTemplate1 { get; set; }
        public DataTemplate DataTemplate2 { get; set; }
        public DataTemplate DataTemplate3 { get; set; }
        public DataTemplate DataTemplate4 { get; set; }
        protected override DataTemplate SelectTemplateCore(object item)
        {
            return item is UserDetail ? DataTemplate1 : item is TopicDetail ? DataTemplate3 : item is DYHDetail ? DataTemplate4 : DataTemplate2;
        }
        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container) => SelectTemplateCore(item);
    }
}
