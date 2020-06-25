using Newtonsoft.Json.Linq;

namespace CoolapkUWP.Models
{
    public class Entity
    {
        public Entity(JObject o)
        {
            if (o.TryGetValue("entityId", out JToken value1)) EntityId = value1.ToString().Replace("\"", string.Empty);
            if (o.TryGetValue("entityType", out JToken value2)) EntityType = value2.ToString();
            if (o.TryGetValue("entityFixed", out JToken value) && int.Parse(value.ToString()) == 1) EntityFixed = true;
            //EntityJsonObject = token;
        }

        public string EntityId { get; private set; }
        public bool EntityFixed { get; internal set; }
        public string EntityType { get; private set; }

        //public JObject EntityJsonObject { get; private set; }
    }
}