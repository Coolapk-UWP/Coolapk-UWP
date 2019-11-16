using CoolapkUWP.Data;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.Data.Json;
using Windows.UI.Xaml.Media;

namespace CoolapkUWP.Control.ViewModels
{
    class SourceFeedViewModel : Entity
    {
        public SourceFeedViewModel(IJsonValue t) : base(t)
        {
            JsonObject token = t.GetObject();
            url = token.TryGetValue("url", out IJsonValue json) ? json.GetString() : $"/feed/{token["id"].ToString().Replace("\"", string.Empty)}";
            if (token["entityType"].GetString() != "article")
            {
                uurl = token["userInfo"].GetObject()["url"].GetString();
                username = token["userInfo"].GetObject()["username"].GetString();
                dateline = Tools.ConvertTime(double.Parse(token["dateline"].ToString().Replace("\"", string.Empty)));
                message = Tools.GetMessageText(token["message"].GetString());
                message_title = token.TryGetValue("message_title", out IJsonValue j) ? j.GetString() : string.Empty;
            }
            else
            {
                dateline = Tools.ConvertTime(token["digest_time"].GetNumber());
                message = Tools.GetMessageText(token["message"].GetString().Substring(0, 120) + "……<a href=\"\">查看更多</a>");
                message_title = token["title"].GetString();
            }
            showMessage_title = !string.IsNullOrEmpty(message_title);
            showPicArr = token.TryGetValue("picArr", out IJsonValue value) && value.GetArray().Count > 0 && !string.IsNullOrEmpty(value.GetArray()[0].GetString());
            GetPic(token);
        }

        async void GetPic(JsonObject token)
        {
            if (showPicArr)
            {
                picArr = new ObservableCollection<ImageData>();
                foreach (var item in token["picArr"].GetArray())
                {
                    pics.Add(item.GetString());
                    picArr.Add(new ImageData { Pic = await ImageCache.GetImage(ImageType.SmallImage, item.GetString()), url = item.GetString() });
                }
            }
            if (token.TryGetValue("pic", out IJsonValue value1) && !string.IsNullOrEmpty(value1.GetString()))
                pic = await ImageCache.GetImage(ImageType.SmallImage, value1.GetString());
        }
        public string url { get; private set; }
        public string uurl { get; private set; }
        public string username { get; private set; }
        public string dateline { get; private set; }
        public bool showMessage_title { get; private set; }
        public string message_title { get; private set; }
        public string message { get; private set; }
        public bool showPicArr { get; private set; }
        public List<string> pics { get; private set; } = new List<string>();
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
