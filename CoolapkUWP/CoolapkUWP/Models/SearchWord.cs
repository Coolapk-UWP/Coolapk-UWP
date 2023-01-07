using Newtonsoft.Json.Linq;
using Windows.UI.Xaml.Controls;

namespace CoolapkUWP.Models
{
    internal class SearchWord
    {
        public Symbol Symbol { get; set; }
        public string Title { get; set; }

        public SearchWord(JObject keys)
        {
            Symbol = keys.Value<string>("logo").Contains("app")
                ? Symbol.Shop
                : keys.Value<string>("logo").Contains("cube")
                    ? Symbol.Shop
                    : keys.Value<string>("logo").Contains("xitongguanli")
                        ? Symbol.Contact
                        : Symbol.Find;
            Title = keys.Value<string>("title");
        }

        public override string ToString()
        {
            switch (Symbol)
            {
                case Symbol.Shop:
                case Symbol.Contact:
                    return Title.Substring(5);

                default:
                    return Title;
            }
        }
    }
}
