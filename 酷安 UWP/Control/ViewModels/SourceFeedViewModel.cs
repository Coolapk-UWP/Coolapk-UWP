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
            url = token.TryGetValue("url", out IJsonValue json) ? json.GetString() : $"/feed/{token["id"].ToString().Replace("\"", string.Empty)}";
            if (token["entityType"].GetString() != "article")
            {
                uurl = token["userInfo"].GetObject()["url"].GetString();
                username = token["userInfo"].GetObject()["username"].GetString();
                dateline = Tools.ConvertTime(double.Parse(token["dateline"].ToString().Replace("\"", string.Empty)));
                message = Tools.GetMessageText(token["message"].GetString());
                message_title = token.TryGetValue("message_title", out IJsonValue j) ? j.GetString() : string.Empty;
            }
            else
            {
                dateline = Tools.ConvertTime(token["digest_time"].GetNumber());
                message = Tools.GetMessageText(token["message"].GetString().Substring(0, 120) + "……<a href=\"\">查看更多</a>");
                message_title = token["title"].GetString();
            }
            showMessage_title = !string.IsNullOrEmpty(message_title);

            showPicArr = token.TryGetValue("picArr", out IJsonValue value) && value.GetArray().Count > 0 && !string.IsNullOrEmpty(value.GetArray()[0].GetString());
            if (showPicArr)
            {
                List<string> vs = new List<string>();
                foreach (var item in value.GetArray())
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
