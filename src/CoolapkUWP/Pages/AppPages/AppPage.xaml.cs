using CoolapkUWP.Core.Helpers;
using CoolapkUWP.Core.Models;
using CoolapkUWP.Helpers;
using CoolapkUWP.Pages.FeedPages;
using CoolapkUWP.ViewModels.FeedListPage;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel;
using System.Data;
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
    public sealed partial class AppPage : Page, INotifyPropertyChanged
    {
        private AppModel appmodels;
        private AppModel AppModels
        {
            get => appmodels;
            set
            {
                appmodels = value;
                RaisePropertyChangedEvent();
            }
        }
        private string AppMassage, AppUpdateTime, /*AppDownloadUrl,*/ id;
        private string AppLink = "";

        public event PropertyChangedEventHandler PropertyChanged;

        internal void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        public AppPage()
        {
            InitializeComponent();
            Rectangle_PointerExited();
        }
        public static string ReplaceHtml(string str)
        {
            //换行和段落
            string s = str.Replace("<br>", "\n").Replace("<br>", "\n").Replace("<br/>", "\n").Replace("<br/>", "\n").Replace("<p>", "").Replace("</p>", "\n").Replace("&nbsp;", " ").Replace("<br />", "").Replace("<br />", "");
            //链接彻底删除！
            while (s.IndexOf("<a", StringComparison.Ordinal) > 0)
            {
                s = s.Replace(@"<a href=""" + Regex.Split(Regex.Split(s, @"<a href=""")[1], @""">")[0] + @""">", "");
                s = s.Replace("</a>", "");
            }
            return s;
        }

        private void titleBar_Loaded(object sender, RoutedEventArgs e)
        {

        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            //将传过来的数据 类型转换一下
            AppLink = e.Parameter as string;
            if (string.IsNullOrEmpty(AppLink)) { return; }
            (bool isSucceed, string result) = await DataHelper.GetHtmlAsync(new Uri(AppLink), "XMLHttpRequest");
            if (isSucceed && !string.IsNullOrEmpty(result))
            {
                JObject json = JObject.Parse(result);
                AppModels = new AppModel(json);
                _ = GenerateActivityAsync();
            }
        }
        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            titleBar.ShowProgressRing();
            if (string.IsNullOrEmpty(AppLink)) { return; }
            (bool HTMLisSucceed, string html) = await DataHelper.GetHtmlAsync(new Uri(AppLink), "com.coolapk.market");
            if (HTMLisSucceed) { LaunchAppViewLoad(html); }
        }
        private void LaunchAppViewLoad(string str)
        {
            #region 小板子的HTML内容获取
            id = Regex.Split(str, @"onclick=""onDownloadApk")[1].Split('(')[1].Split(')')[0].Trim();
            AppMassage = Regex.Split(str, @"<p class=""apk_topba_message"">")[1].Split('<')[0].Trim().Replace("\n", "").Replace(" ", "");
            AppUpdateTime = Regex.Split(str, "更新时间：")[1].Split('<')[0].Trim();

            AppVTText.Text = AppUpdateTime;
            AppMText.Text = Regex.Split(AppMassage, "/")[2] + " " + Regex.Split(AppMassage, "/")[3] + " ";
            AppXText.Text = Regex.Split(AppMassage, "/")[1] + " · " + Regex.Split(AppMassage, "/")[0];

            //加载截图！
            string images = Regex.Split(Regex.Split(str, @"<div class=""ex-screenshot-thumb-carousel"">")[1], "</div>")[0];
            string[] imagearray = Regex.Split(images, "<img");
            for (int i = 0; i < imagearray.Length - 1; i++)
            {
                string imageUrl = imagearray[i + 1].Split('"')[1];
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
                string imageurl = imagearray[i + 1].Split('"')[1];
                Image newImage = new Image
                {
                    //获得图片
                    Source = new BitmapImage(new Uri(imageurl, UriKind.RelativeOrAbsolute))
                };
                //添加到视图
                ScreenShotFlipView.Items.Add(newImage);
            }


            #endregion

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
                    dp.SetText(AppIntroduce.Text);
                }
                else if (selectedItem.Tag.ToString() == "1")
                {
                    dp.SetText(AppDescription.Text);
                }
                else if (selectedItem.Tag.ToString() == "2")
                {
                    dp.SetText(Regex.Split(Tag.ToString(), "/")[4]);
                }
                else if (selectedItem.Tag.ToString() == "3")
                {
                    dp.SetText(AppVersion.Text);
                }
                else if (selectedItem.Tag.ToString() == "24")
                {
                    dp.SetText(AppChangeLog.Text);
                }
                Clipboard.SetContent(dp);
            }
        }

        UserActivitySession _currentActivity;
        private async Task GenerateActivityAsync()
        {
            // Get the default UserActivityChannel and query it for our UserActivity. If the activity doesn't exist, one is created.
            UserActivityChannel channel = UserActivityChannel.GetDefault();
            UserActivity userActivity = await channel.GetOrCreateUserActivityAsync(Utils.GetMD5(AppLink));

            // Populate required properties
            userActivity.VisualElements.DisplayText = AppModels.Title;
            userActivity.VisualElements.AttributionDisplayText = AppModels.Title;
            userActivity.VisualElements.Description = AppModels.Introduce.Length > 3 ? AppModels.Introduce : AppModels.Description;
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
            _ = await Launcher.LaunchUriAsync(new Uri(AppLink));
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (ScreenShotFlipView.Visibility == Visibility.Visible) { _ = CloseFlip_ClickAsync(); }
            else { Frame.GoBack(); }
        }

        private void ScreenShotView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ScreenShotFlipView.Visibility = Visibility.Visible;
            ScreenShotView.SelectedIndex = -1;
            Rectangle_PointerEntered();
        }

        private async Task CloseFlip_ClickAsync()
        {
            Rectangle_PointerExited();
            await Task.Delay(300);
            ScreenShotFlipView.Visibility = Visibility.Collapsed;
        }


        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            // Download the URI
            _ = Launcher.LaunchUriAsync(new Uri(NetworkHelper.ExpandShortUrl(new Uri((sender as FrameworkElement).Tag.ToString()))));
        }

        private void ViewFeed(object sender, RoutedEventArgs e)
        {
            FeedListPageViewModelBase f = FeedListPageViewModelBase.GetProvider(FeedListType.AppPageList, id);
            if (f != null)
            {
                UIHelper.NavigateInSplitPane(typeof(FeedListPage), f);
            }
        }

        //private void TitleBar_BackButtonClick(object sender, RoutedEventArgs e) => Frame.GoBack();

        private void titleBar_RefreshButtonClicked(object sender, RoutedEventArgs e)
        {
            titleBar.ShowProgressRing();
            titleBar.HideProgressRing();
        }

        private void Rectangle_PointerEntered()
        {
            EnterStoryboard.Begin();
        }

        private void Rectangle_PointerExited()
        {
            ExitStoryboard.Begin();
        }
    }
    internal class AppModel : Entity
    {
        public string DownloadUrl { get; private set; }
        public string Url { get; private set; }
        public float Score { get; private set; }
        public string Vote { get; private set; }
        public string Title { get; private set; }
        public string EntityID { get; private set; }
        public string Version { get; private set; }
        public string Apksize { get; private set; }
        public string VoteCount { get; private set; }
        public string FollowNum { get; private set; }
        public string DownloadNum { get; private set; }
        public string ChangeLog { get; private set; }
        public string Introduce { get; private set; }
        public string Description { get; private set; }
        public string LastUpdate { get; private set; }
        public Models.ImageModel Logo { get; private set; }

        public AppModel(JObject json) : base(json)
{
            if (json.TryGetValue("apkDetailDownloadUrl", out JToken apkDetailDownloadUrl) && !string.IsNullOrEmpty(apkDetailDownloadUrl.ToString()))
            {
                DownloadUrl = UIHelper.IsSpecialUser ? apkDetailDownloadUrl.ToString() : string.Empty;
            }
            if (json.TryGetValue("dataRow", out JToken v1))
            {
                JObject dataRow = (JObject)v1;
                if (dataRow.TryGetValue("logo", out JToken logo) && !string.IsNullOrEmpty(logo.ToString()))
                {
                    Logo = new Models.ImageModel(logo.ToString(), ImageType.Icon);
                }
                if (dataRow.TryGetValue("id", out JToken id) && !string.IsNullOrEmpty(id.ToString()))
                {
                    EntityID = id.ToString();
                }
                if (dataRow.TryGetValue("title", out JToken title) && !string.IsNullOrEmpty(title.ToString()))
                {
                    Title = title.ToString();
                }
                else if (dataRow.TryGetValue("shorttitle", out JToken shorttitle) && !string.IsNullOrEmpty(shorttitle.ToString()))
                {
                    Title = shorttitle.ToString();
                }
                if (dataRow.TryGetValue("score_v10", out JToken score_v10) && !string.IsNullOrEmpty(score_v10.ToString()))
                {
                    Vote = score_v10.ToString();
                }
                if (dataRow.TryGetValue("votescore_v10", out JToken votescore_v10) && !string.IsNullOrEmpty(votescore_v10.ToString()))
                {
                    Score = float.Parse(votescore_v10.ToString()) / 2;
                }
                if (dataRow.TryGetValue("version", out JToken version) && !string.IsNullOrEmpty(version.ToString()))
                {
                    Version = version.ToString();
                }
                else if (dataRow.TryGetValue("apkversionname", out JToken apkversionname) && !string.IsNullOrEmpty(apkversionname.ToString()))
                {
                    Version = apkversionname.ToString();
                }
                else if (dataRow.TryGetValue("apkversioncode", out JToken apkversioncode) && !string.IsNullOrEmpty(apkversioncode.ToString()))
                {
                    Version = apkversioncode.ToString();
                }
                if (dataRow.TryGetValue("apksize", out JToken apksize) && !string.IsNullOrEmpty(apksize.ToString()))
                {
                    Apksize = apksize.ToString();
                }
                if (dataRow.TryGetValue("voteCount", out JToken voteCount) && !string.IsNullOrEmpty(voteCount.ToString()))
                {
                    VoteCount = voteCount.ToString();
                }
                if (dataRow.TryGetValue("changelog", out JToken changelog) && !string.IsNullOrEmpty(changelog.ToString()))
                {
                    ChangeLog = changelog.ToString();
                }
                if (dataRow.TryGetValue("introduce", out JToken introduce) && !string.IsNullOrEmpty(introduce.ToString()))
                {
                    Introduce = introduce.ToString();
                    Introduce = UIHelper.CSStoMarkDown(Introduce);
                }
                if (dataRow.TryGetValue("description", out JToken description) && !string.IsNullOrEmpty(description.ToString()))
                {
                    Description = description.ToString();
                }
                if (ChangeLog == null)
                {
                    ChangeLog = Description;
                    Description = null;
                }
            }
        }
    }
    }
