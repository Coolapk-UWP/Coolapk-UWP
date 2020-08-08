using Newtonsoft.Json.Linq;

namespace CoolapkUWP.Models
{
    internal class TopicModel : DyhModel
    {
        public string Commentnum { get; private set; }

        public TopicModel(JObject o) : base(o)
        {
            Commentnum = o["commentnum"].ToString().Replace("\"", string.Empty, System.StringComparison.Ordinal);
        }
    }
}