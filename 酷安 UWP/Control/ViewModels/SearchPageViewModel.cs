using Windows.Data.Json;
using Windows.UI.Xaml.Controls;

namespace CoolapkUWP.Control.ViewModels
{
    class SearchWord
    {
        public Symbol Symbol { get; set; } = Symbol.Find;
        public string Title { get; set; }

        public SearchWord(JsonObject keys)
        {
            if (keys["logo"].GetString().Contains("cube")) Symbol = Symbol.Shop;
            else if (keys["logo"].GetString().Contains("xitongguanli")) Symbol = Symbol.AllApps;
            Title = keys["title"].GetString();        
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
