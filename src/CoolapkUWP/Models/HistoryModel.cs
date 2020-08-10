using CoolapkUWP.Core.Models;
using CoolapkUWP.Helpers;
using Newtonsoft.Json.Linq;

namespace CoolapkUWP.Models
{
    public class HistoryModel : Entity
    {
        public string Title { get; private set; }
        public string Url { get; private set; }
        public string Description { get; private set; }
        public ImageModel Pic { get; private set; }
        public string Id { get; private set; }

        public HistoryModel(JObject o) : base(o)
        {
            if (o.TryGetValue("id", out JToken v1) && !string.IsNullOrEmpty(v1.ToString()))
            {
                Id = v1.ToString();
            }
            if (o.TryGetValue("title", out JToken v2) && !string.IsNullOrEmpty(v2.ToString()))
            {
                Title = v2.ToString();
            }
            if (o.TryGetValue("url", out JToken v3) && !string.IsNullOrEmpty(v3.ToString()))
            {
                Url = v3.ToString();
            }
            if (o.TryGetValue("description", out JToken v4) && !string.IsNullOrEmpty(v4.ToString()))
            {
                Description = v4.ToString();
            }
            else if (o.TryGetValue("target_type_title", out JToken v7) && !string.IsNullOrEmpty(v7.ToString()))
            {
                Description = v7.ToString();
            }
            if (o.TryGetValue("logo", out JToken v6) && !string.IsNullOrEmpty(v6.ToString()))
            {
                Pic = new ImageModel(v6.ToString(), ImageType.Icon);
            }
        }
    }
}