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

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null)
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        protected FeedListDetailBase(JObject o) : base(o)
        {
            EntityFixed = true;
        }
    }

    internal class UserDetail : FeedListDetailBase
    {
        public ImageModel UserFace { get; private set; }
        public BackgroundImageModel Background { get; private set; }
        public int FollowNum { get; private set; }
        public int FansNum { get; private set; }
        public int FeedNum { get; private set; }
        public int Level { get; private set; }
        public int BeLikedNum { get; private set; }
        public string UserName { get; private set; }
        public string Bio { get; private set; }
        public string Verify_title { get; private set; }
        public string Gender { get; private set; }
        public string City { get; private set; }
        public string Astro { get; private set; }
        public string Logintime { get; private set; }
        public string FollowStatus { get; private set; }

        internal UserDetail(JObject o) : base(o)
        {
            var loader = Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse("FeedListPage");
            FollowStatus =
                o.Value<int>("uid").ToString() == SettingsHelper.Get<string>(SettingsHelper.Uid) ? string.Empty : o.Value<int>("isFollow") == 0
                ? loader.GetString("follow")
                : loader.GetString("unFollow");
            BeLikedNum = o.Value<int>("be_like_num");
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
            UserFace = new ImageModel(o.Value<string>("userAvatar"), ImageType.BigAvatar);
            Background = new BackgroundImageModel(o.Value<string>("cover"), ImageType.OriginImage);
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
            UserAvatar =
                showUserButton
                ? new ImageModel(o["userInfo"].Value<string>("userAvatar").Replace("\"", string.Empty, System.StringComparison.Ordinal), ImageType.BigAvatar)
                : null;
        }
    }

    internal class CollectionDetail : FeedListDetailBase
    {
        public bool ShowCoverPic { get; private set; }
        public ImageModel CoverPic { get; private set; }
        public string Title { get; private set; }
        public string Description { get; private set; }
        public double FollowNum { get; private set; }
        public double LikeNum { get; private set; }
        public int ItemsNum { get; private set; }
        public string UUrl { get; private set; }
        public string UserName { get; private set; }
        public ImageModel UserAvatar { get; private set; }
        public string LastUpdate { get; private set; }

        internal CollectionDetail(JObject o) : base(o)
        {
            ShowCoverPic = !string.IsNullOrEmpty(o.Value<string>("cover_pic"));
            if (ShowCoverPic)
            {
                CoverPic = new ImageModel(o.Value<string>("cover_pic"), ImageType.OriginImage);
            }
            FollowNum = o.Value<int>("follow_num");
            LikeNum = o.Value<int>("like_num");
            ItemsNum = o.Value<int>("item_num");
            Title = o.Value<string>("title");
            Description = o.Value<string>("description");
            UUrl = o["userInfo"].Value<string>("url");
            UserName = o["userInfo"].Value<string>("username");
            UserAvatar = new ImageModel(o["userInfo"].Value<string>("userAvatar").Replace("\"", string.Empty, System.StringComparison.Ordinal), ImageType.BigAvatar);
            LastUpdate = DataHelper.ConvertUnixTimeStampToReadable(o.Value<int>("lastupdate")) + "更新";
        }
    }

    internal class ProductDetail : FeedListDetailBase
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
        public double MinPrice { get; private set; }
        public double MaxPrice { get; private set; }
        public string PriceCurrency { get; private set; }
        public string ReleaseTime { get; private set; }
        public string HotNumTXT { get; private set; }

        internal ProductDetail(JObject o) : base(o)
        {
            //bool showUserButton = o.Value<int>("uid") != 0;
            FollowNum = o.Value<int>("follow_num");
            //ShowComboBox = o.Value<int>("is_open_discuss") == 1;
            Logo = new ImageModel(o.Value<string>("logo"), ImageType.Icon);
            Title = o.Value<string>("title");
            UIHelper.ShowMessage(Title);
            Description = o.Value<string>("description");
            MinPrice = o.Value<int>("price_min");
            MaxPrice = o.Value<int>("price_max");
            PriceCurrency = o.Value<string>("price_currency");
            ReleaseTime = o.Value<string>("release_time");
            HotNumTXT = o.Value<string>("hot_num_txt");
            //ShowUserButton = showUserButton;
            //Url = showUserButton ? o["userInfo"].Value<string>("url") : string.Empty;
            //UserName = showUserButton ? o["userInfo"].Value<string>("username") : string.Empty;
            //UserAvatar =
            //    showUserButton
            //    ? new ImageModel(o["userInfo"].Value<string>("userAvatar").Replace("\"", string.Empty, System.StringComparison.Ordinal), ImageType.BigAvatar)
            //    : null;
        }
    }
}