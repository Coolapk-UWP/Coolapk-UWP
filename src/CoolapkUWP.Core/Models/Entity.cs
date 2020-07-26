using Newtonsoft.Json.Linq;

namespace CoolapkUWP.Core.Models
{
    public class Entity
    {
        public Entity(JObject o)
        {
            if (o == null)
            {
                throw new System.ArgumentNullException(nameof(o));
            }

            if (o.TryGetValue("entityId", out JToken v1))
            {
                EntityId = v1.ToString().Replace("\"", string.Empty);
            }

            if (o.TryGetValue("entityType", out JToken v2))
            {
                EntityType = v2.ToString();
            }

            if (o.TryGetValue("entityFixed", out JToken v3) && v3.ToObject<int>() == 1)
            {
                EntityFixed = true;
            }
        }

        public string EntityId { get; private set; }
        public string EntityType { get; private set; }
        public bool EntityFixed { get; set; }
    }
}