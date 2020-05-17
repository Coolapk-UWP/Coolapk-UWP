using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel;
using Windows.System.Threading;

namespace CoolapkUWP.Helpers
{
    class NotificationsNum : INotifyPropertyChanged
    {
        public event EventHandler BadgeNumberChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        private double badgeNum, followNum, messageNum, atMeNum, atCommentMeNum, commentMeNum, feedLikeNum;
        public double BadgeNum
        {
            get => badgeNum;
            private set
            {
                if (value != badgeNum)
                {
                    badgeNum = value;
                    BadgeNumberChanged?.Invoke(this, null);
                }
            }
        }
        public double FollowNum
        {
            get => followNum;
            private set
            {
                if (value != followNum)
                {
                    followNum = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FollowNum)));
                }
            }
        }
        public double MessageNum
        {
            get => messageNum;
            private set
            {
                if (value != messageNum)
                {
                    messageNum = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(messageNum)));
                }
            }
        }
        public double AtMeNum
        {
            get => atMeNum;
            private set
            {
                if (value != atMeNum)
                {
                    atMeNum = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(atMeNum)));
                }
            }
        }
        public double AtCommentMeNum
        {
            get => atCommentMeNum;
            private set
            {
                if (value != atCommentMeNum)
                {
                    atCommentMeNum = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(atCommentMeNum)));
                }
            }
        }
        public double CommentMeNum
        {
            get => commentMeNum;
            private set
            {
                if (value != commentMeNum)
                {
                    commentMeNum = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(commentMeNum)));
                }
            }
        }
        public double FeedLikeNum
        {
            get => feedLikeNum;
            private set
            {
                if (value != feedLikeNum)
                {
                    feedLikeNum = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(feedLikeNum)));
                }
            }
        }

        public void Initial(JObject jo)
        {
            ChangeNumber(jo);
            ThreadPoolTimer PeriodicTimer = ThreadPoolTimer.CreatePeriodicTimer(async (source) => ChangeNumber((JObject)await DataHelper.GetData(DataType.GetNotificationNumbers)), new TimeSpan(0, 0, 30));
        }

        void ChangeNumber(JObject o)
        {
            if (o != null)
            {
                BadgeNum = o.Value<int>("badge");
                FollowNum = o.Value<int>("contacts_follow");
                MessageNum = o.Value<int>("message");
                AtMeNum = o.Value<int>("atme");
                AtCommentMeNum = o.Value<int>("atcommentme");
                CommentMeNum = o.Value<int>("commentme");
                FeedLikeNum = o.Value<int>("feedlike");
            }
        }
    }
}