using CoolapkUWP.Helpers;
using System.ComponentModel;
using Windows.UI.Xaml.Media.Imaging;

namespace CoolapkUWP.Models
{
    internal class ImageModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null)
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public ImageModel(string uri, ImageType type)
        {
            Uri = uri;
            _type = type;
        }

        private readonly ImageType _type;
        private BitmapImage pic;
        private bool isLongPic;

        public BitmapImage Pic
        {
            get
            {
                if (pic == null)
                {
                    GetImage();
                    return ImageCacheHelper.NoPic;
                }
                else
                {
                    return pic;
                }
            }

            private set
            {
                pic = value;
                RaisePropertyChangedEvent();
            }
        }

        public bool IsLongPic
        {
            get => isLongPic;
            private set
            {
                isLongPic = value;
                RaisePropertyChangedEvent();
            }
        }

        public bool IsGif { get => Uri.Substring(Uri.LastIndexOf('.')).ToLower().Contains("gif"); }

        public string Uri { get; }

        private async void GetImage()
        {
            Pic = await ImageCacheHelper.GetImageAsync(_type, Uri);
            IsLongPic = Pic.PixelHeight > Pic.PixelWidth * 2;
        }
    }
}