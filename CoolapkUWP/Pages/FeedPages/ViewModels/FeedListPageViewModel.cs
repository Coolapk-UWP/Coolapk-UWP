using CoolapkUWP.Controls.ViewModels;
using CoolapkUWP.Helpers;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;

namespace CoolapkUWP.Pages.FeedPages.ViewModels
{
    enum FeedListType
    {
        UserPageList,
        TagPageList,
        DyhPageList
    }

    interface ICanChangeSelectedIndex
    {
        int SelectedIndex { get; set; }
        void Reset();
    }
    abstract class FeedListDataProvider
    {
        public static FeedListDataProvider GetProvider(FeedListType type, string id)
        {
            if (string.IsNullOrEmpty(id) || id == "0") return null;
            switch (type)
            {
                case FeedListType.UserPageList:
                    return new UserPageDataProvider(id);
                case FeedListType.TagPageList:
                    return new TagPageDataProvider(id);
                case FeedListType.DyhPageList:
                    return new DyhPageDataProvider(id);
                default:
                    return null;
            }
        }
        public abstract string Id { get; internal set; }
        public abstract FeedListType ListType { get; }
        public readonly ObservableCollection<object> itemCollection = new ObservableCollection<object>();

        public abstract Task<object> GetDetail();
        public abstract Task<List<FeedViewModel>> GetFeeds(int p = -1);
        public abstract string GetTitleBarText();

        public async Task Refresh()
        {
            if (itemCollection.Count > 0) itemCollection.RemoveAt(0);
            itemCollection.Insert(0, await GetDetail());
            List<FeedViewModel> feeds = await GetFeeds(1);
            if (feeds != null)
                for (int i = 0; i < feeds.Count; i++)
                    itemCollection.Insert(i + 1, feeds[i]);
        }
        public async Task LoadNextPage()
        {
            List<FeedViewModel> feeds = await GetFeeds();
            if (feeds != null)
                foreach (var item in feeds)
                    itemCollection.Add(item);
        }

        internal string jsonString = string.Empty;
        public void ObjectToJsonString()
        {
            //JArray array = new JArray();
            //foreach (var item in itemCollection)
            //{
            //    array.Add(JObject.FromObject(item));
            //}
            //jsonString = array.ToString();
            //itemCollection.Clear();
        }

        /// <summary>
        /// 将 <c>JsonString</c> 转换为 <c>Entity</c> 。
        /// </summary>
        /// <returns>JsonString是否为空。</returns>
        public abstract bool JsonStringToObject();
    }
    class UserPageDataProvider : FeedListDataProvider
    {
        int page;
        int firstItem, lastItem;
        public override string Id { get; internal set; }
        public override FeedListType ListType { get => FeedListType.UserPageList; }
        public UserPageDataProvider(string uid) => Id = uid;

        public override async Task<object> GetDetail()
        {
            JObject detail = (JObject)await DataHelper.GetData(DataUriType.GetUserSpace, Id);
            if (detail != null)
            {
                return new UserDetail
                {
                    FollowStatus = detail.Value<int>("uid").ToString() == SettingsHelper.Get<string>("uid") ? string.Empty : detail.Value<int>("isFollow") == 0 ? "关注" : "取消关注",
                    FollowNum = detail.Value<int>("follow"),
                    FansNum = detail.Value<int>("fans"),
                    Level = detail.Value<int>("level"),
                    Gender = detail.Value<int>("gender") == 1 ? "♂" : (detail.Value<int>("gender") == 0 ? "♀" : string.Empty),
                    Logintime = $"{DataHelper.ConvertTime(detail.Value<int>("logintime"))}活跃",
                    FeedNum = detail.Value<int>("feed"),
                    UserFaceUrl = detail.Value<string>("userAvatar"),
                    UserName = detail.Value<string>("username"),
                    Bio = detail.Value<string>("bio"),
                    BackgroundUrl = detail.Value<string>("cover"),
                    Verify_title = detail.Value<string>("verify_title"),
                    City = $"{detail.Value<string>("province")} {detail.Value<string>("city")}",
                    Astro = detail.Value<string>("astro"),
                    UserFace = await ImageCacheHelper.GetImage(ImageType.SmallAvatar, detail.Value<string>("userSmallAvatar")),
                    Background = new ImageBrush { ImageSource = await ImageCacheHelper.GetImage(ImageType.OriginImage, detail.Value<string>("cover")), Stretch = Stretch.UniformToFill }
                };
            }
            else return new UserDetail();
        }

        public override async Task<List<FeedViewModel>> GetFeeds(int p = -1)
        {
            if (p == 1 && page == 0) page = 1;
            JArray array = (JArray)await DataHelper.GetData(DataUriType.GetUserFeeds,
                                                            Id,
                                                            p == -1 ? ++page : p,
                                                            firstItem == 0 ? string.Empty : $"&firstItem={firstItem}",
                                                            lastItem == 0 ? string.Empty : $"&lastItem={lastItem}");
            if (!(array is null) && array.Count != 0)
            {
                if (page == 1 || p == 1)
                    firstItem = array.First.Value<int>("id");
                lastItem = array.Last.Value<int>("id");
                List<FeedViewModel> FeedsCollection = new List<FeedViewModel>();
                foreach (JObject i in array) FeedsCollection.Add(new FeedViewModel(i));
                return FeedsCollection;
            }
            else
            {
                page--;
                return null;
            }
        }

        public override string GetTitleBarText() => (itemCollection[0] as UserDetail).UserName;

