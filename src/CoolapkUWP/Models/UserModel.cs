using CoolapkUWP.Core.Models;
using CoolapkUWP.Helpers;
using Newtonsoft.Json.Linq;

namespace CoolapkUWP.Models
{
    internal class UserModel : Entity
    {
        public UserModel(JObject o) : base(o)
        {
            var loader = Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse("FeedListPage");
            Url = o.Value<string>("url");
            BlockStatus = o.Value<int>("status") == -1 ? loader.GetString("status-1")
                : UIHelper.IsSpecialUser && o.Value<int>("block_status") == -1 ? loader.GetString("block_status-1")
                : UIHelper.IsSpecialUser && o.Value<int>("block_status") == 2 ? loader.GetString("block_status2") : null;
            UserName = o.Value<string>("username") + " " + BlockStatus;
            if (o.TryGetValue("fans", out JToken a))
            {
                FansNum = a.ToString().Replace("\"", string.Empty, System.StringComparison.Ordinal);
                FollowNum = o["follow"].ToString().Replace("\"", string.Empty, System.StringComparison.Ordinal);
                if (o.TryGetValue("bio", out JToken b))
                {
                    Bio = b.ToString();
                }

                LoginTime = DataHelper.ConvertUnixTimeStampToReadable(double.Parse(o["logintime"].ToString().Replace("\"", string.Empty, System.StringComparison.Ordinal))) + "活跃";
            }
            UserAvatar = new ImageModel(o.Value<string>("userSmallAvatar"), ImageType.BigAvatar);
        }

        public string Url { get; private set; }
        public string UserName { get; private set; }
        public string FollowNum { get; private set; }
        public string FansNum { get; private set; }
        public string LoginTime { get; private set; }
        public string Bio { get; private set; }
        public string BlockStatus { get; private set; }
        public ImageModel UserAvatar { get; private set; }
    }
}