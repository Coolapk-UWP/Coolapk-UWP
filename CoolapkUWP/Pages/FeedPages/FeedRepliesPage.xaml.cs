using CoolapkUWP.Helpers;
using CoolapkUWP.Models;
using CoolapkUWP.ViewModels;
using Newtonsoft.Json.Linq;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace CoolapkUWP.Pages.FeedPages
{
    public sealed partial class FeedRepliesPage : Page
    {
        private double id;
        private FeedListProvider provider;

        public FeedRepliesPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            TitleBar.ShowProgressRing();

            provider =
                new FeedListProvider(
                    async (p, _page, _, lastItem) =>
                    {
                        var page = p == -1 ? ++_page : p;
                        return (JArray)await DataHelper.GetDataAsync(
                            DataUriType.GetReplyReplies,
                            id,
                            page,
                            page > 1 ? $"&lastItem={lastItem}" : string.Empty);
                    },
                    (a, b) => ((FeedReplyModel)a).Id == b.Value<int>("id"),
                    (o) => new FeedReplyModel(o, false),
                    () => Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse("NotificationsPage").GetString("noMore"),
                    () => scrollViewer?.ChangeView(null, 0, null),
                    "id");

            FeedReplyList.ItemsSource = provider.Models;

            var reply = e.Parameter as FeedReplyModel;
            TitleBar.Title = string.Format(
                Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse("FeedRepliesPage").GetString("title"),
                reply.Replynum);
            id = reply.Id;
            reply.ShowreplyRows = false;
            reply.EntityFixed = true;
            provider.Models.Add(reply);
            GetReplys(false);
            TitleBar.HideProgressRing();
        }

        private void VScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (!e.IsIntermediate && scrollViewer.VerticalOffset == scrollViewer.ScrollableHeight)
            {
                GetReplys(false);
            }
        }

        private async void GetReplys(bool isRefresh)
        {
            TitleBar.ShowProgressRing();
            await provider.Refresh(isRefresh ? 1 : -1);
            TitleBar.HideProgressRing();
        }

        private void TitleBar_BackButtonClick(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }

        private void TitleBar_RefreshEvent(object sender, RoutedEventArgs e)
        {
            GetReplys(true);
        }
    }
}