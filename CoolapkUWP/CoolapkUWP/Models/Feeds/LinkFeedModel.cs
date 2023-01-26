using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Images;
using CoolapkUWP.Models.Users;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using Windows.ApplicationModel.Resources;

namespace CoolapkUWP.Models.Feeds
{
    public class LinkFeedModel : INotifyPropertyChanged
    {
        private string url;
        public string Url
        {
            get => url;
            set
            {
                url = value;
                RaisePropertyChangedEvent();
            }
        }

        private bool succeed;
        public bool Succeed
        {
            get => succeed;
            set
            {
                succeed = value;
                RaisePropertyChangedEvent();
            }
        }

        private string message;
        public string Message
        {
            get => message;
            set
            {
                message = value;
                RaisePropertyChangedEvent();
            }
        }

        private string messageTitle;
        public string MessageTitle
        {
            get => messageTitle;
            set
            {
                messageTitle = value;
                RaisePropertyChangedEvent();
            }
        }

        private string dateline;
        public string Dateline
        {
            get => dateline;
            set
            {
                dateline = value;
                RaisePropertyChangedEvent();
            }
        }

        private bool showPicArr;
        public bool ShowPicArr
        {
            get => showPicArr;
            set
            {
                showPicArr = value;
                RaisePropertyChangedEvent();
            }
        }

