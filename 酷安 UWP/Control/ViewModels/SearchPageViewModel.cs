using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace CoolapkUWP.Control.ViewModels
{
    class AppViewModel
    {
        public ImageSource Icon { get; set; }
        public string Url { get; set; }
        public string AppName { get; set; }
        public string Size { get; set; }
        public string DownloadNum { get; set; }
    }
    class SearchWord
    {
        public Symbol Symbol { get; set; }
        public string Title { get; set; }
    }
}
