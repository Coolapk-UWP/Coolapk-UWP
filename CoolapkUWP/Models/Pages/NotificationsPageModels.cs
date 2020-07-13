using CoolapkUWP.Helpers;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace CoolapkUWP.Models.Pages.NotificationsPageModels
{
    internal abstract class NotificationModel : Entity
    {
        public ImageModel UserAvatar { get; protected set; }
        public string UserName { get; protected set; }
        public string UserUri { get; protected set; }
        public string Dateline { get; protected set; }
        public string Uri { get; protected set; }
        public double Id { get; protected set; }

        protected NotificationModel(JObject o) : base(o) { }
    }

    internal class SimpleNotificationModel : NotificationModel
    {
        public string Note { get; private set; }

        public SimpleNotificationModel(JObject o) : base(o)
        {
            Id = o.Value<int>("id");
            UserName = o.Value<string>("fromusername");
            UserUri = o.Value<string>("url");
            Dateline = DataHelper.ConvertUnixTimeToReadable(o.Value<int>("dateline"));
            string s = o.Value<string>("note");
            Regex regex = new Regex("<a.*?>.*?</a>"), regex2 = new Regex("href=\".*"), regex3 = new Regex(">.*<");
            while (regex.IsMatch(s))
            {
                var h = regex.Match(s);
                string t = regex3.Match(h.Value).Value.Replace(">", string.Empty);
                t = t.Replace("<", string.Empty);
                string tt = regex2.Match(h.Value).Value.Replace("href=\"", string.Empty);
                if (tt.IndexOf("\"") > 0) tt = tt.Substring(0, tt.IndexOf("\""));
                Uri = tt;
                s = s.Replace(h.Value, t);
            }
            Note = s;
            var u = o["fromUserInfo"].Value<string>("userSmallAvatar");
            if (!string.IsNullOrEmpty(u))
            {
                UserAvatar = new ImageModel(u, ImageType.BigAvatar);
            }
        }
    }

    internal class LikeNotificationModel : NotificationModel
    {
        public string Title { get; private set; }
        public string FeedMessage { get; private set; }

        public LikeNotificationModel(JObject o) : base(o)
        {
            Id = o.Value<int>("id");
            UserUri = "/u/" + o.Value<int>("likeUid");
            Dateline = DataHelper.ConvertUnixTimeToReadable(o.Value<int>("likeTime"));
            UserName = o.Value<string>("likeUsername");
            Uri = o.Value<string>("url");
            Title = "赞了你的" + (o.TryGetValue("feedTypeName", out JToken value) ? value.ToString() : o.Value<string>("infoHtml"));
            FeedMessage = o.Value<string>("message");
            var u = o.Value<string>("likeAvatar");
            if (!string.IsNullOrEmpty(u))
            {
                UserAvatar = new ImageModel(u, ImageType.BigAvatar);
            }
        }
    }

    internal class AtCommentMeNotificationModel : NotificationModel
    {
        public string Message { get; private set; }
        public string FeedMessage { get; private set; }

        public AtCommentMeNotificationModel(JObject o) : base(o)
        {
            Id = o.Value<int>("id");
            UserUri = "/u/" + o.Value<int>("uid");
            Dateline = DataHelper.ConvertUnixTimeToReadable(o.Value<int>("dateline"));
            UserName = o.Value<string>("username");
            Uri = o.Value<string>("url");
            FeedMessage = (o.Value<string>("extra_title"));
            Message = (string.IsNullOrEmpty(o.Value<string>("rusername")) ? string.Empty : $"回复<a href=\"/u/{o.Value<string>("ruid")}\">{o.Value<string>("rusername")}</a>: ") + o.Value<string>("message");
            var uri = o.Value<string>("userAvatar");
            if (!string.IsNullOrEmpty(uri))
            {
                UserAvatar = new ImageModel(uri, ImageType.BigAvatar);
            }
        }
    }
}