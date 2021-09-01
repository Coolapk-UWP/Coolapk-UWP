using Windows.Data.Json;

namespace CoolapkUWP.Control.ViewModels
{
    internal class SimpleFeedReplyViewModel
    {
        public double Id { get; private set; }

        public string Uurl { get; private set; }
        public string PicUrl { get; private set; }
        public string Message { get; private set; }
        public string Username { get; private set; }
        public string ReplyUrl { get; private set; }
        public string ReplyUsername { get; private set; }

        public bool ShowPic { get; private set; }
        public bool IsFeedAuthor { get; private set; }
        public bool ShowReplyUser { get => !string.IsNullOrEmpty(ReplyUsername); }

        public SimpleFeedReplyViewModel(IJsonValue t)
        {
            JsonObject token = t.GetObject();
            if (token.TryGetValue("id", out IJsonValue id))
            {
                Id = id.GetNumber();
            }
            if (token.TryGetValue("uid", out IJsonValue uid))
            {
                Uurl = $"/u/{uid.GetNumber()}";
            }
            if (token.TryGetValue("username", out IJsonValue username))
            {
                Username = username.GetString();
            }
            if (token.TryGetValue("isFeedAuthor", out IJsonValue isFeedAuthor) && isFeedAuthor.GetNumber() == 1)
            {
                IsFeedAuthor = true;
            }
            if (token.TryGetValue("ruid", out IJsonValue ruid))
            {
                ReplyUrl = $"/u/{ruid.GetNumber()}";
            }
            if (token.TryGetValue("rusername", out IJsonValue rusername))
            {
                ReplyUsername = rusername.GetString();
            }
            Message = ShowReplyUser
                ? $"<a href=\"{Uurl}\">{username}{(IsFeedAuthor ? "(楼主)" : string.Empty)}</a>@<a href=\"{ReplyUrl}\">{ReplyUsername}</a>:{(token.TryGetValue("message", out IJsonValue message) ? message.GetString() : string.Empty)}"
                : $"<a href=\"{Uurl}\">{username}{(IsFeedAuthor ? "(楼主)" : string.Empty)}</a>:{(token.TryGetValue("message", out message) ? message.GetString() : string.Empty)}";
            if (token.TryGetValue("pic", out IJsonValue pic) && !string.IsNullOrEmpty(pic.GetString()))
            {
                ShowPic = true;
                PicUrl = pic.GetString();
                Message += $" <a href=\"{PicUrl}\">查看图片</a>";
            }
        }
    }
}
