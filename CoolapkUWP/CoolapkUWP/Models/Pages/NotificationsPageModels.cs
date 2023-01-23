using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Feeds;
using CoolapkUWP.Models.Images;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.ServiceModel.Channels;
using Windows.ApplicationModel.Resources;

namespace CoolapkUWP.Models.Pages
{
    public abstract class NotificationModel : Entity
    {
        public int ID { get; protected set; }

        public string Url { get; protected set; }
        public string UserUrl { get; protected set; }
        public string UserName { get; protected set; }
        public string Dateline { get; protected set; }
        public string BlockStatus { get; protected set; }

        public ImageModel UserAvatar { get; protected set; }

        protected NotificationModel(JObject token) : base(token) { }

        public override string ToString() => $"{UserName} - {Dateline}";
    }

    internal class SimpleNotificationModel : NotificationModel
    {
        public string Note { get; private set; }

        public SimpleNotificationModel(JObject token) : base(token)
        {
            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("FeedListPage");

            if (token.TryGetValue("id", out JToken id))
            {
                ID = id.ToObject<int>();
            }

            if (token.TryGetValue("url", out JToken url))
            {
                UserUrl = url.ToString();
            }

            if (token.TryGetValue("dateline", out JToken dateline))
            {
                Dateline = dateline.ToObject<long>().ConvertUnixTimeStampToReadable();
            }

            if (token.TryGetValue("note", out JToken _note))
            {
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(_note.ToString());
                HtmlNodeCollection nodes = doc.DocumentNode.ChildNodes;
                HtmlNode node = nodes.Last();
                Note = doc.DocumentNode.InnerText;
                Url = node.GetAttributeValue("href", string.Empty);
            }

            if (token.TryGetValue("fromUserAvatar", out JToken fromUserAvatar))
            {
                UserAvatar = new ImageModel(fromUserAvatar.ToString(), ImageType.BigAvatar);
            }

            if (token.TryGetValue("fromUserInfo", out JToken v1))
            {
                JObject fromUserInfo = (JObject)v1;
                BlockStatus = fromUserInfo.Value<int>("status") == -1 ? loader.GetString("Status-1")
                   : fromUserInfo.Value<int>("block_status") == -1 ? loader.GetString("BlockStatus-1")
                   : fromUserInfo.Value<int>("block_status") == 2 ? loader.GetString("BlockStatus2") : null;
            }

            if (token.TryGetValue("fromusername", out JToken fromusername))
            {
                UserName = $"{fromusername} {BlockStatus}";
            }

            if (token.TryGetValue("block_status", out JToken block_status) && block_status.ToString() != "0")
            {
                Dateline += " [已折叠]";
            }

            if (token.TryGetValue("status", out JToken status) && status.ToString() == "-1")
            {
                Dateline += " [仅自己可见]";
            }
        }

        public override string ToString() => Note;
    }

    internal class AtCommentMeNotificationModel : NotificationModel
    {
        public string Message { get; private set; }
        public string FeedMessage { get; private set; }

        public AtCommentMeNotificationModel(JObject token) : base(token)
        {
            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("FeedListPage");

            if (token.TryGetValue("id", out JToken id))
            {
                ID = id.ToObject<int>();
            }

            if (token.TryGetValue("url", out JToken url))
            {
                Url = url.ToString();
            }

            if (token.TryGetValue("uid", out JToken uid))
            {
                UserUrl = $"/u/{uid}";
            }

            if (token.TryGetValue("dateline", out JToken dateline))
            {
                Dateline = dateline.ToObject<long>().ConvertUnixTimeStampToReadable();
            }

            if (token.TryGetValue("extra_title", out JToken extra_title))
            {
                FeedMessage = extra_title.ToString();
            }

            if (token.TryGetValue("userAvatar", out JToken userAvatar))
            {
                UserAvatar = new ImageModel(userAvatar.ToString(), ImageType.BigAvatar);
            }

            Message = $"{(string.IsNullOrEmpty(token.Value<string>("rusername")) ? string.Empty : $"回复<a href=\"/u/{token.Value<string>("ruid")}\">{token.Value<string>("rusername")}</a>: ")}{token.Value<string>("message")}";

            if (token.TryGetValue("userInfo", out JToken v1))
            {
                JObject userInfo = (JObject)v1;
                BlockStatus = userInfo.Value<int>("status") == -1 ? loader.GetString("Status-1")
                   : userInfo.Value<int>("block_status") == -1 ? loader.GetString("BlockStatus-1")
                   : userInfo.Value<int>("block_status") == 2 ? loader.GetString("BlockStatus2") : null;
            }

