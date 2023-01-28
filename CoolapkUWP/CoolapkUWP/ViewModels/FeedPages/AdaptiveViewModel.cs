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
using System.ComponentModel;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.UI.Xaml.Controls;

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

        protected override event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
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
            if (string.IsNullOrEmpty(uid)) { throw new ArgumentException(nameof(uid)); }
            return new AdaptiveViewModel(
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
            if (string.IsNullOrEmpty(id)) { throw new ArgumentException(nameof(id)); }
            if (reply == null)
            {
                return new AdaptiveViewModel(
                    new CoolapkListProvider(
                        (p, firstItem, lastItem) =>
                            UriHelper.GetUri(
                                UriType.GetHotReplies,
                                id,
                                p,
                                p > 1 ? $"&firstItem={firstItem}&lastItem={lastItem}" : string.Empty),
                        (o) => new Entity[] { new FeedReplyModel(o) },
                        "id"))
                { Title = $"热门回复" };
            }
            else
            {
                return new AdaptiveViewModel(
                    new CoolapkListProvider(
                        (p, firstItem, lastItem) =>
                            UriHelper.GetUri(
                                UriType.GetReplyReplies,
                                id,
                                p,
                                p > 1 ? $"&lastItem={lastItem}" : string.Empty),
                        (o) => new Entity[] { new FeedReplyModel(o, false) },
                        "id"))
                { Title = $"回复({reply.ReplyNum})" };
            }
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
