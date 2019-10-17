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
using Windows.System;
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
        static int page = 0;
        public static ObservableCollection<Feed> FeedsCollection = new ObservableCollection<Feed>();
        public IndexPage()
        {
            this.InitializeComponent();
            listView.ItemsSource = FeedsCollection;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            mainPage = e.Parameter as MainPage;
            if (FeedsCollection.Count == 0)
                GetIndexPage(++page);
        }

        public async void GetIndexPage(int page)
        {
            mainPage.ActiveProgressRing();
            if (page == 1)
            {
                timer.Stop();
                timer = new DispatcherTimer();
                JArray Root = await CoolApkSDK.GetIndexList(page);
                if (FeedsCollection.Count != 0)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        if (FeedsCollection[i].GetValue("entityFixed") == "1")
                            FeedsCollection.RemoveAt(0);
                    }
                    for (int i = 0; i < Root.Count; i++)
                    {
                        if (i >= FeedsCollection.Count) break;
                        if (Root.Contains(FeedsCollection[i].jObject))
                        {
                            FeedsCollection.RemoveAt(i);
                            i--;
                        }
                    }
                }
                for (int i = 0; i < Root.Count; i++)
                    FeedsCollection.Insert(i, new Feed((JObject)Root[i]));
                timer.Interval = new TimeSpan(0, 0, 7);
                timer.Tick += (s, e) =>
                {
                    if (flip.SelectedIndex < flip.Items.Count - 1)
                        flip.SelectedIndex++;
                    else
                        flip.SelectedIndex = 0;
                };
                timer.Start();
            }
            else
            {
                JArray Root = await CoolApkSDK.GetIndexList(page);
                if (Root.Count != 0)
                    foreach (JObject i in Root)
                        FeedsCollection.Add(new Feed(i));
                else page--;
            }
            mainPage.DeactiveProgressRing();
        }

        DispatcherTimer timer = new DispatcherTimer();
        FlipView flip;

        private void FlipView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (flip is null) flip = sender as FlipView;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            timer.Stop();
        }

        private void FeedListViewItem_Tapped(object sender, TappedRoutedEventArgs e) => mainPage.Frame.Navigate(typeof(FeedDetailPage), new object[] { ((sender as FrameworkElement).Tag as Feed).GetValue("id"), mainPage, string.Empty, null });
        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (!e.IsIntermediate)
            {
                if (FeedsCollection.Count != 0)
                    if (VScrollViewer.VerticalOffset == 0)
                    {
                        GetIndexPage(1);
                        VScrollViewer.ChangeView(null, 20, null);
                        refreshText.Visibility = Visibility.Collapsed;
                    }
                    else if (VScrollViewer.VerticalOffset == VScrollViewer.ScrollableHeight)
                        GetIndexPage(++page);
            }
            else refreshText.Visibility = Visibility.Visible;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button i = sender as Button;
            if (i.Tag as string == "Refresh")
            {
                GetIndexPage(1);
                VScrollViewer.ChangeView(null, 0, null);
            }
            else mainPage.Frame.Navigate(typeof(UserPage), new object[] { i.Tag as string, mainPage });
        }

        private async void MarkdownTextBlock_LinkClicked(object sender, Microsoft.Toolkit.Uwp.UI.Controls.LinkClickedEventArgs e)
        {
            if (e.Link.IndexOf("/u/") == 0)
                mainPage.Frame.Navigate(typeof(UserPage), new object[] { await CoolApkSDK.GetUserIDByName(e.Link.Replace("/u/", string.Empty)), mainPage });
            if (e.Link.Replace("mailto:",string.Empty).IndexOf("http://image.coolapk.com") == 0)
                await Launcher.LaunchUriAsync(new Uri(e.Link.Replace("mailto:", string.Empty)));
            if (e.Link.IndexOf("http") == 0)
                await Launcher.LaunchUriAsync(new Uri(e.Link));
        }

        private void ListViewItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            GetIndexPage(1);
            VScrollViewer.ChangeView(null, 0, null);
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
    }
}
