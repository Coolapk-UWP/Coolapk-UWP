using CoolapkUWP.Data;
using System.Collections.Generic;
using Windows.Data.Json;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace CoolapkUWP.Control.ViewModels
{
    class FeedReplyViewModel : SimpleFeedReplyViewModel
    {
        public FeedReplyViewModel(IJsonValue t, bool showReplyRow = true) : base(t)
        {
            JsonObject token = t.GetObject();
            dateline = Tools.ConvertTime(double.Parse(token["dateline"].ToString().Replace("\"", string.Empty)));
            message = Tools.GetMessageText(token["message"].GetString());
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
            GetPic();
        }

        private async void GetPic()
        {
            if (showPic)
                pic = await ImageCache.GetImage(ImageType.SmallImage, picUrl);
            if (!string.IsNullOrEmpty(userSmallAvatarUrl))
                userSmallAvatar = await ImageCache.GetImage(ImageType.SmallAvatar, userSmallAvatarUrl);
        }
        string userSmallAvatarUrl;
        public string likenum { get; private set; }
        public string replynum { get; private set; }
        public ImageSource userSmallAvatar { get; private set; } = new BitmapImage();
        public new string message { get; private set; }
        public ImageSource pic { get; private set; }
        public string dateline { get; private set; }
        public bool showreplyRows { get; set; }
        public SimpleFeedReplyViewModel[] replyRows { get; private set; }
        public bool showreplyRowsMore { get => replyRowsMore > 0; }
        public double replyRowsMore { get; private set; }
        public double replyRowsCount { get; private set; }
    }
}
