using Windows.Data.Json;

namespace CoolapkUWP.Control.ViewModels
{
    class Entity
    {
        public Entity(IJsonValue t)
        {
            JsonObject token = t.GetObject();
            if (token.TryGetValue("entityId", out IJsonValue value1)) entityId = value1.ToString().Replace("\"", string.Empty);
            if (token.TryGetValue("entityType", out IJsonValue value2)) entityType = value2.GetString();
            if (token.TryGetValue("entityFixed", out IJsonValue value) && value.GetNumber() == 1) entityFixed = true;
        }
        public string entityId { get; private set; }
        public bool entityFixed { get; private set; }
        public string entityType { get; private set; }
    }

}
