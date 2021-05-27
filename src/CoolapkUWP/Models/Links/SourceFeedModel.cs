using CoolapkUWP.Core.Models;
using CoolapkUWP.Helpers;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Immutable;

namespace CoolapkUWP.Models.Links
{
    public enum LinkType
    {
        ITHome,
    }

    public class SourceFeedModel : Entity
    {
        public string Url { get; private set; }
        public string Message { get; private set; }
        public string MessageTitle { get; private set; }
        public bool ShowMessageTitle { get => !string.IsNullOrEmpty(MessageTitle); }
        public string Uurl { get; private set; }
        public string Username { get; private set; }
        public string Dateline { get; private set; }
        public bool ShowPicArr { get; private set; }
        public BackgroundImageModel Pic { get; private set; }
        public ImmutableArray<ImageModel> PicArr { get; private set; } = ImmutableArray<ImageModel>.Empty;
        public ImageModel UserSmallAvatar { get; private set; }

        public SourceFeedModel(JObject o, LinkType type) : base(o)
        {
            switch (type)
            {
                case LinkType.ITHome:
                    {
                        if (o.TryGetValue("data", out JToken v1))
                        {
                            JObject data = (JObject)v1;
                            if (data.TryGetValue("id", out JToken id))
                            {
                                Url = $"ithome://qcontent?id={id.ToString().Replace("\"", string.Empty, System.StringComparison.Ordinal)}";
                            }
                            if (data.TryGetValue("contents", out JToken contents))
                            {
                                foreach (JObject v in contents as JArray)
                                {
                                    if (v.TryGetValue("content", out JToken content))
                                    { Message += content.ToString() + "\n"; }
                                }
                                Message = Message.PadRight(1);
                                Message = Message.Remove(Message.Length - 1, 1);
                            }
                            if (data.TryGetValue("user", out JToken v2))
                            {
                                JObject user = (JObject)v2;
                                if (user.TryGetValue("userNick", out JToken userNick))
                                {
                                    Username = userNick.ToString();
                                }
                            }
                        }
                    }
                    break;
                default: break;
            }
        }
    }
}