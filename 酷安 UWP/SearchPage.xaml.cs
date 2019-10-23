using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;
using 酷安_UWP.UsersPage;

namespace 酷安_UWP
{
    public sealed partial class SearchPage : Page
    {
        MainPage mainPage;
        int[] pages = new int[2];
        string[] lastItems = new string[2];
        public SearchPage()
        {
            this.InitializeComponent();
            AppsResultList.ItemsSource = new ObservableCollection<AppInfo>();
            FeedList.ItemsSource = new ObservableCollection<Feed>();
            UserList.ItemsSource = new ObservableCollection<UserInfo>();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is MainPage page)
            {
                mainPage = page;
                BackButton.Visibility = Visibility.Collapsed;
            }
            if (e.Parameter is object[] vs)
            {
                mainPage = vs[1] as MainPage;
                SearchTypeComboBox.Visibility = Visibility.Collapsed;
                DetailPivot.SelectedIndex = 2;
                DetailPivot.IsLocked = true;
                string appSearchLink = vs[0] as string;
                if (string.IsNullOrEmpty(appSearchLink)) return;
                SearchApps(appSearchLink);
            }
        }
        #region SearchFeed
        async void SearchFeeds(string keyWord)
        {
            mainPage.ActiveProgressRing();
            string feedType = string.Empty;
            string sortType = string.Empty;
            switch (SearchFeedTypeComboBox.SelectedIndex)
            {
                case 0:
                    feedType = "all";
                    break;
                case 1:
                    feedType = "feed";
                    break;
                case 2:
                    feedType = "feedArticle";
                    break;
                case 3:
                    feedType = "rating";
                    break;
                case 4:
                    feedType = "picture";
                    break;
                case 5:
                    feedType = "question";
                    break;
                case 6:
                    feedType = "answer";
                    break;
                case 7:
                    feedType = "video";
                    break;
                case 8:
                    feedType = "ershou";
                    break;
                case 9:
                    feedType = "vote";
                    break;
            }
            switch (SearchFeedSortTypeComboBox.SelectedIndex)
            {
                case 0:
                    sortType = "default";
                    break;
                case 1:
                    sortType = "hot";
                    break;
                case 2:
                    sortType = "reply";
                    break;
            }
            string r = await CoolApkSDK.GetCoolApkMessage($"/search?type=feed&feedType={feedType}&sort={sortType}&searchValue={keyWord}&page={++pages[0]}{(pages[0] > 1 ? "&lastItem=" + lastItems[0] : string.Empty)}&showAnonymous=-1");
            JArray Root = JObject.Parse(r)["data"] as JArray;
            ObservableCollection<Feed> FeedsCollection = FeedList.ItemsSource as ObservableCollection<Feed>;
            if (pages[0] == 1) FeedsCollection.Clear();
            if (!(Root is null) && Root.Count != 0)
            {
                lastItems[0] = Root.Last["id"].ToString();
                foreach (JObject i in Root)
                    FeedsCollection.Add(new Feed(i));
            }
            else pages[0]--;
            mainPage.DeactiveProgressRing();
        }

        private async void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            string s = (element.Tag as Feed).GetValue("extra_url2");
            if (s.IndexOf("/u/") == 0)
                mainPage.Frame.Navigate(typeof(UserPage), new object[] { await CoolApkSDK.GetUserIDByName(s.Replace("/u/", string.Empty)), mainPage });
            if (s.IndexOf("http") == 0)
                await Launcher.LaunchUriAsync(new Uri(s));
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

        private async void MarkdownTextBlock_LinkClicked(object sender, Microsoft.Toolkit.Uwp.UI.Controls.LinkClickedEventArgs e)
        {
            if (e.Link.IndexOf("/u/") == 0)
                mainPage.Frame.Navigate(typeof(UserPage), new object[] { await CoolApkSDK.GetUserIDByName(e.Link.Replace("/u/", string.Empty)), mainPage });
            if (e.Link.IndexOf("http") == 0)
                await Launcher.LaunchUriAsync(new Uri(e.Link));
        }

        private void Button_Click(object sender, RoutedEventArgs e) => mainPage.Frame.Navigate(typeof(UserPage), new object[] { (sender as FrameworkElement).Tag as string, mainPage });
        #endregion
        #region SearchUser
        async void SearchUsers(string keyWord)
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

