using CoolapkUWP.Helpers;
using Newtonsoft.Json.Linq;
using System.Collections.Immutable;
using System.Text;

namespace CoolapkUWP.Models
{
    public class FeedDetailModel : FeedModelBase
    {
        public FeedDetailModel(JObject token) : base(token)
        {
            Title = token.Value<string>("title");
            if (token.Value<string>("entityType") != "article")
            {
                if (token.TryGetValue("share_num", out JToken s))
                    Share_num = s.ToString().Replace("\"", string.Empty);
                if (token.Value<string>("feedType") == "feedArticle")
                    IsFeedArticle = true;
                if (IsFeedArticle)
                {
                    Has_message_cover = token.TryGetValue("message_cover", out JToken value) && !string.IsNullOrEmpty(value.ToString());
                    if (Has_message_cover)
                    {
                        Message_cover = new ImageModel(value.ToString(), ImageType.SmallImage);
                    }

                    Message_raw_output = string.Empty;
                    StringBuilder builder = new StringBuilder();
                    foreach (JObject item in JArray.Parse(token.Value<string>("message_raw_output")))
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
                    Message_raw_output = builder.ToString();
                }
                IsAnswerFeed = token.Value<string>("feedType") == "answer";
                if (IsAnswerFeed)
                {
                    JObject j = JObject.Parse(token.Value<string>("extraData"));
                    QuestionAnswerNum = j.Value<int>("questionAnswerNum").ToString();
                    QuestionUrl = j.Value<string>("questionUrl");
                }
            }
            ShowTtitle = token.TryGetValue("ttitle", out JToken valuettitle) && !string.IsNullOrEmpty(valuettitle.ToString());
            if (ShowTtitle)
            {
                Ttitle = valuettitle.ToString();
                Turl = token.Value<string>("turl");
                Tpic = new ImageModel(token.Value<string>("tpic"), ImageType.Icon);
            }

            Show_dyh_name = token.TryGetValue("dyh_name", out JToken valuedyh) && !string.IsNullOrEmpty(valuedyh.ToString());
            if (Show_dyh_name)
            {
                Dyh_name = valuedyh.ToString();
                DyhUrl = $"/dyh/{token.Value<int>("dyh_id")}";
            }
            ShowRelationRows = (token.TryGetValue("location", out JToken valuelocation) && !string.IsNullOrEmpty(valuelocation.ToString()))
                               | (token.TryGetValue("relationRows", out JToken valuerelationRows) && (valuerelationRows as JArray ?? new JArray()).Count > 0);
            if (ShowRelationRows)
            {
                var builder = ImmutableArray.CreateBuilder<RelationRowsItem>();
                if (valuelocation != null && !string.IsNullOrEmpty(valuelocation.ToString()))
                    builder.Add(new RelationRowsItem { Title = valuelocation.ToString() });
                if (valuerelationRows != null)
                    foreach (var i in valuerelationRows as JArray)
                    {
                        JObject item = i as JObject;
                        builder.Add(new RelationRowsItem { Title = item.Value<string>("title"), Url = item.Value<string>("url"), Logo = new ImageModel(item.Value<string>("logo"), ImageType.Icon) });
                    }
                if (builder.Count == 0) ShowRelationRows = false;
                RelationRows = builder.ToImmutable();
            }
        }

        public new string Share_num { get; private set; }
        public bool IsFeedArticle { get; private set; }
        public bool IsFeedArticle2 { get => !IsFeedArticle; }
        public bool Has_message_cover { get; private set; }

        public ImageModel Message_cover { get; private set; }
        public ImageModel Tpic { get; private set; }

        public string Message_raw_output { get; private set; }
        public bool ShowTtitle { get; private set; }
        public string Turl { get; private set; }
        public string Ttitle { get; private set; }

        public bool Show_dyh_name { get; private set; }
        public string DyhUrl { get; private set; }
        public string Dyh_name { get; private set; }
        public bool IsAnswerFeed { get; private set; }
        public string QuestionUrl { get; private set; }
        public string Title { get; private set; }
        public string QuestionAnswerNum { get; private set; }
        public bool ShowRelationRows { get; private set; }
        public ImmutableArray<RelationRowsItem> RelationRows { get; private set; } = ImmutableArray<RelationRowsItem>.Empty;
    }
}