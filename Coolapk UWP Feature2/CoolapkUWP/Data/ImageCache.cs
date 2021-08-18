using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;

namespace CoolapkUWP.Data
{
    public enum ImageType
    {
        SmallImage = 0,
        OriginImage,
        SmallAvatar,
        BigAvatar,
        Icon
    }

    internal static class ImageCache
    {
        private static readonly BitmapImage whiteNoPicMode = new BitmapImage(new Uri("ms-appx:/Assets/NoPic/img_placeholder.png")) { DecodePixelHeight = 768, DecodePixelWidth = 768 };
        private static readonly BitmapImage darkNoPicMode = new BitmapImage(new Uri("ms-appx:/Assets/NoPic/img_placeholder_night.png")) { DecodePixelHeight = 768, DecodePixelWidth = 768 };
        public static readonly BitmapImage imageempty = new BitmapImage(new Uri("ms-appx:/Assets/NoPic/image_empty.png")) { DecodePixelHeight = 768, DecodePixelWidth = 768 };
        private static readonly Dictionary<ImageType, StorageFolder> folders = new Dictionary<ImageType, StorageFolder>();
        public static BitmapImage NoPic { get => SettingsHelper.IsDarkTheme() ? darkNoPicMode : whiteNoPicMode; }
        public static readonly string[] defaultNoAvatarUrl = { "http://avatar.coolapk.com/images/avatar_small.gif", "http://avatar.coolapk.com/images/avatar_middle.gif", "http://avatar.coolapk.com/images/avatar_big.gif" };

        private static async Task<StorageFolder> GetFolder(ImageType type)
        {
            StorageFolder folder;
            if (folders.ContainsKey(type)) { folder = folders[type]; }
            else
            {
                folder = await ApplicationData.Current.LocalCacheFolder.TryGetItemAsync(type.ToString()) as StorageFolder;
                if (folder is null) { folder = await ApplicationData.Current.LocalCacheFolder.CreateFolderAsync(type.ToString(), CreationCollisionOption.OpenIfExists); }
                if (!folders.ContainsKey(type)) { folders.Add(type, folder); }
            }
            return folder;
        }

        public static async Task<BitmapImage> GetImage(ImageType type, string url, bool showMessage = false)
        {
            if (string.IsNullOrEmpty(url)) { return NoPic; }
            else if (url.IndexOf("ms-appx") == 0) { return new BitmapImage(new Uri(url)); }
            else
            {
                string fileName = UIHelper.GetMD5(url);
                StorageFolder folder = await GetFolder(type);
                IStorageItem item = await folder.TryGetItemAsync(fileName);
                if (type == ImageType.SmallImage)
                { url += ".s.jpg"; }
                if (item is null)
                {
                    StorageFile file = await folder.CreateFileAsync(fileName, CreationCollisionOption.OpenIfExists);
                    try { await DownloadImage(file, url, showMessage); }
                    catch (FileLoadException) { return GetLocalImage(file.Path); }
                    return GetLocalImage(file.Path);
                }
                else { return item is StorageFile file ? GetLocalImage(file.Path) : NoPic; }
            }
        }

        public static async Task<StorageFile> GetImagePath(ImageType type, string url, bool showMessage = false)
        {
            if (url.IndexOf("ms-appx") == 0) { return await StorageFile.GetFileFromApplicationUriAsync(new Uri(url)); }
            else if (string.IsNullOrEmpty(url) || SettingsHelper.GetBoolen("IsNoPicsMode"))
            { return await StorageFile.GetFileFromApplicationUriAsync(new Uri(SettingsHelper.IsDarkTheme() ? "ms-appx:/Assets/NoPic/img_placeholder_night.png" : "ms-appx:/Assets/NoPic/img_placeholder.png")); }
            else
            {
                string fileName = UIHelper.GetMD5(url);
                StorageFolder folder = await GetFolder(type);
                IStorageItem item = await folder.TryGetItemAsync(fileName);
                if (type == ImageType.SmallImage)
                { url += ".s.jpg"; }
                if (item is null)
                {
                    StorageFile file = await folder.CreateFileAsync(fileName, CreationCollisionOption.OpenIfExists);
                    try { await DownloadImage(file, url, showMessage); }
                    catch (FileLoadException) { return file; }
                    return file;
                }
                else { return item is StorageFile file ? file : null; }
            }
        }

        private static BitmapImage GetLocalImage(string filename) =>
            (filename is null || SettingsHelper.GetBoolen("IsNoPicsMode")) ? NoPic : new BitmapImage(new Uri(filename));

        private static async Task DownloadImage(StorageFile file, string url, bool showMessage)
        {
            if (!SettingsHelper.GetBoolen("IsNoPicsMode"))
            {
                try
                {
                    if (showMessage) { UIHelper.ShowProgressBar(); }
                    using (Stream stream = await new HttpClient().GetStreamAsync(url))
                    using (Stream fs = await file.OpenStreamForWriteAsync())
                    { await stream.CopyToAsync(fs); }
                }
                catch (HttpRequestException ex) { UIHelper.ShowHttpExceptionMessage(ex); }
                finally { if (showMessage) { UIHelper.HideProgressBar(); } }
            }
        }

        public static async Task<double> GetCacheSize(ImageType type, System.Threading.CancellationToken token)
        {
            ulong size = 0;
            StorageFolder folder = await GetFolder(type);
            int index = 0;
            Windows.Storage.Search.StorageFileQueryResult query = folder.CreateFileQuery();
            while (true)
            {
                IReadOnlyList<StorageFile> array = await query.GetFilesAsync((uint)index, 100);
                index += array.Count;
                if (array.Count > 0)
                {
                    foreach (StorageFile item in array)
                    {
                        token.ThrowIfCancellationRequested();
                        size += (await item.GetBasicPropertiesAsync()).Size;
                    }
                }
                else { break; }
            }
            return size;
        }

        public static async Task CleanCache(ImageType type)
        {
            await (await GetFolder(type)).DeleteAsync();
            _ = await ApplicationData.Current.LocalCacheFolder.CreateFolderAsync(type.ToString());
        }
    }
}
