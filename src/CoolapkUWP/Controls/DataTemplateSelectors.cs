using CoolapkUWP.Core.Models;
using CoolapkUWP.Models;
using CoolapkUWP.Models.Pages.NotificationsPageModels;
using Newtonsoft.Json.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace CoolapkUWP.Controls
{
    public sealed class CardTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Feed { get; set; }
        public DataTemplate Others { get; set; }
        public DataTemplate User { get; set; }
        public DataTemplate Topic { get; set; }
        public DataTemplate Dyh { get; set; }
        public DataTemplate App { get; set; }
        public DataTemplate List { get; set; }
        public DataTemplate Product { get; set; }
        public DataTemplate CoolPic { get; set; }
        public DataTemplate LiveTopic { get; set; }
        public DataTemplate MessageCard { get; set; }
        public DataTemplate Images { get; set; }
        public DataTemplate TitleCard { get; set; }
        public DataTemplate LoginCard { get; set; }
        public DataTemplate RefreshCard { get; set; }
        public DataTemplate TextLinkList { get; set; }
        public DataTemplate IconLinks { get; set; }
        public DataTemplate ImageTextScrollCard { get; set; }
        public DataTemplate ScrollCardWithBackColor { get; set; }
        public DataTemplate TabLinkCard { get; set; }
        public DataTemplate SelectorLinkCard { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            if (item is FeedModel f)
            {
                return f.IsCoolPictuers ? CoolPic : Feed;
            }
            else if (item is UserModel) { return User; }
            else if (item is TopicModel) { return Topic; }
            else if (item is DyhModel) { return Dyh; }
            else if (item is ProductModel) { return Product; }
            else if (item is LiveMode) { return LiveTopic; }
            else if (item is AppPageMode) { return App; }
            else if (item is ListModel) { return List; }
            else if (item is IndexPageMessageCardModel) { return MessageCard; }
            else if (item is IndexPageHasEntitiesModel m)
            {
                switch (m.EntitiesType)
                {
                    case EntityType.TabLink: return TabLinkCard;
                    case EntityType.SelectorLink: return SelectorLinkCard;
                    case EntityType.Image: return Images;
                    case EntityType.IconLink: return IconLinks;
                    case EntityType.TextLinks: return TextLinkList;
                    case EntityType.ScrollLink: return ScrollCardWithBackColor;
                    case EntityType.Others:
                    default: return ImageTextScrollCard;
                }
            }
            else if (item is IndexPageOperationCardModel o)
            {
                switch (o.OperationType)
                {
                    case OperationType.Refresh: return RefreshCard;
                    case OperationType.Login: return LoginCard;
                    case OperationType.ShowTitle: return TitleCard;
                    default: return Others;
                }
            }
            else { return Others; }
        }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container) => SelectTemplateCore(item);
    }

    public sealed class ItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Null { get; set; }
        public DataTemplate DataTemplate1 { get; set; }
        public DataTemplate IconLink { get; set; }
        public DataTemplate SquareLinkCard { get; set; }
        public DataTemplate FeedImageText { get; set; }
        public DataTemplate ImageText { get; set; }
        public DataTemplate QuestionFeed { get; set; }
        public DataTemplate TextLink { get; set; }
        public DataTemplate ScrollLinkCard { get; set; }
        public DataTemplate IconMiniLink { get; set; }
        public DataTemplate ImageIcon { get; set; }
        public DataTemplate Feed { get; set; }
        public DataTemplate User { get; set; }
        public DataTemplate Text { get; set; }
        public DataTemplate Link { get; set; }
        public DataTemplate CoolPic { get; set; }
        public DataTemplate Histroy { get; set; }
        public DataTemplate List { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            if (item is SourceFeedModel f)
            {
                return f.IsQuestionFeed ? QuestionFeed : f.ShowMessageTitle ? FeedImageText : f.IsCoolPictuers ? CoolPic : Feed;
            }
            else if (item is UserModel) { return User; }
            else if (item is IndexPageModel)
            {
                switch ((item as IndexPageModel)?.EntityType ?? string.Empty)
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
                            case "iconMiniLinkGridCard":
                            case "iconMiniGridCard": return IconMiniLink;
                            case "imageSquareScrollCard": return SquareLinkCard;
                            case "colorfulFatScrollCard": return ScrollLinkCard;
                            case "apkListCard":
                            case "feedListCard": return List;
                            default: return IconLink;
                        }
                    case "iconButton":
                    case "link": return Link;
                    case "textLink": return TextLink;
                    case "image": return ImageIcon;
                    case "imageText": return ImageText;
                    case "collection":
                    case "history": return Histroy;
                    case "hot": return Text;
                    default: return Null;
                }
            }

            return Null;
        }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container) => SelectTemplateCore(item);
    }

    internal class NotificationsTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Feed { get; set; }
        public DataTemplate Reply { get; set; }
        public DataTemplate Like { get; set; }
        public DataTemplate AtCommentMe { get; set; }
        public DataTemplate Message { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            switch (item)
            {
                case FeedModel _: return Feed;
                case LikeNotificationModel _: return Like;
                case AtCommentMeNotificationModel _: return AtCommentMe;
                case MessageNotificationModel _: return Message;
                default: return Reply;
            }
        }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container) => SelectTemplateCore(item);
    }

    public sealed class SearchTemplateSelector : DataTemplateSelector
    {
        public DataTemplate App { get; set; }
        public DataTemplate SearchWord { get; set; }
        protected override DataTemplate SelectTemplateCore(object item)
        {
            if (item is AppPageMode) return App;
            return SearchWord;
        }
        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container) => SelectTemplateCore(item);
    }

    public static class EntityTemplateSelector
    {
        public static Entity GetEntity(JObject jo, bool isHotFeedPage = false)
        {
            switch (jo.Value<string>("entityType"))
            {
                case "feed":
                case "discovery": return new FeedModel(jo, isHotFeedPage ? FeedDisplayMode.isFirstPageFeed : FeedDisplayMode.normal);
                case "liveTopic": return new LiveMode(jo);
                case "user": return new UserModel(jo);
                case "topic": return new TopicModel(jo);
                case "dyh": return new DyhModel(jo);
                case "apk":
                case "appForum": return new AppPageMode(jo);
                case "product": return new ProductModel(jo);
                case "productBrand": return new ListModel(jo);
                case "entity_type_user_card_manager": return new IndexPageOperationCardModel(jo, OperationType.ShowTitle);
                default:
                    if (jo.TryGetValue("entityTemplate", out JToken v1))
                    {
                        switch (v1.Value<string>())
                        {
                            case "feed": return new FeedModel(jo, isHotFeedPage ? FeedDisplayMode.isFirstPageFeed : FeedDisplayMode.normal);
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
                            case "gridCard": return new IndexPageHasEntitiesModel(jo, EntityType.Others);
                            case "iconMiniLinkGridCard":
                            case "iconMiniGridCard": return new IndexPageHasEntitiesModel(jo, EntityType.ScrollLink);
                            //case "listCard": //return new IndexPageHasEntitiesModel(jo, EntityType.Others);
                            case "headCard":
                            case "imageCarouselCard_1": //return new IndexPageHasEntitiesViewModel(jo, EntitiesType.Image_1);
                            case "imageCard": return new IndexPageHasEntitiesModel(jo, EntityType.Image);
                            //case "apkImageCard":
                            case "configCard":
                                return jo.TryGetValue("url", out JToken v2) && v2.ToString().Length >= 5
                                    ? new IndexPageHasEntitiesModel(jo, EntityType.IconLink)
                                    : null;
                            case "iconLinkGridCard": return new IndexPageHasEntitiesModel(jo, EntityType.IconLink);
                            case "feedGroupListCard":
                            case "feedListCard":
                            case "imageTextGridCard":
                            case "apkListCard":
                            case "textLinkListCard": return new IndexPageHasEntitiesModel(jo, EntityType.TextLinks);
                            case "textCard":
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
    }
}