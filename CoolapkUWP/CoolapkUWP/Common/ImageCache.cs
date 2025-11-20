using CoolapkUWP.Helpers;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;

namespace CoolapkUWP.Common
{
    public class ImageCache : Microsoft.Toolkit.Uwp.UI.ImageCache
    {
        /// <summary>
        /// Private singleton field.
        /// </summary>
        [ThreadStatic]
        private static ImageCache _instance;

        public new static ImageCache Instance = _instance ?? (_instance = new ImageCache());

        public ImageCache() => Initialize();

        private void Initialize()
        {
            HttpRequestHeaders headers = HttpClient.DefaultRequestHeaders;
            headers.Clear();
            foreach (KeyValuePair<string, IEnumerable<string>> header in NetworkHelper.Client.DefaultRequestHeaders)
            {
                headers.Add(header.Key, header.Value);
            }
            headers.UserAgent.Clear();
            foreach (ProductInfoHeaderValue ua in NetworkHelper.Client.DefaultRequestHeaders.UserAgent)
            {
                headers.UserAgent.Add(ua);
            }
        }
    }
}
