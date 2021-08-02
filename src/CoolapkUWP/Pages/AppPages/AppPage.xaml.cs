using CoolapkUWP.Core.Helpers;
using CoolapkUWP.Core.Models;
using CoolapkUWP.Helpers;
using CoolapkUWP.Pages.FeedPages;
using CoolapkUWP.ViewModels.FeedListPage;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.UserActivities;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace CoolapkUWP.Pages.AppPages
{
    /// <summary>
    /// 这是小板子传承的最后的东西了，只有这个页面从第一个版本流传至今，希望后人不要把它删了(●'◡'●)
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

        private static (bool, JToken) GetResult(string json)
        {
            JObject o = JObject.Parse(json);
            if (!string.IsNullOrEmpty(json) &&
                !o.TryGetValue("dataRow", out _) &&
                o.TryGetValue("message", out JToken message))
            {
                UIHelper.ShowMessage(message.ToString());
                return (false, null);
            }
            else { return (true, o); }
        }

        private async void GetAppDetail()
        {
            titleBar.ShowProgressRing();
            (bool isSucceed, string result) = await DataHelper.GetHtmlAsync(new Uri(AppLink), "XMLHttpRequest");
            if (isSucceed && !string.IsNullOrEmpty(result))
            {
                (bool isNotNull, JToken json) = GetResult(result);
                if (isNotNull)
                {
                    AppModels = new AppModel((JObject)json);
                    _ = GenerateActivityAsync();
                }
                else
                {
                    Regex regex = new Regex(@"https://www.coolapk.com/\w+/(\d+)");
                    if (regex.IsMatch(AppLink) && !string.IsNullOrEmpty(regex.Match(AppLink).Groups[1].Value))
                    {
                        string id = regex.Match(AppLink).Groups[1].Value;
                        FeedListPageViewModelBase f = FeedListPageViewModelBase.GetProvider(FeedListType.AppPageList, id);
                        if (f != null)
                        {
                            Frame.GoBack();
                            UIHelper.NavigateInSplitPane(typeof(FeedListPage), f);
                        }
                    }
                }
            }
            titleBar.HideProgressRing();
        }

        private void CopyM_Click(object sender, RoutedEventArgs e)
        {

            if (sender is MenuFlyoutItem selectedItem)
            {
                DataPackage dp = new DataPackage();
                // copy 
                FrameworkElement element = sender as FrameworkElement;
                switch (element.Name)
                {
                    case "Introduce": dp.SetText(AppModels.Introduce); break;
                    case "Description": dp.SetText(AppModels.Description); break;
                    case "PackageName": dp.SetText(AppModels.PackageName); break;
                    case "Version": dp.SetText(AppModels.Version); break;
                    case "ChangeLog": dp.SetText(AppModels.ChangeLog); break;
                }
                Clipboard.SetContent(dp);
            }
        }

        void FeedPage_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            Uri shareLinkString = ValidateAndGetUri(AppLink);
            if (shareLinkString != null)
            {
                DataPackage dataPackage = new DataPackage();
                dataPackage.SetWebLink(shareLinkString);
                dataPackage.Properties.Title = "应用分享";
                dataPackage.Properties.Description = AppLink;
                DataRequest request = args.Request;
                request.Data = dataPackage;
            }
            else
            {
                DataPackage dataPackage = new DataPackage();
                dataPackage.SetText(AppLink);
                dataPackage.Properties.Title = "内容分享";
                dataPackage.Properties.Description = "内含文本";
                DataRequest request = args.Request;
                request.Data = dataPackage;
            }
        }

        private static Uri ValidateAndGetUri(string uriString)
        {
            Uri uri = null;
            try
            {
                uri = new Uri(uriString);
            }
            catch (FormatException)
            {
            }
            return uri;
        }

        protected void ShowUIButton_Click(object sender, RoutedEventArgs e)
        {
            DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested -= FeedPage_DataRequested;
            dataTransferManager.DataRequested += FeedPage_DataRequested;
            DataTransferManager.ShowShareUI();
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            switch (element.Name)
            {
                case "Report":
                    UIHelper.Navigate(typeof(BrowserPage), new object[] { false, "https://m.coolapk.com/mp/apk/report?apkname=" + AppModels.PackageName });
                    break;

                case "GotoUri":
                    _ = Launcher.LaunchUriAsync(new Uri(AppLink));
                    break;

                case "UPanel":
                case "ViewFeed":
                    FeedListPageViewModelBase f = FeedListPageViewModelBase.GetProvider(FeedListType.AppPageList, element.Tag.ToString());
                    if (f != null)
                    {
                        UIHelper.NavigateInSplitPane(typeof(FeedListPage), f);
                    }
                    break;

                case "DownloadButton":
                    _ = Launcher.LaunchUriAsync(new Uri((sender as FrameworkElement).Tag.ToString()));
                    break;

                default:
                    UIHelper.OpenLinkAsync((sender as FrameworkElement).Tag as string);
                    break;
            }
        }

        private void titleBar_RefreshButtonClicked(object sender, RoutedEventArgs e)
        {
            GetAppDetail();
            _ = scrollViewer.ChangeView(null, 0, null);
        }

        private void titleBar_BackButtonClicked(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
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
        public string PubDate { get; private set; }
        public string LastUpdate { get; private set; }
        public string PackageName { get; private set; }
        public string ApkRomVersion { get; private set; }
        public string DeveloperName { get; private set; }
        public string DeveloperUrl { get; private set; }
        public bool ShowPicArr { get; private set; }
        public Models.ImageModel Logo { get; private set; }
        public Models.ImageModel DeveloperAvatar { get; private set; }
        public ImmutableArray<Models.ImageModel> PicArr { get; private set; }

        public AppModel(JObject json) : base(json)
        {
            if (json.TryGetValue("dataRow", out JToken v1) && !string.IsNullOrEmpty(v1.ToString()))
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
                if (dataRow.TryGetValue("apklength", out JToken apklength) && !string.IsNullOrEmpty(apklength.ToString()))
                {
                    Apksize = Utils.GetSizeString(double.Parse(apklength.ToString()));
                }
                else if (dataRow.TryGetValue("apksize", out JToken apksize) && !string.IsNullOrEmpty(apksize.ToString()))
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
                    Introduce = Utils.CSStoMarkDown(Introduce);
                }
                if (dataRow.TryGetValue("description", out JToken description) && !string.IsNullOrEmpty(description.ToString()))
                {
                    Description = description.ToString();
                }
                else if (dataRow.TryGetValue("subtitle", out JToken subtitle) && !string.IsNullOrEmpty(subtitle.ToString()))
                {
                    Description = subtitle.ToString();
                }
                else if (dataRow.TryGetValue("remark", out JToken remark) && !string.IsNullOrEmpty(remark.ToString()))
                {
                    Description = remark.ToString();
                    Description = Utils.CSStoString(Description);
                }
                if (dataRow.TryGetValue("hot_num", out JToken hot_num) && !string.IsNullOrEmpty(hot_num.ToString()))
                {
                    HotNum = Utils.GetNumString(double.Parse(hot_num.ToString()));
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
                if (dataRow.TryGetValue("pubdate", out JToken pubdate) && !string.IsNullOrEmpty(pubdate.ToString()))
                {
                    PubDate = DataHelper.ConvertUnixTimeStampToReadable(double.Parse(pubdate.ToString()));
                }
                if (dataRow.TryGetValue("lastupdate", out JToken lastupdate) && !string.IsNullOrEmpty(lastupdate.ToString()))
                {
                    LastUpdate = DataHelper.ConvertUnixTimeStampToReadable(double.Parse(lastupdate.ToString()));
                }
                if (dataRow.TryGetValue("apkRomVersion", out JToken apkRomVersion) && !string.IsNullOrEmpty(apkRomVersion.ToString()))
                {
                    ApkRomVersion = "Android " + apkRomVersion.ToString();
                }
                if (dataRow.TryGetValue("packageName", out JToken packageName) && !string.IsNullOrEmpty(packageName.ToString()))
                {
                    PackageName = packageName.ToString();
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
                if (dataRow.TryGetValue("developerProfile", out JToken v2) && !string.IsNullOrEmpty(v2.ToString()))
                {
                    JObject developerProfile = (JObject)v2;
                    if (developerProfile.TryGetValue("username", out JToken username) && !string.IsNullOrEmpty(username.ToString()))
                    {
                        DeveloperName = username.ToString();
                    }
                    if (developerProfile.TryGetValue("userAvatar", out JToken userAvatar) && !string.IsNullOrEmpty(userAvatar.ToString()))
                    {
                        DeveloperAvatar = new Models.ImageModel(userAvatar.ToString(), ImageType.BigAvatar);
                    }
                    if (developerProfile.TryGetValue("uid", out JToken uid) && !string.IsNullOrEmpty(uid.ToString()))
                    {
                        DeveloperUrl = "/u/" + uid.ToString();
                    }
                }
                if (string.IsNullOrEmpty(DeveloperName) || DeveloperAvatar == null)
                {
                    if (dataRow.TryGetValue("developername", out JToken developername) && !string.IsNullOrEmpty(developername.ToString()))
                    {
                        DeveloperName = developername.ToString();
                    }
                    else if (dataRow.TryGetValue("updatername", out JToken updatername) && !string.IsNullOrEmpty(updatername.ToString()))
                    {
                        DeveloperName = updatername.ToString();
                    }
                    else if (dataRow.TryGetValue("creatorname", out JToken creatorname) && !string.IsNullOrEmpty(creatorname.ToString()))
                    {
                        DeveloperName = creatorname.ToString();
                    }
                    if (dataRow.TryGetValue("developerurl", out JToken developerurl) && !string.IsNullOrEmpty(developerurl.ToString()))
                    {
                        DeveloperUrl = developerurl.ToString();
                    }
                    else if (dataRow.TryGetValue("developeruid", out JToken developeruid) && !string.IsNullOrEmpty(developeruid.ToString()) && int.Parse(developeruid.ToString()) != 0)
                    {
                        DeveloperUrl = "/u/" + developeruid.ToString();
                    }
                    else if (dataRow.TryGetValue("updateruid", out JToken updateruid) && !string.IsNullOrEmpty(updateruid.ToString()) && int.Parse(updateruid.ToString()) != 0)
                    {
                        DeveloperUrl = "/u/" + updateruid.ToString();
                    }
                    else if (dataRow.TryGetValue("creatoruid", out JToken creatoruid) && !string.IsNullOrEmpty(creatoruid.ToString()) && int.Parse(creatoruid.ToString()) != 0)
                    {
                        DeveloperUrl = "/u/" + creatoruid.ToString();
                    }
                    if (DeveloperAvatar == null)
                    {
                        DeveloperAvatar = Logo;
                    }
                }
            }
        }
    }
}