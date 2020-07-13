using CoolapkUWP.Helpers;
using Newtonsoft.Json.Linq;
using System.Collections.Immutable;
using System.Linq;

namespace CoolapkUWP.Models
{
    public class SourceFeedModel : Entity
    {
        public SourceFeedModel(JObject o) : base(o)
        {
            Url = o.TryGetValue("url", out JToken json) ? json.ToString() : $"/feed/{o["id"].ToString().Replace("\"", string.Empty)}";
            if (o.Value<string>("entityType") == "article")
            {
                Dateline = DataHelper.ConvertUnixTimeToReadable(o.Value<int>("digest_time"));
                Message = o.Value<string>("message").Substring(0, 120) + "……<a href=\"\">查看更多</a>";
                MessageTitle = o.Value<string>("title");
            }
            else
            {
                if (o.Value<string>("feedType") == "question")
                {
                    IsQuestionFeed = true;
                    Url = Url.Replace("/feed/", "/question/", System.StringComparison.Ordinal);
                }
                Uurl = o["userInfo"].Value<string>("url");
                Username = o["userInfo"].Value<string>("username");
                Dateline = DataHelper.ConvertUnixTimeToReadable(double.Parse(o["dateline"].ToString().Replace("\"", string.Empty)));
                Message = o.Value<string>("message");
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
                    var builder = ImmutableArray.CreateBuilder<ImageModel>();
                    foreach (var item in picArr as JArray)
                    {
                        builder.Add(new ImageModel(item.ToString(), ImageType.SmallImage));
                    }

                    PicArr = builder.ToImmutable();
                    foreach (var item in PicArr)
                    {
                        item.ContextArray = PicArr;
                    }
                }
            }
            if (o.TryGetValue("pic", out JToken value1) && !string.IsNullOrEmpty(value1.ToString()))
            {
                Pic = new ImageModel(value1.ToString(), ImageType.SmallImage);
            }
        }

        public string Url { get; private set; }
        public string Uurl { get; private set; }
        public string Username { get; private set; }
        public string Dateline { get; private set; }
        public string MessageTitle { get; private set; }
        public string Message { get; private set; }
        public bool ShowMessageTitle { get => !string.IsNullOrEmpty(MessageTitle); }
        public bool ShowPicArr { get; private set; }
        public bool IsCoolPictuers { get; private set; }
        public bool IsMoreThanOnePic { get; private set; }
        public ImageModel Pic { get; private set; }
        public ImmutableArray<ImageModel> PicArr { get; private set; } = ImmutableArray<ImageModel>.Empty;
        public bool IsQuestionFeed { get; private set; }
    }
}