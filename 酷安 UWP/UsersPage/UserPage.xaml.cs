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
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace 酷安_UWP
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class UserPage : Page
    {
        MainPage mainPage;
        string uid;
        int page = 0;
        string firstItem = string.Empty, lastItem = string.Empty;
        ObservableCollection<Feed> FeedsCollection = new ObservableCollection<Feed>();
        public UserPage()
        {
            this.InitializeComponent();
            listView.ItemsSource = FeedsCollection;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            mainPage = ((object[])e.Parameter)[1] as MainPage;
            uid = (string)((object[])e.Parameter)[0];
            mainPage.ActiveProgressRing();
            if (!string.IsNullOrEmpty(uid))
                LoadProfile();
            //FeedsCollection.Add(((object[])e.Parameter)[2] as Feed);
            if (FeedsCollection.Count == 0)
                ReadNextPageFeeds();
            mainPage.DeactiveProgressRing();
        }

        public async void LoadProfile()
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
            JObject detail = await CoolApkSDK.GetUserProfileByID(uid);
            if (!(detail is null))
            {
                UserDetailGrid.DataContext = new
                {
                    UserFace = getImage(detail["userAvatar"].ToString()),
                    UserName = detail["username"].ToString(),
                    FollowNum = detail["follow"].ToString(),
                    FansNum = detail["fans"].ToString(),
                    Level = detail["level"].ToString(),
                    bio = detail["bio"].ToString(),
                    Backgeound = new ImageBrush { ImageSource = getImage(detail["cover"].ToString()), Stretch = Stretch.UniformToFill },
                    verify_title = detail["verify_title"].ToString(),
                    gender = int.Parse(detail["gender"].ToString()) == 1 ? "♂" : (int.Parse(detail["gender"].ToString()) == 0 ? "♀" : string.Empty),
                    city = $"{detail["province"].ToString()} {detail["city"].ToString()}",
                    astro = detail["astro"].ToString(),
                    logintime = $"{Feed.ConvertTime(detail["logintime"].ToString())}活跃"
                };
                TitleTextBlock.Text = detail["username"].ToString();
                ListPivot.DataContext = new { FeedNum = detail["feed"].ToString() };
            }
            else
            {
                Frame.GoBack();
                await new MessageDialog("用户不存在").ShowAsync();
            }
        }

        async void ReadNextPageFeeds()
        {
            JArray Root = await CoolApkSDK.GetFeedListByID(uid, $"{++page}", firstItem, lastItem);
            if (!(Root is null) && Root.Count != 0)
            {
                firstItem = Root.First["id"].ToString();
                lastItem = Root.Last["id"].ToString();
                foreach (JObject i in Root)
                    FeedsCollection.Add(new Feed(i));
            }
            else page--;
        }

        private void FeedListViewItem_Tapped(object sender, TappedRoutedEventArgs e)
            => mainPage.Frame.Navigate(typeof(FeedDetailPage), new object[] { ((sender as FrameworkElement).Tag as Feed).GetValue("id"), mainPage, string.Empty, null });

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

        async void Refresh()
        {
            mainPage.ActiveProgressRing();
            LoadProfile();
            JArray Root = await CoolApkSDK.GetFeedListByID(uid, "1", firstItem, lastItem);
            if (!(Root is null) && Root.Count != 0)
            {
                firstItem = Root.First["id"].ToString();
                for (int i = 0; i < Root.Count; i++)
                    FeedsCollection.Insert(i, new Feed((JObject)Root[i]));
            }
            mainPage.DeactiveProgressRing();
        }

        private async void MarkdownTextBlock_LinkClicked(object sender, Microsoft.Toolkit.Uwp.UI.Controls.LinkClickedEventArgs e)
        {
            if (e.Link.IndexOf("/u/") == 0)
                mainPage.Frame.Navigate(typeof(UserPage), new object[] { await CoolApkSDK.GetUserIDByName(e.Link.Replace("/u/", string.Empty)), mainPage });
            if (e.Link.IndexOf("http") == 0)
                await Launcher.LaunchUriAsync(new Uri(e.Link));
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
                    ReadNextPageFeeds();
            }
            else refreshText.Visibility = Visibility.Visible;
        }
    }
    public class TemplateSelector2 : DataTemplateSelector
    {
        public DataTemplate DataTemplate1 { get; set; }
        public DataTemplate DataTemplate2 { get; set; }
        protected override DataTemplate SelectTemplateCore(object item)
        {
            Feed feed = item as Feed;
            switch (feed.GetValue("feedType"))
            {
                case "feed":
                    return DataTemplate1;
                case "feedArticle":
                case "answer":
                case "question":
                    return DataTemplate2;
                default:
                    return DataTemplate1;
            }
        }
    }
}
