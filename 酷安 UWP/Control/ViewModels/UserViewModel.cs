using CoolapkUWP.Data;
using Windows.Data.Json;

namespace CoolapkUWP.Control.ViewModels
{
    class UserViewModel : Entity
    {
        public UserViewModel(IJsonValue t) : base(t)
        {
            JsonObject token = t.GetObject();
            url = token["url"].GetString();
            UserName = token["username"].GetString();
            if (token.TryGetValue("fans", out IJsonValue a))
            {
                FansNum = a.ToString().Replace("\"", string.Empty);
                FollowNum = token["follow"].ToString().Replace("\"", string.Empty);
                if (token.TryGetValue("bio", out IJsonValue b))
                    Bio = b.GetString();
                LoginTime = Tools.ConvertTime(double.Parse(token["logintime"].ToString().Replace("\"", string.Empty))) + "活跃";
            }
            GetPic(token);
        }

        async void GetPic(JsonObject token) => UserAvatar = await ImageCache.GetImage(ImageType.SmallAvatar, token["userSmallAvatar"].GetString());
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
