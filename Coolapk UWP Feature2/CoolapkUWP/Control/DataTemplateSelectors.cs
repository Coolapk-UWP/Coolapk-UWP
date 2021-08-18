using CoolapkUWP.Control.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace CoolapkUWP.Control
{
    public class FirstTemplateSelector : DataTemplateSelector
    {
        public DataTemplate DYH { get; set; }
        public DataTemplate Feed { get; set; }
        public DataTemplate Card { get; set; }
        public DataTemplate User { get; set; }
        public DataTemplate Topic { get; set; }
        public DataTemplate Product { get; set; }
        protected override DataTemplate SelectTemplateCore(object item)
        {
            return item is FeedViewModel ? Feed : item is UserViewModel ? User : item is TopicViewModel ? Topic : item is DyhViewModel ? DYH : item is ProductViewModel ? Product : Card;
        }
        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container) => SelectTemplateCore(item);
    }
    public class SecondTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Null { get; set; }
        public DataTemplate ImageCard { get; set; }
        public DataTemplate MessageCard { get; set; }
        public DataTemplate RefreshCard { get; set; }
        public DataTemplate DataTemplate1 { get; set; }
        public DataTemplate TextLinkListCard { get; set; }
        public DataTemplate IconLinkGridCard { get; set; }
        public DataTemplate SelectorLinkCard { get; set; }
        public DataTemplate IconTabLinkGridCard { get; set; }
        public DataTemplate ImageTextScrollCard { get; set; }
        protected override DataTemplate SelectTemplateCore(object item)
        {
            switch ((item as IndexPageViewModel).entityTemplate)
            {
                case "hot":
                case "textCard":
                case "messageCard": return MessageCard;
                case "refreshCard": return RefreshCard;
                case "headCard":
                case "imageCard":
                case "iconButtonGridCard":
                case "imageCarouselCard_1": return ImageCard;
                case "selectorLinkCard": return SelectorLinkCard;
                case "iconGridCard":
                case "iconMiniGridCard":
                case "iconLinkGridCard":
                case "iconMiniLinkGridCard": return IconLinkGridCard;
                case "iconTabLinkGridCard": return IconTabLinkGridCard;
                case "iconListCard":
                case "textLinkListCard":
                case "feedGroupListCard":
                case "feedCoolPictureGridCard": return TextLinkListCard;
                case "iconScrollCard":
                case "feedScrollCard":
                case "imageTextGridCard":
                case "colorfulScrollCard":
                case "imageTextScrollCard":
                case "colorfulFatScrollCard":
                case "imageSquareScrollCard":
                case "iconLongTitleGridCard": return ImageTextScrollCard;
                default: return Null;
            }
        }
        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container) => SelectTemplateCore(item);
    }
    public class ThirdTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Dyh { get; set; }
        public DataTemplate Null { get; set; }
        public DataTemplate User { get; set; }
        public DataTemplate Feed { get; set; }
        public DataTemplate Image { get; set; }
        public DataTemplate Topic { get; set; }
        public DataTemplate Histroy { get; set; }
        public DataTemplate Question { get; set; }
        public DataTemplate TextLink { get; set; }
        public DataTemplate IconLink { get; set; }
        public DataTemplate TextImage { get; set; }
        public DataTemplate FeedArticle { get; set; }
        public DataTemplate ImageSquare { get; set; }
        protected override DataTemplate SelectTemplateCore(object item)
        {
            if (item is FeedViewModel f)
            {
                return f.isQuestionFeed ? Question : f.showMessage_title ? FeedArticle : Feed;
            }
            else if (item is UserViewModel) { return User; }
            else
            {
                switch ((item as IndexPageViewModel).EntityType)
                {
                    case "image_1": return Image;
                    case "textLink": return TextLink;
                    case "dyh":
                    case "apk":
                    case "product":
                    case "appForum":
                    case "IconLink":
                    case "recentHistory": return Dyh;
                    case "topic":
                    case "entity":
                    case "picCategory": return Topic;
                    case "history":
                    case "collection": return Histroy;
                    case "imageText": return TextImage;
                    case "icon":
                    case "iconMini":
                    case "iconLink":
                    case "iconMiniLink": return IconLink;
                    case "imageSquare": return ImageSquare;
                    default: return Null;
                }
            }
        }
        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container) => SelectTemplateCore(item);
    }
    public class SearchPageTemplateSelector : DataTemplateSelector
    {
        public DataTemplate App { get; set; }
        public DataTemplate SearchWord { get; set; }
        protected override DataTemplate SelectTemplateCore(object item)
        {
            return item is AppViewModel ? App : SearchWord;
        }
        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container) => SelectTemplateCore(item);
    }
}
