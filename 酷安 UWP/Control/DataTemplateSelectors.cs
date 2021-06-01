using CoolapkUWP.Control.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace CoolapkUWP.Control
{
    public class FirstTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Feed { get; set; }
        public DataTemplate Card { get; set; }
        public DataTemplate User { get; set; }
        public DataTemplate Topic { get; set; }
        public DataTemplate DYH { get; set; }
        protected override DataTemplate SelectTemplateCore(object item)
        {
            return item is FeedViewModel
                ? Feed
                : item is UserViewModel ? User : item is TopicViewModel ? Topic : item is DyhViewModel ? DYH : Card;
        }
        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container) => SelectTemplateCore(item);
    }
    public class SecondTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Null { get; set; }
        public DataTemplate DataTemplate1 { get; set; }
        public DataTemplate ImageTextScrollCard { get; set; }
        public DataTemplate MessageCard { get; set; }
        public DataTemplate RefreshCard { get; set; }
        public DataTemplate TextLinkListCard { get; set; }
        public DataTemplate IconLinkGridCard { get; set; }
        public DataTemplate IconTabLinkGridCard { get; set; }
        public DataTemplate SelectorLinkCard { get; set; }
        public DataTemplate ImageCard { get; set; }
        protected override DataTemplate SelectTemplateCore(object item)
        {
            switch ((item as IndexPageViewModel).entityTemplate)
            {
                default: return Null;
                case "imageTextGridCard":
                case "imageCarouselCard_1":
                case "imageSquareScrollCard":
                case "iconScrollCard":
                case "imageTextScrollCard":
                case "colorfulFatScrollCard":
                case "colorfulScrollCard":
                case "iconLongTitleGridCard":
                case "feedScrollCard": return ImageTextScrollCard;
                case "textCard":
                case "hot":
                case "messageCard": return MessageCard;
                case "refreshCard": return RefreshCard;
                case "feedGroupListCard":
                case "iconListCard":
                case "textLinkListCard": return TextLinkListCard;
                case "iconGridCard":
                case "iconMiniGridCard":
                case "iconMiniLinkGridCard":
                case "iconLinkGridCard": return IconLinkGridCard;
                case "iconTabLinkGridCard": return IconTabLinkGridCard;
                case "selectorLinkCard": return SelectorLinkCard;
                case "headCard":
                case "iconButtonGridCard":
                case "imageCard": return ImageCard;
            }
        }
        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container) => SelectTemplateCore(item);
    }
    public class ThirdTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Null { get; set; }
        public DataTemplate Image { get; set; }
        public DataTemplate IconLink { get; set; }
        public DataTemplate Feed { get; set; }
        public DataTemplate Dyh { get; set; }
        public DataTemplate Topic { get; set; }
        public DataTemplate FeedArticle { get; set; }
        public DataTemplate Question { get; set; }
        public DataTemplate TextLink { get; set; }
        public DataTemplate User { get; set; }
        public DataTemplate ImageSquare { get; set; }
        public DataTemplate TextImage { get; set; }
        public DataTemplate Histroy { get; set; }
        protected override DataTemplate SelectTemplateCore(object item)
        {
            if (item is FeedViewModel f)
            {
                return f.isQuestionFeed ? Question : f.showMessage_title ? FeedArticle : Feed;
            }
            else if (item is UserViewModel) { return User; }
            else
            {
                switch ((item as IndexPageViewModel).entityType)
                {
                    case "image_1": return Image;
                    case "icon":
                    case "iconMiniLink":
                    case "iconMini":
                    case "iconLink": return IconLink;
                    case "product":
                    case "recentHistory":
                    case "IconLink":
                    case "apk":
                    case "appForum":
                    case "dyh": return Dyh;
                    case "picCategory":
                    case "entity":
                    case "topic": return Topic;
                    case "textLink": return TextLink;
                    case "imageSquare": return ImageSquare;
                    case "imageText": return TextImage;
                    case "collection":
                    case "history": return Histroy;
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
