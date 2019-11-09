using CoolapkUWP.Control.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace CoolapkUWP.Control
{
    public class TemplateSelector : DataTemplateSelector
    {
        public DataTemplate DataTemplate1 { get; set; }
        public DataTemplate DataTemplate2 { get; set; }
        public DataTemplate DataTemplate3 { get; set; }
        public DataTemplate DataTemplate4 { get; set; }
        public DataTemplate DataTemplate5 { get; set; }
        protected override DataTemplate SelectTemplateCore(object item)
        {
            Feed feed = item as Feed;
            if (feed.GetValue("entityType") == "feed_reply") return DataTemplate2;
            else if (feed.GetValue("entityType") == "article") return DataTemplate5;
            switch (feed.GetValue("feedType"))
            {
                case "feed": return DataTemplate1;
                case "feedArticle": return DataTemplate3;
                case "question": return DataTemplate4;
                default: return DataTemplate1;
            }
        }
    }

    public class FirstTemplateSelector : DataTemplateSelector
    {
        public DataTemplate DataTemplate1 { get; set; }
        public DataTemplate DataTemplate2 { get; set; }
        public DataTemplate DataTemplate3 { get; set; }
        public DataTemplate DataTemplate4 { get; set; }
        public DataTemplate DataTemplate5 { get; set; }
        protected override DataTemplate SelectTemplateCore(object item)
        {
            if (item is FeedViewModel) return DataTemplate1;
            else if (item is UserViewModel) return DataTemplate3;
            else if (item is TopicViewModel) return DataTemplate4;
            else if (item is DyhViewModel) return DataTemplate5;
            return DataTemplate2;
        }
    }
    public class SecondTemplateSelector : DataTemplateSelector
    {
        public DataTemplate DataTemplate0 { get; set; }
        public DataTemplate DataTemplate1 { get; set; }
        public DataTemplate DataTemplate2 { get; set; }
        public DataTemplate DataTemplate3 { get; set; }
        public DataTemplate DataTemplate4 { get; set; }
        public DataTemplate DataTemplate5 { get; set; }
        public DataTemplate DataTemplate6 { get; set; }
        public DataTemplate DataTemplate7 { get; set; }
        public DataTemplate DataTemplate8 { get; set; }
        public DataTemplate DataTemplate9 { get; set; }
        protected override DataTemplate SelectTemplateCore(object item)
        {
            Feed feed = item as Feed;
            switch (feed.GetValue("entityTemplate"))
            {
                case "selectorLinkCard": return DataTemplate8;
                case "imageCard": return DataTemplate9;
                case "imageCarouselCard_1": return DataTemplate1;
                case "iconTabLinkGridCard": return DataTemplate7;
                case "iconGridCard":
                case "iconMiniGridCard":
                case "iconMiniLinkGridCard":
                case "iconLinkGridCard": return DataTemplate6;
                case "imageSquareScrollCard":
                case "iconScrollCard":
                case "imageTextScrollCard":
                case "feedScrollCard": return DataTemplate2;
                case "textCard":
                case "messageCard": return DataTemplate3;
                case "refreshCard": return DataTemplate4;
                case "textLinkListCard": return DataTemplate5;
                default: return DataTemplate0;
            }
        }
    }
    public class ThirdTemplateSelector : DataTemplateSelector
    {
        public DataTemplate DataTemplate0 { get; set; }
        public DataTemplate DataTemplate1 { get; set; }
        public DataTemplate DataTemplate2 { get; set; }
        public DataTemplate DataTemplate3 { get; set; }
        public DataTemplate DataTemplate4 { get; set; }
        public DataTemplate DataTemplate5 { get; set; }
        public DataTemplate DataTemplate6 { get; set; }
        public DataTemplate DataTemplate7 { get; set; }
        public DataTemplate DataTemplate8 { get; set; }
        public DataTemplate DataTemplate9 { get; set; }
        protected override DataTemplate SelectTemplateCore(object item)
        {
            Feed feed = item as Feed;
            switch (feed.GetValue("entityType"))
            {
                case "image_1": return DataTemplate1;
                case "icon":
                case "iconMiniLink":
                case "iconLink": return DataTemplate2;
                case "dyh": return DataTemplate3;
                case "topic": return DataTemplate4;
                case "feed":
                    if (feed.GetValue("feedType") == "feedArticle") return DataTemplate5;
                    else if (feed.GetValue("feedType") == "question") return DataTemplate6;
                    else return DataTemplate0;
                case "textLink": return DataTemplate7;
                case "user": return DataTemplate8;
                case "imageSquare": return DataTemplate9;
                default: return DataTemplate0;
            }
        }
    }
    public class FeedTemplateSelector : DataTemplateSelector
    {
        public DataTemplate DataTemplate1 { get; set; }
        public DataTemplate DataTemplate2 { get; set; }
        protected override DataTemplate SelectTemplateCore(object item)
        {
            Feed feed = item as Feed;
            switch (feed.GetValue("feedType"))
            {
                case "feed": return DataTemplate1;
                case "feedArticle":
                case "answer":
                case "question":
                    return DataTemplate2;
                default: return DataTemplate1;
            }
        }
    }
    public class SearchPageTemplateSelector : DataTemplateSelector
    {
        public DataTemplate DataTemplate1 { get; set; }
        public DataTemplate DataTemplate2 { get; set; }
        protected override DataTemplate SelectTemplateCore(object item)
        {
            if (item is AppViewModel) return DataTemplate1;
            return DataTemplate2;
        }
    }
}
