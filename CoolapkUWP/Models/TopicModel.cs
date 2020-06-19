using CoolapkUWP.Helpers;
using Newtonsoft.Json.Linq;
using Windows.UI.Xaml.Media;

namespace CoolapkUWP.Models
{
    internal class TopicModel : DyhModel
    {
        public TopicModel(JObject token) : base(token) => Commentnum = token["commentnum"].ToString().Replace("\"", string.Empty);

        public string Commentnum { get; private set; }
    }

    internal class DyhModel : Entity
    {
        public DyhModel(JObject token) : base(token)
        {
            Url = token.Value<string>("url");
            Title = token.Value<string>("title");
            Follownum = token["follownum"].ToString().Replace("\"", string.Empty);
            Logo = new ImageModel(token.Value<string>("logo"), ImageType.Icon);
        }

        public string Url { get; private set; }
        public string Title { get; private set; }
        public string Follownum { get; private set; }
        public ImageModel Logo { get; private set; }
    }
}