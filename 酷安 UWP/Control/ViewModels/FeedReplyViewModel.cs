using CoolapkUWP.Data;
using System.Collections.Generic;
using System.ComponentModel;
using Windows.Data.Json;
using Windows.UI.Xaml.Media;

namespace CoolapkUWP.Control.ViewModels
{
    class FeedReplyViewModel : SimpleFeedReplyViewModel, INotifyPropertyChanged, ILike
    {
        public FeedReplyViewModel(IJsonValue t, bool showReplyRow = true) : base(t)
        {
            JsonObject token = t.GetObject();
            dateline = Tools.ConvertTime(double.Parse(token["dateline"].ToString().Replace("\"", string.Empty)));
            message = token["message"].GetString();
            userSmallAvatarUrl = token["userInfo"].GetObject()["userSmallAvatar"].GetString();
            likenum = token["likenum"].ToString().Replace("\"", string.Empty);
            replynum = token["replynum"].ToString().Replace("\"", string.Empty);
            token.TryGetValue("replyRowsCount", out IJsonValue value1);
            replyRowsCount = value1?.GetNumber() ?? 0;
            showreplyRows = showReplyRow && replyRowsCount > 0;
            if (showreplyRows)
            {
                List<SimpleFeedReplyViewModel> models = new List<SimpleFeedReplyViewModel>();
                foreach (var item in token["replyRows"].GetArray())
                    models.Add(new SimpleFeedReplyViewModel(item));
                replyRows = models.ToArray();
                replyRowsMore = token["replyRowsMore"].GetNumber();
            }
            liked = token.TryGetValue("userAction", out IJsonValue v) ? v.GetObject()["like"].GetNumber() == 1 : false;
            GetPic();
        }

        private async void GetPic()
        {
            if (showPic)
                pic = new ImageData
                {
                    Pic = await ImageCache.GetImage(ImageType.SmallImage, picUrl),
                    url = picUrl
                };
            if (!string.IsNullOrEmpty(userSmallAvatarUrl))
                userSmallAvatar = await ImageCache.GetImage(ImageType.SmallAvatar, userSmallAvatarUrl);
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
