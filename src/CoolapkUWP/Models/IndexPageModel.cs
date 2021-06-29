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
        public string EntityForward { get; private set; }
        public string Title { get; private set; }
        public string SubTitle { get; private set; }
        public string Url { get; private set; }
        public string Description { get; private set; }
        public BackgroundImageModel Pic { get; private set; }

        public IndexPageModel(JObject token) : base(token)
        {
            Windows.ApplicationModel.Resources.ResourceLoader loader = Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse("FeedListPage");
            if (token.TryGetValue("entityTemplate", out JToken entityTemplate) && !string.IsNullOrEmpty(entityTemplate.ToString()))
            {
                EntityTemplate = entityTemplate.ToString();
            }
            else if (token.TryGetValue("entityForward", out JToken entityForward) && !string.IsNullOrEmpty(entityForward.ToString()))
            {
                EntityForward = entityForward.ToString();
            }
            if (token.TryGetValue("title", out JToken title) && !string.IsNullOrEmpty(title.ToString()))
            {
                Title = title.ToString();
            }
            if (token.TryGetValue("subTitle", out JToken v7) && !string.IsNullOrEmpty(v7.ToString()))
            {
                SubTitle = v7.ToString();
            }
            else if (token.TryGetValue("subtitle", out JToken v10) && !string.IsNullOrEmpty(v10.ToString()))
            {
                SubTitle = v10.ToString();
            }
            else if (token.TryGetValue("hot_num_txt", out JToken hot_num_txt) && !string.IsNullOrEmpty(hot_num_txt.ToString()))
            {
                SubTitle = hot_num_txt.ToString() + loader.GetString("HotNum");
            }
            else if (token.TryGetValue("link_tag", out JToken link_tag) && !string.IsNullOrEmpty(link_tag.ToString()))
            {
                SubTitle = link_tag.ToString();
            }
            else if (token.TryGetValue("apkTypeName", out JToken apkTypeName) && !string.IsNullOrEmpty(apkTypeName.ToString()))
            {
                SubTitle = apkTypeName.ToString();
            }
            else if (token.TryGetValue("typeName", out JToken typeName) && !string.IsNullOrEmpty(typeName.ToString()))
            {
                SubTitle = typeName.ToString();
            }
            else if (token.TryGetValue("keywords", out JToken keywords) && !string.IsNullOrEmpty(keywords.ToString()))
            {
                SubTitle = keywords.ToString();
            }
            else if (token.TryGetValue("catName", out JToken catName) && !string.IsNullOrEmpty(catName.ToString()))
            {
                SubTitle = catName.ToString();
            }
            else if (token.TryGetValue("rss_type", out JToken rss_type) && !string.IsNullOrEmpty(rss_type.ToString()))
            {
                SubTitle = rss_type.ToString();
            }
            else if (token.TryGetValue("product_num", out JToken product_num) && !string.IsNullOrEmpty(product_num.ToString()))
            {
                SubTitle = product_num.ToString() + loader.GetString("product_num");
            }
            else if (token.TryGetValue("description", out JToken description) && !string.IsNullOrEmpty(description.ToString()))
            {
                SubTitle = description.ToString();
            }
            if (token.TryGetValue("video_playback_url", out JToken v0) && !string.IsNullOrEmpty(v0.ToString()))
            {
                Url = v0.ToString();
            }
            else if (token.TryGetValue("url", out JToken v3) && !string.IsNullOrEmpty(v3.ToString()))
            {
                Url = v3.ToString();
            }
            if (token.TryGetValue("description", out JToken v4) && !string.IsNullOrEmpty(v4.ToString()))
            {
                Description = v4.ToString();
            }
            else if (token.TryGetValue("release_time", out JToken release_time) && !string.IsNullOrEmpty(release_time.ToString()))
            {
                Description = loader.GetString("release_time") + release_time.ToString();
            }
            else if (token.TryGetValue("link_tag", out JToken link_tag) && !string.IsNullOrEmpty(link_tag.ToString()))
            {
                Description = link_tag.ToString();
            }
            else if (token.TryGetValue("hot_num_txt", out JToken hot_num_txt) && !string.IsNullOrEmpty(hot_num_txt.ToString()))
            {
                Description = hot_num_txt.ToString() + loader.GetString("HotNum");
            }
            else if (token.TryGetValue("keywords", out JToken keywords) && !string.IsNullOrEmpty(keywords.ToString()))
            {
                Description = keywords.ToString();
            }
            else if (token.TryGetValue("catName", out JToken catName) && !string.IsNullOrEmpty(catName.ToString()))
            {
                Description = catName.ToString();
            }
            else if (token.TryGetValue("apkTypeName", out JToken apkTypeName) && !string.IsNullOrEmpty(apkTypeName.ToString()))
            {
                Description = apkTypeName.ToString();
            }
            else if (token.TryGetValue("typeName", out JToken typeName) && !string.IsNullOrEmpty(typeName.ToString()))
            {
                Description = typeName.ToString();
            }
            else if (token.TryGetValue("rss_type", out JToken rss_type) && !string.IsNullOrEmpty(rss_type.ToString()))
            {
                Description = rss_type.ToString();
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
            else if (token.TryGetValue("pic_url", out JToken v9) && !string.IsNullOrEmpty(v9.ToString()))
            {
                Pic = new BackgroundImageModel(v9.ToString(), ImageType.Icon);
            }
        }
    }

    internal class IndexPageMessageCardModel : Entity
    {
        public ImmutableArray<Entity> Entities { get; private set; }
        public bool ShowEntities { get; private set; }
        public string Description { get; private set; }
        public string Title { get; private set; }

        public IndexPageMessageCardModel(JObject token) : base(token)
        {
            if (token.TryGetValue("title", out JToken title))
            {
                Title = title.ToString();
            }
            if (token.TryGetValue("description", out JToken description))
            {
                Description = description.ToString();
            }
            else if (token.TryGetValue("release_time", out JToken release_time) && !string.IsNullOrEmpty(release_time.ToString()))
            {
                Description = "发布日期：" + release_time.ToString();
            }
            else if (token.TryGetValue("link_tag", out JToken link_tag) && !string.IsNullOrEmpty(link_tag.ToString()))
            {
                Description = link_tag.ToString();
            }
            else if (token.TryGetValue("hot_num_txt", out JToken hot_num_txt) && !string.IsNullOrEmpty(hot_num_txt.ToString()))
            {
                Description = hot_num_txt.ToString() + "热度";
            }
            else if (token.TryGetValue("keywords", out JToken keywords) && !string.IsNullOrEmpty(keywords.ToString()))
            {
                Description = keywords.ToString();
            }
            else if (token.TryGetValue("catName", out JToken catName) && !string.IsNullOrEmpty(catName.ToString()))
            {
                Description = catName.ToString();
            }
            else if (token.TryGetValue("apkTypeName", out JToken apkTypeName) && !string.IsNullOrEmpty(apkTypeName.ToString()))
            {
                Description = apkTypeName.ToString();
            }
            else if (token.TryGetValue("rss_type", out JToken rss_type) && !string.IsNullOrEmpty(rss_type.ToString()))
            {
                Description = rss_type.ToString();
            }
            else if (token.TryGetValue("subTitle", out JToken subTitle) && !string.IsNullOrEmpty(subTitle.ToString()))
            {
                Description = subTitle.ToString();
            }
            if (token.TryGetValue("entities", out JToken entities) && (entities as JArray).Count > 0)
            {
                ImmutableArray<Entity>.Builder buider = ImmutableArray.CreateBuilder<Entity>();
                foreach (JObject item in entities as JArray)
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
                ShowEntities = true;
            }
            else { ShowEntities = false; }
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
        public bool ShowTitle { get; private set; }
        public bool ShowEntities { get; private set; }
        public string EntityTemplate { get; private set; }
        public string Description { get; private set; }
        public ImmutableArray<Entity> Entities { get; private set; }

        public IndexPageHasEntitiesModel(JObject token, EntityType type) : base(token)
        {
            EntitiesType = type;
            if (token.TryGetValue("title", out JToken title) && !string.IsNullOrEmpty(title.ToString()))
            {
                Title = title.ToString();
            }
            if (token.TryGetValue("url", out JToken url) && !string.IsNullOrEmpty(url.ToString()))
            {
                Url = url.ToString();
            }
            if (token.TryGetValue("description", out JToken description) && !string.IsNullOrEmpty(description.ToString()))
            {
                Description = description.ToString();
            }
            else if (token.TryGetValue("release_time", out JToken release_time) && !string.IsNullOrEmpty(release_time.ToString()))
            {
                Description = "发布日期：" + release_time.ToString();
            }
            else if (token.TryGetValue("link_tag", out JToken link_tag) && !string.IsNullOrEmpty(link_tag.ToString()))
            {
                Description = link_tag.ToString();
            }
            else if (token.TryGetValue("hot_num_txt", out JToken hot_num_txt) && !string.IsNullOrEmpty(hot_num_txt.ToString()))
            {
                Description = hot_num_txt.ToString() + "热度";
            }
            else if (token.TryGetValue("keywords", out JToken keywords) && !string.IsNullOrEmpty(keywords.ToString()))
            {
                Description = keywords.ToString();
            }
            else if (token.TryGetValue("catName", out JToken catName) && !string.IsNullOrEmpty(catName.ToString()))
            {
                Description = catName.ToString();
            }
            else if (token.TryGetValue("apkTypeName", out JToken apkTypeName) && !string.IsNullOrEmpty(apkTypeName.ToString()))
            {
                Description = apkTypeName.ToString();
            }
            else if (token.TryGetValue("rss_type", out JToken rss_type) && !string.IsNullOrEmpty(rss_type.ToString()))
            {
                Description = rss_type.ToString();
            }
            else if (token.TryGetValue("subTitle", out JToken subTitle) && !string.IsNullOrEmpty(subTitle.ToString()))
            {
                Description = subTitle.ToString();
            }
            if (token.TryGetValue("entityTemplate", out JToken entityTemplate) && !string.IsNullOrEmpty(entityTemplate.ToString()))
            {
                EntityTemplate = entityTemplate.ToString();
            }
            if (token.TryGetValue("entities", out JToken entities) && (entities as JArray).Count > 0)
            {
                ImmutableArray<Entity>.Builder buider = ImmutableArray.CreateBuilder<Entity>();
                foreach (JObject item in entities as JArray)
                {
                    item.Property("entityType").AddAfterSelf(new JProperty("entityForward", EntityTemplate));
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
                ShowEntities = true;
            }
            else { ShowEntities = false; }
            ShowTitle = !(string.IsNullOrEmpty(Title) && string.IsNullOrEmpty(Url));
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
            if (token.TryGetValue("title", out JToken title) && !string.IsNullOrEmpty(title.ToString()))
            {
                Title = title.ToString();
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