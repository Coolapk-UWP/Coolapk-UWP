using CoolapkUWP.Data;
using System.Collections.Generic;
using Windows.Data.Json;

namespace CoolapkUWP.Control.ViewModels
{
    class SourceFeedViewModel : Entity
    {
        public SourceFeedViewModel(IJsonValue t) : base(t)
        {
            JsonObject token = t.GetObject();
            url = token["url"].GetString();
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
        public string uurl { get; private set; }
        public string username { get; private set; }
        public string dateline { get; private set; }
        public bool showMessage_title { get; private set; }
        public string message_title { get; private set; }
        public string message { get; private set; }
        public bool showPicArr { get; private set; }
        public string[] picArr { get; private set; }
    }
}
