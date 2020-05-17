using CoolapkUWP.Helpers;
using Newtonsoft.Json.Linq;
using Windows.UI.Xaml.Media;

namespace CoolapkUWP.Controls.ViewModels
{
    class TopicViewModel : DyhViewModel
    {
        public TopicViewModel(JToken t) : base(t)
        {
            JObject token = t as JObject;
            commentnum = token["commentnum"].ToString().Replace("\"", string.Empty);
        }
        public string commentnum { get; private set; }
    }

    class DyhViewModel : Entity
    {
        public DyhViewModel(JToken t) : base(t)
        {
            JObject token = t as JObject;
            url = token.Value<string>("url");
            title = token.Value<string>("title");
            follownum = token["follownum"].ToString().Replace("\"", string.Empty);
            GetPic(token);
        }
        async void GetPic(JObject token) => logo = await ImageCacheHelper.GetImage(ImageType.Icon, token.Value<string>("logo"));
        public string url { get; private set; }
        public string title { get; private set; }
        public string follownum { get; private set; }
        private ImageSource logo1;
        public ImageSource logo
        {
            get => logo1; 
            private set
            {
                logo1 = value;
                Changed(this, nameof(logo));
            }
        }
    }
}
