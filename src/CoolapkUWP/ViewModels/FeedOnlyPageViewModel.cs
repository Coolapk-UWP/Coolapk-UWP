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

namespace CoolapkUWP.ViewModels.FeedOnlyPage
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
            var loader = Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse("FeedOnlyPage");
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
                    var entity = GetEntity(item);
                    if (entity != null)
                    {
                        yield return entity;
                    }
                }
            }
            else
            {
                yield return GetEntity(jo);
            }
            yield break;
        }

        private static Entity GetEntity(JObject jo)
        {
            switch (jo.Value<string>("entityType"))
            {
                case "feed": return new FeedModel(jo, FeedDisplayMode.normal);
                case "discovery": return new FeedModel(jo, FeedDisplayMode.normal);
                case "user": return new UserModel(jo);
                case "topic": return new TopicModel(jo);
                case "dyh": return new DyhModel(jo);
                case "product": return new ProductModel(jo);
                case "entity_type_user_card_manager": return new IndexPageOperationCardModel(jo, OperationType.ShowTitle);
                default:
                    if (jo.TryGetValue("entityTemplate", out JToken v1))
                    {
                        switch (v1.Value<string>())
                        {
                            case "feed": return new FeedModel(jo, FeedDisplayMode.normal);
                            case "imageTextGridCard":
                            case "imageSquareScrollCard":
                            case "iconScrollCard":
                            case "iconGridCard":
                            case "feedScrollCard":
                            case "imageTextScrollCard":
                            case "iconMiniLinkGridCard":
                            case "iconMiniGridCard":
                            case "colorfulFatScrollCard":
                            case "colorfulScrollCard":
                            case "iconLongTitleGridCard":
                            case "gridCard": return new IndexPageHasEntitiesModel(jo, EntityType.Others);
                            //case "listCard": //return new IndexPageHasEntitiesModel(jo, EntityType.Others);
                            case "headCard":
                            case "imageCarouselCard_1": //return new IndexPageHasEntitiesViewModel(jo, EntitiesType.Image_1);
                            case "imageCard":
                            case "iconButtonGridCard": return new IndexPageHasEntitiesModel(jo, EntityType.Image);
                            case "configCard":
                                return jo.TryGetValue("url", out JToken v2) && v2.ToString().Length >= 5
                                    ? new IndexPageHasEntitiesModel(jo, EntityType.IconLink)
                                    : null;
                            case "iconLinkGridCard": return new IndexPageHasEntitiesModel(jo, EntityType.IconLink);
                            case "feedGroupListCard":
                            case "textLinkListCard": return new IndexPageHasEntitiesModel(jo, EntityType.TextLinks);
                            case "textCard":
                            case "hot":
                            case "messageCard": return new IndexPageMessageCardModel(jo);
                            case "refreshCard": return new IndexPageOperationCardModel(jo, OperationType.Refresh);
                            case "unLoginCard": return new IndexPageOperationCardModel(jo, OperationType.Login);
                            case "titleCard": return new IndexPageOperationCardModel(jo, OperationType.ShowTitle);
                            case "iconTabLinkGridCard": return new IndexPageHasEntitiesModel(jo, EntityType.TabLink);
                            case "selectorLinkCard": return new IndexPageHasEntitiesModel(jo, EntityType.SelectorLink);
                            default: return null;
                        }
                    }
                    else { return null; }
            }
        }

        public async Task Refresh(int p = -1)
        {
            await provider?.Refresh(p);
            if (p == -2 && provider?.Models.Count == 0)
            {
                provider?.Models.Insert(0, feedModel);
            }
        }
    }
}