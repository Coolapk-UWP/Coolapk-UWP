using CoolapkUWP.Controls;
using Newtonsoft.Json.Linq;

namespace CoolapkUWP.Models
{
    internal class SimpleFeedReplyModel : Entity
    {
        public SimpleFeedReplyModel(JObject o) : base(o)
        {
            Id = o.Value<int>("id");
            Uurl = $"/u/{o.Value<int>("uid")}";
            Username = o.Value<string>("username");
            IsFeedAuthor = o.Value<int>("isFeedAuthor") == 1;
            Rurl = $"/u/{o.Value<int>("ruid")}";
            Rusername = o.Value<string>("rusername");
            
            var loader = Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse("Feed");

            Message = string.IsNullOrEmpty(Rusername)
                ? $"{GetUserLink(Uurl, Username) + GetAuthorString()}: {o.Value<string>("message")}"
                : $"{GetUserLink(Uurl, Username) + GetAuthorString()}@{GetUserLink(Rurl, Rusername)}: {o.Value<string>("message")}";

            ShowPic = o.TryGetValue("pic", out JToken value) && !string.IsNullOrEmpty(value.ToString());
            if (ShowPic)
            {
                PicUri = value.ToString();
                Message += $" <a href=\"{PicUri}\">{loader.GetString("seePic")}</a>";
            }
        }

        private string GetAuthorString()
        {
            return IsFeedAuthor ? TextBlockEx.AuthorBorder : string.Empty;
        }

        private string GetUserLink(string url, string name)
        {
            return $"<a href=\"{url}\" type=\"user-detail\">{name}</a>";
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