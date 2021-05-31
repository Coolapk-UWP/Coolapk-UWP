using CoolapkUWP.Helpers;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace CoolapkUWP.Models
{
    internal class FeedReplyModel : SimpleFeedReplyModel, INotifyPropertyChanged, ICanChangeLikModel, ICanChangeReplyNum, ICanCopy
    {
        private string likenum1;
        private string replynum;
        private bool isCopyEnabled;

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

        public bool IsCopyEnabled
        {
            get => isCopyEnabled;
            set
            {
                isCopyEnabled = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsCopyEnabled)));
            }
        }

        public new string Message { get; private set; }
        public ImageModel UserSmallAvatar { get; private set; }
        public ImageModel Pic { get; private set; }
        public string Dateline { get; private set; }
        public bool ShowreplyRows { get; set; }
        public List<SimpleFeedReplyModel> ReplyRows { get; private set; }
        public bool ShowreplyRowsMore { get => ReplyRowsMore > 0; }
        public double ReplyRowsMore { get; private set; }
        public double ReplyRowsCount { get; private set; }
        public bool Liked { get; set; }
        public bool Liked2 { get => !Liked; }

        string ICanChangeLikModel.Id => $"{Id}";

        public event PropertyChangedEventHandler PropertyChanged;

        public FeedReplyModel(JObject o, bool showReplyRow = true) : base(o)
        {
            Dateline = DataHelper.ConvertUnixTimeStampToReadable(double.Parse(o["dateline"].ToString().Replace("\"", string.Empty, System.StringComparison.Ordinal)));
            Message = o.Value<string>("message");
            string userSmallAvatarUrl = o["userInfo"].Value<string>("userSmallAvatar");
            if (!string.IsNullOrEmpty(userSmallAvatarUrl))
            {
                UserSmallAvatar = new ImageModel(userSmallAvatarUrl, ImageType.BigAvatar);
            }
            Likenum = o["likenum"].ToString().Replace("\"", string.Empty, System.StringComparison.Ordinal);
            Replynum = o["replynum"].ToString().Replace("\"", string.Empty, System.StringComparison.Ordinal);
            o.TryGetValue("replyRowsCount", out JToken value1);
            ReplyRowsCount = int.Parse(value1?.ToString() ?? "0");
            ShowreplyRows = showReplyRow && ReplyRowsCount > 0;
            if (ShowreplyRows)
            {
                ReplyRows = o["replyRows"].Select(item => new SimpleFeedReplyModel((JObject)item)).ToList();
                ReplyRowsMore = o.Value<int>("replyRowsMore");
            }
            Liked = o.TryGetValue("userAction", out JToken v) ? v.Value<int>("like") == 1 : false;
            if (ShowPic)
            {
                Pic = new ImageModel(PicUri, ImageType.SmallImage);
            }
        }
    }
}