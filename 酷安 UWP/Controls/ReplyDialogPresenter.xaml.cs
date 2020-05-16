using CoolapkUWP.Controls.ViewModels;
using CoolapkUWP.Helpers;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace CoolapkUWP.Controls
{
    public sealed partial class ReplyDialogPresenter : Page
    {
        double id;
        int page;
        double lastItem;
        ObservableCollection<FeedReplyViewModel> replys = new ObservableCollection<FeedReplyViewModel>();
        ScrollViewer VScrollViewer;
        public ReplyDialogPresenter(object o, Popup popup)
        {
            this.InitializeComponent();
            UIHelper.ShowProgressBar();
            Height = Window.Current.Bounds.Height;
            Width = Window.Current.Bounds.Width;
            HorizontalAlignment = HorizontalAlignment.Center;
            Window.Current.SizeChanged += WindowSizeChanged;
            TitleBar.BackButtonClick += (s, e) => popup.Hide();
            popup.Closed += (s, e) => Window.Current.SizeChanged -= WindowSizeChanged;
            FeedReplyViewModel reply = o as FeedReplyViewModel;
            TitleBar.Title = $"回复({reply.replynum})";
            TitleBar.RefreshEvent += (s, e) => GetReplys(true);
            id = reply.id;
            FeedReplyList.ItemsSource = replys;
            reply.showreplyRows = false;
            replys.Add(reply);
            GetReplys(false);
            UIHelper.HideProgressBar();
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

        async void GetReplys(bool isRefresh)
        {
            UIHelper.ShowProgressBar();
            int page = isRefresh ? 1 : ++this.page;
            JsonArray array = (JsonArray)await DataHelper.GetData(true, DataType.GetReplyReplies, page, page > 1 ? $"&lastItem={lastItem}" : string.Empty);
            if (array != null && array.Count > 0)
                if (isRefresh)
                {
                    VScrollViewer?.ChangeView(null, 0, null);
                    var d = (from a in replys
                             from b in array
                             where a.id == b.GetObject()["id"].GetNumber()
                             select a).ToArray();
                    foreach (var item in d)
                        replys.Remove(item);
                    for (int i = 0; i < array.Count; i++)
                        replys.Insert(i + 1, new FeedReplyViewModel(array[i]));
                }
                else
                {
                    foreach (var item in array)
                        replys.Add(new FeedReplyViewModel(item, false));
                    lastItem = array.Last().GetObject()["id"].GetNumber();
                }
            else if (!isRefresh) this.page--;
            UIHelper.HideProgressBar();
        }

        private void VScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (VScrollViewer.VerticalOffset == VScrollViewer.ScrollableHeight)
                GetReplys(false);
        }

        private void FeedReplyList_RefreshRequested(object sender, EventArgs e) => GetReplys(true);
    }
}
