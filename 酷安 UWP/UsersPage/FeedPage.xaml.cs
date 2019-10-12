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
    public sealed partial class FeedPage : Page
    {
        MainPage mainPage;
        string uid;

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

            JArray Root = await CoolApkSDK.GetFeedListByID(uid, 1, "");
            foreach (JObject i in Root)
                FeedsCollection.Add(new Feed(i));
            mainPage.DeactiveProgressRing();
        }
        private void BackButton_Click(object sender, RoutedEventArgs e) => Frame.GoBack();
        Uri blank = new Uri("about:blank");
        private void WebView_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
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

        private void FeedListViewItem_Tapped(object sender, TappedRoutedEventArgs e)
        {

        }
    }
}
