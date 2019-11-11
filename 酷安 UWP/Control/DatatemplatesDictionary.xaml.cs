using CoolapkUWP.Control.ViewModels;
using CoolapkUWP.Data;
using CoolapkUWP.Pages.FeedPages;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace CoolapkUWP.Control
{
    public partial class DatatemplatesDictionary
    {
        public DatatemplatesDictionary() => InitializeComponent();

        private void OnTapped(object sender, TappedRoutedEventArgs e)
            => Tools.OpenLink((sender as FrameworkElement).Tag as string);

        private void MarkdownTextBlock_LinkClicked(object sender, Microsoft.Toolkit.Uwp.UI.Controls.LinkClickedEventArgs e)
        {
            if (e.Link.Replace("mailto:", string.Empty).IndexOf("http://image.coolapk.com") == 0) Tools.rootPage.ShowImage(e.Link.Replace("mailto:", string.Empty));
            else Tools.OpenLink(e.Link);
        }

        private void PicA_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender is GridView view)
            {
                if (view.SelectedIndex > -1 && view.Tag is string[] ss)
                    Tools.rootPage.ShowImage(ss[view.SelectedIndex].Remove(ss[view.SelectedIndex].Length - 6));
                view.SelectedIndex = -1;
            }
        }

        private void ListViewItem_Tapped_1(object sender, TappedRoutedEventArgs e)
            => Tools.rootPage.Navigate(typeof(FeedDetailPage), (sender as FrameworkElement).Tag.ToString());

        private void Image_Tapped(object sender, TappedRoutedEventArgs e) => Tools.rootPage.ShowImage((sender as FrameworkElement).Tag as string);

        private void replyRowsItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if ((sender as ListViewItem).ContextFlyout is Flyout flyout)
                flyout.ShowAt(sender as ListViewItem);
        }

        class FlyoutData
        {
            public double id;
            public int page;
            public double lastItem;
            public ScrollViewer VScrollViewer;
            public ObservableCollection<FeedReplyViewModel> replys = new ObservableCollection<FeedReplyViewModel>();
        }

        private void Flyout_Opened(object sender, object e)
        {
            Tools.rootPage.ShowProgressBar();

            Flyout flyout = sender as Flyout;
            Grid mainGrid = (flyout.Content as Grid);
            FeedReplyViewModel feedReplyViewModel = (mainGrid.Tag as FeedReplyViewModel);
            if (feedReplyViewModel != null)
            {
                TitleBar titleBar = mainGrid.FindName("TitleBar") as TitleBar;
                FlyoutData data = new FlyoutData { id = feedReplyViewModel.id };
                titleBar.TitleHeight = 48;
                titleBar.BackButtonClick += (s, ee) => flyout.Hide();
                titleBar.Title = $"回复({feedReplyViewModel.replyTotal})";
                titleBar.RefreshButtonClick += (s, ee) => GetReplys((s as FrameworkElement).Tag as FlyoutData, true);
                titleBar.RefreshButtonTag = data;
                ScrollViewer scrollViewer = (mainGrid.FindName("ScrollViewer") as ScrollViewer);
                scrollViewer.Tag = data;
                scrollViewer.ViewChanged += (s, ee) =>
                {
                    ScrollViewer VScrollViewer = s as ScrollViewer;
                    if (VScrollViewer.VerticalOffset == 0)
                        GetReplys(VScrollViewer.Tag as FlyoutData, true);
                    else if (VScrollViewer.VerticalOffset == VScrollViewer.ScrollableHeight)
                        GetReplys(VScrollViewer.Tag as FlyoutData, false);
                };
                data.VScrollViewer = scrollViewer;
                ListView listView = scrollViewer.FindName("FeedReplyList") as ListView;
                listView.ItemsSource = data.replys;
                feedReplyViewModel.showreplyRows = false;
                data.replys.Add(feedReplyViewModel);
                mainGrid.Tag = data;
            }
            if (mainGrid.Tag is FlyoutData d && d.page == 0)
                GetReplys(d, false);
            Tools.rootPage.HideProgressBar();
        }

        async void GetReplys(FlyoutData data, bool isRefresh)
        {
            Tools.rootPage.ShowProgressBar();
            int page = isRefresh ? 1 : ++data.page;
            string result = await Tools.GetJson($"/feed/replyList?id={data.id}&listType=&page={page}{(page > 1 ? $"&lastItem={data.lastItem}" : string.Empty)}&discussMode=0&feedType=feed_reply&blockStatus=0&fromFeedAuthor=0");
            JsonArray array = Tools.GetDataArray(result);
            if (array != null && array.Count > 0)
                if (isRefresh)
                {
                    data.VScrollViewer.ChangeView(null, 0, null);
                    var d = (from a in data.replys
                             from b in array
                             where a.id == b.GetObject()["id"].GetNumber()
                             select a).ToArray();
                    foreach (var item in d)
                        data.replys.Remove(item);
                    for (int i = 0; i < array.Count; i++)
                        data.replys.Insert(i + 1, new FeedReplyViewModel(array[i]));
                }
                else
                {
                    foreach (var item in array)
                        data.replys.Add(new FeedReplyViewModel(item, false));
                    data.lastItem = array.Last().GetObject()["id"].GetNumber();
                }
            else if (!isRefresh) data.page--;
            Tools.rootPage.HideProgressBar();
        }
    }
}
