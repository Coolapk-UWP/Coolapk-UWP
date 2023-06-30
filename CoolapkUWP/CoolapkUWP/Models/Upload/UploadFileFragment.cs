using CoolapkUWP.Helpers;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace CoolapkUWP.Models.Upload
{
    public class UploadFileFragment
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("resolution")]
        public string Resolution { get; set; }

        [JsonProperty("md5")]
        public string MD5 { get; set; }

        [JsonIgnore]
        public byte[] Bytes { get; set; }

        public static async Task<UploadFileFragment> FromWriteableBitmap(WriteableBitmap bitmap)
        {
            using (InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream())
            {
                BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
                Stream pixelStream = bitmap.PixelBuffer.AsStream();
                byte[] pixels = new byte[pixelStream.Length];
                await pixelStream.ReadAsync(pixels, 0, pixels.Length);

                encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied,
                    (uint)bitmap.PixelWidth,
                    (uint)bitmap.PixelHeight,
                    96.0,
                    96.0,
                    pixels);

                await encoder.FlushAsync();

                byte[] bytes = stream.GetBytes();

                HashAlgorithmProvider Provider = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Md5);
                IBuffer computedHash = Provider.HashData(stream.GetBuffer());

                return new UploadFileFragment
                {
                    Name = $"CoolapkUWP_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.png",
                    Resolution = $"{bitmap.PixelWidth}x{bitmap.PixelHeight}",
                    MD5 = CryptographicBuffer.EncodeToHexString(computedHash),
                    Bytes = bytes
                };
            }
        }

        public override bool Equals(object obj)
        {
            return obj is UploadFileFragment && MD5.Equals((obj as UploadFileFragment).MD5);
        }

        public override int GetHashCode()
        {
            return MD5.GetHashCode();
        }
    }
}
