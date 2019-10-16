using Microsoft.Toolkit.Uwp.UI.Controls;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using 酷安_UWP.UsersPage;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace 酷安_UWP
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class FeedDetailPage : Page
    {
        string id;
        MainPage mainPage;
        ObservableCollection<Feed2> feeds = new ObservableCollection<Feed2>();
        ObservableCollection<Feed2> replys = new ObservableCollection<Feed2>();
        Feed2 reply;
        int feedpage = 1;
        int likepage = 0;
        int sharepage = 0;
        string feedfirstItem, feedlastItem;
        string likefirstItem, likelastItem;
        static ObservableCollection<ImageSource> list = new ObservableCollection<ImageSource>();
        public FeedDetailPage()
        {
            this.InitializeComponent();
            SFlipView.ItemsSource = list;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            //将传过来的数据 类型转换一下
            mainPage = ((object[])e.Parameter)[1] as MainPage;
            string title = (string)((object[])e.Parameter)[2];
            TitleTextBlock.Text = title;
            if (id != (string)((object[])e.Parameter)[0])
            {
                id = (string)((object[])e.Parameter)[0];
                mainPage.ActiveProgressRing();
                if (title == "动态")
                    LoadFeedDetail(id);
                if (title == "回复")
                {
                    TitleBar.Visibility = Visibility.Collapsed;
                    reply = ((object[])e.Parameter)[3] as Feed2;
                    LoadRepliesDetail(id);
                }
            }
        }

        public async void LoadFeedDetail(string id)
        {
            JObject detail = await CoolApkSDK.getFeedDetailById(id);
            JArray array = await CoolApkSDK.getFeedReplyListById(id, 1, 1, 0, string.Empty, string.Empty);
            FeedDetailList.ItemsSource = new Feed[] { new Feed(detail) };
            FeedDetailPivot.DataContext = new
            {
                replynum = detail.GetValue("replynum").ToString(),
                likenum = detail.GetValue("likenum").ToString(),
                forwardnum = detail.GetValue("forwardnum").ToString()
            };
            if (array.Count != 0)
            {
                feedfirstItem = array.First["id"].ToString();
                feedlastItem = array.Last["id"].ToString();
                feeds.Add(new Feed2(detail["hotReplyRows"], "热门回复"));
                feeds.Add(new Feed2(array, "最新回复"));
            }
            else feedpage--;
            mainPage.DeactiveProgressRing();
        }

        public async void LoadRepliesDetail(string id)
        {
            JArray array = await CoolApkSDK.getReplyListById(id, 1, 0, 0, string.Empty);
            FeedDetailList.ItemsSource = new Feed[] { reply };
            FeedDetailPivot.DataContext = new { replynum = reply.GetValue("replynum").ToString(), likenum = string.Empty, forwardnum = string.Empty };
            if (array.Count != 0)
            {
                feedlastItem = array.Last["id"].ToString();
                replys.Add(new Feed2(array, ""));
            }
            else feedpage--;
            mainPage.DeactiveProgressRing();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button i = sender as Button;
            mainPage.Frame.Navigate(typeof(UserPage), new object[] { i.Tag as string, mainPage });
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (TitleTextBlock.Text != "回复")
                Frame.GoBack();
        }

        private async void PivotItem_Loaded(object sender, RoutedEventArgs e)
        {
            PivotItem item = sender as PivotItem;
            switch (item.Tag as string)
            {
                case "1":
                    replyListView.ItemsSource = TitleTextBlock.Text == "回复" ? replys : feeds;
                    if (TitleTextBlock.Text == "回复")
                        FeedDetailPivot.IsLocked = true;
                    break;
                case "2":
                    JArray root = await CoolApkSDK.getFeedLikeUsersListById(id, ++likepage, string.Empty, string.Empty);
                    ObservableCollection<Feed> F = new ObservableCollection<Feed>();
                    if (root.Count != 0)
                    {

                        likefirstItem = root.First["uid"].ToString();
                        likelastItem = root.Last["uid"].ToString();
                        foreach (JObject i in root)
                            F.Add(new Feed(i));
                    }
                    else likepage--;
                    likeListView.ItemsSource = F;
                    break;
                case "3":
                    JArray roots = await CoolApkSDK.getShareListById(id, ++sharepage);
                    ObservableCollection<Feed> Fs = new ObservableCollection<Feed>();
                    if (roots.Count != 0)
                        foreach (JObject i in roots)
                            Fs.Add(new Feed(i));
                    else sharepage--;
                    shareuserListView.ItemsSource = Fs;
                    break;
                default:
                    break;
            }
        }

        private async void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (TitleTextBlock.Text != "回复")
            {
                if (FeedDetailPivot.SelectedIndex == 2)
                {
                    Frame.Navigate(typeof(FeedDetailPage),
                        new object[] { ((sender as FrameworkElement).Tag as Feed).GetValue("id"), mainPage, "动态", (sender as FrameworkElement).Tag });
                }
                else
                {
                    ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
                    ContentDialog1 contentDialog = new ContentDialog1
                    {
                        RequestedTheme = Convert.ToBoolean(localSettings.Values["IsDarkMode"]) ? ElementTheme.Dark : ElementTheme.Light
                    };
                    contentDialog.Navigate(typeof(FeedDetailPage),
                        new object[] { ((sender as FrameworkElement).Tag as Feed).GetValue("id"), mainPage, "回复", (sender as FrameworkElement).Tag });
                    await contentDialog.ShowAsync();
                }
            }
        }

        async void Refresh()
        {
            mainPage.ActiveProgressRing();
            switch (FeedDetailPivot.SelectedIndex.ToString())
            {
                case "0":
                    if (TitleTextBlock.Text == "动态")
                    {
                        JObject detail = await CoolApkSDK.getFeedDetailById(id);
                        JArray array = await CoolApkSDK.getFeedReplyListById(id, 1, 1, 0, feedfirstItem, feedlastItem);
                        FeedDetailList.ItemsSource = new Feed[] { new Feed(detail) };
                        FeedDetailPivot.DataContext = new
                        {
                            replynum = detail.GetValue("replynum").ToString(),
                            likenum = detail.GetValue("likenum").ToString(),
                            forwardnum = detail.GetValue("forwardnum").ToString()
                        };
                        feeds.RemoveAt(0);
                        feeds.Insert(0, new Feed2(detail["hotReplyRows"], "热门回复"));
                        if (array.Count != 0)
                        {
                            feedfirstItem = array.First["id"].ToString();
                            feeds.Insert(1, new Feed2(array, "最新回复"));
                        }
                        else feedpage--;
                        mainPage.DeactiveProgressRing();
                    }
                    if (TitleTextBlock.Text == "回复")
                    {
                        JArray array = await CoolApkSDK.getReplyListById(id, 1, 0, 0, feedlastItem);
                        if (array.Count != 0)
                            feeds.Insert(0, new Feed2(array, string.Empty));
                    }
                    break;
                case "1":
                    JArray root = await CoolApkSDK.getFeedLikeUsersListById(id, 1, likefirstItem, likelastItem);
                    if (root.Count != 0)
                    {
                        likefirstItem = root.First["id"].ToString();
                        ObservableCollection<Feed> F = likeListView.ItemsSource as ObservableCollection<Feed>;
                        for (int i = 0; i < root.Count; i++)
                            F.Insert(i, new Feed((JObject)root[i]));
                    }
                    break;
                case "2":
                    JArray roots = await CoolApkSDK.getShareListById(id, 1);
                    if (roots.Count != 0)
                    {
                        ObservableCollection<Feed> F = shareuserListView.ItemsSource as ObservableCollection<Feed>;
                        string d = F.First().GetValue("id");
                        for (int i = 0; i < roots.Count; i++)
                        {
                            if (d == ((JObject)roots[i]["id"]).ToString()) return;
                            F.Insert(i, new Feed((JObject)roots[i]));
                        }
                    }
                    break;
                default:
                    break;
            }
            mainPage.DeactiveProgressRing();

        }

        private async void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (!e.IsIntermediate)
                if (VScrollViewer.VerticalOffset == 0)
                {
                    Refresh();
                    VScrollViewer.ChangeView(null, 20, null);
                }
                else if (VScrollViewer.VerticalOffset == VScrollViewer.ScrollableHeight)
                {
                    mainPage.ActiveProgressRing();
                    switch (FeedDetailPivot.SelectedIndex.ToString())
                    {
                        case "0":
                            if (TitleTextBlock.Text == "动态")
                            {
                                JArray array = await CoolApkSDK.getFeedReplyListById(id, ++feedpage, 1, 0, feedfirstItem, feedlastItem);
                                if (array.Count != 0)
                                {
                                    feedlastItem = array.Last["id"].ToString();
                                    feeds.Add(new Feed2(array, string.Empty));
                                }
                                else
                                    feedpage--;
                            }
                            if (TitleTextBlock.Text == "回复")
                            {

                                JArray array = await CoolApkSDK.getReplyListById(id, ++feedpage, 0, 0, feedlastItem);
                                if (array.Count != 0)
                                {
                                    feedlastItem = array.Last["id"].ToString();
                                    feeds.Add(new Feed2(array, string.Empty));
                                }
                                else
                                    feedpage--;
                            }
                            break;
                        case "1":
                            JArray root = await CoolApkSDK.getFeedLikeUsersListById(id, ++likepage, likefirstItem, likelastItem);
                            if (root.Count != 0)
                            {
                                likelastItem = root.Last["id"].ToString();
                                ObservableCollection<Feed> F = likeListView.ItemsSource as ObservableCollection<Feed>;
                                foreach (JObject i in root)
                                    F.Add(new Feed(i));
                            }
                            else
                                likepage--;
                            break;
                        case "2":
                            JArray roots = await CoolApkSDK.getShareListById(id, ++sharepage);
                            if (roots.Count != 0)
                            {
                                ObservableCollection<Feed> F = shareuserListView.ItemsSource as ObservableCollection<Feed>;
                                foreach (JObject i in roots)
                                    F.Add(new Feed(i));
                            }
                            else
                                likepage--;
                            break;
                        default:
                            break;
                    }
                    mainPage.DeactiveProgressRing();
                }
        }

        private void ListViewItem_Tapped(object sender, TappedRoutedEventArgs e) =>
            Frame.Navigate(typeof(UserPage), new object[] { (sender as FrameworkElement).Tag as string, mainPage });

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Refresh();
            VScrollViewer.ChangeView(null, 20, null);
        }

        private void MarkdownTextBlock_Loaded(object sender, RoutedEventArgs e)
        {
            MarkdownTextBlock block = sender as MarkdownTextBlock;
            string s = block.Tag as string;
        }

        private async void MarkdownTextBlock_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            if (e.Link.IndexOf("/u/") == 0)
                mainPage.Frame.Navigate(typeof(UserPage), new object[] { await CoolApkSDK.GetUserIDByName(e.Link.Replace("/u/", string.Empty)), mainPage });
            if (e.Link.IndexOf("http") == 0)
                await Launcher.LaunchUriAsync(new Uri(e.Link));
        }

        //https://www.cnblogs.com/arcsinw/p/8638526.html
        private void GridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            list.Clear();
            GridView view = sender as GridView;
            if (view.Tag is string[])
                foreach (var s in view.Tag as string[])
                    list.Add(new BitmapImage(new Uri(s.Remove(s.Length - 6))));
            else if (view.Tag is string)
            {
                string s = view.Tag as string;
                if (string.IsNullOrWhiteSpace(s)) return;
                list.Add(new BitmapImage(new Uri(s)));
            }
            SFlipView.SelectedIndex = view.SelectedIndex;
            SFlipView.Visibility = CloseFlip.Visibility = Visibility.Visible;
            view.SelectedIndex = -1;
        }

        private void CloseFlip_Click(object sender, RoutedEventArgs e) => SFlipView.Visibility = CloseFlip.Visibility = Visibility.Collapsed;
    }

    public class TemplateSelector : DataTemplateSelector
    {
        public DataTemplate DataTemplate1 { get; set; }
        public DataTemplate DataTemplate2 { get; set; }
        protected override DataTemplate SelectTemplateCore(object item)
        {
            Feed feed = item as Feed;
            switch (feed.GetValue("entityType"))
            {
                case "feed":
                    return DataTemplate1;
                case "feed_reply":
                default:
                    return DataTemplate2;
            }
        }
    }
}