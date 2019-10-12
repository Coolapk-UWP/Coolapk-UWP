using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace 酷安_UWP
{
    public class FirstTemplateSelector : DataTemplateSelector
    {
        public DataTemplate DataTemplate0 { get; set; }
        public DataTemplate DataTemplate1 { get; set; }
        public DataTemplate DataTemplate2 { get; set; }
        protected override DataTemplate SelectTemplateCore(object item)
        {
            Feed feed = item as Feed;
            switch (feed.GetValue("entityType"))
            {
                case "feed":
                    return DataTemplate1;
                case "card":
                    return DataTemplate2;
                default:
                    return DataTemplate0;
            }
        }
    }
    public class SecondTemplateSelector : DataTemplateSelector
    {
        public DataTemplate DataTemplate0 { get; set; }
        public DataTemplate DataTemplate1 { get; set; }
        public DataTemplate DataTemplate2 { get; set; }
        public DataTemplate DataTemplate3 { get; set; }
        protected override DataTemplate SelectTemplateCore(object item)
        {
            Feed feed = item as Feed;
            switch (feed.GetValue("entityTemplate"))
            {
                case "imageCarouselCard_1":
                    return DataTemplate1;
                case "iconMiniGridCard":
                case "iconLinkGridCard":
                    return DataTemplate2;
                case "messageCard":
                    return DataTemplate3;
                default:
                    return DataTemplate0;
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
        protected override DataTemplate SelectTemplateCore(object item)
        {
            Feed feed = item as Feed;
            switch (feed.GetValue("entityType"))
            {
                case "image_1":
                    return DataTemplate1;
                case "iconLink":
                    return DataTemplate2;
                case "dyh":
                    return DataTemplate3;
                case "topic":
                    return DataTemplate4;
                default:
                    return DataTemplate0;
            }
        }
    }

}
