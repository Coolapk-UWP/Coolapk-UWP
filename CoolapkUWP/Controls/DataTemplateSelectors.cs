using CoolapkUWP.Controls.ViewModels;
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
        public DataTemplate CoolPic { get; set; }
        public DataTemplate MessageCard { get; set; }
        public DataTemplate Images { get; set; }
        public DataTemplate TitleCard { get; set; }
        public DataTemplate LoginCard { get; set; }
        public DataTemplate RefreshCard { get; set; }
        public DataTemplate TextLinkList { get; set; }
        public DataTemplate IconLinks { get; set; }
        public DataTemplate DataTemplate2 { get; set; }
        public DataTemplate TabLinkCard { get; set; }
        public DataTemplate SelectorLinkCard { get; set; }
        protected override DataTemplate SelectTemplateCore(object item)
        {
            if (item is FeedViewModel f)
            {
                if (f.IsCoolPictuers) return CoolPic;
                else return Feed;
            }
            else if (item is UserViewModel) return User;
            else if (item is TopicViewModel) return Topic;
            else if (item is DyhViewModel) return Dyh;
            else if (item is IndexPageMessageCardViewModel) return MessageCard;
            else if (item is IndexPageHasEntitiesViewModel m)
            {
                switch (m.EntitiesType)
                {
                    case EntitiesType.TabLink: return TabLinkCard;
                    case EntitiesType.SelectorLink: return SelectorLinkCard;
                    case EntitiesType.Image: return Images;
                    case EntitiesType.IconLink: return IconLinks;
                    case EntitiesType.TextLink: return TextLinkList;
                    case EntitiesType.Others:
                    default: return DataTemplate2;
                }
            }
            else if (item is IndexPageOperationCardViewModel o)
            {
                switch (o.OperationType)
                {
                    case OperationType.Refresh: return RefreshCard;
                    case OperationType.Login: return LoginCard;
                    case OperationType.ShowTitle: return TitleCard;
                    default: return Others;
                }
            }
            else return Others;
        }
        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container) => SelectTemplateCore(item);
    }
    public class ThirdTemplateSelector : DataTemplateSelector
    {
        public DataTemplate DataTemplate0 { get; set; }
        public DataTemplate DataTemplate1 { get; set; }
        public DataTemplate DataTemplate2 { get; set; }
        public DataTemplate DataTemplate4 { get; set; }
        public DataTemplate DataTemplate5 { get; set; }
        public DataTemplate QuestionFeed { get; set; }
        public DataTemplate DataTemplate7 { get; set; }
        public DataTemplate Feed { get; set; }
        public DataTemplate User { get; set; }
        public DataTemplate CoolPic { get; set; }
        protected override DataTemplate SelectTemplateCore(object item)
        {
            if (item is FeedViewModel f)
            {
                if (f.IsQuestionFeed) return QuestionFeed;
                else if (f.ShowMessage_title) return DataTemplate5;
                else if (f.IsCoolPictuers) return CoolPic;
                else return Feed;
            }
            else if (item is UserViewModel) return User;
            else switch ((item as IndexPageViewModel).EntityType)
                {
                    case "picCategory":
                    case "imageSquare":
                    case "product":
                    case "icon":
                    case "iconMiniLink":
                    case "iconLink": return DataTemplate2;
                    case "dyh":
                    case "topic": return DataTemplate4;
                    case "textLink": return DataTemplate7;
                }
            return DataTemplate0;
        }
        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container) => SelectTemplateCore(item);
    }
}
