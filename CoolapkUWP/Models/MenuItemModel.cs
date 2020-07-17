using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Linq;

namespace CoolapkUWP.Models
{
    public class MenuItem
    {
        [DebuggerStepThrough]
        private MenuItem(JToken t, bool isInGroup)
        {
            if (t == null) { throw new System.ArgumentNullException(nameof(t)); }
            Title = t.Value<string>("title");
            Uri = t.Value<string>("url");
            IsInGroup = isInGroup;
        }

        [DebuggerStepThrough]
        public MenuItem(JToken t) : this(t, false)
        {
            var a = (JArray)t["entities"];
            if (a != null && a.Count > 0)
            {
                Entities = from ta in a
                           where ta.Value<string>("entityType") == "page"
                           select new MenuItem(ta, true);
            }
        }

        public bool IsInGroup { get; private set; }
        public string Title { get; private set; }
        public string Uri { get; private set; }
        public System.Collections.Generic.IEnumerable<MenuItem> Entities { get; private set; }
    }
}