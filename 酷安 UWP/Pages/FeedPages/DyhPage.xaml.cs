using CoolapkUWP.Control.ViewModels;
using CoolapkUWP.Data;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.Data.Json;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace CoolapkUWP.Pages.FeedPages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class DyhPage : Page
    {
        Style ListViewStyle
        {
            get
            {
                if (Settings.IsMobile) return Application.Current.Resources["ListViewStyle2Mobile"] as Style;
                else return Application.Current.Resources["ListViewStyle2Desktop"] as Style;
            }
        }
        string id;
        int[] page = new int[2];
        double[] firstItem = new double[2];
        double[] lastItem = new double[2];
        ObservableCollection<FeedViewModel>[] FeedsCollection = new ObservableCollection<FeedViewModel>[2];

        public DyhPage()
        {
            this.InitializeComponent();
            FeedsCollection[0] = new ObservableCollection<FeedViewModel>();
            FeedsCollection[1] = new ObservableCollection<FeedViewModel>();
            listView.ItemsSource = FeedsCollection[0];
            listView2.ItemsSource = FeedsCollection[1];
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            id = e.Parameter as string;
            Tools.ShowProgressBar();
            LoadDyhDetail();
            LoadFeeds();
            Tools.HideProgressBar();
        }
        public async void LoadDyhDetail()
        {
            ImageSource getImage(string uri)
            {
                if (Settings.GetBoolen("IsNoPicsMode"))
                {
                    if (Settings.GetBoolen("IsDarkMode"))
                        return new BitmapImage(new Uri("ms-appx:/Assets/img_placeholder_night.png")) { DecodePixelHeight = 150, DecodePixelWidth = 150 };
                    else return new BitmapImage(new Uri("ms-appx:/Assets/img_placeholder.png")) { DecodePixelHeight = 150, DecodePixelWidth = 150 };
                }
                return new BitmapImage(new Uri(uri));
            }

            string r = await Tools.GetJson($"/dyh/detail?dyhId={id}");
            JsonObject detail = Tools.GetJSonObject(r);
            if (detail != null)
            {
                if (detail["is_open_discuss"].GetNumber() == 1) MainPivot.IsLocked = false;
                TitleBar.Title = detail["title"].GetString();
                bool showUserButton = detail["uid"].GetNumber() != 0;
                DetailGrid.DataContext = new
                {
                    Logo = getImage(detail["logo"].GetString()),
                    Title = detail["title"].GetString(),
                    Description = detail["description"].GetString(),
                    FollowNum = detail["follownum"].GetNumber(),
                    ShowUserButton = showUserButton ? Visibility.Visible : Visibility.Collapsed,
                    url = showUserButton ? detail["userInfo"].GetObject()["url"].GetString() : string.Empty,
                    UserName = showUserButton ? detail["userInfo"].GetObject()["username"].GetString() : string.Empty,
                    UserAvatar = showUserButton ? getImage(detail["userInfo"].GetObject()["userSmallAvatar"].ToString().Replace("\"",string.Empty)) : new BitmapImage()
                };
            }
        }

        async void LoadFeeds(int p = -1)
        {
            string r = await Tools.GetJson($"/dyhArticle/list?dyhId={id}&type={(MainPivot.SelectedIndex == 0 ? "all" : "square")}&page={(p == -1 ? ++page[MainPivot.SelectedIndex] : p)}{(firstItem[MainPivot.SelectedIndex] == 0 ? string.Empty : $"&firstItem={firstItem[MainPivot.SelectedIndex]}")}{((lastItem[MainPivot.SelectedIndex] == 0) ? string.Empty : $" & lastItem ={ lastItem[MainPivot.SelectedIndex]}")}");
            JsonArray Root = Tools.GetDataArray(r);
            if (Root != null && Root.Count != 0)
            {
                if (page[MainPivot.SelectedIndex] == 1)
                {
                    firstItem[MainPivot.SelectedIndex] = Root.First().GetObject().TryGetValue("articleId", out IJsonValue value1) ? value1.GetNumber() : Root.First().GetObject()["id"].GetNumber();
                    lastItem[MainPivot.SelectedIndex] = Root.Last().GetObject().TryGetValue("articleId", out IJsonValue value2) ? value2.GetNumber() : Root.Last().GetObject()["id"].GetNumber();
                }
                foreach (var i in Root)
                    FeedsCollection[MainPivot.SelectedIndex].Add(new FeedViewModel(i, FeedDisplayMode.notShowDyhName));
            }
            else page[MainPivot.SelectedIndex]--;
        }

        void Refresh()
        {
            Tools.ShowProgressBar();
            //LoadDyhDetail();
            LoadFeeds(1);
            Tools.HideProgressBar();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            switch (button.Tag as string)
            {
                case "0": break;
                default:
                    Tools.OpenLink(button.Tag as string);
                    break;
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
                    LoadFeeds();
            }
            else refreshText.Visibility = Visibility.Visible;
        }

        private void MainPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(id))
                Refresh();
        }

        private void TitleBar_BackButtonClick(object sender, RoutedEventArgs e) => Frame.GoBack();
    }
}
