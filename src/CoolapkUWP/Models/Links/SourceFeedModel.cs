using CoolapkUWP.Core.Helpers;
using CoolapkUWP.Core.Models;
using CoolapkUWP.Helpers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace CoolapkUWP.Models.Links
{
    public enum LinkType
    {
        Coolapk,
        ITHome,
    }

    public class SourceFeedModel : INotifyPropertyChanged
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

        private bool showMessageTitle;
        public bool ShowMessageTitle
        {
            get => showMessageTitle;
            set
            {
                showMessageTitle = !string.IsNullOrEmpty(MessageTitle);
                RaisePropertyChangedEvent();
            }
        }

        private string uurl;
        public string Uurl
        {
            get => uurl;
            set
            {
                uurl = value;
                RaisePropertyChangedEvent();
            }
        }

        private string username;
        public string Username
        {
            get => username;
            set
            {
                username = value;
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

        private ImageModel userSmallAvatar;
        public ImageModel UserSmallAvatar
        {
            get => userSmallAvatar;
            set
            {
                userSmallAvatar = value;
                RaisePropertyChangedEvent();
            }
        }

        public SourceFeedModel(Uri uri, LinkType type)
        {
            PicArr = ImmutableArray<ImageModel>.Empty;
            GetJson(uri, type);
        }

        private async void GetJson(Uri uri, LinkType type)
        {
            (bool isSucceed, string result) = await DataHelper.GetHtmlAsync(uri, "XMLHttpRequest");
            if (isSucceed)
            {
                JObject o = JObject.Parse(result);
                switch (type)
                {
                    case LinkType.Coolapk:
                        {
                            if (o.TryGetValue("data", out JToken v1))
                            {
                                JObject data = (JObject)v1;
                                if (data.TryGetValue("userInfo", out JToken v2))
                                {
                                    JObject userInfo = (JObject)v2;
                                    if (userInfo.TryGetValue("url", out JToken uurl))
                                    {
                                        Uurl = uurl.ToString();
                                    }
                                    if (userInfo.TryGetValue("username", out JToken username))
                                    {
                                        Username = username.ToString();
                                    }
                                    if (userInfo.TryGetValue("userSmallAvatar", out JToken userSmallAvatar))
                                    {
                                        UserSmallAvatar = new ImageModel(userSmallAvatar.ToString(), ImageType.BigAvatar);
                                    }
                                }
                                if (data.TryGetValue("url", out JToken url))
                                {
                                    Url = url.ToString();
                                }
                                if (data.TryGetValue("feedType", out JToken feedType) && feedType.ToString() == "feedArticle")
                                {
                                    if (data.TryGetValue("message", out JToken message))
                                    {
                                        Message = message.ToString().Substring(0, 120);
                                        Message = Message.Contains("</a>") ? message.ToString().Substring(0, 200) + "……<a href=\"" + Url + "\">查看更多</a>" : Message + "……<a href=\"" + Url + "\">查看更多</a>";
                                    }
                                }
                                else
                                {
                                    if (data.TryGetValue("message", out JToken message))
                                    {
                                        Message = message.ToString();
                                        //Message = Message.PadLeft(1).Remove(0, 1);
                                        //Message = Message.PadRight(1);
                                        //Message = Message.Remove(Message.Length - 1, 1);
                                    }
                                }
                                if (data.TryGetValue("dateline", out JToken dateline))
                                {
                                    Dateline = DataHelper.ConvertUnixTimeStampToReadable((int)dateline);
                                }
                                if (data.TryGetValue("message_title", out JToken message_title))
                                {
                                    MessageTitle = message_title.ToString();
                                }
                                ShowPicArr = data.TryGetValue("picArr", out JToken picArr) && (picArr as JArray).Count > 0 && !string.IsNullOrEmpty((picArr as JArray)[0].ToString());
                                if (picArr != null)
                                {
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
                        }
                        Succeed = true;
                        break;
                    case LinkType.ITHome:
                        {
                            if (o.TryGetValue("data", out JToken v1))
                            {
                                JObject data = (JObject)v1;
                                if (data.TryGetValue("id", out JToken id))
                                {
                                    Url = $"ithome://qcontent?id={id.ToString().Replace("\"", string.Empty, StringComparison.Ordinal)}";
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
                                }
                                if (data.TryGetValue("user", out JToken v2))
                                {
                                    JObject user = (JObject)v2;
                                    if (user.TryGetValue("userNick", out JToken userNick))
                                    {
                                        Username = userNick.ToString();
                                    }
                                }
                                if (data.TryGetValue("pictures", out JToken pictures))
                                {
                                    ShowPicArr = ((JArray)pictures).Any();
                                    PicArr = (from item in pictures as JArray
                                              select new ImageModel((item as JObject).Value<string>("src"), ImageType.SmallImage)).ToImmutableArray();
                                    foreach (ImageModel item in PicArr)
                                    {
                                        item.ContextArray = PicArr;
                                    }
                                }
                                if (data.TryGetValue("createTime", out JToken createTime))
                                {
                                    Dateline = DataHelper.ConvertUnixTimeStampToReadable(Utils.ConvertDateTimeToUnixTimeStamp(Convert.ToDateTime(createTime.ToString())));
                                }
                            }
                        }
                        Succeed = true;
                        break;
                    default: break;
                }
            }
        }

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}