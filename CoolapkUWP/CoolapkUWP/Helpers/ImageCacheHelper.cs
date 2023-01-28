using Microsoft.Toolkit.Uwp.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;

namespace CoolapkUWP.Helpers
{
    public enum ImageType
    {
        Icon,
        Captcha,
        BigAvatar,
        SmallImage,
        OriginImage,
        SmallAvatar,
    }

    internal static partial class ImageCacheHelper
    {
        private static readonly BitmapImage WhiteNoPicMode = new BitmapImage(new Uri("ms-appx:/Assets/NoPic/img_placeholder.png")) { DecodePixelHeight = 768, DecodePixelWidth = 768 };
        private static readonly BitmapImage DarkNoPicMode = new BitmapImage(new Uri("ms-appx:/Assets/NoPic/img_placeholder_night.png")) { DecodePixelHeight = 768, DecodePixelWidth = 768 };
        internal static BitmapImage NoPic { get => ThemeHelper.IsDarkTheme() ? DarkNoPicMode : WhiteNoPicMode; }
        internal static readonly CoreDispatcher Dispatcher = Window.Current.Dispatcher;

        static ImageCacheHelper()
        {
            ImageCache.Instance.CacheDuration = TimeSpan.FromHours(8);
        }

        internal static async Task<BitmapImage> GetImageAsync(ImageType type, string url, bool isforce = false)
        {
            Uri uri = NetworkHelper.ValidateAndGetUri(url);
            if (uri == null) { return NoPic; }

            if (url.IndexOf("ms-appx", StringComparison.Ordinal) == 0)
            {
                return new BitmapImage(uri);
            }
            else if (!isforce && SettingsHelper.Get<bool>(SettingsHelper.IsNoPicsMode))
            {
                return NoPic;
            }
            else
            {
                if (type == ImageType.SmallImage || type == ImageType.SmallAvatar)
                {
                    if (url.Contains("coolapk.com") && !url.EndsWith(".png")) { url += ".s.jpg"; }
                    uri = NetworkHelper.ValidateAndGetUri(url);
                }

                try
                {
                    BitmapImage image = await ImageCache.Instance.GetFromCacheAsync(uri, true);
                    return image;
                }
                catch (FileNotFoundException)
                {
                    try
                    {
                        await ImageCache.Instance.RemoveAsync(new Uri[] { uri });
                        BitmapImage image = await ImageCache.Instance.GetFromCacheAsync(uri, true);
                        return image;
                    }
                    catch (Exception)
                    {
                        string str = ResourceLoader.GetForViewIndependentUse().GetString("ImageLoadError");
                        UIHelper.ShowMessage(str);
                        return NoPic;
                    }
                }
                catch (Exception)
                {
                    string str = ResourceLoader.GetForViewIndependentUse().GetString("ImageLoadError");
                    UIHelper.ShowMessage(str);
                    return NoPic;
                }
            }
        }

        internal static async Task<StorageFile> GetImageFileAsync(ImageType type, string url)
        {
            Uri uri = NetworkHelper.ValidateAndGetUri(url);
            if (uri == null) { return null; }

            if (url.IndexOf("ms-appx", StringComparison.Ordinal) == 0)
            {
                return await StorageFile.GetFileFromApplicationUriAsync(uri);
            }
            else
            {
                if (type == ImageType.SmallImage || type == ImageType.SmallAvatar)
                {
                    if (url.Contains("coolapk.com") && !url.EndsWith(".png")) { url += ".s.jpg"; }
                    uri = NetworkHelper.ValidateAndGetUri(url);
                }

                try
                {
                    StorageFile image = await ImageCache.Instance.GetFileFromCacheAsync(uri);
                    if (image != null)
                    {
                        _ = await ImageCache.Instance.GetFromCacheAsync(uri, true);
                        image = await ImageCache.Instance.GetFileFromCacheAsync(uri);
                    }
                    return image;
                }
                catch (FileNotFoundException)
                {
                    try
                    {
                        StorageFile image = await ImageCache.Instance.GetFileFromCacheAsync(uri);
                        if (image != null)
                        {
                            _ = await ImageCache.Instance.GetFromCacheAsync(uri, true);
                            image = await ImageCache.Instance.GetFileFromCacheAsync(uri);
                        }
                        return image;
                    }
                    catch (Exception)
                    {
                        string str = ResourceLoader.GetForViewIndependentUse().GetString("ImageLoadError");
                        UIHelper.ShowMessage(str);
                        return null;
                    }
                }
                catch (Exception)
                {
                    string str = ResourceLoader.GetForViewIndependentUse().GetString("ImageLoadError");
                    UIHelper.ShowMessage(str);
                    return null;
                }
            }
        }

        internal static Task CleanCacheAsync() => ImageCache.Instance.ClearAsync();
    }

    internal static partial class ImageCacheHelper
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
        internal static async Task<BitmapImage> GetImageAsyncOld(ImageType type, string url, bool isforce = false)
        {
            Uri uri = NetworkHelper.ValidateAndGetUri(url);
            if (uri == null) { return NoPic; }

            if (url.IndexOf("ms-appx", StringComparison.Ordinal) == 0)
            {
                return new BitmapImage(uri);
            }
            else if (!isforce && SettingsHelper.Get<bool>(SettingsHelper.IsNoPicsMode))
            {
                return NoPic;
            }
            else
            {
                string fileName = DataHelper.GetMD5(url);
                StorageFolder folder = await GetFolderAsync(type);
                IStorageItem item = await folder.TryGetItemAsync(fileName);
                if (type == ImageType.SmallImage || type == ImageType.SmallAvatar)
                {
                    if (url.Contains("coolapk.com") && !url.EndsWith(".png")) { url += ".s.jpg"; }
                }
                if (item is null)
                {
                    StorageFile file = await folder.CreateFileAsync(fileName, CreationCollisionOption.OpenIfExists);
                    return await DownloadImageAsync(file, url);
                }
                else
                {
                    return item is StorageFile file ? GetLocalImageAsync(file.Path, isforce) : null;
                }
            }
        }

        [Obsolete]
        private static BitmapImage GetLocalImageAsync(string filename, bool forceGetPic)
        {
            try
            {
                return (filename is null || (!forceGetPic && SettingsHelper.Get<bool>(SettingsHelper.IsNoPicsMode))) ? NoPic : new BitmapImage(new Uri(filename));
            }
            catch (Exception)
            {
                return NoPic;
            }
        }

        [Obsolete]
        private static async Task<BitmapImage> DownloadImageAsync(StorageFile file, string url)
        {
            try
            {
                using (HttpClient hc = new HttpClient())
                using (Stream stream = await hc.GetStreamAsync(new Uri(url)))
                using (Stream fs = await file.OpenStreamForWriteAsync())
                {
                    await stream.CopyToAsync(fs);
                }
                return new BitmapImage(new Uri(file.Path));
            }
            catch (FileLoadException)
            {
                return NoPic;
            }
            catch (HttpRequestException)
            {
                string str = ResourceLoader.GetForViewIndependentUse().GetString("ImageLoadError");
                UIHelper.ShowMessage(str);
                return NoPic;
            }
        }

        [Obsolete]
        internal static async Task CleanOldVersionImageCacheAsync()
        {
            for (int i = 0; i < 5; i++)
            {
                ImageType type = (ImageType)i;
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
