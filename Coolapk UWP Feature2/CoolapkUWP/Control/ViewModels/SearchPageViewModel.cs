using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace CoolapkUWP.Control.ViewModels
{
    internal interface ISearchPageViewModel
    {
        string GetTitle();
    }

    internal class AppViewModel : ISearchPageViewModel
    {
        public ImageSource Icon { get; set; }
        public string Url { get; set; }
        public string AppName { get; set; }
        public string Size { get; set; }
        public string DownloadNum { get; set; }

        public string GetTitle() => AppName;
    }

    internal class SearchWord : ISearchPageViewModel
    {
        public Symbol Symbol { get; set; }
        public string Title { get; set; }

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
