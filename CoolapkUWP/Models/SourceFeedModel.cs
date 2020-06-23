using CoolapkUWP.Helpers;
using Newtonsoft.Json.Linq;
using System.Collections.Immutable;
using System.Linq;

namespace CoolapkUWP.Models
{
    public class SourceFeedModel : Entity
    {
        public SourceFeedModel(JObject token) : base(token)
        {
            Url = token.TryGetValue("url", out JToken json) ? json.ToString() : $"/feed/{token["id"].ToString().Replace("\"", string.Empty)}";
            if (token.Value<string>("entityType") != "article")
            {
                Uurl = token["userInfo"].Value<string>("url");
                Username = token["userInfo"].Value<string>("username");
                Dateline = DataHelper.ConvertTime(double.Parse(token["dateline"].ToString().Replace("\"", string.Empty)));
                Message = token.Value<string>("message");
                Message_title = token.TryGetValue("message_title", out JToken j) ? j.ToString() : string.Empty;
            }
            else
            {
                Dateline = DataHelper.ConvertTime(token.Value<int>("digest_time"));
                Message = token.Value<string>("message").Substring(0, 120) + "……<a href=\"\">查看更多</a>";
                Message_title = token.Value<string>("title");
            }
            ShowMessage_title = !string.IsNullOrEmpty(Message_title);
            ShowPicArr = token.TryGetValue("picArr", out JToken picArr) && (picArr as JArray).Count > 0 && !string.IsNullOrEmpty((picArr as JArray)[0].ToString());
            if (token.Value<string>("feedTypeName") == "酷图")
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
            if (token.TryGetValue("pic", out JToken value1) && !string.IsNullOrEmpty(value1.ToString()))
            {
                Pic = new ImageModel(value1.ToString(), ImageType.SmallImage);
            }
        }

        public string Url { get; private set; }
        public string Uurl { get; private set; }
        public string Username { get; private set; }
        public string Dateline { get; private set; }
        public bool ShowMessage_title { get; private set; }
        public string Message_title { get; private set; }
        public string Message { get; private set; }
        public bool ShowPicArr { get; private set; }
        public bool IsCoolPictuers { get; private set; }
        public bool IsMoreThanOnePic { get; private set; }
        public ImageModel Pic { get; private set; }
        public ImmutableArray<ImageModel> PicArr { get; private set; } = ImmutableArray<ImageModel>.Empty;
    }
}