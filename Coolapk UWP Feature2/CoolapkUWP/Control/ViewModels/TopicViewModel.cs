using CoolapkUWP.Data;
using Windows.Data.Json;

namespace CoolapkUWP.Control.ViewModels
{
    internal class DyhViewModel : Entity
    {
        public string Url { get; private set; }
        public string Title { get; private set; }
        public string Follownum { get; private set; }
        public string Description { get; private set; }
        public string Commentnum { get; private set; }
        public string LastUpdate { get; private set; }
        public BackgroundImageViewModel Logo { get; private set; }

        public DyhViewModel(IJsonValue t) : base(t)
        {
            JsonObject token = t.GetObject();
            if (token.TryGetValue("url", out IJsonValue url))
            {
                Url = url.GetString();
            }
            if (token.TryGetValue("title", out IJsonValue title))
            {
                Title = title.GetString();
            }
            if (token.TryGetValue("label", out IJsonValue label) && !string.IsNullOrEmpty(label.GetString()))
            {
                Title = label.GetString() + Title;
            }
            if (token.TryGetValue("follownum", out IJsonValue follownum) && !string.IsNullOrEmpty(follownum.GetNumber().ToString()))
            {
                Follownum = UIHelper.GetNumString(follownum.GetNumber());
            }
            else if (token.TryGetValue("follow_num", out IJsonValue follow_num) && !string.IsNullOrEmpty(follow_num.GetNumber().ToString()))
            {
                Follownum = UIHelper.GetNumString(follow_num.GetNumber());
            }
            if (token.TryGetValue("logo", out IJsonValue logo) && !string.IsNullOrEmpty(logo.GetString()))
            {
                Logo = new BackgroundImageViewModel(logo.GetString(), ImageType.Icon);
            }
            if (token.TryGetValue("newsnum", out IJsonValue newsnum) && !string.IsNullOrEmpty(newsnum.GetNumber().ToString()))
            {
                Commentnum = newsnum.GetNumber().ToString();
            }
            else if (token.TryGetValue("commentnum", out IJsonValue commentnum) && !string.IsNullOrEmpty(commentnum.GetNumber().ToString()))
            {
                Commentnum = UIHelper.GetNumString(commentnum.GetNumber());
            }
            else if (token.TryGetValue("rating_total_num", out IJsonValue rating_total_num) && !string.IsNullOrEmpty(rating_total_num.GetNumber().ToString()))
            {
                Commentnum = rating_total_num.GetNumber().ToString();
            }
            if (token.TryGetValue("description", out IJsonValue description) && !string.IsNullOrEmpty(description.GetString()))
            {
                Description = description.GetString();
            }
            else if (token.TryGetValue("newtitle", out IJsonValue newtitle) && !string.IsNullOrEmpty(newtitle.GetString()))
            {
                Description = newtitle.GetString();
            }
            else if (token.TryGetValue("username", out IJsonValue username) && !string.IsNullOrEmpty(username.GetString()))
            {
                Description = "作者：" + username.GetString();
            }
            else if (token.TryGetValue("rss_type", out IJsonValue rss_type) && !string.IsNullOrEmpty(rss_type.GetString()))
            {
                Description = rss_type.GetString();
            }
            else if (token.TryGetValue("hot_num", out IJsonValue hot_num) && !string.IsNullOrEmpty(hot_num.GetNumber().ToString()))
            {
                Description = UIHelper.GetNumString(hot_num.GetNumber()) + "热度";
            }
            else if (token.TryGetValue("keywords", out IJsonValue keywords) && !string.IsNullOrEmpty(keywords.ToString()))
            {
                Description = keywords.GetString();
            }
            if (token.TryGetValue("lastupdate", out IJsonValue lastupdate) && !string.IsNullOrEmpty(lastupdate.GetNumber().ToString()))
            {
                LastUpdate = UIHelper.ConvertTime(lastupdate.GetNumber());
            }
            else if (token.TryGetValue("update_time", out IJsonValue update_time) && !string.IsNullOrEmpty(update_time.GetNumber().ToString()))
            {
                LastUpdate = UIHelper.ConvertTime(update_time.GetNumber());
            }
            else if (token.TryGetValue("sell_time", out IJsonValue sell_time) && !string.IsNullOrEmpty(sell_time.GetNumber().ToString()))
            {
                LastUpdate = UIHelper.ConvertTime(sell_time.GetNumber());
            }
            else if (token.TryGetValue("create_time", out IJsonValue create_time) && !string.IsNullOrEmpty(create_time.GetNumber().ToString()))
            {
                LastUpdate = UIHelper.ConvertTime(create_time.GetNumber());
            }
        }
    }

    internal class TopicViewModel : DyhViewModel
    {
        public TopicViewModel(IJsonValue t) : base(t)
        {
        }
    }
}
