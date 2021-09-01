using System.ComponentModel;
using Windows.Data.Json;

namespace CoolapkUWP.Control.ViewModels
{
    internal class Entity : INotifyPropertyChanged
    {
        public string EntityId { get; private set; }
        public bool EntityFixed { get; private set; }
        public string EntityType { get; private set; }

        public Entity(IJsonValue t)
        {
            JsonObject token = t.GetObject();
            if (token.TryGetValue("entityId", out IJsonValue entityId))
            {
                EntityId = entityId.ToString().Replace("\"", string.Empty);
            }
            if (token.TryGetValue("entityType", out IJsonValue entityType))
            {
                EntityType = entityType.GetString();
            }
            if (token.TryGetValue("entityFixed", out IJsonValue entityFixed) && entityFixed.GetNumber() == 1)
            {
                EntityFixed = true;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        internal void Changed(object s, string n)
        {
            PropertyChanged?.Invoke(s, new PropertyChangedEventArgs(n));
        }
    }
}
