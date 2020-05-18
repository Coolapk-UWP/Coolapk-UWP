using CoolapkUWP.Helpers;
using Newtonsoft.Json.Linq;
using System;
using Windows.UI.Xaml.Media;

namespace CoolapkUWP.Controls.ViewModels
{
    interface ILike
    {
        string likenum { get; set; }
        bool liked { get; set; }
        string id { get; }
    }

    class FeedViewModelBase : SourceFeedViewModel, ILike
    {
        public FeedViewModelBase(JToken t) : base(t)
        {
            JObject token = t as JObject;
            if (token.TryGetValue("info", out JToken value1))
                info = value1.ToString();
            likenum = token["likenum"].ToString().Replace("\"", string.Empty);
            replynum = token["replynum"].ToString().Replace("\"", string.Empty);
            share_num = token["forwardnum"].ToString().Replace("\"", string.Empty);
            if (token.Value<string>("entityType") != "article")
            {
                if (token.Value<string>("feedType") == "question")
                {
                    isQuestionFeed = true;
                    question_answer_num = token["question_answer_num"].ToString().Replace("\"", string.Empty);
                    question_follow_num = token["question_follow_num"].ToString().Replace("\"", string.Empty);
                }
                showSourceFeedGrid = !isQuestionFeed && !string.IsNullOrEmpty(token["source_id"]?.ToString());
                if (showSourceFeedGrid)
                {
                    showSourceFeed = token.TryGetValue("forwardSourceFeed", out JToken jsonValue)
                                     && jsonValue != null
                                     && jsonValue.ToString() != "null"
                                     && jsonValue.ToString() != string.Empty;
                    if (showSourceFeed)
                        sourceFeed = new SourceFeedViewModel(jsonValue as JObject);
                }
                //if (token["entityTemplate"].GetString() == "feedByDyhHeader") showUser = false;
                if (showUser) userSmallAvatarUrl = token["userInfo"]["userSmallAvatar"].ToString();
                showExtra_url = token.TryGetValue("extra_title", out JToken valueextra_title) && !string.IsNullOrEmpty(valueextra_title.ToString());
                if (showExtra_url)
                {
                    extra_title = valueextra_title.ToString();
                    extra_url = token.Value<string>("extra_url");
                    if (!string.IsNullOrEmpty(extra_url))
                        if (extra_url.IndexOf("http") == 0)
                            extra_url2 = new Uri(extra_url).Host;
                        else extra_url2 = string.Empty;
                    else extra_url2 = string.Empty;
                    extraPicUrl = token.Value<string>("extra_pic");
                }
                device_title = token.Value<string>("device_title");
            }
            //else showUser = false;
            liked = token.TryGetValue("userAction", out JToken v) ? int.Parse(v["like"].ToString()) == 1 : false;
            GetPic();
        }

        private async void GetPic()
        {
            if (!string.IsNullOrEmpty(userSmallAvatarUrl))
                userSmallAvatar = await ImageCacheHelper.GetImage(ImageType.SmallAvatar, userSmallAvatarUrl);
            if (!string.IsNullOrEmpty(extraPicUrl))
                extra_pic = await ImageCacheHelper.GetImage(ImageType.Icon, extraPicUrl);
        }

        string userSmallAvatarUrl;
        string extraPicUrl;
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