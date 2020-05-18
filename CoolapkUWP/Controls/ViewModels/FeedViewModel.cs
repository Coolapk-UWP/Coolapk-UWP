using CoolapkUWP.Helpers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Windows.UI.Xaml.Media;

namespace CoolapkUWP.Controls.ViewModels
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
        public FeedViewModel(JToken t, FeedDisplayMode mode = FeedDisplayMode.normal) : base(t)
        {
            JObject token = t as JObject;
            if (!string.IsNullOrEmpty(message_title) && !mode.HasFlag(FeedDisplayMode.notShowMessageTitle)) showMessage_title = true;
            if (mode.HasFlag(FeedDisplayMode.isFirstPageFeed))
            {
                info = token.Value<string>("infoHtml").Replace("&nbsp;", string.Empty);
                showDateline = false;
                showReplyRows = token.TryGetValue("replyRows", out JToken value) && (value as JArray ?? new JArray()).Count > 0 ? true : false;
                if (showReplyRows)
                {
                    List<ReplyRowsItem> vs = new List<ReplyRowsItem>();
                    foreach (var i in value as JArray)
                        vs.Add(new ReplyRowsItem(i as JObject));
                    replyRows = vs.ToArray();
                }
            }
            else if (mode.HasFlag(FeedDisplayMode.normal))
                if (token.TryGetValue("info", out JToken value1))
                    info = value1.ToString();

            if (token.Value<string>("entityType") != "article")
            {
                if (token.Value<string>("feedType") == "question")
                    showLikes = false;
                uurl = token["userInfo"].Value<string>("url");
            }
            else
            {
                if (!mode.HasFlag(FeedDisplayMode.notShowDyhName))
                {
                    showDyh = true;
                    dyhlogoUrl = token["dyh_info"].Value<string>("logo");
                    dyhurl = token["dyh_info"].Value<string>("url");
                    dyhname = token.Value<string>("dyh_name");
                }
                showFromInfo = (token["dyh_info"] as JObject).TryGetValue("fromInfo", out JToken value);
                if (showFromInfo)
                {
                    fromInfo = value.ToString();
                    uurl = $"/u/{token.Value<int>("from_uid")}";
                }
            }

            showRelationRows = (token.TryGetValue("location", out JToken valuelocation) && !string.IsNullOrEmpty(valuelocation.ToString()))
                               | (token.TryGetValue("ttitle", out JToken valuettitle) && !string.IsNullOrEmpty(valuettitle.ToString()))
                               | (token.TryGetValue("dyh_name", out JToken valuedyh) && !string.IsNullOrEmpty(valuedyh.ToString()))
                               | (token.TryGetValue("relationRows", out JToken valuerelationRows) && (valuerelationRows as JArray ?? new JArray()).Count > 0);
            if (showRelationRows)
            {
                List<RelationRowsItem> vs = new List<RelationRowsItem>();
                if (valuelocation != null && !string.IsNullOrEmpty(valuelocation.ToString()))
                    vs.Add(new RelationRowsItem { title = valuelocation.ToString() });
                if (valuettitle != null && !string.IsNullOrEmpty(valuettitle.ToString()))
                    vs.Add(new RelationRowsItem { title = valuettitle.ToString(), url = token.Value<string>("turl"), logoUrl = token.Value<string>("tpic") });
                if (token.Value<string>("entityType") != "article" && valuedyh != null && !string.IsNullOrEmpty(valuedyh.ToString()))
                    vs.Add(new RelationRowsItem { title = valuedyh.ToString(), url = $"/dyh/{token["dyh_id"].ToString().Replace("\"", string.Empty)}" });
                if (valuerelationRows != null)
                    foreach (var i in valuerelationRows as JArray)
                    {
                        JObject item = i as JObject;
                        vs.Add(new RelationRowsItem { title = item.Value<string>("title"), url = item.Value<string>("url"), logoUrl = item.Value<string>("logo") });
                    }
                relationRows = vs.ToArray();
                if (vs.Count == 0) showRelationRows = false;
            }
            isStickTop = token.TryGetValue("isStickTop", out JToken j) && int.Parse(j.ToString()) == 1;
            GetPic();
        }

        private async void GetPic()
        {
            if (showDyh && !string.IsNullOrEmpty(dyhlogoUrl))
                dyhlogo = await ImageCacheHelper.GetImage(ImageType.Icon, dyhlogoUrl);
            if (showRelationRows)
                foreach (var item in relationRows)
                    if (!string.IsNullOrEmpty(item.logoUrl))
                        item.logo = await ImageCacheHelper.GetImage(ImageType.Icon, item.logoUrl);
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
        public ReplyRowsItem(JObject token)
        {
            string getMessage(JObject jObject)
            {
                if (string.IsNullOrEmpty(jObject.Value<string>("pic")))
                    return $"<a href=\"/u/{jObject.Value<int>("uid")}\" type=\"user-detail\">{jObject.Value<string>("username")}</a>：{jObject.Value<string>("message")}";
                else
                    return $"<a href=\"/u/{jObject.Value<int>("uid")}\" type=\"user-detail\">{jObject.Value<string>("username")}</a>：{jObject.Value<string>("message")} <a href=\"{jObject.Value<string>("pic")}\">查看图片</a>";
            }
            extraFlag = token.Value<string>("extraFlag");
            id = token.Value<int>("id");
            message = getMessage(token);
        }
        public string extraFlag { get; private set; }
        public double id { get; private set; }
        public string message { get; private set; }
    }
}

