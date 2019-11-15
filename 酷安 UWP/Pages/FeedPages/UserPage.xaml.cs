using CoolapkUWP.Control.ViewModels;
using CoolapkUWP.Data;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Windows.Data.Json;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace CoolapkUWP.Pages.FeedPages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class UserPage : Page, INotifyPropertyChanged
    {
        //ScrollViewer VScrollViewer;
        string uid;
        int page = 0;
        double firstItem = 0, lastItem = 0;
        UserDetail userDetail;
        UserDetail UserDetail
        {
            get => userDetail;
            set
            {
                userDetail = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UserDetail)));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        ObservableCollection<FeedViewModel> FeedsCollection = new ObservableCollection<FeedViewModel>();

        public UserPage()
        {
            this.InitializeComponent();
            listView.ItemsSource = FeedsCollection;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter as string != uid || UserDetailGrid.DataContext == null)
            {
                if (e.Parameter as string != "0")
                {
                    uid = e.Parameter as string;
                    Tools.ShowProgressBar();
                    FeedsCollection.Clear();
                    page = 0;
                    firstItem = lastItem = 0;
                    titleBar.Title = string.Empty;
                    ListHeader.DataContext = UserDetailGrid.DataContext = null;
                    //GetVScrollViewer();
                    LoadProfile();
                    ReadNextPageFeeds();
                    Tools.HideProgressBar();
                }
                else
                {
                    uid = string.Empty;
                    Frame.GoBack();
                }
            }
            //UserDetailGrid.Height = UserDetailGrid.Width;
        }
        /*
        async void GetVScrollViewer()
        {
            while (VScrollViewer is null)
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                    () => VScrollViewer = (VisualTreeHelper.GetChild(listView, 0) as FrameworkElement)?.FindName("ScrollViewer") as ScrollViewer);
                await Task.Delay(1000);
            }
            VScrollViewer.ViewChanged += ScrollViewer_ViewChanged;
        }*/

        public async void LoadProfile()
        {
            string result = await Tools.GetJson("/user/space?uid=" + uid);
            JsonObject detail = Tools.GetJSonObject(result);
            if (detail != null)
            {
                UserDetail = new UserDetail
                {
                    UserFaceUrl = detail["userAvatar"].GetString(),
                    UserName = detail["username"].GetString(),
                    FollowNum = detail["follow"].GetNumber(),
                    FansNum = detail["fans"].GetNumber(),
                    Level = detail["level"].GetNumber(),
                    Bio = detail["bio"].GetString(),
                    BackgroundUrl = detail["cover"].GetString(),
                    Verify_title = detail["verify_title"].GetString(),
                    Gender = detail["gender"].GetNumber() == 1 ? "♂" : (detail["gender"].GetNumber() == 0 ? "♀" : string.Empty),
                    City = $"{detail["province"].GetString()} {detail["city"].GetString()}",
                    Astro = detail["astro"].GetString(),
                    Logintime = $"{Tools.ConvertTime(detail["logintime"].GetNumber())}活跃",
                    FeedNum = detail["feed"].GetNumber(),
                    UserFace = await ImageCache.GetImage(ImageType.SmallAvatar, detail["userSmallAvatar"].GetString()),
                    Background = new ImageBrush { ImageSource = await ImageCache.GetImage(ImageType.OriginImage, detail["cover"].GetString()), Stretch = Stretch.UniformToFill }
                };
                titleBar.Title = detail["username"].GetString();
            }
        }

        async void ReadNextPageFeeds()
        {
            string str = await Tools.GetJson($"/user/feedList?uid={uid}&page={++page}&firstItem={firstItem}&lastItem={lastItem}");
            JsonArray Root = Tools.GetDataArray(str);
            if (Root != null && Root.Count != 0)
            {
                if (page == 1)
                    firstItem = Root.First().GetObject()["id"].GetNumber();
                lastItem = Root.Last().GetObject()["id"].GetNumber();
                foreach (var i in Root)
                    FeedsCollection.Add(new FeedViewModel(i));
            }
            else page--;
        }

        private void TitleBar_BackButtonClick(object sender, RoutedEventArgs e) => Frame.GoBack();

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            switch (button.Tag as string)
            {
                case "2":
                    Tools.Navigate(typeof(UserListPage), new object[] { uid, true, titleBar.Title });
                    break;
                case "3":
                    Tools.Navigate(typeof(UserListPage), new object[] { uid, false, titleBar.Title });
                    break;
            }
        }

        async void Refresh()
        {
            if (UserDetailGrid.DataContext == null && FeedsCollection.Count == 0) return;
            Tools.ShowProgressBar();
            LoadProfile();
            string str = await Tools.GetJson($"/user/feedList?uid={uid}&page=1{(firstItem == 0 ? string.Empty : $"&firstItem={firstItem}")}{(lastItem == 0 ? string.Empty : $"&lastItem ={lastItem}")}");
            JsonArray Root = Tools.GetDataArray(str);
            if (Root != null && Root.Count != 0)
            {
                firstItem = Root.First().GetObject()["id"].GetNumber();
                for (int i = 0; i < Root.Count; i++)
                    FeedsCollection.Insert(i, new FeedViewModel(Root[i]));
            }
            VScrollViewer.ChangeView(null, 20, null);
            Tools.HideProgressBar();
        }

        private void PicA_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender is FrameworkElement fe)
            {
                if (fe != e.OriginalSource) return;
                if (fe.Tag is string s) Tools.ShowImage(s);
            }
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //UserDetailGrid.Height = UserDetailGrid.Width;
        }

        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (!e.IsIntermediate)
            {
                if (VScrollViewer.VerticalOffset == 0)
                {
                    Refresh();
                    refreshText.Visibility = Visibility.Collapsed;
                }
                else if (VScrollViewer.VerticalOffset == VScrollViewer.ScrollableHeight)
                    ReadNextPageFeeds();
            }
            else refreshText.Visibility = Visibility.Visible;
        }
    }

    internal class UserDetail
    {
        public string UserFaceUrl;
        public ImageSource UserFace;
        public string UserName;
        public double FollowNum;
        public double FansNum;
        public double FeedNum;
        public double Level;
        public string Bio;
        public string BackgroundUrl;
        public ImageBrush Background;
        public string Verify_title;
        public string Gender;
        public string City;
        public string Astro;
        public string Logintime;
        public bool Has_bio { get => Bio.Length > 0; }
        public bool Has_verify_title { get => Verify_title.Length > 0; }
    }
}
