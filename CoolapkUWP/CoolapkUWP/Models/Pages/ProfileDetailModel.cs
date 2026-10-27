using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Images;
using Newtonsoft.Json.Linq;

namespace CoolapkUWP.Models.Pages
{
    public sealed class ProfileDetailModel : Entity
    {
        public int UID { get; private set; }
        public int RegDate { get; private set; }
        public int LoginTime { get; private set; }
        public int BlockStatus { get; private set; }

        public int FansNum { get; private set; }
        public int FeedNum { get; private set; }
        public int LevelNum { get; private set; }
        public int FollowNum { get; private set; }

        public string Url { get; private set; }
        public string UserName { get; private set; }
        public string LoginText { get; private set; }
        public string VerifyTitle { get; private set; }
        public string LevelTodayMessage { get; private set; }

        public long Experience { get; private set; }
        public long NextLevelExperience { get; private set; }
        public double NextLevelPercentage { get; private set; }
        public string NextLevelNowExperience => $"{NextLevelPercentage / 100 * NextLevelExperience:F0}/{NextLevelExperience}";

        public ImageModel Cover { get; private set; }
        public ImageModel UserAvatar { get; private set; }

        public ProfileDetailModel(JObject token) : base(token)
        {
            if (token.TryGetValue("cover", out JToken cover))
            {
                Cover = new ImageModel(cover.ToString(), ImageType.OriginImage);
            }

            if (token.TryGetValue("userAvatar", out JToken userAvatar))
            {
                UserAvatar = new ImageModel(userAvatar.ToString(), ImageType.BigAvatar);
            }

            if (token.TryGetValue("url", out JToken url))
            {
                Url = $"https://www.coolapk.com{url}";
            }

            if (token.TryGetValue("fans", out JToken fans))
            {
                FansNum = fans.ToObject<int>();
            }

            if (token.TryGetValue("feed", out JToken feed))
            {
                FeedNum = feed.ToObject<int>();
            }

            if (token.TryGetValue("level", out JToken level))
            {
                LevelNum = level.ToObject<int>();
            }

            if (token.TryGetValue("uid", out JToken uid))
            {
                UID = uid.ToObject<int>();
            }

            if (token.TryGetValue("username", out JToken username))
            {
                UserName = username.ToString();
            }

            if (token.TryGetValue("regdate", out JToken regdate))
            {
                RegDate = regdate.ToObject<int>();
            }

            if (token.TryGetValue("logintime", out JToken logintime))
            {
                LoginTime = logintime.ToObject<int>();
                LoginText = $"{logintime.ToObject<long>().ConvertUnixTimeStampToReadable()}活跃";
            }

            if (token.TryGetValue("block_status", out JToken block_status))
            {
                BlockStatus = block_status.ToObject<int>();
            }

            if (token.TryGetValue("verify_title", out JToken verify_title))
            {
                VerifyTitle = verify_title.ToString();
            }

            if (token.TryGetValue("follow", out JToken follow))
            {
                FollowNum = follow.ToObject<int>();
            }

            if (token.TryGetValue("experience", out JToken experience))
            {
                Experience = experience.ToObject<long>();
            }

            if (token.TryGetValue("level_today_message", out JToken level_today_message))
            {
                LevelTodayMessage = level_today_message.ToString();
            }

            if (token.TryGetValue("next_level_experience", out JToken next_level_experience))
            {
                NextLevelExperience = next_level_experience.ToObject<long>();
            }

            if (token.TryGetValue("next_level_percentage", out JToken next_level_percentage))
            {
                NextLevelPercentage = next_level_percentage.ToObject<double>();
            }
        }
    }
}
