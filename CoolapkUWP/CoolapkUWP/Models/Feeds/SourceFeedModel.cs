using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Images;
using CoolapkUWP.Models.Users;
using Newtonsoft.Json.Linq;
using System.Collections.Immutable;
using System.Linq;

namespace CoolapkUWP.Models.Feeds
{
    public class SourceFeedModel : Entity
    {
        public string Url { get; private set; }
        public string Message { get; private set; }
        public ImageModel Pic { get; private set; }
        public string Dateline { get; private set; }
        public string ShareUrl { get; private set; }
        public UserModel UserInfo { get; private set; }
        public string MessageTitle { get; private set; }
        public UserAction UserAction { get; private set; }
        public string FeedType { get; private set; } = "feed";
        public ImmutableArray<ImageModel> PicArr { get; private set; } = ImmutableArray<ImageModel>.Empty;

        public SourceFeedModel(JObject token) : base(token)
        {
            if (token.TryGetValue("url", out JToken uri) && !string.IsNullOrEmpty(uri.ToString()))
            {
                Url = uri.ToString();
            }
            else if (token.TryGetValue("id", out JToken id))
            {
                Url = $"/feed/{id.ToString().Replace("\"", string.Empty)}";
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
                JObject userAction = (JObject)v2;
                UserAction = new UserAction(userAction);
            }
            else
            {
                UserAction = new UserAction(null);
            }

            if (token.TryGetValue("shareUrl", out JToken shareUrl) && !string.IsNullOrEmpty(shareUrl.ToString()))
            {
                ShareUrl = shareUrl.ToString();
            }
            else
            {
                ShareUrl = "https://www.coolapk.com" + Url != null ? Url.Replace("/question/", "/feed/") : string.Empty; ;
            }

            if (token.TryGetValue("message", out JToken message))
            {
                Message = message.ToString();
            }

            if (token.TryGetValue("message_title", out JToken message_title))
            {
                MessageTitle = message_title.ToString();
            }

            if (token.TryGetValue("feedType", out JToken feedType))
            {
                FeedType = feedType.ToString();
            }

            if (token.TryGetValue("dateline", out JToken dateline))
            {
                Dateline = dateline.ToObject<long>().ConvertUnixTimeStampToReadable();
            }

            if (token.TryGetValue("picArr", out JToken picArr) && (picArr as JArray).Count > 0)
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

            if (token.TryGetValue("pic", out JToken pic) && !string.IsNullOrEmpty(pic.ToString()))
            {
                Pic = new ImageModel(pic.ToString(), ImageType.SmallImage);
            }
        }

        public override string ToString() => Message;
    }
}
