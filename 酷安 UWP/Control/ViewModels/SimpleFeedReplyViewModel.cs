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
            message = Tools.GetMessageText($"<a href=\"{uurl}\">{username}{(isFeedAuthor ? "[楼主]" : string.Empty)}</a>:{token["message"].GetString()}");
        }
        public double id { get; private set; }
        public string uurl { get; private set; }
        public string username { get; private set; }
        public string message { get; private set; }
        public bool isFeedAuthor { get; private set; }
    }
}
