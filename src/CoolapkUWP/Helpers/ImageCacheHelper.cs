using Microsoft.Toolkit.Uwp.UI;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;
using InAppNotify = Microsoft.Toolkit.Uwp.UI.Controls.InAppNotification;

namespace CoolapkUWP.Helpers
{
    public enum ImageType
    {
        SmallImage,
        OriginImage,
        SmallAvatar,
        BigAvatar,
        Icon,
        Captcha,
    }

    internal static partial class ImageCacheHelper
    {
        private static readonly BitmapImage whiteNoPicMode = new BitmapImage(new Uri("ms-appx:/Assets/img_placeholder.png")) { DecodePixelHeight = 100, DecodePixelWidth = 100 };
        private static readonly BitmapImage darkNoPicMode = new BitmapImage(new Uri("ms-appx:/Assets/img_placeholder_night.png")) { DecodePixelHeight = 100, DecodePixelWidth = 100 };
        internal static BitmapImage NoPic { get => SettingsHelper.Get<bool>(SettingsHelper.IsDarkMode) ? darkNoPicMode : whiteNoPicMode; }

        static ImageCacheHelper()
        {
            ImageCache.Instance.CacheDuration = TimeSpan.FromHours(8);
        }

        internal static async Task<BitmapImage> GetImageAsync(ImageType type, string url, Pages.ImageModel model = null, InAppNotify notify = null)
        {
            if (string.IsNullOrEmpty(url)) { return null; }

            if (url.IndexOf("ms-appx", StringComparison.Ordinal) == 0)
            {
                return new BitmapImage(new Uri(url));
            }
            else if (model == null && SettingsHelper.Get<bool>(SettingsHelper.IsNoPicsMode))
            {
                return NoPic;
            }
            else
            {
                if (type == ImageType.SmallImage || type == ImageType.SmallAvatar)
                {
                    url += ".s.jpg";
                }
                var uri = new Uri(url);

                try
                {
                    if (model != null)
                    {
                        model.IsProgressRingActived = true;
                    }
                    var image = await ImageCache.Instance.GetFromCacheAsync(uri, true);
                    return image;
                }
                catch
                {
                    var str = Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse().GetString("ImageLoadError");
                    if (notify == null)
                    {
                        UIHelper.ShowMessage(str);
                    }
                    else
                    {
                        _ = notify.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                        {
                            notify.Show(str, UIHelper.duration);
                        });
                    }
                    return NoPic;
                }
                finally
                {
                    if (model != null)
                    {
                        model.IsProgressRingActived = false;
                    }
                }
            }
        }

        internal static Task CleanCacheAsync()
        {
            return ImageCache.Instance.ClearAsync();
        }
    }

    static partial class ImageCacheHelper
    {
        [Obsolete]
        private static readonly Dictionary<ImageType, StorageFolder> folders = new Dictionary<ImageType, StorageFolder>();

        [Obsolete]
        internal static async Task<StorageFolder> GetFolderAsync(ImageType type)
        {
            StorageFolder folder;
            if (folders.ContainsKey(type))
            {
                folder = folders[type];
            }
            else
            {
                folder = await ApplicationData.Current.LocalCacheFolder.TryGetItemAsync(type.ToString()) as StorageFolder;
                if (folder is null)
                {
                    folder = await ApplicationData.Current.LocalCacheFolder.CreateFolderAsync(type.ToString(), CreationCollisionOption.OpenIfExists);
                }
                if (!folders.ContainsKey(type))
                {
                    folders.Add(type, folder);
                }
            }
            return folder;
        }

        [Obsolete]
        internal static async Task CleanOldVersionImageCacheAsync()
        {
            for (int i = 0; i < 5; i++)
            {
                var type = (ImageType)i;
                await (await GetFolderAsync(type)).DeleteAsync();
                await ApplicationData.Current.LocalCacheFolder.CreateFolderAsync(type.ToString());
            }
        }

        internal static async Task CleanCaptchaCacheAsync()
        {
#pragma warning disable 0612
            await (await GetFolderAsync(ImageType.Captcha)).DeleteAsync();
#pragma warning restore 0612
            await ApplicationData.Current.LocalCacheFolder.CreateFolderAsync("Captcha");
        }
    }
}