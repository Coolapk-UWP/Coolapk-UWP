using CoolapkUWP.Data;
using System;
using System.Collections.Generic;
using Windows.Data.Json;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace CoolapkUWP.Control.ViewModels
{
    class FeedDetailViewModel : FeedViewModelBase
    {
        public FeedDetailViewModel(IJsonValue t) : base(t)
        {
            JsonObject token = t.GetObject();
            title = token["title"].GetString();
            if (token["entityType"].GetString() != "article")
            {
                if (token["feedType"].GetString() == "feedArticle")
                    isFeedArticle = true;
                if (isFeedArticle)
                {
                    has_message_cover = token.TryGetValue("message_cover", out IJsonValue value) && !string.IsNullOrEmpty(value.GetString());
                    if (has_message_cover)
                        message_cover = new BitmapImage(new Uri(value.GetString()));
                    JsonArray array = JsonArray.Parse(token["message_raw_output"].GetString());
                    message_raw_output = string.Empty;
                    foreach (var i in array)
                    {
                        JsonObject item = i.GetObject();
                        if (item["type"].GetString() == "text")
                            message_raw_output += Tools.GetMessageText(item["message"].GetString());
                        else if (item["type"].GetString() == "image")
                        {
                            string d = string.IsNullOrEmpty(item["description"].GetString()) ? string.Empty : item["description"].GetString();
                            message_raw_output += $"\n\n![image]({item["url"].GetString()}.s.jpg)\n\n>{d}\n\n";
                        }
                    }
                }
                if (token["feedType"].GetString() == "answer")
                    isAnswerFeed = true;
                if (isAnswerFeed)
                {
                    JsonObject j = JsonObject.Parse(token["extraData"].GetString());
                    questionAnswerNum = j["questionAnswerNum"].GetNumber().ToString();
                    questionUrl = j["questionUrl"].GetString();
                }
            }
            showTtitle = token.TryGetValue("ttitle", out IJsonValue valuettitle) && !string.IsNullOrEmpty(valuettitle.GetString());
            if (showTtitle)
            {
                ttitle = valuettitle.GetString();
                tpic = new BitmapImage(new Uri(token["tpic"].GetString()));
                turl = token["turl"].GetString();
            }
            show_dyh_name = token.TryGetValue("dyh_name", out IJsonValue valuedyh) && !string.IsNullOrEmpty(valuedyh.GetString());
            if (show_dyh_name)
            {
                dyh_name = valuedyh.GetString();
                dyhUrl = $"/dyh/{token["dyh_id"].GetNumber()}";
            }
            showRelationRows = (token.TryGetValue("location", out IJsonValue valuelocation) && !string.IsNullOrEmpty(valuelocation.GetString()))
                               | (token.TryGetValue("relationRows", out IJsonValue valuerelationRows) && (valuerelationRows.GetArray() ?? new JsonArray()).Count > 0);
            if (showRelationRows)
            {
                List<RelationRowsItem> vs = new List<RelationRowsItem>();
                if (valuelocation != null && !string.IsNullOrEmpty(valuelocation.GetString()))
                    vs.Add(new RelationRowsItem { title = valuelocation.GetString() });
                if (valuerelationRows != null)
                    foreach (var i in valuerelationRows.GetArray())
                    {
                        JsonObject item = i.GetObject();
                        vs.Add(new RelationRowsItem { title = item["title"].GetString(), url = item["url"].GetString(), logo = new BitmapImage(new Uri(item["logo"].GetString())) });
                    }
                if (vs.Count == 0) showRelationRows = false;
                relationRows = vs.ToArray();
            }
        }
        public bool isFeedArticle { get; private set; }
        public bool isFeedArticle2 { get => !isFeedArticle; }
        public bool has_message_cover { get; private set; }
        public ImageSource message_cover { get; private set; }
        public string message_raw_output { get; private set; }
        public bool showTtitle { get; private set; }
        public string turl { get; private set; }
        public string ttitle { get; private set; }
        public ImageSource tpic { get; private set; }
        public bool show_dyh_name { get; private set; }
        public string dyhUrl { get; private set; }// /dyh/{id}
        public string dyh_name { get; private set; }
        public bool isAnswerFeed { get; private set; }
        public string questionUrl { get; private set; }
        public string title { get; private set; }
        public string questionAnswerNum { get; private set; }//extraData.questionAnswerNum
        public bool showRelationRows { get; private set; }
        public RelationRowsItem[] relationRows { get; private set; }
    }
}
