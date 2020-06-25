using CoolapkUWP.Helpers;
using System;
using System.ComponentModel;
using Windows.UI.Xaml.Media.Imaging;

namespace CoolapkUWP.Models
{
    internal class ImageModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ImageModel(string uri, ImageType type)
        {
            Uri = uri;
            _type = type;
        }

        private readonly ImageType _type;
        private WeakReference<BitmapImage> pic;
        private bool isLongPic;

        public BitmapImage Pic
        {
            get
            {
                if (pic != null && pic.TryGetTarget(out BitmapImage image))
                {
                    return image;
                }
                else
                {
                    GetImage();
                    return ImageCacheHelper.NoPic;
                }
            }
            private set
            {
                if(pic == null)
                {
                    pic = new WeakReference<BitmapImage>(value);
                }
                else
                {
                    pic.SetTarget(value);
                }
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
        
        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null)
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private async void GetImage()
        {
            BitmapImage bitmapImage = await ImageCacheHelper.GetImageAsync(_type, Uri);
            Pic = bitmapImage;
            IsLongPic = bitmapImage.PixelHeight > bitmapImage.PixelWidth * 2;
        }
    }
}