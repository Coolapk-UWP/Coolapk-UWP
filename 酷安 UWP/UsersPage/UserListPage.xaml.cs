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
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using 酷安_UWP;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace 酷安_UWP.UsersPage
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class UserListPage : Page
    {
        MainPage mainPage;
        string uid;
        bool isFollowList;
        int page = 1;
        string firstItem, lastItem;
        ObservableCollection<UserInfo> infos = new ObservableCollection<UserInfo>();
        public UserListPage()
        {
            this.InitializeComponent();
            UserList.ItemsSource = infos;
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            object[] vs = e.Parameter as object[];
            mainPage = vs[0] as MainPage;
            uid = vs[1] as string;
            isFollowList = (bool)vs[2];
            title.Text = vs[3] as string + "的" + (isFollowList ? "关注" : "粉丝");
            LoadList(1);
        }
        async void LoadList(int p = -1)
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
            if (p == 1)
            {
                string url = isFollowList ? $"/user/followList?uid={uid}&page={p}" : $"/user/fansList?uid={uid}&page={p}";
                string r = await CoolApkSDK.GetCoolApkMessage(url);
                JArray array = JObject.Parse(r)["data"] as JArray;
                if (!(array is null) && array.Count > 0)
                {
                    firstItem = array.First["fuid"].ToString();
                    lastItem = array.Last["fuid"].ToString();
                    if (infos.Count > 0)
                    {
                        var d = (from a in infos
                                 from b in array
                                 where a.Uid == b[isFollowList ? "fuid" : "uid"].ToString()
                                 select a).ToArray();
                        foreach (var item in d)
                            infos.Remove(item);
                    }
                    for (int i = 0; i < array.Count; i++)
                    {
                        JToken t = isFollowList ? array[i]["fUserInfo"] : array[i]["userInfo"];
                        infos.Insert(i, new UserInfo
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
            }
            else if (p == -1)
            {
                string url = isFollowList ? $"/user/followList?uid={uid}&page={++page}&firstItem={firstItem}&lastItem={lastItem}" : $"/user/fansList?uid={uid}&page={++page}&firstItem={firstItem}&lastItem={lastItem}";
                string r = await CoolApkSDK.GetCoolApkMessage(url);
                JArray array = JObject.Parse(r)["data"] as JArray;
                if (!(array is null) && array.Count > 0)
                {
                    firstItem = array.Last["fuid"].ToString();
                    for (int i = 0; i < array.Count; i++)
                    {
                        JToken t = isFollowList ? array[i]["fUserInfo"] : array[i]["userInfo"];
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
                else page--;
            }
            mainPage.DeactiveProgressRing();
        }

        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (!e.IsIntermediate)
            {
                if (VScrollViewer.VerticalOffset == 0)
                {
                    LoadList(1);
                    VScrollViewer.ChangeView(null, 20, null);
                    refreshText.Visibility = Visibility.Collapsed;
                }
                else if (VScrollViewer.VerticalOffset == VScrollViewer.ScrollableHeight)
                    LoadList();
            }
            else refreshText.Visibility = Visibility.Visible;
        }

        private void ListViewItem_Tapped(object sender, TappedRoutedEventArgs e) => mainPage.Frame.Navigate(typeof(UserPage), new object[] { (sender as FrameworkElement).Tag as string, mainPage });

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as FrameworkElement).Tag as string)
            {
                case "Refresh":
                    LoadList(1);
                    VScrollViewer.ChangeView(null, 20, null);
                    break;
                case "back":
                    Frame.GoBack();
                    break;
            }
        }

    }
    class UserInfo
    {
        public string Uid { get; set; }
        public string UserName { get; set; }
        public string FollowNum { get; set; }
        public string FansNum { get; set; }
        public string LoginTime { get; set; }
        public string Bio { get; set; }
        public ImageSource UserAvatar { get; set; }
    }
}
