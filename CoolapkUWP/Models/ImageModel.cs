using CoolapkUWP.Helpers;
using System;
using System.Collections.Immutable;
using System.ComponentModel;
using Windows.UI.Xaml.Media.Imaging;

namespace CoolapkUWP.Models
{
    public class ImageModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ImageModel(string uri, ImageType type)
        {
            Uri = uri;
            Type = type;
            SettingsHelper.BackgroundChanged.Add(isDarkMode =>
                {
                    _ = UIHelper.ShellDispatcher?.RunAsync(
                        Windows.UI.Core.CoreDispatcherPriority.Normal,
                        () =>
                        {
                            if (pic == null)
                            {
                                GetImage();
                            }
                            else
                            {
                                if (pic.TryGetTarget(out BitmapImage image) && image.UriSource.Scheme == "ms-appx")
                                {
                                    Pic = ImageCacheHelper.NoPic;
                                }
                            }
                        });
                });

            SettingsHelper.NoPicModeChanged.Add(_ => GetImage());
        }

        private WeakReference<BitmapImage> pic;
        private bool isLongPic;
        private ImmutableArray<ImageModel> contextArray = default;

        public ImmutableArray<ImageModel> ContextArray
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
                if (pic == null)
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

        public bool IsGif { get => Uri.Substring(Uri.LastIndexOf('.')).ToLower().Contains("gif", StringComparison.Ordinal); }

        public string Uri { get; }

        public ImageType Type { get; }

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null)
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private async void GetImage()
        {
            var bitmapImage = await ImageCacheHelper.GetImageAsync(Type, Uri);
            Pic = bitmapImage;
            IsLongPic = bitmapImage.PixelHeight > bitmapImage.PixelWidth * 2;
        }
    }
}