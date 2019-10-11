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
                            s = s.Replace('#' + i, $"<img style=\"width: 70; height: 70\" src=\"ms-appx-web:///Emoji/{i}.png\">");
                        else
                            s = s.Replace(i, $"<img style=\"width: 70; height: 70\" src=\"ms-appx-web:///Emoji/{i}.png\">");
                    }
                }
                view.NavigateToString(s);
            }
        }

        private async void FeedListViewItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            await new MessageDialog(((sender as FrameworkElement).Tag as Feed).GetValue("uid")).ShowAsync();
        }
    }

    public class Feed
    {
        JObject jObject;
        public Feed(JObject jObject) => this.jObject = jObject;
        public string GetValue(string value)
        {
            if (jObject.TryGetValue(value, out JToken token))
                return token.ToString();
            else
                return string.Empty;
        }
        public Feed[] GetSelfs() => new Feed[] { this };
        public ImageSource GetValue2(string value) => new BitmapImage(new Uri(jObject.GetValue(value).ToString()));
        public ImageSource[] GetValue3(string value)
        {
            JArray array = (JArray)jObject.GetValue(value);
            List<BitmapImage> images = new List<BitmapImage>();
            foreach (var item in array)
                if (!string.IsNullOrEmpty(item.ToString()))
                    //获取缩略图
                    images.Add(new BitmapImage(new Uri(item.ToString()+"s.jpg")));
            return images.ToArray();
        }
        public Feed[] GetFeeds(string value)
        {
            JArray array = (JArray)jObject.GetValue(value);
            List<Feed> fs = new List<Feed>();
            foreach (JObject item in array)
                if (!string.IsNullOrEmpty(item.ToString()))
                    fs.Add(new Feed(item));
            return fs.ToArray();
        }
        public Uri GetValue4(string value) => new Uri(jObject.GetValue(value).ToString());
    }

    public class Feeds
    {

    }

    public class FirstTemplateSelector : DataTemplateSelector
    {
        public DataTemplate DataTemplate0 { get; set; }
        public DataTemplate DataTemplate1 { get; set; }
        public DataTemplate DataTemplate2 { get; set; }
        protected override DataTemplate SelectTemplateCore(object item)
        {
            Feed feed = item as Feed;
            switch (feed.GetValue("entityType"))
            {
                case "feed":
                    return DataTemplate1;
                case "card":
                    return DataTemplate2;
                default:
                    return DataTemplate0;
            }
        }
    }
    public class SecondTemplateSelector : DataTemplateSelector
    {
        public DataTemplate DataTemplate0 { get; set; }
        public DataTemplate DataTemplate1 { get; set; }
        public DataTemplate DataTemplate2 { get; set; }
        public DataTemplate DataTemplate3 { get; set; }
        protected override DataTemplate SelectTemplateCore(object item)
        {
            Feed feed = item as Feed;
            switch (feed.GetValue("entityTemplate"))
            {
                case "imageCarouselCard_1":
                    return DataTemplate1;
                case "iconMiniGridCard":
                case "iconLinkGridCard":
                    return DataTemplate2;
                case "messageCard":
                    return DataTemplate3;
                default:
                    return DataTemplate0;
            }
        }
    }
    public class ThirdTemplateSelector : DataTemplateSelector
    {
        public DataTemplate DataTemplate0 { get; set; }
        public DataTemplate DataTemplate1 { get; set; }
        public DataTemplate DataTemplate2 { get; set; }
        public DataTemplate DataTemplate3 { get; set; }
        public DataTemplate DataTemplate4 { get; set; }
        protected override DataTemplate SelectTemplateCore(object item)
        {
            Feed feed = item as Feed;
            switch (feed.GetValue("entityType"))
            {
                case "image_1":
                    return DataTemplate1;
                case "iconLink":
                    return DataTemplate2;
                case "dyh":
                    return DataTemplate3;
                case "topic":
                    return DataTemplate4;
                default:
                    return DataTemplate0;
            }
        }
    }
}
