using CoolapkUWP.Helpers;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using Windows.UI.Xaml.Media;

namespace CoolapkUWP.Models
{
    internal interface IHasUriAndTitle
    {
        string Url { get; }
        string Title { get; }
    }

    internal class IndexPageModel : Entity, IHasUriAndTitle
    {
        public IndexPageModel(JObject token) : base(token)
        {
            if (token.TryGetValue("entityTemplate", out JToken v1))
            {
                EntityTemplate = v1.ToString();
            }
            if (token.TryGetValue("title", out JToken v2) && !string.IsNullOrEmpty(v2.ToString()))
            {
                Title = v2.ToString();
            }
            if (token.TryGetValue("url", out JToken v3) && !string.IsNullOrEmpty(v3.ToString()))
            {
                Url = v3.ToString();
            }
            if (token.TryGetValue("description", out JToken v4) && !string.IsNullOrEmpty(v4.ToString()))
            {
                Description = v4.ToString();
            }
            if (token.TryGetValue("pic", out JToken v5) && !string.IsNullOrEmpty(v5.ToString()))
            {
                Pic = new ImageModel(v5.ToString(), ImageType.OriginImage);
            }
            else if (token.TryGetValue("logo", out JToken v6) && !string.IsNullOrEmpty(v6.ToString()))
            {
                Pic = new ImageModel(v6.ToString(), ImageType.Icon);
            }
        }

        public string EntityTemplate { get; private set; }
        public string Title { get; private set; }
        public string Url { get; private set; }
        public string Description { get; private set; }
        public ImageModel Pic { get; private set; }
    }

    internal class IndexPageMessageCardModel : Entity
    {
        public IndexPageMessageCardModel(JObject token) : base(token)
        {
            if (token.TryGetValue("description", out JToken v4))
                Description = v4.ToString();
        }

        public string Description { get; private set; }
    }

    internal enum EntitiesType
    {
        Image,
        TabLink,
        SelectorLink,
        IconLink,
        TextLink,
        Others,
    }

    internal class IndexPageHasEntitiesModel : Entity, IHasUriAndTitle
    {
        public IndexPageHasEntitiesModel(JObject token, EntitiesType type) : base(token)
        {
            EntitiesType = type;
            if (token.TryGetValue("title", out JToken v2) && !string.IsNullOrEmpty(v2.ToString()))
            {
                Title = v2.ToString();
            }
            if (token.TryGetValue("url", out JToken v3) && !string.IsNullOrEmpty(v3.ToString()))
            {
                Url = v3.ToString();
            }
            if (token.TryGetValue("description", out JToken v4) && !string.IsNullOrEmpty(v4.ToString()))
            {
                Description = v4.ToString();
            }
            if (token.TryGetValue("entities", out JToken v7) && (v7 as JArray).Count > 0)
            {
                List<Entity> models = new List<Entity>();
                foreach (JObject item in v7 as JArray)
                {
                    if (item.Value<string>("entityType") == "feed")
                        models.Add(new FeedModel(item));
                    else if (item.Value<string>("entityType") == "user")
                        models.Add(new UserModel(item));
                    else
                        models.Add(new IndexPageModel(item));
                }
                Entities = models.ToArray();
            }
        }

        public EntitiesType EntitiesType { get; private set; }
        public string Title { get; private set; }
        public string Url { get; private set; }
        public string Description { get; private set; }
        public Entity[] Entities { get; private set; }
    }

    internal enum OperationType
    {
        Refresh,
        Login,
        ShowTitle,
    }

    internal class IndexPageOperationCardModel : Entity, IHasUriAndTitle
    {
        public IndexPageOperationCardModel(JObject token, OperationType type) : base(token)
        {
            OperationType = type;
            if (token.TryGetValue("title", out JToken v2) && !string.IsNullOrEmpty(v2.ToString()))
            {
                Title = v2.ToString();
            }
            if (token.TryGetValue("url", out JToken v3) && !string.IsNullOrEmpty(v3.ToString()))
            {
                Url = v3.ToString();
            }
        }

        public OperationType OperationType { get; private set; }
        public string EntityTemplate { get; private set; }
        public string Title { get; private set; }
        public string Url { get; private set; }
    }
}