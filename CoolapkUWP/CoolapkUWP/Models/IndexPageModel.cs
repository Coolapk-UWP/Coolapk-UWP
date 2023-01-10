using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Feeds;
using CoolapkUWP.Models.Images;
using CoolapkUWP.Models.Users;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Immutable;

namespace CoolapkUWP.Models
{
    internal class IndexPageModel : Entity, IList
    {
        public string Url { get; private set; }
        public string Title { get; private set; }
        public string SubTitle { get; private set; }
        public string Description { get; private set; }
        public string EntityForward { get; private set; }
        public string EntityTemplate { get; private set; }
        public ImageModel Pic { get; private set; }

        public IndexPageModel(JObject token) : base(token)
        {
            Windows.ApplicationModel.Resources.ResourceLoader loader = Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse("FeedListPage");

            if (token.TryGetValue("entityTemplate", out JToken entityTemplate))
            {
                EntityTemplate = entityTemplate.ToString();
            }

            if (token.TryGetValue("entityForward", out JToken entityForward))
            {
                EntityForward = entityForward.ToString();
            }

            if (token.TryGetValue("title", out JToken title))
            {
                Title = title.ToString();
            }

            if (token.TryGetValue("subTitle", out JToken subTitle) && !string.IsNullOrEmpty(subTitle.ToString()))
            {
                SubTitle = subTitle.ToString();
            }
            else if (token.TryGetValue("subtitle", out JToken subtitle) && !string.IsNullOrEmpty(subtitle.ToString()))
            {
                SubTitle = subtitle.ToString();
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
            else if (token.TryGetValue("description", out JToken description))
            {
                SubTitle = description.ToString();
            }

            if (token.TryGetValue("video_playback_url", out JToken video_playback_url) && !string.IsNullOrEmpty(video_playback_url.ToString()))
            {
                Url = video_playback_url.ToString();
            }
            else if (token.TryGetValue("url", out JToken url))
            {
                Url = url.ToString();
            }

            if (token.TryGetValue("description", out JToken v1) && !string.IsNullOrEmpty(v1.ToString()))
            {
                Description = v1.ToString();
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
            else if (token.TryGetValue("subTitle", out JToken v2))
            {
                Description = v2.ToString();
            }

            if (token.TryGetValue("cover_pic", out JToken cover_pic) && !string.IsNullOrEmpty(cover_pic.ToString()))
            {
                Pic = new ImageModel(cover_pic.ToString(), ImageType.OriginImage);
            }
            else if (token.TryGetValue("pic", out JToken pic) && !string.IsNullOrEmpty(pic.ToString()))
            {
                Pic = new ImageModel(pic.ToString(), ImageType.OriginImage);
            }
            else if (token.TryGetValue("logo", out JToken logo) && !string.IsNullOrEmpty(logo.ToString()))
            {
                Pic = new ImageModel(logo.ToString(), ImageType.Icon);
            }
            else if (token.TryGetValue("pic_url", out JToken pic_url))
            {
                Pic = new ImageModel(pic_url.ToString(), ImageType.Icon);
            }
        }

        public override string ToString() => $"{Title} - {Description}";
    }

    internal class IndexPageMessageCardModel : Entity
    {
        public string Title { get; private set; }
        public bool ShowEntities { get; private set; }
        public string Description { get; private set; }
        public ImmutableArray<Entity> Entities { get; private set; }

        public IndexPageMessageCardModel(JObject token) : base(token)
        {
            if (token.TryGetValue("title", out JToken title))
            {
                Title = title.ToString();
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
            else if (token.TryGetValue("subTitle", out JToken subTitle))
            {
                Description = subTitle.ToString();
            }

            if (token.TryGetValue("entities", out JToken entities) && (entities as JArray).Count > 0)
            {
                ImmutableArray<Entity>.Builder buider = ImmutableArray.CreateBuilder<Entity>();
                foreach (JObject item in entities as JArray)
                {
                    if (item.TryGetValue("entityType", out JToken entityType))
                    {
                        switch (entityType.ToString())
                        {
                            case "feed":
                                buider.Add(new FeedModel(item));
                                break;

                            //case "user":
                            //    buider.Add(new UserModel(item));
                            //    break;

                            default:
                                buider.Add(new IndexPageModel(item));
                                break;
                        }
                    }
                }
                Entities = buider.ToImmutable();
                ShowEntities = true;
            }
            else { ShowEntities = false; }
        }

        public override string ToString() => $"{Title} - {Description}";
    }

    internal enum EntityType
    {
        Image,
        Others,
        TabLink,
        IconLink,
        TextLinks,
        ScrollLink,
        SelectorLink,
    }

    internal class IndexPageHasEntitiesModel : Entity, IList
    {
        public string Url { get; private set; }
        public string Title { get; private set; }
        public bool ShowPic { get; private set; }
        public bool ShowTitle { get; private set; }
        public ImageModel Pic { get; private set; }
        public bool ShowEntities { get; private set; }
        public string Description { get; private set; }
        public string EntityTemplate { get; private set; }
        public EntityType EntitiesType { get; private set; }
        public ImmutableArray<Entity> Entities { get; private set; }

        public IndexPageHasEntitiesModel(JObject token, EntityType type) : base(token)
        {
            EntitiesType = type;

            if (token.TryGetValue("title", out JToken title))
            {
                Title = title.ToString();
            }

            if (token.TryGetValue("url", out JToken url))
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
            else if (token.TryGetValue("subTitle", out JToken subTitle))
            {
                Description = subTitle.ToString();
            }

            if (token.TryGetValue("entityTemplate", out JToken entityTemplate))
            {
                EntityTemplate = entityTemplate.ToString();
            }

            if (token.TryGetValue("entities", out JToken entities) && (entities as JArray).Count > 0)
            {
                ImmutableArray<Entity>.Builder buider = ImmutableArray.CreateBuilder<Entity>();
                foreach (JObject item in entities as JArray)
                {
                    if (item.TryGetValue("entityType", out JToken entityType))
                    {
                        try { item.Property("entityType").AddAfterSelf(new JProperty("entityForward", EntityTemplate)); }
                        catch (Exception ex) { SettingsHelper.LogManager.GetLogger(nameof(IndexPageModel)).Warn(ex.ExceptionToMessage(), ex); }
                        switch (entityType.ToString())
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
                }

                Entities = buider.ToImmutable();
                ShowEntities = true;
            }
            else { ShowEntities = false; }

            if (token.TryGetValue("pic", out JToken pic) && !string.IsNullOrEmpty(pic.ToString()))
            {
                Pic = new ImageModel(pic.ToString(), ImageType.OriginImage);
            }
            else { ShowPic = false; }

            ShowTitle = !(string.IsNullOrEmpty(Title) && string.IsNullOrEmpty(Url));
        }

        public override string ToString() => $"{Title} - {Description}";
    }

    internal enum OperationType
    {
        Login,
        Refresh,
        ShowTitle,
    }

    internal class IndexPageOperationCardModel : Entity, IHasUriAndTitle
    {
        public string Url { get; private set; }
        public string Title { get; private set; }
        public string EntityTemplate { get; private set; }
        public OperationType OperationType { get; private set; }

        public IndexPageOperationCardModel(JObject token, OperationType type) : base(token)
        {
            OperationType = type;

            if (token.TryGetValue("title", out JToken title))
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

        public override string ToString() => Title;
    }
}
