using CoolapkUWP.Data;
using System;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using Windows.Data.Json;
using Windows.UI.Xaml.Media;

namespace CoolapkUWP.Models.Pages
{
    internal interface INotificationModel
    {
        double id { get; }
        void Initial(IJsonValue o);
    }

    internal class SimpleNotificationModel : INotifyPropertyChanged, INotificationModel
    {
        public ImageSource FromUserAvatar { get; private set; }
        public string FromUserName { get; private set; }
        public string FromUserUri { get; private set; }
        public string Dateline { get; private set; }
        public string Uri { get; private set; }
        public string Note { get; private set; }
        public double id { get; private set; }
        public event PropertyChangedEventHandler PropertyChanged;

        public void Initial(IJsonValue o)
        {
            JsonObject token = o.GetObject();

            if (token.TryGetValue("id", out IJsonValue Id))
            {
                id = Id.GetNumber();
            }
            if (token.TryGetValue("fromusername", out IJsonValue fromusername))
            {
                FromUserName = fromusername.GetString();
            }
            if (token.TryGetValue("url", out IJsonValue url))
            {
                FromUserUri = url.GetString();
            }
            if (token.TryGetValue("dateline", out IJsonValue dateline))
            {
                Dateline = dateline.GetNumber().ConvertTime();
            }
            string note = token["note"].GetString();
            Regex regex = new Regex("<a.*?>.*?</a>"), regex2 = new Regex("href=\".*"), regex3 = new Regex(">.*<");
            while (regex.IsMatch(note))
            {
                Match link = regex.Match(note);
                string content = regex3.Match(link.Value).Value.Replace(">", string.Empty);
                content = content.Replace("<", string.Empty);
                string href = regex2.Match(link.Value).Value.Replace("href=\"", string.Empty);
                if (href.IndexOf("\"", StringComparison.Ordinal) > 0)
                {
                    href = href.Substring(0, href.IndexOf("\"", StringComparison.Ordinal));
                }
                Uri = href;
                note = note.Replace(link.Value, content);
            }
            Note = note;
            if (token.TryGetValue("fromUserInfo", out IJsonValue v1))
            {
                JsonObject fromUserInfo = v1.GetObject();
                if (fromUserInfo.TryGetValue("userSmallAvatar", out IJsonValue userSmallAvatar))
                {
                    GetPic(userSmallAvatar.GetString());
                }
            }
            else if (token.TryGetValue("fromUserAvatar", out IJsonValue fromUserAvatar))
            {
                GetPic(fromUserAvatar.GetString());
            }
            if (SettingsHelper.IsSpecialUser && token.TryGetValue("block_status", out IJsonValue block_status) && block_status.GetNumber() != 0)
            { Dateline += " [已折叠]"; }
            if (token.TryGetValue("status", out IJsonValue status) && status.GetNumber() == -1)
            { Dateline += " [仅自己可见]"; }
        }

