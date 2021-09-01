using System.Collections.Immutable;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;

namespace CoolapkUWP.Control.ViewModels
{
    internal class ImageData
    {
        public string url;
        public BitmapImage Pic { get; set; }
        private ImmutableArray<ImageData> contextArray;
        public bool IsLongPic => ((Pic.PixelHeight * Window.Current.Bounds.Width) > Pic.PixelWidth * Window.Current.Bounds.Height * 1.5) && Pic.PixelHeight > Pic.PixelWidth * 1.5;
        public bool IsWidePic => ((Pic.PixelWidth * Window.Current.Bounds.Height) > Pic.PixelHeight * Window.Current.Bounds.Width * 1.5) && Pic.PixelWidth > Pic.PixelHeight * 1.5;
        public bool IsGif => url.EndsWith(".gif") || url.EndsWith(".GIF");

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
