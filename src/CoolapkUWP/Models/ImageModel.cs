using CoolapkUWP.Helpers;
using System;
using System.Collections.Immutable;
using System.ComponentModel;
using Windows.UI.Xaml.Media.Imaging;

namespace CoolapkUWP.Models
{
    public class ImageModel : INotifyPropertyChanged
    {
        private WeakReference<BitmapImage> pic;
        private bool isLongPic;
        private ImmutableArray<ImageModel> contextArray;

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

        public bool IsGif { get => Uri.Substring(Uri.LastIndexOf('.')).ToUpperInvariant().Contains("GIF", StringComparison.Ordinal); }

        public string Uri { get; }

        public ImageType Type { get; }

        public ImageModel(string uri, ImageType type)
        {
            Uri = uri;
            Type = type;
            SettingsHelper.UiSettingChanged.Add(mode =>
                {
                    switch (mode)
                    {
                        case UiSettingChangedType.LightMode:
                        case UiSettingChangedType.DarkMode:
                            _ = UIHelper.ShellDispatcher?.RunAsync(
                                Windows.UI.Core.CoreDispatcherPriority.Normal,
                                () =>
                                {
                                    if (pic == null)
                                    {
                                        GetImage();
                                    }
                                    else if (pic.TryGetTarget(out BitmapImage image) && image.UriSource != null)
                                    {
                                        Pic = ImageCacheHelper.NoPic;
                                    }
                                });

                            break;

                        case UiSettingChangedType.NoPicChanged:
                            GetImage();
                            break;
                    }
                });
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        private async void GetImage()
        {
            if (SettingsHelper.Get<bool>(SettingsHelper.IsNoPicsMode)) { Pic = ImageCacheHelper.NoPic; }
            BitmapImage bitmapImage = await ImageCacheHelper.GetImageAsync(Type, Uri);
            if (SettingsHelper.Get<bool>(SettingsHelper.IsNoPicsMode)) { return; }
            Pic = bitmapImage;
            IsLongPic = bitmapImage.PixelHeight > bitmapImage.PixelWidth * 2;
        }
    }
}