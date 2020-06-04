using CoolapkUWP.Helpers;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using Windows.UI.Xaml.Media;

namespace CoolapkUWP.Controls.ViewModels
{
    interface IHasUriAndTitle
    {
        bool HasUrl { get; }
        string Url { get; }
        bool HasTitle { get; }
        string Title { get; }
    }
    class IndexPageViewModel : Entity, IHasUriAndTitle
    {
        private ImageSource pic1;

        public IndexPageViewModel(JObject token) : base(token)
        {
            if (token.TryGetValue("entityTemplate", out JToken v1))
                EntityTemplate = v1.ToString();
            if (token.TryGetValue("title", out JToken v2))
            {
                HasTitle = !string.IsNullOrEmpty(v2.ToString());
                if (HasTitle)
                    Title = v2.ToString();
            }
            if (token.TryGetValue("url", out JToken v3))
            {
                HasUrl = !string.IsNullOrEmpty(v3.ToString());
                if (HasUrl)
                    Url = v3.ToString();
            }
            if (token.TryGetValue("description", out JToken v4))
            {
                HasDescription = !string.IsNullOrEmpty(v4.ToString());
                if (HasDescription)
                    Description = v4.ToString();
            }
            GetPic(token);
        }

        private async void GetPic(JObject token)
        {
            if (token.TryGetValue("pic", out JToken v5))
            {
                HasPic = !string.IsNullOrEmpty(v5.ToString());
                if (HasPic)
                    Pic = await ImageCacheHelper.GetImage(ImageType.OriginImage, v5.ToString());
            }
            else if (token.TryGetValue("logo", out JToken v6))
            {
                HasPic = !string.IsNullOrEmpty(v6.ToString());
                if (HasPic)
                    Pic = await ImageCacheHelper.GetImage(ImageType.Icon, v6.ToString());
            }
        }

        public string EntityTemplate { get; private set; }
        public bool HasTitle { get; private set; }
        public string Title { get; private set; }
        public bool HasUrl { get; private set; }
        public string Url { get; private set; }
        public bool HasDescription { get; private set; }
        public string Description { get; private set; }
        public bool HasPic { get; private set; }
        public ImageSource Pic
        {
            get => pic1;
            private set
            {
                pic1 = value;
                Changed(this, nameof(Pic));
            }
        }
    }

    class IndexPageMessageCardViewModel : Entity
    {
        public IndexPageMessageCardViewModel(JObject token) : base(token)
        {
            if (token.TryGetValue("description", out JToken v4))
                Description = v4.ToString();
        }
        public string Description { get; private set; }
    }

    enum EntitiesType
    {
        Image, 
        TabLink,
        SelectorLink,
        IconLink,
        TextLink,
        Others,
    }

    class IndexPageHasEntitiesViewModel : Entity, IHasUriAndTitle
    {
        public IndexPageHasEntitiesViewModel(JObject token, EntitiesType type) : base(token)
        {
            EntitiesType = type;
            if (token.TryGetValue("title", out JToken v2))
            {
                HasTitle = !string.IsNullOrEmpty(v2.ToString());
                if (HasTitle)
                    Title = v2.ToString();
            }
            if (token.TryGetValue("url", out JToken v3))
            {
                HasUrl = !string.IsNullOrEmpty(v3.ToString());
                if (HasUrl)
                    Url = v3.ToString();
            }
            if (token.TryGetValue("description", out JToken v4))
            {
                HasDescription = !string.IsNullOrEmpty(v4.ToString());
                if (HasDescription)
                    Description = v4.ToString();
            }
            if (token.TryGetValue("entities", out JToken v7))
            {
                HasEntities = (v7 as JArray).Count > 0;
                if (HasEntities)
                {
                    List<Entity> models = new List<Entity>();
                    foreach (JObject item in v7 as JArray)
                    {
                        if (item.Value<string>("entityType") == "feed")
                            models.Add(new FeedViewModel(item));
                        else if (item.Value<string>("entityType") == "user")
                            models.Add(new UserViewModel(item));
                        else models.Add(new IndexPageViewModel(item));
                    }
                    Entities = models.ToArray();
                }
            }
        }

        public EntitiesType EntitiesType { get; private set; }
        public bool HasTitle { get; private set; }
        public string Title { get; private set; }
        public bool HasUrl { get; private set; }
        public string Url { get; private set; }
        public bool HasDescription { get; private set; }
        public string Description { get; private set; }
        public bool HasEntities { get; private set; }
        public Entity[] Entities { get; private set; }
    }

    enum OperationType
    {
        Refresh,
        Login,
        ShowTitle,
    }
    class IndexPageOperationCardViewModel : Entity, IHasUriAndTitle
    {
        public IndexPageOperationCardViewModel(JObject token, OperationType type) : base(token)
        {
            OperationType = type;
            if (token.TryGetValue("title", out JToken v2))
            {
                HasTitle = !string.IsNullOrEmpty(v2.ToString());
                if (HasTitle)
                    Title = v2.ToString();
            }
            if (token.TryGetValue("url", out JToken v3))
            {
                HasUrl = !string.IsNullOrEmpty(v3.ToString());
                if (HasUrl)
                    Url = v3.ToString();
            }
        }
        public OperationType OperationType { get; private set; }
        public string EntityTemplate { get; private set; }
        public bool HasTitle { get; private set; }
        public string Title { get; private set; }
        public bool HasUrl { get; private set; }
        public string Url { get; private set; }
    }
}
