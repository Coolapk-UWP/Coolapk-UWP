using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace CoolapkUWP.Data
{
    internal class NotificationsNum : INotifyPropertyChanged
    {
        public event EventHandler BadgeNumberChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        private double badgeNum;
        public double BadgeNum
        {
            get => badgeNum;
            private set
            {
                if (value != badgeNum)
                {
                    badgeNum = value;
                    TileManager.SetBadgeNumber(value.ToString());
                    BadgeNumberChanged?.Invoke(this, null);
                }
            }
        }

        public double followNum, messageNum, atMeNum, atCommentMeNum, commentMeNum, feedLikeNum;
        private Timer timer;

        public void Initial(JsonObject jo)
        {
            ChangeNumber(jo);
            timer = new Timer(async (s) => ChangeNumber(UIHelper.GetJSonObject(await UIHelper.GetJson("/notification/checkCount"))), string.Empty, 30000, 30000);
        }

        public async Task RefreshNotificationsNum(bool isBackground = false)
        {
            if (!isBackground && timer != null) { timer.Dispose(); }
            ChangeNumber(UIHelper.GetJSonObject(await UIHelper.GetJson("/notification/checkCount", isBackground)));
            if (!isBackground) { timer = new Timer(async (s) => ChangeNumber(UIHelper.GetJSonObject(await UIHelper.GetJson("/notification/checkCount"))), string.Empty, 30000, 30000); }
        }

        private void ChangeNumber(JsonObject o)
        {
            if (o != null)
            {
                if (o.TryGetValue("badge", out IJsonValue badge) && !string.IsNullOrEmpty(badge.GetNumber().ToString()))
                {
                    BadgeNum = badge.GetNumber();
                }
                bool numChanged = false;
                if (o.TryGetValue("contacts_follow", out IJsonValue contacts_follow) && contacts_follow.GetNumber() != followNum)
                {
                    followNum = contacts_follow.GetNumber();
                    numChanged = true;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(followNum)));
                }
                if (o.TryGetValue("message", out IJsonValue message) && message.GetNumber() != messageNum)
                {
                    messageNum = message.GetNumber();
                    numChanged = true;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(messageNum)));
                }
                if (o.TryGetValue("atme", out IJsonValue atme) && atme.GetNumber() != atMeNum)
                {
                    atMeNum = atme.GetNumber();
                    numChanged = true;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(atMeNum)));
                }
                if (o.TryGetValue("atcommentme", out IJsonValue atcommentme) && atcommentme.GetNumber() != atCommentMeNum)
                {
                    atCommentMeNum = atcommentme.GetNumber();
                    numChanged = true;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(atCommentMeNum)));
                }
                if (o.TryGetValue("commentme", out IJsonValue commentme) && commentme.GetNumber() != commentMeNum)
                {
                    commentMeNum = commentme.GetNumber();
                    numChanged = true;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(commentMeNum)));
                }
                if (o.TryGetValue("feedlike", out IJsonValue feedlike) && feedlike.GetNumber() != feedLikeNum)
                {
                    feedLikeNum = feedlike.GetNumber();
                    numChanged = true;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(feedLikeNum)));
                }
                //if (numChanged)
                //{
                //    TileManager.SetTile(followNum, messageNum, atMeNum, atCommentMeNum, commentMeNum, feedLikeNum);
                //}
            }
        }
    }
}
