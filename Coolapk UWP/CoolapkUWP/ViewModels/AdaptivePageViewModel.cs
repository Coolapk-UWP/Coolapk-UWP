using CoolapkUWP.Controls;
using CoolapkUWP.Core.Helpers;
using CoolapkUWP.Core.Models;
using CoolapkUWP.Core.Providers;
using CoolapkUWP.Helpers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace CoolapkUWP.ViewModels.AdaptivePage
{
    internal enum ListType
    {
        UserFeed,
        FeedInfo,
        IndexPage,
        FullUrl,
    }

    internal class ViewModel : IViewModel
    {
        public readonly CoolapkListProvider Provider;
        public ObservableCollection<Entity> Models;
        public double[] VerticalOffsets { get; set; } = new double[1];
        public string Title { get; protected set; } = string.Empty;
        public int ComboBoxSelectedIndex { get; private set; }
        internal bool ShowTitleBar { get; }
        internal ImmutableList<CoolapkListProvider> TabProviders { get; private set; } = ImmutableList<CoolapkListProvider>.Empty;

        internal ViewModel(string id, ListType type = ListType.IndexPage, string branch = null, bool showtitle = true)
        {
            ShowTitleBar = showtitle;
            Title = GetTitle(type, branch);
            if (type == ListType.IndexPage)
            {
                GetUri(id);
            }
            Provider = GetProvider(id, type, branch);
            Models = Provider.Models;
        }

        private static string GetTitle(ListType type, string branch)
        {
            Windows.ApplicationModel.Resources.ResourceLoader loader = Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse("AdaptivePage");
            switch (type)
            {
                case ListType.UserFeed:
                    return $"{loader.GetString("UserFeed")}{loader.GetString(branch)}";
                case ListType.FeedInfo:
                    return $"{loader.GetString("FeedInfo")}{loader.GetString(branch)}";
                default: return null;
            }
        }

        private CoolapkListProvider GetProvider(string id, ListType type, string branch)
        {
            return new CoolapkListProvider(
                    GetUri(id, type, branch),
                    (a, b) => a.EntityId == b.Value<string>("entityId").Replace("\"", string.Empty, StringComparison.Ordinal),
                    GetEntities,
                    "entityId");
        }

        internal static Func<int, int, string, string, Uri> GetUri(string id, ListType type, string branch)
        {
            switch (type)
            {
                case ListType.UserFeed:
                    return (p, page, firstItem, lastItem) =>
                        UriHelper.GetUri(
                            UriType.GetUserFeeds,
                            id,
                            p < 0 ? ++page : p,
                            string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}",
                            string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}",
                            branch);
                case ListType.FeedInfo:
                    return (p, page, firstItem, lastItem) =>
                        UriHelper.GetUri(
                            UriType.GetFeedInfos,
                            id,
                            p < 0 ? ++page : p,
                            string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}",
                            string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}",
                            branch);
                case ListType.IndexPage:
                    return (p, page, firstItem, lastItem) =>
                        UriHelper.GetUri(
                            UriType.GetIndexPage,
                            id,
                            id.Contains("?") ? "&" : "?",
                            p < 0 ? ++page : p);
                case ListType.FullUrl:
                    return (p, page, firstItem, lastItem) =>
                        new Uri(id);
                default: return null;
            }
        }

        private IEnumerable<Entity> GetEntities(JObject jo)
        {
            if (ComboBoxSelectedIndex == -1 && jo.TryGetValue("entityTemplate", out JToken t) && t?.ToString() == "configCard")
            {
                JObject j = JObject.Parse(jo.Value<string>("extraData"));
                Title = j.Value<string>("pageTitle");
                yield return null;
            }
            else if (jo.TryGetValue("entityTemplate", out JToken tt) && tt?.ToString() == "fabCard") { yield return null; }
            else if (tt?.ToString() == "feedCoolPictureGridCard")
            {
                foreach (JObject item in jo.Value<JArray>("entities"))
                {
                    Entity entity = EntityTemplateSelector.GetEntity(item);
                    if (entity != null)
                    {
                        yield return entity;
                    }
                }
            }
            else
            {
                yield return EntityTemplateSelector.GetEntity(jo);
            }
            yield break;
        }

        public async Task Refresh(int p = -1)
        {
            await Provider?.Refresh(p);
        }

        internal void AddTab(string uri) => TabProviders = TabProviders.Add(GetProvider(uri, ListType.IndexPage, string.Empty));

        public async Task SetComboBoxSelectedIndex(int value)
        {
            if (value < 0 && value >= TabProviders.Count) { return; }

            ComboBoxSelectedIndex = value;
            if (TabProviders[ComboBoxSelectedIndex].Models.Count == 0)
            {
                await Refresh();
            }
        }

        private string GetUri(string uri)
        {
            if (uri.Contains("&title=", StringComparison.Ordinal))
            {
                const string Value = "&title=";
                Title = uri.Substring(uri.LastIndexOf(Value, StringComparison.Ordinal) + Value.Length);
            }

            if (uri.IndexOf("/page", StringComparison.Ordinal) == -1 && (uri.StartsWith("#", StringComparison.Ordinal) || (!uri.Contains("/main/") && !uri.Contains("/user/") && !uri.Contains("/apk/") && !uri.Contains("/appForum/") && !uri.Contains("/picture/") && !uri.Contains("/topic/") && !uri.Contains("/discovery/"))))
            {
                uri = "/page/dataList?url=" + uri;
            }
            else if (uri.IndexOf("/page", StringComparison.Ordinal) == 0 && !uri.Contains("/page/dataList", StringComparison.Ordinal))
            {
                uri = uri.Replace("/page", "/page/dataList", StringComparison.Ordinal);
            }
            return uri.Replace("#", "%23", StringComparison.Ordinal);
        }
    }
}