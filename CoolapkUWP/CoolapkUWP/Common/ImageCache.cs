using CoolapkUWP.Helpers;
using System;
using System.Net.Http;

namespace CoolapkUWP.Common
{
    public class ImageCache : Microsoft.Toolkit.Uwp.UI.ImageCache
    {
        /// <summary>
        /// Private singleton field.
        /// </summary>
        [ThreadStatic]
        private static ImageCache _instance;

        public static new ImageCache Instance => _instance ?? (_instance = new ImageCache { CacheDuration = TimeSpan.FromHours(8) });

        public ImageCache() => Initialize();

        private void Initialize()
        {
            NetworkHelper.SetRequestHeaders(HttpClient);
            HttpClient.DefaultRequestHeaders.Add("X-App-Token", NetworkHelper.TokenCreator.GetToken());
        }
    }
}