        private async void GetPic(string u)
        {
            if (!string.IsNullOrEmpty(u))
            {
                FromUserAvatar = ImageCache.defaultNoAvatarUrl.Contains(u) ? ImageCache.NoPic : await ImageCache.GetImage(ImageType.SmallAvatar, u);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FromUserAvatar)));
            }
        }
    }

    internal class LikeNotificationModel : INotifyPropertyChanged, INotificationModel
    {
        public ImageSource LikeUserAvatar { get; private set; }
        public string LikeUserName { get; private set; }
        public string LikeUserUri { get; private set; }
        public string Dateline { get; private set; }
        public string Uri { get; private set; }
        public string FeedMessage { get; private set; }
        public string Title { get; private set; }
        public double id { get; private set; }
        public event PropertyChangedEventHandler PropertyChanged;

        public void Initial(IJsonValue o)
        {
            JsonObject token = o.GetObject();
            if (token.TryGetValue("id", out IJsonValue Id))
            {
                id = Id.GetNumber();
            }
            if (token.TryGetValue("likeUsername", out IJsonValue likeUsername))
            {
                LikeUserName = likeUsername.GetString();
            }
            if (token.TryGetValue("likeUid", out IJsonValue likeUid))
            {
                LikeUserUri = $"/u/{likeUid.GetNumber()}";
            }
            if (token.TryGetValue("likeTime", out IJsonValue likeTime))
            {
                Dateline = likeTime.GetNumber().ConvertTime();
            }
            if (token.TryGetValue("url", out IJsonValue url))
            {
                Uri = url.GetString();
            }
            if (token.TryGetValue("likeAvatar", out IJsonValue likeAvatar))
            {
                GetPic(likeAvatar.GetString());
            }
            if (token.TryGetValue("feedTypeName", out IJsonValue feedTypeName))
            {
                Title = $"赞了你的{feedTypeName.GetString()}";
            }
            else if (token.TryGetValue("infoHtml", out IJsonValue infoHtml))
            {
                Title = $"赞了你的{infoHtml.GetString()}";
            }
            if (token.TryGetValue("message", out IJsonValue message))
            {
                FeedMessage = message.GetString();
            }
        }

        private async void GetPic(string u)
        {
            if (!string.IsNullOrEmpty(u))
            {
                LikeUserAvatar = ImageCache.defaultNoAvatarUrl.Contains(u) ? ImageCache.NoPic : await ImageCache.GetImage(ImageType.BigAvatar, u);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LikeUserAvatar)));
            }
        }
    }

    internal class AtCommentMeNotificationModel : INotifyPropertyChanged, INotificationModel
    {
        public ImageSource UserAvatar { get; private set; }
        public string UserName { get; private set; }
        public string UserUri { get; private set; }
        public string Dateline { get; private set; }
        public string Uri { get; private set; }
        public string Message { get; private set; }
        public string FeedMessage { get; private set; }
        public double id { get; private set; }
        public event PropertyChangedEventHandler PropertyChanged;

        public void Initial(IJsonValue o)
        {
            JsonObject token = o.GetObject();
            id = token["id"].GetNumber();

            UserName = token["username"].GetString();
            UserUri = $"/u/{token["uid"].GetNumber()}";
            Dateline = token["dateline"].GetNumber().ConvertTime();
            Uri = token["url"].GetString();
            GetPic(token["userAvatar"].GetString());
            Message = (string.IsNullOrEmpty(token["rusername"].GetString()) ? string.Empty : $"回复<a href=\"/u/{token["ruid"].GetNumber()}\">{token["rusername"].GetString()}</a>: ") + token["message"].GetString();
            FeedMessage = (token["extra_title"].GetString());
        }

        private async void GetPic(string u)
        {
            if (!string.IsNullOrEmpty(u))
            {
                UserAvatar = ImageCache.defaultNoAvatarUrl.Contains(u) ? ImageCache.NoPic : await ImageCache.GetImage(ImageType.BigAvatar, u);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UserAvatar)));
            }
        }
    }

    internal class MessageNotificationModel : INotifyPropertyChanged, INotificationModel
    {
        public ImageSource UserAvatar { get; private set; }
        public string UserName { get; private set; }
        public string UserUri { get; private set; }
        public string Dateline { get; private set; }
        public string Uri { get; private set; }
        public string FeedMessage { get; private set; }
        public double id { get; private set; }
        public event PropertyChangedEventHandler PropertyChanged;

        public void Initial(IJsonValue o)
        {
            JsonObject token = o.GetObject();
            if (token.TryGetValue("id", out IJsonValue Id))
            {
                id = Id.GetNumber();
                UserUri = $"/u/{id}";
            }
            if (token.TryGetValue("dateline", out IJsonValue dateline))
            {
                Dateline = dateline.GetNumber().ConvertTime();
            }
            if (token.TryGetValue("messageUserInfo", out IJsonValue v1))
            {
                JsonObject messageUserInfo = v1.GetObject();
                if (messageUserInfo.TryGetValue("username", out IJsonValue username))
                {
                    UserName = username.GetString();
                }
                if (messageUserInfo.TryGetValue("userAvatar", out IJsonValue userAvatar))
                {
                    string avatar = userAvatar.GetString();
                    if (!string.IsNullOrEmpty(avatar))
                    {
                        GetPic(avatar);
                    }
                }
            }
            if (token.TryGetValue("ukey", out IJsonValue ukey))
            {
                Uri = ukey.GetString();
            }
            if (token.TryGetValue("message", out IJsonValue message))
            {
                FeedMessage = message.GetString();
            }
            if (token.TryGetValue("is_top", out IJsonValue is_top) && is_top.GetNumber() == 1)
            {
                Dateline += " [置顶]";
            }
        }

        private async void GetPic(string u)
        {
            if (!string.IsNullOrEmpty(u))
            {
                UserAvatar = ImageCache.defaultNoAvatarUrl.Contains(u) ? ImageCache.NoPic : await ImageCache.GetImage(ImageType.BigAvatar, u);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UserAvatar)));
            }
        }
    }
}
