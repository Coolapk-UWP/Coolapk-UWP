using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
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
        string uid;
        public UserPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            uid = (string)((object[])e.Parameter)[0];
            mainPage = ((object[])e.Parameter)[1] as MainPage;
            LoadProfile();
        }

        public async void LoadProfile()
        {
            mainPage.ActiveProgressRing();
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
                    bio = detail["bio"].ToString()
                };
                TitleTextBlock.Text = detail["username"].ToString();
                ListPivot.DataContext = new { FeedNum = detail["feed"].ToString() };
                FeedListFrame.Navigate(typeof(FeedPage), new object[] { detail["uid"].ToString(), mainPage });
                mainPage.DeactiveProgressRing();
            }
            else
            {
                Frame.GoBack();
                await new MessageDialog("用户不存在").ShowAsync();
            }
        }


        private void Button_Click(object sender, RoutedEventArgs e) => Frame.GoBack();

        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            var sv = sender as ScrollViewer;

            if (!e.IsIntermediate)
                if (sv.VerticalOffset == sv.ScrollableHeight)
                {
                    (((ListPivot.Items[ListPivot.SelectedIndex] as PivotItem).Content as Frame).Content as IReadNextPage).ReadNextPage();
                }
        }
    }
}
