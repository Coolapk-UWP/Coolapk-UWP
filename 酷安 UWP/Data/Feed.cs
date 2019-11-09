using CoolapkUWP.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Data.Json;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace CoolapkUWP
{
    public class Feed : Control.ViewModels.IEntity
    {
        public JsonObject jObject;
        public Feed(JsonObject jObject)
        {
            this.jObject = jObject;
            if (jObject.TryGetValue("entityId", out IJsonValue value1)) entityId = value1.ToString().Replace("\"", string.Empty);
            if (jObject.TryGetValue("entityFixed", out IJsonValue value2) && value2.ToString().Replace("\"", string.Empty) == "1") entityFixed = true;
            if (jObject.TryGetValue("entityType", out IJsonValue value3)) entityType = value3.ToString().Replace("\"", string.Empty);
        }

        public string entityId { get; private set; }
        public bool entityFixed { get; private set; }
        public string entityType { get; private set; }

        public Style ListViewStyle
        {
            get
            {
                if (Settings.IsMobile) return Application.Current.Resources["ListViewStyle2Mobile"] as Style;
                else return Application.Current.Resources["ListViewStyle2Desktop"] as Style;
            }
        }

        public string GetValue(string key)
        {
            if (!(jObject is null) && jObject.TryGetValue(key, out IJsonValue token))
                switch (key)
                {
                    case "message": return Tools.GetMessageText(token.ToString().Replace("\"", string.Empty));
                    case "message_raw_output":
                        JsonArray array = JsonArray.Parse(token.ToString().Replace("\"", string.Empty));
                        string s = string.Empty;
                        foreach (var item in array)
                        {
                            if (item.GetObject()["type"].ToString().Replace("\"", string.Empty) == "text")
                                s += Tools.GetMessageText(item.GetObject()["message"].ToString().Replace("\"", string.Empty));
                            else if (item.GetObject()["type"].ToString().Replace("\"", string.Empty) == "image")
                            {
                                string d = string.IsNullOrEmpty(item.GetObject()["description"].ToString().Replace("\"", string.Empty)) ? string.Empty : item.GetObject()["description"].ToString().Replace("\"", string.Empty);
                                s += $"\n\n![image]({item.GetObject()["url"].ToString().Replace("\"", string.Empty)}.s.jpg)\n\n>{d}\n\n";
                            }
                        }
                        return s;
                    case "extra_url":
                        if (!string.IsNullOrEmpty(token.ToString().Replace("\"", string.Empty)))
                            if (token.ToString().Replace("\"", string.Empty).IndexOf("http") == 0)
                                return new Uri(token.ToString().Replace("\"", string.Empty)).Host;
                            else return string.Empty;
                        else return string.Empty;
                    case "infoHtml": return token.ToString().Replace("\"", string.Empty).Replace("&nbsp;", string.Empty);
                    case "createdate":
                    case "dateline": return Tools.ConvertTime(double.Parse(token.ToString().Replace("\"", string.Empty)));
                    case "logintime": return Tools.ConvertTime(double.Parse(token.ToString().Replace("\"", string.Empty))) + "活跃";
                    default:
                        switch (token.ValueType)
                        {
                            case JsonValueType.Null:
                                return null;
                            case JsonValueType.Boolean:
                                return token.GetBoolean().ToString();
                            case JsonValueType.Number:
                                return token.ToString().Replace("\"", string.Empty).ToString();
                            case JsonValueType.String:
                                return token.ToString().Replace("\"", string.Empty);
                            case JsonValueType.Array:
                                return token.GetArray().ToString();
                            case JsonValueType.Object:
                                return token.GetObject().ToString();
                        }
                        return token.ToString();
                }
            else switch (key)
                {
                    case "message2":
                        if (string.IsNullOrEmpty(jObject["pic"].ToString().Replace("\"", string.Empty)))
                            return $"[{jObject["username"]}](/u/{jObject["uid"]})：{Tools.GetMessageText(jObject["message"].ToString().Replace("\"", string.Empty))}";
                        else
                            return $"[{jObject["username"]}](/u/{jObject["uid"]})：{Tools.GetMessageText(jObject["message"].ToString().Replace("\"", string.Empty))}\n[查看图片]({jObject["pic"]})";
                    case "extra_url2": return jObject["extra_url"].ToString().Replace("\"", string.Empty);
                    case "dyh_id2": return "/dyh/" + jObject["dyh_id"].ToString().Replace("\"", string.Empty);
                    default: return string.Empty;
                }
        }

        public string GetValue(string path, string key)
        {
            if (!(jObject is null) && jObject.TryGetValue(path, out IJsonValue t))
            {
                JsonObject jObject = t.GetObject();
                if (!(jObject is null) && jObject.TryGetValue(key, out IJsonValue token))
                {
                    switch (key)
                    {
                        case "message": return Tools.GetMessageText(token.ToString().Replace("\"", string.Empty));
                        case "message_raw_output":
                            JsonArray array = JsonArray.Parse(token.ToString().Replace("\"", string.Empty));
                            string s = string.Empty;
                            foreach (var item in array)
                            {
                                if (item.GetObject()["type"].ToString().Replace("\"", string.Empty) == "text")
                                    s += Tools.GetMessageText(item.GetObject()["message"].ToString().Replace("\"", string.Empty));
                                else if (item.GetObject()["type"].ToString().Replace("\"", string.Empty) == "image")
                                    s += $"\n\n![image]({item.GetObject()["url"].ToString().Replace("\"", string.Empty)}.s.jpg)\n\n>{item.GetObject()["description"].ToString().Replace("\"", string.Empty)}\n\n";
                            }
                            return s;
                        case "extra_url":
                            if (!string.IsNullOrEmpty(token.ToString().Replace("\"", string.Empty)))
                                if (token.ToString().Replace("\"", string.Empty).IndexOf("http") == 0)
                                    return new Uri(token.ToString().Replace("\"", string.Empty)).Host;
                                else return string.Empty;
                            else return string.Empty;
                        case "infoHtml": return token.ToString().Replace("\"", string.Empty).Replace("&nbsp;", string.Empty);
                        case "dateline": return Tools.ConvertTime(double.Parse(token.ToString().Replace("\"", string.Empty)));
                        default:
                            switch (token.ValueType)
                            {
                                case JsonValueType.Null:
                                    return null;
                                case JsonValueType.Boolean:
                                    return token.GetBoolean().ToString();
                                case JsonValueType.Number:
                                    return token.ToString().Replace("\"", string.Empty).ToString();
                                case JsonValueType.String:
                                    return token.ToString().Replace("\"", string.Empty);
                                case JsonValueType.Array:
                                    return token.GetArray().ToString();
                                case JsonValueType.Object:
                                    return token.GetObject().ToString();
                            }
                            return token.ToString();
                    }
                }
                return string.Empty;
            }
            else switch (key)
                {
                    case "message2":
                        if (string.IsNullOrEmpty(jObject["pic"].ToString().Replace("\"", string.Empty)))
                            return $"[{jObject["username"]}](/u/{jObject["uid"]})：{Tools.GetMessageText(jObject["message"].ToString().Replace("\"", string.Empty))}";
                        else
                            return $"[{jObject["username"]}](/u/{jObject["uid"]})：{Tools.GetMessageText(jObject["message"].ToString().Replace("\"", string.Empty))}\n[查看图片]({jObject["pic"]})";
                    case "extra_url2": return jObject["extra_url"].ToString().Replace("\"", string.Empty);
                    default: return string.Empty;
                }
        }


        // 获取缩略图
        public ImageSource GetSmallImage(string value)
        {
            if (jObject.TryGetValue(value, out IJsonValue token))
            {
                string s = token.ToString().Replace("\"", string.Empty);
                if (!string.IsNullOrEmpty(s))
                    if (Settings.GetBoolen("IsNoPicsMode"))
                        if (Settings.GetBoolen("IsDarkMode"))
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
            if (jObject.TryGetValue(key, out IJsonValue token))
            {
                string s = token.ToString().Replace("\"", string.Empty);
                if (!string.IsNullOrEmpty(s))
                    if (Settings.GetBoolen("IsNoPicsMode"))
                        if (Settings.GetBoolen("IsDarkMode"))
                            return new BitmapImage(new Uri("ms-appx:/Assets/img_placeholder_night.png"));
                        else return new BitmapImage(new Uri("ms-appx:/Assets/img_placeholder.png"));
                    else return new BitmapImage(new Uri(s));
                else return new BitmapImage();
            }
            else return new BitmapImage();
        }

        public ImageSource GetImage(string path, string key)
        {
            if (jObject.TryGetValue(path, out IJsonValue t))
                if (t.GetObject().TryGetValue(key, out IJsonValue token))
                {
                    string s = token.ToString().Replace("\"", string.Empty);
                    if (!string.IsNullOrEmpty(s))
                        if (Settings.GetBoolen("IsNoPicsMode"))
                            if (Settings.GetBoolen("IsDarkMode"))
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
            if (jObject.TryGetValue(value, out IJsonValue token))
            {
                JsonArray array = token.GetArray();
                foreach (var item in array)
                    if (!string.IsNullOrEmpty(item.ToString().Replace("\"", string.Empty)))
                        if (Settings.GetBoolen("IsNoPicsMode"))
                            if (Settings.GetBoolen("IsDarkMode"))
                                images.Add(new BitmapImage(new Uri("ms-appx:/Assets/img_placeholder_night.png")));
                            else images.Add(new BitmapImage(new Uri("ms-appx:/Assets/img_placeholder.png")));
                        else images.Add(new BitmapImage(new Uri(item.ToString().Replace("\"", string.Empty) + ".s.jpg")));
            }
            return images.ToArray();
        }

        public Feed[] GetFeed(string value)
        {
            if (jObject.TryGetValue(value, out IJsonValue token))
            {
                if (token is null)
                    return new Feed[] { new Feed(new JsonObject()) };
                else if (value == "extraData")
                    return new Feed[] { new Feed(JsonObject.Parse(token.ToString().Replace("\"", string.Empty))) };
                else if (token.ValueType != JsonValueType.Object)
                {
                    if (!jObject.TryGetValue("v", out IJsonValue jtoken))
                        jObject.Add("v", new JsonObject());
                    return new Feed[] { new Feed(new JsonObject()) };
                }
                return new Feed[] { new Feed(token as JsonObject) };
            }
            else return new Feed[] { new Feed(new JsonObject()) };
        }

        public Feed[] GetFeeds(string value)
        {
            JsonArray array = jObject[value].GetArray();
            List<Feed> fs = new List<Feed>();
            if (!(array is null))
                foreach (var item in array)
                    if (!string.IsNullOrEmpty(item.ToString()))
                        fs.Add(new Feed(item.GetObject()));
            return fs.ToArray();
        }
        public string[] GetSmallImagesUrl(string key, bool isReturnFakePic)
        {
            if (jObject.TryGetValue(key, out IJsonValue token))
            {
                JsonArray array = token.GetArray();
                if (array is null) return new string[] { };
                else
                {
                    List<string> s = new List<string>();
                    foreach (var item in array)
                        if (!string.IsNullOrEmpty(item.ToString().Replace("\"", string.Empty)))
                            if (Settings.GetBoolen("IsNoPicsMode") && isReturnFakePic)
                                if (Settings.GetBoolen("IsDarkMode"))
                                    s.Add("ms-appx:/Assets/img_placeholder_night.png");
                                else s.Add("ms-appx:/Assets/img_placeholder.png");
                            else s.Add(item.ToString().Replace("\"", string.Empty) + ".s.jpg");
                    return s.ToArray();
                }
            }
            return new string[] { };
        }
        public string[] GetSmallImagesUrl(string path, string key, bool isReturnFakePic)
        {
            if (jObject.TryGetValue(path, out IJsonValue t))
            {
                JsonObject jObject = t.GetObject();
                if (!(jObject is null) && jObject.TryGetValue(key, out IJsonValue token))
                {
                    JsonArray array = token.GetArray();
                    if (array is null) return new string[] { };
                    else
                    {
                        List<string> s = new List<string>();
                        foreach (var item in array)
                            if (!string.IsNullOrEmpty(item.ToString().Replace("\"", string.Empty)))
                                if (Settings.GetBoolen("IsNoPicsMode") && isReturnFakePic)
                                    if (Settings.GetBoolen("IsDarkMode"))
                                        s.Add("ms-appx:/Assets/img_placeholder_night.png");
                                    else s.Add("ms-appx:/Assets/img_placeholder.png");
                                else s.Add(item.ToString().Replace("\"", string.Empty) + ".s.jpg");
                        return s.ToArray();
                    }
                }
                return new string[] { };
            }
            return new string[] { };
        }
        public bool GetVisibility(string key)
        {
            if (jObject.TryGetValue(key, out IJsonValue token))
            {
                if (string.IsNullOrEmpty(token.ToString().Replace("\"", string.Empty).Replace("{", string.Empty).Replace("}", string.Empty))) return false;
                else switch (key)
                    {
                        case "isStickTop":
                        case "isFeedAuthor":
                            if (token.ToString().Replace("\"", string.Empty) == "1") return true;
                            else return false;
                        case "feedType":
                            if (token.ToString().Replace("\"", string.Empty) == "answer") return true;
                            else return false;
                        case "v": return true;
                        default: return true;
                    }
            }
            else if (key == "v2")
            {
                if (jObject.TryGetValue("v", out IJsonValue token2)) return false;
                else return true;
            }
            else if (key == "feedType2")
            {
                if (jObject.TryGetValue("feedType", out IJsonValue token2))
                    if (token2.ToString().Replace("\"", string.Empty) == "question") return false;
                return true;
            }
            else return false;
        }
        public bool GetVisibility(string path, string key)
        {
            if (jObject.TryGetValue(path, out IJsonValue t))
            {
                JsonObject jObject = t.GetObject();
                if (!(jObject is null) && jObject.TryGetValue(path, out IJsonValue token))
                {
                    if (string.IsNullOrEmpty(token.ToString().Replace("\"", string.Empty))) return false;
                    else switch (key)
                        {
                            case "isFeedAuthor":
                                if (token.ToString().Replace("\"", string.Empty) == "1") return true;
                                else return false;
                            case "feedType":
                                if (token.ToString().Replace("\"", string.Empty) == "answer") return true;
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
                case "picArr": return jObject[key].ValueType != JsonValueType.Null;
                default: return GetFeeds(key).Count() > 0 ? true : false;
            }
        }
        public Feed[] GetSelfs() => new Feed[] { this };
    }
    class Feed2 : Feed
    {
        IJsonValue jToken = null;
        public string ListType { get; private set; }
        public Feed2(IJsonValue token, string type) : this(new JsonObject(), type) => jToken = token;

        public Feed2(JsonObject jObject) : base(jObject) { }
        public Feed2(JsonObject jObject, string type) : base(jObject) => ListType = type;

        public new Feed2[] GetFeeds(string value)
        {
            JsonArray array = new JsonArray();
            if (jToken is null)
                array = jObject[value].GetArray();
            else
                array = jToken.GetArray();
            List<Feed2> fs = new List<Feed2>();
            foreach (var item in array)
                if (!string.IsNullOrEmpty(item.ToString()))
                    fs.Add(new Feed2(item.GetObject()));
            return fs.ToArray();
        }
    }
}
