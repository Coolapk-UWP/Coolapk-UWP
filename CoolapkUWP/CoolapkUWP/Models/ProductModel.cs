using CoolapkUWP.Core.Models;
using CoolapkUWP.Helpers;
using Newtonsoft.Json.Linq;

namespace CoolapkUWP.Models
{
    internal class ProductModel : Entity
    {
        public string Url { get; private set; }
        public string Title { get; private set; }
        public string Follownum { get; private set; }
        public string Description { get; private set; }
        public string Commentnum { get; private set; }
        public string LastUpdate { get; private set; }
        public ImageModel Logo { get; private set; }

        public ProductModel(JObject o) : base(o)
        {
            Url = o.Value<string>("url");
            Title = o.Value<string>("title");
            Follownum = o["follow_num"].ToString().Replace("\"", string.Empty, System.StringComparison.Ordinal);
            Logo = new ImageModel(o.Value<string>("logo"), ImageType.Icon);
            if (!string.IsNullOrEmpty((string)o["feed_comment_num"]))
            {
                Commentnum = o["feed_comment_num"].ToString().Replace("\"", string.Empty, System.StringComparison.Ordinal);
            }
            if (o.TryGetValue("description", out JToken description) && !string.IsNullOrEmpty(description.ToString()))
            {
                Description = description.ToString();
            }
            else if (o.TryGetValue("release_time", out JToken release_time) && !string.IsNullOrEmpty(release_time.ToString()))
            {
                Description = "发布日期：" + release_time.ToString();
            }
            else if (o.TryGetValue("link_tag", out JToken link_tag) && !string.IsNullOrEmpty(link_tag.ToString()))
            {
                Description = link_tag.ToString();
            }
            else if (o.TryGetValue("hot_num_txt", out JToken hot_num_txt) && !string.IsNullOrEmpty(hot_num_txt.ToString()))
            {
                Description = hot_num_txt.ToString() + "热度";
            }
            if (o.TryGetValue("update_time", out JToken update_time) && !string.IsNullOrEmpty(update_time.ToString()))
            {
                LastUpdate = DataHelper.ConvertUnixTimeStampToReadable(o.Value<int>("update_time"));
            }
        }
    }
}