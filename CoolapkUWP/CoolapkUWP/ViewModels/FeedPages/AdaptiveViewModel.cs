using CoolapkUWP.Controls.DataTemplates;
using CoolapkUWP.Helpers;
using CoolapkUWP.Models;
using CoolapkUWP.Models.Feeds;
using CoolapkUWP.Models.Users;
using CoolapkUWP.ViewModels.DataSource;
using CoolapkUWP.ViewModels.Providers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoolapkUWP.ViewModels.FeedPages
{
    public class AdaptiveViewModel : DataSourceBase<Entity>, IViewModel
    {
        private readonly string Uri;
        private readonly List<Type> EntityTypes;
        protected bool IsInitPage => Uri == "/main/init";
        protected bool IsIndexPage => !Uri.Contains("?");
        protected bool IsHotFeedPage => Uri == "/main/indexV8" || Uri == "/main/index";

        private readonly CoolapkListProvider Provider;

        private string title = string.Empty;
        public string Title
        {
            get => title;
            protected set
            {
                if (title != value)
                {
                    title = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private bool isShowTitle;
        public bool IsShowTitle
        {
            get => isShowTitle;
            set
            {
                if (isShowTitle != value)
                {
                    isShowTitle = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        internal AdaptiveViewModel(string uri, List<Type> types = null)
        {
            Uri = GetUri(uri);
            EntityTypes = types;
            Provider = new CoolapkListProvider(
                (p, _, __) => UriHelper.GetUri(UriType.GetIndexPage, Uri, IsIndexPage ? "?" : "&", p),
                GetEntities,
                "entityId");
        }

        internal AdaptiveViewModel(CoolapkListProvider provider, List<Type> types = null)
        {
            Provider = provider;
            EntityTypes = types;
        }

        public static AdaptiveViewModel GetUserListProvider(string uid, bool isFollowList, string name)
        {
            return string.IsNullOrEmpty(uid)
                ? throw new ArgumentException(nameof(uid))
                : new AdaptiveViewModel(
                new CoolapkListProvider(
                    (p, firstItem, lastItem) =>
                        UriHelper.GetUri(
                            UriType.GetUserList,
                            isFollowList ? "followList" : "fansList",
                            uid,
                            p,
                            string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}",
                            string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}"),
                    (o) => new Entity[] { new UserModel((JObject)(isFollowList ? o["fUserInfo"] : o["userInfo"])) },
                    "fuid"))
                { Title = $"{name}的{(isFollowList ? "关注" : "粉丝")}" };
        }

        public static AdaptiveViewModel GetReplyListProvider(string id, FeedReplyModel reply = null)
        {
            return string.IsNullOrEmpty(id)
                ? throw new ArgumentException(nameof(id))
                : reply == null
                ? new AdaptiveViewModel(
                    new CoolapkListProvider(
                        (p, firstItem, lastItem) =>
                            UriHelper.GetUri(
                                UriType.GetHotReplies,
                                id,
                                p,
                                p > 1 ? $"&firstItem={firstItem}&lastItem={lastItem}" : string.Empty),
                        (o) => new Entity[] { new FeedReplyModel(o) },
                        "uid"))
                { Title = $"热门回复" }
                : new AdaptiveViewModel(
                    new CoolapkListProvider(
                        (p, firstItem, lastItem) =>
                            UriHelper.GetUri(
                                UriType.GetReplyReplies,
                                id,
                                p,
                                p > 1 ? $"&lastItem={lastItem}" : string.Empty),
                        (o) => new Entity[] { new FeedReplyModel(o, false) },
                        "uid"))
                { Title = $"回复({reply.ReplyNum})" };
        }

        public static AdaptiveViewModel GetHistoryProvider(string title)
        {
            if (string.IsNullOrEmpty(title)) { throw new ArgumentException(nameof(title)); }

            UriType type = UriType.CheckLoginInfo;

            switch (title)
            {
                case "我的常去":
                    type = UriType.GetUserRecentHistory;
                    break;
                case "浏览历史":
                    type = UriType.GetUserHistory;
                    break;
                default: throw new ArgumentException(nameof(title));
            }

            return new AdaptiveViewModel(
                new CoolapkListProvider(
                    (p, firstItem, lastItem) =>
                        UriHelper.GetUri(
                            type,
                            p,
                            string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}",
                            string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}"),
                    (o) => new Entity[] { new HistoryModel(o) },
                    "uid"))
                { Title = title };
        }

        public static AdaptiveViewModel GetUserFeedsProvider(string uid, string branch)
        {
            return string.IsNullOrEmpty(uid)
                ? throw new ArgumentException(nameof(uid))
                : new AdaptiveViewModel(
                    new CoolapkListProvider(
                        (p, firstItem, lastItem) =>
                            UriHelper.GetUri(
                                UriType.GetUserFeeds,
                                uid,
                                p,
                                string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}",
                                string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}",
                                branch),
                        (o) => new Entity[] { new FeedModel(o) },
                        "uid"));
        }

        public async Task Refresh(bool reset = false)
        {
            if (reset)
            {
                await Reset();
            }
            else
            {
                _ = await LoadItemsAsync(20);
            }
        }

        bool IViewModel.IsEqual(IViewModel other) => other is AdaptiveViewModel model && IsEqual(model);

        public bool IsEqual(AdaptiveViewModel other) => !string.IsNullOrWhiteSpace(Uri) ? Uri == other.Uri : Provider == other.Provider;

        private string GetUri(string uri)
        {
            if (uri.Contains("&title="))
            {
                const string Value = "&title=";
                Title = uri.Substring(uri.LastIndexOf(Value, StringComparison.Ordinal) + Value.Length);
            }

            if (uri.StartsWith("url="))
            {
                uri = uri.Replace("url=", string.Empty);
            }

            if (uri.IndexOf("/page", StringComparison.Ordinal) == -1 && (uri.StartsWith("#", StringComparison.Ordinal) || (!uri.Contains("/main/") && !uri.Contains("/user/") && !uri.Contains("/apk/") && !uri.Contains("/appForum/") && !uri.Contains("/picture/") && !uri.Contains("/topic/") && !uri.Contains("/discovery/"))))
            {
                uri = "/page/dataList?url=" + uri;
            }
            else if (uri.IndexOf("/page", StringComparison.Ordinal) == 0 && !uri.Contains("/page/dataList"))
            {
                uri = uri.Replace("/page", "/page/dataList");
            }
            return uri.Replace("#", "%23");
        }

        private IEnumerable<Entity> GetEntities(JObject json)
        {
            if (json.TryGetValue("entityTemplate", out JToken t) && t?.ToString() == "configCard")
            {
                JObject j = JObject.Parse(json.Value<string>("extraData"));
                Title = j.Value<string>("pageTitle");
                yield return null;
            }
            else if (json.TryGetValue("entityTemplate", out JToken tt) && tt?.ToString() == "fabCard") { yield return null; }
            else if (tt?.ToString() == "feedCoolPictureGridCard")
            {
                foreach (JToken item in json.Value<JArray>("entities"))
                {
                    Entity entity = EntityTemplateSelector.GetEntity((JObject)item, IsHotFeedPage);
                    if (entity != null)
                    {
                        yield return entity;
                    }
                }
            }
            else
            {
                yield return EntityTemplateSelector.GetEntity(json, IsHotFeedPage);
            }
            yield break;
        }

        protected override async Task<IList<Entity>> LoadItemsAsync(uint count)
        {
            List<Entity> Models = new List<Entity>();
            while (Models.Count < count)
            {
                int temp = Models.Count;
                if (Models.Count > 0) { _currentPage++; }
                await Provider.GetEntity(Models, _currentPage);
                if (Models.Count <= 0 || Models.Count <= temp) { break; }
            }
            return Models;
        }

        protected override void AddItems(IList<Entity> items)
        {
            if (items != null)
            {
                foreach (Entity item in items)
                {
                    if (item is NullEntity) { continue; }
                    if (EntityTypes == null || EntityTypes.Contains(item.GetType())) { Add(item); }
                }
            }
        }
    }
}
