using CoolapkUWP.Data;
using System.Collections.Generic;
using System.Text;
using Windows.Data.Json;
using Windows.UI.Xaml.Media;

namespace CoolapkUWP.Control.ViewModels
{
    internal class FeedDetailViewModel : FeedViewModelBase
    {
        public bool ShowTtitle { get; private set; }
        public bool ShowDyhName { get; private set; }
        public bool IsAnswerFeed { get; private set; }
        public bool IsFeedArticle { get; private set; }
        public bool HasMessageCover { get; private set; }
        public bool ShowRelationRows { get; private set; }
        public bool NotFeedArticle { get => !IsFeedArticle; }

        private ImageSource tpic;
        private ImageSource dyhpic;
        private ImageSource messagecover;

        private readonly string tpicUrl;
        public string Turl { get; private set; }
        public string Title { get; private set; }
        public string Ttitle { get; private set; }
        public string DyhUrl { get; private set; }
        public string DyhName { get; private set; }
        public string ShareNum { get; private set; }
        public string DyhSubtitle { get; private set; }
        public string QuestionUrl { get; private set; }
        public string MessageCoverUrl { get; private set; }
        public string MessageRawOutput { get; private set; }
        public string QuestionAnswerNum { get; private set; }

        public RelationRowsItem[] RelationRows { get; private set; }
        public List<string> FeedArticlePics { get; private set; } = new List<string>();

        public ImageSource MessageCover
        {
            get => messagecover;
            private set
            {
                messagecover = value;
                Changed(this, nameof(MessageCover));
            }
        }

        public ImageSource Tpic
        {
            get => tpic;
            private set
            {
                tpic = value;
                Changed(this, nameof(Tpic));
            }
        }

        public ImageSource DyhPic
        {
            get => dyhpic;
            private set
            {
                dyhpic = value;
                Changed(this, nameof(DyhPic));
            }
        }

