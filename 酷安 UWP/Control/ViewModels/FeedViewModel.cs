using CoolapkUWP.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Windows.Data.Json;
using Windows.UI.Xaml.Media;

namespace CoolapkUWP.Control.ViewModels
{
    [Flags]
    enum FeedDisplayMode
    {
        normal = 0,
        isFirstPageFeed = 0x01,
        notShowDyhName = 0x02,
        notShowMessageTitle = 0x04
    }
    class FeedViewModel : FeedViewModelBase
    {
        public FeedViewModel(IJsonValue t, FeedDisplayMode mode = FeedDisplayMode.normal) : base(t)
        {
            JsonObject token = t.GetObject();
            if (!string.IsNullOrEmpty(message_title) && !mode.HasFlag(FeedDisplayMode.notShowMessageTitle)) showMessage_title = true;
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

            if (token["entityType"].GetString() != "article")
            {
                if (token["feedType"].GetString() == "question")
                    showLikes = false;
                uurl = token["userInfo"].GetObject()["url"].GetString();
            }
            else
            {
                if (!mode.HasFlag(FeedDisplayMode.notShowDyhName))
                {
                    showDyh = true;
                    dyhlogoUrl = token["dyh_info"].GetObject()["logo"].GetString();
                    dyhurl = token["dyh_info"].GetObject()["url"].GetString();
                    dyhname = token["dyh_name"].GetString();
                }
                showFromInfo = token["dyh_info"].GetObject().TryGetValue("fromInfo", out IJsonValue value);
                if (showFromInfo)
                {
                    fromInfo = value.GetString();
                    uurl = $"/u/{token["from_uid"].GetNumber()}";
                }
            }

            showRelationRows = (token.TryGetValue("location", out IJsonValue valuelocation) && !string.IsNullOrEmpty(valuelocation.GetString()))
                               | (token.TryGetValue("ttitle", out IJsonValue valuettitle) && !string.IsNullOrEmpty(valuettitle.GetString()))
                               | (token.TryGetValue("dyh_name", out IJsonValue valuedyh) && !string.IsNullOrEmpty(valuedyh.GetString()))
                               | (token.TryGetValue("relationRows", out IJsonValue valuerelationRows) && (valuerelationRows.GetArray() ?? new JsonArray()).Count > 0);
            if (showRelationRows)
            {
                List<RelationRowsItem> vs = new List<RelationRowsItem>();
                if (valuelocation != null && !string.IsNullOrEmpty(valuelocation.GetString()))
                    vs.Add(new RelationRowsItem { title = valuelocation.GetString() });
                if (valuettitle != null && !string.IsNullOrEmpty(valuettitle.GetString()))
                    vs.Add(new RelationRowsItem { title = valuettitle.GetString(), url = token["turl"].GetString(), logoUrl = token["tpic"].GetString() });
                if (token["entityType"].GetString() != "article" && valuedyh != null && !string.IsNullOrEmpty(valuedyh.GetString()))
                    vs.Add(new RelationRowsItem { title = valuedyh.GetString(), url = $"/dyh/{token["dyh_id"].ToString().Replace("\"", string.Empty)}" });
                if (valuerelationRows != null)
                    foreach (var i in valuerelationRows.GetArray())
                    {
                        JsonObject item = i.GetObject();
                        vs.Add(new RelationRowsItem { title = item["title"].GetString(), url = item["url"].GetString(), logoUrl = item["logo"].GetString() });
                    }
                relationRows = vs.ToArray();
                if (vs.Count == 0) showRelationRows = false;
            }
            isStickTop = token.TryGetValue("isStickTop", out IJsonValue j) && j.GetNumber() == 1;
            GetPic();
        }

        private async void GetPic()
        {
            if (showDyh && !string.IsNullOrEmpty(dyhlogoUrl))
                dyhlogo = await ImageCache.GetImage(ImageType.Icon, dyhlogoUrl);
            if (showRelationRows)
                foreach (var item in relationRows)
                    if (!string.IsNullOrEmpty(item.logoUrl))
                        item.logo = await ImageCache.GetImage(ImageType.Icon, item.logoUrl);
        }

        string dyhlogoUrl;
        private ImageSource dyhlogo1;

        public new string uurl { get; private set; }
        public new string info { get; private set; }
        public bool isStickTop { get; private set; }
        public bool showDyh { get; private set; }
        public bool showFromInfo { get; private set; }
        public string dyhurl { get; private set; }
        public string dyhname { get; private set; }
        public string fromInfo { get; private set; }
        public ImageSource dyhlogo
        {
            get => dyhlogo1;
            private set
            {
                dyhlogo1 = value;
                Changed(this, nameof(dyhlogo));
            }
        }
        public bool showDateline { get; private set; } = true;
        public bool showRelationRows { get; private set; }
        public bool showReplyRows { get; private set; }
        public bool showLikes { get; private set; } = true;
        public ReplyRowsItem[] replyRows { get; private set; }
        public RelationRowsItem[] relationRows { get; private set; }
        public new bool showMessage_title { get; private set; }
    }
    class RelationRowsItem : INotifyPropertyChanged
    {
        public string url { get; set; }
        public string logoUrl;
        private ImageSource logo1;

        public event PropertyChangedEventHandler PropertyChanged;

        public ImageSource logo
        {
            get => logo1;
            set
            {
                logo1 = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(logo)));
            }
        }
        public string title { get; set; }
    }
    class ReplyRowsItem
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
}

