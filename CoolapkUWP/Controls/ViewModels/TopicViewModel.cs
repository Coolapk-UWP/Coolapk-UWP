using CoolapkUWP.Helpers;
using Newtonsoft.Json.Linq;
using Windows.UI.Xaml.Media;

namespace CoolapkUWP.Controls.ViewModels
{
    class TopicViewModel : DyhViewModel
    {
        public TopicViewModel(JObject token) : base(token) => Commentnum = token["commentnum"].ToString().Replace("\"", string.Empty);
        public string Commentnum { get; private set; }
    }

    class DyhViewModel : Entity
    {
        public DyhViewModel(JObject token) : base(token)
        {
            Url = token.Value<string>("url");
            Title = token.Value<string>("title");
            Follownum = token["follownum"].ToString().Replace("\"", string.Empty);
            GetPic(token);
        }
        async void GetPic(JObject token) => Logo = await ImageCacheHelper.GetImage(ImageType.Icon, token.Value<string>("logo"));
        public string Url { get; private set; }
        public string Title { get; private set; }
        public string Follownum { get; private set; }
        private ImageSource logo1;
        public ImageSource Logo
        {
            get => logo1;
            private set
            {
                logo1 = value;
                Changed(this, nameof(Logo));
            }
        }
    }
}
