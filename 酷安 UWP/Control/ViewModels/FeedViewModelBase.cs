using System;
using Windows.Data.Json;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace CoolapkUWP.Control.ViewModels
{
    class FeedViewModelBase : SourceFeedViewModel
    {
        public FeedViewModelBase(IJsonValue t) : base(t)
        {
            JsonObject token = t.GetObject();
            if (token.TryGetValue("info", out IJsonValue value1))
                info = value1.GetString();
            likenum = token["likenum"].ToString().Replace("\"", string.Empty);
            replynum = token["replynum"].ToString().Replace("\"", string.Empty);
            share_num = token["forwardnum"].ToString().Replace("\"", string.Empty);
            if (token["entityType"].GetString() != "article")
            {
                showSourceFeedGrid = !string.IsNullOrEmpty(token["source_id"]?.GetString());
                if (showSourceFeedGrid)
                {
                    showSourceFeed = token.TryGetValue("forwardSourceFeed", out IJsonValue jsonValue)
                                     && jsonValue != null
                                     && jsonValue.ToString() != "null";
                    if (showSourceFeed)
                        sourceFeed = new SourceFeedViewModel(jsonValue.GetObject());
                }
                if (token["feedType"].GetString() == "question")
                {
                    isQuestionFeed = true;
                    question_answer_num = token["question_answer_num"].ToString().Replace("\"", string.Empty);
                    question_follow_num = token["question_follow_num"].ToString().Replace("\"", string.Empty);
                }
                showUser = true;
                if (!string.IsNullOrEmpty(token["userInfo"].GetObject()["userSmallAvatar"].GetString()))
                    userSmallAvatar = new BitmapImage(new Uri(token["userInfo"].GetObject()["userSmallAvatar"].GetString()));

                showExtra_url = token.TryGetValue("extra_title", out IJsonValue valueextra_title) && !string.IsNullOrEmpty(valueextra_title.GetString());
                if (showExtra_url)
                {
                    extra_title = valueextra_title.GetString();
                    extra_url = token["extra_url"].GetString();
                    if (!string.IsNullOrEmpty(extra_url))
                        if (extra_url.IndexOf("http") == 0)
                            extra_url2 = new Uri(extra_url).Host;
                        else extra_url2 = string.Empty;
                    else extra_url2 = string.Empty;
                    if (!string.IsNullOrEmpty(token["extra_pic"].GetString()))
                        extra_pic = new BitmapImage(new Uri(token["extra_pic"].GetString()));
                }
                device_title = token["device_title"].GetString();
            }
        }
        public string info { get; private set; }
        public string likenum { get; private set; }
        public string replynum { get; private set; }
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
        public ImageSource extra_pic { get; private set; } = new BitmapImage();
        public bool isQuestionFeed { get; private set; }
        public string question_answer_num { get; private set; }
        public string question_follow_num { get; private set; }
        public bool showUser { get; private set; }
        public bool showUser2 { get => !showUser; }
        public ImageSource userSmallAvatar { get; private set; } = new BitmapImage();
    }
}
