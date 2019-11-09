using CoolapkUWP.Data;
using System;
using System.Collections.Generic;
using Windows.Data.Json;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace CoolapkUWP.Control.ViewModels
{
    [Flags]
    public enum FeedDisplayMode
    {
        normal = 0,
        isFirstPageFeed = 0x01,
        notShowDyhName = 0x02
    }

    public interface IEntity
    {
        string entityId { get; }
        bool entityFixed { get; }
        string entityType { get; }
    }

    public class FeedViewModel : SourceFeed
    {
        public FeedViewModel(IJsonValue t, FeedDisplayMode mode) : base(t, mode)
        {
            JsonObject token = t.GetObject();
            if (mode.HasFlag(FeedDisplayMode.isFirstPageFeed))
            {
                info = token["infoHtml"].GetString().Replace("&nbsp;", string.Empty);
                showDateline = false;
                showReplyRows = token.TryGetValue("replyRows", out IJsonValue value) && (value.GetArray() ?? new JsonArray()).Count > 0 ? true : false;
                if (showReplyRows)
                {
                    List<ReplyRowsItem> vs = new List<ReplyRowsItem>();
                    foreach (var i in value.GetArray())
                        vs.Add(new ReplyRowsItem(i.GetObject()));
                    replyRows = vs.ToArray();
                }
            }
            else if (mode.HasFlag(FeedDisplayMode.normal))
                if (token.TryGetValue("info", out IJsonValue value1))
                    info = value1.GetString();

            likenum = token["likenum"].ToString().Replace("\"", string.Empty);
            replynum = token["replynum"].ToString().Replace("\"", string.Empty);
            share_num = token["forwardnum"].ToString().Replace("\"", string.Empty);

            if (token["entityType"].GetString() != "article")
            {
                if (token["feedType"].GetString() == "question") showLikes = false;
                if (token.TryGetValue("share_num", out IJsonValue value))
                    share_num = value.ToString().Replace("\"", string.Empty);
                showUser = true;
                if (!string.IsNullOrEmpty(token["userInfo"].GetObject()["userSmallAvatar"].GetString()))
                    userSmallAvatar = new BitmapImage(new Uri(token["userInfo"].GetObject()["userSmallAvatar"].GetString()));
                device_title = token["device_title"].GetString();

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
                        extra_pic = new BitmapImage(new System.Uri(token["extra_pic"].GetString()));
                }

                showSourceFeedGrid = !string.IsNullOrEmpty(token["source_id"]?.GetString());
                if (showSourceFeedGrid)
                {
                    showSourceFeed = token.TryGetValue("forwardSourceFeed", out IJsonValue jsonValue)
                                     && jsonValue != null
                                     && jsonValue.ToString() != "null";
                    if (showSourceFeed)
                        sourceFeed = new SourceFeed(jsonValue.GetObject(), FeedDisplayMode.normal);
                }
            }
            else
            {
                if (token["dyh_info"].GetObject().TryGetValue("fromInfo", out IJsonValue value))
                {
                    showFromInfo = true;
                    fromInfo = value.GetString();
                    uurl = $"/u/{token["from_uid"].GetNumber()}";
                }
                if (!mode.HasFlag(FeedDisplayMode.notShowDyhName))
                {
                    showDyh = true;
                    if (!string.IsNullOrEmpty(token["dyh_info"].GetObject()["logo"].GetString()))
                        dyhlogo = new BitmapImage(new Uri(token["dyh_info"].GetObject()["logo"].GetString()));
                    dyhurl = token["dyh_info"].GetObject()["url"].GetString();
                    dyhname = token["dyh_name"].GetString();
                }
            }

            showRelationRows = token.TryGetValue("location", out IJsonValue valuelocation)
                               | token.TryGetValue("ttitle", out IJsonValue valuettitle)
                               | token.TryGetValue("dyh_name", out IJsonValue valuedyh)
                               | (token.TryGetValue("relationRows", out IJsonValue valuerelationRows) && (valuerelationRows.GetArray() ?? new JsonArray()).Count > 0 ? true : false);
            if (showRelationRows)
            {
                List<RelationRowsItem> vs = new List<RelationRowsItem>();
                if (valuelocation != null && !string.IsNullOrEmpty(valuelocation.GetString()))
                    vs.Add(new RelationRowsItem { title = valuelocation.GetString() });
                ImageSource l = new BitmapImage();
                if (valuettitle != null && !string.IsNullOrEmpty(valuettitle.GetString()))
                {
                    if (!string.IsNullOrEmpty(token["tpic"].GetString()))
                        l = new BitmapImage(new Uri(token["tpic"].GetString()));
                    vs.Add(new RelationRowsItem { title = valuettitle.GetString(), url = token["turl"].GetString(), logo = l });
                }
                if (token["entityType"].GetString() != "article" && valuedyh != null && !string.IsNullOrEmpty(valuedyh.GetString()))
                    vs.Add(new RelationRowsItem { title = valuedyh.GetString(), url = $"/dyh/{token["dyh_id"].GetNumber()}" });
                if (valuerelationRows != null)
                    foreach (var i in valuerelationRows.GetArray())
                    {
                        JsonObject item = i.GetObject();
                        l = new BitmapImage(new Uri(item["logo"].GetString()));
                        vs.Add(new RelationRowsItem { title = item["title"].GetString(), url = item["url"].GetString(), logo = l });
                    }
                relationRows = vs.ToArray();
                if (vs.Count == 0) showRelationRows = false;
            }

            isStickTop = token.TryGetValue("isStickTop", out IJsonValue j) && j.GetNumber() == 1 ? true : false;
        }
        public bool isStickTop { get; private set; }
        public bool showUser { get; private set; }
        public bool showDyh { get; private set; }
        public bool showUser2 { get => !showUser; }
        public bool showFromInfo { get; private set; }
        public string dyhurl { get; private set; }
        public string dyhname { get; private set; }
        public string fromInfo { get; private set; }
        public ImageSource dyhlogo { get; private set; }
        public bool showDateline { get; private set; } = true;
        public bool showSourceFeed { get; private set; }
        public bool showSourceFeed2 { get => !showSourceFeed; }
        public bool showSourceFeedGrid { get; private set; }
        public bool showExtra_url { get; private set; }
        public bool showRelationRows { get; private set; }
        public bool showReplyRows { get; private set; }
        public bool showLikes { get; private set; } = true;
        public string extra_title { get; private set; }
        public string extra_url { get; private set; }
        public string extra_url2 { get; private set; }
        public ImageSource extra_pic { get; private set; } = new BitmapImage();

        public string fromname { get; private set; }
        public string likenum { get; private set; }
        public string replynum { get; private set; }
        public string share_num { get; private set; }
        public int question_answer_num { get; private set; }
        public int question_follow_num { get; private set; }
        public string device_title { get; private set; }
        public ImageSource userSmallAvatar { get; private set; } = new BitmapImage();
        public string info { get; private set; }
        public SourceFeed sourceFeed { get; private set; }
        public ReplyRowsItem[] replyRows { get; private set; }
        public RelationRowsItem[] relationRows { get; private set; }
    }
    public class RelationRowsItem
    {
        public string url { get; set; }
        public ImageSource logo { get; set; } = new BitmapImage();
        public string title { get; set; }
    }
    public class ReplyRowsItem
    {
        public ReplyRowsItem(JsonObject token)
        {
            string getMessage(JsonObject jObject)
            {
                if (string.IsNullOrEmpty(jObject["pic"].GetString()))
                    return $"[{jObject["username"].GetString()}](/u/{jObject["uid"].GetNumber()})：{Tools.GetMessageText(jObject["message"].GetString())}";
                else
                    return $"[{jObject["username"].GetString()}](/u/{jObject["uid"].GetNumber()})：{Tools.GetMessageText(jObject["message"].GetString())}\n[查看图片]({jObject["pic"].GetString()})";
            }
            extraFlag = token["extraFlag"].GetString();
            id = token["id"].GetNumber();
            message = getMessage(token);
        }
        public string extraFlag { get; private set; }
        public double id { get; private set; }
        public string message { get; private set; }
    }
    public interface IFeed
    {
        string url { get; }
        string message { get; }
    }

    public class SourceFeed : IEntity, IFeed
    {
        public SourceFeed(IJsonValue t, FeedDisplayMode mode)
        {
            JsonObject token = t.GetObject();
            url = token["url"].GetString();
            if (token.TryGetValue("entityId", out IJsonValue value1)) entityId = value1.ToString().Replace("\"", string.Empty);
            entityType = token["entityType"].GetString();
            if (token.TryGetValue("entityFixed", out IJsonValue value) && value.GetNumber() == 1) entityFixed = true;
            if (token["entityType"].GetString() != "article")
            {
                uurl = token["userInfo"].GetObject()["url"].GetString();
                username = token["username"].GetString();
                string s = token["dateline"].ToString().Replace("\"", string.Empty);
                dateline = Tools.ConvertTime(double.Parse(s));
                message = Tools.GetMessageText(token["message"].GetString());
                message_title = token["message_title"].GetString();
            }
            else
            {
                dateline = Tools.ConvertTime(token["digest_time"].GetNumber());
                message = Tools.GetMessageText(token["message"].GetString().Substring(0, 120) + "……<a href=\"\">查看更多</a>");
                message_title = token["title"].GetString();
            }
            showMessage_title = !string.IsNullOrEmpty(message_title);

            showPicArr = token["picArr"].GetArray().Count > 0 && !string.IsNullOrEmpty(token["picArr"].GetArray()[0].GetString()) ? true : false;
            if (showPicArr)
            {
                List<string> vs = new List<string>();
                foreach (var item in token["picArr"].GetArray())
                    vs.Add(item.GetString() + ".s.jpg");
                picArr = vs.ToArray();
            }
        }
        public string url { get; private set; }
        public string uurl { get; set; }
        public string username { get; private set; }
        public string dateline { get; private set; }
        public bool showMessage_title { get; private set; }
        public string message_title { get; private set; }
        public string message { get; private set; }
        public bool showPicArr { get; private set; }
        public string[] picArr { get; private set; }
        public string entityId { get; private set; }
        public bool entityFixed { get; private set; }
        public string entityType { get; private set; }
    }
}

