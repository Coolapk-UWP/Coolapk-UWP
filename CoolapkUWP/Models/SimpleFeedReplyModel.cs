using Newtonsoft.Json.Linq;

namespace CoolapkUWP.Models
{
    internal class SimpleFeedReplyModel
    {
        public SimpleFeedReplyModel(JObject token)
        {
            Id = token.Value<int>("id");
            Uurl = $"/u/{token.Value<int>("uid")}";
            Username = token.Value<string>("username");
            IsFeedAuthor = token.Value<int>("isFeedAuthor") == 1;
            Rurl = $"/u/{token.Value<int>("ruid")}";
            Rusername = token.Value<string>("rusername");
            Message = string.IsNullOrEmpty(Rusername)
                ? $"<a href=\"{Uurl}\" type=\"user-detail\">{Username}{(IsFeedAuthor ? "[楼主]" : string.Empty)}</a>: {token.Value<string>("message")}"
                : $"<a href=\"{Uurl}\" type=\"user-detail\">{Username}{(IsFeedAuthor ? "[楼主]" : string.Empty)}</a>@<a href=\"{Rurl}\" type=\"user-detail\">{Rusername}</a>: {token.Value<string>("message")}";

            ShowPic = token.TryGetValue("pic", out JToken value) && !string.IsNullOrEmpty(value.ToString());
            if (ShowPic)
            {
                PicUri = value.ToString();
                Message += $" <a href=\"{PicUri}\">查看图片</a>";
            }
        }

        public string Rusername { get; private set; }
        public string Rurl { get; private set; }
        public double Id { get; private set; }
        public string Uurl { get; private set; }
        public string Username { get; private set; }
        public string Message { get; private set; }
        public bool IsFeedAuthor { get; private set; }
        public bool ShowPic { get; protected set; }
        public string PicUri { get; private set; }
    }
}