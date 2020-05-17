using CoolapkUWP.Helpers;
using Newtonsoft.Json.Linq;

namespace CoolapkUWP.Controls.ViewModels
{
    class UserViewModel : Entity
    {
        public UserViewModel(JToken t) : base(t)
        {
            JObject token = t as JObject;
            url = token.Value<string>("url");
            UserName = token.Value<string>("username");
            if (token.TryGetValue("fans", out JToken a))
            {
                FansNum = a.ToString().Replace("\"", string.Empty);
                FollowNum = token["follow"].ToString().Replace("\"", string.Empty);
                if (token.TryGetValue("bio", out JToken b))
                    Bio = b.ToString();
                LoginTime = DataHelper.ConvertTime(double.Parse(token["logintime"].ToString().Replace("\"", string.Empty))) + "活跃";
            }
            GetPic(token);
        }

        async void GetPic(JObject token) => UserAvatar = await ImageCacheHelper.GetImage(ImageType.SmallAvatar, token.Value<string>("userSmallAvatar"));
        public string url { get; private set; }
        public string UserName { get; private set; }
        public string FollowNum { get; private set; }
        public string FansNum { get; private set; }
        public string LoginTime { get; private set; }
        public string Bio { get; private set; }
        private Windows.UI.Xaml.Media.ImageSource userAvatar;
        public Windows.UI.Xaml.Media.ImageSource UserAvatar
        {
            get => userAvatar;
            private set
            {
                userAvatar = value;
                Changed(this, nameof(UserAvatar));
            }
        }
    }
}
