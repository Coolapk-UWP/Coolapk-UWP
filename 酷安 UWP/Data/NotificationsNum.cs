using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace CoolapkUWP.Data
{
    class NotificationsNum : INotifyPropertyChanged
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
                    TileManager.SetBadgeNum(value);
                    BadgeNumberChanged?.Invoke(this, null);
                }
            }
        }

        public double followNum, messageNum, atMeNum, atCommentMeNum, commentMeNum, feedLikeNum;

        Timer timer;

        public void Initial(JsonObject jo)
        {
            ChangeNumber(jo);
            timer = new Timer(async (s) => ChangeNumber(Tools.GetJSonObject(await Tools.GetJson("/notification/checkCount"))), string.Empty, 30000, 30000);
        }

        public async Task RefreshNotificationsNum(bool isBackground = false)
        {
            if (!isBackground && timer != null) timer.Dispose();
            ChangeNumber(Tools.GetJSonObject(await Tools.GetJson("/notification/checkCount", isBackground)));
            if (!isBackground) timer = new Timer(async (s) => ChangeNumber(Tools.GetJSonObject(await Tools.GetJson("/notification/checkCount"))), string.Empty, 30000, 30000);
        }

        void ChangeNumber(JsonObject o)
        {
            if (o != null)
            {
                BadgeNum = o["badge"].GetNumber();
                bool numChanged = false;
                if (o["contacts_follow"].GetNumber() != followNum)
                {
                    followNum = o["contacts_follow"].GetNumber();
                    numChanged = true;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(followNum)));
                }
                if (o["message"].GetNumber() != messageNum)
                {
                    messageNum = o["message"].GetNumber();
                    numChanged = true;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(messageNum)));
                }
                if (o["atme"].GetNumber() != atMeNum)
                {
                    atMeNum = o["atme"].GetNumber();
                    numChanged = true;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(atMeNum)));
                }
                if (o["atcommentme"].GetNumber() != atCommentMeNum)
                {
                    atCommentMeNum = o["atcommentme"].GetNumber();
                    numChanged = true;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(atCommentMeNum)));
                }
                if (o["commentme"].GetNumber() != commentMeNum)
                {
                    commentMeNum = o["commentme"].GetNumber();
                    numChanged = true;
                    //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(commentMeNum)));
                }
                if (o["feedlike"].GetNumber() != feedLikeNum)
                {
                    feedLikeNum = o["feedlike"].GetNumber();
                    numChanged = true;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(feedLikeNum)));
                }
                if (numChanged)
                    TileManager.SetTile(followNum, messageNum, atMeNum, atCommentMeNum, commentMeNum, feedLikeNum);
            }
        }
    }
}
