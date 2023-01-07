using CoolapkUWP.Helpers;
using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;

namespace CoolapkUWP.Models.Images
{
    public class ImageModel : INotifyPropertyChanged, IPic
    {
        protected WeakReference<BitmapImage> pic;
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
                    _ = GetImage();
                    return ImageCacheHelper.NoPic;
                }
            }
            protected set
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

        private bool isLongPic;
        public bool IsLongPic
        {
            get => isLongPic;
            private set
            {
                if (isLongPic != value)
                {
                    isLongPic = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private bool isWidePic;
        public bool IsWidePic
        {
            get => isWidePic;
            private set
            {
                if (isWidePic != value)
                {
                    isWidePic = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        protected ImmutableArray<ImageModel> contextArray;
        public ImmutableArray<ImageModel> ContextArray
        {
            get => contextArray;
            set
            {
                if (contextArray.IsDefaultOrEmpty)
                {
                    contextArray = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        public bool IsGif => Uri.Substring(Uri.LastIndexOf('.')).ToUpperInvariant().Contains("GIF");

        private string uri;
        public string Uri
        {
            get => uri;
            set
            {
                if (uri != value)
                {
                    uri = value;
                    if (pic != null && pic.TryGetTarget(out BitmapImage _))
                    {
                        _ = GetImage();
                    }
                }
            }
        }

        private ImageType type;
        public ImageType Type
        {
            get => type;
            set
            {
                if (type != value)
                {
                    type = value;
                    if (pic != null && pic.TryGetTarget(out BitmapImage _))
                    {
                        _ = GetImage();
                    }
                }
            }
        }

        public BitmapImage RealPic
        {
            get
            {
                if (pic != null && pic.TryGetTarget(out BitmapImage image))
                {
                    return image;
                }
                else
                {
                    GetImage().Wait();
                    return Pic;
                }
            }
        }

        public ImageModel(string uri, ImageType type)
        {
            Uri = uri;
            Type = type;
            ThemeHelper.UISettingChanged.Add(mode =>
            {
                switch (mode)
                {
                    case UISettingChangedType.LightMode:
                    case UISettingChangedType.DarkMode:
                        _ = DispatcherHelper.ExecuteOnUIThreadAsync(() =>
                            {
                                if (SettingsHelper.Get<bool>(SettingsHelper.IsNoPicsMode))
                                {
                                    if (pic != null && pic.TryGetTarget(out BitmapImage _))
                                    {
                                        Pic = ImageCacheHelper.NoPic;
                                    }
                                }
                            });
                        break;

                    case UISettingChangedType.NoPicChanged:
                        _ = DispatcherHelper.ExecuteOnUIThreadAsync(async () =>
                        {
                            if (pic != null && pic.TryGetTarget(out BitmapImage _))
                            {
                                await GetImage();
                            }
                        });
                        break;
                }
            });
        }

        public event TypedEventHandler<ImageModel, object> LoadStarted;
        public event TypedEventHandler<ImageModel, object> LoadCompleted;

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        private async Task GetImage()
        {
            LoadStarted?.Invoke(this, null);
            if (SettingsHelper.Get<bool>(SettingsHelper.IsNoPicsMode)) { Pic = ImageCacheHelper.NoPic; }
            BitmapImage bitmapImage = await ImageCacheHelper.GetImageAsync(Type, Uri);
            if (bitmapImage.Dispatcher.HasThreadAccess)
            {
                Pic = bitmapImage;
                IsLongPic =
                    ((bitmapImage.PixelHeight * Window.Current.Bounds.Width) > bitmapImage.PixelWidth * Window.Current.Bounds.Height * 1.5)
                    && bitmapImage.PixelHeight > bitmapImage.PixelWidth * 1.5;
                IsWidePic =
                    ((bitmapImage.PixelWidth * Window.Current.Bounds.Height) > bitmapImage.PixelHeight * Window.Current.Bounds.Width * 1.5)
                    && bitmapImage.PixelWidth > bitmapImage.PixelHeight * 1.5;
            }
            else
            {
                StorageFile file = await ImageCacheHelper.GetImageFileAsync(Type, Uri);
                using (IRandomAccessStreamWithContentType stream = await file.OpenReadAsync())
                {
                    BitmapImage image = new BitmapImage();
                    await image.SetSourceAsync(stream);
                    Pic = image;
                    IsLongPic =
                        ((image.PixelHeight * Window.Current.Bounds.Width) > image.PixelWidth * Window.Current.Bounds.Height * 1.5)
                        && image.PixelHeight > image.PixelWidth * 1.5;
                    IsWidePic =
                        ((image.PixelWidth * Window.Current.Bounds.Height) > image.PixelHeight * Window.Current.Bounds.Width * 1.5)
                        && image.PixelWidth > image.PixelHeight * 1.5;
                }
            }
            LoadCompleted?.Invoke(this, null);
        }

        public async Task Refresh() => await DispatcherHelper.ExecuteOnUIThreadAsync(GetImage);

        public override string ToString() => Uri;
    }
}
