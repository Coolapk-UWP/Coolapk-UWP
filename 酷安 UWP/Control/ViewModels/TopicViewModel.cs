using CoolapkUWP.Data;
using Windows.Data.Json;
using Windows.UI.Xaml.Media;

namespace CoolapkUWP.Control.ViewModels
{
    class TopicViewModel : DyhViewModel
    {
        public TopicViewModel(IJsonValue t) : base(t)
        {
            JsonObject token = t.GetObject();
            commentnum = token["commentnum"].ToString().Replace("\"", string.Empty);
        }
        public string commentnum { get; private set; }
    }

    class DyhViewModel : Entity
    {
        public DyhViewModel(IJsonValue t) : base(t)
        {
            JsonObject token = t.GetObject();
            url = token["url"].GetString();
            title = token["title"].GetString();
            follownum = token["follownum"].ToString().Replace("\"", string.Empty);
            GetPic(token);
        }
        async void GetPic(JsonObject token) => logo = await ImageCache.GetImage(ImageType.Icon, token["logo"].GetString());
        public string url { get; private set; }
        public string title { get; private set; }
        public string follownum { get; private set; }
        public ImageSource logo { get; private set; }
    }
}
