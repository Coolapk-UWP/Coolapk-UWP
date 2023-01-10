using CoolapkUWP.Models;
using CoolapkUWP.Models.Feeds;
using Newtonsoft.Json.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using static CoolapkUWP.Models.Feeds.FeedModel;

namespace CoolapkUWP.Controls.DataTemplates
{
    public sealed class CardTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Feed { get; set; }
        public DataTemplate List { get; set; }
        public DataTemplate Images { get; set; }
        public DataTemplate Others { get; set; }
        public DataTemplate FeedReply { get; set; }
        public DataTemplate IconLinks { get; set; }
        public DataTemplate ListWithSubtitle { get; set; }
        protected override DataTemplate SelectTemplateCore(object item)
        {
            if (item is FeedModel) { return Feed; }
            else if (item is FeedReplyModel) { return FeedReply; }
            else if (item is IndexPageHasEntitiesModel IndexPageHasEntitiesModel)
            {
                switch (IndexPageHasEntitiesModel.EntitiesType)
                {
                    case EntityType.Image: return Images;
                    case EntityType.IconLink: return IconLinks;
                    default: return Others;
                }
            }
            else if (item is IList) { return List; }
            else if (item is IListWithSubtitle) { return ListWithSubtitle; }
            else { return Others; }
        }
    }

    public sealed class ItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Null { get; set; }
        public DataTemplate Feed { get; set; }
        public DataTemplate List { get; set; }
        public DataTemplate IconLink { get; set; }
        public DataTemplate FeedReply { get; set; }
        public DataTemplate ListWithSubtitle { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            if (item is FeedModel) { return Feed; }
            else if (item is FeedReplyModel) { return FeedReply; }
            else if (item is IndexPageModel IndexPageModel)
            {
                switch (IndexPageModel?.EntityType ?? string.Empty)
                {
                    case "imageSquare":
                    case "icon":
                    case "iconMiniLink":
                    case "recentHistory":
                    case "iconMini":
                    case "IconLink":
                    case "dyh":
                    case "apk":
                    case "appForum":
                    case "picCategory":
                    case "product":
                    case "entity":
                    case "topic":
                        switch ((item as IndexPageModel).EntityForward)
                        {
                            case "apkListCard":
                            case "feedListCard": return List;
                            default: return IconLink;
                        }
                    default: return Null;
                }
            }
            else if (item is IList) { return List; }
            else if (item is IListWithSubtitle) { return ListWithSubtitle; }
            else { return Null; }
        }
    }

    public sealed class SearchTemplateSelector : DataTemplateSelector
    {
        public DataTemplate App { get; set; }
        public DataTemplate SearchWord { get; set; }
        protected override DataTemplate SelectTemplateCore(object item)
        {
            //if (item is AppModel) return App;
            return SearchWord;
        }
        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container) => SelectTemplateCore(item);
    }

    public static class EntityTemplateSelector
    {
        public static Entity GetEntity(JObject json, bool isHotFeedPage = false)
        {
            switch (json.Value<string>("entityType"))
            {
                case "feed":
                case "discovery": return new FeedModel(json, isHotFeedPage ? FeedDisplayMode.isFirstPageFeed : FeedDisplayMode.normal);
                case "history": return new HistoryModel(json);
                default:
                    if (json.TryGetValue("entityTemplate", out JToken entityTemplate) && !string.IsNullOrEmpty(entityTemplate.ToString()))
                    {
                        switch (entityTemplate.ToString())
                        {
                            case "headCard":
                            case "imageCard":
                            case "imageCarouselCard_1": return new IndexPageHasEntitiesModel(json, EntityType.Image);
                            case "configCard":
                                return json.TryGetValue("url", out JToken url) && url.ToString().Length >= 5
                                    ? new IndexPageHasEntitiesModel(json, EntityType.IconLink)
                                    : null;
                            case "iconLinkGridCard": return new IndexPageHasEntitiesModel(json, EntityType.IconLink);
                            default: return null;
                        }
                    }
                    return null;
            }
        }
    }
}
