using CoolapkUWP.Core.Helpers;
using CoolapkUWP.Core.Models;
using CoolapkUWP.Core.Providers;
using CoolapkUWP.Helpers;
using CoolapkUWP.Models;
using CoolapkUWP.Models.Pages.FeedListPageModels;
using CoolapkUWP.Pages.FeedPages;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace CoolapkUWP.ViewModels.FeedListPage
{
    internal abstract class FeedListPageViewModelBase : IViewModel
    {
        public string Id { get; }
        public FeedListType ListType { get; }
        protected abstract CoolapkListProvider Provider { get; }
        public ObservableCollection<Entity> Models { get => Provider?.Models ?? null; }
        public double[] VerticalOffsets { get; set; } = new double[1];

        public string Title { get; protected set; }

        protected const string idName = "id";
        protected static readonly Func<JObject, Entity[]> getEntity = (o) => new Entity[] { new FeedModel(o) };
        protected static readonly Func<Entity, JToken, bool> isEqual = (a, b) => a is FeedListDetailBase ? false : ((FeedModel)a).Id == b.Value<int>("id").ToString();

        protected FeedListPageViewModelBase(string id, FeedListType type)
        {
            Id = string.IsNullOrEmpty(id)
                ? throw new ArgumentException(nameof(id))
                : id;
            ListType = type;
        }

        public static FeedListPageViewModelBase GetProvider(FeedListType type, string id)
        {
            if (string.IsNullOrEmpty(id) || id == "0") return null;
            switch (type)
            {
                case FeedListType.UserPageList: return new UserViewModel(id);
                case FeedListType.TagPageList: return new TagViewModel(id);
                case FeedListType.DyhPageList: return new DyhViewModel(id);
                case FeedListType.CollectionPageList: return new CollectionViewModel(id);
                default: return null;
            }
        }

        public void ChangeCopyMode(bool mode)
        {
            if (Models.Count == 0) { return; }
            if (Models[0] is FeedListDetailBase detail)
            {
                detail.IsCopyEnabled = mode;
            }
        }

        private async Task<FeedListDetailBase> GetDetail()
        {
            UriType type;
            switch (ListType)
            {
                case FeedListType.UserPageList:
                    type = UriType.GetUserSpace;
                    break;

                case FeedListType.TagPageList:
                    type = UriType.GetTagDetail;
                    break;

                case FeedListType.DyhPageList:
                    type = UriType.GetDyhDetail;
                    break;

                case FeedListType.CollectionPageList:
                    type = UriType.GetCollectionDetail;
                    break;

                default:
                    throw new ArgumentException($"{typeof(FeedListType).FullName}值错误");
            }
            var (isSucceed, result) = await DataHelper.GetDataAsync(UriHelper.GetUri(type, Id), true);
            if (!isSucceed) { return null; }

            var o = (JObject)result;
            FeedListDetailBase d = null;
            if (o != null)
            {
                switch (ListType)
                {
                    case FeedListType.UserPageList:
                        d = new UserDetail(o);
                        break;

                    case FeedListType.TagPageList:
                        d = new TopicDetail(o);
                        break;

                    case FeedListType.DyhPageList:
                        d = new DyhDetail(o);
                        break;

                    case FeedListType.CollectionPageList:
                        d = new CollectionDetail(o);
                        break;
                }
            }
            return d;
        }

        protected abstract string GetTitleBarText(FeedListDetailBase detail);

        public async Task Refresh(int p)
        {
            await Provider.Refresh(p);

            ICanComboBoxChangeSelectedIndex it = null;
            if (Models.Count > 0)
            {
                it = Models[0] as ICanComboBoxChangeSelectedIndex;
                Models.RemoveAt(0);
            }
            var item = await GetDetail();
            Title = GetTitleBarText(item);
            if (it != null)
            {
                await (item as ICanComboBoxChangeSelectedIndex).SetComboBoxSelectedIndex(it.ComboBoxSelectedIndex);
            }
            Models.Insert(0, item);
        }
    }

    internal class UserViewModel : FeedListPageViewModelBase
    {
        protected override CoolapkListProvider Provider { get; }

        internal UserViewModel(string uid) : base(uid, FeedListType.UserPageList)
        {
            Provider =
                new CoolapkListProvider(
                    (p, page, firstItem, lastItem) =>
                        UriHelper.GetUri(
                            UriType.GetUserFeeds,
                            Id,
                            p < 0 ? ++page : p,
                            string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}",
                            string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}"),
                    isEqual, getEntity, idName);
        }

        public void Report()
        {
            UIHelper.Navigate(typeof(Pages.BrowserPage), new object[] { false, $"https://m.coolapk.com/mp/do?c=user&m=report&id={Id}" });
        }

        protected override string GetTitleBarText(FeedListDetailBase detail) => (detail as UserDetail).UserName;
    }

    internal class TagViewModel : FeedListPageViewModelBase, ICanComboBoxChangeSelectedIndex
    {
        public int ComboBoxSelectedIndex { get; private set; }

        private string sortType = "lastupdate_desc";

        protected override CoolapkListProvider Provider { get; }

        internal TagViewModel(string tag) : base(tag, FeedListType.TagPageList)
        {
            Provider =
                new CoolapkListProvider(
                    (p, page, firstItem, lastItem) =>
                        UriHelper.GetUri(
                            UriType.GetTagFeeds,
                            Id,
                            p < 0 ? ++page : p,
                            string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}",
                            string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}",
                            sortType),
                    isEqual, getEntity, idName);
        }

        protected override string GetTitleBarText(FeedListDetailBase detail) => (detail as TopicDetail).Title;

        public async Task SetComboBoxSelectedIndex(int value)
        {
            switch (value)
            {
                case -1: return;
                case 0:
                    sortType = "lastupdate_desc";
                    break;

                case 1:
                    sortType = "dateline_desc";
                    break;

                case 2:
                    sortType = "popular";
                    break;
            }
            ComboBoxSelectedIndex = value;
            if (Provider != null)
            {
                await Refresh(-2);
            }
        }
    }

    internal class DyhViewModel : FeedListPageViewModelBase, ICanComboBoxChangeSelectedIndex
    {
        public int ComboBoxSelectedIndex { get; private set; }

        public async Task SetComboBoxSelectedIndex(int value)
        {
            if (value > -1)
            {
                ComboBoxSelectedIndex = value;
                if (Provider != null)
                {
                    await Refresh(-2);
                }
            }
        }

        protected override CoolapkListProvider Provider { get; }

        internal DyhViewModel(string id) : base(id, FeedListType.DyhPageList)
        {
            Provider =
                new CoolapkListProvider(
                    (p, page, firstItem, lastItem) =>
                        UriHelper.GetUri(
                            UriType.GetDyhFeeds,
                            Id,
                            ComboBoxSelectedIndex == 0 ? "all" : "square",
                            p < 0 ? ++page : p,
                            string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}",
                            string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}"),
                    isEqual, getEntity, idName);
        }

        protected override string GetTitleBarText(FeedListDetailBase detail) => (detail as DyhDetail).Title;
    }

    internal class CollectionViewModel : FeedListPageViewModelBase, ICanComboBoxChangeSelectedIndex
    {
        protected override CoolapkListProvider Provider { get; }

        public int ComboBoxSelectedIndex { get; private set; }

        public ObservableCollection<string> ComboBoxItems { get; private set; }
        private System.Collections.Generic.List<string> comboBoxLinks;
        public async Task SetComboBoxSelectedIndex(int value)
        {
            if (value > -1)
            {
                ComboBoxSelectedIndex = value;
                if (Provider != null)
                {
                    await Refresh(-2);
                }
            }
        }


        internal CollectionViewModel(string id) : base(id, FeedListType.CollectionPageList)
        {
            ComboBoxItems = new ObservableCollection<string>(new[] { "全部" });
            comboBoxLinks = new System.Collections.Generic.List<string>(new[] { string.Empty });
            Provider =
                new CoolapkListProvider(
                    (p, page, firstItem, lastItem) =>
                    {
                        if (ComboBoxSelectedIndex == 0 && string.IsNullOrEmpty(comboBoxLinks[0]))
                        {
                            return UriHelper.GetUri(
                                UriType.GetCollectionContents,
                                Id,
                                p < 0 ? ++page : p,
                                string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}");
                        }
                        else
                        {
                            var str = $"/v6/page/dataList?url={comboBoxLinks[ComboBoxSelectedIndex]}&page={(p < 0 ? ++page : p)}{(string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}")}";
                            return new Uri(UriHelper.BaseUri, str.Replace("#", "%23", StringComparison.Ordinal));
                        }
                    },
                    isEqual, (o) =>
                    {
                        if (o.Value<string>("entityType") == "card" && o.Value<string>("entityTemplate") == "selectorLinkCard")
                        {
                            var array = o["entities"] as JArray;
                            for (int i = 0; i < array.Count; i++)
                            {
                                string title = i == 0 ? "全部" : array[i].Value<string>("title");
                                var url = array[i].Value<string>("url") + "&title=" + title;
                                if (i == 0)
                                {
                                    comboBoxLinks[i] = url;
                                }
                                else
                                {
                                    ComboBoxItems.Add(title);
                                    comboBoxLinks.Add(url);
                                }
                            }
                            return null;
                        }
                        else
                        {
                            return new[] { new FeedModel(o) };
                        }
                    }, "entityId");
        }

        protected override string GetTitleBarText(FeedListDetailBase detail) => (detail as CollectionDetail).Title;
    }

}