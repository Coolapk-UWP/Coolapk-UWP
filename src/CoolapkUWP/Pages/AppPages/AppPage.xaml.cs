using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using Windows.ApplicationModel.DataTransfer;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace CoolapkUWP.Pages.AppPages
{
    public sealed partial class AppPage : Page
    {
        string jstr = "", vmstr = "", dstr = "", vstr, mstr, nstr, iurl, vtstr, rstr, pstr, ddstr;
        string applink = "";
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
            applink = e.Parameter as string;
        }
        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            titleBar.ShowProgressRing();
            if (string.IsNullOrEmpty(applink)) return;
            this.Tag = applink;
            try
            {
                LaunchAppViewLoad(await new HttpClient().GetStringAsync(Tag.ToString()));
            }
            catch (HttpRequestException ex) { /*Tools.ShowHttpExceptionMessage(ex);*/ }
        }
        private void LaunchAppViewLoad(String str)
        {
            try { jstr = ReplaceHtml(Regex.Split(Regex.Split(Regex.Split(str, "应用简介</p>")[1], @"<div class=""apk_left_title_info"">")[1], "</div>")[0].Trim()); } catch (Exception) { }
            try { vmstr = ReplaceHtml(Regex.Split(Regex.Split(str, @"<p class=""apk_left_title_info"">")[2], "</p>")[0].Replace("<br />", "").Replace("<br/>", "").Trim()); } catch (Exception) { }
            try { dstr = ReplaceHtml(Regex.Split(Regex.Split(str, @"<p class=""apk_left_title_info"">")[1], "</p>")[0].Replace("<br />", "").Replace("<br/>", "").Trim()); } catch (Exception) { }
            vstr = Regex.Split(str, @"<p class=""detail_app_title"">")[1].Split('>')[1].Split('<')[0].Trim();
            mstr = Regex.Split(str, @"<p class=""apk_topba_message"">")[1].Split('<')[0].Trim().Replace("\n", "").Replace(" ", "");
            nstr = Regex.Split(str, @"<p class=""detail_app_title"">")[1].Split('<')[0].Trim();
            iurl = Regex.Split(str, @"<div class=""apk_topbar"">")[1].Split('"')[1].Trim();
            vtstr = Regex.Split(str, "更新时间：")[1].Split('<')[0].Trim();
            rstr = Regex.Split(str, @"<p class=""rank_num"">")[1].Split('<')[0].Trim();
            pstr = Regex.Split(str, @"<p class=""apk_rank_p1"">")[1].Split('<')[0].Trim();

            //Download URI
            //ddstr = Regex.Split(Regex.Split(Regex.Split(str, "function onDownloadApk")[1], "window.location.href")[1], @"""")[1];

            AppIconImage.Source = new BitmapImage(new Uri(iurl, UriKind.RelativeOrAbsolute));
            titleBar.Title = AppTitleText.Text = nstr;
            AppVTText.Text = vtstr;
            AppV2Text.Text = vstr;
            AppVText.Text = vstr;
            AppMText.Text = Regex.Split(mstr, "/")[2] + " " + Regex.Split(mstr, "/")[3] + " " + rstr + "分";
            AppXText.Text = Regex.Split(mstr, "/")[1] + " · " + Regex.Split(mstr, "/")[0];

            if (Regex.Split(str, @"<p class=""apk_left_title_info"">").Length > 3)
            {
                //当应用有点评
                AppVMText.Text = vmstr;
                AppDText.Text = dstr;
                DPanel.Visibility = Visibility.Visible;
            }
            else
            {
                //当应用无点评的时候（小编要是一个一个全好好点评我就不用加判断了嘛！）
                AppVMText.Text = dstr;
                AppDText.Text = "";
            }
            if (dstr.Contains("更新时间") && dstr.Contains("ROM") && dstr.Contains("名称")) UPanel.Visibility = Visibility.Collapsed;


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
            AppJText.Text = jstr;

            //评分。。
            AppRText.Text = rstr;
            AppRating.PlaceholderValue = Double.Parse(rstr);
            AppPText.Text = pstr;


            /*
            //获取开发者
            string knstr = Web.ReplaceHtml(Regex.Split(Regex.Split(str, "开发者名称：")[1], "</p>")[0]);
            try
            {
                AppKNText.Text = knstr;
                AppKImage.Source = new BitmapImage(new Uri(await CoolApkSDK.GetCoolApkUserFaceUri(knstr), UriKind.RelativeOrAbsolute));
            }
            catch (Exception)
            {
                KPanel.Visibility = Visibility.Collapsed;
            }*/
            titleBar.HideProgressRing();
        }

        private void CopyM_Click(object sender, RoutedEventArgs e)
        {

            if (sender is MenuFlyoutItem selectedItem)
            {
                DataPackage dp = new DataPackage();
                // copy 
                if (selectedItem.Tag.ToString() == "0")
                {
                    dp.SetText(jstr);
                }
                else if (selectedItem.Tag.ToString() == "1")
                {
                    dp.SetText(dstr);
                }
                else if (selectedItem.Tag.ToString() == "2")
                {
                    dp.SetText(Regex.Split(Tag.ToString(), "/")[4]);
                }
                else if (selectedItem.Tag.ToString() == "3")
                {
                    dp.SetText(vstr);
                }
                else if (selectedItem.Tag.ToString() == "24")
                {
                    dp.SetText(vmstr);
                }
                Clipboard.SetContent(dp);
            }
        }
        private async void GotoUri_Click(object sender, RoutedEventArgs e)
        {
            // Launch the URI
            var success = await Launcher.LaunchUriAsync(new Uri(applink));
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private void ScreenShotView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ScreenShotFlipView.Visibility = Visibility.Visible;
            CloseFlip.Visibility = Visibility.Visible;
            ScreenShotView.SelectedIndex = -1;
        }

        private void CloseFlip_Click(object sender, RoutedEventArgs e)
        {
            ScreenShotFlipView.Visibility = Visibility.Collapsed;
            CloseFlip.Visibility = Visibility.Collapsed;
        }


        private async void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            // Download the URI
            var success = await Launcher.LaunchUriAsync(new Uri(ddstr));
        }

        private void TitleBar_BackButtonClick(object sender, RoutedEventArgs e) => Frame.GoBack();

        private void titleBar_RefreshButtonClicked(object sender, RoutedEventArgs e)
        {
            titleBar.ShowProgressRing();
            titleBar.HideProgressRing();
        }
    }
}
