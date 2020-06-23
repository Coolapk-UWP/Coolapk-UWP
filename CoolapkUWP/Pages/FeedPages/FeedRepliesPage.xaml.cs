using CoolapkUWP.Helpers;
using CoolapkUWP.Models;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace CoolapkUWP.Pages.FeedPages
{
    public sealed partial class FeedRepliesPage : Page
    {
        private double id;
        private int page;
        private double lastItem;
        private readonly ObservableCollection<FeedReplyModel> replys = new ObservableCollection<FeedReplyModel>();

        public FeedRepliesPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            TitleBar.ShowProgressRing();
            FeedReplyList.ItemsSource = replys;

            var reply = e.Parameter as FeedReplyModel;
            TitleBar.Title = $"回复({reply.Replynum})";
            id = reply.Id;
            reply.ShowreplyRows = false;
            replys.Add(reply);
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
            int page = isRefresh ? 1 : ++this.page;
            var array = (JArray)await DataHelper.GetDataAsync(DataUriType.GetReplyReplies, id, page, page > 1 ? $"&lastItem={lastItem}" : string.Empty);
            if (array != null && array.Count > 0)
            {
                if (isRefresh)
                {
                    scrollViewer?.ChangeView(null, 0, null);
                    var d = (from a in replys
                             from b in array
                             where a.Id == b.Value<int>("id")
                             select a).ToArray();
                    foreach (var item in d)
                        replys.Remove(item);
                    for (int i = 0; i < array.Count; i++)
                        replys.Insert(i + 1, new FeedReplyModel((JObject)array[i]));
                }
                else
                {
                    foreach (JObject item in array)
                        replys.Add(new FeedReplyModel(item, false));
                    lastItem = array.Last.Value<int>("id");
                }
            }
            else if (!isRefresh) this.page--;
            TitleBar.HideProgressRing();
        }

        private void TitleBar_BackButtonClick(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }

        private void TitleBar_RefreshEvent(object sender, RoutedEventArgs e) => GetReplys(true);
    }
}