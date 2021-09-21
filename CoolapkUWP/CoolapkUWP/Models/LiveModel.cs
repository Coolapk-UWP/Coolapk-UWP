using CoolapkUWP.Core.Models;
using CoolapkUWP.Helpers;
using Newtonsoft.Json.Linq;

namespace CoolapkUWP.Models
{
    internal class LiveMode : Entity
    {
        public string Url { get; private set; }
        public string Title { get; private set; }
        public string Message { get; private set; }
        public string Uurl { get; private set; }
        public string Username { get; private set; }
        public BackgroundImageModel Pic { get; private set; }
        public ImageModel UserSmallAvatar { get; private set; }

        public LiveMode(JObject o) : base(o)
        {
            Url = o.Value<string>("video_playback_url");
            Title = o.Value<string>("title");
            Message = o.Value<string>("description");
            Uurl = o["userInfo"].Value<string>("url");
            Username = o["userInfo"].Value<string>("username");
            string userSmallAvatarUrl = o["userInfo"].Value<string>("userSmallAvatar");
            if (!string.IsNullOrEmpty(userSmallAvatarUrl))
            {
                UserSmallAvatar = new ImageModel(userSmallAvatarUrl, ImageType.BigAvatar);
            }
            if (o.TryGetValue("pic_url", out JToken value1) && !string.IsNullOrEmpty(value1.ToString()))
            {
                Pic = new BackgroundImageModel(value1.ToString(), ImageType.SmallImage);
            }
        }
    }
}