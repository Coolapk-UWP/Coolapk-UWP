using CoolapkUWP.Helpers;
using CoolapkUWP.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace CoolapkUWP.Controls
{
    public sealed partial class FeedShellListControl : UserControl
    {
        private string listType = "lastupdate_desc";
        private string isFromAuthor = "0";
        private readonly ObservableCollection<FeedReplyModel> hotReplies = new ObservableCollection<FeedReplyModel>();
        private readonly ObservableCollection<FeedReplyModel> replies = new ObservableCollection<FeedReplyModel>();
        private readonly ObservableCollection<UserModel> likes = new ObservableCollection<UserModel>();
        private readonly ObservableCollection<SourceFeedModel> shares = new ObservableCollection<SourceFeedModel>();
        private int repliesPage, likesPage, sharesPage, hotRepliesPage;
        private double replyFirstItem, replyLastItem, likeFirstItem, likeLastItem, hotReplyFirstItem, hotReplyLastItem;
        private readonly string[] icons = new string[] { "" /*0xE70D;*/, ""/*0xE70E*/};

        public string FeedId { get; set; }

        public FeedShellListControl()
        {
            this.InitializeComponent();
        }

        private void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if ((sender as FrameworkElement).Tag is string s) UIHelper.OpenLinkAsync(s);
        }

        private void StackPanel_Tapped(object sender, TappedRoutedEventArgs e)
        {
            UIHelper.OpenLinkAsync((sender as FrameworkElement).Tag as string);
        }

        private void ChangeHotReplysDisplayModeListViewItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (iconText.Text == icons[1])
            {
                GetMoreHotReplyListViewItem.Visibility = hotReplyListView.Visibility = Visibility.Collapsed;
                iconText.Text = icons[0];
            }
            else
            {
                GetMoreHotReplyListViewItem.Visibility = hotReplyListView.Visibility = Visibility.Visible;
                iconText.Text = icons[1];
            }
        }

        private void FeedDetailPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FeedDetailPivot != null)
            {
                switch (FeedDetailPivot.SelectedIndex)
                {
                    case 1:
                        if (likes.Count == 0)
                        {
                            RefreshLikes();
                        }

                        break;

                    case 2:
                        if (shares.Count == 0)
                        {
                            RefreshShares();
                        }

                        break;
                }
            }
        }

        private void ListViewItem_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if(e.Key==Windows.System.VirtualKey.Enter || e.Key == Windows.System.VirtualKey.Space)
            {
                StackPanel_Tapped(sender, null);
            }
        }

        internal void ChangeSelection(int i)
        {
            FeedDetailPivot.SelectedIndex = i;
        }

        private void GetMoreHotReplyListViewItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            RefreshHotReplies();
        }

        internal void ChangeFeedSorting(int i)
        {
            switch (i)
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

            repliesPage = 1;
            replyFirstItem = replyLastItem = 0;
            replies.Clear();
            Refresh();
        }

        internal void Refresh()
        {
            switch (FeedDetailPivot?.SelectedIndex)
            {
                case 0:
                    RefreshFeedReplies(1);
                    break;

                case 1:
                    RefreshLikes(1);
                    break;

                case 2:
                    RefreshShares();
                    break;
            }
        }

        internal async void RefreshHotReplies(int p = -1)
        {
            JArray array = (JArray)await DataHelper.GetDataAsync(
                                DataUriType.GetHotReplies,
                                FeedId,
                                p == -1 ? ++hotRepliesPage : p,
                                hotRepliesPage > 1 ? $"&firstItem={hotReplyFirstItem}&lastItem={hotReplyLastItem}" : string.Empty);
            if (array != null && array.Count > 0)
            {
                if (p == -1 || hotRepliesPage == 1)
                {
                    var d = (from a in hotReplies
                             from b in array
                             where a.Id == b.Value<int>("id")
                             select a).ToArray();
                    foreach (var item in d)
                        hotReplies.Remove(item);
                    for (int i = 0; i < array.Count; i++)
                        hotReplies.Insert(i, new FeedReplyModel((JObject)array[i]));
                    hotReplyFirstItem = array.First.Value<int>("id");
                    if (hotRepliesPage == 1)
                        hotReplyLastItem = array.Last.Value<int>("id");
                }
                else
                {
                    hotReplyLastItem = array.Last.Value<int>("id");
                    foreach (JObject item in array)
                        hotReplies.Add(new FeedReplyModel(item));
                }
            }
            else if (p == -1)
            {
                hotRepliesPage--;
                UIHelper.ShowMessage("没有更多热门回复了");
            }
        }

        internal async void RefreshFeedReplies(int p = -1)
        {
            JArray array = (JArray)await DataHelper.GetDataAsync(
                                DataUriType.GetFeedReplies,
                                FeedId,
                                listType,
                                p == -1 ? ++repliesPage : p,
                                replyFirstItem == 0 ? string.Empty : $"&firstItem={replyFirstItem}",
                                replyLastItem == 0 ? string.Empty : $"&lastItem={replyLastItem}",
                                isFromAuthor);
            if (array != null && array.Count != 0)
            {
                if (p == 1 || repliesPage == 1)
                {
                    var d = (from a in replies
                             from b in array
                             where a.Id == b.Value<int>("id")
                             select a).ToArray();
                    foreach (var item in d)
                        replies.Remove(item);
                    replyFirstItem = array.First.Value<int>("id");
                    if (repliesPage == 1)
                        replyLastItem = array.Last.Value<int>("id");
                    for (int i = 0; i < array.Count; i++)
                        replies.Insert(i, new FeedReplyModel((JObject)array[i]));
                }
                else
                {
                    replyLastItem = array.Last.Value<int>("id");
                    foreach (JObject item in array)
                        replies.Add(new FeedReplyModel(item));
                }
            }
            else if (p == -1)
            {
                repliesPage--;
                UIHelper.ShowMessage("没有更多回复了");
            }
        }

        internal async void RefreshLikes(int p = -1)
        {
            JArray array = (JArray)await DataHelper.GetDataAsync(
                                            DataUriType.GetLikeList,
                                            FeedId,
                                            p == -1 ? ++likesPage : p,
                                            likeFirstItem == 0 ? string.Empty : $"&firstItem={likeFirstItem}",
                                            likeLastItem == 0 ? string.Empty : $"&lastItem={likeLastItem}");
            if (array != null && array.Count != 0)
            {
                if (p == 1 || likesPage == 1)
                {
                    likeFirstItem = array.First.Value<int>("uid");
                    if (likesPage == 1)
                        likeLastItem = array.Last.Value<int>("uid");
                    var d = (from a in likes
                             from b in array
                             where a.Url == b.Value<string>("url")
                             select a).ToArray();
                    foreach (var item in d)
                        likes.Remove(item);
                    for (int i = 0; i < array.Count; i++)
                        likes.Insert(i, new UserModel((JObject)array[i]));
                }
                else
                {
                    likeLastItem = array.Last.Value<int>("uid");
                    foreach (JObject item in array)
                        likes.Add(new UserModel(item));
                }
            }
            else if (p == -1)
            {
                likesPage--;
                UIHelper.ShowMessage("没有更多点赞用户了");
            }
        }

        internal async void RefreshShares()
        {
            var array = (JArray)await DataHelper.GetDataAsync(DataUriType.GetShareList, FeedId, ++sharesPage);
            if (array != null && array.Count != 0)
            {
                if (sharesPage == 1)
                    foreach (JObject item in array)
                        shares.Add(new SourceFeedModel(item));
                else for (int i = 0; i < array.Count; i++)
                    {
                        if (shares.First()?.Url == array[i].Value<string>("url")) return;
                        shares.Insert(i, new SourceFeedModel((JObject)array[i]));
                    }
            }
            else
            {
                sharesPage--;
                UIHelper.ShowMessage("没有更多转发了");
            }
        }
    }
}