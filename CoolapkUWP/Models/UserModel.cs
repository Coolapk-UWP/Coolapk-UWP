using CoolapkUWP.Helpers;
using Newtonsoft.Json.Linq;

namespace CoolapkUWP.Models
{
    internal class UserModel : Entity
    {
        public UserModel(JObject o) : base(o)
        {
            Url = o.Value<string>("url");
            UserName = o.Value<string>("username");
            if (o.TryGetValue("fans", out JToken a))
            {
                FansNum = a.ToString().Replace("\"", string.Empty);
                FollowNum = o["follow"].ToString().Replace("\"", string.Empty);
                if (o.TryGetValue("bio", out JToken b))
                {
                    Bio = b.ToString();
                }

                LoginTime = DataHelper.ConvertTime(double.Parse(o["logintime"].ToString().Replace("\"", string.Empty))) + "活跃";
            }
            UserAvatar = new ImageModel(o.Value<string>("userSmallAvatar"), ImageType.BigAvatar);
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