using CoolapkUWP.Core.Models;
using CoolapkUWP.Helpers;
using Newtonsoft.Json.Linq;

namespace CoolapkUWP.Models
{
    internal class ListModel : Entity
    {
        public ListModel(JObject token) : base(token)
        {
            Windows.ApplicationModel.Resources.ResourceLoader loader = Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse("FeedListPage");
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

        public string Title { get; private set; }
        public string SubTitle { get; private set; }
        public string Url { get; private set; }
        public string Description { get; private set; }
        public ImageModel Pic { get; private set; }
    }
}