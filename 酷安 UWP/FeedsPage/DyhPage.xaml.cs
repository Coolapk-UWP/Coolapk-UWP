using Newtonsoft.Json.Linq;
using System;
using System.Collections.ObjectModel;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
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
    public sealed partial class DyhPage : Page
    {
        Style listviewStyle { get; set; }
        MainPage mainPage;
        string id;
        int[] page = new int[2];
        string[] firstItem = new string[2];
        string[] lastItem = new string[2];
        ObservableCollection<Feed>[] FeedsCollection = new ObservableCollection<Feed>[2];

        public DyhPage()
        {
            this.InitializeComponent();
            if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile") listviewStyle = Application.Current.Resources["ListViewStyle2Mobile"] as Style;
            else listviewStyle = Application.Current.Resources["ListViewStyle2Desktop"] as Style;
            FeedsCollection[0] = new ObservableCollection<Feed>();
            FeedsCollection[1] = new ObservableCollection<Feed>();
            listView.ItemsSource = FeedsCollection[0];
            listView2.ItemsSource = FeedsCollection[1];
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            mainPage = ((object[])e.Parameter)[1] as MainPage;
            id = (string)((object[])e.Parameter)[0];
            mainPage.ActiveProgressRing();
            LoadDyhDetail();
            LoadFeeds();
            mainPage.DeactiveProgressRing();
        }
        public async void LoadDyhDetail()
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

            string r = await Tools.GetCoolApkMessage($"/dyh/detail?dyhId={id}");
            JObject detail = JObject.Parse(r)["data"] as JObject;
            if (!(detail is null))
            {
                if (detail["is_open_discuss"].ToString() == "1") MainPivot.IsLocked = false;
                TitleTextBlock.Text = detail["title"].ToString();
                DetailGrid.DataContext = new
                {
                    Logo = getImage(detail["logo"].ToString()),
                    Title = detail["title"].ToString(),
                    Description = detail["description"].ToString(),
                    FollowNum = detail["follownum"].ToString(),
                    Uid = detail["uid"].ToString(),
                    UserName = detail["username"].ToString(),
                    UserAvatar = getImage(detail["userAvatar"].ToString())
                };
            }
        }

        async void LoadFeeds(int p = -1)
        {
            string r = await Tools.GetCoolApkMessage($"/dyhArticle/list?dyhId={id}&type={(MainPivot.SelectedIndex == 0 ? "all" : "square")}&page={(p == -1 ? ++page[MainPivot.SelectedIndex] : p)}{(string.IsNullOrEmpty(firstItem[MainPivot.SelectedIndex]) ? string.Empty : $"&firstItem={firstItem[MainPivot.SelectedIndex]}")}{(string.IsNullOrEmpty(lastItem[MainPivot.SelectedIndex]) ? string.Empty : $"&lastItem={lastItem[MainPivot.SelectedIndex]}")}");
            JArray Root = JObject.Parse(r)["data"] as JArray;
            if (!(Root is null) && Root.Count != 0)
            {
                if (page[MainPivot.SelectedIndex] == 1)
                {
                    firstItem[MainPivot.SelectedIndex] = Root.First["articleId"].ToString();
                    lastItem[MainPivot.SelectedIndex] = Root.Last["articleId"].ToString();
                }
                foreach (JObject i in Root)
                    FeedsCollection[MainPivot.SelectedIndex].Add(new Feed(i));
            }
            else page[MainPivot.SelectedIndex]--;
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
            //LoadDyhDetail();
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

        private void MainPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(id))
                Refresh();
        }
    }
}
