using CoolapkUWP.Helpers;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel;
using Windows.UI.Xaml.Media;

namespace CoolapkUWP.Models
{
    internal interface ILike
    {
        string Likenum { get; set; }
        bool Liked { get; set; }
        string Id { get; }
    }

    internal class FeedModelBase : SourceFeedModel, ILike, INotifyPropertyChanged
    {
        public FeedModelBase(JObject token) : base(token)
        {
            if (token.TryGetValue("info", out JToken value1))
                Info = value1.ToString();
            Likenum = token["likenum"].ToString().Replace("\"", string.Empty);
            Replynum = token["replynum"].ToString().Replace("\"", string.Empty);
            Share_num = token["forwardnum"].ToString().Replace("\"", string.Empty);
            if (token.Value<string>("entityType") != "article")
            {
                if (token.Value<string>("feedType") == "question")
                {
                    IsQuestionFeed = true;
                    Question_answer_num = token["question_answer_num"].ToString().Replace("\"", string.Empty);
                    Question_follow_num = token["question_follow_num"].ToString().Replace("\"", string.Empty);
                }
                ShowSourceFeedGrid = !IsQuestionFeed && !string.IsNullOrEmpty(token["source_id"]?.ToString());
                if (ShowSourceFeedGrid)
                {
                    ShowSourceFeed = token.TryGetValue("forwardSourceFeed", out JToken jsonValue)
                                     && jsonValue != null
                                     && jsonValue.ToString() != "null"
                                     && jsonValue.ToString() != string.Empty;
                    if (ShowSourceFeed)
                        SourceFeed = new SourceFeedModel(jsonValue as JObject);
                }
                //if (token["entityTemplate"].GetString() == "feedByDyhHeader") showUser = false;
                if (ShowUser)
                {
                    var userSmallAvatarUrl = token["userInfo"]["userSmallAvatar"].ToString();
                    if (!string.IsNullOrEmpty(userSmallAvatarUrl))
                        UserSmallAvatar = new ImageModel(userSmallAvatarUrl, ImageType.BigAvatar);
                }

                ShowExtra_url = token.TryGetValue("extra_title", out JToken valueextra_title) && !string.IsNullOrEmpty(valueextra_title.ToString());
                if (ShowExtra_url)
                {
                    Extra_title = valueextra_title.ToString();
                    Extra_url = token.Value<string>("extra_url");
                    if (!string.IsNullOrEmpty(Extra_url))
                        if (Extra_url.IndexOf("http") == 0)
                            Extra_url2 = new Uri(Extra_url).Host;
                        else Extra_url2 = string.Empty;
                    else Extra_url2 = string.Empty;
                    var extraPicUrl = token.Value<string>("extra_pic");
                    if (!string.IsNullOrEmpty(extraPicUrl))
                        Extra_pic = new ImageModel(extraPicUrl, ImageType.Icon);
                }
                Device_title = token.Value<string>("device_title");
            }
            //else showUser = false;
            Liked = token.TryGetValue("userAction", out JToken v) ? int.Parse(v["like"].ToString()) == 1 : false;
        }

        private string likenum1;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Info { get; private set; }
        public string Share_num { get; private set; }
        public string Device_title { get; private set; }
        public bool ShowSourceFeed { get; private set; }
        public bool ShowSourceFeed2 { get => !ShowSourceFeed; }
        public bool ShowSourceFeedGrid { get; private set; }
        public SourceFeedModel SourceFeed { get; private set; }
        public bool ShowExtra_url { get; private set; }
        public string Extra_title { get; private set; }
        public string Extra_url { get; private set; }
        public string Extra_url2 { get; private set; }
        public bool IsQuestionFeed { get; private set; }
        public string Question_answer_num { get; private set; }
        public string Question_follow_num { get; private set; }
        public bool ShowUser { get; private set; } = true;
        public bool ShowUser2 { get => !ShowUser; }

        public string Likenum
        {
            get => likenum1;
            set
            {
                likenum1 = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("likenum"));
            }
        }

        public string Replynum { get; private set; }
        public bool Liked { get; set; }
        public bool Liked2 { get => !Liked; }
        public string Id => EntityId;
        public ImageModel Extra_pic { get; private set; }
        public ImageModel UserSmallAvatar { get; private set; }
    }
}