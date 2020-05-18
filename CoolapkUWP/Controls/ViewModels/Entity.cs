using Newtonsoft.Json.Linq;
using System.ComponentModel;

namespace CoolapkUWP.Controls.ViewModels
{
    class Entity : INotifyPropertyChanged
    {
        public Entity(JToken t)
        {
            JObject token = t as JObject;
            if (token.TryGetValue("entityId", out JToken value1)) entityId = value1.ToString().Replace("\"", string.Empty);
            if (token.TryGetValue("entityType", out JToken value2)) entityType = value2.ToString();
            if (token.TryGetValue("entityFixed", out JToken value) && int.Parse(value.ToString()) == 1) entityFixed = true;
        }
        public string entityId { get; private set; }
        public bool entityFixed { get; private set; }
        public string entityType { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        internal void Changed(object s, string n) => PropertyChanged?.Invoke(s, new PropertyChangedEventArgs(n));
    }
}