        public FeedDetailViewModel(IJsonValue t) : base(t)
        {
            JsonObject token = t.GetObject();

            if (token.TryGetValue("title", out IJsonValue title))
            {
                Title = title.GetString();
            }

            if (token.TryGetValue("entityType", out IJsonValue entityType) && entityType.GetString() != "article")
            {
                if (token.TryGetValue("share_num", out IJsonValue share_num))
                {
                    ShareNum = share_num.GetNumber().ToString();
                }
                if (token.TryGetValue("feedType", out IJsonValue feedType) && feedType.GetString() == "feedArticle")
                {
                    IsFeedArticle = true;
                    if (token.TryGetValue("message_cover", out IJsonValue message_cover) && !string.IsNullOrEmpty(message_cover.GetString()))
                    {
                        HasMessageCover = true;
                        MessageCoverUrl = message_cover.GetString();
                        GetCover(MessageCoverUrl);
                    }
                    if (token.TryGetValue("message_raw_output", out IJsonValue message_raw_output))
                    {
                        JsonArray array = JsonArray.Parse(message_raw_output.GetString());
                        MessageRawOutput = string.Empty;
                        StringBuilder builder = new StringBuilder();
                        foreach (IJsonValue item in array)
                        {
                            if (item.GetObject().TryGetValue("type", out IJsonValue type))
                            {
                                switch (type.GetString())
                                {
                                    case "text":
                                        if (item.GetObject().TryGetValue("message", out IJsonValue message))
                                        {
                                            _ = builder.Append(message.GetString());
                                        }
                                        break;

                                    case "image":
                                        string description = item.GetObject().TryGetValue("description", out IJsonValue des) && !string.IsNullOrEmpty(des.GetString()) ? des.GetString() : string.Empty;
                                        string uri = item.GetObject().TryGetValue("url", out IJsonValue url) && !string.IsNullOrEmpty(url.GetString()) ? url.GetString() : string.Empty;
                                        _ = builder.Append($"\n<a t=\"image\" href=\"{uri}\">{description}</a>\n");
                                        break;
                                }
                            }
                        }
                        MessageRawOutput = builder.ToString();
                    }
                }
                else if (feedType.GetString() == "answer")
                {
                    IsAnswerFeed = true;
                    if (token.TryGetValue("extraData", out IJsonValue v1))
                    {
                        JsonObject extraData = JsonObject.Parse(v1.GetString());
                        if (extraData.TryGetValue("questionAnswerNum", out IJsonValue questionAnswerNum))
                        {
                            QuestionAnswerNum = questionAnswerNum.GetNumber().ToString();
                        }
                        if (extraData.TryGetValue("questionUrl", out IJsonValue questionUrl))
                        {
                            QuestionUrl = questionUrl.GetString();
                        }
                    }
                }
            }

            if (token.TryGetValue("targetRow", out IJsonValue v) && !string.IsNullOrEmpty(v.GetObject().ToString()))
            {
                ShowDyhName = true;
                JsonObject targetRow = v.GetObject();
                if (targetRow.TryGetValue("logo", out IJsonValue logo))
                {
                    GetDyhPic(logo.GetString());
                }
                if (targetRow.TryGetValue("title", out IJsonValue dyhtitle))
                {
                    DyhName = dyhtitle.GetString();
                }
                if (targetRow.TryGetValue("url", out IJsonValue url))
                {
                    DyhUrl = url.GetString();
                }
                if (targetRow.TryGetValue("subTitle", out IJsonValue subTitle))
                {
                    DyhSubtitle = subTitle.GetString();
                }
            }

            if (token.TryGetValue("ttitle", out IJsonValue valuettitle) && !ShowDyhName && !string.IsNullOrEmpty(valuettitle.GetString()))
            {
                ShowTtitle = true;
                Ttitle = valuettitle.GetString();
                if (token.TryGetValue("turl", out IJsonValue turi))
                {
                    Turl = turi.GetString();
                }
                if (token.TryGetValue("tpic", out IJsonValue tpicurl))
                {
                    tpicUrl = tpicurl.GetString();
                    GetTpic(tpicUrl);
                }
            }

            List<RelationRowsItem> Rows = new List<RelationRowsItem>();

            if (token.TryGetValue("location", out IJsonValue location) && !string.IsNullOrEmpty(location.GetString()))
            {
                ShowRelationRows = true;
                if (location != null && !string.IsNullOrEmpty(location.GetString()))
                {
                    Rows.Add(new RelationRowsItem
                    {
                        title = location.GetString(),
                        logoUrl = "https://www.microsoft.com/design/fluent/assets/images/icon-app-maps@2x.png"
                    });
                }
            }

            if (token.TryGetValue("relationRows", out IJsonValue relationRows) && (relationRows.GetArray() ?? new JsonArray()).Count > 0)
            {
                ShowRelationRows = true;
                if (relationRows != null)
                {
                    foreach (IJsonValue item in relationRows.GetArray())
                    {
                        Rows.Add(new RelationRowsItem
                        {
                            title = item.GetObject().TryGetValue("title", out IJsonValue rowstitle) ? rowstitle.GetString() : string.Empty,
                            url = item.GetObject().TryGetValue("url", out IJsonValue url) ? url.GetString() : string.Empty,
                            logoUrl = item.GetObject().TryGetValue("logo", out IJsonValue logo) ? logo.GetString() : string.Empty
                        });
                    }
                }
            }

            if (Rows.Count >= 0)
            {
                ShowRelationRows = true;
                RelationRows = Rows.ToArray();
                GetRowPic();
            }
        }

        private async void GetDyhPic(string logo)
        {
            DyhPic = await ImageCache.GetImage(ImageType.Icon, logo);
        }

        private async void GetCover(string message_cover)
        {
            DyhPic = await ImageCache.GetImage(ImageType.OriginImage, message_cover);
        }

        private async void GetTpic(string tpicurl)
        {
            Tpic = await ImageCache.GetImage(ImageType.Icon, tpicurl);
        }

        private async void GetRowPic()
        {
            foreach (RelationRowsItem item in RelationRows)
            {
                item.logo = !string.IsNullOrEmpty(item.logoUrl) ? await ImageCache.GetImage(ImageType.Icon, item.logoUrl) : ImageCache.NoPic;
            }
        }
    }
}
