using Newtonsoft.Json.Linq;
using Windows.UI.Xaml.Controls;

namespace CoolapkUWP.Models
{
    internal class SearchWord
    {
        public Symbol Symbol { get; set; } = Symbol.Find;
        public string Title { get; set; }

        public SearchWord(JObject keys)
        {
            if (keys.Value<string>("logo").Contains("cube")) Symbol = Symbol.Shop;
            else if (keys.Value<string>("logo").Contains("xitongguanli")) Symbol = Symbol.AllApps;
            Title = keys.Value<string>("title");
        }

        public string GetTitle()
        {
            switch (Symbol)
            {
                case Symbol.Shop:
                    return Title.Substring(5);

                case Symbol.Contact:
                    return Title.Substring(5);

                default:
                    return Title;
            }
        }
    }
}