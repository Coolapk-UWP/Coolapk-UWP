using CoolapkUWP.Control.ViewModels;
using CoolapkUWP.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace CoolapkUWP.Control
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class ReplyDialogPresenter : Page
    {
        double id;
        int page;
        double lastItem;
        ObservableCollection<FeedReplyViewModel> replys = new ObservableCollection<FeedReplyViewModel>();

        public ReplyDialogPresenter(object o, Popup popup)
        {
            this.InitializeComponent();
            Tools.ShowProgressBar();
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
            Tools.HideProgressBar();
        }

        private void WindowSizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            Height = e.Size.Height;
            Width = e.Size.Width;
        }

        async void GetReplys(bool isRefresh)
        {
            Tools.ShowProgressBar();
            int page = isRefresh ? 1 : ++this.page;
            string result = await Tools.GetJson($"/feed/replyList?id={id}&listType=&page={page}{(page > 1 ? $"&lastItem={lastItem}" : string.Empty)}&discussMode=0&feedType=feed_reply&blockStatus=0&fromFeedAuthor=0");
            JsonArray array = Tools.GetDataArray(result);
            if (array != null && array.Count > 0)
                if (isRefresh)
                {
                    VScrollViewer.ChangeView(null, 0, null);
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
            Tools.HideProgressBar();
        }

        private void VScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (VScrollViewer.VerticalOffset == 0)
                GetReplys(true);
            else if (VScrollViewer.VerticalOffset == VScrollViewer.ScrollableHeight)
                GetReplys(false);
        }
    }
}
