using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace CoolapkUWP.Helpers
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
                    BadgeNumberChanged?.Invoke(this, null);
                }
            }
        }

        public double followNum, messageNum, atMeNum, atCommentMeNum, commentMeNum, feedLikeNum;

        Timer timer;

        public void Initial(JsonObject jo)
        {
            ChangeNumber(jo);
            timer = new Timer(async (s) => ChangeNumber((JsonObject)await DataHelper.GetData(false, DataType.GetNotificationNumbers)), string.Empty, 30000, 30000);
        }

        public async Task RefreshNotificationsNum()
        {
            if (timer != null) timer.Dispose();
            ChangeNumber((JsonObject)await DataHelper.GetData(false, DataType.GetNotificationNumbers));
            timer = new Timer(async (s) => ChangeNumber((JsonObject)await DataHelper.GetData(false, DataType.GetNotificationNumbers)), string.Empty, 30000, 30000);
        }

        void ChangeNumber(JsonObject o)
        {
            if (o != null)
            {
                BadgeNum = o["badge"].GetNumber();
                if (o["contacts_follow"].GetNumber() != followNum)
                {
                    followNum = o["contacts_follow"].GetNumber();
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(followNum)));
                }
                if (o["message"].GetNumber() != messageNum)
                {
                    messageNum = o["message"].GetNumber();
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(messageNum)));
                }
                if (o["atme"].GetNumber() != atMeNum)
                {
                    atMeNum = o["atme"].GetNumber();
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(atMeNum)));
                }
                if (o["atcommentme"].GetNumber() != atCommentMeNum)
                {
                    atCommentMeNum = o["atcommentme"].GetNumber();
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(atCommentMeNum)));
                }
                if (o["commentme"].GetNumber() != commentMeNum)
                {
                    commentMeNum = o["commentme"].GetNumber();
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(commentMeNum)));
                }
                if (o["feedlike"].GetNumber() != feedLikeNum)
                {
                    feedLikeNum = o["feedlike"].GetNumber();
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(feedLikeNum)));
                }
            }
        }
    }
}
