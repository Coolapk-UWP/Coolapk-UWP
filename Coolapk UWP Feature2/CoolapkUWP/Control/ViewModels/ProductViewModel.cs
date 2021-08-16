using CoolapkUWP.Data;
using Windows.Data.Json;
using Windows.UI.Xaml.Media;

namespace CoolapkUWP.Control.ViewModels
{
    internal class ProductViewModel : Entity
    {
        public string Url { get; private set; }
        public string Title { get; private set; }
        public string Follownum { get; private set; }
        public string Description { get; private set; }
        public string Commentnum { get; private set; }
        public string LastUpdate { get; private set; }
        public BackgroundImageViewModel Logo { get; private set; }

        public ProductViewModel(IJsonValue o) : base(o)
        {
            JsonObject token = o.GetObject();
            if (token.TryGetValue("url", out IJsonValue url))
            {
                Url = url.GetString();
            }
            if (token.TryGetValue("title", out IJsonValue title))
            {
                Title = title.GetString();
            }
            if (token.TryGetValue("follow_num", out IJsonValue follow_num))
            {
                Follownum = UIHelper.GetNumString(follow_num.GetNumber());
            }
            if (token.TryGetValue("logo", out IJsonValue logo))
            {
                Logo = new BackgroundImageViewModel(logo.GetString(), ImageType.Icon);
            }
            if (token.TryGetValue("feed_comment_num", out IJsonValue feed_comment_num))
            {
                Commentnum = UIHelper.GetNumString(feed_comment_num.GetNumber());
            }
            if (token.TryGetValue("description", out IJsonValue description) && !string.IsNullOrEmpty(description.GetString()))
            {
                Description = description.GetString();
            }
            else if (token.TryGetValue("release_time", out IJsonValue release_time) && !string.IsNullOrEmpty(release_time.GetString()))
            {
                Description = "发布日期：" + release_time.GetString();
            }
            else if (token.TryGetValue("link_tag", out IJsonValue link_tag) && !string.IsNullOrEmpty(link_tag.GetString()))
            {
                Description = link_tag.GetString();
            }
            else if (token.TryGetValue("hot_num_txt", out IJsonValue hot_num_txt) && !string.IsNullOrEmpty(UIHelper.GetValue(hot_num_txt)))
            {
                Description = UIHelper.GetValue(hot_num_txt) + "热度";
            }
            if (token.TryGetValue("update_time", out IJsonValue update_time) && !string.IsNullOrEmpty(update_time.GetNumber().ToString()))
            {
                LastUpdate = UIHelper.ConvertTime(update_time.GetNumber());
            }
        }
    }
}
