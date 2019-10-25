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
using 酷安_UWP.UsersPage;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace 酷安_UWP
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class UserPage : Page
    {
        Style listviewStyle { get; set; }
        MainPage mainPage;
        string uid;
        int page = 0;
        string firstItem = string.Empty, lastItem = string.Empty;
        ObservableCollection<Feed> FeedsCollection = new ObservableCollection<Feed>();
        public UserPage()
        {
            this.InitializeComponent();
            listView.ItemsSource = FeedsCollection;
            if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile") listviewStyle = Application.Current.Resources["ListViewStyle2Mobile"] as Style;
            else listviewStyle = Application.Current.Resources["ListViewStyle2Desktop"] as Style;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            mainPage = ((object[])e.Parameter)[1] as MainPage;
            uid = (string)((object[])e.Parameter)[0];
            mainPage.ActiveProgressRing();
            LoadProfile();
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
            JObject detail = await Tools.GetUserProfileByID(uid);
            if (!(detail is null))
            {
                UserDetailGrid.DataContext = new
                {
                    UserFace = getImage(detail["userAvatar"].ToString()),
                    UserFaceUrl = detail["userAvatar"].ToString(),
                    UserName = detail["username"].ToString(),
                    FollowNum = detail["follow"].ToString(),
                    FansNum = detail["fans"].ToString(),
                    Level = detail["level"].ToString(),
                    bio = detail["bio"].ToString(),
                    Background = new ImageBrush { ImageSource = getImage(detail["cover"].ToString()), Stretch = Stretch.UniformToFill },
                    BackgroundUrl = detail["cover"].ToString(),
                    verify_title = detail["verify_title"].ToString(),
                    gender = int.Parse(detail["gender"].ToString()) == 1 ? "♂" : (int.Parse(detail["gender"].ToString()) == 0 ? "♀" : string.Empty),
                    city = $"{detail["province"].ToString()} {detail["city"].ToString()}",
                    astro = detail["astro"].ToString(),
                    logintime = $"{Tools.ConvertTime(detail["logintime"].ToString())}活跃"
                };
                TitleTextBlock.Text = detail["username"].ToString();
                ListHeader.DataContext = new { FeedNum = detail["feed"].ToString() };
            }
            else
            {
                Frame.GoBack();
                await new MessageDialog("用户不存在").ShowAsync();
            }
        }

        async void ReadNextPageFeeds()
        {
            JArray Root = await Tools.GetFeedListByID(uid, $"{++page}", firstItem, lastItem);
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
                case "2":
                    mainPage.Frame.Navigate(typeof(UserListPage), new object[] { mainPage, uid, true, TitleTextBlock.Text });
                    break;
                case "3":
                    mainPage.Frame.Navigate(typeof(UserListPage), new object[] { mainPage, uid, false, TitleTextBlock.Text });
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
            JArray Root = await Tools.GetFeedListByID(uid, "1", firstItem, lastItem);
            if (!(Root is null) && Root.Count != 0)
            {
                firstItem = Root.First["id"].ToString();
                for (int i = 0; i < Root.Count; i++)
                    FeedsCollection.Insert(i, new Feed((JObject)Root[i]));
            }
            mainPage.DeactiveProgressRing();
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
                    ReadNextPageFeeds();
            }
            else refreshText.Visibility = Visibility.Visible;
        }
    }
}
