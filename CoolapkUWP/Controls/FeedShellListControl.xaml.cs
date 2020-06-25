using CoolapkUWP.Helpers;
using CoolapkUWP.Models;
using CoolapkUWP.ViewModels;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace CoolapkUWP.Controls
{
    public sealed partial class FeedShellListControl : UserControl
    {
        private string replyListType = "lastupdate_desc";
        private string isFromAuthor = "0";
        private readonly string[] icons = new string[] { "" /*0xE70D;*/, ""/*0xE70E*/};
        private readonly Windows.ApplicationModel.Resources.ResourceLoader loader = Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse("FeedShellListControl");

        private readonly FeedListProvider hotReplyProvider;
        private readonly FeedListProvider replyProvider;
        private readonly FeedListProvider likeProvider;
        private readonly FeedListProvider shareProvider;

        public string FeedId { get; set; }

        public event EventHandler NeedMoveToTop;
        public event EventHandler<bool> NeedProgressRing;

        public FeedShellListControl()
        {
            this.InitializeComponent();

            hotReplyProvider =
                new FeedListProvider(
                    async (p, page, firstItem, lastItem) =>
                        (JArray)await DataHelper.GetDataAsync(
                            DataUriType.GetHotReplies,
                            FeedId,
                            p == -1 ? ++page : p,
                            page > 1 ? $"&firstItem={firstItem}&lastItem={lastItem}" : string.Empty),
                    (a, b) => ((FeedReplyModel)a).Id == b.Value<int>("id"),
                    (o) => new FeedReplyModel(o),
                    () => loader.GetString("noMoreHotReply"),
                    () => NeedMoveToTop?.Invoke(this, null),
                    "id");
            hotReplyList.ItemsSource = hotReplyProvider.Models;

            replyProvider =
                new FeedListProvider(
                    async (p, page, firstItem, lastItem) =>
                        (JArray)await DataHelper.GetDataAsync(
                            DataUriType.GetFeedReplies,
                            FeedId,
                            replyListType,
                            p == -1 ? ++page : p,
                            page > 1 ? $"&firstItem={firstItem}&lastItem={lastItem}" : string.Empty,
                            isFromAuthor),
                    (a, b) => ((FeedReplyModel)a).Id == b.Value<int>("id"),
                    (o) => new FeedReplyModel(o),
                    () => loader.GetString("noMoreReply"),
                    () => NeedMoveToTop?.Invoke(this, null),
                    "id");
            replyList.ItemsSource = replyProvider.Models;

            likeProvider =
                new FeedListProvider(
                    async (p, page, firstItem, lastItem) =>
                        (JArray)await DataHelper.GetDataAsync(
                            DataUriType.GetLikeList,
                            FeedId,
                            p == -1 ? ++page : p,
                            page > 1 ? $"&firstItem={firstItem}&lastItem={lastItem}" : string.Empty),
                    (a, b) => ((UserModel)a).Url == b.Value<string>("url"),
                    (o) => new UserModel(o),
                    () => loader.GetString("noMoreLikeUser"),
                    () => NeedMoveToTop?.Invoke(this, null),
                    "uid");
            likeList.ItemsSource = likeProvider.Models;

            shareProvider =
                new FeedListProvider(
                    async (p, page, firstItem, lastItem) => (JArray)await DataHelper.GetDataAsync(DataUriType.GetShareList, FeedId, p == -1 ? ++page : p),
                    (a, b) => ((SourceFeedModel)a).Url == b.Value<string>("url"),
                    (o) => new SourceFeedModel(o),
                    () => loader.GetString("noMoreShare"),
                    () => NeedMoveToTop?.Invoke(this, null),
                    "id");
            shareList.ItemsSource = shareProvider.Models;
        }

        internal void ChangeSelection(int i)
        {
            FeedDetailPivot.SelectedIndex = i;
        }

        internal void ChangeFeedSorting(int i)
        {
            switch (i)
            {
                case -1: return;
                case 0:
                    replyListType = "lastupdate_desc";
                    isFromAuthor = "0";
                    break;

                case 1:
                    replyListType = "dateline_desc";
                    isFromAuthor = "0";
                    break;

                case 2:
                    replyListType = "popular";
                    isFromAuthor = "0";
                    break;

                case 3:
                    replyListType = string.Empty;
                    isFromAuthor = "1";
                    break;
            }

            replyProvider.Reset();
            Refresh();
        }

        internal async Task Refresh(bool isRefresh = true)
        {
            var n = isRefresh ? 1 : -1;
            NeedProgressRing?.Invoke(this, true);
            switch (FeedDetailPivot?.SelectedIndex)
            {
                case 0:
                    await replyProvider.Refresh(n);
                    break;

                case 1:
                    await likeProvider.Refresh(n);
                    break;

                case 2:
                    await shareProvider.Refresh(n);
                    break;
            }
            NeedProgressRing?.Invoke(this, false);
        }

        internal async void RefreshHotReply()
        {
            NeedProgressRing?.Invoke(this, true);
            await hotReplyProvider.Refresh();
            NeedProgressRing?.Invoke(this, false);
        }

        private void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (e != null && !UIHelper.IsOriginSource(sender, e.OriginalSource)) { return; }

            if ((sender as FrameworkElement).Tag is string s)
            {
                UIHelper.OpenLinkAsync(s);
            }
        }

        private void ChangeHotReplysDisplayModeListViewItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (iconText.Text == icons[1])
            {
                GetMoreHotReplyListViewItem.Visibility = hotReplyList.Visibility = Visibility.Collapsed;
                iconText.Text = icons[0];
            }
            else
            {
                GetMoreHotReplyListViewItem.Visibility = hotReplyList.Visibility = Visibility.Visible;
                iconText.Text = icons[1];
            }
        }

        private async void FeedDetailPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FeedDetailPivot != null)
            {
                NeedProgressRing?.Invoke(this, true);
                switch (FeedDetailPivot.SelectedIndex)
                {
                    case 1:
                        if (likeProvider.Models.Count == 0)
                        {
                            await likeProvider.Refresh();
                        }

                        break;

                    case 2:
                        if (shareProvider.Models.Count == 0)
                        {
                            await shareProvider.Refresh();
                        }

                        break;
                }
                NeedProgressRing?.Invoke(this, false);
            }
        }

        private void ListViewItem_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter || e.Key == Windows.System.VirtualKey.Space)
            {
                Grid_Tapped(sender, null);
            }
        }

        private async void GetMoreHotReplyListViewItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            NeedProgressRing?.Invoke(this, true);
            await hotReplyProvider.Refresh();
            NeedProgressRing?.Invoke(this, false);
        }
    }
}