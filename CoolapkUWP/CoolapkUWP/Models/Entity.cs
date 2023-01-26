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
        public string EntityForward { get; private set; }

        public Entity(JObject token)
        {
            if (token == null) { return; }

            if (token.TryGetValue("entityId", out JToken entityId))
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

            if (token.TryGetValue("entityType", out JToken entityType))
            {
                EntityType = entityType.ToString();
            }

            if (token.TryGetValue("entityFixed", out JToken entityFixed))
            {
                EntityFixed = Convert.ToBoolean(entityFixed.ToObject<int>());
            }

            if (token.TryGetValue("entityForward", out JToken entityForward))
            {
                EntityForward = entityForward.ToString();
            }
        }

        public override string ToString() => $"{EntityType} - {EntityID}";
    }

    public class NullEntity : Entity
    {
        public NullEntity(JObject token = null) : base(token) { }
    }
}
