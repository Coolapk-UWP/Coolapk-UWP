using CoolapkUWP.Helpers;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel;

namespace CoolapkUWP.Models
{
    public class FeedModelBase : SourceFeedModel, ICanChangeLikModel, INotifyPropertyChanged, ICanChangeReplyNum, ICanCopy
    {
        private string likenum1;
        private string replynum;
        private bool isCopyEnabled;

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

        //public bool ShowSourceFeed2 { get => !ShowSourceFeed; }

        public string Info { get; private set; }
        public string ShareNum { get; private set; }
        public string DeviceTitle { get; private set; }
        public bool ShowSourceFeed { get; private set; }
        public bool ShowSourceFeedGrid { get; private set; }
        public SourceFeedModel SourceFeed { get; private set; }
        public bool ShowExtraUrl { get; private set; }
        public string ExtraTitle { get; private set; }
        public string ExtraUrl { get; private set; }
        public string ExtraUrl2 { get; private set; }
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
            if (name != null)
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public FeedModelBase(JObject token) : base(token)
        {
            if (token.TryGetValue("info", out JToken value1))
            {
                Info = value1.ToString();
            }

            Likenum = token["likenum"].ToString().Replace("\"", string.Empty, StringComparison.Ordinal);
            Replynum = token["replynum"].ToString().Replace("\"", string.Empty, StringComparison.Ordinal);
            ShareNum = token["forwardnum"].ToString().Replace("\"", string.Empty, StringComparison.Ordinal);
            if (token.Value<string>("entityType") != "article")
            {
                if (IsQuestionFeed)
                {
                    QuestionAnswerNum = token["question_answer_num"].ToString().Replace("\"", string.Empty, StringComparison.Ordinal);
                    QuestionFollowNum = token["question_follow_num"].ToString().Replace("\"", string.Empty, StringComparison.Ordinal);
                }
                ShowSourceFeedGrid = !IsQuestionFeed && !string.IsNullOrEmpty(token["source_id"]?.ToString());
                if (ShowSourceFeedGrid)
                {
                    ShowSourceFeed = token.TryGetValue("forwardSourceFeed", out JToken jsonValue) &&
                                     jsonValue != null &&
                                     jsonValue.ToString() != "null" &&
                                     string.IsNullOrEmpty(jsonValue.ToString());
                    if (ShowSourceFeed)
                    {
                        SourceFeed = new SourceFeedModel(jsonValue as JObject);
                    }
                }
                //if (token["entityTemplate"].GetString() == "feedByDyhHeader") showUser = false;
                if (ShowUser)
                {
                    var userSmallAvatarUrl = token["userInfo"].Value<string>("userSmallAvatar");
                    if (!string.IsNullOrEmpty(userSmallAvatarUrl))
                    {
                        UserSmallAvatar = new ImageModel(userSmallAvatarUrl, ImageType.BigAvatar);
                    }
                }

                ShowExtraUrl = token.TryGetValue("extra_title", out JToken valueextra_title) && !string.IsNullOrEmpty(valueextra_title.ToString());
                if (ShowExtraUrl)
                {
                    ExtraTitle = valueextra_title.ToString();
                    ExtraUrl = token.Value<string>("extra_url");
                    ExtraUrl2 = (ExtraUrl?.IndexOf("http", StringComparison.Ordinal) ?? -1) == 0 ? new Uri(ExtraUrl).Host : string.Empty;
                    var extraPicUrl = token.Value<string>("extra_pic");
                    if (!string.IsNullOrEmpty(extraPicUrl))
                    {
                        ExtraPic = new BackgroundImageModel(extraPicUrl, ImageType.Icon);
                    }
                }
                DeviceTitle = token.Value<string>("device_title");
            }
            //else showUser = false;
            Liked = token.TryGetValue("userAction", out JToken v) ? int.Parse(v["like"].ToString()) == 1 : false;
        }
    }
}