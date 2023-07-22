using CoolapkUWP.Helpers;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using mtuc = Microsoft.Toolkit.Uwp.Connectivity;

namespace CoolapkUWP.Models
{
    public class NotificationsModel : INotifyPropertyChanged
    {
        public static NotificationsModel Instance = new NotificationsModel();

        private readonly DispatcherTimer timer;
        private int badgeNum, followNum, messageNum, atMeNum, atCommentMeNum, commentMeNum, feedLikeNum, cloudInstall, notification;

        /// <summary>
        /// 新的消息总数。
        /// </summary>
        public int BadgeNum
        {
            get => badgeNum;
            private set
            {
                if (value != badgeNum)
                {
                    badgeNum = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        /// <summary>
        /// 新增粉丝数。
        /// </summary>
        public int FollowNum
        {
            get => followNum;
            private set
            {
                if (value != followNum)
                {
                    followNum = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        /// <summary>
        /// 新私信数。
        /// </summary>
        public int MessageNum
        {
            get => messageNum;
            private set
            {
                if (value != messageNum)
                {
                    messageNum = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        /// <summary>
        /// 新“@我的动态”数。
        /// </summary>
        public int AtMeNum
        {
            get => atMeNum;
            private set
            {
                if (value != atMeNum)
                {
                    atMeNum = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        /// <summary>
        /// 新“@我的回复”数。
        /// </summary>
        public int AtCommentMeNum
        {
            get => atCommentMeNum;
            private set
            {
                if (value != atCommentMeNum)
                {
                    atCommentMeNum = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        /// <summary>
        /// 新回复数。
        /// </summary>
        public int CommentMeNum
        {
            get => commentMeNum;
            private set
            {
                if (value != commentMeNum)
                {
                    commentMeNum = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        /// <summary>
        /// 新“收到的赞”数。
        /// </summary>
        public int FeedLikeNum
        {
            get => feedLikeNum;
            private set
            {
                if (value != feedLikeNum)
                {
                    feedLikeNum = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        /// <summary>
        /// 新“云安装”数。
        /// </summary>
        public int CloudInstall
        {
            get => cloudInstall;
            private set
            {
                if (value != cloudInstall)
                {
                    cloudInstall = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        /// <summary>
        /// 新“通知”数。
        /// </summary>
        public int Notification
        {
            get => notification;
            private set
            {
                if (value != notification)
                {
                    notification = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        public NotificationsModel()
        {
            Instance = Instance ?? this;
            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMinutes(1)
            };
            timer.Tick += async (o, a) =>
            {
                if (mtuc.NetworkHelper.Instance.ConnectionInformation.IsInternetAvailable)
                {
                    await Update();
                }
            };
            timer.Start();
        }

        ~NotificationsModel()
        {
            Clear();
            timer.Stop();
        }

        /// <summary>
        /// 将数字归零。
        /// </summary>
        public void Clear() => BadgeNum = FollowNum = MessageNum = AtMeNum = AtCommentMeNum = CommentMeNum = FeedLikeNum = CloudInstall = Notification = 0;

        public async Task Update()
        {
            try
            {
                (bool isSucceed, JToken result) = await RequestHelper.GetDataAsync(UriHelper.GetUri(UriType.GetNotificationNumbers), true);
                if (!isSucceed) { return; }
                ChangeNumber((JObject)result);
            }
            catch { Clear(); }
        }

        private void ChangeNumber(JObject token)
        {
            if (token != null)
            {
                if (token.TryGetValue("cloudInstall", out JToken cloudInstall) && cloudInstall != null)
                {
                    CloudInstall = token.Value<int>("cloudInstall");
                }
                if (token.TryGetValue("notification", out JToken notification) && notification != null)
                {
                    Notification = token.Value<int>("notification");
                }
                if (token.TryGetValue("badge", out JToken badge) && badge != null)
                {
                    BadgeNum = token.Value<int>("badge");
                    UIHelper.SetBadgeNumber(BadgeNum.ToString());
                }
                if (token.TryGetValue("contacts_follow", out JToken contacts_follow) && contacts_follow != null)
                {
                    FollowNum = token.Value<int>("contacts_follow");
                }
                if (token.TryGetValue("message", out JToken message) && message != null)
                {
                    MessageNum = token.Value<int>("message");
                }
                if (token.TryGetValue("atme", out JToken atme) && atme != null)
                {
                    AtMeNum = token.Value<int>("atme");
                }
                if (token.TryGetValue("atcommentme", out JToken atcommentme) && atcommentme != null)
                {
                    AtCommentMeNum = token.Value<int>("atcommentme");
                }
                if (token.TryGetValue("commentme", out JToken commentme) && commentme != null)
                {
                    CommentMeNum = token.Value<int>("commentme");
                }
                if (token.TryGetValue("feedlike", out JToken feedlike) && feedlike != null)
                {
                    FeedLikeNum = token.Value<int>("feedlike");
                }
            }
        }
    }
}
