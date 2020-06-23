using CoolapkUWP.Helpers;
using Newtonsoft.Json.Linq;

namespace CoolapkUWP.Models.Pages.FeedListPageModels
{
    internal interface IDetail
    {
        void Initialize(JObject o);
    }

    internal class UserDetail : IDetail
    {
        public ImageModel UserFace { get; private set; }
        public string UserName { get; private set; }
        public double FollowNum { get; private set; }
        public double FansNum { get; private set; }
        public double FeedNum { get; private set; }
        public double Level { get; private set; }
        public string Bio { get; private set; }
        public ImageModel Background { get; private set; }
        public string Verify_title { get; private set; }
        public string Gender { get; private set; }
        public string City { get; private set; }
        public string Astro { get; private set; }
        public string Logintime { get; private set; }
        public string FollowStatus { get; private set; }

        public void Initialize(JObject o)
        {
            FollowStatus = o.Value<int>("uid").ToString() == SettingsHelper.Get<string>(SettingsHelper.Uid) ? string.Empty : o.Value<int>("isFollow") == 0 ? "关注" : "取消关注";
            FollowNum = o.Value<int>("follow");
            FansNum = o.Value<int>("fans");
            Level = o.Value<int>("level");
            Gender = o.Value<int>("gender") == 1 ? "♂" : o.Value<int>("gender") == 0 ? "♀" : string.Empty;
            Logintime = $"{DataHelper.ConvertTime(o.Value<int>("logintime"))}活跃";
            FeedNum = o.Value<int>("feed");
            UserName = o.Value<string>("username");
            Bio = o.Value<string>("bio");
            Verify_title = o.Value<string>("verify_title");
            City = $"{o.Value<string>("province")} {o.Value<string>("city")}";
            Astro = o.Value<string>("astro");
            UserFace = new ImageModel(o.Value<string>("userSmallAvatar"), ImageType.BigAvatar);
            Background = new ImageModel(o.Value<string>("cover"), ImageType.OriginImage);
        }
    }

    internal class TopicDetail : IDetail
    {
        public ImageModel Logo { get; private set; }
        public string Title { get; private set; }
        public double FollowNum { get; private set; }
        public double CommentNum { get; private set; }
        public string Description { get; private set; }
        public int SelectedIndex { get; private set; }

        public void Initialize(JObject o)
        {
            Logo = new ImageModel(o.Value<string>("logo"), ImageType.Icon);
            Title = o.Value<string>("title");
            Description = o.Value<string>("description");
            FollowNum = o.TryGetValue("follownum", out JToken t) ? int.Parse(t.ToString()) : o.Value<int>("follow_num");
            CommentNum = o.TryGetValue("commentnum", out JToken tt) ? int.Parse(tt.ToString()) : o.Value<int>("rating_total_num");
            SelectedIndex = SelectedIndex;
        }
    }

    internal class DyhDetail : IDetail
    {
        public ImageModel Logo { get; private set; }
        public string Title { get; private set; }
        public string Description { get; private set; }
        public double FollowNum { get; private set; }
        public bool ShowUserButton { get; private set; }
        public string Url { get; private set; }
        public string UserName { get; private set; }
        public ImageModel UserAvatar { get; private set; }
        public int SelectedIndex { get; private set; }
        public bool ShowComboBox { get; private set; }

        public void Initialize(JObject detail)
        {
            bool showUserButton = detail.Value<int>("uid") != 0;
            FollowNum = detail.Value<int>("follownum");
            ShowComboBox = detail.Value<int>("is_open_discuss") == 1;
            Logo = new ImageModel(detail.Value<string>("logo"), ImageType.Icon);
            Title = detail.Value<string>("title");
            Description = detail.Value<string>("description");
            ShowUserButton = showUserButton;
            Url = showUserButton ? detail["userInfo"].Value<string>("url") : string.Empty;
            UserName = showUserButton ? detail["userInfo"].Value<string>("username") : string.Empty;
            UserAvatar = showUserButton ? new ImageModel(detail["userInfo"].Value<string>("userSmallAvatar").Replace("\"", string.Empty), ImageType.BigAvatar) : null;
            SelectedIndex = SelectedIndex;

        }
    }
}
