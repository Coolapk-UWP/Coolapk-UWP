using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Images;
using Newtonsoft.Json.Linq;

namespace CoolapkUWP.Models
{
    public class HistoryModel : Entity, IHasDescription
    {
        public string Url { get; private set; }
        public string Title { get; private set; }
        public ImageModel Pic { get; private set; }
        public string Description { get; private set; }

        public HistoryModel(JObject token) : base(token)
        {
            if (token.TryGetValue("title", out JToken title))
            {
                Title = title.ToString();
            }

            if (token.TryGetValue("url", out JToken url))
            {
                Url = url.ToString();
            }

            if (token.TryGetValue("description", out JToken description))
            {
                Description = description.ToString();
            }
            else if (token.TryGetValue("target_type_title", out JToken target_type_title) && !string.IsNullOrEmpty(target_type_title.ToString()))
            {
                Description = target_type_title.ToString();
            }
            else if (token.TryGetValue("dateline", out JToken dateline))
            {
                Description = dateline.ToObject<long>().ConvertUnixTimeStampToReadable();
            }

            if (token.TryGetValue("logo", out JToken logo))
            {
                Pic = new ImageModel(logo.ToString(), ImageType.Icon);
            }
        }

        public override string ToString() => $"{Title} - {Description}";
    }
}
