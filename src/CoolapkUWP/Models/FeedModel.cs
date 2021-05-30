using CoolapkUWP.Helpers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

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
        public new bool ShowMessageTitle { get; private set; }
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
        public bool HaveUserInfo { get; private set; }
        public List<ReplyRowsItem> ReplyRows { get; private set; }
        public List<RelationRowsItem> RelationRows { get; private set; }

        public FeedModel(JObject token, FeedDisplayMode mode = FeedDisplayMode.normal) : base(token)
        {
            ShowMessageTitle = !string.IsNullOrEmpty(MessageTitle) && !mode.HasFlag(FeedDisplayMode.notShowMessageTitle);
            if (mode.HasFlag(FeedDisplayMode.isFirstPageFeed))
            {
                Regex r = new Regex("<a.*?>", RegexOptions.IgnoreCase);
                Regex r1 = new Regex("<a.*?/>", RegexOptions.IgnoreCase);
                Regex r2 = new Regex("</a.*?>", RegexOptions.IgnoreCase);
                Info = token.Value<string>("infoHtml").Replace("&nbsp;", string.Empty, StringComparison.Ordinal);
                Info = r.Replace(Info, "");
                Info = r1.Replace(Info, "");
                Info = r2.Replace(Info, "");
                ShowDateline = false;
                ShowReplyRows = token.TryGetValue("replyRows", out JToken value) && ((value as JArray)?.Count ?? 0) > 0;
                if (ShowReplyRows)
                {
                    ReplyRows = value.Select(item => new ReplyRowsItem((JObject)item)).ToList();
                }
            }
            else if (mode.HasFlag(FeedDisplayMode.normal) && token.TryGetValue("info", out JToken value1))
            {
                Info = value1.ToString();
            }

            if (token.Value<string>("entityType") == "article")
            {
                ShowDyh = !mode.HasFlag(FeedDisplayMode.notShowDyhName);
                if (ShowDyh)
                {
                    Dyhurl = token["dyh_info"].Value<string>("url");
                    Dyhname = token.Value<string>("dyh_name");
                    string dyhlogoUrl = token["dyh_info"].Value<string>("logo");
                    if (!string.IsNullOrEmpty(dyhlogoUrl))
                    {
                        Dyhlogo = new ImageModel(dyhlogoUrl, ImageType.Icon);
                    }
                }
                ShowFromInfo = (token["dyh_info"] as JObject).TryGetValue("fromInfo", out JToken value);
                if (ShowFromInfo)
                {
                    FromInfo = value.ToString();
                    Uurl = $"/u/{token.Value<int>("from_uid")}";
                }
            }
            else
            {
                ShowLikes = token.Value<string>("feedType") != "question";
                try
                {
                    HaveUserInfo = !string.IsNullOrEmpty((string)token["userInfo"]);
                }
                catch
                {
                    HaveUserInfo = false;
                }
                Uurl = HaveUserInfo ? token["userInfo"].Value<string>("url") : "/u/" + token.Value<string>("uid");
            }

            ShowRelationRows = (token.TryGetValue("location", out JToken vLocation) && !string.IsNullOrEmpty(vLocation.ToString())) |
                               (token.TryGetValue("ttitle", out JToken vTtitle) && !string.IsNullOrEmpty(vTtitle.ToString())) |
                               (token.TryGetValue("dyh_name", out JToken vDyh) && !string.IsNullOrEmpty(vDyh.ToString())) |
                               (token.TryGetValue("relationRows", out JToken vRelationRows) && ((vRelationRows as JArray)?.Count ?? 0) > 0);
            if (ShowRelationRows)
            {
                List<RelationRowsItem> buider = new List<RelationRowsItem>();
                if (vLocation != null && !string.IsNullOrEmpty(vLocation.ToString()))
                {
                    buider.Add(new RelationRowsItem { Title = vLocation.ToString() });
                }

                if (vTtitle != null && !string.IsNullOrEmpty(vTtitle.ToString()))
                {
                    buider.Add(new RelationRowsItem { Title = vTtitle.ToString(), Url = token.Value<string>("turl"), Logo = new ImageModel(token.Value<string>("tpic"), ImageType.Icon) });
                }

                if (token.Value<string>("entityType") != "article" && vDyh != null && !string.IsNullOrEmpty(vDyh.ToString()))
                {
                    buider.Add(new RelationRowsItem { Title = vDyh.ToString(), Url = $"/dyh/{token["dyh_id"].ToString().Replace("\"", string.Empty)}" });
                }

                if (vRelationRows != null)
                {
                    foreach (JToken i in vRelationRows as JArray)
                    {
                        JObject item = i as JObject;
                        buider.Add(new RelationRowsItem
                        {
                            Title = item.Value<string>("title"),
                            Url = item.Value<string>("url"),
                            Logo = new ImageModel(item.Value<string>("logo"), ImageType.Icon)
                        });
                    }
                }

                ShowRelationRows = buider.Count != 0;
                RelationRows = buider;
            }
            IsStickTop = token.TryGetValue("isStickTop", out JToken j) && int.Parse(j.ToString()) == 1;
        }
    }

    public class RelationRowsItem
    {
        public string Url { get; set; }
        public ImageModel Logo { get; set; }
        public string Title { get; set; }
    }

    public class ReplyRowsItem
    {
        public string ExtraFlag { get; private set; }
        public double Id { get; private set; }
        public string Message { get; private set; }

        public ReplyRowsItem(JObject token)
        {
            ExtraFlag = token.Value<string>("extraFlag");
            Id = token.Value<int>("id");
            Message = string.IsNullOrEmpty(token.Value<string>("pic"))
                ? $"<a href=\"/u/{token.Value<int>("uid")}\" type=\"user-detail\">{token.Value<string>("username")}</a>：{token.Value<string>("message")}"
                : $"<a href=\"/u/{token.Value<int>("uid")}\" type=\"user-detail\">{token.Value<string>("username")}</a>：{token.Value<string>("message")} <a href=\"{token.Value<string>("pic")}\">查看图片</a>";
        }
    }
}