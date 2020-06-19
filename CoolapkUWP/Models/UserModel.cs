using CoolapkUWP.Helpers;
using Newtonsoft.Json.Linq;

namespace CoolapkUWP.Models
{
    internal class UserModel : Entity
    {
        public UserModel(JObject token) : base(token)
        {
            Url = token.Value<string>("url");
            UserName = token.Value<string>("username");
            if (token.TryGetValue("fans", out JToken a))
            {
                FansNum = a.ToString().Replace("\"", string.Empty);
                FollowNum = token["follow"].ToString().Replace("\"", string.Empty);
                if (token.TryGetValue("bio", out JToken b))
                    Bio = b.ToString();
                LoginTime = DataHelper.ConvertTime(double.Parse(token["logintime"].ToString().Replace("\"", string.Empty))) + "活跃";
            }
            UserAvatar = new ImageModel(token.Value<string>("userSmallAvatar"), ImageType.BigAvatar);
        }

        public string Url { get; private set; }
        public string UserName { get; private set; }
        public string FollowNum { get; private set; }
        public string FansNum { get; private set; }
        public string LoginTime { get; private set; }
        public string Bio { get; private set; }
        public ImageModel UserAvatar { get; private set; }
    }
}