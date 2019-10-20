using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using 酷安_UWP.Data;

namespace 酷安_UWP
{
    static class Process
    {
        public static string ProcessMessage(string s, ApplicationDataContainer localSettings)
        {
            s = s.Replace("\n", "\n\n");
            s = s.Replace("&#039;", "\'");
            s = s.Replace("</a>", "</a> ");
            foreach (var i in Emojis.emojis)
            {
                if (s.Contains(i))
                {
                    if (i.Contains('('))
                        s = s.Replace($"#{i})", $"\n![#{i})](ms-appx:/Emoji/{i}.png =24)");
                    else if (Convert.ToBoolean(localSettings.Values["IsUseOldEmojiMode"]))
                        if (Emojis.oldEmojis.Contains(s))
                            s = s.Replace(i, $"\n![{i}(ms-appx:/Emoji/{i}2.png =24)");
                        else s = s.Replace(i, $"\n![{i}(ms-appx:/Emoji/{i}.png =24)");
                    else s = s.Replace(i, $"\n![{i}(ms-appx:/Emoji/{i}.png =24)");
                }
            }
            Regex regex = new Regex("<a.*?>\\S*"), regex2 = new Regex("href=\".*"), regex3 = new Regex(">.*<");
            while (regex.IsMatch(s))
            {
                var h = regex.Match(s);
                if (!h.Value.Contains("</a>"))
                {
                    s = s.Replace(h.Value + " ", h.Value);
                    continue;
                }
                string t = regex3.Match(h.Value).Value.Replace(">", string.Empty);
                t = t.Replace("<", string.Empty);
                string tt = regex2.Match(h.Value).Value.Replace("href=", string.Empty);
                tt = tt.Replace("\"", string.Empty);
                tt = tt.Replace($">{t}</a>", string.Empty);
                if (t == "查看更多") tt = "getmore";
                s = s.Replace(h.Value, $"[{t}]({tt})");
            }
            return s;
        }

        public static string ConvertTime(string timestr)
        {
            DateTime time = new DateTime(1970, 1, 1).ToLocalTime().Add(new TimeSpan(Convert.ToInt64(timestr + "0000000")));
            TimeSpan tt = DateTime.Now.Subtract(time);
            if (tt.TotalDays > 30)
                return $"{time.Year}/{time.Month}/{time.Day}";
            else if (tt.Days > 0)
                return $"{tt.Days}天前";
            else if (tt.Hours > 0)
                return $"{tt.Hours}小时前";
            else if (tt.Minutes > 0)
                return $"{tt.Minutes}分钟前";
            else return "刚刚";
        }

    }

    public class Feed
    {
        static ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        public JObject jObject;
        public Feed(JObject jObject) => this.jObject = jObject;

        public string GetValue(string path, string key)
        {
            if (!(jObject is null) && jObject.TryGetValue(path, out JToken t))
            {
                JObject jObject = t as JObject;
                if (!(jObject is null) && jObject.TryGetValue(key, out JToken token))
                {
                    if (key == "message") return Process.ProcessMessage(token.ToString(), localSettings);
                    else if (key == "message_raw_output")
                    {
                        JArray array = JArray.Parse(token.ToString());
                        string s = string.Empty;
                        foreach (JObject item in array)
                        {
                            if (item["type"].ToString() == "text")
                                s += Process.ProcessMessage(item["message"].ToString(), localSettings);
                            else if (item["type"].ToString() == "image")
                                s += $"\n\n![image]({item["url"].ToString()}.s.jpg)\n\n>{item["description"].ToString()}\n\n";
                        }
                        return s;
                    }
                    else if (key == "extra_url")
                    {
                        if (!string.IsNullOrEmpty(token.ToString()))
                            if (token.ToString().IndexOf("http") == 0)
                                return new Uri(token.ToString()).Host;
                            else return string.Empty;
                        else return string.Empty;
                    }
                    else if (key == "infoHtml") return token.ToString().Replace("&nbsp;", string.Empty);
                    else if (key == "dateline") return Process.ConvertTime(token.ToString());
                    else return token.ToString();
                }
                else if (key == "message2")
                {
                    if (string.IsNullOrEmpty(jObject["pic"].ToString()))
                        return $"[{jObject["username"]}](/u/{jObject["uid"]})：{Process.ProcessMessage(jObject["message"].ToString(), localSettings)}";
                    else
                        return $"[{jObject["username"]}](/u/{jObject["uid"]})：{Process.ProcessMessage(jObject["message"].ToString(), localSettings)}\n[查看图片]({jObject["pic"]})";
                }
                else if (key == "extra_url2") return jObject["extra_url"].ToString();
                else return string.Empty;
            }
            else return string.Empty;
        }

