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
            if (!string.IsNullOrEmpty(Message_title) && !mode.HasFlag(FeedDisplayMode.notShowMessageTitle)) ShowMessage_title = true;
            if (mode.HasFlag(FeedDisplayMode.isFirstPageFeed))
            {
                Info = token.Value<string>("infoHtml").Replace("&nbsp;", string.Empty);
                ShowDateline = false;
                ShowReplyRows = token.TryGetValue("replyRows", out JToken value) && (value as JArray ?? new JArray()).Count > 0 ? true : false;
                if (ShowReplyRows)
                {
                    List<ReplyRowsItem> vs = new List<ReplyRowsItem>();
                    foreach (var i in value as JArray)
                        vs.Add(new ReplyRowsItem(i as JObject));
                    ReplyRows = vs.ToArray();
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
                    dyhlogoUrl = token["dyh_info"].Value<string>("logo");
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
                List<RelationRowsItem> vs = new List<RelationRowsItem>();
                if (valuelocation != null && !string.IsNullOrEmpty(valuelocation.ToString()))
                    vs.Add(new RelationRowsItem { Title = valuelocation.ToString() });
                if (valuettitle != null && !string.IsNullOrEmpty(valuettitle.ToString()))
                    vs.Add(new RelationRowsItem { Title = valuettitle.ToString(), Url = token.Value<string>("turl"), logoUrl = token.Value<string>("tpic") });
                if (token.Value<string>("entityType") != "article" && valuedyh != null && !string.IsNullOrEmpty(valuedyh.ToString()))
                    vs.Add(new RelationRowsItem { Title = valuedyh.ToString(), Url = $"/dyh/{token["dyh_id"].ToString().Replace("\"", string.Empty)}" });
                if (valuerelationRows != null)
                    foreach (var i in valuerelationRows as JArray)
                    {
                        JObject item = i as JObject;
                        vs.Add(new RelationRowsItem { Title = item.Value<string>("title"), Url = item.Value<string>("url"), logoUrl = item.Value<string>("logo") });
                    }
                RelationRows = vs.ToArray();
                if (vs.Count == 0) ShowRelationRows = false;
            }
            IsStickTop = token.TryGetValue("isStickTop", out JToken j) && int.Parse(j.ToString()) == 1;
            GetPic();
        }

        private async void GetPic()
        {
            if (ShowDyh && !string.IsNullOrEmpty(dyhlogoUrl))
                Dyhlogo = await ImageCacheHelper.GetImage(ImageType.Icon, dyhlogoUrl);
            if (ShowRelationRows)
                foreach (var item in RelationRows)
                    if (!string.IsNullOrEmpty(item.logoUrl))
                        item.Logo = await ImageCacheHelper.GetImage(ImageType.Icon, item.logoUrl);
        }

        private readonly string dyhlogoUrl;
        private ImageSource dyhlogo1;

        public new string Uurl { get; private set; }
        public new string Info { get; private set; }
        public bool IsStickTop { get; private set; }
        public bool ShowDyh { get; private set; }
        public bool ShowFromInfo { get; private set; }
        public string Dyhurl { get; private set; }
        public string Dyhname { get; private set; }
        public string FromInfo { get; private set; }
        public ImageSource Dyhlogo
        {
            get => dyhlogo1;
            private set
            {
                dyhlogo1 = value;
                Changed(this, nameof(Dyhlogo));
            }
        }
        public bool ShowDateline { get; private set; } = true;
        public bool ShowRelationRows { get; private set; }
        public bool ShowReplyRows { get; private set; }
        public bool ShowLikes { get; private set; } = true;
        public ReplyRowsItem[] ReplyRows { get; private set; }
        public RelationRowsItem[] RelationRows { get; private set; }
        public new bool ShowMessage_title { get; private set; }
    }
    class RelationRowsItem : INotifyPropertyChanged
    {
        public string Url { get; set; }
        public string logoUrl;
        private ImageSource logo1;

        public event PropertyChangedEventHandler PropertyChanged;

        public ImageSource Logo
        {
            get => logo1;
            set
            {
                logo1 = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Logo)));
            }
        }
        public string Title { get; set; }
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
            ExtraFlag = token.Value<string>("extraFlag");
            Id = token.Value<int>("id");
            Message = getMessage(token);
        }
        public string ExtraFlag { get; private set; }
        public double Id { get; private set; }
        public string Message { get; private set; }
    }
}

