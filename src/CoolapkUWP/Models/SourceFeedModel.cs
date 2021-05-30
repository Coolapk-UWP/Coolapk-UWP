using CoolapkUWP.Core.Models;
using CoolapkUWP.Helpers;
using Newtonsoft.Json.Linq;
using System.Collections.Immutable;
using System.Linq;

namespace CoolapkUWP.Models
{
    public class SourceFeedModel : Entity
    {
        public string Url { get; private set; }
        public string QRUrl { get => "https://www.coolapk.com" + Url.Replace("/question/", "/feed/", System.StringComparison.Ordinal); }
        public string Shareurl { get; private set; }
        public string Uurl { get; private set; }
        public string Username { get; private set; }
        public string Dateline { get; private set; }
        public string MessageTitle { get; private set; }
        public string Message { get; private set; }
        public bool ShowMessageTitle { get => !string.IsNullOrEmpty(MessageTitle); }
        public bool ShowPicArr { get; private set; }
        public bool IsCoolPictuers { get; private set; }
        public bool IsMoreThanOnePic { get; private set; }
        public bool HaveUserInfo { get; private set; }
        public BackgroundImageModel Pic { get; private set; }
        public ImmutableArray<ImageModel> PicArr { get; private set; } = ImmutableArray<ImageModel>.Empty;
        public bool IsQuestionFeed { get; private set; }
        public bool IsRatingFeed { get; private set; }
        public bool IsBlock { get; private set; }

        public SourceFeedModel(JObject o) : base(o)
        {
            Url = o.TryGetValue("url", out JToken json) ? json.ToString() : $"/feed/{o["id"].ToString().Replace("\"", string.Empty, System.StringComparison.Ordinal)}";
            if (o.Value<string>("entityType") == "article")
            {
                Dateline = DataHelper.ConvertUnixTimeStampToReadable(o.Value<int>("digest_time"));
                Message = o.Value<string>("message").Substring(0, 120) + "……<a href=\"" + Url + "\">查看更多</a>";
                MessageTitle = o.Value<string>("title");
            }
            else
            {
                IsQuestionFeed = o.Value<string>("feedType") == "question";
                if (IsQuestionFeed)
                {
                    Url = Url.Replace("/feed/", "/question/", System.StringComparison.Ordinal);
                }
                try
                {
                    HaveUserInfo = !string.IsNullOrEmpty((string)o["userInfo"]);
                }
                catch
                {
                    HaveUserInfo = false;
                }
                if (HaveUserInfo)
                {
                    Uurl = o["userInfo"].Value<string>("url");
                    Username = o["userInfo"].Value<string>("username");
                }
                else
                {
                    Uurl = "/u/" + o.Value<string>("uid");
                    Username = o.Value<string>("username");
                }
                Dateline = DataHelper.ConvertUnixTimeStampToReadable(double.Parse(o["dateline"].ToString().Replace("\"", string.Empty, System.StringComparison.Ordinal)));
                IsRatingFeed = o.Value<string>("feedType") == "rating";
                if (IsRatingFeed)
                    Message = "【评分】" + o.Value<string>("rating_score") + "分\n" + o.Value<string>("message");
                else Message = o.Value<string>("message");
                MessageTitle = o.TryGetValue("message_title", out JToken j) ? j.ToString() : string.Empty;
            }
            ShowPicArr = o.TryGetValue("picArr", out JToken picArr) && (picArr as JArray).Count > 0 && !string.IsNullOrEmpty((picArr as JArray)[0].ToString());
            if (o.Value<string>("feedTypeName") == "酷图")
            {
                ShowPicArr = IsCoolPictuers = true;
            }
            if (picArr != null)
            {
                IsMoreThanOnePic = picArr.Count() > 1;
                if (ShowPicArr || IsCoolPictuers)
                {
                    PicArr = (from item in picArr
                              select new ImageModel(item.ToString(), ImageType.SmallImage)).ToImmutableArray();

                    foreach (ImageModel item in PicArr)
                    {
                        item.ContextArray = PicArr;
                    }
                }
            }
            if (o.TryGetValue("pic", out JToken value1) && !string.IsNullOrEmpty(value1.ToString()))
            {
                Pic = new BackgroundImageModel(value1.ToString(), ImageType.SmallImage);
            }
            Shareurl = string.IsNullOrEmpty(o.Value<string>("shareUrl")) ? QRUrl : o.Value<string>("shareUrl");
            IsBlock = o.TryGetValue("block_status", out JToken v) && v.ToString() != "0";
            if (UIHelper.IsSpecialUser && IsBlock)
                Username += " [已折叠]";
            if (o.TryGetValue("status", out JToken s) && s.ToString() == "-1")
                Username += " [仅自己可见]";

        }
    }
}