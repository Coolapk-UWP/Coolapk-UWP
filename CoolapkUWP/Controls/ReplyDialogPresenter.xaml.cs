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
using Windows.UI.Xaml.Controls.Primitives;

namespace CoolapkUWP.Controls
{
    public sealed partial class ReplyDialogPresenter : Page
    {
        private readonly double id;
        private int page;
        private double lastItem;
        private readonly ObservableCollection<FeedReplyModel> replys = new ObservableCollection<FeedReplyModel>();
        private ScrollViewer VScrollViewer;

        public ReplyDialogPresenter(object o, Popup popup)
        {
            this.InitializeComponent();
            UIHelper.ShowProgressRing();
            Height = Window.Current.Bounds.Height;
            Width = Window.Current.Bounds.Width;
            HorizontalAlignment = HorizontalAlignment.Center;
            Window.Current.SizeChanged += WindowSizeChanged;
            TitleBar.BackButtonClick += (s, e) => popup.Hide();
            popup.Closed += (s, e) => Window.Current.SizeChanged -= WindowSizeChanged;
            FeedReplyModel reply = o as FeedReplyModel;
            TitleBar.Title = $"回复({reply.Replynum})";
            TitleBar.RefreshEvent += (s, e) => GetReplys(true);
            id = reply.Id;
            FeedReplyList.ItemsSource = replys;
            reply.ShowreplyRows = false;
            replys.Add(reply);
            GetReplys(false);
            UIHelper.HideProgressRing();
            Task.Run(async () =>
            {
                await Task.Delay(200);
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    VScrollViewer = VisualTree.FindDescendantByName(FeedReplyList, "ScrollViewer") as ScrollViewer;
                    VScrollViewer.ViewChanged += VScrollViewer_ViewChanged;
                });
            });
        }

        private void WindowSizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            Height = e.Size.Height;
            Width = e.Size.Width;
        }

        private async void GetReplys(bool isRefresh)
        {
            UIHelper.ShowProgressRing();
            int page = isRefresh ? 1 : ++this.page;
            JArray array = (JArray)await DataHelper.GetDataAsync(DataUriType.GetReplyReplies, id, page, page > 1 ? $"&lastItem={lastItem}" : string.Empty);
            if (array != null && array.Count > 0)
                if (isRefresh)
                {
                    VScrollViewer?.ChangeView(null, 0, null);
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
            else if (!isRefresh) this.page--;
            UIHelper.HideProgressRing();
        }

        private void VScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (VScrollViewer.VerticalOffset == VScrollViewer.ScrollableHeight)
                GetReplys(false);
        }

        private void FeedReplyList_RefreshRequested(object sender, EventArgs e) => GetReplys(true);
    }
}