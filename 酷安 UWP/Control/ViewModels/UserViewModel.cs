using CoolapkUWP.Data;
using Windows.Data.Json;
using Windows.UI.Xaml.Media.Imaging;

namespace CoolapkUWP.Control.ViewModels
{
    public class UserViewModel : IEntity
    {
        public UserViewModel(IJsonValue t)
        {
            JsonObject token = t.GetObject();
            url = token["url"].GetString();
            UserName = token["username"].GetString();
            FansNum = token["fans"].GetString();
            FollowNum = token["follow"].GetString();
            Bio = token["bio"].GetString();
            LoginTime = Tools.ConvertTime(double.Parse(token["logintime"].GetString())) + "活跃";
            UserAvatar = new BitmapImage(new System.Uri(token["userSmallAvatar"].GetString()));
            if (token.TryGetValue("entityId", out IJsonValue value1)) entityId = value1.ToString().Replace("\"", string.Empty);
            entityType = token["entityType"].GetString();
            if (token.TryGetValue("entityFixed", out IJsonValue value) && value.GetNumber() == 1) entityFixed = true;
        }
        public string url { get; private set; }
        public string UserName { get; private set; }
        public string FollowNum { get; private set; }
        public string FansNum { get; private set; }
        public string LoginTime { get; private set; }
        public string Bio { get; private set; }
        public Windows.UI.Xaml.Media.ImageSource UserAvatar { get; private set; }
        public string entityId { get; private set; }
        public string entityType { get; private set; }
        public bool entityFixed { get; private set; }
    }
}
