using CoolapkUWP.Helpers;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Media;

namespace CoolapkUWP.Controls.ViewModels
{
    class FeedDetailViewModel : FeedViewModelBase
    {
        public FeedDetailViewModel(JToken t) : base(t)
        {
            JObject token = t as JObject;
            title = token.Value<string>("title");
            if (token.Value<string>("entityType") != "article")
            {
                if (token.TryGetValue("share_num", out JToken s))
                    share_num = s.ToString().Replace("\"", string.Empty);
                if (token.Value<string>("feedType") == "feedArticle")
                    isFeedArticle = true;
                if (isFeedArticle)
                {
                    has_message_cover = token.TryGetValue("message_cover", out JToken value) && !string.IsNullOrEmpty(value.ToString());
                    if (has_message_cover)
                        message_cover_url = value.ToString();
                    JArray array = JArray.Parse(token.Value<string>("message_raw_output"));
                    message_raw_output = string.Empty;
                    StringBuilder builder = new StringBuilder();
                    foreach (var i in array)
                    {
                        JObject item = i as JObject;
                        if (item.Value<string>("type") == "text")
                            builder.Append(item.Value<string>("message"));
                        else if (item.Value<string>("type") == "image")
                        {
                            string description = string.IsNullOrEmpty(item.Value<string>("description")) ? string.Empty : item.Value<string>("description");
                            string uri = item.Value<string>("url");
                            builder.Append($"\n<a t=\"image\" href=\"{uri}\">{description}</a>\n");
                            feedArticlePics.Add(uri);
                        }
                    }
                    message_raw_output = builder.ToString();
                }
                if (token.Value<string>("feedType") == "answer")
                    isAnswerFeed = true;
                if (isAnswerFeed)
                {
                    JObject j = JObject.Parse(token.Value<string>("extraData"));
                    questionAnswerNum = j.Value<int>("questionAnswerNum").ToString();
                    questionUrl = j.Value<string>("questionUrl");
                }
            }
            showTtitle = token.TryGetValue("ttitle", out JToken valuettitle) && !string.IsNullOrEmpty(valuettitle.ToString());
            if (showTtitle)
            {
                ttitle = valuettitle.ToString();
                turl = token.Value<string>("turl");
                tpicUrl = token.Value<string>("tpic");
            }
            show_dyh_name = token.TryGetValue("dyh_name", out JToken valuedyh) && !string.IsNullOrEmpty(valuedyh.ToString());
            if (show_dyh_name)
            {
                dyh_name = valuedyh.ToString();
                dyhUrl = $"/dyh/{token.Value<int>("dyh_id")}";
            }
            showRelationRows = (token.TryGetValue("location", out JToken valuelocation) && !string.IsNullOrEmpty(valuelocation.ToString()))
                               | (token.TryGetValue("relationRows", out JToken valuerelationRows) && (valuerelationRows as JArray ?? new JArray()).Count > 0);
            if (showRelationRows)
            {
                List<RelationRowsItem> vs = new List<RelationRowsItem>();
                if (valuelocation != null && !string.IsNullOrEmpty(valuelocation.ToString()))
                    vs.Add(new RelationRowsItem { title = valuelocation.ToString() });
                if (valuerelationRows != null)
                    foreach (var i in valuerelationRows as JArray)
                    {
                        JObject item = i as JObject;
                        vs.Add(new RelationRowsItem { title = item.Value<string>("title"), url = item.Value<string>("url"), logoUrl = item.Value<string>("logo") });
                    }
                if (vs.Count == 0) showRelationRows = false;
                relationRows = vs.ToArray();
            }
            GetPic();
        }

        private async void GetPic()
        {
            if (has_message_cover)
                message_cover = await ImageCacheHelper.GetImage(ImageType.SmallImage, message_cover_url);
            if (showTtitle)
                tpic = await ImageCacheHelper.GetImage(ImageType.Icon, tpicUrl);
            if (showRelationRows)
                foreach (var item in relationRows)
                    if (!string.IsNullOrEmpty(item.logoUrl))
                        item.logo = await ImageCacheHelper.GetImage(ImageType.Icon, item.logoUrl);
        }

        string tpicUrl;
        private ImageSource tpic1;
        private ImageSource message_cover1;
        public List<string> feedArticlePics { get; private set; } = new List<string>();
        public new string share_num { get; private set; }
        public bool isFeedArticle { get; private set; }
        public bool isFeedArticle2 { get => !isFeedArticle; }
        public bool has_message_cover { get; private set; }
        public string message_cover_url { get; private set; }
        public ImageSource message_cover
        {
            get => message_cover1;
            private set
            {
                message_cover1 = value;
                Changed(this, nameof(message_cover));
            }
        }
        public string message_raw_output { get; private set; }
        public bool showTtitle { get; private set; }
        public string turl { get; private set; }
        public string ttitle { get; private set; }
        public ImageSource tpic
        {
            get => tpic1;
            private set
            {
                tpic1 = value;
                Changed(this, nameof(tpic));
            }
        }
        public bool show_dyh_name { get; private set; }
        public string dyhUrl { get; private set; }
        public string dyh_name { get; private set; }
        public bool isAnswerFeed { get; private set; }
        public string questionUrl { get; private set; }
        public string title { get; private set; }
        public string questionAnswerNum { get; private set; }
        public bool showRelationRows { get; private set; }
        public RelationRowsItem[] relationRows { get; private set; }
    }
}
