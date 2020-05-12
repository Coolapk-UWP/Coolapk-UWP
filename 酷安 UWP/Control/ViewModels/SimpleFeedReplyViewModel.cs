using CoolapkUWP.Data;
using Windows.Data.Json;

namespace CoolapkUWP.Control.ViewModels
{
    class SimpleFeedReplyViewModel
    {
        public SimpleFeedReplyViewModel(IJsonValue t)
        {
            JsonObject token = t.GetObject();
            id = token["id"].GetNumber();
            uurl = $"/u/{token["uid"].GetNumber()}";
            username = token["username"].GetString();
            isFeedAuthor = token["isFeedAuthor"].GetNumber() == 1;
            rurl = $"/u/{token["ruid"].GetNumber()}";
            rusername = token["rusername"].GetString();
            if (showRuser)
                message = $"<a href=\"{uurl}\">{username}{(isFeedAuthor ? "(楼主)" : string.Empty)}</a>@<a href=\"{rurl}\">{rusername}</a>: {token["message"].GetString()}";
            else message = $"<a href=\"{uurl}\">{username}{(isFeedAuthor ? "(楼主)" : string.Empty)}</a>: {token["message"].GetString()}";
            showPic = token.TryGetValue("pic", out IJsonValue value) && !string.IsNullOrEmpty(value.GetString());
            if (showPic)
            {
                picUrl = value.GetString();
                message += $" <a href=\"{picUrl}\">查看图片</a>";
            }
        }
        public bool showRuser { get => !string.IsNullOrEmpty(rusername); }
        public string rusername { get; private set; }
        public string rurl { get; private set; }
        public double id { get; private set; }
        public string uurl { get; private set; }
        public string username { get; private set; }
        public string message { get; private set; }
        public bool isFeedAuthor { get; private set; }
        public bool showPic { get; private set; }
        public string picUrl { get; private set; }
    }
}
