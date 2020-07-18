using CoolapkUWP.Core.Models;
using CoolapkUWP.Helpers;
using Newtonsoft.Json.Linq;
using System.ComponentModel;

namespace CoolapkUWP.Models.Pages.FeedListPageModels
{
    internal abstract class FeedListDetailBase : Entity, INotifyPropertyChanged
    {
        private bool isCopyEnabled;

        public bool IsCopyEnabled
        {
            get => isCopyEnabled;
            set
            {
                isCopyEnabled = value;
                RaisePropertyChangedEvent();
            }
        }

        protected FeedListDetailBase(JObject o) : base(o) => EntityFixed = true;

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null)
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    internal class UserDetail : FeedListDetailBase
    {
        public ImageModel UserFace { get; private set; }
        public ImageModel Background { get; private set; }
        public double FollowNum { get; private set; }
        public double FansNum { get; private set; }
        public double FeedNum { get; private set; }
        public double Level { get; private set; }
        public string UserName { get; private set; }
        public string Bio { get; private set; }
        public string Verify_title { get; private set; }
        public string Gender { get; private set; }
        public string City { get; private set; }
        public string Astro { get; private set; }
        public string Logintime { get; private set; }
        public string FollowStatus { get; private set; }

        internal UserDetail(JObject o): base(o)
        {
            var loader = Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse("FeedListPage");
            FollowStatus = o.Value<int>("uid").ToString() == SettingsHelper.Get<string>(SettingsHelper.Uid) ? string.Empty : o.Value<int>("isFollow") == 0
                ? loader.GetString("follow")
                : loader.GetString("unFollow");
            FollowNum = o.Value<int>("follow");
            FansNum = o.Value<int>("fans");
            Level = o.Value<int>("level");
            Gender = o.Value<int>("gender") == 1 ? "♂" : o.Value<int>("gender") == 0 ? "♀" : string.Empty;
            Logintime = $"{DataHelper.ConvertUnixTimeStampToReadable(o.Value<int>("logintime"))}活跃";
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

    internal class TopicDetail : FeedListDetailBase
    {
        public ImageModel Logo { get; private set; }
        public string Title { get; private set; }
        public double FollowNum { get; private set; }
        public double CommentNum { get; private set; }
        public string Description { get; private set; }
        public int SelectedIndex { get; private set; }

        internal TopicDetail(JObject o) : base(o)
        {
            Logo = new ImageModel(o.Value<string>("logo"), ImageType.Icon);
            Title = o.Value<string>("title");
            Description = o.Value<string>("description");
            FollowNum = o.TryGetValue("follownum", out JToken t) ? int.Parse(t.ToString()) : o.Value<int>("follow_num");
            CommentNum = o.TryGetValue("commentnum", out JToken tt) ? int.Parse(tt.ToString()) : o.Value<int>("rating_total_num");
        }
    }

    internal class DyhDetail : FeedListDetailBase
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

        internal DyhDetail(JObject o) : base(o)
        {
            bool showUserButton = o.Value<int>("uid") != 0;
            FollowNum = o.Value<int>("follownum");
            ShowComboBox = o.Value<int>("is_open_discuss") == 1;
            Logo = new ImageModel(o.Value<string>("logo"), ImageType.Icon);
            Title = o.Value<string>("title");
            Description = o.Value<string>("description");
            ShowUserButton = showUserButton;
            Url = showUserButton ? o["userInfo"].Value<string>("url") : string.Empty;
            UserName = showUserButton ? o["userInfo"].Value<string>("username") : string.Empty;
            UserAvatar = showUserButton ? new ImageModel(o["userInfo"].Value<string>("userSmallAvatar").Replace("\"", string.Empty), ImageType.BigAvatar) : null;
        }
    }
}
