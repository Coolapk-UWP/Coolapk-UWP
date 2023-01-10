using CoolapkUWP.Helpers;
using Newtonsoft.Json.Linq;
using System;

namespace CoolapkUWP.Models
{
    public class Entity
    {
        public bool EntityFixed { get; set; }
        public int EntityID { get; private set; }
        public string EntityIDText { get; private set; }
        public string EntityType { get; private set; }

        public Entity(JObject o)
        {
            if (o == null)
            {
                //throw new ArgumentNullException(nameof(o));
                return;
            }

            if (o.TryGetValue("entityId", out JToken entityId))
            {
                try
                {
                    EntityID = entityId.ToObject<int>();
                }
                catch (Exception ex)
                {
                    SettingsHelper.LogManager.GetLogger(nameof(Entity)).Warn(ex.ExceptionToMessage(), ex);
                    EntityIDText = entityId.ToString();
                }
            }

            if (o.TryGetValue("entityType", out JToken entityType))
            {
                EntityType = entityType.ToString();
            }

            if (o.TryGetValue("entityFixed", out JToken entityFixed))
            {
                EntityFixed = Convert.ToBoolean(entityFixed.ToObject<int>());
            }
        }

        public override string ToString()
        {
            return $"{EntityType} - {EntityID}";
        }
    }

    public class NullModel : Entity
    {
        public NullModel(JObject o = null) : base(o)
        {

        }
    }
}
