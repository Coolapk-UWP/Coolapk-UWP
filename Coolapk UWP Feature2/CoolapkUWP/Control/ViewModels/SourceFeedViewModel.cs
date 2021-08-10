using CoolapkUWP.Data;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.Data.Json;
using Windows.UI.Xaml.Media;

namespace CoolapkUWP.Control.ViewModels
{
    internal class SourceFeedViewModel : Entity
    {
        public SourceFeedViewModel(IJsonValue t) : base(t)
        {
            JsonObject token = t.GetObject();
            url = token.TryGetValue("url", out IJsonValue json) ? json.GetString() : $"/feed/{token["id"].ToString().Replace("\"", string.Empty)}";
            shareurl = token.TryGetValue("shareUrl", out IJsonValue shareUrl) && !string.IsNullOrEmpty(shareUrl.GetString())
                ? shareUrl.GetString()
                : "https://www.coolapk.com" + url;
            if (token["entityType"].GetString() != "article")
            {
                uurl = token["userInfo"].GetObject()["url"].GetString();
                username = token["userInfo"].GetObject()["username"].GetString();
                dateline = UIHelper.ConvertTime(double.Parse(token["dateline"].ToString().Replace("\"", string.Empty)));
                message = token["message"].GetString().Replace("<a href=\"\">查看更多</a>", "<a href=\"" + url + "\">查看更多</a>");
                message_title = token.TryGetValue("message_title", out IJsonValue j) ? j.GetString() : string.Empty;
            }
            else
            {
                dateline = UIHelper.ConvertTime(token["digest_time"].GetNumber());
                message = message.Contains("</a>") ? token["message"].GetString().Substring(0, 200) + "...<a href=\"" + url + "\">查看更多</a>" : message + "...<a href=\"" + url + "\">查看更多</a>";
                message_title = token["title"].GetString();
            }
            showMessage_title = !string.IsNullOrEmpty(message_title);
            showPicArr = token.TryGetValue("picArr", out IJsonValue value) && value.GetArray().Count > 0 && !string.IsNullOrEmpty(value.GetArray().ToString());
            if (showPicArr)
            {
                picArr = new ObservableCollection<ImageData>();
                GetPicArr(value);
            }
            if (token.TryGetValue("pic", out IJsonValue value1) && !string.IsNullOrEmpty(value1.GetString()))
            {
                havePic = true;
                picUrl = value1.GetString();
                GetPic(picUrl);
            }
        }

        private async void GetPicArr(IJsonValue value)
        {
            foreach (IJsonValue item in value.GetArray())
            {
                pics.Add(item.GetString());
                picArr.Add(new ImageData { Pic = await ImageCache.GetImage(ImageType.SmallImage, item.GetString()), url = item.GetString() });
            }
        }

        private async void GetPic(string picUrl)
        {
            pic = await ImageCache.GetImage(ImageType.SmallImage, picUrl);
        }

        public string url { get; private set; }
        public string uurl { get; private set; }
        public string shareurl { get; private set; }
        public string username { get; private set; }
        public string dateline { get; private set; }
        public bool showMessage_title { get; private set; }
        public string message_title { get; private set; }
        public string message { get; private set; }
        public bool showPicArr { get; private set; }
        public bool havePic { get; private set; }
        public string picUrl { get; private set; }
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
