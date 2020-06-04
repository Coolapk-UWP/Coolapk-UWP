using Newtonsoft.Json.Linq;
using System.ComponentModel;

namespace CoolapkUWP.Controls.ViewModels
{
    class Entity : INotifyPropertyChanged
    {
        public Entity(JObject token)
        {
            if (token.TryGetValue("entityId", out JToken value1)) EntityId = value1.ToString().Replace("\"", string.Empty);
            if (token.TryGetValue("entityType", out JToken value2)) EntityType = value2.ToString();
            if (token.TryGetValue("entityFixed", out JToken value) && int.Parse(value.ToString()) == 1) EntityFixed = true;
            //EntityJsonObject = token;
        }
        public string EntityId { get; private set; }
        public bool EntityFixed { get; private set; }
        public string EntityType { get; private set; }
        //public JObject EntityJsonObject { get; private set; }
        public event PropertyChangedEventHandler PropertyChanged;

        internal void Changed(object s, string n) => PropertyChanged?.Invoke(s, new PropertyChangedEventArgs(n));
    }
}
