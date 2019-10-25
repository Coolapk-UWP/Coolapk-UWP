using Microsoft.Toolkit.Uwp.UI.Controls;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
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
        ObservableCollection<Feed> answers = new ObservableCollection<Feed>();
        ObservableCollection<Feed2> replys = new ObservableCollection<Feed2>();
        Feed2 reply;
        int feedpage = 1;
        int likepage = 0;
        int sharepage = 0;
        int answerpage = 0;
        string feedfirstItem, feedlastItem;
        string likefirstItem, likelastItem;
        string answerfirstItem, answerlastItem;
        string answerSortType = "reply";
        string listType = "lastupdate_desc", isFromAuthor = "0";
        public Style listviewStyle
        {
            get
            {
                if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile") return Application.Current.Resources["ListViewStyle2Mobile"] as Style;
                else return Application.Current.Resources["ListViewStyle2Desktop"] as Style;
            }
        }

        //static ObservableCollection<ImageSource> list = new ObservableCollection<ImageSource>();
        public FeedDetailPage()
        {
            this.InitializeComponent();
            //SFlipView.ItemsSource = list;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            //将传过来的数据 类型转换一下
            mainPage = ((object[])e.Parameter)[1] as MainPage;
            string title = (string)((object[])e.Parameter)[2];
            string id = (string)((object[])e.Parameter)[0];
            if (this.id != id)
            {
                mainPage.ActiveProgressRing();
                this.id = id;
                if (title == "回复")
                {
                    FeedDetailPivot.Visibility = Visibility.Visible;
                    TitleTextBlock.Text = title;
                    TitleBar.Visibility = Visibility.Collapsed;
                    reply = ((object[])e.Parameter)[3] as Feed2;
                    LoadRepliesDetail(id);
                }
                else LoadFeedDetail(id);
            }
        }

        public async void LoadFeedDetail(string id)
        {
            JObject detail = await Tools.GetFeedDetailById(id);
            FeedDetailList.ItemsSource = new Feed[] { new Feed(detail) };
            TitleTextBlock.Text = detail["title"].ToString();
            if (detail["feedTypeName"].ToString() == "提问")
            {
                AnswerList.Visibility = Visibility.Visible;
                AnswerList.ItemsSource = answers;
                JArray array = await Tools.GetAnswerListById(id, answerSortType, $"{++answerpage}", answerfirstItem, answerlastItem);
                if (!(array is null) && array.Count != 0)
                {
                    foreach (JObject item in array)
                        answers.Add(new Feed(item));
                    answerfirstItem = array.First["id"].ToString();
                    answerlastItem = array.Last["id"].ToString();
                }
                else answerpage--;
            }
            else
            {
                FeedDetailPivot.DataContext = new
                {
                    replynum = detail.GetValue("replynum").ToString(),
                    likenum = detail.GetValue("likenum").ToString(),
                    forwardnum = detail.GetValue("forwardnum").ToString()
                };
                FeedDetailPivot.Visibility = Visibility.Visible;
                JArray array = await Tools.GetFeedReplyListById(id, listType, "1", isFromAuthor, string.Empty, string.Empty);
                ChangeModeButton.IsEnabled = true;
                if (array.Count != 0)
                {
                    feedfirstItem = array.First["id"].ToString();
                    feedlastItem = array.Last["id"].ToString();
                    feeds.Add(new Feed2(detail["hotReplyRows"], "热门回复"));
                    feeds.Add(new Feed2(array, "最新回复"));
                }
                else feedpage--;
            }
            mainPage.DeactiveProgressRing();
        }

        public async void LoadRepliesDetail(string id)
        {
            JArray array = await Tools.GetReplyListById(id, "1", string.Empty);
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
        private void Button_Click(object sender, RoutedEventArgs e) => mainPage.Frame.Navigate(typeof(UserPage), new object[] { (sender as Button).Tag as string, mainPage });

        private void BackButton_Click(object sender, RoutedEventArgs e) => Frame.GoBack();

        private async void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (TitleTextBlock.Text != "回复")
            {
                if ((sender as FrameworkElement).Tag is Feed f)
                {
                    if (f.GetValue("infoHtml") != "回复")
                        mainPage.Frame.Navigate(typeof(FeedDetailPage), new object[] { f.GetValue("id"), mainPage, string.Empty, null });
                    else
                    {
                        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
                        ContentDialog1 contentDialog = new ContentDialog1
                        {
                            RequestedTheme = Convert.ToBoolean(localSettings.Values["IsDarkMode"]) ? ElementTheme.Dark : ElementTheme.Light
                        };
                        contentDialog.Navigate(typeof(FeedDetailPage),
                            new object[] { f.GetValue("id"), mainPage, "回复", (sender as FrameworkElement).Tag });
                        await contentDialog.ShowAsync();
                    }
                }
                else if ((sender as FrameworkElement).Tag is Feed[] fs)
                    mainPage.Frame.Navigate(typeof(FeedDetailPage), new object[] { fs[0].GetValue("id"), mainPage, string.Empty, null });
            }
        }

        async void Refresh()
        {
            mainPage.ActiveProgressRing();
            switch (FeedDetailPivot.SelectedIndex.ToString())
            {
                case "0":
                    if (TitleTextBlock.Text == "回复")
                    {
                        JArray array = await Tools.GetReplyListById(id, "1", feedlastItem);
                        if (array.Count != 0)
                            feeds.Insert(0, new Feed2(array, string.Empty));
                    }
                    else if (FeedDetailPivot.Visibility == Visibility.Collapsed)
                    {
                        JArray array = await Tools.GetAnswerListById(id, answerSortType, "1", answerfirstItem, answerlastItem);
                        if (!(array is null) && array.Count != 0)
                        {
                            for (int i = 0; i < array.Count; i++)
                                for (int j = 0; j < answers.Count; j++)
                                    if (((JObject)array[i])["id"].ToString() == answers[j].GetValue("id"))
                                        answers.RemoveAt(j);
                            for (int i = 0; i < array.Count; i++)
                                answers.Insert(i, new Feed((JObject)array[i]));
                            answerfirstItem = array.First["id"].ToString();
                        }
                    }
                    else
                    {
                        JObject detail = await Tools.GetFeedDetailById(id);
                        JArray array = await Tools.GetFeedReplyListById(id, listType, "1", isFromAuthor, feedfirstItem, feedlastItem);
                        FeedDetailList.ItemsSource = new Feed[] { new Feed(detail) };
                        FeedDetailPivot.DataContext = new
                        {
                            replynum = detail.GetValue("replynum").ToString(),
                            likenum = detail.GetValue("likenum").ToString(),
                            forwardnum = detail.GetValue("forwardnum").ToString()
                        };
                        //if (feeds.Count > 0) feeds.RemoveAt(0);
                        //feeds.Insert(0, new Feed2(detail["hotReplyRows"], "热门回复"));
                        if (array.Count != 0)
                        {
                            feedfirstItem = array.First["id"].ToString();
                            if (string.IsNullOrEmpty(feedlastItem)) feedlastItem = array.Last["id"].ToString();
                            feeds.Insert(0, new Feed2(array, string.Empty));
                        }
                        else feedpage--;
                        mainPage.DeactiveProgressRing();
                    }

                    break;
                case "1":
                    JArray root = await Tools.GetFeedLikeUsersListById(id, $"{++likepage}", likefirstItem, likelastItem);
                    if (root.Count != 0)
                    {
                        likefirstItem = root.First["uid"].ToString();
                        ObservableCollection<Feed> F = likeListView.ItemsSource as ObservableCollection<Feed>;
                        for (int i = 0; i < root.Count; i++)
                            F.Insert(i, new Feed((JObject)root[i]));
                    }
                    break;
                case "2":
                    JArray roots = await Tools.GetForwardListById(id, $"{++sharepage}");
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
            {
                if (VScrollViewer.VerticalOffset == 0)
                {
                    Refresh();
                    VScrollViewer.ChangeView(null, 20, null);
                    refreshText.Visibility = Visibility.Collapsed;
                }
                else if (VScrollViewer.VerticalOffset == VScrollViewer.ScrollableHeight)
                {
                    mainPage.ActiveProgressRing();
                    switch (FeedDetailPivot.SelectedIndex.ToString())
                    {
                        case "0":
                            if (TitleTextBlock.Text == "回复")
                            {
                                JArray array = await Tools.GetReplyListById(id, $"{++feedpage}", feedlastItem);
                                if (array.Count != 0)
                                {
                                    feedlastItem = array.Last["id"].ToString();
                                    replys.Add(new Feed2(array, string.Empty));
                                }
                                else feedpage--;
                            }
                            else if (FeedDetailPivot.Visibility == Visibility.Collapsed)
                            {
                                JArray array = await Tools.GetAnswerListById(id, answerSortType, $"{++answerpage}", answerfirstItem, answerlastItem);
                                if (!(array is null) && array.Count != 0)
                                {
                                    foreach (JObject item in array)
                                        answers.Add(new Feed(item));
                                    answerlastItem = array.Last["id"].ToString();
                                }
                                else answerpage--;
                            }
                            else
                            {
                                JArray array = await Tools.GetFeedReplyListById(id, listType, $"{++feedpage}", isFromAuthor, feedfirstItem, feedlastItem);
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
                            JArray root = await Tools.GetFeedLikeUsersListById(id, $"{++likepage}", likefirstItem, likelastItem);
                            if (root.Count != 0)
                            {
                                likelastItem = root.Last["uid"].ToString();
                                ObservableCollection<Feed> F = likeListView.ItemsSource as ObservableCollection<Feed>;
                                foreach (JObject i in root)
                                    F.Add(new Feed(i));
                            }
                            else
                                likepage--;
                            break;
                        case "2":
                            JArray roots = await Tools.GetForwardListById(id, $"{++sharepage}");
                            if (roots.Count != 0)
                            {
                                ObservableCollection<Feed> F = shareuserListView.ItemsSource as ObservableCollection<Feed>;
                                foreach (JObject i in roots)
                                    F.Add(new Feed(i));
                            }
                            else
                                sharepage--;
                            break;
                    }
                    mainPage.DeactiveProgressRing();
                }
            }
            else refreshText.Visibility = Visibility.Visible;
        }

        private void ListViewItem_Tapped(object sender, TappedRoutedEventArgs e) =>
            Frame.Navigate(typeof(UserPage), new object[] { (sender as FrameworkElement).Tag as string, mainPage });

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Refresh();
            VScrollViewer.ChangeView(null, 0, null);
        }

        private void MarkdownTextBlock_LinkClicked(object sender, LinkClickedEventArgs e) => Tools.OpenLink(e.Link, mainPage);

        private void StackPanel_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            if (element.Tag is null) return;
            else if (element.Tag is string s) Tools.OpenLink(s, mainPage);
            else if (element.Tag is Feed f) Tools.OpenLink(f.GetValue("url"), mainPage);
        }

        private void MarkdownTextBlock_ImageResolving(object sender, ImageResolvingEventArgs e)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            if (e.Url.IndexOf("ms-appx") != 0)
                if (Convert.ToBoolean(localSettings.Values["IsNoPicsMode"]))
                {
                    e.Handled = true;
                    if (Convert.ToBoolean(localSettings.Values["IsDarkMode"]))
                        e.Image = new BitmapImage(new Uri("ms-appx:/Assets/img_placeholder_night.png")) { DecodePixelHeight = 150, DecodePixelWidth = 150 };
                    else e.Image = new BitmapImage(new Uri("ms-appx:/Assets/img_placeholder.png")) { DecodePixelHeight = 150, DecodePixelWidth = 150 };
                }
        }

        private void MarkdownTextBlock_ImageClicked(object sender, LinkClickedEventArgs e)
        {
            if (e.Link.IndexOf("http") == 0) ShowImageControl.ShowImage(e.Link.Remove(e.Link.Length - 6));

            /*
            if (e.Link.IndexOf("http") == 0)
            {
                list.Clear();
                list.Add(new BitmapImage(new Uri(e.Link.Remove(e.Link.Length - 6))));
                SFlipView.Visibility = CloseFlip.Visibility = Visibility.Visible;
            }*/
        }

        private void Image_Tapped(object sender, TappedRoutedEventArgs e) => ShowImageControl.ShowImage((sender as FrameworkElement).Tag as string);

        private void ListViewItem_Tapped_1(object sender, TappedRoutedEventArgs e)
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

        private async void FeedDetailPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Pivot item = sender as Pivot;
            switch (item.SelectedIndex)
            {
                case 0:
                    replyListView.ItemsSource = TitleTextBlock.Text == "回复" ? replys : feeds;
                    if (TitleTextBlock.Text == "回复")
                        FeedDetailPivot.IsLocked = true;
                    break;
                case 1:
                    JArray root = await Tools.GetFeedLikeUsersListById(id, $"{++likepage} ", string.Empty, string.Empty);
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
                case 2:
                    JArray roots = await Tools.GetForwardListById(id, $"{++sharepage}");
                    ObservableCollection<Feed> Fs = new ObservableCollection<Feed>();
                    if (roots.Count != 0)
                        foreach (JObject i in roots)
                            Fs.Add(new Feed(i));
                    else sharepage--;
                    shareuserListView.ItemsSource = Fs;
                    break;
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox box = sender as ComboBox;
            switch (box.SelectedIndex)
            {
                case 0:
                    answerSortType = "reply";
                    break;
                case 1:
                    answerSortType = "like";
                    break;
                case 2:
                    answerSortType = "dateline";
                    break;
            }
            answers.Clear();
            answerfirstItem = answerlastItem = string.Empty;
            answerpage = 1;
            Refresh();
        }

        private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            foreach (MenuFlyoutItem i in (ChangeModeButton.Flyout as MenuFlyout).Items)
                i.Icon = null;
            MenuFlyoutItem item = sender as MenuFlyoutItem;
            item.Icon = new SymbolIcon(Symbol.Accept);
            switch (item.Tag as string)
            {
                case "0":
                    listType = "lastupdate_desc";
                    isFromAuthor = "0";
                    break;
                case "1":
                    listType = "dateline_desc";
                    isFromAuthor = "0";
                    break;
                case "2":
                    listType = "popular";
                    isFromAuthor = "0";
                    break;
                case "3":
                    listType = string.Empty;
                    isFromAuthor = "1";
                    break;
            }
            feedpage = 1;
            feedfirstItem = feedlastItem = string.Empty;
            feeds.Clear();
            Refresh();
        }

        //https://www.cnblogs.com/arcsinw/p/8638526.html
        private void GridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GridView view = sender as GridView;
            if (view.SelectedIndex > -1)
            {
                if (view.Tag is string[] ss)
                    ShowImageControl.ShowImage(ss[view.SelectedIndex].Remove(ss[view.SelectedIndex].Length - 6));
                else if (view.Tag is string s)
                {
                    if (string.IsNullOrWhiteSpace(s)) return;
                    ShowImageControl.ShowImage(s);
                }
            }
            view.SelectedIndex = -1;
            /*
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
            view.SelectedIndex = -1;*/
        }

        //        private void CloseFlip_Click(object sender, RoutedEventArgs e) => SFlipView.Visibility = CloseFlip.Visibility = Visibility.Collapsed;
    }

    public class TemplateSelector : DataTemplateSelector
    {
        public DataTemplate DataTemplate1 { get; set; }
        public DataTemplate DataTemplate2 { get; set; }
        public DataTemplate DataTemplate3 { get; set; }
        public DataTemplate DataTemplate4 { get; set; }
        protected override DataTemplate SelectTemplateCore(object item)
        {
            Feed feed = item as Feed;
            if (feed.GetValue("entityType") == "feed_reply") return DataTemplate2;
            switch (feed.GetValue("feedType"))
            {
                case "feed": return DataTemplate1;
                case "feedArticle": return DataTemplate3;
                case "question": return DataTemplate4;
                default: return DataTemplate1;
            }
        }
    }
}