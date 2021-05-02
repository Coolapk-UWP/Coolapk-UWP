using Windows.UI.Xaml.Media.Imaging;

namespace CoolapkUWP.Control.ViewModels
{
    class ImageData
    {
        public string url;
        public BitmapImage Pic { get; set; }
        public bool IsLongPic { get => Pic.PixelHeight > Pic.PixelWidth * 2; }
        public bool IsGif { get => url.Substring(url.LastIndexOf('.')).ToLower().Contains("gif"); }
    }
}
