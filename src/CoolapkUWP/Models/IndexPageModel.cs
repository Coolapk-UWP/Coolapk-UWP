using CoolapkUWP.Core.Models;
using CoolapkUWP.Helpers;
using Newtonsoft.Json.Linq;
using System.Collections.Immutable;

namespace CoolapkUWP.Models
{
    internal interface IHasUriAndTitle
    {
        string Url { get; }
        string Title { get; }
    }

    internal class IndexPageModel : Entity, IHasUriAndTitle
    {
        public string EntityTemplate { get; private set; }
        public string Title { get; private set; }
        public string SubTitle { get; private set; }
        public string Url { get; private set; }
        public string Description { get; private set; }
        public BackgroundImageModel Pic { get; private set; }

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
            if (token.TryGetValue("subTitle", out JToken v7) && !string.IsNullOrEmpty(v7.ToString()))
            {
                SubTitle = v7.ToString();
            }
            if (token.TryGetValue("url", out JToken v3) && !string.IsNullOrEmpty(v3.ToString()))
            {
                Url = v3.ToString();
            }
            if (token.TryGetValue("description", out JToken v4) && !string.IsNullOrEmpty(v4.ToString()))
            {
                Description = v4.ToString();
            }
            else if (token.TryGetValue("subTitle", out JToken v9) && !string.IsNullOrEmpty(v9.ToString()))
            {
                Description = v9.ToString();
            }
            if (token.TryGetValue("cover_pic", out JToken v8) && !string.IsNullOrEmpty(v8.ToString()))
            {
                Pic = new BackgroundImageModel(v8.ToString(), ImageType.OriginImage);
            }
            else if (token.TryGetValue("pic", out JToken v5) && !string.IsNullOrEmpty(v5.ToString()))
            {
                Pic = new BackgroundImageModel(v5.ToString(), ImageType.OriginImage);
            }
            else if (token.TryGetValue("logo", out JToken v6) && !string.IsNullOrEmpty(v6.ToString()))
            {
                Pic = new BackgroundImageModel(v6.ToString(), ImageType.Icon);
            }
        }
    }

    internal class IndexPageMessageCardModel : Entity
    {
        public string Description { get; private set; }
        public string Title { get; private set; }

        public IndexPageMessageCardModel(JObject token) : base(token)
        {
            if (token.TryGetValue("title", out JToken v))
            {
                Title = v.ToString();
            }
            if (token.TryGetValue("description", out JToken v4))
            {
                Description = v4.ToString();
            }
        }
    }

    internal enum EntityType
    {
        Image,
        TabLink,
        SelectorLink,
        IconLink,
        TextLinks,
        ScrollLink,
        Others,
    }

    internal class IndexPageHasEntitiesModel : Entity, IHasUriAndTitle
    {
        public EntityType EntitiesType { get; private set; }
        public string Title { get; private set; }
        public string Url { get; private set; }
        public string Description { get; private set; }
        public ImmutableArray<Entity> Entities { get; private set; }

        public IndexPageHasEntitiesModel(JObject token, EntityType type) : base(token)
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
                var buider = ImmutableArray.CreateBuilder<Entity>();
                foreach (JObject item in v7 as JArray)
                {
                    switch (item.Value<string>("entityType"))
                    {
                        case "feed":
                            buider.Add(new FeedModel(item));
                            break;

                        case "user":
                            buider.Add(new UserModel(item));
                            break;

                        default:
                            buider.Add(new IndexPageModel(item));
                            break;
                    }
                }
                Entities = buider.ToImmutable();
            }
        }
    }

    internal enum OperationType
    {
        Refresh,
        Login,
        ShowTitle,
    }

    internal class IndexPageOperationCardModel : Entity, IHasUriAndTitle
    {
        public OperationType OperationType { get; private set; }
        public string EntityTemplate { get; private set; }
        public string Title { get; private set; }
        public string Url { get; private set; }

        public IndexPageOperationCardModel(JObject token, OperationType type) : base(token)
        {
            OperationType = type;
            if (token.TryGetValue("title", out JToken v2) && !string.IsNullOrEmpty(v2.ToString()))
            {
                Title = v2.ToString();
            }
            switch (type)
            {
                case OperationType.ShowTitle when token.TryGetValue("url", out JToken v3) && !string.IsNullOrEmpty(v3.ToString()):
                    Url = v3.ToString();
                    break;

                case OperationType.Refresh:
                    Url = "Refresh";
                    break;

                case OperationType.Login:
                    Url = "Login";
                    break;
            }
        }
    }
}