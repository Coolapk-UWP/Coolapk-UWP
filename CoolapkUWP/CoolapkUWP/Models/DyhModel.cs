using CoolapkUWP.Core.Models;
using CoolapkUWP.Helpers;
using Newtonsoft.Json.Linq;
using CoolapkUWP.Core.Helpers;

namespace CoolapkUWP.Models
{
    internal class DyhModel : Entity
    {
        public string Url { get; private set; }
        public string Title { get; private set; }
        public string Follownum { get; private set; }
        public string Description { get; private set; }
        public string Commentnum { get; private set; }
        public string LastUpdate { get; private set; }
        public ImageModel Logo { get; private set; }

        public DyhModel(JObject o) : base(o)
        {
            Url = o.Value<string>("url");
            Title = o.Value<string>("title");
            if (!string.IsNullOrEmpty((string)o["follownum"]))
                Follownum = o["follownum"].ToString().Replace("\"", string.Empty, System.StringComparison.Ordinal);
            else if (!string.IsNullOrEmpty((string)o["follow_num"]))
                Follownum = o["follow_num"].ToString().Replace("\"", string.Empty, System.StringComparison.Ordinal);
            Logo = new ImageModel(o.Value<string>("logo"), ImageType.Icon);
            if (!string.IsNullOrEmpty((string)o["newsnum"]))
            {
                Commentnum = o["newsnum"].ToString().Replace("\"", string.Empty, System.StringComparison.Ordinal);
            }
            else if (!string.IsNullOrEmpty((string)o["commentnum"]))
            {
                Commentnum = o["commentnum"].ToString().Replace("\"", string.Empty, System.StringComparison.Ordinal);
            }
            else if (!string.IsNullOrEmpty((string)o["rating_total_num"]))
            {
                Commentnum = o["rating_total_num"].ToString().Replace("\"", string.Empty, System.StringComparison.Ordinal);
            }
            if (o.TryGetValue("description", out JToken description) && !string.IsNullOrEmpty(description.ToString()))
            {
                Description = description.ToString();
            }
            else if (o.TryGetValue("newtitle", out JToken newtitle) && !string.IsNullOrEmpty(newtitle.ToString()))
            {
                Description = newtitle.ToString();
            }
            else if (o.TryGetValue("username", out JToken username) && !string.IsNullOrEmpty(username.ToString()))
            {
                Description = "作者" + username.ToString();
            }
            else if (o.TryGetValue("rss_type", out JToken rss_type) && !string.IsNullOrEmpty(rss_type.ToString()))
            {
                Description = rss_type.ToString();
            }
            else if (o.TryGetValue("hot_num", out JToken hot_num) && !string.IsNullOrEmpty(hot_num.ToString()))
            {
                Description = Utils.GetNumString(double.Parse(hot_num.ToString())) + "热度";
            }
            if (o.TryGetValue("lastupdate", out JToken lastupdate) && !string.IsNullOrEmpty(lastupdate.ToString()))
            {
                LastUpdate = DataHelper.ConvertUnixTimeStampToReadable(o.Value<int>("lastupdate"));
            }
        }
    }
}