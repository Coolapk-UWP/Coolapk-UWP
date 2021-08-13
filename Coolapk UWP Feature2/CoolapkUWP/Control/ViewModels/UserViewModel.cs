using CoolapkUWP.Data;
using System.Linq;
using Windows.Data.Json;

namespace CoolapkUWP.Control.ViewModels
{
    internal class UserViewModel : Entity
    {
        public UserViewModel(IJsonValue t) : base(t)
        {
            JsonObject token = t.GetObject();
            url = token["url"].GetString();
            UserName = token["username"].GetString();
            Background = new BackgroundImageViewModel(token["cover"].GetString(), ImageType.OriginImage);
            if (token.TryGetValue("fans", out IJsonValue a))
            {
                FansNum = a.ToString().Replace("\"", string.Empty);
                FollowNum = token["follow"].ToString().Replace("\"", string.Empty);
                if (token.TryGetValue("bio", out IJsonValue b))
                { Bio = b.GetString(); }
                LoginTime = UIHelper.ConvertTime(double.Parse(token["logintime"].ToString().Replace("\"", string.Empty))) + "活跃";
                UserAvatarUrl = token["userSmallAvatar"].GetString();
            }
            GetPic(UserAvatarUrl);
        }

        private async void GetPic(string UserAvatarUrl) => UserAvatar = ImageCache.defaultNoAvatarUrl.Contains(UserAvatarUrl) ? ImageCache.NoPic : await ImageCache.GetImage(ImageType.SmallAvatar, UserAvatarUrl);
        public string url { get; private set; }
        public string UserAvatarUrl { get; private set; }
        public string UserName { get; private set; }
        public string FollowNum { get; private set; }
        public string FansNum { get; private set; }
        public string LoginTime { get; private set; }
        public string Bio { get; private set; }
        public BackgroundImageViewModel Background { get; private set; }
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
