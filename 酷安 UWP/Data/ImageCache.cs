using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
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

    static class ImageCache
    {
        static BitmapImage whiteNoPicMode = new BitmapImage(new Uri("ms-appx:/Assets/img_placeholder.png")) { DecodePixelHeight = 100, DecodePixelWidth = 100 };
        static BitmapImage darkNoPicMode = new BitmapImage(new Uri("ms-appx:/Assets/img_placeholder_night.png")) { DecodePixelHeight = 100, DecodePixelWidth = 100 };
        static Dictionary<ImageType, StorageFolder> folders = new Dictionary<ImageType, StorageFolder>();

        public static async Task<BitmapImage> GetImage(ImageType type, string url, bool showMessage = false)
        {
            if (string.IsNullOrEmpty(url)) return new BitmapImage();
            else if (url.IndexOf("ms-appx") == 0) return new BitmapImage(new Uri(url));
            else
            {
                string fileName = Tools.GetMD5(url);
                StorageFolder folder = await GetFolder(type);
                var item = await folder.TryGetItemAsync(fileName);
                if (type == ImageType.SmallImage)
                    url += ".s.jpg";
                if (item is null)
                {
                    StorageFile file = await folder.CreateFileAsync(fileName, CreationCollisionOption.OpenIfExists);
                    try { await DownloadImage(file, url, showMessage); }
                    catch (Exception e) { if (e is FileLoadException) return GetLocalImage(file.Path); }
                    return GetLocalImage(file.Path);
                }
                else if (item is StorageFile file) return GetLocalImage(file.Path);
                else return null;
            }
        }

        private static async Task<StorageFolder> GetFolder(ImageType type)
        {
            StorageFolder folder;
            if (folders.ContainsKey(type))
                folder = folders[type];
            else
            {
                folder = await ApplicationData.Current.LocalCacheFolder.TryGetItemAsync(type.ToString()) as StorageFolder;
                if (folder is null) folder = await ApplicationData.Current.LocalCacheFolder.CreateFolderAsync(type.ToString(), CreationCollisionOption.OpenIfExists);
                if (!folders.ContainsKey(type)) folders.Add(type, folder);
            }
            return folder;
        }

        static BitmapImage GetLocalImage(string filename)
        {
            if (filename is null || Settings.GetBoolen("IsNoPicsMode"))
                return Settings.GetBoolen("IsDarkMode") ? darkNoPicMode : whiteNoPicMode;
            else
                return new BitmapImage(new Uri(filename));
        }

        static async Task DownloadImage(StorageFile file, string url, bool showMessage)
        {
            if (!Settings.GetBoolen("IsNoPicsMode"))
                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        HttpContent content = (await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead)).Content;
                        long size = content.Headers.ContentLength ?? 0;
                        if ((await file.GetBasicPropertiesAsync()).Size != (ulong)size)
                        {
                            int downedSize = 0;
                            int readLength = 0;
                            byte[] bytes = new byte[32768];
                            using (Stream stream = await content.ReadAsStreamAsync())
                            using (Stream fs = await file.OpenStreamForWriteAsync())
                            {
                                readLength = stream.Read(bytes, downedSize, bytes.Length);
                                while (readLength > 0)
                                {
                                    fs.Write(bytes, 0, readLength);
                                    fs.Position = downedSize += readLength;
                                    if (showMessage) Tools.ShowMessage($"{Tools.GetSizeString(downedSize)}/{Tools.GetSizeString(size)}", true);
                                    readLength = stream.Read(bytes, 0, bytes.Length);
                                }
                                if (showMessage) Tools.ShowMessage("已完成。");
                            }
                        }
                    }
                }
                catch (HttpRequestException ex) { Tools.ShowHttpExceptionMessage(ex); }
            else GetLocalImage(null);
        }

        public static async Task<string> GetCacheSize(System.Threading.CancellationToken token)
        {
            ulong size = 0;
            for (int i = 0; i < 5; i++)
            {
                StorageFolder folder = await GetFolder((ImageType)i);
                int index = 0;
                var query = folder.CreateFileQuery();
                while (true)
                {
                    var array = await query.GetFilesAsync((uint)index, 100);
                    index += array.Count;
                    if (array.Count > 0)
                        foreach (var item in array)
                        {
                            token.ThrowIfCancellationRequested();
                            size += (await item.GetBasicPropertiesAsync()).Size;
                        }
                    else break;
                }
            }
            return Tools.GetSizeString(size);
        }

        public static async Task CleanCache()
        {
            Tools.ShowProgressBar();
            for (int i = 0; i < 5; i++)
            {
                await (await GetFolder((ImageType)i)).DeleteAsync();
                await ApplicationData.Current.LocalCacheFolder.CreateFolderAsync(((ImageType)i).ToString());
            }

            Tools.HideProgressBar();
        }
    }
}