        private bool showUser = true;
        public bool ShowUser
        {
            get => showUser;
            set
            {
                if (showUser != value)
                {
                    showUser = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private bool isCopyEnabled;
        public bool IsCopyEnabled
        {
            get => isCopyEnabled;
            set
            {
                if (isCopyEnabled != value)
                {
                    isCopyEnabled = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private ImmutableArray<ImageModel> picArr;
        public ImmutableArray<ImageModel> PicArr
        {
            get => picArr;
            set
            {
                picArr = value;
                RaisePropertyChangedEvent();
            }
        }

        private LinkUserModel userInfo;
        public LinkUserModel UserInfo
        {
            get => userInfo;
            set
            {
                userInfo = value;
                RaisePropertyChangedEvent();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        internal void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        public LinkFeedModel(Uri uri, LinkType type, bool isPost = false, MultipartFormDataContent content = null)
        {
            PicArr = ImmutableArray<ImageModel>.Empty;
            if (!string.IsNullOrEmpty(uri.ToString())) { GetJson(uri, type, isPost, content); }
        }

        private async void GetJson(Uri uri, LinkType type, bool isPost, MultipartFormDataContent content)
        {
            bool isSucceed;
            string result;
            if (isPost) { (isSucceed, result) = await RequestHelper.PostStringAsync(uri, content); }
            else { (isSucceed, result) = await RequestHelper.GetStringAsync(uri, "XMLHttpRequest"); }
            if (isSucceed && !string.IsNullOrEmpty(result))
            {
                JObject json = JObject.Parse(result);
                ReadJson(json, type);
            }
        }

        private void ReadJson(JObject json, LinkType type)
        {
            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("Feed");
            switch (type)
            {
                case LinkType.Coolapk:
                    {
                        if (json.TryGetValue("data", out JToken v1))
                        {
                            JObject data = (JObject)v1;
                            if (data.TryGetValue("userInfo", out JToken v2))
                            {
                                JObject userInfo = (JObject)v2;
                                LinkUserModel UserModel = new LinkUserModel();
                                if (userInfo.TryGetValue("url", out JToken uurl))
                                {
                                    UserModel.Url = uurl.ToString();
                                }
                                if (userInfo.TryGetValue("username", out JToken username))
                                {
                                    UserModel.UserName = username.ToString();
                                }
                                UserInfo = UserModel;
                            }
                            if (data.TryGetValue("url", out JToken url))
                            {
                                Url = url.ToString();
                            }
                            if (data.TryGetValue("feedType", out JToken feedType) && feedType.ToString() == "feedArticle")
                            {
                                if (data.TryGetValue("message", out JToken message))
                                {
                                    Message = message.ToString();
                                    if (Message.Contains("</a>") ? Message.Length - 200 >= 7 : Message.Length - 120 >= 7)
                                    {
                                        Message = message.ToString().Substring(0, 120);
                                        Message = Message.Contains("</a>") ? message.ToString().Substring(0, 200) + "...<a href=\"" + Url + "\">" + loader.GetString("Readmore") + "</a>" : Message + "...<a href=\"" + Url + "\">" + loader.GetString("readmore") + "</a>";
                                    }
                                }
                            }
                            else
                            {
                                if (data.TryGetValue("message", out JToken message))
                                {
                                    Message = message.ToString();
                                }
                            }
                            if (data.TryGetValue("dateline", out JToken dateline))
                            {
                                Dateline = dateline.ToObject<long>().ConvertUnixTimeStampToReadable();
                            }
                            if (data.TryGetValue("message_title", out JToken message_title))
                            {
                                MessageTitle = message_title.ToString();
                            }
                            ShowPicArr = data.TryGetValue("picArr", out JToken picArr) && (picArr as JArray).Count > 0 && picArr != null;
                            if (ShowPicArr)
                            {
                                PicArr = (from item in picArr
                                          select new ImageModel(item.ToString(), ImageType.Icon)).ToImmutableArray();

                                foreach (ImageModel item in PicArr)
                                {
                                    item.ContextArray = PicArr;
                                }
                            }
                        }
                    }
                    Succeed = true;
                    break;
                case LinkType.Bilibili:
                    {
                        if (json.TryGetValue("data", out JToken v1))
                        {
                            JObject data = (JObject)v1;
                            if (data.TryGetValue("card", out JToken v2))
                            {
                                JObject card = (JObject)v2;
                                if (card.TryGetValue("card", out JToken v3))
                                {
                                    JObject card1 = JObject.Parse(v3.ToString());
                                    if (card1.TryGetValue("item", out JToken v4))
                                    {
                                        JObject item = (JObject)v4;
                                        if (item.TryGetValue("description", out JToken description))
                                        {
                                            Message = description.ToString();
                                        }
                                        if (item.TryGetValue("title", out JToken title))
                                        {
                                            MessageTitle = title.ToString();
                                        }
                                        if (item.TryGetValue("upload_time", out JToken upload_time))
                                        {
                                            Dateline = upload_time.ToObject<long>().ConvertUnixTimeStampToReadable();
                                        }
                                        if (item.TryGetValue("pictures", out JToken pictures))
                                        {
                                            ShowPicArr = ((JArray)pictures).Any();
                                            PicArr = (from items in pictures as JArray
                                                      select new ImageModel((items as JObject).Value<string>("img_src").Replace("\"", string.Empty), ImageType.OriginImage)).ToImmutableArray();
                                            foreach (ImageModel items in PicArr)
                                            {
                                                items.ContextArray = PicArr;
                                            }
                                        }
                                    }
                                    if (card1.TryGetValue("user", out JToken v5))
                                    {
                                        JObject user = (JObject)v5;
                                        LinkUserModel UserModel = new LinkUserModel();
                                        if (user.TryGetValue("name", out JToken name))
                                        {
                                            UserModel.UserName = name.ToString();
                                        }
                                        if (user.TryGetValue("uid", out JToken uid))
                                        {
                                            UserModel.Url = "https://space.bilibili.com/" + uid.ToString();
                                        }
                                        UserInfo = UserModel;
                                    }
                                }
                            }
                            if (data.TryGetValue("desc", out JToken v6))
                            {
                                JObject desc = (JObject)v6;
                                if (data.TryGetValue("dynamic_id_str", out JToken dynamic_id_str))
                                {
                                    Url = "https://t.bilibili.com/" + dynamic_id_str;
                                }
                            }
                            if (Message.Length - 120 >= 7)
                            {
                                Message = message.ToString().Substring(0, 120) + "...<a href=\"" + Url + "\">";
                            }
                        }
                    }
                    break;
                case LinkType.ITHome:
                    {
                        if (json.TryGetValue("data", out JToken v1))
                        {
                            JObject data = (JObject)v1;
                            if (data.TryGetValue("id", out JToken id))
                            {
                                Url = $"ithome://qcontent?id={id.ToString().Replace("\"", string.Empty)}";
                            }
                            if (data.TryGetValue("contents", out JToken contents))
                            {
                                foreach (JObject v in contents as JArray)
                                {
                                    if (v.TryGetValue("content", out JToken content) && v.TryGetValue("type", out JToken type2))
                                    {
                                        switch (type2.ToString())
                                        {
                                            case "0":
                                                Message += content.ToString();
                                                break;
                                            case "2":
                                                if (v.TryGetValue("link", out JToken link) && !string.IsNullOrEmpty(link.ToString()))
                                                { Message += "<a class=\"feed-link-url\" href=\"" + link.ToString() + "\" target=\"_blank\" rel=\"nofollow\">查看链接</a>"; }
                                                else { Message += content.ToString(); }
                                                break;
                                            case "3":
                                                if (v.TryGetValue("topicId", out JToken topicId) && !string.IsNullOrEmpty(topicId.ToString()))
                                                { Message += "<a class=\"feed-link-tag\" href=\"" + "ithome://qtopic?id=" + topicId.ToString() + "\">" + content.ToString() + "</a>"; }
                                                else { Message += content.ToString(); }
                                                break;
                                            default:
                                                Message += content.ToString();
                                                break;
                                        }
                                    }
                                }
                                if (Message.Length - 120 >= 7)
                                {
                                    Message = message.ToString().Substring(0, 120) + "...<a href=\"" + Url + "\">";
                                }
                            }
                            if (data.TryGetValue("user", out JToken v2))
                            {
                                JObject user = (JObject)v2;
                                LinkUserModel UserModel = new LinkUserModel();
                                if (user.TryGetValue("userNick", out JToken userNick))
                                {
                                    UserModel.UserName = userNick.ToString();
                                }
                                UserInfo = UserModel;
                            }
                            if (data.TryGetValue("pictures", out JToken pictures))
                            {
                                ShowPicArr = ((JArray)pictures).Any();
                                PicArr = (from item in pictures as JArray
                                          select new ImageModel((item as JObject).Value<string>("src"), ImageType.OriginImage)).ToImmutableArray();
                                foreach (ImageModel item in PicArr)
                                {
                                    item.ContextArray = PicArr;
                                }
                            }
                            if (data.TryGetValue("createTime", out JToken createTime))
                            {
                                Dateline = Convert.ToInt64(Convert.ToDateTime(createTime.ToString()).ConvertDateTimeToUnixTimeStamp()).ConvertUnixTimeStampToReadable();
                            }
                        }
                    }
                    Succeed = true;
                    break;
                default: break;
            }
        }
    }
}
