using CoolapkUWP.Data;
using System;
using Windows.Data.Json;
using Windows.UI.Xaml.Media;

namespace CoolapkUWP.Control.ViewModels
{
    internal interface ILike
    {
        string likenum { get; set; }
        bool liked { get; set; }
        string id { get; }
    }

    internal class FeedViewModelBase : SourceFeedViewModel, ILike
    {
        public FeedViewModelBase(IJsonValue t) : base(t)
        {
            JsonObject token = t.GetObject();
            if (token.TryGetValue("info", out IJsonValue value1))
            { info = value1.GetString(); }
            likenum = token["likenum"].ToString().Replace("\"", string.Empty);
            replynum = token["replynum"].ToString().Replace("\"", string.Empty);
            share_num = token["forwardnum"].ToString().Replace("\"", string.Empty);
            if (token["entityType"].GetString() != "article")
            {
                if (token["feedType"].GetString() == "question")
                {
                    isQuestionFeed = true;
                    question_answer_num = token["question_answer_num"].ToString().Replace("\"", string.Empty);
                    question_follow_num = token["question_follow_num"].ToString().Replace("\"", string.Empty);
                }
                showSourceFeedGrid = !isQuestionFeed && !string.IsNullOrEmpty(token["source_id"]?.GetString());
                if (showSourceFeedGrid)
                {
                    showSourceFeed = token.TryGetValue("forwardSourceFeed", out IJsonValue jsonValue)
                                     && jsonValue != null
                                     && jsonValue.ToString() != "null";
                    if (showSourceFeed)
                    { sourceFeed = new SourceFeedViewModel(jsonValue.GetObject()); }
                }
                //if (token["entityTemplate"].GetString() == "feedByDyhHeader") showUser = false;
                if (showUser) { userSmallAvatarUrl = token["userInfo"].GetObject()["userSmallAvatar"].GetString(); }
                showExtra_url = token.TryGetValue("extra_title", out IJsonValue valueextra_title) && !string.IsNullOrEmpty(valueextra_title.GetString());
                if (showExtra_url)
                {
                    extra_title = valueextra_title.GetString();
                    extra_url = token["extra_url"].GetString();
                    extra_url2 = !string.IsNullOrEmpty(extra_url) ? extra_url.IndexOf("http") == 0 ? new Uri(extra_url).Host : string.Empty : string.Empty;
                    extraPicUrl = token["extra_pic"].GetString();
                }
                device_title = token["device_title"].GetString();
            }
            //else showUser = false;
            liked = token["extra_fromApi"].GetString() != "V11_HOME_TAB_NEWS"
                && token.TryGetValue("userAction", out IJsonValue v) && v.GetObject()["like"].GetNumber() == 1;
            GetPic();
        }

        private async void GetPic()
        {
            if (!string.IsNullOrEmpty(userSmallAvatarUrl))
            { userSmallAvatar = await ImageCache.GetImage(ImageType.SmallAvatar, userSmallAvatarUrl); }
            if (!string.IsNullOrEmpty(extraPicUrl))
            { extra_pic = await ImageCache.GetImage(ImageType.Icon, extraPicUrl); }
        }

        public string userSmallAvatarUrl;
        private readonly string extraPicUrl;
        private ImageSource extra_pic1;
        private ImageSource userSmallAvatar1;
        private string likenum1;

        public string info { get; private set; }
        public string share_num { get; private set; }
        public string device_title { get; private set; }
        public bool showSourceFeed { get; private set; }
        public bool showSourceFeed2 { get => !showSourceFeed; }
        public bool showSourceFeedGrid { get; private set; }
        public SourceFeedViewModel sourceFeed { get; private set; }
        public bool showExtra_url { get; private set; }
        public string extra_title { get; private set; }
        public string extra_url { get; private set; }
        public string extra_url2 { get; private set; }
        public bool isQuestionFeed { get; private set; }
        public string question_answer_num { get; private set; }
        public string question_follow_num { get; private set; }
        public bool showUser { get; private set; } = true;
        public bool showUser2 { get => !showUser; }
        public string likenum
        {
            get => likenum1;
            set
            {
                likenum1 = value;
                Changed(this, "likenum");
            }
        }
        public string replynum { get; private set; }
        public bool liked { get; set; }
        public bool liked2 { get => !liked; }
        public ImageSource extra_pic
        {
            get => extra_pic1;
            private set
            {
                extra_pic1 = value;
                Changed(this, nameof(extra_pic));
            }
        }
        public ImageSource userSmallAvatar
        {
            get => userSmallAvatar1;
            private set
            {
                userSmallAvatar1 = value;
                Changed(this, nameof(userSmallAvatar));
            }
        }

        public string id => entityId;
    }
}