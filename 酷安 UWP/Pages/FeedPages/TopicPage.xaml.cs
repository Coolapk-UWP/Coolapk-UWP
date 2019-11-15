using CoolapkUWP.Control.ViewModels;
using CoolapkUWP.Data;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.Data.Json;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace CoolapkUWP.Pages.FeedPages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class TopicPage : Page
    {
        string tag;
        int page;
        double firstItem;
        double lastItem;
        ObservableCollection<FeedViewModel> FeedsCollection = new ObservableCollection<FeedViewModel>();

        public TopicPage()
        {
            this.InitializeComponent();
            listView.ItemsSource = FeedsCollection;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (tag != e.Parameter as string)
            {
                Tools.ShowProgressBar();
                tag = e.Parameter as string;
                page = 0;
                firstItem = lastItem = 0;
                FeedsCollection.Clear();
                DetailGrid.DataContext = null;
                TitleBar.Title = tag;
                LoadTagDetail();
                LoadFeeds();
                Tools.HideProgressBar();
            }
        }

        public async void LoadTagDetail()
        {
            string r = await Tools.GetJson($"/topic/newTagDetail?tag={tag}");
            JsonObject detail = Tools.GetJSonObject(r);
            if (!(detail is null))
            {
                TitleBar.Title = detail["title"].GetString();
                DetailGrid.DataContext = new
                {
                    Logo = await ImageCache.GetImage(ImageType.Icon, detail["logo"].GetString()),
                    Title = detail["title"].GetString(),
                    FollowNum = detail.TryGetValue("follownum", out IJsonValue t) ? t.GetNumber() : detail["follow_num"].GetNumber(),
                    CommentNum = detail.TryGetValue("commentnum", out IJsonValue tt) ? tt.GetNumber() : detail["rating_total_num"].GetNumber(),
                    Description = detail["description"].GetString(),
                };
            }
        }

        async void LoadFeeds(int p = -1)
        {
            string sortType = string.Empty;
            switch (FeedTypeComboBox.SelectedIndex)
            {
                case 0:
                    sortType = "lastupdate_desc";
                    break;
                case 1:
                    sortType = "dateline_desc";
                    break;
                case 2:
                    sortType = "popular";
                    break;
            }
            string r = await Tools.GetJson($"/topic/tagFeedList?tag={tag}&page={(p == -1 ? ++page : p)}{(firstItem == 0 ? string.Empty : $"&firstItem={firstItem}")}{(lastItem == 0 ? string.Empty : $"&lastItem={lastItem}")}&listType={sortType}&blockStatus=0");
            JsonArray Root = Tools.GetDataArray(r);
            if (!(Root is null) && Root.Count != 0)
            {
                if (page == 1)
                {
                    firstItem = Root.First().GetObject()["id"].GetNumber();
                    lastItem = Root.Last().GetObject()["id"].GetNumber();
                }
                foreach (var i in Root)
                    FeedsCollection.Add(new FeedViewModel(i));
            }
            else page--;
        }

        void Refresh()
        {
            if (FeedsCollection.Count == 0) return;
            Tools.ShowProgressBar();
            LoadTagDetail();
            LoadFeeds(1);
            Tools.HideProgressBar();
        }

        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (!e.IsIntermediate)
            {
                if (VScrollViewer.VerticalOffset == 0)
                {
                    Refresh();
                    VScrollViewer.ChangeView(null, 20, null);
                    refreshText.Visibility = Visibility.Collapsed;
                }
                else if (VScrollViewer.VerticalOffset == VScrollViewer.ScrollableHeight)
                    LoadFeeds();
            }
            else refreshText.Visibility = Visibility.Visible;
        }

        private void FeedTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tag is null) return;
            firstItem = lastItem = 0;
            page = 0;
            FeedsCollection.Clear();
            LoadFeeds();
        }

        private void TitleBar_BackButtonClick(object sender, RoutedEventArgs e) => Frame.GoBack();
    }
}
