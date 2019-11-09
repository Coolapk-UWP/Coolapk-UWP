using Windows.Data.Json;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace CoolapkUWP.Control.ViewModels
{
    public class TopicViewModel : DyhViewModel
    {
        public TopicViewModel(IJsonValue t) : base(t)
        {
            JsonObject token = t.GetObject();
            commentnum = token["commentnum"].GetString();
        }
        public string commentnum { get; private set; }
    }

    public class DyhViewModel:IEntity
    {
        public DyhViewModel(IJsonValue t)
        {
            JsonObject token = t.GetObject();
            url = token["url"].GetString();
            title = token["title"].GetString();
            follownum = token["follownum"].GetString();
            logo = new BitmapImage(new System.Uri(token["logo"].GetString()));
            if (token.TryGetValue("entityId", out IJsonValue value1)) entityId = value1.ToString().Replace("\"", string.Empty);
            entityType = token["entityType"].GetString();
            if (token.TryGetValue("entityFixed", out IJsonValue value) && value.GetNumber() == 1) entityFixed = true;
        }

        public string url { get; private set; }
        public string title { get; private set; }
        public string follownum { get; private set; }
        public ImageSource logo { get; private set; }
        public string entityId { get; private set; }
        public string entityType { get; private set; }
        public bool entityFixed { get; private set; }
    }
}