            mainPage.ActiveProgressRing();
            ObservableCollection<UserInfo> infos = UserList.ItemsSource as ObservableCollection<UserInfo>;
            string r = await CoolApkSDK.GetCoolApkMessage($"/search?type=user&searchValue={keyWord}&page={++pages[1]}{(pages[1] > 1 ? "&lastItem=" + lastItems[1] : string.Empty)}&showAnonymous=-1");
            JArray array = JObject.Parse(r)["data"] as JArray;
            if (!(array is null) && array.Count > 0)
            {
                lastItems[1] = array.Last["uid"].ToString();
                if (infos.Count > 0)
                {
                    var d = (from a in infos
                             from b in array
                             where a.Uid == b["uid"].ToString()
                             select a).ToArray();
                    foreach (var item in d)
                        infos.Remove(item);
                }
                for (int i = 0; i < array.Count; i++)
                {
                    JToken t = array[i];
                    infos.Add(new UserInfo
                    {
                        Uid = t["uid"].ToString(),
                        UserName = t["username"].ToString(),
                        FansNum = t["fans"].ToString(),
                        FollowNum = t["follow"].ToString(),
                        Bio = t["bio"].ToString(),
                        LoginTime = Process.ConvertTime(t["logintime"].ToString()) + "活跃",
                        UserAvatar = getImage(t["userSmallAvatar"].ToString())
                    });
                }
            }
            else pages[1]--;
            mainPage.DeactiveProgressRing();
        }
        private void ListViewItem_Tapped(object sender, TappedRoutedEventArgs e) => mainPage.Frame.Navigate(typeof(UserPage), new object[] { (sender as FrameworkElement).Tag as string, mainPage });
        #endregion
        #region SearchApp
        private async void SearchApps(string keyWord)
        {
            mainPage.ActiveProgressRing();
            ObservableCollection<AppInfo> infos = AppsResultList.ItemsSource as ObservableCollection<AppInfo>;
            infos.Clear();
            string str = await new HttpClient().GetStringAsync(new Uri("https://www.coolapk.com/search?q=" + keyWord));
            string body = Regex.Split(str, @"<div class=""left_nav"">")[1];
            body = Regex.Split(body, @"<div class=""panel-footer ex-card-footer text-center"">")[0];
            //&nbsp;处理
            body = body.Replace("&nbsp;", " ");
            string[] bodylist = Regex.Split(body, @"<a href=""");
            string[] bodys = Regex.Split(body, @"\n");
            for (int i = 0; i < bodylist.Length - 1; i++)
            {
                infos.Add(new AppInfo
                {
                    GridTag = bodys[i * 15 + 5].Split('"')[1],
                    Icon = new BitmapImage(new Uri(bodys[i * 15 + 5 + 3].Split('"')[3], UriKind.RelativeOrAbsolute)),
                    AppName = bodys[i * 15 + 5 + 5].Split('>')[1].Split('<')[0],
                    Size = bodys[i * 15 + 5 + 6].Split('>')[1].Split('<')[0],
                    DownloadNum = bodys[i * 15 + 5 + 7].Split('>')[1].Split('<')[0]
                });
            }
            mainPage.DeactiveProgressRing();
        }

        private void AppResultList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AppsResultList.SelectedIndex == -1) return;
            mainPage.Frame.Navigate(typeof(AppPage), new object[] { "https://www.coolapk.com" + (AppsResultList.Items[AppsResultList.SelectedIndex] as AppInfo).GridTag, mainPage });
            AppsResultList.SelectedIndex = -1;
        }
        #endregion
        private void SearchTextBox_KeyUp(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
                StartSearch();
        }

        void StartSearch()
        {
            if (string.IsNullOrEmpty(SearchText.Text))
                DetailPivot.Visibility = Visibility.Collapsed;
            else
            {
                switch (DetailPivot.SelectedIndex)
                {
                    case 0:
                        SearchFeeds(SearchText.Text);
                        break;
                    case 1:
                        SearchUsers(SearchText.Text);
                        break;
                    case 2:
                        SearchApps(SearchText.Text);
                        break;
                }
                DetailPivot.Visibility = Visibility.Visible;
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e) => StartSearch();

        private void BackButton_Click(object sender, RoutedEventArgs e) => Frame.GoBack();

        private void SearchTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SearchTypeComboBox.SelectedIndex != -1 && !(DetailPivot is null))
                DetailPivot.SelectedIndex = SearchTypeComboBox.SelectedIndex;
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (BackButton.Visibility == Visibility.Collapsed && e.NewSize.Width >= 640)
                BackButtonColumnDefinition.Width = new GridLength(0);
            else if (BackButton.Visibility == Visibility.Collapsed && e.NewSize.Width < 640)
                BackButtonColumnDefinition.Width = new GridLength(48);
        }

        private void DetailPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DetailPivot.SelectedIndex != -1 && !(SearchTypeComboBox is null))
                SearchTypeComboBox.SelectedIndex = DetailPivot.SelectedIndex;
            StartSearch();
        }

        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            ScrollViewer viewer = sender as ScrollViewer;
            if (!e.IsIntermediate)
                if (viewer.VerticalOffset == viewer.ScrollableHeight)
                    StartSearch();
        }

        private void SearchFeedTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            pages[0] = 0;
            lastItems[0] = string.Empty;
            StartSearch();
        }
    }
    class AppInfo
    {
        public ImageSource Icon { get; set; }
        public string GridTag { get; set; }
        public string AppName { get; set; }
        public string Size { get; set; }
        public string DownloadNum { get; set; }
    }
}
