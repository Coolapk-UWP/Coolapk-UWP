using CoolapkUWP.Core.Helpers;
using CoolapkUWP.Core.Models;
using CoolapkUWP.Core.Providers;
using CoolapkUWP.Helpers;
using CoolapkUWP.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace CoolapkUWP.ViewModels.UserListPage
{
    internal class ViewModel : IViewModel
    {
        private readonly CoolapkListProvider provider;
        public ObservableCollection<Entity> Models { get => provider?.Models ?? null; }

        public double[] VerticalOffsets { get; set; } = new double[1];
        public string Title { get; }

        internal ViewModel(string uid, bool isFollowList, string name)
        {
            if (string.IsNullOrEmpty(uid)) { throw new ArgumentException(nameof(uid)); }

            Title = $"{name}的{(isFollowList ? "关注" : "粉丝")}";
            provider =
                new CoolapkListProvider(
                    (p, page, firstItem, lastItem) =>
                        UriHelper.GetUri(
                            UriType.GetUserList,
                            isFollowList ? "followList" : "fansList",
                            uid,
                            p < 0 ? ++page : p,
                            string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}",
                            string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}"),
                    (a, b) => ((UserModel)a).UserName == b.Value<string>(isFollowList ? "fusername" : "username"),
                    (o) => new Entity[] { new UserModel((JObject)(isFollowList ? o["fUserInfo"] : o["userInfo"])) },
                    "fuid");
        }

        public async Task Refresh(int p = -1) => await provider?.Refresh(p);
    }
}