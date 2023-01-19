using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel;

namespace CoolapkUWP.Models.Feeds
{
    public class FeedModelBase : SourceFeedModel, INotifyPropertyChanged, ICanFollow, ICanLike, ICanReply, ICanStar, ICanCopy
    {
        private int likeNum;
        public int LikeNum
        {
            get => likeNum;
            set
            {
                if (likeNum != value)
                {
                    likeNum = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private int replyNum;
        public int ReplyNum
        {
            get => replyNum;
            set
            {
                if (replyNum != value)
                {
                    replyNum = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private int starNum;
        public int StarNum
        {
            get => starNum;
            set
            {
                if (starNum != value)
                {
                    starNum = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        public bool Liked
        {
            get => UserAction.Like;
            set
            {
                if (UserAction.Like != value)
                {
                    UserAction.Like = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        public bool Followed
        {
            get => UserAction.FollowAuthor;
            set
            {
                if (UserAction.FollowAuthor != value)
                {
                    UserAction.FollowAuthor = value;
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

        public int ID => EntityID;
        public int UID => UserInfo.UID;
        public bool Stared { get; set; }
        public string Info { get; private set; }
        public int ShareNum { get; private set; }
        public string DeviceTitle { get; private set; }
        public bool ShowUser { get; private set; } = true;

        public event PropertyChangedEventHandler PropertyChanged;

        internal void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        public FeedModelBase(JObject token) : base(token)
        {
            if (token.TryGetValue("info", out JToken info) && !string.IsNullOrEmpty(info.ToString()))
            {
                Info = info.ToString();
            }
            else if (token.TryGetValue("feedTypeName", out JToken feedTypeName))
            {
                Info = feedTypeName.ToString();
            }

            if (token.TryGetValue("likenum", out JToken likenum))
            {
                LikeNum = Convert.ToInt32(likenum.ToString());
            }

            if (token.TryGetValue("favnum", out JToken favnum))
            {
                StarNum = Convert.ToInt32(favnum.ToString());
            }

            if (token.TryGetValue("replynum", out JToken replynum))
            {
                ReplyNum = Convert.ToInt32(replynum.ToString());
            }

            if (token.TryGetValue("forwardnum", out JToken forwardnum))
            {
                ShareNum = Convert.ToInt32(forwardnum.ToString());
            }

            if (token.TryGetValue("device_title", out JToken device_title))
            {
                DeviceTitle = device_title.ToString();
            }
        }
    }
}
