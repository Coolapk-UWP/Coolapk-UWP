using CoolapkUWP.Core.Helpers;
using CoolapkUWP.Core.Models;
using CoolapkUWP.Helpers;
using CoolapkUWP.Pages.FeedPages;
using CoolapkUWP.ViewModels.FeedListPage;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Data;
using System.Linq;
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
        private string AppMassage, AppUpdateTime;
        private string AppLink;

        public event PropertyChangedEventHandler PropertyChanged;

        internal void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        public AppPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            //将传过来的数据 类型转换一下
            AppLink = e.Parameter as string;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(AppLink)) { return; }
            else { GetAppDetail(); }
        }

        private async void GetAppDetail()
        {
            titleBar.ShowProgressRing();
            (bool isSucceed, string result) = await DataHelper.GetHtmlAsync(new Uri(AppLink), "XMLHttpRequest");
            if (isSucceed && !string.IsNullOrEmpty(result))
            {
                JObject json = JObject.Parse(result);
                AppModels = new AppModel(json);
                _ = GenerateActivityAsync();
            }
            (bool HTMLisSucceed, string html) = await DataHelper.GetHtmlAsync(new Uri(AppLink), "com.coolapk.market");
            if (HTMLisSucceed) { LaunchAppViewLoad(html); }
            titleBar.HideProgressRing();
        }

        private void LaunchAppViewLoad(string str)
        {
            #region 小板子的HTML内容获取
            AppMassage = Regex.Split(str, @"<p class=""apk_topba_message"">")[1].Split('<')[0].Trim().Replace("\n", "").Replace(" ", "");
            AppUpdateTime = Regex.Split(str, "更新时间：")[1].Split('<')[0].Trim();

            AppVTText.Text = AppUpdateTime;
            AppXText.Text = Regex.Split(AppMassage, "/")[1] + " · " + Regex.Split(AppMassage, "/")[0];
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

        private UserActivitySession _currentActivity;
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
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }

        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            // Download the URI
            _ = Launcher.LaunchUriAsync(new Uri((sender as FrameworkElement).Tag.ToString()));
        }

        private void ViewFeed(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            FeedListPageViewModelBase f = FeedListPageViewModelBase.GetProvider(FeedListType.AppPageList, element.Tag.ToString());
            if (f != null)
            {
                UIHelper.NavigateInSplitPane(typeof(FeedListPage), f);
            }
        }

        private void titleBar_RefreshButtonClicked(object sender, RoutedEventArgs e)
        {
            GetAppDetail();
            _ = scrollViewer.ChangeView(null, 0, null);
        }

        private void titleBar_Loaded(object sender, RoutedEventArgs e)
        {

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
        public string HotNum { get; private set; }
        public string VoteCount { get; private set; }
        public string FollowNum { get; private set; }
        public string CommentNum { get; private set; }
        public string DownloadNum { get; private set; }
        public string ChangeLog { get; private set; }
        public string Introduce { get; private set; }
        public string Description { get; private set; }
        public string LastUpdate { get; private set; }
        public bool ShowPicArr { get; private set; }
        public Models.ImageModel Logo { get; private set; }
        public ImmutableArray<Models.ImageModel> PicArr { get; private set; }

        public AppModel(JObject json) : base(json)
        {
            if (json.TryGetValue("dataRow", out JToken v1))
            {
                JObject dataRow = (JObject)v1;
                if (dataRow.TryGetValue("logo", out JToken logo) && !string.IsNullOrEmpty(logo.ToString()))
                {
                    Logo = new Models.ImageModel(logo.ToString(), ImageType.Icon);
                }
                if (dataRow.TryGetValue("apkfile", out JToken apkfile) && apkfile.ToString().IndexOf("http") == 0)
                {
                    DownloadUrl = UIHelper.IsSpecialUser ? apkfile.ToString() : string.Empty;
                }
                else if (json.TryGetValue("apkDetailDownloadUrl", out JToken apkDetailDownloadUrl) && !string.IsNullOrEmpty(apkDetailDownloadUrl.ToString()))
                {
                    DownloadUrl = UIHelper.IsSpecialUser ? NetworkHelper.ExpandShortUrl(new Uri(apkDetailDownloadUrl.ToString())) : string.Empty;
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
                if (dataRow.TryGetValue("changelog", out JToken changelog) && !string.IsNullOrEmpty(changelog.ToString()))
                {
                    ChangeLog = changelog.ToString();
                }
                if (dataRow.TryGetValue("introduce", out JToken introduce) && !string.IsNullOrEmpty(introduce.ToString()))
                {
                    Introduce = introduce.ToString();
                    Introduce = UIHelper.CSStoMarkDown(Introduce);
                }
                if (dataRow.TryGetValue("remark", out JToken remark) && !string.IsNullOrEmpty(remark.ToString()))
                {
                    Description = remark.ToString();
                    Description = UIHelper.CSStoMarkDown(Description);
                }
                else if (dataRow.TryGetValue("description", out JToken description) && !string.IsNullOrEmpty(description.ToString()))
                {
                    Description = description.ToString();
                }
                else if (dataRow.TryGetValue("subtitle", out JToken subtitle) && !string.IsNullOrEmpty(subtitle.ToString()))
                {
                    Description = subtitle.ToString();
                }
                if (dataRow.TryGetValue("hot_num", out JToken hot_num) && !string.IsNullOrEmpty(hot_num.ToString()))
                {
                    HotNum = hot_num.ToString();
                }
                if (dataRow.TryGetValue("voteCount", out JToken voteCount) && !string.IsNullOrEmpty(voteCount.ToString()))
                {
                    VoteCount = voteCount.ToString();
                }
                if (dataRow.TryGetValue("followCount", out JToken followCount) && !string.IsNullOrEmpty(followCount.ToString()))
                {
                    FollowNum = followCount.ToString();
                }
                if (dataRow.TryGetValue("commentCount", out JToken commentCount) && !string.IsNullOrEmpty(commentCount.ToString()))
                {
                    CommentNum = commentCount.ToString();
                }
                if (dataRow.TryGetValue("downCount", out JToken downCount) && !string.IsNullOrEmpty(downCount.ToString()))
                {
                    DownloadNum = downCount.ToString();
                }
                if (ChangeLog == null)
                {
                    ChangeLog = Description;
                    Description = null;
                }
                ShowPicArr = dataRow.TryGetValue("screenList", out JToken screenList) && (screenList as JArray).Count > 0 && screenList != null;
                if (ShowPicArr)
                {
                    PicArr = (from item in screenList
                              select new Models.ImageModel(item.ToString(), ImageType.OriginImage)).ToImmutableArray();

                    foreach (Models.ImageModel item in PicArr)
                    {
                        item.ContextArray = PicArr;
                    }
                }
            }
        }
    }
}