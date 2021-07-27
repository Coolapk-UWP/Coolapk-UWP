using CoolapkUWP.Core.Models;
using CoolapkUWP.Helpers;
using CoolapkUWP.Pages.FeedPages;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace CoolapkUWP.Models
{
    public class SourceFeedModel : Entity
    {
        public string Url { get; private set; }
        public string QRUrl { get; private set; }
        public string Shareurl { get; private set; }
        public string Uurl { get; private set; }
        public string Username { get; private set; }
        public string Dateline { get; private set; }
        public string MessageTitle { get; private set; }
        public string Message { get; private set; }
        public bool ShowMessageTitle { get => !string.IsNullOrEmpty(MessageTitle); }
        public string EntityForward { get; private set; }
        public bool ShowPicArr { get; private set; }
        public bool IsCoolPictuers { get; private set; }
        public bool IsMoreThanOnePic { get; private set; }
        public bool HavePic { get; private set; }
        public BackgroundImageModel Pic { get; private set; }
        public ImmutableArray<ImageModel> PicArr { get; private set; } = ImmutableArray<ImageModel>.Empty;
        public bool IsQuestionFeed { get; private set; }
        public bool IsRatingFeed { get; private set; }
        public bool IsBlock { get; private set; }

        public SourceFeedModel(JObject o) : base(o)
        {
            Windows.ApplicationModel.Resources.ResourceLoader loader = Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse("Feed");
            if (o.TryGetValue("entityForward", out JToken entityForward) && !string.IsNullOrEmpty(entityForward.ToString()))
            {
                EntityForward = entityForward.ToString();
            }
            if (o.TryGetValue("url", out JToken uri) && !string.IsNullOrEmpty(uri.ToString()))
            {
                Url = uri.ToString();
            }
            else if (o.TryGetValue("id", out JToken id) && !string.IsNullOrEmpty(id.ToString()))
            {
                Url = $"/feed/{id.ToString().Replace("\"", string.Empty, StringComparison.Ordinal)}";
            }
            if (o.Value<string>("entityType") == "article")
            {
                Dateline = DataHelper.ConvertUnixTimeStampToReadable(o.Value<int>("digest_time"));
                Message = o.Value<string>("message").Substring(0, 120);
                Message = Message.Contains("</a>") ? o.Value<string>("message").Substring(0, 200) + "...<a href=\"" + Url + "\">" + loader.GetString("readmore") + "</a>" : Message + "...<a href=\"" + Url + "\">" + loader.GetString("readmore") + "</a>";
                MessageTitle = o.Value<string>("title");
            }
            else if (o.TryGetValue("collection_item_info", out JToken v1) && v1 != null)
            {
                JObject collection_item_info = (JObject)v1;
                if (collection_item_info.TryGetValue("url", out JToken url) && !string.IsNullOrEmpty(url.ToString()))
                {
                    Url = url.ToString();
                }
                if (collection_item_info.TryGetValue("dateline", out JToken dateline) && !string.IsNullOrEmpty(dateline.ToString()))
                {
                    Dateline = DataHelper.ConvertUnixTimeStampToReadable(double.Parse(dateline.ToString()));
                }
                if (collection_item_info.TryGetValue("content", out JToken content) && !string.IsNullOrEmpty(content.ToString()))
                {
                    Message = content.ToString();
                    Message = UIHelper.CSStoMarkDown(Message);
                }
                if (collection_item_info.TryGetValue("title", out JToken title) && !string.IsNullOrEmpty(title.ToString()))
                {
                    MessageTitle = title.ToString();
                }
                if (o.TryGetValue("userInfo", out JToken v2) && !string.IsNullOrEmpty(v2.ToString()))
                {
                    JObject userInfo = (JObject)v2;
                    if (userInfo.TryGetValue("url", out JToken urI))
                    {
                        Uurl = urI.ToString();
                    }
                    if (userInfo.TryGetValue("username", out JToken username))
                    {
                        Username = username.ToString();
                    }
                }
                else
                {
                    if (o.TryGetValue("uid", out JToken uid))
                    {
                        Uurl = "/u/" + uid.ToString();
                    }
                    if (o.TryGetValue("username", out JToken username))
                    {
                        Username = username.ToString();
                    }
                }
                if (Username == null)
                {
                    if (o.TryGetValue("title", out JToken title1) && !string.IsNullOrEmpty(title1.ToString()))
                    {
                        Username = title1.ToString();
                    }
                }
            }
            else
            {
                IsQuestionFeed = o.Value<string>("feedType") == "question";
                if (IsQuestionFeed)
                {
                    Url = Url.Replace("/feed/", "/question/", StringComparison.Ordinal);
                }
                if (o.TryGetValue("userInfo", out JToken v2) && !string.IsNullOrEmpty(v2.ToString()))
                {
                    JObject userInfo = (JObject)v2;
                    if (userInfo.TryGetValue("url", out JToken url))
                    {
                        Uurl = url.ToString();
                    }
                    if (userInfo.TryGetValue("username", out JToken username))
                    {
                        Username = username.ToString();
                    }
                }
                else
                {
                    if (o.TryGetValue("uid", out JToken uid))
                    {
                        Uurl = "/u/" + uid.ToString();
                    }
                    if (o.TryGetValue("username", out JToken username))
                    {
                        Username = username.ToString();
                    }
                }
                Dateline = DataHelper.ConvertUnixTimeStampToReadable(double.Parse(o["dateline"].ToString().Replace("\"", string.Empty, System.StringComparison.Ordinal)));
                IsRatingFeed = o.Value<string>("feedType") == "rating";
                Message = (IsRatingFeed ? "【评分】" + o.Value<string>("rating_score") + "分\n" + o.Value<string>("message") : o.Value<string>("message")).Replace("<a href=\"\">查看更多</a>", "<a href=\"" + Url + "\">" + loader.GetString("readmore") + "</a>");
                MessageTitle = o.TryGetValue("message_title", out JToken message_title) ? message_title.ToString()
                    : o.TryGetValue("editor_title", out JToken editor_title) ? editor_title.ToString() : string.Empty;
            }
            ShowPicArr = o.TryGetValue("picArr", out JToken picArr) && (picArr as JArray).Count > 0 && !string.IsNullOrEmpty((picArr as JArray)[0].ToString());
            if (o.Value<string>("feedTypeName") == "酷图")
            {
                ShowPicArr = IsCoolPictuers = true;
            }
            if (picArr != null)
            {
                IsMoreThanOnePic = picArr.Count() > 1;
                if (ShowPicArr || IsCoolPictuers)
                {
                    PicArr = (from item in picArr
                              select new ImageModel(item.ToString(), ImageType.Icon)).ToImmutableArray();

                    foreach (ImageModel item in PicArr)
                    {
                        item.ContextArray = PicArr;
                    }
                }
            }
            if (o.TryGetValue("pic", out JToken value1) && !string.IsNullOrEmpty(value1.ToString()))
            {
                HavePic = true;
                Pic = new BackgroundImageModel(value1.ToString(), ImageType.SmallImage);
            }
            QRUrl = "https://www.coolapk.com" + Url != null ? Url.Replace("/question/", "/feed/", StringComparison.Ordinal) : string.Empty;
            Shareurl = string.IsNullOrEmpty(o.Value<string>("shareUrl")) ? QRUrl : o.Value<string>("shareUrl");
            IsBlock = o.TryGetValue("block_status", out JToken v) && v.ToString() != "0";
            if (UIHelper.IsSpecialUser && IsBlock)
            { Username += " [已折叠]"; }
            if (o.TryGetValue("status", out JToken s) && s.ToString() == "-1")
            { Username += " [仅自己可见]"; }
        }
    }
}