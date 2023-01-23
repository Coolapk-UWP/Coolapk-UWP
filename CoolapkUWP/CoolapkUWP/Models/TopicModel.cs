using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Images;
using Newtonsoft.Json.Linq;

namespace CoolapkUWP.Models
{
    public class TopicModel : Entity, IHasDescription
    {
        public string Url { get; private set; }
        public string Title { get; private set; }
        public string FollowNum { get; private set; }
        public string Description { get; private set; }
        public string CommentNum { get; private set; }
        public string LastUpdate { get; private set; }
        public ImageModel Logo { get; private set; }

        public ImageModel Pic => Logo;

        public TopicModel(JObject token) : base(token)
        {
            if (token.TryGetValue("url", out JToken url) && !string.IsNullOrEmpty(url.ToString()))
            {
                Url = url.ToString();
            }

            if (token.TryGetValue("title", out JToken title) && !string.IsNullOrEmpty(title.ToString()))
            {
                Title = title.ToString();
            }

            if (token.TryGetValue("follownum", out JToken follownum) && !string.IsNullOrEmpty(follownum.ToString()))
            {
                FollowNum = follownum.ToString();
            }
            else if (token.TryGetValue("follow_num", out JToken follow_num) && !string.IsNullOrEmpty(follow_num.ToString()))
            {
                FollowNum = follow_num.ToString();
            }

            if (token.TryGetValue("logo", out JToken logo) && !string.IsNullOrEmpty(logo.ToString()))
            {
                Logo = new ImageModel(logo.ToString(), ImageType.Icon);
            }

            if (token.TryGetValue("newsnum", out JToken newsnum) && !string.IsNullOrEmpty(newsnum.ToString()))
            {
                CommentNum = newsnum.ToString();
            }
            else if (token.TryGetValue("commentnum", out JToken commentnum) && !string.IsNullOrEmpty(commentnum.ToString()))
            {
                CommentNum = commentnum.ToString();
            }
            else if (token.TryGetValue("rating_total_num", out JToken rating_total_num) && !string.IsNullOrEmpty(rating_total_num.ToString()))
            {
                CommentNum = rating_total_num.ToString();
            }

            if (token.TryGetValue("description", out JToken description) && !string.IsNullOrEmpty(description.ToString()))
            {
                Description = description.ToString();
            }
            else if (token.TryGetValue("newtitle", out JToken newtitle) && !string.IsNullOrEmpty(newtitle.ToString()))
            {
                Description = newtitle.ToString();
            }
            else if (token.TryGetValue("username", out JToken username) && !string.IsNullOrEmpty(username.ToString()))
            {
                Description = "作者" + username.ToString();
            }
            else if (token.TryGetValue("rss_type", out JToken rss_type) && !string.IsNullOrEmpty(rss_type.ToString()))
            {
                Description = rss_type.ToString();
            }
            else if (token.TryGetValue("hot_num", out JToken hot_num) && !string.IsNullOrEmpty(hot_num.ToString()))
            {
                Description = DataHelper.GetNumString(double.Parse(hot_num.ToString())) + "热度";
            }

            if (token.TryGetValue("lastupdate", out JToken lastupdate) && !string.IsNullOrEmpty(lastupdate.ToString()))
            {
                LastUpdate = lastupdate.ToObject<long>().ConvertUnixTimeStampToReadable();
            }
        }

        public override string ToString() => $"{Title} - {Description}";
    }
}
