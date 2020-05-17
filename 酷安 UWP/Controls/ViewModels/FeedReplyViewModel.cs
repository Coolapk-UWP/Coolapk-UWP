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
            dateline = DataHelper.ConvertTime(double.Parse(token["dateline"].ToString().Replace("\"", string.Empty)));
            message = token.Value<string>("message");
            userSmallAvatarUrl = token["userInfo"].Value<string>("userSmallAvatar");
            likenum = token["likenum"].ToString().Replace("\"", string.Empty);
            replynum = token["replynum"].ToString().Replace("\"", string.Empty);
            token.TryGetValue("replyRowsCount", out JToken value1);
            replyRowsCount = int.Parse(value1?.ToString() ?? "0");
            showreplyRows = showReplyRow && replyRowsCount > 0;
            if (showreplyRows)
            {
                List<SimpleFeedReplyViewModel> models = new List<SimpleFeedReplyViewModel>();
                foreach (var item in token["replyRows"] as JArray)
                    models.Add(new SimpleFeedReplyViewModel(item));
                replyRows = models.ToArray();
                replyRowsMore = token.Value<int>("replyRowsMore");
            }
            liked = token.TryGetValue("userAction", out JToken v) ? v.Value<int>("like") == 1 : false;
            GetPic();
        }

        private async void GetPic()
        {
            if (showPic)
                pic = new ImageData
                {
                    Pic = await ImageCacheHelper.GetImage(ImageType.SmallImage, picUrl),
                    url = picUrl
                };
            if (!string.IsNullOrEmpty(userSmallAvatarUrl))
                userSmallAvatar = await ImageCacheHelper.GetImage(ImageType.SmallAvatar, userSmallAvatarUrl);
        }
        string userSmallAvatarUrl;
        private ImageSource userSmallAvatar1;
        private ImageData pic1;
        private string likenum1;

        public event PropertyChangedEventHandler PropertyChanged;

        public string likenum
        {
            get => likenum1;
            set
            {
                likenum1 = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(likenum)));
            }
        }
        public string replynum { get; private set; }
        public ImageSource userSmallAvatar
        {
            get => userSmallAvatar1;
            private set
            {
                userSmallAvatar1 = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(userSmallAvatar)));
            }
        }
        public new string message { get; private set; }
        public ImageData pic
        {
            get => pic1;
            private set
            {
                pic1 = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(pic)));
            }
        }
        public string dateline { get; private set; }
        public bool showreplyRows { get; set; }
        public SimpleFeedReplyViewModel[] replyRows { get; private set; }
        public bool showreplyRowsMore { get => replyRowsMore > 0; }
        public double replyRowsMore { get; private set; }
        public double replyRowsCount { get; private set; }
        public bool liked { get; set; }
        public bool liked2 { get => !liked; }

        string ILike.id => id.ToString();
    }
}
