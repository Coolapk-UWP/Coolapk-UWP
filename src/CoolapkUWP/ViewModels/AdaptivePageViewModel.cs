using CoolapkUWP.Controls;
using CoolapkUWP.Core.Helpers;
using CoolapkUWP.Core.Models;
using CoolapkUWP.Core.Providers;
using CoolapkUWP.Helpers;
using CoolapkUWP.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace CoolapkUWP.ViewModels.AdaptivePage
{
    internal enum ListType
    {
        UserFeed,
        FeedInfo,
    }

    internal class ViewModel : IViewModel
    {
        private readonly CoolapkListProvider provider;
        private readonly FeedModel feedModel;

        public ObservableCollection<Entity> Models;
        public double[] VerticalOffsets { get; set; } = new double[1];
        public string Title { get; protected set; } = string.Empty;

        internal ViewModel(string id, ListType type, string branch)
        {
            Title = GetTitle(type, branch);
            provider = GetProvider(id, type, branch);
            Models = provider.Models;
        }

        private static string GetTitle(ListType type, string branch)
        {
            Windows.ApplicationModel.Resources.ResourceLoader loader = Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse("AdaptivePage");
            switch (type)
            {
                case ListType.UserFeed:
                    return $"{loader.GetString("UserFeed")}{loader.GetString(branch)}";
                case ListType.FeedInfo:
                    return $"{loader.GetString("FeedInfo")}{loader.GetString(branch)}";
                default: return null;
            }
        }

        private CoolapkListProvider GetProvider(string id, ListType type, string branch)
        {
            return new CoolapkListProvider(
                    GetUri(id, type, branch),
                    (a, b) => a.EntityId == b.Value<string>("entityId").Replace("\"", string.Empty, StringComparison.Ordinal),
                    GetEntities,
                    "entityId");
        }

        internal static Func<int, int, string, string, Uri> GetUri(string id, ListType type, string branch)
        {
            switch (type)
            {
                case ListType.UserFeed:
                    return (p, page, firstItem, lastItem) =>
                        UriHelper.GetUri(
                            UriType.GetUserFeeds,
                            id,
                            p < 0 ? ++page : p,
                            string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}",
                            string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}",
                            branch);
                case ListType.FeedInfo:
                    return (p, page, firstItem, lastItem) =>
                        UriHelper.GetUri(
                            UriType.GetFeedInfos,
                            id,
                            p < 0 ? ++page : p,
                            string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}",
                            string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}",
                            branch);
                default: return null;
            }
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
                    Entity entity = EntityTemplateSelector.GetEntity(item);
                    if (entity != null)
                    {
                        yield return entity;
                    }
                }
            }
            else
            {
                yield return EntityTemplateSelector.GetEntity(jo);
            }
            yield break;
        }

        public async Task Refresh(int p = -1)
        {
            await provider?.Refresh(p);
        }
    }
}