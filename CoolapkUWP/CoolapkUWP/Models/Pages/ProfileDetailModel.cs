using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Images;
using Newtonsoft.Json.Linq;
using System.ComponentModel;

namespace CoolapkUWP.Models.Pages
{
    public class ProfileDetailModel : Entity, INotifyPropertyChanged
    {
        public ImageModel UserAvatar { get; private set; }
        public string Url { get; private set; }
        public double FansNum { get; private set; }
        public double FeedNum { get; private set; }
        public double LevelNum { get; private set; }
        public string UserName { get; private set; }
        public double FollowNum { get; private set; }
        public string LevelTodayMessage { get; private set; }
        public double NextLevelExperience { get; private set; }
        public double NextLevelPercentage { get; private set; }
        public string NextLevelNowExperience { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }

        public ProfileDetailModel(JObject token) : base(token)
        {
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
                FansNum = fans.ToObject<double>();
            }

            if (token.TryGetValue("feed", out JToken feed))
            {
                FeedNum = feed.ToObject<double>();
            }

            if (token.TryGetValue("level", out JToken level))
            {
                LevelNum = level.ToObject<double>();
            }

            if (token.TryGetValue("username", out JToken username))
            {
                UserName = username.ToString();
            }

            if (token.TryGetValue("follow", out JToken follow))
            {
                FollowNum = follow.ToObject<double>();
            }

            if (token.TryGetValue("level_today_message", out JToken level_today_message))
            {
                LevelTodayMessage = level_today_message.ToString();
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
        }
    }
}
