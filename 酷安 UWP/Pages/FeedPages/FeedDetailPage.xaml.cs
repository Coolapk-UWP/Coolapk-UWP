using CoolapkUWP.Control;
using CoolapkUWP.Control.ViewModels;
using CoolapkUWP.Data;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Windows.Data.Json;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace CoolapkUWP.Pages.FeedPages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class FeedDetailPage : Page, INotifyPropertyChanged
    {
        string id;
        FeedDetailViewModel feedDetail = null;
        FeedDetailViewModel FeedDetail
        {
            get => feedDetail;
            set
            {
                feedDetail = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FeedDetail"));
            }
        }
        ObservableCollection<Feed2> feeds = new ObservableCollection<Feed2>();
        ObservableCollection<FeedViewModel> answers = new ObservableCollection<FeedViewModel>();
        ObservableCollection<FeedReplyViewModel> replys = new ObservableCollection<FeedReplyViewModel>();

        int feedpage = 1;
        int likepage = 0;
        int sharepage = 0;
        int answerpage = 0;
        string feedfirstItem, feedlastItem;
        string likefirstItem, likelastItem;
        string answerfirstItem, answerlastItem;
        string answerSortType = "reply";
        string listType = "lastupdate_desc", isFromAuthor = "0";
        public event PropertyChangedEventHandler PropertyChanged;
        public FeedDetailPage() => this.InitializeComponent();

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (id != ((object[])e.Parameter)[0] as string)
            {
                Tools.rootPage.ShowProgressBar();
                id = ((object[])e.Parameter)[0] as string;
                TitleBar.Title = ((object[])e.Parameter)[1] as string;
                //if (TitleBar.Title == "回复")
                //{
                //    FeedDetailPivot.Visibility = Visibility.Visible;
                //    TitleBar.Visibility = Visibility.Collapsed;
                //    LoadRepliesDetail(id);
                //}
                //else 
                LoadFeedDetail();
            }
        }

        public async void LoadFeedDetail()
        {
            string result = await Tools.GetJson("/feed/detail?id=" + id);
            JsonObject detail = Tools.GetJSonObject(result);
            if (detail != null)
            {
                FeedDetail = new FeedDetailViewModel(detail);
                TitleBar.Title = FeedDetail.title;
                if (FeedDetail.isQuestionFeed)
                {
                    FindName("AnswerList");
                    AnswerList.ItemsSource = answers;
                    string r = await Tools.GetJson($"/question/answerList?id={id}&sort={answerSortType}&page={++answerpage}");
                    JsonArray array = Tools.GetDataArray(r);
                    if (!(array is null) && array.Count != 0)
                    {
                        foreach (var item in array)
                            answers.Add(new FeedViewModel(item, FeedDisplayMode.notShowMessageTitle));
                        answerfirstItem = array.First().GetObject()["id"].ToString();
                        answerlastItem = array.Last().GetObject()["id"].ToString();
                    }
                    else answerpage--;
                }
                else
                {
                    FindName("FeedDetailPivot");
                    string r = await Tools.GetJson($"/feed/replyList?id={id}&listType={listType}&page={1}&discussMode=1&feedType=feed&blockStatus=0&fromFeedAuthor={isFromAuthor}");
                    JsonArray array = Tools.GetDataArray(r);
                    ChangeModeButton.IsEnabled = true;
                    if (array.Count != 0)
                    {
                        feedfirstItem = array.First().GetObject()["id"].ToString();
                        feedlastItem = array.Last().GetObject()["id"].ToString();
                        feeds.Add(new Feed2(detail["hotReplyRows"], "热门回复"));
                        feeds.Add(new Feed2(array, "最新回复"));
                    }
                    else feedpage--;
                }
            }
            Tools.rootPage.HideProgressBar();
        }

        //public async void LoadRepliesDetail(string id)
        //{
        //    JsonArray array = await Tools.GetReplyListById(id, "1", string.Empty);
        //    FeedDetailList.ItemsSource = new Feed[] { reply };
        //    FeedDetailPivot.DataContext = new { replynum = reply.GetValue("replynum").ToString(), likenum = string.Empty, forwardnum = string.Empty };
        //    if (array.Count != 0)
        //    {
        //        feedlastItem = array.Last().GetObject()["id"].ToString();
        //        replys.Add(new Feed2(array, ""));
        //    }
        //    else feedpage--;
        //    Tools.rootPage.HideProgressBar();
        //}

        private void Button_Click(object sender, RoutedEventArgs e) => Tools.OpenLink((sender as Button).Tag as string);

        private void BackButton_Click(object sender, RoutedEventArgs e) => Frame.GoBack();

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Refresh();
            VScrollViewer.ChangeView(null, 0, null);
        }

        private void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //if (TitleBar.Title != "回复")
            //{
            //    if ((sender as FrameworkElement).Tag is Feed f)
            //    {
            //        if (f.GetValue("infoHtml") != "回复")
            //            Tools.rootPage.Navigate(typeof(FeedDetailPage), new object[] { f.GetValue("id"), Tools.rootPage, string.Empty, null });
            //        else
            //        {
            //            ContentDialog1 contentDialog = new ContentDialog1
            //            {
            //                RequestedTheme = Settings.GetBoolen("IsDarkMode") ? ElementTheme.Dark : ElementTheme.Light
            //            };
            //            contentDialog.Navigate(typeof(FeedDetailPage),
            //                new object[] { f.GetValue("id"), Tools.rootPage, "回复", (sender as FrameworkElement).Tag });
            //            await contentDialog.ShowAsync();
            //        }
            //    }
            //    else 
            //    if ((sender as FrameworkElement).Tag is string s)
            //        Tools.OpenLink(s);
            //}
        }

        async void Refresh()
        {
            Tools.rootPage.ShowProgressBar();
            if (FeedDetailPivot != null)
            {
                switch (FeedDetailPivot.SelectedIndex.ToString())
                {
                    case "0":
                        //if (TitleBar.Title == "回复")
                        //{
                        //    JsonArray array = await Tools.GetReplyListById(id, "1", feedlastItem);
                        //    if (array.Count != 0)
                        //        feeds.Insert(0, new Feed2(array, $"第1页"));
                        //}
                        //else
                        {
                            string res = await Tools.GetJson("/feed/detail?id=" + id);
                            JsonObject detail = Tools.GetJSonObject(res);
                            if (detail != null)
                            {
                                FeedDetail = new FeedDetailViewModel(detail);
                                string re = await Tools.GetJson($"/feed/replyList?id={id}&listType={listType}&page={1}&firstItem={feedfirstItem}&lastItem={feedlastItem}&discussMode=1&feedType=feed&blockStatus=0&fromFeedAuthor={isFromAuthor}");
                                JsonArray array = Tools.GetDataArray(re);
                                //if (feeds.Count > 0) feeds.RemoveAt(0);
                                //feeds.Insert(0, new Feed2(detail["hotReplyRows"], "热门回复"));
                                if (array.Count != 0)
                                {
                                    feedfirstItem = array.First().GetObject()["id"].ToString();
                                    if (string.IsNullOrEmpty(feedlastItem)) feedlastItem = array.Last().GetObject()["id"].ToString();
                                    feeds.Insert(0, new Feed2(array, $"第{feedpage}页"));
                                }
                                else feedpage--;
                            }
                        }
                        break;
                    case "1":
                        string result = await Tools.GetJson($"/feed/likeList?id={id}&listType=lastupdate_desc&page={++likepage}&firstItem={likefirstItem}&lastItem={likelastItem}");
                        JsonArray root = Tools.GetDataArray(result);
                        if (root.Count != 0)
                        {
                            likefirstItem = root.First().GetObject()["uid"].ToString();
                            ObservableCollection<User> F = likeListView.ItemsSource as ObservableCollection<User>;
                            foreach (IJsonValue i in root)
                            {
                                JsonObject o = i.GetObject();
                                F.Add(new User
                                {
                                    Url = o["url"].GetString(),
                                    UserAvatar = new BitmapImage(new Uri(o["userSmallAvatar"].GetString())),
                                    UserName = o["username"].GetString()
                                });
                            }
                        }
                        break;
                    case "2":
                        string r = await Tools.GetJson($"/feed/forwardList?id={id}&type=feed&page={++sharepage}");
                        JsonArray roots = Tools.GetDataArray(r);
                        if (roots.Count != 0)
                        {
                            ObservableCollection<SourceFeedViewModel> F = shareuserListView.ItemsSource as ObservableCollection<SourceFeedViewModel>;
                            for (int i = 0; i < roots.Count; i++)
                            {
                                if (F.First().url == roots[i].GetObject()["url"].GetNumber().ToString()) return;
                                F.Insert(i, new SourceFeedViewModel(roots[i]));
                            }
                        }
                        break;
                }
            }
            else if (AnswerList != null)
            {
                string re = await Tools.GetJson($"/question/answerList?id={id}&sort={answerSortType}&page={++answerpage}&firstItem={answerfirstItem}&lastItem={answerlastItem}");
                JsonArray array = Tools.GetDataArray(re);
                if (array != null && array.Count != 0)
                {
                    var d = (from a in answers
                             from b in array
                             where a.url == b.GetObject()["url"].GetString()
                             select a).ToArray();
                    foreach (var item in d) answers.Remove(item);
                    for (int i = 0; i < array.Count; i++)
                        answers.Insert(i, new FeedViewModel(array[i], FeedDisplayMode.notShowMessageTitle));
                    answerfirstItem = array.First().GetObject()["id"].ToString();
                }
            }
            else LoadFeedDetail();
            Tools.rootPage.HideProgressBar();
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
                    Tools.rootPage.ShowProgressBar();
                    if (FeedDetailPivot != null)
                    {
                        switch (FeedDetailPivot.SelectedIndex.ToString())
                        {
                            case "0":
                                //if (TitleBar.Title == "回复")
                                //{
                                //    JsonArray array = await Tools.GetReplyListById(id, $"{++feedpage}", feedlastItem);
                                //    if (array.Count != 0)
                                //    {
                                //        feedlastItem = array.Last().GetObject()["id"].ToString();
                                //        replys.Add(new Feed2(array, string.Empty));
                                //    }
                                //    else feedpage--;
                                //}
                                //else 
                                {
                                    string re = await Tools.GetJson($"/feed/replyList?id={id}&listType={listType}&page={1}&firstItem={feedfirstItem}&lastItem={feedlastItem}&discussMode=1&feedType=feed&blockStatus=0&fromFeedAuthor={isFromAuthor}");
                                    JsonArray array = Tools.GetDataArray(re);
                                    if (array.Count != 0)
                                    {
                                        feedlastItem = array.Last().GetObject()["id"].ToString();
                                        feeds.Add(new Feed2(array, $"第{feedpage}页"));
                                    }
                                    else
                                        feedpage--;
                                }
                                break;
                            case "1":
                                string result = await Tools.GetJson($"/feed/likeList?id={id}&listType=lastupdate_desc&page={++likepage}&firstItem={likefirstItem}&lastItem={likelastItem}");
                                JsonArray root = Tools.GetDataArray(result);
                                if (root.Count != 0)
                                {
                                    likelastItem = root.Last().GetObject()["uid"].ToString();
                                    ObservableCollection<User> F = likeListView.ItemsSource as ObservableCollection<User>;
                                    foreach (IJsonValue i in root)
                                    {
                                        JsonObject o = i.GetObject();
                                        F.Add(new User
                                        {
                                            Url = o["url"].GetString(),
                                            UserAvatar = new BitmapImage(new Uri(o["userSmallAvatar"].GetString())),
                                            UserName = o["username"].GetString()
                                        });
                                    }
                                }
                                else
                                    likepage--;
                                break;
                            case "2":
                                string r = await Tools.GetJson($"/feed/forwardList?id={id}&type=feed&page={++sharepage}");
                                JsonArray roots = Tools.GetDataArray(r);
                                if (roots.Count != 0)
                                {
                                    ObservableCollection<SourceFeedViewModel> F = shareuserListView.ItemsSource as ObservableCollection<SourceFeedViewModel>;
                                    foreach (var i in roots)
                                        F.Add(new SourceFeedViewModel(i));
                                }
                                else sharepage--;
                                break;
                        }
                    }
                    else if (AnswerList != null)
                    {
                        string re = await Tools.GetJson($"/question/answerList?id={id}&sort={answerSortType}&page={++answerpage}&firstItem={answerfirstItem}&lastItem={answerlastItem}");
                        JsonArray array = Tools.GetDataArray(re);
                        if (!(array is null) && array.Count != 0)
                        {
                            foreach (var item in array)
                                answers.Add(new FeedViewModel(item, FeedDisplayMode.notShowMessageTitle));
                            answerlastItem = array.Last().GetObject()["id"].ToString();
                        }
                        else answerpage--;
                    }
                    Tools.rootPage.HideProgressBar();
                }
            }
            else refreshText.Visibility = Visibility.Visible;
        }

        private void ListViewItem_Tapped(object sender, TappedRoutedEventArgs e) =>
            Frame.Navigate(typeof(UserPage), (sender as FrameworkElement).Tag as string);

        private void MarkdownTextBlock_LinkClicked(object sender, Microsoft.Toolkit.Uwp.UI.Controls.LinkClickedEventArgs e) => Tools.OpenLink(e.Link);

        private void StackPanel_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            if (element.Tag is null) return;
            else if (element.Tag is string s) Tools.OpenLink(s);
            else if (element.Tag is Feed f) Tools.OpenLink(f.GetValue("url"));
        }

        private void MarkdownTextBlock_ImageResolving(object sender, Microsoft.Toolkit.Uwp.UI.Controls.ImageResolvingEventArgs e)
        {
            if (e.Url.IndexOf("ms-appx") != 0)
                if (Settings.GetBoolen("IsNoPicsMode"))
                {
                    e.Handled = true;
                    if (Settings.GetBoolen("IsDarkMode"))
                        e.Image = new BitmapImage(new Uri("ms-appx:/Assets/img_placeholder_night.png")) { DecodePixelHeight = 150, DecodePixelWidth = 150 };
                    else e.Image = new BitmapImage(new Uri("ms-appx:/Assets/img_placeholder.png")) { DecodePixelHeight = 150, DecodePixelWidth = 150 };
                }
        }

        private void MarkdownTextBlock_ImageClicked(object sender, Microsoft.Toolkit.Uwp.UI.Controls.LinkClickedEventArgs e)
        {
            if (e.Link.IndexOf("http") == 0) Tools.rootPage.ShowImage(e.Link.Remove(e.Link.Length - 6));
        }

        private void Image_Tapped(object sender, TappedRoutedEventArgs e) => Tools.rootPage.ShowImage((sender as FrameworkElement).Tag as string);

        private async void FeedDetailPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Pivot item = sender as Pivot;
            if (FeedDetailPivot != null)
            {
                switch (item.SelectedIndex)
                {
                    case 0:
                        replyListView.ItemsSource =
                            //TitleBar.Title == "回复" ? replys : 
                            feeds;
                        if (TitleBar.Title == "回复")
                            FeedDetailPivot.IsLocked = true;
                        break;
                    case 1:
                        string result = await Tools.GetJson($"/feed/likeList?id={id}&listType=lastupdate_desc&page={++likepage}");
                        JsonArray root = Tools.GetDataArray(result);
                        ObservableCollection<User> F = new ObservableCollection<User>();
                        if (root.Count != 0)
                        {
                            likefirstItem = root.First().GetObject()["uid"].ToString();
                            likelastItem = root.Last().GetObject()["uid"].ToString();
                            foreach (IJsonValue i in root)
                            {
                                JsonObject o = i.GetObject();
                                F.Add(new User
                                {
                                    Url = o["url"].GetString(),
                                    UserAvatar = new BitmapImage(new Uri(o["userSmallAvatar"].GetString())),
                                    UserName = o["username"].GetString()
                                });
                            }
                        }
                        else likepage--;
                        likeListView.ItemsSource = F;
                        break;
                    case 2:
                        string r = await Tools.GetJson($"/feed/forwardList?id={id}&type=feed&page={++sharepage}");
                        JsonArray roots = Tools.GetDataArray(r);
                        ObservableCollection<SourceFeedViewModel> Fs = shareuserListView.ItemsSource as ObservableCollection<SourceFeedViewModel>;
                        if (roots.Count != 0)
                            foreach (var i in roots)
                                Fs.Add(new SourceFeedViewModel(i));
                        else sharepage--;
                        shareuserListView.ItemsSource = Fs;
                        break;
                }
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
                    Tools.rootPage.ShowImage(ss[view.SelectedIndex].Remove(ss[view.SelectedIndex].Length - 6));
                else if (view.Tag is string s)
                {
                    if (string.IsNullOrWhiteSpace(s)) return;
                    Tools.rootPage.ShowImage(s);
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
    class User
    {
        public ImageSource UserAvatar { get; set; }
        public string UserName { get; set; }
        public string Url { get; set; }
    }
}