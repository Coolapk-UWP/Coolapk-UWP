using CoolapkUWP.Data;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Windows.Data.Json;
using Windows.UI.Xaml.Media;

namespace CoolapkUWP.Control.ViewModels
{
    internal class FeedReplyViewModel : SimpleFeedReplyViewModel, INotifyPropertyChanged, ILike
    {
        public bool Liked { get; set; }
        public bool Liked2 { get => !Liked; }
        public bool ShowReplyRows { get; set; }
        public bool ShowReplyRowsMore { get => ReplyRowsMore > 0; }
        public double ReplyRowsMore { get; private set; }
        public double ReplyRowsCount { get; private set; }

        public string Replynum { get; private set; }
        public string Dateline { get; private set; }
        public new string Message { get; private set; }

        string ILike.id => Id.ToString();
        private readonly string userSmallAvatarUrl;
        public SimpleFeedReplyViewModel[] ReplyRows { get; private set; }

        private string likenum;
        public string Likenum
        {
            get => likenum;
            set
            {
                likenum = value;
                RaisePropertyChangedEvent();
            }
        }

        private ImageData pic;
        public ImageData Pic
        {
            get => pic;
            private set
            {
                pic = value;
                RaisePropertyChangedEvent();
            }
        }

        private ImageSource userSmallAvatar;
        public ImageSource UserSmallAvatar
        {
            get => userSmallAvatar;
            private set
            {
                userSmallAvatar = value;
                RaisePropertyChangedEvent();
            }
        }

        public FeedReplyViewModel(IJsonValue t, bool ShowReplyRow = true) : base(t)
        {
            JsonObject token = t.GetObject();
            if (token.TryGetValue("dateline", out IJsonValue dateline))
            {
                Dateline = UIHelper.ConvertTime(dateline.GetNumber());
            }
            if (token.TryGetValue("message", out IJsonValue message))
            {
                Message = message.GetString();
            }
            if (token.TryGetValue("userInfo", out IJsonValue v))
            {
                JsonObject userInfo = v.GetObject();
                if (userInfo.TryGetValue("userSmallAvatar", out IJsonValue userSmallAvatar))
                {
                    userSmallAvatarUrl = userSmallAvatar.GetString();
                    if (ImageCache.defaultNoAvatarUrl.Contains(userSmallAvatarUrl))
                    {
                        GetAvatar();
                    }
                    else
                    {
                        UserSmallAvatar = ImageCache.NoPic;
                    }
                }
            }
            if (token.TryGetValue("likenum", out IJsonValue likenum))
            {
                Likenum = likenum.GetNumber().ToString();
            }
            if (token.TryGetValue("replynum", out IJsonValue replynum))
            {
                Replynum = replynum.GetNumber().ToString();
            }
            if (token.TryGetValue("replyRowsCount", out IJsonValue replyRowsCount))
            {
                ReplyRowsCount = replyRowsCount?.GetNumber() ?? 0;
            }
            if (ShowReplyRow && ReplyRowsCount > 0)
            {
                ShowReplyRows = true;
                if (token.TryGetValue("replyRows", out IJsonValue replyRows))
                {
                    List<SimpleFeedReplyViewModel> models = new List<SimpleFeedReplyViewModel>();
                    foreach (IJsonValue item in replyRows.GetArray())
                    {
                        models.Add(new SimpleFeedReplyViewModel(item));
                    }
                    ReplyRows = models.ToArray();
                    if (token.TryGetValue("replyRowsMore", out IJsonValue replyRowsMore))
                    {
                        ReplyRowsMore = replyRowsMore.GetNumber();
                    }
                }
            }
            Liked = token.TryGetValue("userAction", out IJsonValue userAction) && userAction.GetObject().TryGetValue("like", out IJsonValue like) && like.GetNumber() == 1;
            GetPic();
        }

        private async void GetPic()
        {
            if (ShowPic)
            {
                Pic = new ImageData
                {
                    Pic = await ImageCache.GetImage(ImageType.SmallImage, PicUrl),
                    url = PicUrl
                };
            }
        }

        private async void GetAvatar()
        {
            UserSmallAvatar = await ImageCache.GetImage(ImageType.SmallAvatar, userSmallAvatarUrl);
        }

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
