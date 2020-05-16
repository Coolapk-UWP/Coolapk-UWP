using CoolapkUWP.Helpers;
using System.Collections.Generic;
using System.Text;
using Windows.Data.Json;
using Windows.UI.Xaml.Media;

namespace CoolapkUWP.Controls.ViewModels
{
    class FeedDetailViewModel : FeedViewModelBase
    {
        public FeedDetailViewModel(IJsonValue t) : base(t)
        {
            JsonObject token = t.GetObject();
            title = token["title"].GetString();
            if (token["entityType"].GetString() != "article")
            {
                if (token.TryGetValue("share_num", out IJsonValue s))
                    share_num = s.ToString().Replace("\"", string.Empty);
                if (token["feedType"].GetString() == "feedArticle")
                    isFeedArticle = true;
                if (isFeedArticle)
                {
                    has_message_cover = token.TryGetValue("message_cover", out IJsonValue value) && !string.IsNullOrEmpty(value.GetString());
                    if (has_message_cover)
                        message_cover_url = value.GetString();
                    JsonArray array = JsonArray.Parse(token["message_raw_output"].GetString());
                    message_raw_output = string.Empty;
                    StringBuilder builder = new StringBuilder();
                    foreach (var i in array)
                    {
                        JsonObject item = i.GetObject();
                        if (item["type"].GetString() == "text")
                            builder.Append(item["message"].GetString());
                        else if (item["type"].GetString() == "image")
                        {
                            string description = string.IsNullOrEmpty(item["description"].GetString()) ? string.Empty : item["description"].GetString();
                            string uri = item["url"].GetString();
                            builder.Append($"\n<a t=\"image\" href=\"{uri}\">{description}</a>\n");
                            feedArticlePics.Add(uri);
                        }
                    }
                    message_raw_output = builder.ToString();
                }
                if (token["feedType"].GetString() == "answer")
                    isAnswerFeed = true;
                if (isAnswerFeed)
                {
                    JsonObject j = JsonObject.Parse(token["extraData"].GetString());
                    questionAnswerNum = j["questionAnswerNum"].GetNumber().ToString();
                    questionUrl = j["questionUrl"].GetString();
                }
            }
            showTtitle = token.TryGetValue("ttitle", out IJsonValue valuettitle) && !string.IsNullOrEmpty(valuettitle.GetString());
            if (showTtitle)
            {
                ttitle = valuettitle.GetString();
                turl = token["turl"].GetString();
                tpicUrl = token["tpic"].GetString();
            }
            show_dyh_name = token.TryGetValue("dyh_name", out IJsonValue valuedyh) && !string.IsNullOrEmpty(valuedyh.GetString());
            if (show_dyh_name)
            {
                dyh_name = valuedyh.GetString();
                dyhUrl = $"/dyh/{token["dyh_id"].GetNumber()}";
            }
            showRelationRows = (token.TryGetValue("location", out IJsonValue valuelocation) && !string.IsNullOrEmpty(valuelocation.GetString()))
                               | (token.TryGetValue("relationRows", out IJsonValue valuerelationRows) && (valuerelationRows.GetArray() ?? new JsonArray()).Count > 0);
            if (showRelationRows)
            {
                List<RelationRowsItem> vs = new List<RelationRowsItem>();
                if (valuelocation != null && !string.IsNullOrEmpty(valuelocation.GetString()))
                    vs.Add(new RelationRowsItem { title = valuelocation.GetString() });
                if (valuerelationRows != null)
                    foreach (var i in valuerelationRows.GetArray())
                    {
                        JsonObject item = i.GetObject();
                        vs.Add(new RelationRowsItem { title = item["title"].GetString(), url = item["url"].GetString(), logoUrl = item["logo"].GetString() });
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
