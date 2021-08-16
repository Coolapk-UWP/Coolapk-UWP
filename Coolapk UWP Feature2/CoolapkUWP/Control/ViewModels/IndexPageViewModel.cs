using CoolapkUWP.Data;
using System.Collections.Generic;
using Windows.Data.Json;
using Windows.UI.Xaml.Media;

namespace CoolapkUWP.Control.ViewModels
{
    internal class IndexPageViewModel : Entity
    {
        private ImageSource pic1;

        public IndexPageViewModel(IJsonValue t) : base(t)
        {
            JsonObject token = t.GetObject();
            if (token.TryGetValue("entityTemplate", out IJsonValue v1))
            { entityTemplate = v1.GetString(); }
            if (token.TryGetValue("title", out IJsonValue v2) && v2.ValueType == JsonValueType.String)
            {
                hasTitle = !string.IsNullOrEmpty(v2.GetString());
                if (hasTitle)
                { title = v2.GetString(); }
            }
            if (token.TryGetValue("url", out IJsonValue v3) && v3.ValueType == JsonValueType.String)
            {
                hasUrl = !string.IsNullOrEmpty(v3.GetString());
                if (hasUrl)
                { url = v3.GetString(); }
            }
            if (token.TryGetValue("subTitle", out IJsonValue v8) && !string.IsNullOrEmpty(v8.GetString()))
            {
                subTitle = v8.GetString();
            }
            else if (token.TryGetValue("subtitle", out IJsonValue v10) && !string.IsNullOrEmpty(v10.GetString()))
            {
                subTitle = v10.GetString();
            }
            else if (token.TryGetValue("hot_num_txt", out IJsonValue hot_num_txt) && !string.IsNullOrEmpty(hot_num_txt.GetString()))
            {
                subTitle = hot_num_txt.GetString() + "热度";
            }
            else if (token.TryGetValue("link_tag", out IJsonValue link_tag) && !string.IsNullOrEmpty(link_tag.GetString()))
            {
                subTitle = link_tag.GetString();
            }
            else if (token.TryGetValue("apkTypeName", out IJsonValue apkTypeName) && !string.IsNullOrEmpty(apkTypeName.GetString()))
            {
                subTitle = apkTypeName.GetString();
            }
            else if (token.TryGetValue("keywords", out IJsonValue keywords) && !string.IsNullOrEmpty(keywords.GetString()))
            {
                subTitle = keywords.GetString();
            }
            else if (token.TryGetValue("catName", out IJsonValue catName) && !string.IsNullOrEmpty(catName.GetString()))
            {
                subTitle = catName.GetString();
            }
            else if (token.TryGetValue("rss_type", out IJsonValue rss_type) && !string.IsNullOrEmpty(rss_type.GetString()))
            {
                subTitle = rss_type.GetString();
            }
            else if (token.TryGetValue("description", out IJsonValue description) && !string.IsNullOrEmpty(description.GetString()))
            {
                subTitle = description.GetString();
            }
            else if (token.TryGetValue("commentnum", out IJsonValue commentnum) && !string.IsNullOrEmpty(commentnum.GetNumber().ToString()))
            {
                subTitle = UIHelper.GetNumString(commentnum.GetNumber()) + "评论";
            }
            if (token.TryGetValue("description", out IJsonValue v4) && v4.ValueType == JsonValueType.String)
            {
                hasDescription = !string.IsNullOrEmpty(v4.GetString());
                if (hasDescription)
                { description = v4.GetString(); }
            }
            if (token.TryGetValue("entities", out IJsonValue v7) && v7.ValueType == JsonValueType.Array)
            {
                hasEntities = v7.GetArray().Count > 0;
                if (hasEntities)
                {
                    List<Entity> models = new List<Entity>();
                    foreach (IJsonValue item in v7.GetArray())
                    {
                        JsonObject o = item.GetObject();
                        if (o["entityType"].GetString() == "feed")
                        { models.Add(new FeedViewModel(item)); }
                        else if (o["entityType"].GetString() == "user")
                        { models.Add(new UserViewModel(item)); }
                        else
                        { models.Add(new IndexPageViewModel(item)); }
                    }
                    entities = models.ToArray();
                }
            }
            hasHead = hasTitle || hasUrl;
            GetPic(token);
        }

        private async void GetPic(JsonObject token)
        {
            if (token.TryGetValue("pic", out IJsonValue v5) && v5.ValueType == JsonValueType.String)
            {
                hasPic = !string.IsNullOrEmpty(v5.GetString());
                if (hasPic)
                { pic = await ImageCache.GetImage(ImageType.Icon, v5.GetString()); }
            }
            else if (token.TryGetValue("logo", out IJsonValue v6) && v6.ValueType == JsonValueType.String)
            {
                hasPic = !string.IsNullOrEmpty(v6.GetString());
                if (hasPic)
                { pic = await ImageCache.GetImage(ImageType.Icon, v6.GetString()); }
            }
            else if (token.TryGetValue("cover_pic", out IJsonValue v7) && v7.ValueType == JsonValueType.String)
            {
                hasPic = !string.IsNullOrEmpty(v7.GetString());
                if (hasPic)
                { pic = await ImageCache.GetImage(ImageType.Icon, v7.GetString()); }
            }
            else if (token.TryGetValue("pic_url", out IJsonValue v8) && v8.ValueType == JsonValueType.String)
            {
                hasPic = !string.IsNullOrEmpty(v8.GetString());
                if (hasPic)
                { pic = await ImageCache.GetImage(ImageType.Icon, v8.GetString()); }
            }
        }

        public string entityTemplate { get; private set; }
        public bool hasTitle { get; private set; }
        public string title { get; private set; }
        public string subTitle { get; private set; }
        public bool hasUrl { get; private set; }
        public string url { get; private set; }
        public bool hasDescription { get; private set; }
        public string description { get; private set; }
        public bool hasPic { get; private set; }
        public bool hasHead { get; private set; }
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
