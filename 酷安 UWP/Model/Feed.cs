using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace 酷安_UWP
{
    public class Feed
    {
        protected JObject jObject;
        public Feed(JObject jObject) => this.jObject = jObject;
        public string GetValue(string value)
        {
            if (jObject.TryGetValue(value, out JToken token))
                return token.ToString();
            else
                return string.Empty;
        }
        public Feed[] GetSelfs() => new Feed[] { this };
        public ImageSource GetValue2(string value)
        {
            string s = jObject.GetValue(value).ToString();
            if (!string.IsNullOrEmpty(s))
                return new BitmapImage(new Uri(s + ".s.jpg"));
            else return new BitmapImage();
        }

        public ImageSource[] GetValue3(string value)
        {
            JArray array = (JArray)jObject.GetValue(value);
            List<BitmapImage> images = new List<BitmapImage>();
            foreach (var item in array)
                if (!string.IsNullOrEmpty(item.ToString()))
                    //获取缩略图
                    images.Add(new BitmapImage(new Uri(item.ToString() + ".s.jpg")));
            return images.ToArray();
        }
        public Feed[] GetFeeds(string value)
        {
            JArray array = (JArray)jObject.GetValue(value);
            List<Feed> fs = new List<Feed>();
            foreach (JObject item in array)
                if (!string.IsNullOrEmpty(item.ToString()))
                    fs.Add(new Feed(item));
            return fs.ToArray();
        }
        public string[] GetValue4(string value)
        {
            JArray array = (JArray)jObject.GetValue(value);
            List<string> s = new List<string>();
            foreach (var item in array)
                if (!string.IsNullOrEmpty(item.ToString()))
                    s.Add(item.ToString() + ".s.jpg");
            return s.ToArray();
        }
    }
    public class Feed2 : Feed
    {
        JToken jToken = null;
        public string ListType { get; private set; }
        public Feed2(JToken token, string type) : this(new JObject(), type) => jToken = token;

        public Feed2(JObject jObject) : base(jObject) { }
        public Feed2(JObject jObject, string type) : base(jObject) => ListType = type;
        public void SetJObject(JObject jObject) => base.jObject = jObject;
        public new Feed2[] GetFeeds(string value)
        {
            JArray array = new JArray();
            if (jToken is null)
                array = (JArray)jObject.GetValue(value);
            else
                array = jToken as JArray;
            List<Feed2> fs = new List<Feed2>();
            foreach (JObject item in array)
                if (!string.IsNullOrEmpty(item.ToString()))
                    fs.Add(new Feed2(item));
            return fs.ToArray();
        }
    }
}
