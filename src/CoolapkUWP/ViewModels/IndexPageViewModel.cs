using CoolapkUWP.Core.Helpers;
using CoolapkUWP.Core.Models;
using CoolapkUWP.Core.Providers;
using CoolapkUWP.Helpers;
using CoolapkUWP.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace CoolapkUWP.ViewModels.IndexPage
{
    internal class ViewModel : IViewModel, ICanComboBoxChangeSelectedIndex
    {
        private readonly string mainUri;
        internal CoolapkListProvider mainProvider { get; private set; }
        internal readonly ObservableCollection<Entity> mainModels;
        internal ImmutableList<CoolapkListProvider> tabProviders { get; private set; } = ImmutableList<CoolapkListProvider>.Empty;
        protected bool IsHotFeedPage { get => mainUri == "/main/indexV8" || mainUri == "/main/index"; }
        protected bool IsIndexPage { get => !mainUri.Contains("?"); }
        protected bool IsInitPage { get => mainUri == "/main/init"; }
        internal bool ShowTitleBar { get; }

        public int ComboBoxSelectedIndex { get; private set; }
        public double[] VerticalOffsets { get; set; } = new double[1];
        public string Title { get; protected set; }

        internal ViewModel(string uri, bool showTitleBar)
        {
            ShowTitleBar = showTitleBar;
            mainUri = GetUri(uri);

            mainProvider = GetProvider(mainUri);
            //UIHelper.ShowMessage(mainUri);
            mainModels = mainProvider.Models;
        }

        public async Task Refresh(int p = -1)
        {
            if (tabProviders.Count == 0)
            {
                await mainProvider?.Refresh(p);
            }
            else if (p == -1 && ComboBoxSelectedIndex < tabProviders.Count)
            {
                await tabProviders[ComboBoxSelectedIndex]?.Refresh(p);
            }
        }

        private string GetUri(string uri)
        {
            if (uri.Contains("&title=", StringComparison.Ordinal))
            {
                const string Value = "&title=";
                Title = uri.Substring(uri.LastIndexOf(Value, StringComparison.Ordinal) + Value.Length);
            }

            if (uri.IndexOf("/page", StringComparison.Ordinal) == -1 && (uri.StartsWith("#", StringComparison.Ordinal) || (!uri.Contains("/main/") && !uri.Contains("/user/") && !uri.Contains("/apk/") && !uri.Contains("/appForum/") && !uri.Contains("/picture/") && !uri.Contains("/topic/") && !uri.Contains("/discovery/"))))
            {
                uri = "/page/dataList?url=" + uri;
            }
            else if (uri.IndexOf("/page", StringComparison.Ordinal) == 0 && !uri.Contains("/page/dataList", StringComparison.Ordinal))
            {
                uri = uri.Replace("/page", "/page/dataList", StringComparison.Ordinal);
            }
            return uri.Replace("#", "%23", StringComparison.Ordinal);
        }

        private static Entity GetEntity(JObject jo, bool isHotFeedPage)
        {
            switch (jo.Value<string>("entityType"))
            {
                case "feed":
                case "discovery": return new FeedModel(jo, isHotFeedPage ? FeedDisplayMode.isFirstPageFeed : FeedDisplayMode.normal);
                case "liveTopic": return new LiveMode(jo);
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
                            case "feed": return new FeedModel(jo, isHotFeedPage ? FeedDisplayMode.isFirstPageFeed : FeedDisplayMode.normal);
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

        private IEnumerable<Entity> GetEntities(JObject jo)
        {
            if (ComboBoxSelectedIndex == -1 && jo.TryGetValue("entityTemplate", out JToken t) && t?.ToString() == "configCard")
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
                    p < 0 ? ++page : p);
        }

        internal void AddTab(string uri) => tabProviders = tabProviders.Add(GetProvider(uri));

        public async Task SetComboBoxSelectedIndex(int value)
        {
            if (value < 0 && value >= tabProviders.Count) { return; }

            ComboBoxSelectedIndex = value;
            if (tabProviders[ComboBoxSelectedIndex].Models.Count == 0)
            {
                await Refresh();
            }
        }
    }
}