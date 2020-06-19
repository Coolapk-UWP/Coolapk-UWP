using CoolapkUWP.Helpers;
using CoolapkUWP.Models;
using CoolapkUWP.Models.Pages.FeedListPageModels;
using CoolapkUWP.Pages.FeedPages;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace CoolapkUWP.ViewModels.FeedListDataProvider
{
    internal interface ICanChangeSelectedIndex
    {
        int SelectedIndex { get; set; }

        void Reset();
    }

    internal abstract class FeedListDataProvider
    {
        public static FeedListDataProvider GetProvider(FeedListType type, string id)
        {
            if (string.IsNullOrEmpty(id) || id == "0") return null;
            switch (type)
            {
                case FeedListType.UserPageList: return new UserPageDataProvider(id);
                case FeedListType.TagPageList: return new TagPageDataProvider(id);
                case FeedListType.DyhPageList: return new DyhPageDataProvider(id);
                default: return null;
            }
        }

        protected FeedListDataProvider() { }

        public string Id { get; protected set; }
        public FeedListType ListType { get; protected set; }
        public readonly ObservableCollection<object> itemCollection = new ObservableCollection<object>();
        public string Title { get; protected set; }

        public async Task<IDetail> GetDetail()
        {
            JObject o;
            IDetail d;
            DataUriType type;
            switch (ListType)
            {
                case FeedListType.UserPageList:
                    type = DataUriType.GetUserSpace;
                    d = new UserDetail();
                    break;
                case FeedListType.TagPageList:
                    type = DataUriType.GetTagDetail;
                    d = new TopicDetail();
                    break;
                case FeedListType.DyhPageList:
                    type = DataUriType.GetDyhDetail;
                    d = new DyhDetail();
                    break;
                default:
                    throw new System.Exception("ListType值错误");
            }
            o = (JObject)await DataHelper.GetDataAsync(type, Id);
            if (o != null)
            {
                d.Initialize(o);
            }
            Title = GetTitleBarText(d);
            return d;

        }

        public abstract Task<List<FeedModel>> GetFeeds(int p = -1);

        internal abstract string GetTitleBarText(IDetail detail);

        public async Task Refresh()
        {
            if (itemCollection.Count > 0) itemCollection.RemoveAt(0);
            itemCollection.Insert(0, await GetDetail());
            List<FeedModel> feeds = await GetFeeds(1);
            if (feeds != null)
                for (int i = 0; i < feeds.Count; i++)
                    itemCollection.Insert(i + 1, feeds[i]);
        }

        public async Task LoadNextPage()
        {
            List<FeedModel> feeds = await GetFeeds();
            if (feeds != null)
                foreach (var item in feeds)
                    itemCollection.Add(item);
        }
    }

    internal class UserPageDataProvider : FeedListDataProvider
    {
        private int page;
        private int firstItem, lastItem;

        private UserPageDataProvider() => ListType = FeedListType.UserPageList;

        internal UserPageDataProvider(string uid) : this() => Id = uid;

        public override async Task<List<FeedModel>> GetFeeds(int p = -1)
        {
            if (p == 1 && page == 0) page = 1;
            JArray array = (JArray)await DataHelper.GetDataAsync(DataUriType.GetUserFeeds,
                                                            Id,
                                                            p == -1 ? ++page : p,
                                                            firstItem == 0 ? string.Empty : $"&firstItem={firstItem}",
                                                            lastItem == 0 ? string.Empty : $"&lastItem={lastItem}");
            if (!(array is null) && array.Count != 0)
            {
                if (page == 1 || p == 1)
                    firstItem = array.First.Value<int>("id");
                lastItem = array.Last.Value<int>("id");
                List<FeedModel> FeedsCollection = new List<FeedModel>();
                foreach (JObject i in array) FeedsCollection.Add(new FeedModel(i));
                return FeedsCollection;
            }
            else
            {
                page--;
                return null;
            }
        }

        internal override string GetTitleBarText(IDetail detail) => (detail as UserDetail).UserName;
    }

    internal class TagPageDataProvider : FeedListDataProvider, ICanChangeSelectedIndex
    {
        private int page, _selectedIndex;

        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                if (value > -1)
                    _selectedIndex = value;
            }
        }

        private double firstItem, lastItem;

        private TagPageDataProvider() => ListType = FeedListType.TagPageList;

        internal TagPageDataProvider(string tag) : this() => Id = tag;

        public void Reset() => firstItem = lastItem = page = 0;

        public override async Task<List<FeedModel>> GetFeeds(int p = -1)
        {
            string sortType = string.Empty;
            switch (SelectedIndex)
            {
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
            if (p == 1 && page == 0) page = 1;
            JArray array = (JArray)await DataHelper.GetDataAsync(DataUriType.GetTagFeeds,
                                                            Id,
                                                            p == -1 ? ++page : p,
                                                            firstItem == 0 ? string.Empty : $"&firstItem={firstItem}",
                                                            lastItem == 0 ? string.Empty : $"&lastItem={lastItem}",
                                                            sortType);
            if (!(array is null) && array.Count != 0)
            {
                if (page == 1 || p == 1)
                    firstItem = array.First.Value<int>("id");
                lastItem = array.Last.Value<int>("id");
                List<FeedModel> FeedsCollection = new List<FeedModel>();
                foreach (JObject i in array) FeedsCollection.Add(new FeedModel(i));
                return FeedsCollection;
            }
            else
            {
                page--;
                return null;
            }
        }

        internal override string GetTitleBarText(IDetail detail) => (detail as TopicDetail).Title;
    }

    internal class DyhPageDataProvider : FeedListDataProvider, ICanChangeSelectedIndex
    {
        private int page, _selectedIndex;

        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                if (value > -1)
                    _selectedIndex = value;
            }
        }

        private double firstItem, lastItem;

        private DyhPageDataProvider() => ListType = FeedListType.DyhPageList;

        internal DyhPageDataProvider(string id) : this() => Id = id;

        public void Reset() => firstItem = lastItem = page = 0;

        public override async Task<List<FeedModel>> GetFeeds(int p = -1)
        {
            if (p == 1 && page == 0) page = 1;
            JArray array = (JArray)await DataHelper.GetDataAsync(DataUriType.GetDyhFeeds,
                                                            Id,
                                                            SelectedIndex == 0 ? "all" : "square",
                                                            p == -1 ? ++page : p,
                                                            firstItem == 0 ? string.Empty : $"&firstItem={firstItem}",
                                                            lastItem == 0 ? string.Empty : $"&lastItem={lastItem}");
            if (!(array is null) && array.Count != 0)
            {
                if (page == 1 || p == 1)
                    firstItem = array.First.Value<int>("id");
                lastItem = array.Last.Value<int>("id");
                List<FeedModel> FeedsCollection = new List<FeedModel>();
                foreach (JObject i in array) FeedsCollection.Add(new FeedModel(i));
                return FeedsCollection;
            }
            else
            {
                page--;
                return null;
            }
        }

        internal override string GetTitleBarText(IDetail detail) => (detail as DyhDetail).Title;
    }
}