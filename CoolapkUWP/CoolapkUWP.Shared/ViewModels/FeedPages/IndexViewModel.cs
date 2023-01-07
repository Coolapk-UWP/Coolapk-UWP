using CoolapkUWP.Helpers;
using CoolapkUWP.Models;
using CoolapkUWP.Models.Feeds;
using CoolapkUWP.ViewModels.DataSource;
using CoolapkUWP.ViewModels.Providers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using static CoolapkUWP.Models.Feeds.FeedModel;

namespace CoolapkUWP.ViewModels.FeedPages
{
    internal class IndexViewModel : DataSourceBase<Entity>, IViewModel
    {
        private readonly string Uri;
        protected bool IsInitPage => Uri == "/main/init";
        protected bool IsIndexPage => !Uri.Contains("?");
        protected bool IsHotFeedPage => Uri == "/main/indexV8" || Uri == "/main/index";

        internal bool ShowTitleBar { get; }
        private readonly CoolapkListProvider Provider;

        public string Title { get; protected set; }
        public double[] VerticalOffsets { get; set; } = new double[1];

        internal IndexViewModel(string uri, bool showTitleBar = true)
        {
            Uri = GetUri(uri);
            ShowTitleBar = showTitleBar;
            Title = ResourceLoader.GetForCurrentView("MainPage").GetString("Home");
            Provider = new CoolapkListProvider(
                (p, _, __) => UriHelper.GetUri(UriType.GetIndexPage, Uri, IsIndexPage ? "?" : "&", p),
                GetEntities,
                "entityId");
        }

        public async Task Refresh(bool reset = false)
        {
            if (reset)
            {
                await Reset();
            }
            else
            {
                _ = await LoadItemsAsync(20);
            }
        }

        private string GetUri(string uri)
        {
            if (uri.Contains("&title="))
            {
                const string Value = "&title=";
                Title = uri.Substring(uri.LastIndexOf(Value, StringComparison.Ordinal) + Value.Length);
            }

            if (uri.IndexOf("/page", StringComparison.Ordinal) == -1 && (uri.StartsWith("#", StringComparison.Ordinal) || (!uri.Contains("/main/") && !uri.Contains("/user/") && !uri.Contains("/apk/") && !uri.Contains("/appForum/") && !uri.Contains("/picture/") && !uri.Contains("/topic/") && !uri.Contains("/discovery/"))))
            {
                uri = "/page/dataList?url=" + uri;
            }
            else if (uri.IndexOf("/page", StringComparison.Ordinal) == 0 && !uri.Contains("/page/dataList"))
            {
                uri = uri.Replace("/page", "/page/dataList");
            }
            return uri.Replace("#", "%23");
        }

        private IEnumerable<Entity> GetEntities(JObject jo)
        {
            if (jo.TryGetValue("entityTemplate", out JToken t) && t?.ToString() == "configCard")
            {
                JObject j = JObject.Parse(jo.Value<string>("extraData"));
                Title = j.Value<string>("pageTitle");
                yield return null;
            }
            else if (jo.TryGetValue("entityTemplate", out JToken tt) && tt?.ToString() == "fabCard") { yield return null; }
            else if (tt?.ToString() == "feedCoolPictureGridCard")
            {
                foreach (JObject item in jo.Value<JArray>("entities"))
                {
                    Entity entity = GetEntity(item, IsHotFeedPage);
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

        private static Entity GetEntity(JObject jo, bool isHotFeedPage = false)
        {
            switch (jo.Value<string>("entityType"))
            {
                case "feed":
                case "discovery": return new FeedModel(jo, isHotFeedPage ? FeedDisplayMode.isFirstPageFeed : FeedDisplayMode.normal);
                default:
                    if (jo.TryGetValue("entityTemplate", out JToken entityTemplate) && !string.IsNullOrEmpty(entityTemplate.ToString()))
                    {
                        switch (entityTemplate.ToString())
                        {
                            case "headCard":
                            case "imageCard":
                            case "imageCarouselCard_1": return new IndexPageHasEntitiesModel(jo, EntityType.Image);
                            default: return null;
                        }
                    }
                    return null;
            }
        }

        protected override async Task<IList<Entity>> LoadItemsAsync(uint count)
        {
            List<Entity> Models = new List<Entity>();
            while (Models.Count < count)
            {
                int temp = Models.Count;
                if (Models.Count > 0) { _currentPage++; }
                await Provider.GetEntity(Models, _currentPage);
                if (Models.Count <= 0 || Models.Count <= temp) { break; }
            }
            return Models;
        }

        protected override void AddItems(IList<Entity> items)
        {
            if (items != null)
            {
                foreach (Entity item in items)
                {
                    if (item is NullModel) { continue; }
                    Add(item);
                    InvokeProgressChanged(item, items);
                }
            }
        }
    }
}
