using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Images;
using CoolapkUWP.Models.Users;
using Newtonsoft.Json.Linq;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using Windows.ApplicationModel.Resources;

namespace CoolapkUWP.Models.Pages
{
    public abstract class FeedListDetailBase : Entity, INotifyPropertyChanged
    {
        private bool isCopyEnabled;
        public bool IsCopyEnabled
        {
            get => isCopyEnabled;
            set
            {
                if (isCopyEnabled != value)
                {
                    isCopyEnabled = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        protected FeedListDetailBase(JObject token) : base(token)
        {
            EntityFixed = true;
        }
    }

    internal class UserDetail : FeedListDetailBase, ICanFollow
    {
        private bool followed;
        public bool Followed
        {
            get => followed;
            set
            {
                if (followed != value)
                {
                    followed = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        public int UID { get; private set; }
        public int FeedNum { get; private set; }
        public int LikeNum { get; private set; }
        public int FansNum { get; private set; }
        public int LevelNum { get; private set; }
        public int FollowNum { get; private set; }

        public string Bio { get; private set; }
        public string City { get; private set; }
        public string Astro { get; private set; }
        public string Gender { get; private set; }
        public string UserName { get; private set; }
        public string LoginTime { get; private set; }
        public string BlockStatus { get; private set; }
        public string VerifyTitle { get; private set; }
        public string FollowGlyph { get; private set; }
        public string FollowStatus { get; private set; }

        public double NextLevelExperience { get; private set; }
        public double NextLevelPercentage { get; private set; }
        public string NextLevelNowExperience { get; private set; }

        public ImageModel Cover { get; private set; }
        public ImageModel UserAvatar { get; private set; }

        internal UserDetail(JObject token) : base(token)
        {
            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("FeedListPage");

            if (token.TryGetValue("uid", out JToken uid))
            {
                UID = uid.ToObject<int>();
            }

            if (token.TryGetValue("feed", out JToken feed))
            {
                FansNum = feed.ToObject<int>();
            }

            if (token.TryGetValue("be_like_num", out JToken be_like_num))
            {
                LikeNum = be_like_num.ToObject<int>();
            }

            if (token.TryGetValue("fans", out JToken fans))
            {
                FansNum = fans.ToObject<int>();
            }

            if (token.TryGetValue("level", out JToken level))
            {
                LevelNum = level.ToObject<int>();
            }

            if (token.TryGetValue("follow", out JToken follow))
            {
                FollowNum = follow.ToObject<int>();
            }

            if (token.TryGetValue("bio", out JToken bio))
            {
                Bio = bio.ToString();
            }

            if (token.TryGetValue("province", out JToken province) && token.TryGetValue("city", out JToken city))
            {
                City = province.ToString() == city.ToString() ? city.ToString() : $"{province} {city}";
            }

            if (token.TryGetValue("astro", out JToken astro))
            {
                Astro = astro.ToString();
            }

            if (token.TryGetValue("gender", out JToken gender))
            {
                Gender = gender.ToObject<int>() == 1 ? "♂"
                    : gender.ToObject<int>() == 0 ? "♀"
                    : string.Empty;
            }

            if (token.TryGetValue("isFollow", out JToken isFollow))
            {
                Followed = isFollow.ToObject<int>() != 0;
            }

            if (token.TryGetValue("displayUsername", out JToken displayUsername))
            {
                UserName = displayUsername.ToString();
            }

            if (token.TryGetValue("logintime", out JToken logintime))
            {
                LoginTime = $"{logintime.ToObject<double>().ConvertUnixTimeStampToReadable()}活跃";
            }

            if (token.TryGetValue("block_status", out JToken block_status))
            {
                BlockStatus = block_status.ToObject<int>() == -1 ? loader.GetString("BlockStatus-1")
                    : block_status.ToObject<int>() == 2 ? loader.GetString("BlockStatus2") : null;
            }

            if (token.TryGetValue("verify_title", out JToken verify_title))
            {
                VerifyTitle = verify_title.ToString();
            }

            if (token.TryGetValue("isBlackList", out JToken isBlackList) && token.TryGetValue("isFans", out JToken isFans))
            {
                FollowStatus = uid.ToString() == SettingsHelper.Get<string>(SettingsHelper.Uid) ? string.Empty
                    : isBlackList.ToObject<int>() == 1 ? loader.GetString("InBlackList")
                    : isFollow.ToObject<int>() == 0 ? isFans.ToObject<int>() == 1 ? loader.GetString("FollowFan") : loader.GetString("Follow")
                    : isFans.ToObject<int>() == 1 ? loader.GetString("UnfollowFan") : loader.GetString("UnFollow");
                FollowGlyph = uid.ToString() == SettingsHelper.Get<string>(SettingsHelper.Uid) ? string.Empty
                    : isBlackList.ToObject<int>() == 1 ? "\uE8F8"
                    : isFollow.ToObject<int>() == 0 ? isFans.ToObject<int>() == 1 ? "\uE97A" : "\uE710"
                    : isFans.ToObject<int>() == 1 ? "\uE8EE" : "\uE8FB";
            }

            if (token.TryGetValue("next_level_experience", out JToken next_level_experience))
            {
                NextLevelExperience = next_level_experience.ToObject<double>();
            }

            if (token.TryGetValue("next_level_percentage", out JToken next_level_percentage))
            {
                NextLevelPercentage = next_level_percentage.ToObject<double>();
            }

            NextLevelNowExperience = $"{NextLevelPercentage / 100 * NextLevelExperience:F0}/{NextLevelExperience}";

            if (token.TryGetValue("cover", out JToken cover))
            {
                Cover = new ImageModel(cover.ToString(), ImageType.OriginImage);
            }

            if (token.TryGetValue("userAvatar", out JToken userAvatar))
            {
                UserAvatar = new ImageModel(userAvatar.ToString(), ImageType.BigAvatar);
            }

        }

        public override string ToString() => $"{UserName} - {Bio}";
    }

    internal class TopicDetail : FeedListDetailBase
    {
        public string Title { get; private set; }
        public string HotNum { get; private set; }
        public string FollowNum { get; private set; }
        public string CommentNum { get; private set; }
        public string Description { get; private set; }
        public string FollowGlyph { get; private set; }
        public string FollowStatus { get; private set; }

        public ImageModel Logo { get; private set; }

        public ImmutableArray<UserModel> FollowUsers { get; private set; } = ImmutableArray<UserModel>.Empty;

        internal TopicDetail(JObject token) : base(token)
        {
            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("FeedListPage");

            if (token.TryGetValue("logo", out JToken logo))
            {
                Logo = new ImageModel(logo.ToString(), ImageType.Icon);
            }

            if (token.TryGetValue("title", out JToken title))
            {
                Title = title.ToString();
            }

            if (token.TryGetValue("hot_num_txt", out JToken hot_num_text))
            {
                HotNum = $"{hot_num_text}{loader.GetString("HotNum")}";
            }

            if (token.TryGetValue("follownum_txt", out JToken follownum_text))
            {
                FollowNum = $"{follownum_text}{loader.GetString("Follow")}";
            }

            if (token.TryGetValue("commentnum_txt", out JToken commentnum_text))
            {
                CommentNum = $"{commentnum_text}{loader.GetString("CommentNum")}";
            }

            if (token.TryGetValue("description", out JToken description) && !string.IsNullOrEmpty(description.ToString()))
            {
                Description = description.ToString();
            }
            else if (token.TryGetValue("intro", out JToken intro))
            {
                Description = intro.ToString();
            }

            if (token.TryGetValue("userAction", out JToken userAction) && ((JObject)userAction).TryGetValue("follow", out JToken follow))
            {
                FollowStatus = follow.ToObject<int>() == 0 ? loader.GetString("Follow") : loader.GetString("UnFollow");
                FollowGlyph = follow.ToObject<int>() == 0 ? "\uE710" : "\uE8FB";
            }

            if (token.TryGetValue("recent_follow_list", out JToken recent_follow_list) && (recent_follow_list as JArray).Count > 0)
            {
                FollowUsers = recent_follow_list.Select(
                    x => ((JObject)x).TryGetValue("userInfo", out JToken userInfo)
                        ? new UserModel((JObject)userInfo) : null)
                    .Where(x => x != null)
                    .ToImmutableArray();
            }

        }

        public override string ToString() => $"{Title} - {Description}";
    }
}
