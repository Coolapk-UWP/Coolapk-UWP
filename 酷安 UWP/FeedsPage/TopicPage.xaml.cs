using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace 酷安_UWP.FeedsPage
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class TopicPage : Page
    {
        Style listviewStyle { get; set; }
        MainPage mainPage;
        string tag;
        int page;
        string firstItem;
        string lastItem;
        ObservableCollection<Feed> FeedsCollection = new ObservableCollection<Feed>();

        public TopicPage()
        {
            this.InitializeComponent();
            if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile") listviewStyle = Application.Current.Resources["ListViewStyle2Mobile"] as Style;
            else listviewStyle = Application.Current.Resources["ListViewStyle2Desktop"] as Style;
            listView.ItemsSource = FeedsCollection;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            mainPage = ((object[])e.Parameter)[1] as MainPage;
            tag = (string)((object[])e.Parameter)[0];
            mainPage.ActiveProgressRing();
            TitleTextBlock.Text = tag;
            LoadTagDetail();
            LoadFeeds();
            mainPage.DeactiveProgressRing();
        }

        public async void LoadTagDetail()
        {
            ImageSource getImage(string uri)
            {
                ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
                if (Convert.ToBoolean(localSettings.Values["IsNoPicsMode"]))
                {
                    if (Convert.ToBoolean(localSettings.Values["IsDarkMode"]))
                        return new BitmapImage(new Uri("ms-appx:/Assets/img_placeholder_night.png")) { DecodePixelHeight = 150, DecodePixelWidth = 150 };
                    else return new BitmapImage(new Uri("ms-appx:/Assets/img_placeholder.png")) { DecodePixelHeight = 150, DecodePixelWidth = 150 };
                }
                return new BitmapImage(new Uri(uri));
            }
            string r = await Tools.GetCoolApkMessage($"/topic/newTagDetail?tag={tag}");
            JObject detail = JObject.Parse(r)["data"] as JObject;
            if (!(detail is null))
            {
                TitleTextBlock.Text = detail["title"].ToString();
                DetailGrid.DataContext = new
                {
                    Logo = getImage(detail["logo"].ToString()),
                    Title = detail["title"].ToString(),
                    FollowNum = detail.TryGetValue("follownum", out JToken t) ? t.ToString() : detail["follow_num"].ToString(),
                    CommentNum = detail.TryGetValue("commentnum", out JToken tt) ? tt.ToString() : detail["rating_total_num"].ToString(),
                    Description = detail["description"].ToString(),
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
            string r = await Tools.GetCoolApkMessage($"/topic/tagFeedList?tag={tag}&page={(p == -1 ? ++page : p)}{(string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}")}{(string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}")}&listType={sortType}&blockStatus=0");
            JArray Root = JObject.Parse(r)["data"] as JArray;
            if (!(Root is null) && Root.Count != 0)
            {
                if (page == 1)
                    firstItem = Root.First["id"].ToString();
                lastItem = Root.Last["id"].ToString();
                foreach (JObject i in Root)
                    FeedsCollection.Add(new Feed(i));
            }
            else page--;
        }


        private void FeedListViewItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if ((sender as FrameworkElement).Tag is Feed)
                mainPage.Frame.Navigate(typeof(FeedDetailPage), new object[] { ((sender as FrameworkElement).Tag as Feed).GetValue("id"), mainPage, string.Empty, null });
            else if ((sender as FrameworkElement).Tag is Feed[])
            {
                var f = (sender as FrameworkElement).Tag as Feed[];
                if (!string.IsNullOrEmpty(f[0].jObject.ToString()))
                    mainPage.Frame.Navigate(typeof(FeedDetailPage), new object[] { f[0].GetValue("id"), mainPage, string.Empty, null });
            }
        }

        void Refresh()
        {
            mainPage.ActiveProgressRing();
            LoadTagDetail();
            LoadFeeds(1);
            mainPage.DeactiveProgressRing();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            switch (button.Tag as string)
            {
                case "0":
                    Frame.GoBack();
                    break;
                case "1":
                    Refresh();
                    break;
                default:
                    mainPage.Frame.Navigate(typeof(UserPage), new object[] { button.Tag as string, mainPage });
                    break;
            }
        }

        private void MarkdownTextBlock_LinkClicked(object sender, Microsoft.Toolkit.Uwp.UI.Controls.LinkClickedEventArgs e) => Tools.OpenLink(e.Link, mainPage);
        private void Grid_Tapped(object sender, TappedRoutedEventArgs e) => Tools.OpenLink((sender as FrameworkElement).Tag as string, mainPage);

        private void PicA_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender is GridView view)
            {
                if (view.SelectedIndex > -1 && view.Tag is string[] ss)
                    ShowImageControl.ShowImage(ss[view.SelectedIndex].Remove(ss[view.SelectedIndex].Length - 6));
                view.SelectedIndex = -1;
            }
            else if (sender is FrameworkElement fe)
            {
                if (fe != e.OriginalSource) return;
                if (fe.Tag is string s) ShowImageControl.ShowImage(s);
            }
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
            firstItem = lastItem = string.Empty;
            page = 0;
            FeedsCollection.Clear();
            LoadFeeds();
        }
    }
}
