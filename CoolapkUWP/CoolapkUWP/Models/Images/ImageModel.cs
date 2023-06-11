using CoolapkUWP.Common;
using CoolapkUWP.Helpers;
using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;

namespace CoolapkUWP.Models.Images
{
    public class ImageModel : INotifyPropertyChanged, IPic
    {
        private static SemaphoreSlim semaphoreSlim = new SemaphoreSlim(SettingsHelper.Get<int>(SettingsHelper.SemaphoreSlimCount));

        public CoreDispatcher Dispatcher { get; }

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

        private bool isLongPic = false;
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

        private bool isWidePic = false;
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

        protected ImmutableArray<ImageModel> contextArray = ImmutableArray<ImageModel>.Empty;
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

        private bool isLoading = true;
        public bool IsLoading
        {
            get => isLoading;
            private set
            {
                if (isLoading != value)
                {
                    isLoading = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        public ImageModel(string uri, ImageType type) : this(uri, type, Window.Current?.Dispatcher ?? CoreApplication.MainView.Dispatcher)
        {
        }

        public ImageModel(string uri, ImageType type, CoreDispatcher dispatcher)
        {
            Dispatcher = dispatcher;
            Uri = uri;
            Type = type;
            ThemeHelper.UISettingChanged.Add(mode =>
            {
                switch (mode)
                {
                    case UISettingChangedType.LightMode:
                    case UISettingChangedType.DarkMode:
                        if (SettingsHelper.Get<bool>(SettingsHelper.IsNoPicsMode))
                        {
                            if (pic != null && pic.TryGetTarget(out BitmapImage _))
                            {
                                Pic = ImageCacheHelper.NoPic;
                            }
                        }
                        break;

                    case UISettingChangedType.NoPicChanged:
                        if (pic != null && pic.TryGetTarget(out BitmapImage _))
                        {
                            _ = GetImage();
                        }
                        break;
                }
            });
        }

        public event TypedEventHandler<ImageModel, object> LoadStarted;
        public event TypedEventHandler<ImageModel, object> LoadCompleted;

        public event PropertyChangedEventHandler PropertyChanged;

        private async void RaisePropertyChangedEvent([CallerMemberName] string name = null)
        {
            if (name != null)
            {
                if (Dispatcher?.HasThreadAccess == false)
                {
                    await Dispatcher.ResumeForegroundAsync();
                }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }

        public static void SetSemaphoreSlim(int initialCount)
        {
            semaphoreSlim.Dispose();
            semaphoreSlim = new SemaphoreSlim(initialCount);
        }

        private async Task GetImage()
        {
            await ThreadSwitcher.ResumeBackgroundAsync();
            try
            {
                IsLoading = true;
                LoadStarted?.Invoke(this, null);
                await semaphoreSlim.WaitAsync();
                if (SettingsHelper.Get<bool>(SettingsHelper.IsNoPicsMode)) { Pic = await ImageCacheHelper.GetNoPicAsync(Dispatcher); }
                BitmapImage bitmapImage = await ImageCacheHelper.GetImageAsync(Type, Uri, Dispatcher);
                Pic = bitmapImage;
                await bitmapImage.Dispatcher.ResumeForegroundAsync();
                double PixelWidth = bitmapImage.PixelWidth;
                double PixelHeight = bitmapImage.PixelHeight;
                Rect Bounds = Window.Current != null
                    ? await Window.Current.Dispatcher.AwaitableRunAsync(() => Window.Current.Bounds)
                    : await CoreApplication.MainView.Dispatcher.AwaitableRunAsync(() => Window.Current.Bounds);
                IsLongPic = PixelHeight * Bounds.Width > PixelWidth * Bounds.Height * 1.5
                            && PixelHeight > PixelWidth * 1.5;
                IsWidePic = PixelWidth * Bounds.Height > PixelHeight * Bounds.Width * 1.5
                            && PixelWidth > PixelHeight * 1.5;
            }
            finally
            {
                LoadCompleted?.Invoke(this, null);
                semaphoreSlim.Release();
                IsLoading = false;
            }
        }

        public async Task Refresh() => await GetImage();

        public override string ToString() => Uri;
    }
}
