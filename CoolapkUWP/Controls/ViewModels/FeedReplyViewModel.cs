using CoolapkUWP.Helpers;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using Windows.UI.Xaml.Media;

namespace CoolapkUWP.Controls.ViewModels
{
    class FeedReplyViewModel : SimpleFeedReplyViewModel, INotifyPropertyChanged, ILike
    {
        public FeedReplyViewModel(JToken t, bool showReplyRow = true) : base(t)
        {
            JObject token = t as JObject;
            Dateline = DataHelper.ConvertTime(double.Parse(token["dateline"].ToString().Replace("\"", string.Empty)));
            Message = token.Value<string>("message");
            userSmallAvatarUrl = token["userInfo"].Value<string>("userSmallAvatar");
            Likenum = token["likenum"].ToString().Replace("\"", string.Empty);
            Replynum = token["replynum"].ToString().Replace("\"", string.Empty);
            token.TryGetValue("replyRowsCount", out JToken value1);
            ReplyRowsCount = int.Parse(value1?.ToString() ?? "0");
            ShowreplyRows = showReplyRow && ReplyRowsCount > 0;
            if (ShowreplyRows)
            {
                List<SimpleFeedReplyViewModel> models = new List<SimpleFeedReplyViewModel>();
                foreach (var item in token["replyRows"] as JArray)
                    models.Add(new SimpleFeedReplyViewModel(item));
                ReplyRows = models.ToArray();
                ReplyRowsMore = token.Value<int>("replyRowsMore");
            }
            Liked = token.TryGetValue("userAction", out JToken v) ? v.Value<int>("like") == 1 : false;
            GetPic();
        }

        private async void GetPic()
        {
            if (showPic)
                Pic = new ImageData
                {
                    Pic = await ImageCacheHelper.GetImage(ImageType.SmallImage, picUrl),
                    url = picUrl
                };
            if (!string.IsNullOrEmpty(userSmallAvatarUrl))
                UserSmallAvatar = await ImageCacheHelper.GetImage(ImageType.SmallAvatar, userSmallAvatarUrl);
        }

        readonly string userSmallAvatarUrl;
        private ImageSource userSmallAvatar1;
        private ImageData pic1;
        private string likenum1;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Likenum
        {
            get => likenum1;
            set
            {
                likenum1 = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Likenum)));
            }
        }
        public string Replynum { get; private set; }
        public ImageSource UserSmallAvatar
        {
            get => userSmallAvatar1;
            private set
            {
                userSmallAvatar1 = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UserSmallAvatar)));
            }
        }
        public string Message { get; private set; }
        public ImageData Pic
        {
            get => pic1;
            private set
            {
                pic1 = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Pic)));
            }
        }
        public string Dateline { get; private set; }
        public bool ShowreplyRows { get; set; }
        public SimpleFeedReplyViewModel[] ReplyRows { get; private set; }
        public bool ShowreplyRowsMore { get => ReplyRowsMore > 0; }
        public double ReplyRowsMore { get; private set; }
        public double ReplyRowsCount { get; private set; }
        public bool Liked { get; set; }
        public bool Liked2 { get => !Liked; }

        string ILike.Id => id.ToString();
    }
}
