using CoolapkUWP.Helpers;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace CoolapkUWP.Controls
{
    public sealed partial class FeedShellListControl : UserControl
    {
        public event EventHandler<bool> NeedProgressRing;

        private ViewModels.FeedDetailList.ViewModel provider;

        public FeedShellListControl()
        {
            this.InitializeComponent();
        }

        internal void SetProvider(ViewModels.FeedDetailList.ViewModel provider)
        {
            this.provider = provider;
            replyList.ItemsSource = provider.ReplyProvider.Models;
            likeList.ItemsSource = provider.LikeProvider.Models;
            shareList.ItemsSource = provider.ShareProvider.Models;
            if (provider.HotReplys.Length > 0)
            {
                FindName(nameof(hotReplyList));
                hotReplyList.ItemsSource = provider.HotReplys;
            }
            else if (hotReplyList != null)
            {
                UnloadObject(hotReplyList);
            }
        }

        internal void ChangeSelection(int i)
        {
            FeedDetailPivot.SelectedIndex = i;
        }

        internal async void ChangeFeedSorting(int i)
        {
            if (i > 0 && hotReplyList != null)
            {
                hotReplyList.Visibility = Visibility.Collapsed;
            }
            NeedProgressRing?.Invoke(this, true);
            await provider.SetComboBoxSelectedIndex(i);
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

        private async void FeedDetailPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FeedDetailPivot != null)
            {
                NeedProgressRing?.Invoke(this, true);
                await provider.SetSelectedIndex(FeedDetailPivot.SelectedIndex);
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

        private void GetMoreHotReplyListViewItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            UIHelper.NavigateInSplitPane(typeof(Pages.FeedPages.FeedRepliesPage), new ViewModels.FeedRepliesPage.ViewModel(provider.Id, ViewModels.FeedRepliesPage.ListType.HotReply));
        }
    }
}