        public override bool JsonStringToObject()
        {
            bool r = string.IsNullOrEmpty(jsonString);
            if (!r)
            {
                int i = 0;
                foreach (JObject item in JArray.Parse(jsonString))
                {
                    if (i == 0)
                        itemCollection.Add(item.ToObject<UserDetail>());
                    else
                        itemCollection.Add(item.ToObject<FeedViewModel>());
                    i++;
                }
            }
            return r;
        }
    }
    class TagPageDataProvider : FeedListDataProvider, ICanChangeSelectedIndex
    {
        int page, _selectedIndex;
        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                if (value > -1)
                    _selectedIndex = value;
            }
        }
        double firstItem, lastItem;
        public override string Id { get; internal set; }
        public override FeedListType ListType { get => FeedListType.TagPageList; }
        public TagPageDataProvider(string tag) => Id = tag;

        public void Reset() => firstItem = lastItem = page = 0;

        public override async Task<object> GetDetail()
        {
            JObject detail = (JObject)await DataHelper.GetData(DataUriType.GetTagDetail, Id);
            if (detail != null)
            {
                return new TopicDetail
                {
                    Logo = await ImageCacheHelper.GetImage(ImageType.Icon, detail.Value<string>("logo")),
                    Title = detail.Value<string>("title"),
                    Description = detail.Value<string>("description"),
                    FollowNum = detail.TryGetValue("follownum", out JToken t) ? int.Parse(t.ToString()) : detail.Value<int>("follow_num"),
                    CommentNum = detail.TryGetValue("commentnum", out JToken tt) ? int.Parse(tt.ToString()) : detail.Value<int>("rating_total_num"),
                    SelectedIndex = SelectedIndex
                };
            }
            else return new TopicDetail();
        }

        public override async Task<List<FeedViewModel>> GetFeeds(int p = -1)
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
            JArray array = (JArray)await DataHelper.GetData(DataUriType.GetTagFeeds,
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
                List<FeedViewModel> FeedsCollection = new List<FeedViewModel>();
                foreach (JObject i in array) FeedsCollection.Add(new FeedViewModel(i));
                return FeedsCollection;
            }
            else
            {
                page--;
                return null;
            }
        }

        public override string GetTitleBarText() => (itemCollection[0] as TopicDetail).Title;

        public override bool JsonStringToObject()
        {
            bool r = string.IsNullOrEmpty(jsonString);
            if (!r)
            {
                int i = 0;
                foreach (JObject item in JArray.Parse(jsonString))
                {
                    if (i == 0)
                        itemCollection.Add(item.ToObject<TopicDetail>());
                    else
                        itemCollection.Add(item.ToObject<FeedViewModel>());
                    i++;
                }
            }
            return r;
        }
    }
    class DyhPageDataProvider : FeedListDataProvider, ICanChangeSelectedIndex
    {
        public override string Id { get; internal set; }
        int page, _selectedIndex;
        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                if (value > -1)
                    _selectedIndex = value;
            }
        }
        double firstItem, lastItem;
        public override FeedListType ListType { get => FeedListType.DyhPageList; }
        public DyhPageDataProvider(string id) => Id = id;

        public void Reset() => firstItem = lastItem = page = 0;

        public override async Task<object> GetDetail()
        {
            JObject detail = (JObject)await DataHelper.GetData(DataUriType.GetDyhDetail, Id);
            if (detail != null)
            {
                bool showUserButton = detail.Value<int>("uid") != 0;
                return new DyhDetail
                {
                    FollowNum = detail.Value<int>("follownum"),
                    ShowComboBox = detail.Value<int>("is_open_discuss") == 1,
                    Logo = await ImageCacheHelper.GetImage(ImageType.Icon, detail.Value<string>("logo")),
                    Title = detail.Value<string>("title"),
                    Description = detail.Value<string>("description"),
                    ShowUserButton = showUserButton,
                    Url = showUserButton ? detail["userInfo"].Value<string>("url") : string.Empty,
                    UserName = showUserButton ? detail["userInfo"].Value<string>("username") : string.Empty,
                    UserAvatar = showUserButton ? await ImageCacheHelper.GetImage(ImageType.SmallAvatar, detail["userInfo"].Value<string>("userSmallAvatar").Replace("\"", string.Empty)) : null,
                    SelectedIndex = SelectedIndex,
                };
            }
            else return new DyhDetail();
        }

        public override async Task<List<FeedViewModel>> GetFeeds(int p = -1)
        {
            if (p == 1 && page == 0) page = 1;
            JArray array = (JArray)await DataHelper.GetData(DataUriType.GetDyhFeeds,
                                                            Id,
                                                            SelectedIndex == 0 ? "all" : "square",
                                                            p == -1 ? ++page : p,
                                                            firstItem == 0 ? string.Empty : $"&firstItem={firstItem}",
                                                            (lastItem == 0) ? string.Empty : $"&lastItem={lastItem}");
            if (!(array is null) && array.Count != 0)
            {
                if (page == 1 || p == 1)
                    firstItem = array.First.Value<int>("id");
                lastItem = array.Last.Value<int>("id");
                List<FeedViewModel> FeedsCollection = new List<FeedViewModel>();
                foreach (JObject i in array) FeedsCollection.Add(new FeedViewModel(i));
                return FeedsCollection;
            }
            else
            {
                page--;
                return null;
            }
        }

        public override string GetTitleBarText() => (itemCollection[0] as DyhDetail).Title;

        public override bool JsonStringToObject()
        {
            bool r = string.IsNullOrEmpty(jsonString);
            if (!r)
            {
                int i = 0;
                foreach (JObject item in JArray.Parse(jsonString))
                {
                    if (i == 0)
                        itemCollection.Add(item.ToObject<DyhDetail>());
                    else
                        itemCollection.Add(item.ToObject<FeedViewModel>());
                    i++;
                }
            }
            return r;
        }

    }
}
