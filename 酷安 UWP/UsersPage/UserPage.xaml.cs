using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
        static string uid;
        static int page = 0;
        static string firstItem = string.Empty, lastItem = string.Empty;
        static ObservableCollection<Feed> FeedsCollection = new ObservableCollection<Feed>();
        public UserPage()
        {
            this.InitializeComponent();
            listView.ItemsSource = FeedsCollection;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            mainPage = ((object[])e.Parameter)[1] as MainPage;
            if (uid != (string)((object[])e.Parameter)[0])
            {
                uid = (string)((object[])e.Parameter)[0];
                mainPage.ActiveProgressRing();
                FeedsCollection.Clear();
                page = 0;
                firstItem = lastItem = string.Empty;
                LoadProfile();
                ReadNextPageFeeds();
                mainPage.DeactiveProgressRing();
            }
            else Refresh();
            VScrollViewer.ChangeView(null, 20, null);
        }

        public async void LoadProfile()
        {
            JObject detail = await CoolApkSDK.GetUserProfileByID(uid);
            if (!(detail is null))
            {
                UserDetailGrid.DataContext = new
                {
                    UserFace = new BitmapImage(new Uri(detail["userAvatar"].ToString())) as ImageSource,
                    UserName = detail["username"].ToString(),
                    FollowNum = detail["follow"].ToString(),
                    FansNum = detail["fans"].ToString(),
                    Level = detail["level"].ToString(),
                    bio = detail["bio"].ToString(),
                    Backgeound = new ImageBrush { ImageSource = new BitmapImage(new Uri(detail["cover"].ToString())) },
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
            JArray Root = await CoolApkSDK.GetFeedListByID(uid, ++page, firstItem, lastItem);
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
            => mainPage.Frame.Navigate(typeof(FeedDetailPage), new object[] { ((sender as FrameworkElement).Tag as Feed).GetValue("id"), mainPage, "动态", null });

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
                    VScrollViewer.ChangeView(null, 20, null);
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
            JArray Root = await CoolApkSDK.GetFeedListByID(uid, 1, firstItem, lastItem);
            if (!(Root is null) && Root.Count != 0)
            {
                firstItem = Root.First["id"].ToString();
                for (int i = 0; i < Root.Count; i++)
                    FeedsCollection.Insert(i, new Feed((JObject)Root[i]));
            }
            mainPage.DeactiveProgressRing();
        }

        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (!e.IsIntermediate)
                if (VScrollViewer.VerticalOffset == 0)
                {
                    Refresh();
                    VScrollViewer.ChangeView(null, 20, null);
                }
                else if (VScrollViewer.VerticalOffset == VScrollViewer.ScrollableHeight)
                    ReadNextPageFeeds();
        }
    }
}
