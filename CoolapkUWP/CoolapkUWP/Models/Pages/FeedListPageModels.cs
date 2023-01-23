using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Images;
using CoolapkUWP.Models.Users;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
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
                FeedNum = feed.ToObject<int>();
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
                LoginTime = $"{logintime.ToObject<long>().ConvertUnixTimeStampToReadable()}活跃";
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
        public ImageModel Logo { get; private set; }

        public string Title { get; private set; }
        public string HotNum { get; private set; }
        public string FollowNum { get; private set; }
        public string CommentNum { get; private set; }
        public string Description { get; private set; }
        public string FollowGlyph { get; private set; }
        public string FollowStatus { get; private set; }

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

    internal class DyhDetail : FeedListDetailBase
    {
        public string Uurl { get; private set; }
        public string Title { get; private set; }
        public string UserName { get; private set; }
        public string FollowNum { get; private set; }
        public string Description { get; private set; }
        public string FollowGlyph { get; private set; }
        public string FollowStatus { get; private set; }

        public ImageModel Logo { get; private set; }
        public ImageModel UserAvatar { get; private set; }

        internal DyhDetail(JObject token) : base(token)
        {
            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("FeedListPage");

            if (token.TryGetValue("uid", out JToken uid))
            {
                Uurl = $"/u/{uid}";
            }

            if (token.TryGetValue("title", out JToken title))
            {
                Title = title.ToString();
            }

            if (token.TryGetValue("author", out JToken author))
            {
                UserName = author.ToString();
            }

            if (token.TryGetValue("follownum", out JToken follownum))
            {
                FollowNum = $"{follownum}{loader.GetString("SubscribeNum")}";
            }

            if (token.TryGetValue("description", out JToken description))
            {
                Description = description.ToString();
            }

            if (token.TryGetValue("userAction", out JToken userAction) && ((JObject)userAction).TryGetValue("follow", out JToken follow))
            {
                FollowStatus = follow.ToObject<int>() == 0 ? loader.GetString("Subscribe") : loader.GetString("Unsubscribe");
                FollowGlyph = follow.ToObject<int>() == 0 ? "\uE710" : "\uE8FB";
            }

            if (token.TryGetValue("logo", out JToken logo))
            {
                Logo = new ImageModel(logo.ToString(), ImageType.Icon);
            }

            if (token.TryGetValue("userAvatar", out JToken userAvatar))
            {
                UserAvatar = new ImageModel(userAvatar.ToString(), ImageType.BigAvatar);
            }
        }

        internal class ProductDetail : FeedListDetailBase
        {
            public int Star1Count { get; private set; }
            public int Star2Count { get; private set; }
            public int Star3Count { get; private set; }
            public int Star4Count { get; private set; }
            public int Star5Count { get; private set; }
            public int OwnerStar1Count { get; private set; }
            public int OwnerStar2Count { get; private set; }
            public int OwnerStar3Count { get; private set; }
            public int OwnerStar4Count { get; private set; }
            public int OwnerStar5Count { get; private set; }

            public string Title { get; private set; }
            public string HotNum { get; private set; }
            public string StarCount { get; private set; }
            public string FollowNum { get; private set; }
            public string CommentNum { get; private set; }
            public string RatingCount { get; private set; }
            public string Description { get; private set; }
            public string FollowGlyph { get; private set; }
            public string FollowStatus { get; private set; }

            public double OwnerScore { get; private set; }
            public double RatingScore { get; private set; }
            public double Star1Percent { get; private set; }
            public double Star2Percent { get; private set; }
            public double Star3Percent { get; private set; }
            public double Star4Percent { get; private set; }
            public double Star5Percent { get; private set; }
            public double OwnerStar1Percent { get; private set; }
            public double OwnerStar2Percent { get; private set; }
            public double OwnerStar3Percent { get; private set; }
            public double OwnerStar4Percent { get; private set; }
            public double OwnerStar5Percent { get; private set; }

            public ImageModel Logo { get; private set; }

            public ImmutableArray<string> TagArr { get; private set; } = ImmutableArray<string>.Empty;

            public ImmutableArray<UserModel> FollowUsers { get; private set; } = ImmutableArray<UserModel>.Empty;

            public ImmutableArray<ImageModel> CoverArr { get; private set; } = ImmutableArray<ImageModel>.Empty;

            internal ProductDetail(JObject token) : base(token)
            {
                ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("FeedListPage");

                int MaxStarCount = 0, MaxOwnerStarCount = 0;

                if (token.TryGetValue("star_1_count", out JToken star_1_count))
                {
                    Star1Count = star_1_count.ToObject<int>();
                    MaxStarCount = Math.Max(Star1Count, MaxStarCount);
                }

                if (token.TryGetValue("star_2_count", out JToken star_2_count))
                {
                    Star2Count = star_2_count.ToObject<int>();
                    MaxStarCount = Math.Max(Star2Count, MaxStarCount);
                }

                if (token.TryGetValue("star_3_count", out JToken star_3_count))
                {
                    Star3Count = star_3_count.ToObject<int>();
                    MaxStarCount = Math.Max(Star3Count, MaxStarCount);
                }

                if (token.TryGetValue("star_4_count", out JToken star_4_count))
                {
                    Star4Count = star_4_count.ToObject<int>();
                    MaxStarCount = Math.Max(Star4Count, MaxStarCount);
                }

                if (token.TryGetValue("star_5_count", out JToken star_5_count))
                {
                    Star5Count = star_5_count.ToObject<int>();
                    MaxStarCount = Math.Max(Star5Count, MaxStarCount);
                }

                if (token.TryGetValue("owner_star_1_count", out JToken owner_star_1_count))
                {
                    OwnerStar1Count = owner_star_1_count.ToObject<int>();
                    MaxOwnerStarCount = Math.Max(OwnerStar1Count, MaxOwnerStarCount);
                }

                if (token.TryGetValue("owner_star_2_count", out JToken owner_star_2_count))
                {
                    OwnerStar2Count = owner_star_2_count.ToObject<int>();
                    MaxOwnerStarCount = Math.Max(OwnerStar2Count, MaxOwnerStarCount);
                }

                if (token.TryGetValue("owner_star_3_count", out JToken owner_star_3_count))
                {
                    OwnerStar3Count = owner_star_3_count.ToObject<int>();
                    MaxOwnerStarCount = Math.Max(OwnerStar3Count, MaxOwnerStarCount);
                }

                if (token.TryGetValue("owner_star_4_count", out JToken owner_star_4_count))
                {
                    OwnerStar4Count = owner_star_4_count.ToObject<int>();
                    MaxOwnerStarCount = Math.Max(OwnerStar4Count, MaxOwnerStarCount);
                }

                if (token.TryGetValue("owner_star_5_count", out JToken owner_star_5_count))
                {
                    OwnerStar5Count = owner_star_5_count.ToObject<int>();
                    MaxOwnerStarCount = Math.Max(OwnerStar5Count, MaxOwnerStarCount);
                }

                Star1Percent = Star1Count * 100 / MaxStarCount;
                Star2Percent = Star2Count * 100 / MaxStarCount;
                Star3Percent = Star3Count * 100 / MaxStarCount;
                Star4Percent = Star4Count * 100 / MaxStarCount;
                Star5Percent = Star5Count * 100 / MaxStarCount;

                OwnerStar1Percent = OwnerStar1Count * 100 / MaxOwnerStarCount;
                OwnerStar2Percent = OwnerStar2Count * 100 / MaxOwnerStarCount;
                OwnerStar3Percent = OwnerStar3Count * 100 / MaxOwnerStarCount;
                OwnerStar4Percent = OwnerStar4Count * 100 / MaxOwnerStarCount;
                OwnerStar5Percent = OwnerStar5Count * 100 / MaxOwnerStarCount;

                if (token.TryGetValue("title", out JToken title))
                {
                    Title = title.ToString();
                }

                if (token.TryGetValue("hot_num_txt", out JToken hot_num_text))
                {
                    HotNum = $"{hot_num_text}{loader.GetString("HotNum")}";
                }

                if (token.TryGetValue("star_total_count", out JToken star_total_count))
                {
                    StarCount = $"{star_total_count}位酷友打分";
                }

                if (token.TryGetValue("follow_num_txt", out JToken follownum_text))
                {
                    FollowNum = $"{follownum_text}{loader.GetString("Follow")}";
                }

                if (token.TryGetValue("feed_comment_num_txt", out JToken commentnum_text))
                {
                    CommentNum = $"{commentnum_text}{loader.GetString("CommentNum")}";
                }

                if (token.TryGetValue("rating_total_num", out JToken rating_total_num))
                {
                    RatingCount = $"{rating_total_num}位机主打分";
                }

                if (token.TryGetValue("description", out JToken description))
                {
                    Description = description.ToString();
                }

                if (token.TryGetValue("userAction", out JToken userAction) && ((JObject)userAction).TryGetValue("follow", out JToken follow))
                {
                    FollowStatus = follow.ToObject<int>() == 0 ? loader.GetString("Follow") : loader.GetString("UnFollow");
                    FollowGlyph = follow.ToObject<int>() == 0 ? "\uE710" : "\uE8FB";
                }

                if (token.TryGetValue("owner_star_average_score", out JToken owner_star_average_score))
                {
                    OwnerScore = owner_star_average_score.ToObject<double>();
                }

                if (token.TryGetValue("rating_average_score", out JToken rating_average_score))
                {
                    RatingScore = rating_average_score.ToObject<double>();
                }

                if (token.TryGetValue("logo", out JToken logo))
                {
                    Logo = new ImageModel(logo.ToString(), ImageType.Icon);
                }

                if (token.TryGetValue("tagArr", out JToken tagArr) && (tagArr as JArray).Count > 0)
                {
                    TagArr = tagArr.Select(x => x.ToString()).ToImmutableArray();
                }

                if (token.TryGetValue("recent_follow_list", out JToken recent_follow_list) && (recent_follow_list as JArray).Count > 0)
                {
                    FollowUsers = recent_follow_list.Select(
                        x => ((JObject)x).TryGetValue("userInfo", out JToken userInfo)
                            ? new UserModel((JObject)userInfo) : null)
                        .Where(x => x != null)
                        .ToImmutableArray();
                }

                if (token.TryGetValue("coverArr", out JToken coverArr) && (coverArr as JArray).Count > 0)
                {
                    CoverArr = coverArr.Select(
                        x => !string.IsNullOrEmpty(x.ToString())
                            ? new ImageModel(x.ToString(), ImageType.SmallImage) : null)
                        .Where(x => x != null)
                        .ToImmutableArray();

                    foreach (ImageModel item in CoverArr)
                    {
                        item.ContextArray = CoverArr;
                    }
                }
            }
        }
    }
}
