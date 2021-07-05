using System.Collections.Immutable;
using Windows.UI.Xaml.Media.Imaging;

namespace CoolapkUWP.Control.ViewModels
{
    internal class ImageData
    {
        public string url;
        public BitmapImage Pic { get; set; }
        private ImmutableArray<ImageData> contextArray;
        public bool IsLongPic { get => Pic.PixelHeight > Pic.PixelWidth * 2; }
        public bool IsGif { get => url.Substring(url.LastIndexOf('.')).ToLower().Contains("gif"); }

        public ImmutableArray<ImageData> ContextArray
        {
            get => contextArray;
            set
            {
                if (contextArray.IsDefaultOrEmpty)
                {
                    contextArray = value;
                }
            }
        }
    }
}
