using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace 酷安_UWP
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class IndexPage : Page
    {
        MainPage mainPage;
        int page = 1;
        string lastItem;
        public IndexPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            mainPage = e.Parameter as MainPage;
            LoadIndex();
        }

        public async void LoadIndex()
        {
            mainPage.ActiveProgressRing();
            ObservableCollection<Feed> FeedsCollection = new ObservableCollection<Feed>();
            listView.ItemsSource = FeedsCollection;

            JArray Root = await CoolApkSDK.GetIndexList(1, string.Empty);
            lastItem = Root.Last["entityId"].ToString();
            foreach (JObject i in Root)
                FeedsCollection.Add(new Feed(i));
            timer.Interval = new TimeSpan(0, 0, 7);
            timer.Tick += (s, e) =>
            {
                if (flip.SelectedIndex < flip.Items.Count - 1)
                    flip.SelectedIndex++;
                else
                    flip.SelectedIndex = 0;
            };
            timer.Start();
            mainPage.DeactiveProgressRing();
        }

        DispatcherTimer timer = new DispatcherTimer();
        FlipView flip;

        private void FlipView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (flip is null)
                flip = sender as FlipView;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            timer.Stop();
        }

        private void FeedListViewItem_Tapped(object sender, TappedRoutedEventArgs e) => mainPage.Frame.Navigate(typeof(FeedDetailPage), new object[] { ((sender as FrameworkElement).Tag as Feed).GetValue("id"), mainPage, "动态", null });
        private async void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            var sv = sender as ScrollViewer;

            if (!e.IsIntermediate)
                if (sv.VerticalOffset == sv.ScrollableHeight)
                {
                    mainPage.ActiveProgressRing();

                    ObservableCollection<Feed> FeedsCollection = listView.ItemsSource as ObservableCollection<Feed>;
                    JArray Root = await CoolApkSDK.GetIndexList(++page, lastItem);
                    if (Root.Count != 0)
                    {
                        lastItem = Root.Last["entityId"].ToString();
                        foreach (JObject i in Root)
                            FeedsCollection.Add(new Feed(i));
                    }
                    else page--;
                    mainPage.DeactiveProgressRing();
                }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button i = sender as Button;
            mainPage.Frame.Navigate(typeof(UserPage), new object[] { i.Tag as string, mainPage });
        }
    }
}
