using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace 酷安_UWP
{
    public class Feed
    {
        static ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        public JObject jObject;
        public Feed(JObject jObject) => this.jObject = jObject;
        public Style listviewStyle
        {
            get
            {
                if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile") return Application.Current.Resources["ListViewStyle2Mobile"] as Style;
                else return Application.Current.Resources["ListViewStyle2Desktop"] as Style;
            }
        }
        public string GetValue(string key)
        {
            if (!(jObject is null) && jObject.TryGetValue(key, out JToken token))
                switch (key)
                {
                    case "message": return Tools.ProcessMessage(token.ToString(), localSettings);
                    case "message_raw_output":
                        JArray array = JArray.Parse(token.ToString());
                        string s = string.Empty;
                        foreach (var item in array)
                        {
                            if (item["type"].ToString() == "text")
                                s += Tools.ProcessMessage(item["message"].ToString(), localSettings);
                            else if (item["type"].ToString() == "image")
                            {
                                string d = string.IsNullOrEmpty(item["description"].ToString()) ? string.Empty : item["description"].ToString();
                                s += $"\n\n![image]({item["url"].ToString()}.s.jpg)\n\n>{d}\n\n";
                            }
                        }
                        return s;
                    case "extra_url":
                        if (!string.IsNullOrEmpty(token.ToString()))
                            if (token.ToString().IndexOf("http") == 0)
                                return new Uri(token.ToString()).Host;
                            else return string.Empty;
                        else return string.Empty;
                    case "infoHtml": return token.ToString().Replace("&nbsp;", string.Empty);
                    case "dateline": return Tools.ConvertTime(token.ToString());
                    case "logintime": return Tools.ConvertTime(token.ToString()) + "活跃";
                    default: return token.ToString();
                }
            else switch (key)
                {
                    case "message2":
                        if (string.IsNullOrEmpty(jObject["pic"].ToString()))
                            return $"[{jObject["username"]}](/u/{jObject["uid"]})：{Tools.ProcessMessage(jObject["message"].ToString(), localSettings)}";
                        else
                            return $"[{jObject["username"]}](/u/{jObject["uid"]})：{Tools.ProcessMessage(jObject["message"].ToString(), localSettings)}\n[查看图片]({jObject["pic"]})";
                    case "extra_url2": return jObject["extra_url"].ToString();
                    case "dyh_id2": return "/dyh/" + jObject["dyh_id"].ToString();
                    default: return string.Empty;
                }
        }

        public string GetValue(string path, string key)
        {
            if (!(jObject is null) && jObject.TryGetValue(path, out JToken t))
            {
                if (t.HasValues)
                {
                    JObject jObject = JObject.Parse(t.ToString());
                    if (!(jObject is null) && jObject.TryGetValue(key, out JToken token))
                    {
                        switch (key)
                        {
                            case "message": return Tools.ProcessMessage(token.ToString(), localSettings);
                            case "message_raw_output":
                                JArray array = JArray.Parse(token.ToString());
                                string s = string.Empty;
                                foreach (JObject item in array)
                                {
                                    if (item["type"].ToString() == "text")
                                        s += Tools.ProcessMessage(item["message"].ToString(), localSettings);
                                    else if (item["type"].ToString() == "image")
                                        s += $"\n\n![image]({item["url"].ToString()}.s.jpg)\n\n>{item["description"].ToString()}\n\n";
                                }
                                return s;
                            case "extra_url":
                                if (!string.IsNullOrEmpty(token.ToString()))
                                    if (token.ToString().IndexOf("http") == 0)
                                        return new Uri(token.ToString()).Host;
                                    else return string.Empty;
                                else return string.Empty;
                            case "infoHtml": return token.ToString().Replace("&nbsp;", string.Empty);
                            case "dateline": return Tools.ConvertTime(token.ToString());
                            default: return token.ToString();
                        }
                    }
                    return string.Empty;
                }
                return string.Empty;
            }
            else switch (key)
                {
                    case "message2":
                        if (string.IsNullOrEmpty(jObject["pic"].ToString()))
                            return $"[{jObject["username"]}](/u/{jObject["uid"]})：{Tools.ProcessMessage(jObject["message"].ToString(), localSettings)}";
                        else
                            return $"[{jObject["username"]}](/u/{jObject["uid"]})：{Tools.ProcessMessage(jObject["message"].ToString(), localSettings)}\n[查看图片]({jObject["pic"]})";
                    case "extra_url2": return jObject["extra_url"].ToString();
                    default: return string.Empty;
                }
        }


        // 获取缩略图
        public ImageSource GetSmallImage(string value)
        {
            if (jObject.TryGetValue(value, out JToken token))
            {
                string s = token.ToString();
                if (!string.IsNullOrEmpty(s))
                    if (Convert.ToBoolean(localSettings.Values["IsNoPicsMode"]))
                        if (Convert.ToBoolean(localSettings.Values["IsDarkMode"]))
                            return new BitmapImage(new Uri("ms-appx:/Assets/img_placeholder_night.png"));
                        else return new BitmapImage(new Uri("ms-appx:/Assets/img_placeholder.png"));
                    else return new BitmapImage(new Uri(s + ".s.jpg"));
                else return new BitmapImage();
            }
            else return new BitmapImage();
        }
        // 获取原图
        public ImageSource GetImage(string key)
        {
            if (jObject.TryGetValue(key, out JToken token))
            {
                string s = token.ToString();
                if (!string.IsNullOrEmpty(s))
                    if (Convert.ToBoolean(localSettings.Values["IsNoPicsMode"]))
                        if (Convert.ToBoolean(localSettings.Values["IsDarkMode"]))
                            return new BitmapImage(new Uri("ms-appx:/Assets/img_placeholder_night.png"));
                        else return new BitmapImage(new Uri("ms-appx:/Assets/img_placeholder.png"));
                    else return new BitmapImage(new Uri(s));
                else return new BitmapImage();
            }
            else return new BitmapImage();
        }

        public ImageSource GetImage(string path, string key)
        {
            if (jObject.TryGetValue(path, out JToken t))
                if ((t as JObject).TryGetValue(key, out JToken token))
                {
                    string s = token.ToString();
                    if (!string.IsNullOrEmpty(s))
                        if (Convert.ToBoolean(localSettings.Values["IsNoPicsMode"]))
                            if (Convert.ToBoolean(localSettings.Values["IsDarkMode"]))
                                return new BitmapImage(new Uri("ms-appx:/Assets/img_placeholder_night.png"));
                            else return new BitmapImage(new Uri("ms-appx:/Assets/img_placeholder.png"));
                        else return new BitmapImage(new Uri(s));
                    else return new BitmapImage();
                }
                else return new BitmapImage();
            else return new BitmapImage();
        }


        public ImageSource[] GetSmallImages(string value)
        {
            List<BitmapImage> images = new List<BitmapImage>();
            if (jObject.TryGetValue(value, out JToken token))
            {
                JArray array = (JArray)token;
                foreach (var item in array)
                    if (!string.IsNullOrEmpty(item.ToString()))
                        if (Convert.ToBoolean(localSettings.Values["IsNoPicsMode"]))
                            if (Convert.ToBoolean(localSettings.Values["IsDarkMode"]))
                                images.Add(new BitmapImage(new Uri("ms-appx:/Assets/img_placeholder_night.png")));
                            else images.Add(new BitmapImage(new Uri("ms-appx:/Assets/img_placeholder.png")));
                        else images.Add(new BitmapImage(new Uri(item.ToString() + ".s.jpg")));
            }
            return images.ToArray();
        }

        public Feed[] GetFeed(string value)
        {
            if (jObject.TryGetValue(value, out JToken token))
            {
                if (token is null)
                    return new Feed[] { new Feed(new JObject()) };
                else if (value == "extraData")
                    return new Feed[] { new Feed(JObject.Parse(token.ToString())) };
                else if (!token.HasValues)
                {
                    if (!jObject.TryGetValue("v", out JToken jtoken))
                        jObject.Add("v", new JValue(true));
                    return new Feed[] { new Feed(new JObject()) };
                }
                return new Feed[] { new Feed(token as JObject) };
            }
            else return new Feed[] { new Feed(new JObject()) };
        }

        public Feed[] GetFeeds(string value)
        {
            JArray array = (JArray)jObject.GetValue(value);
            List<Feed> fs = new List<Feed>();
            if (!(array is null))
                foreach (JObject item in array)
                    if (!string.IsNullOrEmpty(item.ToString()))
                        fs.Add(new Feed(item));
            return fs.ToArray();
        }
        public string[] GetSmallImagesUrl(string key, bool isReturnFakePic)
        {
            if (jObject.TryGetValue(key, out JToken token))
            {
                JArray array = (JArray)token;
                if (array is null) return new string[] { };
                else
                {
                    List<string> s = new List<string>();
                    foreach (var item in array)
                        if (!string.IsNullOrEmpty(item.ToString()))
                            if (Convert.ToBoolean(localSettings.Values["IsNoPicsMode"]) && isReturnFakePic)
                                if (Convert.ToBoolean(localSettings.Values["IsDarkMode"]))
                                    s.Add("ms-appx:/Assets/img_placeholder_night.png");
                                else s.Add("ms-appx:/Assets/img_placeholder.png");
                            else s.Add(item.ToString() + ".s.jpg");
                    return s.ToArray();
                }
            }
            return new string[] { };
        }
        public string[] GetSmallImagesUrl(string path, string key, bool isReturnFakePic)
        {
            if (jObject.TryGetValue(path, out JToken t))
            {
                JObject jObject = t as JObject;
                if (!(jObject is null) && jObject.TryGetValue(key, out JToken token))
                {
                    JArray array = (JArray)token;
                    if (array is null) return new string[] { };
                    else
                    {
                        List<string> s = new List<string>();
                        foreach (var item in array)
                            if (!string.IsNullOrEmpty(item.ToString()))
                                if (Convert.ToBoolean(localSettings.Values["IsNoPicsMode"]) && isReturnFakePic)
                                    if (Convert.ToBoolean(localSettings.Values["IsDarkMode"]))
                                        s.Add("ms-appx:/Assets/img_placeholder_night.png");
                                    else s.Add("ms-appx:/Assets/img_placeholder.png");
                                else s.Add(item.ToString() + ".s.jpg");
                        return s.ToArray();
                    }
                }
                return new string[] { };
            }
            return new string[] { };
        }
        public bool GetVisibility(string key)
        {
            if (jObject.TryGetValue(key, out JToken token))
            {
                if (string.IsNullOrEmpty(token.ToString())) return false;
                else switch (key)
                    {
                        case "isStickTop":
                        case "isFeedAuthor":
                            if (token.ToString() == "1") return true;
                            else return false;
                        case "feedType":
                            if (token.ToString() == "answer") return true;
                            else return false;
                        case "v": return true;
                        default: return true;
                    }
            }
            else if (key == "v2")
            {
                if (jObject.TryGetValue("v", out JToken token2)) return false;
                else return true;
            }
            else if (key == "feedType2")
            {
                if (jObject.TryGetValue("feedType", out JToken token2))
                    if (token2.ToString() == "question") return false;
                return true;
            }
            else return false;
        }
        public bool GetVisibility(string path, string key)
        {
            if (jObject.TryGetValue(path, out JToken t))
            {
                JObject jObject = t as JObject;
                if (!(jObject is null) && jObject.TryGetValue(path, out JToken token))
                {
                    if (string.IsNullOrEmpty(token.ToString())) return false;
                    else switch (key)
                        {
                            case "isFeedAuthor":
                                if (token.ToString() == "1") return true;
                                else return false;
                            case "feedType":
                                if (token.ToString() == "answer") return true;
                                else return false;
                            default: return true;
                        }
                }
                else return true;
            }
            else return false;
        }
        public bool GetVisibility2(string key)
        {
            switch (key)
            {
                case "picArr": return jObject[key].HasValues ? true : false;
                default: return GetFeeds(key).Count() > 0 ? true : false;
            }
        }
        public Feed[] GetSelfs() => new Feed[] { this };
    }
    class Feed2 : Feed
    {
        JToken jToken = null;
        public string ListType { get; private set; }
        public Feed2(JToken token, string type) : this(new JObject(), type) => jToken = token;

        public Feed2(JObject jObject) : base(jObject) { }
        public Feed2(JObject jObject, string type) : base(jObject) => ListType = type;

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
