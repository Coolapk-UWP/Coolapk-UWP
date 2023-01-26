using CoolapkUWP.Controls;
using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Images;
using CoolapkUWP.Models.Users;
using Newtonsoft.Json.Linq;
using System.Collections.Immutable;
using System.Linq;

namespace CoolapkUWP.Models.Feeds
{
    public class SourceFeedReplyModel : Entity
    {
        public int ID { get; private set; }
        public int BlockStatus { get; private set; }

        public bool IsFeedAuthor { get; private set; }

        public string Rurl { get; private set; }
        public string PicUri { get; private set; }
        public string Message { get; protected set; }
        public string Rusername { get; private set; }

        public UserModel UserInfo { get; private set; }
        public UserAction UserAction { get; private set; }

        public ImmutableArray<ImageModel> PicArr { get; private set; } = ImmutableArray<ImageModel>.Empty;

        public SourceFeedReplyModel(JObject token) : base(token)
        {
            if (token.TryGetValue("id", out JToken id))
            {
                ID = id.ToObject<int>();
            }

            if (token.TryGetValue("userInfo", out JToken v1))
            {
                JObject userInfo = (JObject)v1;
                UserInfo = new UserModel(userInfo);
            }
            else
            {
                UserInfo = new UserModel(null);
            }

            if (token.TryGetValue("userAction", out JToken v2))
            {
                JObject userAction = (JObject)v1;
                UserAction = new UserAction(userAction);
            }
            else
            {
                UserAction = new UserAction(null);
            }

            if (token.TryGetValue("isFeedAuthor", out JToken isFeedAuthor))
            {
                IsFeedAuthor = isFeedAuthor.ToObject<int>() == 1;
            }

            if (token.TryGetValue("ruid", out JToken ruid))
            {
                Rurl = $"/u/{ruid}";
            }

            if (token.TryGetValue("rusername", out JToken rusername))
            {
                Rusername = rusername.ToString();
            }

            Windows.ApplicationModel.Resources.ResourceLoader loader = Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse("Feed");

            if (token.TryGetValue("message", out JToken message))
            {
                Message =
                string.IsNullOrEmpty(Rusername)
                ? $"{GetUserLink(UserInfo.Url, UserInfo.UserName) + GetAuthorString(IsFeedAuthor)}: {message}"
                : $"{GetUserLink(UserInfo.Url, UserInfo.UserName) + GetAuthorString(IsFeedAuthor)}@{GetUserLink(Rurl, Rusername)}: {message}";
            }

            if (token.TryGetValue("pic", out JToken pic) && !string.IsNullOrEmpty(pic.ToString()))
            {
                PicUri = pic.ToString();
                Message += $" <a href=\"{PicUri}\">{loader.GetString("SeePic")}</a>";
            }

            if (token.TryGetValue("picArr", out JToken picArr) && (picArr as JArray).Count > 0 && !string.IsNullOrEmpty((picArr as JArray)[0].ToString()))
            {
                PicArr = picArr.Select(
                    x => !string.IsNullOrEmpty(x.ToString())
                        ? new ImageModel(x.ToString(), ImageType.SmallImage) : null)
                    .Where(x => x != null)
                    .ToImmutableArray();

                foreach (ImageModel item in PicArr)
                {
                    item.ContextArray = PicArr;
                }
            }

            if (token.TryGetValue("block_status", out JToken block_status))
            {
                BlockStatus = block_status.ToObject<int>();
            }
        }

        private static string GetAuthorString(bool isFeedAuthor) => isFeedAuthor ? TextBlockEx.AuthorBorder : string.Empty;

        private static string GetUserLink(string url, string name) => $"<a href=\"{url}\" type=\"user-detail\">{name}</a>";

        public override string ToString() => Message;
    }
}
