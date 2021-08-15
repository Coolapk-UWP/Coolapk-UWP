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
            if (token.TryGetValue("url", out IJsonValue Url))
            {
                url = Url.GetString();
            }
            if (token.TryGetValue("username", out IJsonValue username))
            {
                UserName = username.GetString();
            }
            if (token.TryGetValue("cover", out IJsonValue cover))
            {
                Background = new BackgroundImageViewModel(cover.GetString(), ImageType.OriginImage);
            }
            if (token.TryGetValue("fans", out IJsonValue fans))
            {
                FansNum = fans.ToString().Replace("\"", string.Empty);
            }
            if (token.TryGetValue("follow", out IJsonValue follow))
            {
                FollowNum = follow.ToString().Replace("\"", string.Empty);
            }
            if (token.TryGetValue("bio", out IJsonValue bio))
            {
                Bio = bio.GetString();
            }
            if (token.TryGetValue("logintime", out IJsonValue logintime))
            {
                LoginTime = UIHelper.ConvertTime(double.Parse(logintime.ToString().Replace("\"", string.Empty))) + "活跃";
            }
            if (token.TryGetValue("userAvatar", out IJsonValue userAvatar))
            {
                UserAvatarUrl = userAvatar.GetString();
                GetPic(UserAvatarUrl);
            }
            if (token.TryGetValue("email", out IJsonValue email))
            {
                Email = email.GetString();
            }
            if (token.TryGetValue("province", out IJsonValue province))
            {
                City = province.GetString();
            }
            if (token.TryGetValue("city", out IJsonValue city))
            {
                City += city.GetString();
            }
            if (token.TryGetValue("uid", out IJsonValue uid))
            {
                Uid = uid.GetNumber().ToString();
            }
            if (token.TryGetValue("mobile", out IJsonValue mobile))
            {
                Mobile = mobile.GetString();
            }
            if (token.TryGetValue("birthday", out IJsonValue birthday))
            {
                BirthDay = birthday.GetNumber().ToString();
            }
            if (token.TryGetValue("birthyear", out IJsonValue birthyear))
            {
                BirthYear = birthyear.GetNumber().ToString();
            }
            if (token.TryGetValue("birthmonth", out IJsonValue birthmonth))
            {
                BirthMonth = birthmonth.GetNumber().ToString();
            }
        }

        private async void GetPic(string UserAvatarUrl) => UserAvatar = ImageCache.defaultNoAvatarUrl.Contains(UserAvatarUrl) ? ImageCache.NoPic : await ImageCache.GetImage(ImageType.SmallAvatar, UserAvatarUrl);

        public string url { get; private set; }
        public string Bio { get; private set; }
        public string Uid { get; private set; }
        public string City { get; private set; }
        public string Email { get; private set; }
        public string Mobile { get; private set; }
        public string FansNum { get; private set; }
        public string UserName { get; private set; }
        public string BirthDay { get; private set; }
        public string FollowNum { get; private set; }
        public string LoginTime { get; private set; }
        public string BirthYear { get; private set; }
        public string BirthMonth { get; private set; }
        public string UserAvatarUrl { get; private set; }
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
