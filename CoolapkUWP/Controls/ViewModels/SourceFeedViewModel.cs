using CoolapkUWP.Helpers;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml.Media;

namespace CoolapkUWP.Controls.ViewModels
{
    class SourceFeedViewModel : Entity
    {
        public SourceFeedViewModel(JToken t) : base(t)
        {
            JObject token = t as JObject;
            url = token.TryGetValue("url", out JToken json) ? json.ToString() : $"/feed/{token["id"].ToString().Replace("\"", string.Empty)}";
            if (token.Value<string>("entityType") != "article")
            {
                uurl = token["userInfo"].Value<string>("url");
                username = token["userInfo"].Value<string>("username");
                dateline = DataHelper.ConvertTime(double.Parse(token["dateline"].ToString().Replace("\"", string.Empty)));
                message = token.Value<string>("message");
                message_title = token.TryGetValue("message_title", out JToken j) ? j.ToString() : string.Empty;
            }
            else
            {
                dateline = DataHelper.ConvertTime(token.Value<int>("digest_time"));
                message = token.Value<string>("message").Substring(0, 120) + "……<a href=\"\">查看更多</a>";
                message_title = token.Value<string>("title");
            }
            showMessage_title = !string.IsNullOrEmpty(message_title);
            showPicArr = token.TryGetValue("picArr", out JToken value) && (value as JArray).Count > 0 && !string.IsNullOrEmpty((value as JArray)[0].ToString());
            if (token.Value<string>("feedTypeName") == "酷图" && !((Windows.UI.Xaml.Window.Current.Content as Windows.UI.Xaml.Controls.Frame).Content is Pages.FeedPages.FeedListPage))
            {
                isCoolPictuers = true;
                showPicArr = false;
            }
            pics = (token["picArr"] as JArray).Select(i => i.ToString()).ToList();
            GetPic(token);
        }

        async void GetPic(JObject token)
        {
            if (showPicArr || isCoolPictuers)
            {
                picArr = new ObservableCollection<ImageData>();
                foreach (var item in token["picArr"] as JArray)
                    picArr.Add(new ImageData { Pic = await ImageCacheHelper.GetImage(ImageType.SmallImage, item.ToString()), url = item.ToString() });
            }
            if (token.TryGetValue("pic", out JToken value1) && !string.IsNullOrEmpty(value1.ToString()))
                pic = await ImageCacheHelper.GetImage(ImageType.SmallImage, value1.ToString());
        }
        public string url { get; private set; }
        public string uurl { get; private set; }
        public string username { get; private set; }
        public string dateline { get; private set; }
        public bool showMessage_title { get; private set; }
        public string message_title { get; private set; }
        public string message { get; private set; }
        public bool showPicArr { get; private set; }
        public bool isCoolPictuers { get; private set; }
        public bool isMoreThanOnePic { get => pics.Count > 1; }
        public List<string> pics { get; private set; }
        public ObservableCollection<ImageData> picArr { get; private set; }
        private ImageSource pic1;
        public ImageSource pic
        {
            get => pic1;
            private set
            {
                pic1 = value;
                Changed(this, nameof(pic));
            }
        }
    }
}
