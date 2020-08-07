using CoolapkUWP.Helpers;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoolapkUWP.Models
{
    public class FeedDetailModel : FeedModelBase
    {
        private int questionAnswerNum;

        public FeedDetailModel(JObject o) : base(o)
        {
            Title = o.Value<string>("title");
            if (o.Value<string>("entityType") != "article")
            {
                if (o.Value<string>("feedType") == "feedArticle")
                {
                    IsFeedArticle = true;
                }
                if (IsFeedArticle)
                {
                    HasMessageCover = o.TryGetValue("message_cover", out JToken value) && !string.IsNullOrEmpty(value.ToString());
                    if (HasMessageCover)
                    {
                        MessageCover = new ImageModel(value.ToString(), ImageType.SmallImage);
                    }

                    MessageRawOutput = string.Empty;
                    StringBuilder builder = new StringBuilder();
                    foreach (JObject item in JArray.Parse(o.Value<string>("message_raw_output")))
                    {
                        switch (item.Value<string>("type"))
                        {
                            case "text":
                                builder.Append(item.Value<string>("message"));
                                break;
                            case "image":
                                string description = string.IsNullOrEmpty(item.Value<string>("description")) ? string.Empty : item.Value<string>("description");
                                string uri = item.Value<string>("url");
                                builder.Append($"\n<a t=\"image\" href=\"{uri}\">{description}</a>\n");
                                break;
                        }
                    }
                    MessageRawOutput = builder.ToString();
                }
                IsAnswerFeed = o.Value<string>("feedType") == "answer";
                if (IsAnswerFeed)
                {
                    JObject j = JObject.Parse(o.Value<string>("extraData"));
                    QuestionAnswerNum = j.Value<int>("questionAnswerNum");
                    QuestionUrl = j.Value<string>("questionUrl");
                }
            }
            ShowTtitle = o.TryGetValue("ttitle", out JToken valuettitle) && !string.IsNullOrEmpty(valuettitle.ToString());
            if (ShowTtitle)
            {
                Ttitle = valuettitle.ToString();
                Turl = o.Value<string>("turl");
                Tpic = new ImageModel(o.Value<string>("tpic"), ImageType.Icon);
            }

            ShowDyhName = o.TryGetValue("dyh_name", out JToken valuedyh) && !string.IsNullOrEmpty(valuedyh.ToString());
            if (ShowDyhName)
            {
                DyhName = valuedyh.ToString();
                DyhUrl = $"/dyh/{o.Value<int>("dyh_id")}";
            }
            ShowRelationRows = (o.TryGetValue("location", out JToken valuelocation) && !string.IsNullOrEmpty(valuelocation.ToString()))
                             | (o.TryGetValue("relationRows", out JToken valuerelationRows) && (valuerelationRows as JArray ?? new JArray()).Count > 0);
            if (ShowRelationRows)
            {
                var builder = new List<RelationRowsItem>();
                if (valuelocation != null && !string.IsNullOrEmpty(valuelocation.ToString()))
                {
                    builder.Add(new RelationRowsItem { Title = valuelocation.ToString() });
                }

                if (valuerelationRows != null)
                {
                    foreach (JObject item in valuerelationRows as JArray)
                    {
                        builder.Add(new RelationRowsItem { Title = item.Value<string>("title"), Url = item.Value<string>("url"), Logo = new ImageModel(item.Value<string>("logo"), ImageType.Icon) });
                    }
                }

                if (builder.Count == 0)
                {
                    ShowRelationRows = false;
                }
                RelationRows = builder;
            }

            ShowHotReplies = o.TryGetValue("hotReplyRows", out JToken hotReplyRows) && !string.IsNullOrEmpty(hotReplyRows.ToString());
            if (ShowHotReplies)
            {
                HotReplies = hotReplyRows.Select(item => new FeedReplyModel((JObject)item)).ToList();
            }
        }

        public bool IsFeedArticle { get; private set; }
        public bool IsFeedArticle2 { get => !IsFeedArticle; }
        public bool HasMessageCover { get; private set; }

        public ImageModel MessageCover { get; private set; }
        public ImageModel Tpic { get; private set; }

        public string MessageRawOutput { get; private set; }
        public bool ShowTtitle { get; private set; }
        public string Turl { get; private set; }
        public string Ttitle { get; private set; }

        public bool ShowDyhName { get; private set; }
        public string DyhUrl { get; private set; }
        public string DyhName { get; private set; }
        public bool IsAnswerFeed { get; private set; }
        public string QuestionUrl { get; private set; }
        public string Title { get; private set; }
        public new int QuestionAnswerNum
        {
            get => questionAnswerNum > 0 ? questionAnswerNum : int.Parse(base.QuestionAnswerNum ?? "0");
            private set => questionAnswerNum = value;
        }
        public bool ShowRelationRows { get; private set; }
        internal List<RelationRowsItem> RelationRows { get; private set; }
        public bool ShowHotReplies { get; private set; }
        internal List<FeedReplyModel> HotReplies { get; private set; }
    }
}