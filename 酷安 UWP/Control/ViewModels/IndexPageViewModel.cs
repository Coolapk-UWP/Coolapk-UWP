using CoolapkUWP.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.UI.Xaml.Media;

namespace CoolapkUWP.Control.ViewModels
{
    class IndexPageViewModel : Entity
    {
        private ImageSource pic1;

        public IndexPageViewModel(IJsonValue t) : base(t)
        {
            JsonObject token = t.GetObject();
            if (token.TryGetValue("entityTemplate", out IJsonValue v1))
                entityTemplate = v1.GetString();
            if (token.TryGetValue("title", out IJsonValue v2) && v2.ValueType == JsonValueType.String)
            {
                hasTitle = !string.IsNullOrEmpty(v2.GetString());
                if (hasTitle)
                    title = v2.GetString();
            }
            if (token.TryGetValue("url", out IJsonValue v3) && v3.ValueType == JsonValueType.String)
            {
                hasUrl = !string.IsNullOrEmpty(v3.GetString());
                if (hasUrl)
                    url = v3.GetString();
            }
            if (token.TryGetValue("description", out IJsonValue v4) && v4.ValueType == JsonValueType.String)
            {
                hasDescription = !string.IsNullOrEmpty(v4.GetString());
                if (hasDescription)
                    description = v4.GetString();
            }
            if (token.TryGetValue("entities", out IJsonValue v7) && v7.ValueType == JsonValueType.Array)
            {
                hasEntities = v7.GetArray().Count > 0;
                if (hasEntities)
                {
                    List<Entity> models = new List<Entity>();
                    foreach (var item in v7.GetArray())
                    {
                        JsonObject o = item.GetObject();
                        if (o["entityType"].GetString() == "feed")
                            models.Add(new FeedViewModel(item));
                        else if (o["entityType"].GetString() == "user")
                            models.Add(new UserViewModel(item));
                        else
                            models.Add(new IndexPageViewModel(item));
                    }
                    entities = models.ToArray();
                }
            }
            GetPic(token);
        }

        private async void GetPic(JsonObject token)
        {
            if (token.TryGetValue("pic", out IJsonValue v5) && v5.ValueType == JsonValueType.String)
            {
                hasPic = !string.IsNullOrEmpty(v5.GetString());
                if (hasPic)
                    pic = await ImageCache.GetImage(ImageType.Icon, v5.GetString());
            }
            else if (token.TryGetValue("logo", out IJsonValue v6) && v6.ValueType == JsonValueType.String)
            {
                hasPic = !string.IsNullOrEmpty(v6.GetString());
                if (hasPic)
                    pic = await ImageCache.GetImage(ImageType.Icon, v6.GetString());
            }
        }

        public string entityTemplate { get; private set; }
        public bool hasTitle { get; private set; }
        public string title { get; private set; }
        public bool hasUrl { get; private set; }
        public string url { get; private set; }
        public bool hasDescription { get; private set; }
        public string description { get; private set; }
        public bool hasPic { get; private set; }
        public ImageSource pic
        {
            get => pic1;
            private set
            {
                pic1 = value;
                Changed(this, nameof(pic));
            }
        }
        public bool hasEntities { get; private set; }
        public Entity[] entities { get; private set; }
        public IndexPageViewModel[] self { get => new IndexPageViewModel[] { this }; }
    }
}
