using Newtonsoft.Json.Linq;

namespace CoolapkUWP.Models
{
    internal interface IFeedModel
    {
        double Id { get; }

        void Initial(JObject o);
    }
}