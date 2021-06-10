using Newtonsoft.Json.Linq;
using System.Collections.Immutable;
using System.ComponentModel;
using Windows.Storage;
using ImageSource = Windows.UI.Xaml.Media.ImageSource;

namespace CoolapkUWP.Models.Controls
{
    public class UserHubModel : INotifyPropertyChanged
    {

        private ImageSource userAvatar;
        private string userName;
        private double followNum;
        private double fansNum;
        private double feedNum;
        private double levelNum;
        private double nextLevelExperience;
        private double nextLevelPercentage;
        private string nextLevelNowExperience;
        private string levelTodayMessage;

        public ImageSource UserAvatar
        {
            get => userAvatar;
            private set
            {
                userAvatar = value;
                RaisePropertyChangedEvent();
            }
        }

        public string UserName
        {
            get => userName;
            private set
            {
                userName = value;
                RaisePropertyChangedEvent();
            }
        }

        public double FollowNum
        {
            get => followNum;
            private set
            {
                followNum = value;
                RaisePropertyChangedEvent();
            }
        }

        public double FansNum
        {
            get => fansNum;
            private set
            {
                fansNum = value;
                RaisePropertyChangedEvent();
            }
        }

        public double FeedNum
        {
            get => feedNum;
            private set
            {
                feedNum = value;
                RaisePropertyChangedEvent();
            }
        }

        public double LevelNum
        {
            get => levelNum;
            private set
            {
                levelNum = value;
                RaisePropertyChangedEvent();
            }
        }

        public double NextLevelExperience
        {
            get => nextLevelExperience;
            private set
            {
                nextLevelExperience = value;
                RaisePropertyChangedEvent();
            }
        }

        public double NextLevelPercentage
        {
            get => nextLevelPercentage;
            private set
            {
                nextLevelPercentage = value;
                RaisePropertyChangedEvent();
            }
        }

        public string NextLevelNowExperience
        {
            get => nextLevelNowExperience;
            private set
            {
                nextLevelNowExperience = value;
                RaisePropertyChangedEvent();
            }
        }

        public string LevelTodayMessage
        {
            get => levelTodayMessage;
            private set
            {
                levelTodayMessage = value;
                RaisePropertyChangedEvent();
            }
        }

        public bool IsAuthor { get; private set; }
        public bool IsSpecial { get; private set; }

        #region
        private static readonly ImmutableArray<string> authors = new string[]
        {
            "536381",//wherewhere
            "695942",//一块小板子
        }.ToImmutableArray();

        private static readonly ImmutableArray<string> specials = new string[]
        {
            "1222543",
            "1893913",
            "2134270",
            "1494629",
            "3327704",
            "3591060",
        }.ToImmutableArray();
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null)
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public UserHubModel(JObject o, ImageSource image)
        {
            UserAvatar = image;
            UserName = o.Value<string>("username");
            FeedNum = o.Value<int>("feed");
            FollowNum = o.Value<int>("follow");
            FansNum = o.Value<int>("fans");
            LevelNum = o.Value<int>("level");
            NextLevelExperience = o.Value<int>("next_level_experience");
            NextLevelPercentage = o.Value<double>("next_level_percentage");
            LevelTodayMessage = o.Value<string>("level_today_message");
            NextLevelNowExperience = $"{nextLevelPercentage / 100 * nextLevelExperience:F0}/{nextLevelExperience}";
            if (o.TryGetValue("entityId", out JToken u))
            {
                FindIsAuthor(u.ToString());
                FindIsSpecial(u.ToString());
            }
            ApplicationData.Current.LocalSettings.Values["IsAuthor"] = IsAuthor;
            ApplicationData.Current.LocalSettings.Values["IsSpecial"] = IsSpecial;
        }

        #region
        private void FindIsAuthor(string uid)
        {
            foreach (string i in authors)
            {
                if (uid == i)
                {
                    IsAuthor = true;
                    break;
                }
            }
        }

        private void FindIsSpecial(string uid)
        {
            foreach (string i in specials)
            {
                if (uid == i)
                {
                    IsSpecial = true;
                    break;
                }
            }
        }
        #endregion
    }
}