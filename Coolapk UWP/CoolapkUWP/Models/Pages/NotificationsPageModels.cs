using CoolapkUWP.Core.Models;
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
        public string BlockStatus { get; private set; }

        public SimpleNotificationModel(JObject o) : base(o)
        {
            var loader = Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse("FeedListPage");
            BlockStatus = o["fromUserInfo"].Value<int>("status") == -1 ? loader.GetString("status-1")
               : UIHelper.IsSpecialUser && o["fromUserInfo"].Value<int>("block_status") == -1 ? loader.GetString("block_status-1")
               : UIHelper.IsSpecialUser && o["fromUserInfo"].Value<int>("block_status") == 2 ? loader.GetString("block_status2") : null;

            Id = o.Value<int>("id");
            UserName = o.Value<string>("fromusername") + " " + BlockStatus;
            UserUri = o.Value<string>("url");
            Dateline = DataHelper.ConvertUnixTimeStampToReadable(o.Value<int>("dateline"));

            var note = o.Value<string>("note");
            Regex regex = new Regex("<a.*?>.*?</a>"), regex2 = new Regex("href=\".*"), regex3 = new Regex(">.*<");
            while (regex.IsMatch(note))
            {
                var link = regex.Match(note);
                var content = regex3.Match(link.Value).Value.Replace(">", string.Empty, System.StringComparison.Ordinal);
                content = content.Replace("<", string.Empty, System.StringComparison.Ordinal);
                var href = regex2.Match(link.Value).Value.Replace("href=\"", string.Empty, System.StringComparison.Ordinal);
                if (href.IndexOf("\"", System.StringComparison.Ordinal) > 0)
                {
                    href = href.Substring(0, href.IndexOf("\"", System.StringComparison.Ordinal));
                }

                Uri = href;
                note = note.Replace(link.Value, content, System.StringComparison.Ordinal);
            }
            Note = note;
            var avatar = o["fromUserInfo"].Value<string>("userSmallAvatar");
            if (!string.IsNullOrEmpty(avatar))
            {
                UserAvatar = new ImageModel(avatar, ImageType.BigAvatar);
            }
            if (UIHelper.IsSpecialUser && o.TryGetValue("block_status", out JToken v) && v.ToString() != "0")
            { Dateline += " [已折叠]"; }
            if (o.TryGetValue("status", out JToken s) && s.ToString() == "-1")
            { Dateline += " [仅自己可见]"; }
        }
    }

    internal class LikeNotificationModel : NotificationModel
    {
        public string Title { get; private set; }
        public string FeedMessage { get; private set; }
        public string BlockStatus { get; private set; }

        public LikeNotificationModel(JObject o) : base(o)
        {
            var loader = Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse("FeedListPage");
            //BlockStatus = o["likeUserInfo"].Value<int>("status") == -1 ? loader.GetString("status-1")
            //   : UIHelper.IsSpecialUser && o["likeUserInfo"].Value<int>("block_status") == -1 ? loader.GetString("block_status-1")
            //   : UIHelper.IsSpecialUser && o["likeUserInfo"].Value<int>("block_status") == 2 ? loader.GetString("block_status2") : null;
            Id = o.Value<int>("id");
            UserUri = "/u/" + o.Value<int>("likeUid");
            Dateline = DataHelper.ConvertUnixTimeStampToReadable(o.Value<int>("likeTime"));
            UserName = o.Value<string>("likeUsername") + " " + BlockStatus;
            Uri = o.Value<string>("url");
            Title = "赞了你的" + (o.TryGetValue("feedTypeName", out JToken value) ? value.ToString() : o.Value<string>("infoHtml"));
            FeedMessage = o.Value<string>("message");
            var avatar = o.Value<string>("likeAvatar");
            if (!string.IsNullOrEmpty(avatar))
            {
                UserAvatar = new ImageModel(avatar, ImageType.BigAvatar);
            }
            if (UIHelper.IsSpecialUser && o.TryGetValue("block_status", out JToken v) && v.ToString() != "0")
            { Dateline += " [已折叠]"; }
            if (o.TryGetValue("status", out JToken s) && s.ToString() == "-1")
            { Dateline += " [仅自己可见]"; }
        }
    }

    internal class AtCommentMeNotificationModel : NotificationModel
    {
        public string Message { get; private set; }
        public string FeedMessage { get; private set; }
        public string BlockStatus { get; private set; }

        public AtCommentMeNotificationModel(JObject o) : base(o)
        {
            var loader = Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse("FeedListPage");
            BlockStatus = o["userInfo"].Value<int>("status") == -1 ? loader.GetString("status-1")
               : UIHelper.IsSpecialUser && o["userInfo"].Value<int>("block_status") == -1 ? loader.GetString("block_status-1")
               : UIHelper.IsSpecialUser && o["userInfo"].Value<int>("block_status") == 2 ? loader.GetString("block_status2") : null;
            Id = o.Value<int>("id");
            UserUri = "/u/" + o.Value<int>("uid");
            Dateline = DataHelper.ConvertUnixTimeStampToReadable(o.Value<int>("dateline"));
            UserName = o.Value<string>("username") + " " + BlockStatus;
            Uri = o.Value<string>("url");
            FeedMessage = o.Value<string>("extra_title");
            Message =
                (string.IsNullOrEmpty(o.Value<string>("rusername"))
                 ? string.Empty
                 : $"回复<a href=\"/u/{o.Value<string>("ruid")}\">{o.Value<string>("rusername")}</a>: ")
                + o.Value<string>("message");
            var avatar = o.Value<string>("userAvatar");
            if (!string.IsNullOrEmpty(avatar))
            {
                UserAvatar = new ImageModel(avatar, ImageType.BigAvatar);
            }
            if (UIHelper.IsSpecialUser && o.TryGetValue("block_status", out JToken v) && v.ToString() != "0")
            { Dateline += " [已折叠]"; }
            if (o.TryGetValue("status", out JToken s) && s.ToString() == "-1")
            { Dateline += " [仅自己可见]"; }
        }
    }

    internal class MessageNotificationModel : NotificationModel
    {
        public string FeedMessage { get; private set; }
        public string BlockStatus { get; private set; }

        public MessageNotificationModel(JObject o) : base(o)
        {
            var loader = Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse("FeedListPage");
            BlockStatus = o["messageUserInfo"].Value<int>("status") == -1 ? loader.GetString("status-1")
                : UIHelper.IsSpecialUser && o["messageUserInfo"].Value<int>("block_status") == -1 ? loader.GetString("block_status-1")
                : UIHelper.IsSpecialUser && o["messageUserInfo"].Value<int>("block_status") == 2 ? loader.GetString("block_status2") : null;
            Id = o.Value<int>("id");
            UserUri = "/u/" + o.Value<int>("uid");
            Dateline = DataHelper.ConvertUnixTimeStampToReadable(o.Value<int>("dateline"));
            UserName = o["messageUserInfo"].Value<string>("username") + " " + BlockStatus;
            Uri = o.Value<string>("ukey");
            FeedMessage = o.Value<string>("message");
            var avatar = o["messageUserInfo"].Value<string>("userAvatar");
            if (!string.IsNullOrEmpty(avatar))
            {
                UserAvatar = new ImageModel(avatar, ImageType.BigAvatar);
            }
            if (o.Value<int>("is_top") == 1)
            { Dateline += " " + "[置顶]"; }
        }
    }
}