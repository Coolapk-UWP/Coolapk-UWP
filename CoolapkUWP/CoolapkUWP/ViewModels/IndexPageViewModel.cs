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

namespace CoolapkUWP.ViewModels.IndexPage
{
    internal class ViewModel : IViewModel, ICanComboBoxChangeSelectedIndex
    {
        private readonly string mainUri;
        internal CoolapkListProvider mainProvider { get; private set; }
        internal readonly ObservableCollection<Entity> mainModels;
        internal ImmutableList<CoolapkListProvider> tabProviders { get; private set; } = ImmutableList<CoolapkListProvider>.Empty;
        protected bool IsHotFeedPage => mainUri == "/main/indexV8" || mainUri == "/main/index";
        protected bool IsIndexPage => !mainUri.Contains("?");
        protected bool IsInitPage => mainUri == "/main/init";
        internal bool ShowTitleBar { get; }

        public int ComboBoxSelectedIndex { get; private set; }
        public double[] VerticalOffsets { get; set; } = new double[1];
        public string Title { get; protected set; }

        internal ViewModel(string uri, bool showTitleBar)
        {
            ShowTitleBar = showTitleBar;
            mainUri = GetUri(uri);

            mainProvider = GetProvider(mainUri);
            //UIHelper.ShowMessage(mainUri);
            mainModels = mainProvider.Models;
        }

        public async Task Refresh(int p = -1)
        {
            if (tabProviders.Count == 0)
            {
                await mainProvider?.Refresh(p);
            }
            else if (p == -1 && ComboBoxSelectedIndex < tabProviders.Count)
            {
                await tabProviders[ComboBoxSelectedIndex]?.Refresh(p);
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
                    Entity entity = EntityTemplateSelector.GetEntity(item, IsHotFeedPage);
                    if (entity != null)
                    {
                        yield return entity;
                    }
                }
            }
            else
            {
                yield return EntityTemplateSelector.GetEntity(jo, IsHotFeedPage);
            }
            yield break;
        }

        private CoolapkListProvider GetProvider(string uri)
        {
            return new CoolapkListProvider(
                    GetUri(uri, IsIndexPage),
                    (a, b) => a.EntityId == b.Value<string>("entityId").Replace("\"", string.Empty, StringComparison.Ordinal),
                    GetEntities,
                    "entityId");
        }

        internal static Func<int, int, string, string, Uri> GetUri(string uri, bool isHotFeedPage)
        {
            return (p, page, _, __) =>
                UriHelper.GetUri(
                    UriType.GetIndexPage,
                    uri,
                    isHotFeedPage ? "?" : "&",
                    p < 0 ? ++page : p);
        }

        internal void AddTab(string uri) => tabProviders = tabProviders.Add(GetProvider(uri));

        public async Task SetComboBoxSelectedIndex(int value)
        {
            if (value < 0 && value >= tabProviders.Count) { return; }

            ComboBoxSelectedIndex = value;
            if (tabProviders[ComboBoxSelectedIndex].Models.Count == 0)
            {
                await Refresh();
            }
        }
    }
}