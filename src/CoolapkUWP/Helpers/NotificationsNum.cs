using CoolapkUWP.Core.Helpers;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel;
using Windows.System.Threading;

namespace CoolapkUWP.Helpers
{
    /// <summary>
    ///     用于记录各种通知的数量，并可定时（每1min）向服务器获取新的数据。
    /// </summary>
    internal class NotificationNums : INotifyPropertyChanged
    {
        /// <summary>
        ///     BadgeNum更改时触发的事件。
        /// </summary>
        public event EventHandler BadgeNumberChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        private double badgeNum, followNum, messageNum, atMeNum, atCommentMeNum, commentMeNum, feedLikeNum, cloudInstall, notification;

        /// <summary>
        ///     新的消息总数。
        /// </summary>
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

        /// <summary>
        ///     新增粉丝数。
        /// </summary>
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

        /// <summary>
        ///     新私信数。
        /// </summary>
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

        /// <summary>
        ///     新“@我的动态”数。
        /// </summary>
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

        /// <summary>
        ///     新“@我的回复”数。
        /// </summary>
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

        /// <summary>
        ///     新回复数。
        /// </summary>
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

        /// <summary>
        ///     新“收到的赞”数。
        /// </summary>
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

        /// <summary>
        ///     新“云安装”数。
        /// </summary>
        public double CloudInstall
        {
            get => cloudInstall;
            private set
            {
                if (value != cloudInstall)
                {
                    cloudInstall = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(cloudInstall)));
                }
            }
        }

        /// <summary>
        ///     新“通知”数。
        /// </summary>
        public double Notification
        {
            get => notification;
            private set
            {
                if (value != notification)
                {
                    notification = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(notification)));
                }
            }
        }

        private ThreadPoolTimer timer;

        /// <summary>
        ///     初始化数值并初始化计时器。
        /// </summary>
        /// <param name="jo">
        ///     用于初始化数值的、包含“NotifyCount”的 <c> JObject </c>。
        /// </param>
        public void Initial(JObject jo)
        {
            ChangeNumber(jo);
            if (timer == null)
            {
                timer = ThreadPoolTimer.CreatePeriodicTimer(async (source) =>
                {
                    //LiveTileTask.UpdateTile();
                    (bool isSucceed, JToken result) = await DataHelper.GetDataAsync(UriHelper.GetUri(UriType.GetNotificationNumbers), true);
                    if (!isSucceed) { return; }

                    ChangeNumber((JObject)result);
                }, new TimeSpan(0, 1, 0));
            }
        }

        /// <summary>
        ///     将数字归零并取消计时器。
        /// </summary>
        public void ClearNums()
        {
            BadgeNum = FollowNum = MessageNum = AtMeNum = AtCommentMeNum = CommentMeNum = FeedLikeNum = CloudInstall = Notification = 0;
            timer?.Cancel();
            timer = null;
        }

        private void ChangeNumber(JObject o)
        {
            if (o != null)
            {
                CloudInstall = o.Value<int>("cloudInstall");
                Notification = o.Value<int>("notification");
                BadgeNum = o.Value<int>("badge");
                FollowNum = o.Value<int>("contacts_follow");
                MessageNum = o.Value<int>("message");
                AtMeNum = o.Value<int>("atme");
                AtCommentMeNum = o.Value<int>("atcommentme");
                CommentMeNum = o.Value<int>("commentme");
                FeedLikeNum = o.Value<int>("feedlike");
            }
            UIHelper.SetBadgeNumber(BadgeNum.ToString());
            //UIHelper.ShowMessage(BadgeNum.ToString());
        }
    }
}