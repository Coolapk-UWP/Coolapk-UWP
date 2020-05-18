using Newtonsoft.Json.Linq;

namespace CoolapkUWP.Controls.ViewModels
{
    class SimpleFeedReplyViewModel
    {
        public SimpleFeedReplyViewModel(JToken t)
        {
            JObject token = t as JObject;
            id = token.Value<int>("id");
            uurl = $"/u/{token.Value<int>("uid")}";
            username = token.Value<string>("username");
            isFeedAuthor = token.Value<int>("isFeedAuthor") == 1;
            rurl = $"/u/{token.Value<int>("ruid")}";
            rusername = token.Value<string>("rusername");
            if (showRuser)
                message = $"<a href=\"{uurl}\" type=\"user-detail\">{username}{(isFeedAuthor ? "[楼主]" : string.Empty)}</a>@<a href=\"{rurl}\" type=\"user-detail\">{rusername}</a>: {token.Value<string>("message")}";
            else message = $"<a href=\"{uurl}\" type=\"user-detail\">{username}{(isFeedAuthor ? "[楼主]" : string.Empty)}</a>: {token.Value<string>("message")}";
            showPic = token.TryGetValue("pic", out JToken value) && !string.IsNullOrEmpty(value.ToString());
            if (showPic)
            {
                picUrl = value.ToString();
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
