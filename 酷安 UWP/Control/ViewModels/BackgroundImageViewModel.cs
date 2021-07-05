﻿using ColorThiefDotNet;
using CoolapkUWP.Data;
using System;
using System.ComponentModel;
using Windows.Graphics.Imaging;
using Windows.UI.Xaml.Media.Imaging;

namespace CoolapkUWP.Control.ViewModels
{
    internal class BackgroundImageViewModel : INotifyPropertyChanged
    {
        private WeakReference<BitmapImage> pic;
        private static readonly Windows.UI.Color fallbackColor = Windows.UI.Color.FromArgb(0x99, 0, 0, 0);
        private static readonly ColorThief thief = new ColorThief();
        private Windows.UI.Color backgroundColor = fallbackColor;

        public Windows.UI.Color BackgroundColor
        {
            get => backgroundColor;
            private set
            {
                backgroundColor = value;
                RaisePropertyChangedEvent();
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
                    return ImageCache.NoPic;
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
                    if (value.UriSource is null)
                    {
                        SetBrush();
                    }
                    else { BackgroundColor = fallbackColor; }
                }
                RaisePropertyChangedEvent();
            }
        }

        public string Uri { get; }

        public ImageType Type { get; }

        public BackgroundImageViewModel(string uri, ImageType type)
        {
            Uri = uri;
            Type = type;
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
                        Pic = ImageCache.NoPic;
                    }
                });
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null)
            { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        private async void GetImage()
        {
            if (SettingHelper.GetBoolen("IsNoPicsMode")) { Pic = ImageCache.NoPic; }
            BitmapImage bitmapImage = await ImageCache.GetImage(Type, Uri);
            if (SettingHelper.GetBoolen("IsNoPicsMode")) { return; }
            Pic = bitmapImage;
        }

        private async void SetBrush()
        {
            Windows.Storage.StorageFile file = await ImageCache.GetImagePath(Type, Uri);
            if (file is null) { return; }
            using (Windows.Storage.Streams.IRandomAccessStreamWithContentType stream = await file.OpenReadAsync())
            {
                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);
                QuantizedColor color = await thief.GetColor(decoder);
                BackgroundColor =
                    Windows.UI.Color.FromArgb(
                        color.Color.A,
                        color.Color.R,
                        color.Color.G,
                        color.Color.B);
            }
        }
    }
}
