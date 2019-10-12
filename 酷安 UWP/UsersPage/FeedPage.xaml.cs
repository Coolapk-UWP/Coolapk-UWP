using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
//using 酷安_UWP.CoolApk;

namespace 酷安_UWP
{
    public interface IReadNextPage { void ReadNextPage(); }
    public sealed partial class FeedPage : Page, IReadNextPage
    {
        MainPage mainPage;
        string uid;
        int page = 0;
        string firstItem = string.Empty, lastItem = string.Empty;

        public FeedPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            //将传过来的数据 类型转换一下
            uid = (string)((object[])e.Parameter)[0];
            mainPage = ((object[])e.Parameter)[1] as MainPage;
            LoadFeeds(uid);
        }

        public async void LoadFeeds(string uid)
        {
            mainPage.ActiveProgressRing();
            //绑定一个列表
            ObservableCollection<Feed> FeedsCollection = new ObservableCollection<Feed>();
            listView.ItemsSource = FeedsCollection;

            JArray Root = await CoolApkSDK.GetFeedListByID(uid, ++page, firstItem, lastItem);
            if (!(Root is null))
            {
                firstItem = Root.First["id"].ToString();
                lastItem = Root.Last["id"].ToString();
                foreach (JObject i in Root)
                    FeedsCollection.Add(new Feed(i));
            }
            else page--;
            mainPage.DeactiveProgressRing();
        }
        Uri blank = new Uri("about:blank");
        private void WebView_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            try
            {
                WebView view = sender as WebView;
                string s = view.Tag as string;
                s = "<body style=\"font-family:\"segoe ui\",\"microsoft yahei\",\"microsoft mhei\",stheititc,sans-serif\">" + s + "</body>";
                if (view.Source.Equals(blank) && !(s is null))
                {
                    foreach (var i in IndexPage.emojis)
                    {
                        if (s.Contains(i))
                        {
                            if (i.Contains('('))
                                s = s.Replace('#' + i, $"<img style=\"width: 30; height: 30\" src=\"ms-appx-web:///Emoji/{i}.png\">");
                            else
                                s = s.Replace(i, $"<img style=\"width: 30; height: 30\" src=\"ms-appx-web:///Emoji/{i}.png\">");
                        }
                    }
                    view.NavigateToString(s);
                }
            }
            catch { }
        }


        private void FeedListViewItem_Tapped(object sender, TappedRoutedEventArgs e) => mainPage.Frame.Navigate(typeof(FeedDetailPage), new object[] { ((sender as FrameworkElement).Tag as Feed).GetValue("id"), mainPage, "动态", null });

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button i = sender as Button;
            mainPage.Frame.Navigate(typeof(UserPage), new object[] { i.Tag as string, mainPage });
        }

        public async void ReadNextPage()
        {
            mainPage.ActiveProgressRing();

            ObservableCollection<Feed> FeedsCollection = listView.ItemsSource as ObservableCollection<Feed>;
            JArray Root = await CoolApkSDK.GetFeedListByID(uid, ++page, firstItem, lastItem);
            if (Root.Count != 0)
            {
                lastItem = Root.Last["id"].ToString();
                foreach (JObject i in Root)
                    FeedsCollection.Add(new Feed(i));
            }
            else page--;
            mainPage.DeactiveProgressRing();
        }
    }
}