        public string GetValue(string key)
        {
            if (!(jObject is null) && jObject.TryGetValue(key, out JToken token))
                if (key == "message") return Process.ProcessMessage(token.ToString(), localSettings);
                else if (key == "message_raw_output")
                {
                    JArray array = JArray.Parse(token.ToString());
                    string s = string.Empty;
                    foreach (JObject item in array)
                    {
                        if (item["type"].ToString() == "text")
                            s += Process.ProcessMessage(item["message"].ToString(), localSettings);
                        else if (item["type"].ToString() == "image")
                            s += $"\n\n![image]({item["url"].ToString()}.s.jpg)\n\n>{item["description"].ToString()}\n\n";
                    }
                    return s;
                }
                else if (key == "extra_url")
                {
                    if (!string.IsNullOrEmpty(token.ToString()))
                        if (token.ToString().IndexOf("http") == 0)
                            return new Uri(token.ToString()).Host;
                        else return string.Empty;
                    else return string.Empty;
                }
                else if (key == "infoHtml") return token.ToString().Replace("&nbsp;", string.Empty);
                else if (key == "dateline") return Process.ConvertTime(token.ToString());
                else if (key == "logintime") return Process.ConvertTime(token.ToString()) + "活跃";
                else return token.ToString();
            else if (key == "message2")
            {
                if (string.IsNullOrEmpty(jObject["pic"].ToString()))
                    return $"[{jObject["username"]}](/u/{jObject["uid"]})：{Process.ProcessMessage(jObject["message"].ToString(), localSettings)}";
                else
                    return $"[{jObject["username"]}](/u/{jObject["uid"]})：{Process.ProcessMessage(jObject["message"].ToString(), localSettings)}\n[查看图片]({jObject["pic"]})";
            }
            else if (key == "extra_url2") return jObject["extra_url"].ToString();
            else return string.Empty;
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
        public ImageSource GetImage(string value)
        {
            if (jObject.TryGetValue(value, out JToken token))
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
        public Visibility GetVisibility(string key)
        {
            if (jObject.TryGetValue(key, out JToken token))
            {
                if (string.IsNullOrEmpty(token.ToString())) return Visibility.Collapsed;
                else if (key == "isFeedAuthor")
                    if (token.ToString() == "1")
                        return Visibility.Visible;
                    else return Visibility.Collapsed;
                else if (key == "feedType")
                    if (token.ToString() == "answer")
                        return Visibility.Visible;
                    else return Visibility.Collapsed;
                else if (key == "v") return Visibility.Visible;
                else return Visibility.Visible;
            }
            else if (key == "v2")
            {
                if (jObject.TryGetValue("v", out JToken token2))
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
            else return Visibility.Collapsed;
        }
        public Visibility GetVisibility(string path, string key)
        {
            if (jObject.TryGetValue(path, out JToken t))
            {
                JObject jObject = t as JObject;
                if (!(jObject is null) && jObject.TryGetValue(path, out JToken token))
                {
                    if (string.IsNullOrEmpty(token.ToString())) return Visibility.Collapsed;
                    else if (key == "feedType")
                        if (token.ToString() == "answer")
                            return Visibility.Visible;
                        else return Visibility.Collapsed;
                    else return Visibility.Visible;
                }
                else return Visibility.Visible;
            }
            else return Visibility.Collapsed;
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
