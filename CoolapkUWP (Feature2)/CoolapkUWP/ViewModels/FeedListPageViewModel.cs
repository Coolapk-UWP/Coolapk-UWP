using CoolapkUWP.Control.ViewModels;
using CoolapkUWP.Data;
using CoolapkUWP.Models.Pages;
using CoolapkUWP.Pages.FeedPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using static CoolapkUWP.ViewModels.Interfaces;

namespace CoolapkUWP.ViewModels
{
    internal abstract class FeedListPageViewModelBase : IViewModel
    {
    }

    internal interface IFeedListDataProvider
    {
        string Id { get; }
        FeedListType ListType { get; }
        Task<object> GetDetail();
        Task<List<FeedViewModel>> GetFeeds(int p = -1);
        string GetTitleBarText(object o);
    }

    internal interface ICanChangeSelectedIndex : IFeedListDataProvider
    {
        int SelectedIndex { get; set; }
        void Reset();
    }

    internal class UserPageDataProvider : ICanChangeSelectedIndex
    {
        public string Id { get; private set; }

        private int page, _selectedIndex;
        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                if (value > -1)
                { _selectedIndex = value; }
            }
        }

        private double firstItem, lastItem;
        public FeedListType ListType { get => FeedListType.UserPageList; }
        public UserPageDataProvider(string uid) => Id = uid;

        public void Reset() => firstItem = lastItem = page = 0;

        public async Task<object> GetDetail()
        {
            JsonObject detail = UIHelper.GetJSonObject(await UIHelper.GetJson("/user/space?uid=" + Id));
            return detail != null
                ? new UserDetail
                {
                    FollowStatus = detail["uid"].GetNumber().ToString() == SettingsHelper.GetString("uid") ? string.Empty : detail["isFollow"].GetNumber() == 0 ? "关注" : "取消关注",
                    UserFaceUrl = detail["userAvatar"].GetString(),
                    UserName = detail["username"].GetString(),
                    FollowNum = detail["follow"].GetNumber(),
                    FansNum = detail["fans"].GetNumber(),
                    Level = detail["level"].GetNumber(),
                    Bio = detail["bio"].GetString(),
                    BackgroundUrl = detail["cover"].GetString(),
                    Verify_title = detail["verify_title"].GetString(),
                    Gender = detail["gender"].GetNumber() == 1 ? "♂" : (detail["gender"].GetNumber() == 0 ? "♀" : string.Empty),
                    City = $"{detail["province"].GetString()} {detail["city"].GetString()}",
                    Astro = detail["astro"].GetString(),
                    Logintime = $"{detail["logintime"].GetNumber().ConvertTime()}活跃",
                    FeedNum = detail["feed"].GetNumber(),
                    UserFace = await ImageCache.GetImage(ImageType.SmallAvatar, detail["userSmallAvatar"].GetString()),
                    Background = new ImageViewModel(detail["cover"].GetString(), ImageType.OriginImage),
                    SelectedIndex = SelectedIndex,
                }
                : null;
        }

        public async Task<List<FeedViewModel>> GetFeeds(int p = -1)
        {
            string sortType = "feed";
            switch (SelectedIndex)
            {
                case 0:
                    sortType = "feed";
                    break;
                case 1:
                    sortType = "htmlFeed";
                    break;
                case 2:
                    sortType = "questionAndAnswer";
                    break;
                default:
                    break;
            }
            if (p == 1 && page == 0) { page = 1; }
            JsonArray Root = UIHelper.GetDataArray(await UIHelper.GetJson($"/user/{sortType}List?uid={Id}&page={(p == -1 ? ++page : p)}{(firstItem == 0 ? string.Empty : $"&firstItem={firstItem}")}{(lastItem == 0 ? string.Empty : $"&lastItem={lastItem}")}"));
            if (!(Root is null) && Root.Count != 0)
            {
                if (page == 1 || p == 1)
                { firstItem = Root.First()?.GetObject()["id"].GetNumber() ?? firstItem; }
                lastItem = Root.Last().GetObject()["id"].GetNumber();
                List<FeedViewModel> FeedsCollection = new List<FeedViewModel>();
                foreach (IJsonValue i in Root) { FeedsCollection.Add(new FeedViewModel(i)); }
                return FeedsCollection;
            }
            else
            {
                page--;
                return null;
            }
        }

        public string GetTitleBarText(object o) => (o as UserDetail).UserName;
    }

    internal class TagPageDataProvider : ICanChangeSelectedIndex
    {
        public string Id { get; private set; }

        private int page, _selectedIndex;
        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                if (value > -1)
                { _selectedIndex = value; }
            }
        }

        private double firstItem, lastItem;
        public FeedListType ListType { get => FeedListType.TagPageList; }
        public TagPageDataProvider(string tag) => Id = tag;

        public void Reset() => firstItem = lastItem = page = 0;

        public async Task<object> GetDetail()
        {
            JsonObject detail = UIHelper.GetJSonObject(await UIHelper.GetJson("/topic/newTagDetail?tag=" + Id));
            return detail != null
                ? new TopicDetail
                {
                    Logo = await ImageCache.GetImage(ImageType.Icon, detail["logo"].GetString()),
                    Title = detail["title"].GetString(),
                    FollowNum = detail.TryGetValue("follownum", out IJsonValue t) ? t.GetNumber() : detail["follow_num"].GetNumber(),
                    CommentNum = detail.TryGetValue("commentnum", out IJsonValue tt) ? tt.GetNumber() : detail["rating_total_num"].GetNumber(),
                    Description = detail["description"].GetString(),
                    SelectedIndex = SelectedIndex
                }
                : null;
        }

        public async Task<List<FeedViewModel>> GetFeeds(int p = -1)
        {
            string sortType = "lastupdate_desc";
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
                default:
                    break;
            }
            if (p == 1 && page == 0) { page = 1; }
            JsonArray Root = UIHelper.GetDataArray(await UIHelper.GetJson($"/topic/tagFeedList?tag={Id}&page={(p == -1 ? ++page : p)}{(firstItem == 0 ? string.Empty : $"&firstItem={firstItem}")}{(lastItem == 0 ? string.Empty : $"&lastItem={lastItem}")}&listType={sortType}&blockStatus=0"));
            if (!(Root is null) && Root.Count != 0)
            {
                if (page == 1 || p == 1)
                { firstItem = Root.First()?.GetObject()["id"].GetNumber() ?? firstItem; }
                lastItem = Root.Last()?.GetObject()["id"].GetNumber() ?? lastItem;
                List<FeedViewModel> FeedsCollection = new List<FeedViewModel>();
                foreach (IJsonValue i in Root) { FeedsCollection.Add(new FeedViewModel(i)); }
                return FeedsCollection;
            }
            else
            {
                page--;
                return null;
            }
        }

        public string GetTitleBarText(object o) => (o as TopicDetail).Title;
    }

    internal class DYHPageDataProvider : ICanChangeSelectedIndex
    {
        public string Id { get; private set; }

        private int page, _selectedIndex;
        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                if (value > -1)
                { _selectedIndex = value; }
            }
        }

        private double firstItem, lastItem;
        public FeedListType ListType { get => FeedListType.DYHPageList; }
        public DYHPageDataProvider(string id) => Id = id;

        public void Reset() => firstItem = lastItem = page = 0;

        public async Task<object> GetDetail()
        {
            JsonObject detail = UIHelper.GetJSonObject(await UIHelper.GetJson("/dyh/detail?dyhId=" + Id));
            if (detail != null)
            {
                bool showUserButton = detail["uid"].GetNumber() != 0;
                return new DYHDetail
                {
                    Logo = await ImageCache.GetImage(ImageType.Icon, detail["logo"].GetString()),
                    Title = detail["title"].GetString(),
                    Description = detail["description"].GetString(),
                    FollowNum = detail["follownum"].GetNumber(),
                    ShowUserButton = showUserButton,
                    Url = showUserButton ? detail["userInfo"].GetObject()["url"].GetString() : string.Empty,
                    UserName = showUserButton ? detail["userInfo"].GetObject()["username"].GetString() : string.Empty,
                    UserAvatar = showUserButton ? await ImageCache.GetImage(ImageType.SmallAvatar, detail["userInfo"].GetObject()["userSmallAvatar"].ToString().Replace("\"", string.Empty)) : null,
                    SelectedIndex = SelectedIndex,
                    ShowComboBox = detail["is_open_discuss"].GetNumber() == 1
                };
            }
            else { return null; }
        }

        public async Task<List<FeedViewModel>> GetFeeds(int p = -1)
        {
            if (p == 1 && page == 0) { page = 1; }
            JsonArray Root = UIHelper.GetDataArray(await UIHelper.GetJson($"/dyhArticle/list?dyhId={Id}&type={(SelectedIndex == 0 ? "all" : "square")}&page={(p == -1 ? ++page : p)}{(firstItem == 0 ? string.Empty : $"&firstItem={firstItem}")}{((lastItem == 0) ? string.Empty : $"&lastItem={lastItem}")}"));
            if (!(Root is null) && Root.Count != 0)
            {
                if (page == 1 || p == 1)
                { firstItem = Root.First()?.GetObject()["id"].GetNumber() ?? firstItem; }
                lastItem = Root.Last()?.GetObject()["id"].GetNumber() ?? lastItem;
                List<FeedViewModel> FeedsCollection = new List<FeedViewModel>();
                foreach (IJsonValue i in Root) { FeedsCollection.Add(new FeedViewModel(i)); }
                return FeedsCollection;
            }
            else
            {
                page--;
                return null;
            }
        }

        public string GetTitleBarText(object o) => (o as DYHDetail).Title;
    }

    internal class ProductPageDataProvider : ICanChangeSelectedIndex
    {
        public string Id { get; private set; }

        private int page, _selectedIndex;
        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                if (value > -1)
                { _selectedIndex = value; }
            }
        }

        private double firstItem, lastItem;
        public FeedListType ListType { get => FeedListType.ProductPageList; }
        public ProductPageDataProvider(string id) => Id = id;

        public void Reset() => firstItem = lastItem = page = 0;

        public async Task<object> GetDetail()
        {
            JsonObject detail = UIHelper.GetJSonObject(await UIHelper.GetJson("/product/detail?id=" + Id));
            return detail != null
                ? new ProductDetail
                {
                    Title = detail["title"].GetString(),
                    FollowNum = detail["follow_num"].GetNumber(),
                    CommentNum = detail["hot_num_txt"].GetValue(),
                    Description = detail["description"].GetString(),
                    Logo = await ImageCache.GetImage(ImageType.Icon, detail["logo"].GetString()),
                    SelectedIndex = SelectedIndex
                }
                : null;
        }

        public async Task<List<FeedViewModel>> GetFeeds(int p = -1)
        {
            string sortType = "feed";
            switch (SelectedIndex)
            {
                case 0:
                    sortType = "feed";
                    break;
                case 1:
                    sortType = "answer";
                    break;
                case 2:
                    sortType = "article";
                    break;
                default:
                    break;
            }
            if (p == 1 && page == 0) { page = 1; }
            JsonArray Root = UIHelper.GetDataArray(await UIHelper.GetJson($"/page/dataList?url=/page?url=/product/feedList?type={sortType}&id={Id}&page={(p == -1 ? ++page : p)}{(firstItem == 0 ? string.Empty : $"&firstItem={firstItem}")}{((lastItem == 0) ? string.Empty : $"&lastItem={lastItem}")}"));
            if (!(Root is null) && Root.Count != 0)
            {
                if (page == 1 || p == 1)
                { firstItem = Root.First()?.GetObject()["id"].GetNumber() ?? firstItem; }
                lastItem = Root.Last()?.GetObject()["id"].GetNumber() ?? lastItem;
                List<FeedViewModel> FeedsCollection = new List<FeedViewModel>();
                foreach (IJsonValue i in Root) { FeedsCollection.Add(new FeedViewModel(i)); }
                return FeedsCollection;
            }
            else
            {
                page--;
                return null;
            }
        }

        public string GetTitleBarText(object o) => (o as ProductDetail).Title;
    }

    internal class APPPageDataProvider : ICanChangeSelectedIndex
    {
        public string Id { get; private set; }

        private int page, _selectedIndex;
        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                if (value > -1)
                { _selectedIndex = value; }
            }
        }

        private double firstItem, lastItem;
        public FeedListType ListType { get => FeedListType.APPPageList; }
        public APPPageDataProvider(string id) => Id = id;

        public void Reset() => firstItem = lastItem = page = 0;

        public async Task<object> GetDetail()
        {
            JsonObject detail = UIHelper.GetJSonObject(await UIHelper.GetJson($"/apk/detail?id={Id}&installed=0"));
            return detail != null
                ? new APPDetail
                {
                    Title = detail["title"].GetString(),
                    FollowNum = detail["follownum"].GetNumber(),
                    CommentNum = detail["commentnum"].GetValue(),
                    Description = detail["description"].GetString(),
                    Logo = await ImageCache.GetImage(ImageType.Icon, detail["logo"].GetString())
                }
                : null;
        }

        public async Task<List<FeedViewModel>> GetFeeds(int p = -1)
        {
            string sortType = "lastupdate_desc";
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
                default:
                    break;
            }
            if (p == 1 && page == 0) { page = 1; }
            JsonArray Root = UIHelper.GetDataArray(await UIHelper.GetJson($"/page/dataList?url=%23/feed/apkCommentList?isIncludeTop=1&id={Id}&sort={sortType}&page={(p == -1 ? ++page : p)}{(firstItem == 0 ? string.Empty : $"&firstItem={firstItem}")}{((lastItem == 0) ? string.Empty : $"&lastItem={lastItem}")}"));
            if (!(Root is null) && Root.Count != 0)
            {
                if (page == 1 || p == 1)
                { firstItem = Root.First()?.GetObject()["id"].GetNumber() ?? firstItem; }
                lastItem = Root.Last()?.GetObject()["id"].GetNumber() ?? lastItem;
                List<FeedViewModel> FeedsCollection = new List<FeedViewModel>();
                foreach (IJsonValue i in Root) { FeedsCollection.Add(new FeedViewModel(i)); }
                return FeedsCollection;
            }
            else
            {
                page--;
                return null;
            }
        }

        public string GetTitleBarText(object o) => (o as APPDetail).Title;
    }
}
