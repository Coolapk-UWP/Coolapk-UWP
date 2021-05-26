using CoolapkUWP.Models;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace CoolapkUWP.Controls
{
    public class FirstTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Feed { get; set; }
        public DataTemplate Others { get; set; }
        public DataTemplate User { get; set; }
        public DataTemplate Topic { get; set; }
        public DataTemplate Dyh { get; set; }
        public DataTemplate Product { get; set; }
        public DataTemplate CoolPic { get; set; }
        public DataTemplate MessageCard { get; set; }
        public DataTemplate Images { get; set; }
        public DataTemplate TitleCard { get; set; }
        public DataTemplate LoginCard { get; set; }
        public DataTemplate RefreshCard { get; set; }
        public DataTemplate TextLinkList { get; set; }
        public DataTemplate IconLinks { get; set; }
        public DataTemplate ImageTextScrollCard { get; set; }
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

    public class ThirdTemplateSelector : DataTemplateSelector
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
        public DataTemplate Feed { get; set; }
        public DataTemplate User { get; set; }
        public DataTemplate CoolPic { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            if (item is SourceFeedModel f)
            {
                return f.IsQuestionFeed ? QuestionFeed : f.ShowMessageTitle ? FeedImageText : f.IsCoolPictuers ? CoolPic : Feed;
            }
            else if (item is UserModel) { return User; }
            else
            {
                switch ((item as IndexPageModel)?.EntityType ?? string.Empty)
                {
                    case "imageSquare":
                    case "icon":
                    case "iconMiniLink":
                    case "iconMini":
                    case "IconLink": return IconLink;
                    case "dyh":
                    case "product": return ScrollLinkCard;
                    case "picCategory":
                    case "entity":
                    case "topic": return SquareLinkCard;
                    case "textLink": return TextLink;
                    case "imageText": return ImageText;
                }
            }

            return Null;
        }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container) => SelectTemplateCore(item);
    }
}