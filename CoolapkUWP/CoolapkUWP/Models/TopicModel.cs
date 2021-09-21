using Newtonsoft.Json.Linq;

namespace CoolapkUWP.Models
{
    internal class TopicModel : DyhModel
    {
        public string Commentnum { get; private set; }

        public TopicModel(JObject o) : base(o)
        {
            if (!string.IsNullOrEmpty((string)o["commentnum"]))
                Commentnum = o["commentnum"].ToString().Replace("\"", string.Empty, System.StringComparison.Ordinal);
            else if (!string.IsNullOrEmpty((string)o["rating_total_num"]))
                Commentnum = o["rating_total_num"].ToString().Replace("\"", string.Empty, System.StringComparison.Ordinal);
        }
    }
}