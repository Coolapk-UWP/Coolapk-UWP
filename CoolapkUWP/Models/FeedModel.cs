using CoolapkUWP.Helpers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Immutable;

namespace CoolapkUWP.Models
{
    [Flags]
    internal enum FeedDisplayMode
    {
        normal = 0,
        isFirstPageFeed = 0x01,
        notShowDyhName = 0x02,
        notShowMessageTitle = 0x04
    }

    internal class FeedModel : FeedModelBase
    {

        public FeedModel(JObject token, FeedDisplayMode mode = FeedDisplayMode.normal) : base(token)
        {
            if (!string.IsNullOrEmpty(Message_title) && !mode.HasFlag(FeedDisplayMode.notShowMessageTitle)) ShowMessage_title = true;
            if (mode.HasFlag(FeedDisplayMode.isFirstPageFeed))
            {
                Info = token.Value<string>("infoHtml").Replace("&nbsp;", string.Empty);
                ShowDateline = false;
                ShowReplyRows = token.TryGetValue("replyRows", out JToken value) && (value as JArray ?? new JArray()).Count > 0 ? true : false;
                if (ShowReplyRows)
                {
                    var buider = ImmutableArray.CreateBuilder<ReplyRowsItem>();
                    foreach (var i in value as JArray)
                    {
                        buider.Add(new ReplyRowsItem(i as JObject));
                    }

                    ReplyRows = buider.ToImmutable();
                }
            }
            else if (mode.HasFlag(FeedDisplayMode.normal))
                if (token.TryGetValue("info", out JToken value1))
                    Info = value1.ToString();

            if (token.Value<string>("entityType") != "article")
            {
                if (token.Value<string>("feedType") == "question")
                    ShowLikes = false;
                Uurl = token["userInfo"].Value<string>("url");
            }
            else
            {
                if (!mode.HasFlag(FeedDisplayMode.notShowDyhName))
                {
                    ShowDyh = true;
                    var dyhlogoUrl = token["dyh_info"].Value<string>("logo");
                    if (!string.IsNullOrEmpty(dyhlogoUrl))
                        Dyhlogo = new ImageModel(dyhlogoUrl, ImageType.Icon);
                    Dyhurl = token["dyh_info"].Value<string>("url");
                    Dyhname = token.Value<string>("dyh_name");
                }
                ShowFromInfo = (token["dyh_info"] as JObject).TryGetValue("fromInfo", out JToken value);
                if (ShowFromInfo)
                {
                    FromInfo = value.ToString();
                    Uurl = $"/u/{token.Value<int>("from_uid")}";
                }
            }

            ShowRelationRows = (token.TryGetValue("location", out JToken valuelocation) && !string.IsNullOrEmpty(valuelocation.ToString()))
                               | (token.TryGetValue("ttitle", out JToken valuettitle) && !string.IsNullOrEmpty(valuettitle.ToString()))
                               | (token.TryGetValue("dyh_name", out JToken valuedyh) && !string.IsNullOrEmpty(valuedyh.ToString()))
                               | (token.TryGetValue("relationRows", out JToken valuerelationRows) && (valuerelationRows as JArray ?? new JArray()).Count > 0);
            if (ShowRelationRows)
            {
                var buider = ImmutableArray.CreateBuilder<RelationRowsItem>();
                if (!(valuelocation == null || string.IsNullOrEmpty(valuelocation.ToString())))
                {
                    buider.Add(new RelationRowsItem { Title = valuelocation.ToString() });
                }

                if (!(valuettitle == null || string.IsNullOrEmpty(valuettitle.ToString())))
                {
                    buider.Add(new RelationRowsItem { Title = valuettitle.ToString(), Url = token.Value<string>("turl"), Logo = new ImageModel(token.Value<string>("tpic"), ImageType.Icon) });
                }

                if (!(token.Value<string>("entityType") == "article" || valuedyh == null || string.IsNullOrEmpty(valuedyh.ToString())))
                {
                    buider.Add(new RelationRowsItem { Title = valuedyh.ToString(), Url = $"/dyh/{token["dyh_id"].ToString().Replace("\"", string.Empty)}" });
                }

                if (valuerelationRows != null)
                {
                    foreach (var i in valuerelationRows as JArray)
                    {
                        JObject item = i as JObject;
                        buider.Add(new RelationRowsItem { Title = item.Value<string>("title"), Url = item.Value<string>("url"), Logo = new ImageModel(item.Value<string>("logo"), ImageType.Icon) });
                    }
                }

                RelationRows = buider.ToImmutable();
                if (buider.Count == 0) { ShowRelationRows = false; }
            }
            IsStickTop = token.TryGetValue("isStickTop", out JToken j) && int.Parse(j.ToString()) == 1;
        }

        public new string Uurl { get; private set; }
        public new string Info { get; private set; }
        public bool IsStickTop { get; private set; }
        public bool ShowDyh { get; private set; }
        public bool ShowFromInfo { get; private set; }
        public string Dyhurl { get; private set; }
        public string Dyhname { get; private set; }
        public string FromInfo { get; private set; }
        public ImageModel Dyhlogo { get; private set; }
        public bool ShowDateline { get; private set; } = true;
        public bool ShowRelationRows { get; private set; }
        public bool ShowReplyRows { get; private set; }
        public bool ShowLikes { get; private set; } = true;
        public ImmutableArray<ReplyRowsItem> ReplyRows { get; private set; } = ImmutableArray<ReplyRowsItem>.Empty;
        public ImmutableArray<RelationRowsItem> RelationRows { get; private set; } = ImmutableArray<RelationRowsItem>.Empty;
        public new bool ShowMessage_title { get; private set; }
    }

    public class RelationRowsItem
    {
        public string Url { get; set; }
        public ImageModel Logo { get; set; }
        public string Title { get; set; }
    }

    public class ReplyRowsItem
    {
        public ReplyRowsItem(JObject token)
        {
            ExtraFlag = token.Value<string>("extraFlag");
            Id = token.Value<int>("id");
            if (string.IsNullOrEmpty(token.Value<string>("pic")))
                Message = $"<a href=\"/u/{token.Value<int>("uid")}\" type=\"user-detail\">{token.Value<string>("username")}</a>：{token.Value<string>("message")}";
            else
                Message = $"<a href=\"/u/{token.Value<int>("uid")}\" type=\"user-detail\">{token.Value<string>("username")}</a>：{token.Value<string>("message")} <a href=\"{token.Value<string>("pic")}\">查看图片</a>";
        }

        public string ExtraFlag { get; private set; }
        public double Id { get; private set; }
        public string Message { get; private set; }
    }
}