using CoolapkUWP.Helpers;
using CoolapkUWP.Pages.FeedPages;
using CoolapkUWP.ViewModels.FeedListPage;
using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.UserActivities;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace CoolapkUWP.Pages.AppPages
{
    /// <summary>
    /// 这是小板子留下的最后的东西了，只有这个页面从第一个版本流传至今，希望后人不要把它删了(●'◡'●)
    /// </summary>
    public sealed partial class AppPage : Page
    {
        string AppInfo = "", AppVersionMassage = "", AppReview = "", AppVersion, AppMassage, AppName, AppLogo, AppUpdateTime, AppScore, AppCommendNum, AppDownloadUrl, id;
        string AppLink = "";
        public AppPage()
        {
            this.InitializeComponent();
        }
        public static string ReplaceHtml(string str)
        {
            //换行和段落
            string s = str.Replace("<br>", "\n").Replace("<br>", "\n").Replace("<br/>", "\n").Replace("<br />", "\n").Replace("<p>", "").Replace("</p>", "\n").Replace("&nbsp;", " ");
            //链接彻底删除！
            while (s.IndexOf("<a") > 0)
            {
                s = s.Replace(@"<a href=""" + Regex.Split(Regex.Split(s, @"<a href=""")[1], @""">")[0] + @""">", "");
                s = s.Replace("</a>", "");
            }
            return s;
        }

        private void titleBar_Loaded(object sender, RoutedEventArgs e)
        {

        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            //将传过来的数据 类型转换一下
            AppLink = e.Parameter as string;
        }
        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            titleBar.ShowProgressRing();
            if (string.IsNullOrEmpty(AppLink)) return;
            Tag = AppLink;
            try
            {
                LaunchAppViewLoad(await new HttpClient().GetStringAsync(Tag.ToString()));
            }
            catch (HttpRequestException ex) { /*Tools.ShowHttpExceptionMessage(ex);*/ }
        }
        private void LaunchAppViewLoad(String str)
        {
            try { AppInfo = ReplaceHtml(Regex.Split(Regex.Split(Regex.Split(str, "应用简介</p>")[1], @"<div class=""apk_left_title_info"">")[1], "</div>")[0].Trim()); } catch (Exception) { }
            try { AppVersionMassage = ReplaceHtml(Regex.Split(Regex.Split(str, @"<p class=""apk_left_title_info"">")[2], "</p>")[0].Replace("<br />", "").Replace("<br/>", "").Trim()); } catch (Exception) { }
            try { AppReview = ReplaceHtml(Regex.Split(Regex.Split(str, @"<p class=""apk_left_title_info"">")[1], "</p>")[0].Replace("<br />", "").Replace("<br/>", "").Trim()); } catch (Exception) { }

            id = Regex.Split(str, @"onclick=""onDownloadApk")[1].Split('(')[1].Split(')')[0].Trim();
            AppVersion = Regex.Split(str, @"<p class=""detail_app_title"">")[1].Split('>')[1].Split('<')[0].Trim();
            AppMassage = Regex.Split(str, @"<p class=""apk_topba_message"">")[1].Split('<')[0].Trim().Replace("\n", "").Replace(" ", "");
            AppName = Regex.Split(str, @"<p class=""detail_app_title"">")[1].Split('<')[0].Trim();
            AppLogo = Regex.Split(str, @"<div class=""apk_topbar"">")[1].Split('"')[1].Trim();
            AppUpdateTime = Regex.Split(str, "更新时间：")[1].Split('<')[0].Trim();
            AppScore = Regex.Split(str, @"<p class=""rank_num"">")[1].Split('<')[0].Trim();
            AppCommendNum = Regex.Split(str, @"<p class=""apk_rank_p1"">")[1].Split('<')[0].Trim();

            //UIHelper.ShowMessage(id);

            //Download URI
            //AppDownloadUrl = Regex.Split(Regex.Split(Regex.Split(str, "function onDownloadApk")[1], "window.location.href")[1], @"""")[1];

            AppIconImage.Source = new BitmapImage(new Uri(AppLogo, UriKind.RelativeOrAbsolute));
            titleBar.Title = AppTitleText.Text = AppName;
            AppVTText.Text = AppUpdateTime;
            AppV2Text.Text = AppVersion;
            AppVText.Text = AppVersion;
            AppMText.Text = Regex.Split(AppMassage, "/")[2] + " " + Regex.Split(AppMassage, "/")[3] + " " + AppScore + "分";
            AppXText.Text = Regex.Split(AppMassage, "/")[1] + " · " + Regex.Split(AppMassage, "/")[0];

            if (Regex.Split(str, @"<p class=""apk_left_title_info"">").Length > 3)
            {
                //当应用有点评
                AppVMText.Text = AppVersionMassage;
                AppDText.Text = AppReview;
                DPanel.Visibility = Visibility.Visible;
            }
            else
            {
                //当应用无点评的时候（小编要是一个一个全好好点评我就不用加判断了嘛！）
                AppVMText.Text = AppReview;
                AppDText.Text = "";
            }
            if (AppReview.Contains("更新时间") && AppReview.Contains("ROM") && AppReview.Contains("名称")) UPanel.Visibility = Visibility.Collapsed;


            //加载截图！
            String images = Regex.Split(Regex.Split(str, @"<div class=""ex-screenshot-thumb-carousel"">")[1], "</div>")[0];
            String[] imagearray = Regex.Split(images, "<img");
            for (int i = 0; i < imagearray.Length - 1; i++)
            {
                String imageUrl = imagearray[i + 1].Split('"')[1];
                if (!imageUrl.Equals(""))
                {
                    Image newImage = new Image
                    {
                        Height = 100,
                        //获得图片
                        Source = new BitmapImage(new Uri(imageUrl, UriKind.RelativeOrAbsolute))
                    };
                    //添加到缩略视图
                    ScreenShotView.Items.Add(newImage);
                }
            }
            images = Regex.Split(Regex.Split(str, @"<div class=""carousel-inner"">")[1], @"<a class=""left carousel-control""")[0];
            imagearray = Regex.Split(images, "<img");
            for (int i = 0; i < imagearray.Length - 1; i++)
            {
                String imageurl = imagearray[i + 1].Split('"')[1];
                Image newImage = new Image
                {
                    //获得图片
                    Source = new BitmapImage(new Uri(imageurl, UriKind.RelativeOrAbsolute))
                };
                //添加到视图
                ScreenShotFlipView.Items.Add(newImage);
            }

            //还有简介（丧心病狂啊）
            AppJText.Text = AppInfo;

            //评分。。
            AppRText.Text = AppScore;
            AppRating.PlaceholderValue = Double.Parse(AppScore);
            AppPText.Text = AppCommendNum;


            /*
            //获取开发者
            string kAppName = Web.ReplaceHtml(Regex.Split(Regex.Split(str, "开发者名称：")[1], "</p>")[0]);
            try
            {
                AppKNText.Text = kAppName;
                AppKImage.Source = new BitmapImage(new Uri(await CoolApkSDK.GetCoolApkUserFaceUri(kAppName), UriKind.RelativeOrAbsolute));
            }
            catch (Exception)
            {
                KPanel.Visibility = Visibility.Collapsed;
            }*/
            GenerateActivityAsync();
            titleBar.HideProgressRing();
        }

        private void Report_Click(object sender, RoutedEventArgs e)
        {
            UIHelper.Navigate(typeof(BrowserPage), new object[] { false, "https://m.coolapk.com/mp/apk/report?apkname=" + Regex.Split(Tag.ToString(), "/")[4] });
        }

        private void CopyM_Click(object sender, RoutedEventArgs e)
        {

            if (sender is MenuFlyoutItem selectedItem)
            {
                DataPackage dp = new DataPackage();
                // copy 
                if (selectedItem.Tag.ToString() == "0")
                {
                    dp.SetText(AppInfo);
                }
                else if (selectedItem.Tag.ToString() == "1")
                {
                    dp.SetText(AppReview);
                }
                else if (selectedItem.Tag.ToString() == "2")
                {
                    dp.SetText(Regex.Split(Tag.ToString(), "/")[4]);
                }
                else if (selectedItem.Tag.ToString() == "3")
                {
                    dp.SetText(AppVersion);
                }
                else if (selectedItem.Tag.ToString() == "24")
                {
                    dp.SetText(AppVersionMassage);
                }
                Clipboard.SetContent(dp);
            }
        }

        UserActivitySession _currentActivity;
        private async Task GenerateActivityAsync()
        {
            // Get the default UserActivityChannel and query it for our UserActivity. If the activity doesn't exist, one is created.
            UserActivityChannel channel = UserActivityChannel.GetDefault();
            UserActivity userActivity = await channel.GetOrCreateUserActivityAsync(AppName);

            // Populate required properties
            userActivity.VisualElements.DisplayText = AppName;
            userActivity.VisualElements.AttributionDisplayText = AppName;
            if (AppInfo.Length > 3)
                userActivity.VisualElements.Description = AppInfo;
            else
                userActivity.VisualElements.Description = AppReview;
            userActivity.ActivationUri = new Uri("coolapk://" + AppLink);

            //Save
            await userActivity.SaveAsync(); //save the new metadata

            // Dispose of any current UserActivitySession, and create a new one.
            _currentActivity?.Dispose();
            _currentActivity = userActivity.CreateSession();
        }

        private async void GotoUri_Click(object sender, RoutedEventArgs e)
        {
            // Launch the URI
            var success = await Launcher.LaunchUriAsync(new Uri(AppLink));
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (ScreenShotFlipView.Visibility == Visibility.Visible)
                CloseFlip_Click();
            else Frame.GoBack();
        }

        private void ScreenShotView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ScreenShotFlipView.Visibility = Visibility.Visible;
            //CloseFlip.Visibility = Visibility.Visible;
            //CloseBlock.Visibility = Visibility.Visible;
            ScreenShotView.SelectedIndex = -1;
        }

        private void CloseFlip_Click()
        {
            ScreenShotFlipView.Visibility = Visibility.Collapsed;
            //CloseBlock.Visibility = Visibility.Collapsed;
            //CloseFlip.Visibility = Visibility.Collapsed;
        }


        private async void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            // Download the URI
            var success = await Launcher.LaunchUriAsync(new Uri(AppDownloadUrl));
        }

        private async void ViewFeed(object sender, RoutedEventArgs e)
        {
            var f = FeedListPageViewModelBase.GetProvider(FeedListType.AppPageList, id);
            if (f != null)
            {
                UIHelper.NavigateInSplitPane(typeof(FeedListPage), f);
            }
        }

        private void TitleBar_BackButtonClick(object sender, RoutedEventArgs e) => Frame.GoBack();

        private void titleBar_RefreshButtonClicked(object sender, RoutedEventArgs e)
        {
            titleBar.ShowProgressRing();
            titleBar.HideProgressRing();
        }
    }
}
