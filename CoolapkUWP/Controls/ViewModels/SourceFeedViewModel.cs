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
            Url = token.TryGetValue("url", out JToken json) ? json.ToString() : $"/feed/{token["id"].ToString().Replace("\"", string.Empty)}";
            if (token.Value<string>("entityType") != "article")
            {
                Uurl = token["userInfo"].Value<string>("url");
                Username = token["userInfo"].Value<string>("username");
                Dateline = DataHelper.ConvertTime(double.Parse(token["dateline"].ToString().Replace("\"", string.Empty)));
                Message = token.Value<string>("message");
                Message_title = token.TryGetValue("message_title", out JToken j) ? j.ToString() : string.Empty;
            }
            else
            {
                Dateline = DataHelper.ConvertTime(token.Value<int>("digest_time"));
                Message = token.Value<string>("message").Substring(0, 120) + "……<a href=\"\">查看更多</a>";
                Message_title = token.Value<string>("title");
            }
            ShowMessage_title = !string.IsNullOrEmpty(Message_title);
            ShowPicArr = token.TryGetValue("picArr", out JToken value) && (value as JArray).Count > 0 && !string.IsNullOrEmpty((value as JArray)[0].ToString());
            if (token.Value<string>("feedTypeName") == "酷图" && !((Windows.UI.Xaml.Window.Current.Content as Windows.UI.Xaml.Controls.Frame).Content is Pages.FeedPages.FeedListPage))
            {
                IsCoolPictuers = true;
                ShowPicArr = false;
            }
            Pics = (token["picArr"] as JArray).Select(i => i.ToString()).ToList();
            GetPic(token);
        }

        async void GetPic(JObject token)
        {
            if (ShowPicArr || IsCoolPictuers)
            {
                PicArr = new ObservableCollection<ImageData>();
                foreach (var item in token["picArr"] as JArray)
                    PicArr.Add(new ImageData { Pic = await ImageCacheHelper.GetImage(ImageType.SmallImage, item.ToString()), url = item.ToString() });
            }
            if (token.TryGetValue("pic", out JToken value1) && !string.IsNullOrEmpty(value1.ToString()))
                Pic = await ImageCacheHelper.GetImage(ImageType.SmallImage, value1.ToString());
        }
        public string Url { get; private set; }
        public string Uurl { get; private set; }
        public string Username { get; private set; }
        public string Dateline { get; private set; }
        public bool ShowMessage_title { get; private set; }
        public string Message_title { get; private set; }
        public string Message { get; private set; }
        public bool ShowPicArr { get; private set; }
        public bool IsCoolPictuers { get; private set; }
        public bool IsMoreThanOnePic { get => Pics.Count > 1; }
        public List<string> Pics { get; private set; }
        public ObservableCollection<ImageData> PicArr { get; private set; }
        private ImageSource pic1;
        public ImageSource Pic
        {
            get => pic1;
            private set
            {
                pic1 = value;
                Changed(this, nameof(Pic));
            }
        }
    }
}
