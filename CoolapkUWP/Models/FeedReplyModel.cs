using CoolapkUWP.Helpers;
using Newtonsoft.Json.Linq;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;

namespace CoolapkUWP.Models
{
    internal class FeedReplyModel : SimpleFeedReplyModel, INotifyPropertyChanged, ICanChangeLikModel, ICanChangeReplyNum, ICanCopy
    {
        public FeedReplyModel(JObject o, bool showReplyRow = true) : base(o)
        {
            Dateline = DataHelper.ConvertUnixTimeStampToReadable(double.Parse(o["dateline"].ToString().Replace("\"", string.Empty)));
            Message = o.Value<string>("message");
            var userSmallAvatarUrl = o["userInfo"].Value<string>("userSmallAvatar");
            if (!string.IsNullOrEmpty(userSmallAvatarUrl))
            {
                UserSmallAvatar = new ImageModel(userSmallAvatarUrl, ImageType.BigAvatar);
            }
            Likenum = o["likenum"].ToString().Replace("\"", string.Empty);
            Replynum = o["replynum"].ToString().Replace("\"", string.Empty);
            o.TryGetValue("replyRowsCount", out JToken value1);
            ReplyRowsCount = int.Parse(value1?.ToString() ?? "0");
            ShowreplyRows = showReplyRow && ReplyRowsCount > 0;
            if (ShowreplyRows)
            {
                ReplyRows = (from item in o["replyRows"]
                             select new SimpleFeedReplyModel((JObject)item)).ToImmutableArray();

                ReplyRowsMore = o.Value<int>("replyRowsMore");
            }
            Liked = o.TryGetValue("userAction", out JToken v) ? v.Value<int>("like") == 1 : false;
            if (ShowPic)
            {
                Pic = new ImageModel(PicUri, ImageType.SmallImage);
            }
        }

        private string likenum1;
        private string replynum;

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

        public string Replynum
        {
            get => replynum;
            set
            {
                replynum = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Replynum)));
            }
        }
        public new string Message { get; private set; }
        public ImageModel UserSmallAvatar { get; private set; }
        public ImageModel Pic { get; private set; }
        public string Dateline { get; private set; }
        public bool ShowreplyRows { get; set; }
        public ImmutableArray<SimpleFeedReplyModel> ReplyRows { get; private set; } = default;
        public bool ShowreplyRowsMore { get => ReplyRowsMore > 0; }
        public double ReplyRowsMore { get; private set; }
        public double ReplyRowsCount { get; private set; }
        public bool Liked { get; set; }
        public bool Liked2 { get => !Liked; }

        private bool isCopyEnabled;

        public bool IsCopyEnabled
        {
            get => isCopyEnabled;
            set
            {
                isCopyEnabled = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsCopyEnabled)));
            }
        }
        string ICanChangeLikModel.Id => Id.ToString();
    }
}