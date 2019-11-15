using CoolapkUWP.Control;
using CoolapkUWP.Control.ViewModels;
using CoolapkUWP.Data;
using System;
using System.Collections.Generic;
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
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FeedDetail)));
            }
        }
        ObservableCollection<FeedReplyViewModel> hotReplys = new ObservableCollection<FeedReplyViewModel>();
        ObservableCollection<FeedReplyViewModel> replys = new ObservableCollection<FeedReplyViewModel>();
        ObservableCollection<FeedViewModel> answers = new ObservableCollection<FeedViewModel>();

        int feedPage, likePage, sharePage, answerPage, hotReplyPage;
        double feedFirstItem, feedLastItem, likeFirstItem, likeLastItem, answerFirstItem, answerLastItem, hotReplyFirstItem, hotReplyLastItem;
        string answerSortType = "reply";
        string listType = "lastupdate_desc", isFromAuthor = "0";
        public event PropertyChangedEventHandler PropertyChanged;
        public FeedDetailPage() => this.InitializeComponent();

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (id != e.Parameter as string)
            {
                Tools.ShowProgressBar();
                id = e.Parameter as string;
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
                    if (answerPage != 0 || answerLastItem != 0) return;
                    FindName("AnswerList");
                    AnswerList.ItemsSource = answers;
                    string r = await Tools.GetJson($"/question/answerList?id={id}&sort={answerSortType}&page={++answerPage}");
                    JsonArray array = Tools.GetDataArray(r);
                    if (!(array is null) && array.Count != 0)
                    {
                        foreach (var item in array)
                            answers.Add(new FeedViewModel(item, FeedDisplayMode.notShowMessageTitle));
                        answerFirstItem = array.First().GetObject()["id"].GetNumber();
                        answerLastItem = array.Last().GetObject()["id"].GetNumber();
                    }
                    else answerPage--;
                }
                else
                {
                    FindName("FeedDetailPivot");
                    if (feedArticleTitle != null)
                        feedArticleTitle.Height = feedArticleTitle.Width * 0.44;
                    string r = await Tools.GetJson($"/feed/replyList?id={id}&listType={listType}&page={++feedPage}&discussMode=1&feedType=feed&blockStatus=0&fromFeedAuthor={isFromAuthor}");
                    JsonArray array = Tools.GetDataArray(r);
                    if (array.Count != 0)
                    {
                        feedFirstItem = array.First().GetObject()["id"].GetNumber();
                        feedLastItem = array.Last().GetObject()["id"].GetNumber();
                        hotReplyListView.ItemsSource = hotReplys;
                        replyListView.ItemsSource = replys;
                        JsonArray values = detail["hotReplyRows"].GetArray();
                        foreach (var item in values)
                            hotReplys.Add(new FeedReplyViewModel(item));
                        foreach (var item in array)
                            replys.Add(new FeedReplyViewModel(item));
                    }
                    else feedPage--;
                }
            }
            Tools.HideProgressBar();
        }

        private void Button_Click(object sender, RoutedEventArgs e) => Tools.OpenLink((sender as Button).Tag as string);

        private void BackButton_Click(object sender, RoutedEventArgs e) => Frame.GoBack();

        private void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if ((sender as FrameworkElement).Tag is string s)
                Tools.OpenLink(s);
        }

        async void Refresh()
        {
            Tools.ShowProgressBar();
            if (FeedDetailPivot != null)
            {
                switch (FeedDetailPivot.SelectedIndex)
                {
                    case 0:
                        string res = await Tools.GetJson("/feed/detail?id=" + id);
                        JsonObject detail = Tools.GetJSonObject(res);
                        if (detail != null)
                        {
                            FeedDetail = new FeedDetailViewModel(detail);
                            hotReplys.Clear();
                            JsonArray values = detail["hotReplyRows"].GetArray();
                            foreach (var item in values)
                                hotReplys.Add(new FeedReplyViewModel(item));
                            if (feedPage == 0) feedPage = 1;
                            string re = await Tools.GetJson($"/feed/replyList?id={id}&listType={listType}&page={1}&discussMode=1&feedType=feed&blockStatus=0&fromFeedAuthor={isFromAuthor}");
                            JsonArray array = Tools.GetDataArray(re);
                            if (array.Count != 0)
                            {
                                feedFirstItem = array.First().GetObject()["id"].GetNumber();
                                if (feedLastItem == 0) feedLastItem = array.Last().GetObject()["id"].GetNumber();
                                var d = (from a in replys
                                         from b in array
                                         where a.id == b.GetObject()["id"].GetNumber()
                                         select a).ToArray();
                                foreach (var item in d)
                                    replys.Remove(item);
                                foreach (var item in array)
                                    replys.Add(new FeedReplyViewModel(item));
                            }
                            else feedPage--;
                        }
                        break;
                    case 1:
                        string result = await Tools.GetJson($"/feed/likeList?id={id}&listType=lastupdate_desc&page={++likePage}&firstItem={likeFirstItem}&lastItem={likeLastItem}");
                        JsonArray root = Tools.GetDataArray(result);
                        if (root.Count != 0)
                        {
                            likeFirstItem = root.First().GetObject()["uid"].GetNumber();
                            ObservableCollection<UserViewModel> F = likeListView.ItemsSource as ObservableCollection<UserViewModel>;
                            foreach (IJsonValue i in root)
                                F.Add(new UserViewModel(i));
                        }
                        break;
                    case 2:
                        string r = await Tools.GetJson($"/feed/forwardList?id={id}&type=feed&page={++sharePage}");
                        JsonArray roots = Tools.GetDataArray(r);
                        if (roots.Count != 0)
                        {
                            ObservableCollection<SourceFeedViewModel> F = shareuserListView.ItemsSource as ObservableCollection<SourceFeedViewModel>;
                            for (int i = 0; i < roots.Count; i++)
                            {
                                if (F.First()?.url == roots[i].GetObject()["url"].GetString()) return;
                                F.Insert(i, new SourceFeedViewModel(roots[i]));
                            }
                        }
                        break;
                }
            }
            else if (AnswerList != null)
            {
                if (answerLastItem != 0) return;
                string re = await Tools.GetJson($"/question/answerList?id={id}&sort={answerSortType}&page={++answerPage}");
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
                    answerFirstItem = array.First().GetObject()["id"].GetNumber();
                    if (answerPage == 1)
                        answerLastItem = array.Last().GetObject()["id"].GetNumber();
                }
                else answerPage--;
            }
            else LoadFeedDetail();
            VScrollViewer.ChangeView(null, 20, null);
            Tools.HideProgressBar();
        }

        private void ChangeHotReplysDisplayModeListViewItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (iconText.Text == "")//(Symbol)0xE70E)
            {
                hotReplyListView.Visibility = Visibility.Collapsed;
                iconText.Text = "";//(Symbol)0xE70D;
            }
            else
            {
                hotReplyListView.Visibility = Visibility.Visible;
                iconText.Text = "";//(Symbol)0xE70E;
            }
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
                    Tools.ShowProgressBar();
                    if (FeedDetailPivot != null)
                    {
                        switch (FeedDetailPivot.SelectedIndex)
                        {
                            case 0:
                                string re = await Tools.GetJson($"/feed/replyList?id={id}&listType={listType}&page={++feedPage}&firstItem={feedFirstItem}&lastItem={feedLastItem}&discussMode=1&feedType=feed&blockStatus=0&fromFeedAuthor={isFromAuthor}");
                                JsonArray array = Tools.GetDataArray(re);
                                if (array.Count != 0)
                                {
                                    feedLastItem = array.Last().GetObject()["id"].GetNumber();
                                    foreach (var item in array)
                                        replys.Add(new FeedReplyViewModel(item));
                                }
                                else
                                    feedPage--;
                                break;
                            case 1:
                                string result = await Tools.GetJson($"/feed/likeList?id={id}&listType=lastupdate_desc&page={++likePage}&firstItem={likeFirstItem}&lastItem={likeLastItem}");
                                JsonArray root = Tools.GetDataArray(result);
                                if (root.Count != 0)
                                {
                                    likeLastItem = root.Last().GetObject()["uid"].GetNumber();
                                    ObservableCollection<UserViewModel> F = likeListView.ItemsSource as ObservableCollection<UserViewModel>;
                                    foreach (IJsonValue i in root)
                                        F.Add(new UserViewModel(i));
                                }
                                else
                                    likePage--;
                                break;
                            case 2:
                                string r = await Tools.GetJson($"/feed/forwardList?id={id}&type=feed&page={++sharePage}");
                                JsonArray roots = Tools.GetDataArray(r);
                                if (roots.Count != 0)
                                {
                                    ObservableCollection<SourceFeedViewModel> F = shareuserListView.ItemsSource as ObservableCollection<SourceFeedViewModel>;
                                    foreach (var i in roots)
                                        F.Add(new SourceFeedViewModel(i));
                                }
                                else sharePage--;
                                break;
                        }
                    }
                    else if (AnswerList != null)
                    {
                        string re = await Tools.GetJson($"/question/answerList?id={id}&sort={answerSortType}&page={++answerPage}&firstItem={answerFirstItem}&lastItem={answerLastItem}");
                        JsonArray array = Tools.GetDataArray(re);
                        if (!(array is null) && array.Count != 0)
                        {
                            foreach (var item in array)
                                answers.Add(new FeedViewModel(item, FeedDisplayMode.notShowMessageTitle));
                            answerLastItem = array.Last().GetObject()["id"].GetNumber();
                        }
                        else answerPage--;
                    }
                    Tools.HideProgressBar();
                }
            }
            else refreshText.Visibility = Visibility.Visible;
        }

        private void MarkdownTextBlock_LinkClicked(object sender, Microsoft.Toolkit.Uwp.UI.Controls.LinkClickedEventArgs e) => Tools.OpenLink(e.Link);

        private void StackPanel_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            if (element.Tag is null) return;
            else if (element.Tag is string s) Tools.OpenLink(s);
        }

        private async void MarkdownTextBlock_ImageResolving(object sender, Microsoft.Toolkit.Uwp.UI.Controls.ImageResolvingEventArgs e)
        {
            e.Image = await ImageCache.GetImage(ImageType.SmallImage, e.Url);
            e.Handled = true;
            Tools.SetEmojiPadding(sender);
        }

        private void MarkdownTextBlock_ImageClicked(object sender, Microsoft.Toolkit.Uwp.UI.Controls.LinkClickedEventArgs e)
        {
            if (e.Link.IndexOf("http") == 0) Tools.ShowImage(e.Link);
        }

        private void Image_Tapped(object sender, TappedRoutedEventArgs e) => Tools.ShowImage((sender as FrameworkElement).Tag as string);

        private async void GetMoreHotReplyListViewItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Tools.ShowProgressBar();
            if (hotReplyPage == 0)
                hotReplys.Clear();
            string r = await Tools.GetJson($"/feed/hotReplyList?id={id}&page={++hotReplyPage}{(hotReplyPage > 1 ? $"&firstItem={hotReplyFirstItem}&lastItem={hotReplyLastItem}" : string.Empty)}&discussMode=1");
            JsonArray array = Tools.GetDataArray(r);
            if (array != null && array.Count > 0)
            {
                foreach (var item in array)
                    hotReplys.Add(new FeedReplyViewModel(item));
                hotReplyFirstItem = array.First().GetObject()["id"].GetNumber();
                hotReplyLastItem = array.Last().GetObject()["id"].GetNumber();
            }
            else hotReplyPage--;
            Tools.HideProgressBar();
        }

        private async void FeedDetailPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Pivot item = sender as Pivot;
            if (FeedDetailPivot != null)
            {
                switch (item.SelectedIndex)
                {
                    case 1:
                        if ((likeListView?.ItemsSource as ObservableCollection<UserViewModel>)?.Count > 0) return;
                        string result = await Tools.GetJson($"/feed/likeList?id={id}&listType=lastupdate_desc&page={++likePage}");
                        JsonArray root = Tools.GetDataArray(result);
                        ObservableCollection<UserViewModel> F = new ObservableCollection<UserViewModel>();
                        if (root.Count != 0)
                        {
                            likeFirstItem = root.First().GetObject()["uid"].GetNumber();
                            likeLastItem = root.Last().GetObject()["uid"].GetNumber();
                            foreach (IJsonValue i in root)
                                F.Add(new UserViewModel(i));
                        }
                        else likePage--;
                        likeListView.ItemsSource = F;
                        break;
                    case 2:
                        if ((shareuserListView.ItemsSource as ObservableCollection<SourceFeedViewModel>)?.Count > 0) return;
                        string r = await Tools.GetJson($"/feed/forwardList?id={id}&type=feed&page={++sharePage}");
                        JsonArray roots = Tools.GetDataArray(r);
                        ObservableCollection<SourceFeedViewModel> Fs = new ObservableCollection<SourceFeedViewModel>();
                        if (roots.Count != 0)
                            foreach (var i in roots)
                                Fs.Add(new SourceFeedViewModel(i));
                        else sharePage--;
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
            answerFirstItem = answerLastItem = 0;
            answerPage = 0;
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
            feedPage = 1;
            feedFirstItem = feedLastItem = 0;
            replys.Clear();
            Refresh();
        }

        //https://www.cnblogs.com/arcsinw/p/8638526.html
        private void GridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GridView view = sender as GridView;
            if (view.SelectedIndex > -1)
            {
                if (view.Tag is List<string> ss)
                    Tools.ShowImages(ss.ToArray(), view.SelectedIndex);
                else if (view.Tag is string s)
                {
                    if (string.IsNullOrWhiteSpace(s)) return;
                    Tools.ShowImage(s);
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


        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (feedArticleTitle != null)
                feedArticleTitle.Height = feedArticleTitle.Width * 0.44;
        }
    }
}