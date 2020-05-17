using CoolapkUWP.Helpers;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using Windows.UI.Xaml.Media;

namespace CoolapkUWP.Controls.ViewModels
{
    class IndexPageViewModel : Entity
    {
        private ImageSource pic1;

        public IndexPageViewModel(JToken t) : base(t)
        {
            JObject token = t as JObject;
            if (token.TryGetValue("entityTemplate", out JToken v1))
                entityTemplate = v1.ToString();
            if (token.TryGetValue("title", out JToken v2))
            {
                hasTitle = !string.IsNullOrEmpty(v2.ToString());
                if (hasTitle)
                    title = v2.ToString();
            }
            if (token.TryGetValue("url", out JToken v3))
            {
                hasUrl = !string.IsNullOrEmpty(v3.ToString());
                if (hasUrl)
                    url = v3.ToString();
            }
            if (token.TryGetValue("description", out JToken v4))
            {
                hasDescription = !string.IsNullOrEmpty(v4.ToString());
                if (hasDescription)
                    description = v4.ToString();
            }
            if (token.TryGetValue("entities", out JToken v7))
            {
                hasEntities = (v7 as JArray).Count > 0;
                if (hasEntities)
                {
                    List<Entity> models = new List<Entity>();
                    foreach (var item in v7 as JArray)
                    {
                        JObject o = item as JObject;
                        if (o.Value<string>("entityType") == "feed")
                            models.Add(new FeedViewModel(item));
                        else if (o.Value<string>("entityType") == "user")
                            models.Add(new UserViewModel(item));
                        else
                            models.Add(new IndexPageViewModel(item));
                    }
                    entities = models.ToArray();
                }
            }
            GetPic(token);
        }

        private async void GetPic(JObject token)
        {
            if (token.TryGetValue("pic", out JToken v5))
            {
                hasPic = !string.IsNullOrEmpty(v5.ToString());
                if (hasPic)
                    pic = await ImageCacheHelper.GetImage(ImageType.Icon, v5.ToString());
            }
            else if (token.TryGetValue("logo", out JToken v6))
            {
                hasPic = !string.IsNullOrEmpty(v6.ToString());
                if (hasPic)
                    pic = await ImageCacheHelper.GetImage(ImageType.Icon, v6.ToString());
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