            if (token.TryGetValue("username", out JToken username))
            {
                UserName = $"{username} {BlockStatus}";
            }

            if (token.TryGetValue("block_status", out JToken block_status) && block_status.ToString() != "0")
            {
                Dateline += " [已折叠]";
            }

            if (token.TryGetValue("status", out JToken status) && status.ToString() == "-1")
            {
                Dateline += " [仅自己可见]";
            }
        }

        public override string ToString() => Message;
    }

    internal class LikeNotificationModel : NotificationModel
    {
        public string Title { get; private set; }

        public FeedModel FeedDetail { get; private set; }

        public LikeNotificationModel(JObject token) : base(token)
        {
            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("FeedListPage");

            if (token.TryGetValue("id", out JToken id))
            {
                ID = id.ToObject<int>();
            }

            if (token.TryGetValue("url", out JToken url))
            {
                Url = url.ToString();
            }

            if (token.TryGetValue("feedTypeName", out JToken feedTypeName))
            {
                Title = $"赞了你的{feedTypeName}";
            }
            else if (token.TryGetValue("infoHtml", out JToken infoHtml))
            {
                Title = $"赞了你的{infoHtml}";
            }

            if (token.TryGetValue("likeUid", out JToken likeUid))
            {
                UserUrl = $"/u/{likeUid}";
            }

            if (token.TryGetValue("likeTime", out JToken likeTime))
            {
                Dateline = likeTime.ToObject<long>().ConvertUnixTimeStampToReadable();
            }

            if (token.TryGetValue("likeAvatar", out JToken likeAvatar))
            {
                UserAvatar = new ImageModel(likeAvatar.ToString(), ImageType.BigAvatar);
            }

            if (token.TryGetValue("likeUserInfo", out JToken v1))
            {
                JObject likeUserInfo = (JObject)v1;
                BlockStatus = likeUserInfo.Value<int>("status") == -1 ? loader.GetString("Status-1")
                   : likeUserInfo.Value<int>("block_status") == -1 ? loader.GetString("BlockStatus-1")
                   : likeUserInfo.Value<int>("block_status") == 2 ? loader.GetString("BlockStatus2") : null;
            }

            if (token.TryGetValue("likeUsername", out JToken likeUsername))
            {
                UserName = $"{likeUsername} {BlockStatus}";
            }

            if (token.TryGetValue("block_status", out JToken block_status) && block_status.ToString() != "0")
            {
                Dateline += " [已折叠]";
            }

            if (token.TryGetValue("status", out JToken status) && status.ToString() == "-1")
            {
                Dateline += " [仅自己可见]";
            }

            FeedDetail = new FeedModel(token) { ShowButtons = false };
        }

        public override string ToString() => Title;
    }

    internal class MessageNotificationModel : NotificationModel
    {
        public string FeedMessage { get; private set; }

        public MessageNotificationModel(JObject token) : base(token)
        {
            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("FeedListPage");

            if (token.TryGetValue("id", out JToken id))
            {
                ID = id.ToObject<int>();
            }

            if (token.TryGetValue("ukey", out JToken ukey))
            {
                Url = ukey.ToString();
            }

            if (token.TryGetValue("uid", out JToken uid))
            {
                UserUrl = $"/u/{uid}";
            }

            if (token.TryGetValue("dateline", out JToken dateline))
            {
                Dateline = dateline.ToObject<long>().ConvertUnixTimeStampToReadable();
            }

            if (token.TryGetValue("message", out JToken message))
            {
                FeedMessage = message.ToString();
            }

            if (token.TryGetValue("messageUserInfo", out JToken v1))
            {
                JObject messageUserInfo = (JObject)v1;

                if (messageUserInfo.TryGetValue("userAvatar", out JToken userAvatar))
                {
                    UserAvatar = new ImageModel(userAvatar.ToString(), ImageType.BigAvatar);
                }

                BlockStatus = messageUserInfo.Value<int>("status") == -1 ? loader.GetString("Status-1")
                   : messageUserInfo.Value<int>("block_status") == -1 ? loader.GetString("BlockStatus-1")
                   : messageUserInfo.Value<int>("block_status") == 2 ? loader.GetString("BlockStatus2") : null;

                if (messageUserInfo.TryGetValue("username", out JToken username))
                {
                    UserName = $"{username} {BlockStatus}";
                }

            }

            if (token.TryGetValue("is_top", out JToken is_top) && is_top.ToObject<int>() == 1)
            {
                Dateline += " " + "[置顶]";
            }
        }
    }
}
