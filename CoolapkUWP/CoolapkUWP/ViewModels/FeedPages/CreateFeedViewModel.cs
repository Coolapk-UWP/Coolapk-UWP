using CoolapkUWP.Helpers;
using CoolapkUWP.Models;
using CoolapkUWP.Models.Users;
using CoolapkUWP.ViewModels.DataSource;
using CoolapkUWP.ViewModels.Providers;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace CoolapkUWP.ViewModels.FeedPages
{
    public class CreateFeedViewModel : IViewModel
    {
        private string title = string.Empty;
        public string Title
        {
            get => title;
            set
            {
                if (title != value)
                {
                    title = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        public CreateUserItemSourse CreateUserItemSourse = new CreateUserItemSourse();
        public CreateTopicItemSourse CreateTopicItemSourse = new CreateTopicItemSourse();

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        public async Task Refresh(bool reset)
        {
            await CreateUserItemSourse.Refresh(reset);
            await CreateTopicItemSourse.Refresh(reset);
        }
    }

    public class CreateUserItemSourse : EntityItemSourse
    {
        private string keyword = string.Empty;
        public string Keyword
        {
            get => keyword;
            set
            {
                if (keyword != value)
                {
                    keyword = value;
                    UpdateProvider(value);
                }
            }
        }

        public CreateUserItemSourse(string keyword = " ")
        {
            Keyword = keyword;
        }

        private void UpdateProvider(string keyword)
        {
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                Provider = new CoolapkListProvider(
                    (p, firstItem, lastItem) =>
                    UriHelper.GetUri(
                        UriType.SearchCreateUsers,
                        keyword,
                        p,
                        p > 1 ? $"&firstItem={firstItem}&lastItem={lastItem}" : string.Empty),
                    GetEntities,
                    "uid");
            }
            else if (SettingsHelper.Get<string>(SettingsHelper.Uid) is string uid && !string.IsNullOrEmpty(uid))
            {
                Provider = new CoolapkListProvider(
                    (p, firstItem, lastItem) =>
                        UriHelper.GetUri(
                            UriType.GetUserList,
                            "followList",
                            uid,
                            p,
                            string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}",
                            string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}"),
                    (o) => new Entity[] { new UserModel((JObject)o["fUserInfo"]) },
                    "fuid");
            }
        }

        private IEnumerable<Entity> GetEntities(JObject jo)
        {
            yield return new UserModel(jo);
        }
    }

    public class CreateTopicItemSourse : EntityItemSourse
    {
        private string keyword = string.Empty;
        public string Keyword
        {
            get => keyword;
            set
            {
                if (keyword != value)
                {
                    keyword = value;
                    UpdateProvider(value);
                }
            }
        }

        public CreateTopicItemSourse(string keyword = " ")
        {
            Keyword = keyword;
        }

        private void UpdateProvider(string keyword)
        {
            Provider = new CoolapkListProvider(
                (p, firstItem, lastItem) =>
                UriHelper.GetUri(
                    UriType.SearchCreateTags,
                    keyword,
                    p,
                    p > 1 ? $"&firstItem={firstItem}&lastItem={lastItem}" : string.Empty),
                GetEntities,
                "id");
        }

        private IEnumerable<Entity> GetEntities(JObject jo)
        {
            yield return new TopicModel(jo);
        }
    }
}
