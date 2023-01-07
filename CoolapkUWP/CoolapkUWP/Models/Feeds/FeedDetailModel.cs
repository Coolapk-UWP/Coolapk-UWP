using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Images;
using Newtonsoft.Json.Linq;
using System.Text;

namespace CoolapkUWP.Models.Feeds
{
    public class FeedDetailModel : FeedModelBase
    {
        public string Title { get; private set; }
        public bool IsAnswerFeed { get; private set; }
        public bool IsFeedArticle { get; private set; }
        public string QuestionUrl { get; private set; }
        public string MessageRawOutput { get; private set; }
        public ImageModel MessageCover { get; private set; }

        public FeedDetailModel(JObject token) : base(token)
        {
            if (token.TryGetValue("title", out JToken title))
            {
                Title = title.ToString();
            }

            if (EntityType != "article")
            {
                switch (FeedType)
                {
                    case "answer":
                        IsAnswerFeed = true;
                        if (token.TryGetValue("extraData", out JToken extraData))
                        {
                            JObject j = JObject.Parse(extraData.ToString());
                            if (j.TryGetValue("questionUrl", out JToken questionUrl))
                            {
                                QuestionUrl = questionUrl.ToString();
                            }
                        }

                        MessageRawOutput = string.Empty;
                        StringBuilder builder = new StringBuilder();
                        if (token.TryGetValue("message_raw_output", out JToken message_raw_output))
                        {
                            foreach (JObject item in JArray.Parse(message_raw_output.ToString()))
                            {
                                if (item.TryGetValue("type", out JToken type))
                                {
                                    switch (type.ToString())
                                    {
                                        case "text":
                                            if (item.TryGetValue("message", out JToken message))
                                            {
                                                builder.Append(message.ToString());
                                            }
                                            break;

                                        case "image":
                                            if (item.TryGetValue("uri", out JToken uri))
                                            {
                                                item.TryGetValue("description", out JToken description);
                                                builder.Append($"\n<img src=\"{uri}\" alt=\"{description}\">{description}</a>\n");
                                            }
                                            break;
                                    }
                                }
                            }
                        }
                        MessageRawOutput = builder.ToString();
                        break;

                    case "feedArticle":
                        IsFeedArticle = true;
                        if (token.TryGetValue("message_cover", out JToken message_cover) && !string.IsNullOrEmpty(message_cover.ToString()))
                        {
                            MessageCover = new ImageModel(message_cover.ToString(), ImageType.SmallImage);
                        }

                        MessageRawOutput = string.Empty;
                        builder = new StringBuilder();
                        if (token.TryGetValue("message_raw_output", out message_raw_output))
                        {
                            foreach (JObject item in JArray.Parse(message_raw_output.ToString()))
                            {
                                if (item.TryGetValue("type", out JToken type))
                                {
                                    switch (type.ToString())
                                    {
                                        case "text":
                                            if (item.TryGetValue("message", out JToken message))
                                            {
                                                builder.Append(message.ToString());
                                            }
                                            break;

                                        case "image":
                                            if (item.TryGetValue("url", out JToken uri))
                                            {
                                                item.TryGetValue("description", out JToken description);
                                                builder.Append($"\n<img src=\"{uri}\" alt=\"{description}\"/>\n");
                                            }
                                            break;
                                    }
                                }
                            }
                        }
                        MessageRawOutput = builder.ToString();
                        break;
                }
            }
        }
    }
}
