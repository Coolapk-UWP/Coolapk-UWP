using CoolapkUWP.Data;
using Windows.Data.Json;
using Windows.UI.Xaml.Media.Imaging;

namespace CoolapkUWP.Control.ViewModels
{
    class UserViewModel : Entity
    {
        public UserViewModel(IJsonValue t) : base(t)
        {
            JsonObject token = t.GetObject();
            url = token["url"].GetString();
            UserName = token["username"].GetString();
            FansNum = token["fans"].ToString().Replace("\"",string.Empty);
            FollowNum = token["follow"].ToString().Replace("\"", string.Empty);
            Bio = token["bio"].GetString();
            LoginTime = Tools.ConvertTime(double.Parse(token["logintime"].ToString().Replace("\"", string.Empty))) + "活跃";
            UserAvatar = new BitmapImage(new System.Uri(token["userSmallAvatar"].GetString()));
        }
        public string url { get; private set; }
        public string UserName { get; private set; }
        public string FollowNum { get; private set; }
        public string FansNum { get; private set; }
        public string LoginTime { get; private set; }
        public string Bio { get; private set; }
        public Windows.UI.Xaml.Media.ImageSource UserAvatar { get; private set; }
    }
}
