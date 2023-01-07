using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Images;
using Newtonsoft.Json.Linq;
using System.ComponentModel;

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

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        protected FeedListDetailBase(JObject token) : base(token)
        {
            EntityFixed = true;
        }
    }

    internal class UserDetail : FeedListDetailBase
    {
        public ImageModel UserAvatar { get; private set; }

        public string Bio { get; private set; }
        public string UserName { get; private set; }

        internal UserDetail(JObject token) : base(token)
        {
            if (token.TryGetValue("userAvatar", out JToken userAvatar))
            {
                UserAvatar = new ImageModel(userAvatar.ToString(), ImageType.BigAvatar);
            }

            if (token.TryGetValue("bio", out JToken bio))
            {
                Bio = bio.ToString();
            }

            if (token.TryGetValue("username", out JToken username))
            {
                UserName = username.ToString();
            }
        }

        public override string ToString() => $"{UserName} - {Bio}";
    }

    internal class TopicDetail : FeedListDetailBase
    {
        public ImageModel Logo { get; private set; }

        public string Title { get; private set; }
        public string Description { get; private set; }

        internal TopicDetail(JObject token) : base(token)
        {
            if (token.TryGetValue("logo", out JToken logo) && !string.IsNullOrEmpty(logo.ToString()))
            {
                Logo = new ImageModel(logo.ToString(), ImageType.Icon);
            }

            if (token.TryGetValue("title", out JToken title) && !string.IsNullOrEmpty(title.ToString()))
            {
                Title = title.ToString();
            }

            if (token.TryGetValue("description", out JToken description) && !string.IsNullOrEmpty(description.ToString()))
            {
                Description = description.ToString();
            }
            else if (token.TryGetValue("intro", out JToken intro) && !string.IsNullOrEmpty(intro.ToString()))
            {
                Description = intro.ToString();
            }
        }

        public override string ToString() => $"{Title} - {Description}";
    }
}
