using CoolapkUWP.Models;
using CoolapkUWP.Models.Feeds;
using CoolapkUWP.Models.Pages;
using CoolapkUWP.Models.Users;
using Newtonsoft.Json.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using static CoolapkUWP.Models.Feeds.FeedModel;

namespace CoolapkUWP.Controls.DataTemplates
{
    public sealed class CardTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Feed { get; set; }
        public DataTemplate User { get; set; }
        public DataTemplate List { get; set; }
        public DataTemplate Images { get; set; }
        public DataTemplate Others { get; set; }
        public DataTemplate FeedReply { get; set; }
        public DataTemplate IconLinks { get; set; }
        public DataTemplate LoginCard { get; set; }
        public DataTemplate TitleCard { get; set; }
        public DataTemplate CommentMe { get; set; }
        public DataTemplate LikeNotify { get; set; }
        public DataTemplate AtCommentMe { get; set; }
        public DataTemplate RefreshCard { get; set; }
        public DataTemplate MessageCard { get; set; }
        public DataTemplate SubtitleList { get; set; }
        public DataTemplate MessageNotify { get; set; }
        public DataTemplate GridScrollCard { get; set; }
        public DataTemplate ImageTextScrollCard { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            if (item is FeedModel) { return Feed; }
            else if (item is UserModel) { return User; }
            else if (item is FeedReplyModel) { return FeedReply; }
            else if (item is IndexPageMessageCardModel) { return MessageCard; }
            else if (item is IndexPageHasEntitiesModel IndexPageHasEntitiesModel)
            {
                switch (IndexPageHasEntitiesModel.EntitiesType)
                {
                    case EntityType.Image: return Images;
                    case EntityType.IconLink: return IconLinks;
                    case EntityType.GridLink: return GridScrollCard;
                    case EntityType.Others:
                    default: return ImageTextScrollCard;
                }
            }
            else if (item is IndexPageOperationCardModel IndexPageOperationCardModel)
            {
                switch (IndexPageOperationCardModel.OperationType)
                {
                    case OperationType.Refresh: return RefreshCard;
                    case OperationType.Login: return LoginCard;
                    case OperationType.ShowTitle: return TitleCard;
                    default: return Others;
                }
            }
            else if (item is LikeNotificationModel) { return LikeNotify; }
            else if (item is SimpleNotificationModel) { return CommentMe; }
            else if (item is MessageNotificationModel) { return MessageNotify; }
            else if (item is AtCommentMeNotificationModel) { return AtCommentMe; }
            else if (item is IHasDescription) { return List; }
            else if (item is IHasSubtitle) { return SubtitleList; }
            else { return Others; }
        }
    }

    public sealed class ItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Feed { get; set; }
        public DataTemplate User { get; set; }
        public DataTemplate List { get; set; }
        public DataTemplate Link { get; set; }
        public DataTemplate Empty { get; set; }
        public DataTemplate IconLink { get; set; }
        public DataTemplate MiniUser { get; set; }
        public DataTemplate FeedReply { get; set; }
        public DataTemplate ImageText { get; set; }
        public DataTemplate MiniIconLink { get; set; }
        public DataTemplate SubtitleList { get; set; }
        public DataTemplate FeedImageText { get; set; }
        public DataTemplate SquareLinkCard { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            if (item is UserModel user)
            {
                return user.EntityForward == "iconScrollCard" ? MiniUser : User;
            }
            else if (item is SourceFeedModel feed)
            {
                return feed.EntityForward == "imageTextScrollCard" ? FeedImageText : Feed;
            }
            else if (item is CollectionModel collection)
            {
                return collection.EntityForward == "iconMiniGridCard" ? MiniIconLink : List;
            }
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
                            case "imageSquareScrollCard": return SquareLinkCard;
                            case "apkListCard":
                            case "feedListCard": return List;
                            default: return IconLink;
                        }
                    case "iconButton":
                    case "link": return Link;
                    case "imageText": return ImageText;
                    default: return Empty;
                }
            }
            else
            {
                return item is IHasDescription ? List : item is IHasSubtitle ? SubtitleList : Empty;
            }
        }
    }

    public sealed class ProfileCardTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Others { get; set; }
        public DataTemplate TitleCard { get; set; }
        public DataTemplate TextLinkList { get; set; }
        public DataTemplate ImageTextScrollCard { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            if (item is IndexPageHasEntitiesModel IndexPageHasEntitiesModel)
            {
                switch (IndexPageHasEntitiesModel.EntitiesType)
                {
                    case EntityType.TextLinks: return TextLinkList;
                    default: return ImageTextScrollCard;
                }
            }
            else if (item is IndexPageOperationCardModel IndexPageOperationCardModel)
            {
                switch (IndexPageOperationCardModel.OperationType)
                {
                    case OperationType.ShowTitle: return TitleCard;
                    default: return Others;
                }
            }
            else { return Others; }
        }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container) => SelectTemplateCore(item);
    }

    public sealed class ProfileItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Empty { get; set; }
        public DataTemplate History { get; set; }
        public DataTemplate IconLink { get; set; }
        public DataTemplate TextLink { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            if (item is CollectionModel)
            {
                return History;
            }
            else if (item is IndexPageModel IndexPageModel)
            {
                switch (IndexPageModel?.EntityType)
                {
                    case "topic":
                    case "recentHistory": return IconLink;
                    case "textLink": return TextLink;
                    case "collection":
                    case "history": return History;
                    default: return Empty;
                }
            }
            else { return Empty; }
        }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container) => SelectTemplateCore(item);
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
                case "discovery": return new FeedModel(json, isHotFeedPage ? FeedDisplayMode.IsFirstPageFeed : FeedDisplayMode.Normal);
                case "user": return new UserModel(json);
                case "topic": return new TopicModel(json);
                case "history": return new HistoryModel(json);
                case "collection": return new CollectionModel(json);
                case "entity_type_user_card_manager": return new IndexPageOperationCardModel(json, OperationType.ShowTitle);
                default:
                    if (json.TryGetValue("entityTemplate", out JToken entityTemplate) && !string.IsNullOrEmpty(entityTemplate.ToString()))
                    {
                        switch (entityTemplate.ToString())
                        {
                            case "feed": return new FeedModel(json, isHotFeedPage ? FeedDisplayMode.IsFirstPageFeed : FeedDisplayMode.Normal);
                            case "imageSquareScrollCard":
                            case "iconScrollCard":
                            case "iconGridCard":
                            case "feedScrollCard":
                            case "imageTextScrollCard":
                            case "colorfulFatScrollCard":
                            case "colorfulScrollCard":
                            case "iconLongTitleGridCard":
                            case "linkCard":
                            case "iconButtonGridCard":
                            case "apkScrollCardWithBackground":
                            case "imageScrollCard":
                            case "apkScrollCard":
                            //case "iconListCard":
                            //case "listCard":
                            case "gridCard": return new IndexPageHasEntitiesModel(json, EntityType.Others);
                            case "iconMiniLinkGridCard":
                            case "iconMiniGridCard": return new IndexPageHasEntitiesModel(json, EntityType.GridLink);
                            //case "listCard": //return new IndexPageHasEntitiesModel(jo, EntityType.Others);
                            case "headCard":
                            case "imageCarouselCard_1": //return new IndexPageHasEntitiesViewModel(jo, EntitiesType.Image_1);
                            case "imageCard": return new IndexPageHasEntitiesModel(json, EntityType.Image);
                            //case "apkImageCard":
                            case "configCard":
                                return json.TryGetValue("url", out JToken url) && url.ToString().Length >= 5
                                    ? new IndexPageHasEntitiesModel(json, EntityType.IconLink)
                                    : null;
                            case "iconLinkGridCard": return new IndexPageHasEntitiesModel(json, EntityType.IconLink);
                            case "feedGroupListCard":
                            case "feedListCard":
                            case "imageTextGridCard":
                            case "apkListCard":
                            case "textLinkListCard": return new IndexPageHasEntitiesModel(json, EntityType.TextLinks);
                            case "textCard":
                            case "messageCard": return new IndexPageMessageCardModel(json);
                            case "refreshCard": return new IndexPageOperationCardModel(json, OperationType.Refresh);
                            case "unLoginCard": return new IndexPageOperationCardModel(json, OperationType.Login);
                            case "titleCard": return new IndexPageOperationCardModel(json, OperationType.ShowTitle);
                            case "iconTabLinkGridCard": return new IndexPageHasEntitiesModel(json, EntityType.TabLink);
                            case "selectorLinkCard": return new IndexPageHasEntitiesModel(json, EntityType.SelectorLink);
                            default: return null;
                        }
                    }
                    return null;
            }
        }
    }
}
