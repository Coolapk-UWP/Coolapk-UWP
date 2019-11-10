using Windows.Data.Json;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace CoolapkUWP.Control.ViewModels
{
    class TopicViewModel : DyhViewModel
    {
        public TopicViewModel(IJsonValue t) : base(t)
        {
            JsonObject token = t.GetObject();
            commentnum = token["commentnum"].GetString();
        }
        public string commentnum { get; private set; }
    }

    class DyhViewModel:Entity
    {
        public DyhViewModel(IJsonValue t) : base(t)
        {
            JsonObject token = t.GetObject();
            url = token["url"].GetString();
            title = token["title"].GetString();
            follownum = token["follownum"].GetString();
            logo = new BitmapImage(new System.Uri(token["logo"].GetString()));
        }

        public string url { get; private set; }
        public string title { get; private set; }
        public string follownum { get; private set; }
        public ImageSource logo { get; private set; }
    }
}
