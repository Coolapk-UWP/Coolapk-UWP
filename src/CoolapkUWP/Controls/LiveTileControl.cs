using CoolapkUWP.Core.Helpers;
using CoolapkUWP.Core.Models;
using CoolapkUWP.Core.Providers;
using CoolapkUWP.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Foundation;

namespace CoolapkUWP.Controls
{
    class LiveTileControl : IBackgroundTask
    {
        private readonly string mainUri;
        protected bool IsHotFeedPage { get => mainUri == "/main/indexV8"; }
        protected bool IsIndexPage { get => mainUri.Contains("index") && !mainUri.Contains("index?") || mainUri.Contains("init") && !mainUri.Contains("init?") || mainUri.Contains("list") && !mainUri.Contains("list?") || mainUri.Contains("List") && !mainUri.Contains("List?") || mainUri.Contains("dyhSubscribe") && !mainUri.Contains("dyhSubscribe?"); }

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();

            await GetLatestNews();

            deferral.Complete();
        }

        private IAsyncOperation<string> GetLatestNews()
        {
            try
            {
                return AsyncInfo.Run(token => GetNews());
            }
            catch (Exception)
            {
                // ignored
            }
            return null;
        }

        private async Task<string> GetNews()
        {
            try
            {
                var response = GetProvider(mainUri);

            }
            catch (Exception)
            {
                // ignored
            }
            return null;
        }

        private static Entity GetEntity(JObject jo, bool isHotFeedPage)
        {
            switch (jo.Value<string>("entityType"))
            {
                case "feed": return new FeedModel(jo, isHotFeedPage ? FeedDisplayMode.isFirstPageFeed : FeedDisplayMode.normal);
                case "discovery": return new FeedModel(jo, isHotFeedPage ? FeedDisplayMode.isFirstPageFeed : FeedDisplayMode.normal);
                default:
                    if (jo.TryGetValue("entityTemplate", out JToken v1))
                    {
                        switch (v1.Value<string>())
                        {
                            case "feed": return new FeedModel(jo, isHotFeedPage ? FeedDisplayMode.isFirstPageFeed : FeedDisplayMode.normal);
                            default: return null;
                        }
                    }
                    else return null;
            }
        }

        private IEnumerable<Entity> GetEntities(JObject jo)
        {
            if (jo.TryGetValue("entityTemplate", out JToken tt) && tt?.ToString() == "fabCard") { yield return null; }
            else if (tt?.ToString() == "feedCoolPictureGridCard")
            {
                foreach (JObject item in jo.Value<JArray>("entities"))
                {
                    var entity = GetEntity(item, IsHotFeedPage);
                    if (entity != null)
                    {
                        yield return entity;
                    }
                }
            }
            else
            {
                yield return GetEntity(jo, IsHotFeedPage);
            }
            yield break;
        }

        private CoolapkListProvider GetProvider(string uri)
        {
            return new CoolapkListProvider(
                    GetUri(uri, IsIndexPage),
                    (a, b) => a.EntityId == b.Value<string>("entityId").Replace("\"", string.Empty, StringComparison.Ordinal),
                    GetEntities,
                    "entityId");
        }

        internal static Func<int, int, string, string, Uri> GetUri(string uri, bool isHotFeedPage)
        {
            return (p, page, _, __) =>
                UriHelper.GetUri(
                    UriType.GetIndexPage,
                    uri,
                    isHotFeedPage ? "?" : "&",
                    1);
        }
    }
}
