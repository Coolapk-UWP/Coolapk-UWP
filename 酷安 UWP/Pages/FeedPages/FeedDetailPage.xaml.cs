using CoolapkUWP.Control;
using CoolapkUWP.Control.ViewModels;
using CoolapkUWP.Data;
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
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace CoolapkUWP.Pages.FeedPages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class FeedDetailPage : Page, INotifyPropertyChanged
    {
        string feedId;
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
        ObservableCollection<FeedReplyViewModel> hotReplies = new ObservableCollection<FeedReplyViewModel>();
        ObservableCollection<FeedReplyViewModel> replies = new ObservableCollection<FeedReplyViewModel>();
        ObservableCollection<FeedViewModel> answers = new ObservableCollection<FeedViewModel>();
        ObservableCollection<UserViewModel> likes = new ObservableCollection<UserViewModel>();
        ObservableCollection<SourceFeedViewModel> shares = new ObservableCollection<SourceFeedViewModel>();
        int repliesPage, likesPage, sharesPage, answersPage, hotRepliesPage;
        double replyFirstItem, replyLastItem, likeFirstItem, likeLastItem, answerFirstItem, answerLastItem, hotReplyFirstItem, hotReplyLastItem;
        string answerSortType = "reply";
        string[] comboBoxItems = new string[] { "最近回复", "按时间排序", "按热度排序", "只看楼主" };

        public event PropertyChangedEventHandler PropertyChanged;
        public FeedDetailPage()
        {
            this.InitializeComponent();
            Task.Run(async () =>
            {
                await Task.Delay(300);
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    (VisualTree.FindDescendantByName(MainListView, "ScrollViewer") as ScrollViewer).ViewChanged += ScrollViewer_ViewChanged;
                    (VisualTree.FindDescendantByName(RightSideListView, "ScrollViewer") as ScrollViewer).ViewChanged += ScrollViewer_ViewChanged;
                });
            });
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            feedId = e.Parameter as string;
            LoadFeedDetail();
        }

        public async void LoadFeedDetail()
        {
            Tools.ShowProgressBar();
            if (string.IsNullOrEmpty(feedId)) return;
            JsonObject detail = Tools.GetJSonObject(await Tools.GetJson("/feed/detail?id=" + feedId));
            if (detail != null)
            {
                FeedDetail = new FeedDetailViewModel(detail);
                TitleBar.Title = FeedDetail.title;
                if (FeedDetail.isQuestionFeed)
                {
                    if (answersPage != 0 || answerLastItem != 0) return;
                    FindName(nameof(AnswersListView));
                    PivotItemPanel.Visibility = Visibility.Collapsed;
                    RefreshAnswers();
                }
                else
                {
                    FindName(nameof(FeedDetailPivot));
                    if (feedArticleTitle != null)
                    {
                        feedArticleTitle.Height = feedArticleTitle.Width * 0.44;
                        Page_SizeChanged(null, null);
                    }
                    else if (FeedDetail.isCoolPictuers)
                        Page_SizeChanged(null, null);
                    RefreshHotFeed();
                    RefreshFeedReply();
                    TitleBar.ComboBoxVisibility = Visibility.Visible;
                    TitleBar.ComboBoxSelectedIndex = 0;
                }
            }
            Tools.HideProgressBar();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            string tag = (sender as Button).Tag as string;
            switch (tag)
            {
                case "reply":
                    FeedDetailPivot.SelectedIndex = 0;
                    ToReplyPivotItemButton.BorderThickness = new Thickness(0, 0, 0, 2);
                    ToLikePivotItemButton.BorderThickness = ToSharePivotItemButton.BorderThickness = new Thickness(0);
                    TitleBar.ComboBoxVisibility = Visibility.Visible;
                    break;
                case "like":
                    FeedDetailPivot.SelectedIndex = 1;
                    ToLikePivotItemButton.BorderThickness = new Thickness(0, 0, 0, 2);
                    ToReplyPivotItemButton.BorderThickness = ToSharePivotItemButton.BorderThickness = new Thickness(0);
                    TitleBar.ComboBoxVisibility = Visibility.Collapsed;
                    break;
                case "share":
                    FeedDetailPivot.SelectedIndex = 2;
                    ToSharePivotItemButton.BorderThickness = new Thickness(0, 0, 0, 2);
                    ToReplyPivotItemButton.BorderThickness = ToLikePivotItemButton.BorderThickness = new Thickness(0);
                    TitleBar.ComboBoxVisibility = Visibility.Collapsed;
                    break;
                case "MakeLike":
                    if (FeedDetail.liked)
                    {
                        JsonObject o = Tools.GetJSonObject(await Tools.GetJson($"/feed/unlike?id={feedId}&detail=0"));
                        if (o != null)
                        {
                            FeedDetail.likenum = o["count"].GetNumber().ToString();
                            (sender as Button).Content = "点赞";
                            FeedDetail.liked = false;
                        }
                    }
                    else
                    {
                        JsonObject o = Tools.GetJSonObject(await Tools.GetJson($"/feed/like?id={feedId}&detail=0"));
                        if (o != null)
                        {
                            FeedDetail.likenum = o["count"].GetNumber().ToString();
                            (sender as Button).Content = "已点赞";
                            FeedDetail.liked = true;
                        }
                    }
                    break;
                default:
                    Tools.OpenLink(tag);
                    break;
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e) => Frame.GoBack();

        private void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if ((sender as FrameworkElement).Tag is string s) Tools.OpenLink(s);
        }

        async void Refresh()
        {
            Tools.ShowProgressBar();
            if (FeedDetailPivot != null)
            {
                switch (FeedDetailPivot.SelectedIndex)
                {
                    case 0:
                        RefreshFeedReply(1);
                        break;
                    case 1:
                        RefreshLikes(1);
                        break;
                    case 2:
                        RefreshShares();
                        break;
                }
            }
            else if (AnswersListView != null)
            {
                if (answerLastItem != 0) return;
                RefreshAnswers(1);
            }
            else
            {
                LoadFeedDetail();
                Tools.HideProgressBar();
                return;
            }
            if (RefreshAll) await RefreshFeed();
            Tools.HideProgressBar();
        }
        async Task RefreshFeed()
        {
            JsonObject detail = Tools.GetJSonObject(await Tools.GetJson("/feed/detail?id=" + feedId));
            if (detail != null) FeedDetail = new FeedDetailViewModel(detail);
        }
        async void RefreshHotFeed(int p = -1)
        {
            JsonArray array = Tools.GetDataArray(await Tools.GetJson($"/feed/hotReplyList?id={feedId}&page={(p == -1 ? ++hotRepliesPage : p)}{(hotRepliesPage > 1 ? $"&firstItem={hotReplyFirstItem}&lastItem={hotReplyLastItem}" : string.Empty)}&discussMode=1"));
            if (array != null && array.Count > 0)
            {
                if (p == -1 || hotRepliesPage == 1)
                {
                    var d = (from a in hotReplies
                             from b in array
                             where a.id == b.GetObject()["id"].GetNumber()
                             select a).ToArray();
                    foreach (var item in d)
                        hotReplies.Remove(item);
                    for (int i = 0; i < array.Count; i++)
                        hotReplies.Insert(i, new FeedReplyViewModel(array[i]));
                    hotReplyFirstItem = array.First().GetObject()["id"].GetNumber();
                    if (hotRepliesPage == 1)
                        hotReplyLastItem = array.Last().GetObject()["id"].GetNumber();
                }
                else
                {
                    hotReplyLastItem = array.Last().GetObject()["id"].GetNumber();
                    foreach (var item in array)
                        hotReplies.Add(new FeedReplyViewModel(item));
                }
            }
            else if (p == -1)
            {
                hotRepliesPage--;
                Tools.ShowMessage("没有更多热门回复了");
            }
        }
        async void RefreshFeedReply(int p = -1)
        {
            string listType = string.Empty, isFromAuthor = string.Empty;
            switch (TitleBar.ComboBoxSelectedIndex)
            {
                case -1: return;
                case 0:
                    listType = "lastupdate_desc";
                    isFromAuthor = "0";
                    break;
                case 1:
                    listType = "dateline_desc";
                    isFromAuthor = "0";
                    break;
                case 2:
                    listType = "popular";
                    isFromAuthor = "0";
                    break;
                case 3:
                    listType = string.Empty;
                    isFromAuthor = "1";
                    break;
            }
            JsonArray array = Tools.GetDataArray(await Tools.GetJson($"/feed/replyList?id={feedId}&listType={listType}&page={(p == -1 ? ++repliesPage : p)}{(replyFirstItem == 0 ? string.Empty : $"&firstItem={replyFirstItem}")}{(replyLastItem == 0 ? string.Empty : $"&lastItem={replyLastItem}")}&discussMode=1&feedType=feed&blockStatus=0&fromFeedAuthor={isFromAuthor}"));
            if (array != null && array.Count != 0)
            {
                if (p == 1 || repliesPage == 1)
                {
                    var d = (from a in replies
                             from b in array
                             where a.id == b.GetObject()["id"].GetNumber()
                             select a).ToArray();
                    foreach (var item in d)
                        replies.Remove(item);
                    replyFirstItem = array.First().GetObject()["id"].GetNumber();
                    if (repliesPage == 1)
                        replyLastItem = array.Last().GetObject()["id"].GetNumber();
                    for (int i = 0; i < array.Count; i++)
                        replies.Insert(i, new FeedReplyViewModel(array[i]));
                }
                else
                {
                    replyLastItem = array.Last().GetObject()["id"].GetNumber();
                    foreach (var item in array)
                        replies.Add(new FeedReplyViewModel(item));
                }
            }
            else if (p == -1)
            {
                repliesPage--;
                Tools.ShowMessage("没有更多回复了");
            }
        }
        async void RefreshAnswers(int p = -1)
        {
            JsonArray array = Tools.GetDataArray(await Tools.GetJson($"/question/answerList?id={feedId}&sort={answerSortType}&page={(p == -1 ? ++answersPage : p)}{(answerFirstItem == 0 ? string.Empty : $"&firstItem={answerFirstItem}")}{(answerLastItem == 0 ? string.Empty : $"&lastItem={answerLastItem}")}"));
            if (array != null && array.Count != 0)
            {
                if (p == 1 || answersPage == 1)
                {
                    var d = (from a in answers
                             from b in array
                             where a.url == b.GetObject()["url"].GetString()
                             select a).ToArray();
                    foreach (var item in d)
                        answers.Remove(item);
                    for (int i = 0; i < array.Count; i++)
                        answers.Insert(i, new FeedViewModel(array[i], FeedDisplayMode.notShowMessageTitle));
                    answerFirstItem = array.First().GetObject()["id"].GetNumber();
                    if (answersPage == 1)
                        answerLastItem = array.Last().GetObject()["id"].GetNumber();
                }
                else
                {
                    answerLastItem = array.Last().GetObject()["id"].GetNumber();
                    foreach (var item in array)
                        answers.Add(new FeedViewModel(item, FeedDisplayMode.notShowMessageTitle));
                }
            }
            else if (p == -1)
            {
                answersPage--;
                Tools.ShowMessage("没有更多回答了");
            }
        }
        async void RefreshLikes(int p = -1)
        {
            JsonArray array = Tools.GetDataArray(await Tools.GetJson($"/feed/likeList?id={feedId}&listType=lastupdate_desc&page={(p == -1 ? ++likesPage : p)}{(likeFirstItem == 0 ? string.Empty : $"&firstItem={likeFirstItem}")}{(likeLastItem == 0 ? string.Empty : $"&lastItem={likeLastItem}")}"));
            if (array != null && array.Count != 0)
            {
                if (p == 1 || likesPage == 1)
                {
                    likeFirstItem = array.First().GetObject()["uid"].GetNumber();
                    if (likesPage == 1)
                        likeLastItem = array.Last().GetObject()["uid"].GetNumber();
                    var d = (from a in likes
                             from b in array
                             where a.url == b.GetObject()["url"].GetString()
                             select a).ToArray();
                    foreach (var item in d)
                        likes.Remove(item);
                    for (int i = 0; i < array.Count; i++)
                        likes.Insert(i, new UserViewModel(array[i]));
                }
                else
                {
                    likeLastItem = array.Last().GetObject()["uid"].GetNumber();
                    foreach (var item in array)
                        likes.Add(new UserViewModel(item));
                }
            }
            else if (p == -1)
            {
                likesPage--;
                Tools.ShowMessage("没有更多点赞用户了");
            }
        }
        async void RefreshShares()
        {
            string r = await Tools.GetJson($"/feed/forwardList?id={feedId}&type=feed&page={++sharesPage}");
            JsonArray array = Tools.GetDataArray(r);
            if (array != null && array.Count != 0)
            {
                if (sharesPage == 1)
                    foreach (var item in array)
                        shares.Add(new SourceFeedViewModel(item));
                else for (int i = 0; i < array.Count; i++)
                    {
                        if (shares.First()?.url == array[i].GetObject()["url"].GetString()) return;
                        shares.Insert(i, new SourceFeedViewModel(array[i]));
                    }
            }
            else
            {
                sharesPage--;
                Tools.ShowMessage("没有更多转发了");
            }
        }

        private void ChangeHotReplysDisplayModeListViewItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (iconText.Text == "")//(Symbol)0xE70E)
            {
                GetMoreHotReplyListViewItem.Visibility = hotReplyListView.Visibility = Visibility.Collapsed;
                iconText.Text = "";//(Symbol)0xE70D;
            }
            else
            {
                GetMoreHotReplyListViewItem.Visibility = hotReplyListView.Visibility = Visibility.Visible;
                iconText.Text = "";//(Symbol)0xE70E;
            }
        }

        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            ScrollViewer scrollViewer = sender as ScrollViewer;
            if (!e.IsIntermediate)
            {
                double a = scrollViewer.VerticalOffset;
                if (a == scrollViewer.ScrollableHeight)
                {
                    Tools.ShowProgressBar();
                    scrollViewer.ChangeView(null, a, null);
                    if (FeedDetailPivot != null)
                        switch (FeedDetailPivot.SelectedIndex)
                        {
                            case 0:
                                RefreshFeedReply();
                                break;
                            case 1:
                                RefreshLikes();
                                break;
                            case 2:
                                RefreshShares();
                                break;
                        }
                    else if (AnswersListView != null) RefreshAnswers();
                    Tools.HideProgressBar();
                }
            }
        }

        private void ChangeFeedSortingComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!FeedDetail.isQuestionFeed && TitleBar.ComboBoxSelectedIndex != -1)
            {
                repliesPage = 1;
                replyFirstItem = replyLastItem = 0;
                replies.Clear();
                Refresh();
            }
        }

        private void StackPanel_Tapped(object sender, TappedRoutedEventArgs e) => Tools.OpenLink((sender as FrameworkElement).Tag as string);
        
        private void Image_Tapped(object sender, TappedRoutedEventArgs e) => Tools.ShowImage((sender as FrameworkElement).Tag as string, ImageType.SmallImage);

        private void GetMoreHotReplyListViewItem_Tapped(object sender, TappedRoutedEventArgs e) => RefreshHotFeed();

        private void FeedDetailPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FeedDetailPivot != null)
                switch (FeedDetailPivot.SelectedIndex)
                {
                    case 1:
                        if (likes.Count == 0) RefreshLikes();
                        break;
                    case 2:
                        if (shares.Count == 0) RefreshShares();
                        break;
                }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox box = sender as ComboBox;
            switch (box.SelectedIndex)
            {
                case -1: return;
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
            answersPage = 0;
            Refresh();
        }

        private void GridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is GridView view && view.SelectedIndex > -1)
            {
                if (view.Tag is List<string> ss) Tools.ShowImages(ss.ToArray(), view.SelectedIndex);
                else if (view.Tag is string s && !string.IsNullOrWhiteSpace(s))
                    Tools.ShowImage(s, ImageType.SmallImage);
                view.SelectedIndex = -1;
            }
            else if (sender is ListView viewb && viewb.SelectedIndex > -1)
            {
                if (viewb.Tag is List<string> ss) Tools.ShowImages(ss.ToArray(), viewb.SelectedIndex);
                viewb.SelectedIndex = -1;
            }
        }
        #region 界面模式切换
        private double _detailListHeight;
        double detailListHeight
        {
            get => _detailListHeight;
            set
            {
                _detailListHeight = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(detailListHeight)));
            }
        }
        bool RefreshAll;
        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (feedArticleTitle != null)
                feedArticleTitle.Height = feedArticleTitle.Width * 0.44;
            void set()
            {
                RightSideListView.Padding = Settings.stackPanelMargin;
                detailList.Padding = new Thickness(0, Settings.PageTitleHeight, 0, PivotItemPanel.ActualHeight + 16);
                detailListHeight = e?.NewSize.Height ?? Window.Current.Bounds.Height;
                RightColumnDefinition.Width = new GridLength(1, GridUnitType.Star);
                MainListView.SetValue(ScrollViewer.VerticalScrollModeProperty, ScrollMode.Disabled);
                MainListView.SetValue(ScrollViewer.VerticalScrollBarVisibilityProperty, ScrollBarVisibility.Disabled);
                MainListView.Padding = new Thickness(0);
                MainListView.Margin = new Thickness(0);
                detailList.SetValue(ScrollViewer.VerticalScrollModeProperty, ScrollMode.Auto);
                RightSideListView.SetValue(ScrollViewer.VerticalScrollModeProperty, ScrollMode.Auto);
                RightSideListView.SetValue(Grid.ColumnProperty, 1);
                RightSideListView.SetValue(Grid.RowProperty, 0);
                RightSideListView.InvalidateArrange();
                RefreshAll = false;
            }
            if ((e?.NewSize.Width ?? Window.Current.Bounds.Width) >= 768 && !((FeedDetail?.isFeedArticle ?? false) || (FeedDetail?.isCoolPictuers ?? false)))
            {
                LeftColumnDefinition.Width = new GridLength(384);
                set();
                PivotItemPanel.Margin = new Thickness(0, 0, Window.Current.Bounds.Width - 384, 0);
            }
            else if ((e?.NewSize.Width ?? Window.Current.Bounds.Width) >= 1024 && ((FeedDetail?.isFeedArticle ?? false) || (FeedDetail?.isCoolPictuers ?? false)))
            {
                LeftColumnDefinition.Width = new GridLength(640);
                set();
                PivotItemPanel.Margin = new Thickness(0, 0, Window.Current.Bounds.Width - 648, 0);
            }
            else
            {
                MainListView.Margin = new Thickness(0, 0, 0, PivotItemPanel.ActualHeight + 16);
                MainListView.Padding = Settings.stackPanelMargin;
                PivotItemPanel.Margin = new Thickness(0);
                detailList.Padding = RightSideListView.Padding = new Thickness(0, 0, 0, 12);
                detailListHeight = double.NaN;
                LeftColumnDefinition.Width = new GridLength(1, GridUnitType.Star);
                RightColumnDefinition.Width = new GridLength(0);
                MainListView.SetValue(ScrollViewer.VerticalScrollModeProperty, ScrollMode.Auto);
                MainListView.SetValue(ScrollViewer.VerticalScrollBarVisibilityProperty, ScrollBarVisibility.Auto);
                detailList.SetValue(ScrollViewer.VerticalScrollModeProperty, ScrollMode.Disabled);
                RightSideListView.SetValue(ScrollViewer.VerticalScrollModeProperty, ScrollMode.Disabled);
                RightSideListView.SetValue(Grid.ColumnProperty, 0);
                RightSideListView.SetValue(Grid.RowProperty, 1);
                RefreshAll = true;
            }
        }
        #endregion
    }
}