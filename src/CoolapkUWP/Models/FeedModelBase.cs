using CoolapkUWP.Core.Helpers;
using CoolapkUWP.Helpers;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace CoolapkUWP.Models
{
    public class FeedModelBase : SourceFeedModel, ICanChangeLikModel, INotifyPropertyChanged, ICanChangeReplyNum, ICanCopy
    {
        private string likenum1;
        private string replynum;
        private bool isCopyEnabled;
        private bool showLinkSourceFeed;

        public string Likenum
        {
            get => likenum1;
            set
            {
                likenum1 = value;
                RaisePropertyChangedEvent();
            }
        }

        public string Replynum
        {
            get => replynum;
            set
            {
                replynum = value;
                RaisePropertyChangedEvent();
            }
        }

        public bool IsCopyEnabled
        {
            get => isCopyEnabled;
            set
            {
                isCopyEnabled = value;
                RaisePropertyChangedEvent();
            }
        }

        public bool ShowLinkSourceFeed
        {
            get => showLinkSourceFeed;
            set
            {
                showLinkSourceFeed = value;
                RaisePropertyChangedEvent();
            }
        }

        //public bool ShowSourceFeed2 { get => !ShowSourceFeed; }

        public string Info { get; private set; }
        public string ShareNum { get; private set; }
        public string DeviceTitle { get; private set; }
        public string ChangeCount { get; private set; }
        public string ChangeTitle { get; private set; }
        public bool ShowSourceFeed { get; private set; }
        public bool ShowSourceFeedGrid { get; private set; }
        public SourceFeedModel SourceFeed { get; private set; }
        public Links.SourceFeedModel LinkSourceFeed { get; private set; }
        public bool ShowExtraUrl { get; private set; }
        public bool ShowVideo { get; private set; }
        public string ExtraTitle { get; private set; }
        public string ExtraUrl { get; private set; }
        public string ExtraUrl2 { get; private set; }
        public string VideoUrl { get; private set; }
        public string QuestionAnswerNum { get; private set; }
        public string QuestionFollowNum { get; private set; }
        public bool ShowUser { get; private set; } = true;
        public bool ShowUser2 { get => !ShowUser; }
        public bool Liked { get; set; }
        public bool Liked2 { get => !Liked; }
        public string Id => EntityId;
        public BackgroundImageModel ExtraPic { get; private set; }
        public ImageModel UserSmallAvatar { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        internal void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        public FeedModelBase(JObject token) : base(token)
        {
            if (token.TryGetValue("info", out JToken info))
            {
                Info = info.ToString();
            }
            VideoUrl = null;
            if (token.TryGetValue("likenum", out JToken likenum))
            {
                Likenum = likenum.ToString().Replace("\"", string.Empty, StringComparison.Ordinal);
            }
            if (token.TryGetValue("replynum", out JToken replynum))
            {
                Replynum = replynum.ToString().Replace("\"", string.Empty, StringComparison.Ordinal);
            }
            if (token.TryGetValue("forwardnum", out JToken forwardnum))
            {
                ShareNum = forwardnum.ToString().Replace("\"", string.Empty, StringComparison.Ordinal);
            }
            ChangeCount = token.TryGetValue("change_count", out JToken change_count) || change_count != null
                ? change_count.ToString().Replace("\"", string.Empty, StringComparison.Ordinal)
                : token["isModified"].ToString().Replace("\"", string.Empty, StringComparison.Ordinal);
            ChangeTitle = ChangeCount == "0" ? null : "已编辑" + ChangeCount + "次";
            if (token.TryGetValue("entityType",out JToken entityType) && entityType.ToString() != "article")
            {
                if (IsQuestionFeed)
                {
                    if (token.TryGetValue("question_answer_num", out JToken question_answer_num))
                    {
                        QuestionAnswerNum = question_answer_num.ToString().Replace("\"", string.Empty, StringComparison.Ordinal);
                    }
                    if (token.TryGetValue("question_follow_num", out JToken question_follow_num))
                    {
                        QuestionFollowNum = question_follow_num.ToString().Replace("\"", string.Empty, StringComparison.Ordinal);
                    }
                }
                ShowSourceFeedGrid = !IsQuestionFeed && (!string.IsNullOrEmpty(token["forwardid"]?.ToString()) || (!string.IsNullOrEmpty(token["source_id"]?.ToString())));
                if (ShowSourceFeedGrid)
                {
                    ShowSourceFeed = token.TryGetValue("forwardSourceFeed", out JToken jsonValue) &&
                                     jsonValue != null &&
                                     jsonValue.ToString() != "null" &&
                                     !string.IsNullOrEmpty(jsonValue.ToString());
                    if (ShowSourceFeed)
                    {
                        SourceFeed = new SourceFeedModel(jsonValue as JObject);
                    }
                }
                //if (token["entityTemplate"].GetString() == "feedByDyhHeader") showUser = false;
                if (ShowUser)
                {
                    if (token.TryGetValue("userInfo", out JToken v2) && !string.IsNullOrEmpty(v2.ToString()))
                    {
                        JObject userInfo = (JObject)v2;
                        if (userInfo.TryGetValue("userSmallAvatar", out JToken userSmallAvatar))
                        { UserSmallAvatar = new ImageModel(userSmallAvatar.ToString(), ImageType.BigAvatar); }
                    }
                    else
                    {
                        if (token.TryGetValue("userAvatar", out JToken userAvatar))
                        { UserSmallAvatar = new ImageModel(userAvatar.ToString(), ImageType.BigAvatar); }
                    }
                }

                ShowExtraUrl = token.TryGetValue("extra_title", out JToken valueextra_title) && !string.IsNullOrEmpty(valueextra_title.ToString());
                if (ShowExtraUrl)
                {
                    if (token.TryGetValue("extra_url", out JToken extra_url))
                    {
                        ExtraUrl = extra_url.ToString();
                        if (ExtraUrl.Contains("b23.tv")|| ExtraUrl.Contains("t.cn"))
                        {
                            ExtraUrl = NetworkHelper.ExpandShortUrl(new Uri(ExtraUrl));
                        }
                    }
                    if (ExtraUrl.Contains("coolapk") && ExtraUrl.Contains("feed"))
                    {
                        LinkSourceFeed = new Links.SourceFeedModel(new Uri(ExtraUrl), Links.LinkType.Coolapk);
                        ShowLinkSourceFeed = true;
                    }
                    else if (ExtraUrl.Contains("bilibili") && ExtraUrl.Contains("t.bilibili"))
                    {
                        Regex GetID = new Regex(@"/t.*?/([\d|\w]+)");
                        Uri uri = UriHelper.GetLinkUri(UriType.GetBilibiliFeed, LinkType.Bilibili, GetID.Match(ExtraUrl).Groups[1].Value);
                        MultipartFormDataContent content = new MultipartFormDataContent { { new StringContent(GetID.Match(ExtraUrl).Groups[1].Value), "dynamic_id" } };
                        LinkSourceFeed = new Links.SourceFeedModel(uri, Links.LinkType.Bilibili, true, content);
                        ShowLinkSourceFeed = true;
                    }
                    else if (ExtraUrl.Contains("ithome") && ExtraUrl.Contains("qcontent"))
                    {
                        Regex GetID = new Regex(@"[%26|%3F]id%3D([\d|\w]+)");
                        Uri uri = UriHelper.GetLinkUri(UriType.GetITHomeFeed, LinkType.ITHome, GetID.Match(ExtraUrl).Groups[1].Value);
                        LinkSourceFeed = new Links.SourceFeedModel(uri, Links.LinkType.ITHome);
                        ShowLinkSourceFeed = true;
                    }
                    ExtraTitle = valueextra_title.ToString();
                    ExtraUrl2 = (ExtraUrl?.IndexOf("http", StringComparison.Ordinal) ?? -1) == 0 ? new Uri(ExtraUrl).Host : string.Empty;
                    if (token.TryGetValue("extra_pic", out JToken extra_pic) && !string.IsNullOrEmpty(extra_pic.ToString()))
                    {
                        ExtraPic = new BackgroundImageModel(extra_pic.ToString(), ImageType.Icon);
                    }
                }

                ShowVideo = token.TryGetValue("media_type", out JToken media_type) && media_type.ToString() == "2" && token.TryGetValue("media_url", out JToken media_url) && (media_url.Contains("bilibili") || media_url.Contains("b23"));

                if (media_type.ToString() != "0")
                {
                    ShowExtraUrl = true;
                    ExtraTitle = "视频分享";
                    ExtraUrl = token.Value<string>("media_url");
                    ExtraUrl2 = (ExtraUrl?.IndexOf("http", StringComparison.Ordinal) ?? -1) == 0 ? new Uri(ExtraUrl).Host : string.Empty;
                    string extraPicUrl = token.Value<string>("media_pic");
                    if (!string.IsNullOrEmpty(extraPicUrl))
                    {
                        ExtraPic = new BackgroundImageModel(extraPicUrl, ImageType.Icon);
                    }
                }

                if (token.TryGetValue("device_title", out JToken device_title))
                {
                    DeviceTitle = device_title.ToString();
                }
            }

            if(token.TryGetValue("userAction", out JToken v1))
            {
                JObject userAction = (JObject)v1;
                Liked = userAction.TryGetValue("like", out JToken like) && like.ToString() == "1";
            }
        }
    }
}