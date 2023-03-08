using CoolapkUWP.BackgroundTasks;
using CoolapkUWP.Controls;
using CoolapkUWP.Controls.DataTemplates;
using CoolapkUWP.Helpers;
using CoolapkUWP.Models;
using CoolapkUWP.Models.Pages;
using CoolapkUWP.Pages.FeedPages;
using CoolapkUWP.ViewModels.DataSource;
using CoolapkUWP.ViewModels.Providers;
using Microsoft.Toolkit.Uwp.Notifications;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.UI.Notifications;
using Windows.UI.StartScreen;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using TileSize = Windows.UI.StartScreen.TileSize;

namespace CoolapkUWP.ViewModels.FeedPages
{
    public abstract class FeedListViewModel : IViewModel
    {
        protected const string idName = "id";

        public string ID { get; }
        public FeedListType ListType { get; }
        public DataTemplateSelector DataTemplateSelector;

        private string title;
        public string Title
        {
            get => title;
            protected set
            {
                title = value;
                RaisePropertyChangedEvent();
            }
        }

        private List<ShyHeaderItem> itemSource;
        public List<ShyHeaderItem> ItemSource
        {
            get => itemSource;
            protected set
            {
                itemSource = value;
                RaisePropertyChangedEvent();
            }
        }
        private FeedListDetailBase detail;
        public FeedListDetailBase Detail
        {
            get => detail;
            protected set
            {
                detail = value;
                RaisePropertyChangedEvent();
                Title = GetTitleBarText(value);
                DetailDataTemplate = DataTemplateSelector.SelectTemplate(value);
            }
        }

        private DataTemplate detailDataTemplate;
        public DataTemplate DetailDataTemplate
        {
            get => detailDataTemplate;
            protected set
            {
                detailDataTemplate = value;
                RaisePropertyChangedEvent();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        protected FeedListViewModel(string id, FeedListType type)
        {
            ID = string.IsNullOrEmpty(id)
                ? throw new ArgumentException(nameof(id))
                : id;
            ListType = type;
        }

        public static FeedListViewModel GetProvider(FeedListType type, string id)
        {
            if (string.IsNullOrEmpty(id) || id == "0") { return null; }
            switch (type)
            {
                case FeedListType.UserPageList: return new UserViewModel(id);
                case FeedListType.TagPageList: return new TagViewModel(id);
                case FeedListType.DyhPageList: return new DyhViewModel(id);
                case FeedListType.ProductPageList: return new ProductViewModel(id);
                default: return null;
            }
        }

        public void ChangeCopyMode(bool mode)
        {
            if (Detail != null)
            {
                Detail.IsCopyEnabled = mode;
            }
        }

        public abstract Task<bool> PinSecondaryTile(Entity entity);

        public abstract Task<FeedListDetailBase> GetDetail();

        public abstract Task Refresh(bool reset = false);

        protected abstract string GetTitleBarText(FeedListDetailBase detail);

        private IEnumerable<Entity> GetEntities(JObject jo)
        {
            yield return EntityTemplateSelector.GetEntity(jo);
        }

        public class UserViewModel : FeedListViewModel
        {
            public FeedListItemSourse FeedItemSourse { get; private set; }
            public FeedListItemSourse HtmlFeedItemSourse { get; private set; }
            public FeedListItemSourse QAItemSourse { get; private set; }

            internal UserViewModel(string uid) : base(uid, FeedListType.UserPageList) { }

            public override async Task Refresh(bool reset = false)
            {
                if (Detail == null || reset)
                {
                    Detail = await GetDetail();
                }
                if (ItemSource == null)
                {
                    List<ShyHeaderItem> ItemSource = new List<ShyHeaderItem>();
                    if (FeedItemSourse == null || FeedItemSourse.ID != ID)
                    {
                        CoolapkListProvider Provider = new CoolapkListProvider(
                            (p, firstItem, lastItem) => UriHelper.GetUri(UriType.GetUserFeeds, ID, p, string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}", string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}", "feed"),
                            GetEntities,
                            idName);
                        FeedItemSourse = new FeedListItemSourse(ID, Provider);
                        ItemSource.Add(new ShyHeaderItem
                        {
                            Header = "动态",
                            ItemSource = FeedItemSourse
                        });
                    }
                    if (HtmlFeedItemSourse == null || HtmlFeedItemSourse.ID != ID)
                    {
                        CoolapkListProvider Provider = new CoolapkListProvider(
                            (p, firstItem, lastItem) => UriHelper.GetUri(UriType.GetUserFeeds, ID, p, string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}", string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}", "htmlFeed"),
                            GetEntities,
                            idName);
                        HtmlFeedItemSourse = new FeedListItemSourse(ID, Provider);
                        ItemSource.Add(new ShyHeaderItem
                        {
                            Header = "图文",
                            ItemSource = HtmlFeedItemSourse
                        });
                    }
                    if (QAItemSourse == null || QAItemSourse.ID != ID)
                    {
                        CoolapkListProvider Provider = new CoolapkListProvider(
                            (p, firstItem, lastItem) => UriHelper.GetUri(UriType.GetUserFeeds, ID, p, string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}", string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}", "questionAndAnswer"),
                            GetEntities,
                            idName);
                        QAItemSourse = new FeedListItemSourse(ID, Provider);
                        ItemSource.Add(new ShyHeaderItem
                        {
                            Header = "问答",
                            ItemSource = QAItemSourse
                        });
                    }
                    base.ItemSource = ItemSource;
                }
            }

            protected override string GetTitleBarText(FeedListDetailBase detail) => (detail as UserDetail)?.UserName;

            public override async Task<FeedListDetailBase> GetDetail()
            {
                (bool isSucceed, JToken result) = await RequestHelper.GetDataAsync(UriHelper.GetUri(UriType.GetUserSpace, ID), true);
                if (!isSucceed) { return null; }

                JObject token = (JObject)result;
                FeedListDetailBase detail = null;

                if (token != null)
                {
                    detail = new UserDetail(token);
                }

                return detail;
            }

            public override async Task<bool> PinSecondaryTile(Entity entity)
            {
                UserDetail user = (UserDetail)entity;

                ResourceLoader loader = ResourceLoader.GetForViewIndependentUse();

                // Construct a unique tile ID, which you will need to use later for updating the tile
                string tileId = user.Url.GetMD5();

                bool isPinned = SecondaryTile.Exists(tileId);
                if (isPinned)
                {
                    UIHelper.ShowMessage(loader.GetString("AlreadyPinnedTile"));
                }
                else
                {
                    // Use a display name you like
                    string displayName = user.UserName;

                    // Provide all the required info in arguments so that when user
                    // clicks your tile, you can navigate them to the correct content
                    string arguments = user.Url;

                    // Initialize the tile with required arguments
                    SecondaryTile tile = new SecondaryTile(
                        tileId,
                        displayName,
                        arguments,
                        new Uri("ms-appx:///Assets/Square150x150Logo.png"),
                        TileSize.Default);

                    // Enable wide and large tile sizes
                    tile.VisualElements.Wide310x150Logo = new Uri("ms-appx:///Assets/Wide310x150Logo.png");
                    tile.VisualElements.Square310x310Logo = new Uri("ms-appx:///Assets/LargeTile.png");

                    // Add a small size logo for better looking small tile
                    tile.VisualElements.Square71x71Logo = new Uri("ms-appx:///Assets/SmallTile.png");

                    // Add a unique corner logo for the secondary tile
                    tile.VisualElements.Square44x44Logo = new Uri("ms-appx:///Assets/Square44x44Logo.png");

                    // Show the display name on all sizes
                    tile.VisualElements.ShowNameOnSquare150x150Logo = true;
                    tile.VisualElements.ShowNameOnWide310x150Logo = true;
                    tile.VisualElements.ShowNameOnSquare310x310Logo = true;

                    // Pin the tile
                    isPinned = await tile.RequestCreateAsync();

                    if (isPinned) { UIHelper.ShowMessage(loader.GetString("PinnedTileSucceeded")); }
                }

                if (isPinned)
                {
                    try
                    {
                        TileUpdater tileUpdater = TileUpdateManager.CreateTileUpdaterForSecondaryTile(tileId);
                        tileUpdater.Clear();
                        tileUpdater.EnableNotificationQueue(true);
                        TileContent tileContent = LiveTileTask.GetUserTitle(user);
                        TileNotification tileNotification = new TileNotification(tileContent.GetXml());
                        tileUpdater.Update(tileNotification);
                    }
                    catch (Exception ex)
                    {
                        SettingsHelper.LogManager.GetLogger(nameof(FeedShellDetailControl)).Error(ex.ExceptionToMessage(), ex);
                    }

                    return isPinned;
                }

                UIHelper.ShowMessage(loader.GetString("PinnedTileFailed"));
                return isPinned;
            }

        }

        internal class TagViewModel : FeedListViewModel
        {
            public FeedListItemSourse LastupdateItemSourse { get; private set; }
            public FeedListItemSourse DatelineItemSourse { get; private set; }
            public FeedListItemSourse PopularItemSourse { get; private set; }

            internal TagViewModel(string id) : base(id, FeedListType.TagPageList) { }

            public override async Task Refresh(bool reset = false)
            {
                if (Detail == null || reset)
                {
                    Detail = await GetDetail();
                }
                if (ItemSource == null)
                {
                    List<ShyHeaderItem> ItemSource = new List<ShyHeaderItem>();
                    if (LastupdateItemSourse == null || LastupdateItemSourse.ID != ID)
                    {
                        CoolapkListProvider Provider = new CoolapkListProvider(
                            (p, firstItem, lastItem) => UriHelper.GetUri(UriType.GetTagFeeds, ID, p, string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}", string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}", "lastupdate_desc"),
                            GetEntities,
                            idName);
                        LastupdateItemSourse = new FeedListItemSourse(ID, Provider);
                        ItemSource.Add(new ShyHeaderItem
                        {
                            Header = "最近回复",
                            ItemSource = LastupdateItemSourse
                        });
                    }
                    if (DatelineItemSourse == null || DatelineItemSourse.ID != ID)
                    {
                        CoolapkListProvider Provider = new CoolapkListProvider(
                            (p, firstItem, lastItem) => UriHelper.GetUri(UriType.GetTagFeeds, ID, p, string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}", string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}", "dateline_desc"),
                            GetEntities,
                            idName);
                        DatelineItemSourse = new FeedListItemSourse(ID, Provider);
                        ItemSource.Add(new ShyHeaderItem
                        {
                            Header = "最近发布",
                            ItemSource = DatelineItemSourse
                        });
                    }
                    if (PopularItemSourse == null || PopularItemSourse.ID != ID)
                    {
                        CoolapkListProvider Provider = new CoolapkListProvider(
                            (p, firstItem, lastItem) => UriHelper.GetUri(UriType.GetTagFeeds, ID, p, string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}", string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}", "popular"),
                            GetEntities,
                            idName);
                        PopularItemSourse = new FeedListItemSourse(ID, Provider);
                        ItemSource.Add(new ShyHeaderItem
                        {
                            Header = "热门动态",
                            ItemSource = PopularItemSourse
                        });
                    }
                    base.ItemSource = ItemSource;
                }
            }

            protected override string GetTitleBarText(FeedListDetailBase detail) => (detail as TopicDetail)?.Title;

            public override async Task<FeedListDetailBase> GetDetail()
            {
                (bool isSucceed, JToken result) = await RequestHelper.GetDataAsync(UriHelper.GetUri(UriType.GetTagDetail, ID), true);
                if (!isSucceed) { return null; }

                JObject token = (JObject)result;
                FeedListDetailBase detail = null;

                if (token != null)
                {
                    detail = new TopicDetail(token);
                }

                return detail;
            }

            public override Task<bool> PinSecondaryTile(Entity entity) => Task.Run(() => false);
        }

        internal class DyhViewModel : FeedListViewModel
        {
            public FeedListItemSourse AllItemSourse { get; private set; }
            public FeedListItemSourse SquareItemSourse { get; private set; }

            internal DyhViewModel(string id) : base(id, FeedListType.DyhPageList) { }

            public override async Task Refresh(bool reset = false)
            {
                if (Detail == null || reset)
                {
                    Detail = await GetDetail();
                }
                if (ItemSource == null)
                {
                    List<ShyHeaderItem> ItemSource = new List<ShyHeaderItem>();
                    if (AllItemSourse == null || AllItemSourse.ID != ID)
                    {
                        CoolapkListProvider Provider = new CoolapkListProvider(
                            (p, firstItem, lastItem) => UriHelper.GetUri(UriType.GetDyhFeeds, ID, "all", p, string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}", string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}"),
                            GetEntities,
                            idName);
                        AllItemSourse = new FeedListItemSourse(ID, Provider);
                        ItemSource.Add(new ShyHeaderItem
                        {
                            Header = "精选",
                            ItemSource = AllItemSourse
                        });
                    }
                    if (SquareItemSourse == null || SquareItemSourse.ID != ID)
                    {
                        CoolapkListProvider Provider = new CoolapkListProvider(
                            (p, firstItem, lastItem) => UriHelper.GetUri(UriType.GetTagFeeds, ID, "square", p, string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}", string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}"),
                            GetEntities,
                            idName);
                        SquareItemSourse = new FeedListItemSourse(ID, Provider);
                        ItemSource.Add(new ShyHeaderItem
                        {
                            Header = "广场",
                            ItemSource = SquareItemSourse
                        });
                    }
                    base.ItemSource = ItemSource;
                }
            }

            protected override string GetTitleBarText(FeedListDetailBase detail) => (detail as DyhDetail)?.Title;

            public override async Task<FeedListDetailBase> GetDetail()
            {
                (bool isSucceed, JToken result) = await RequestHelper.GetDataAsync(UriHelper.GetUri(UriType.GetDyhDetail, ID), true);
                if (!isSucceed) { return null; }

                JObject token = (JObject)result;
                FeedListDetailBase detail = null;

                if (token != null)
                {
                    detail = new DyhDetail(token);
                }

                return detail;
            }

            public override Task<bool> PinSecondaryTile(Entity entity) => Task.Run(() => false);
        }

        internal class ProductViewModel : FeedListViewModel
        {
            public FeedListItemSourse FeedItemSourse { get; private set; }
            public FeedListItemSourse AnswerItemSourse { get; private set; }
            public FeedListItemSourse ArticleItemSourse { get; private set; }
            public FeedListItemSourse VideoItemSourse { get; private set; }
            public FeedListItemSourse TradeItemSourse { get; private set; }

            internal ProductViewModel(string id) : base(id, FeedListType.ProductPageList) { }

            public override async Task Refresh(bool reset = false)
            {
                if (Detail == null || reset)
                {
                    Detail = await GetDetail();
                }
                if (ItemSource == null)
                {
                    List<ShyHeaderItem> ItemSource = new List<ShyHeaderItem>();
                    if (FeedItemSourse == null || FeedItemSourse.ID != ID)
                    {
                        CoolapkListProvider Provider = new CoolapkListProvider(
                            (p, firstItem, lastItem) => UriHelper.GetUri(UriType.GetProductFeeds, ID, p, string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}", string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}", "feed"),
                            GetEntities,
                            idName);
                        FeedItemSourse = new FeedListItemSourse(ID, Provider);
                        ItemSource.Add(new ShyHeaderItem
                        {
                            Header = "讨论",
                            ItemSource = FeedItemSourse
                        });
                    }
                    if (AnswerItemSourse == null || AnswerItemSourse.ID != ID)
                    {
                        CoolapkListProvider Provider = new CoolapkListProvider(
                            (p, firstItem, lastItem) => UriHelper.GetUri(UriType.GetProductFeeds, ID, p, string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}", string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}", "answer"),
                            GetEntities,
                            idName);
                        AnswerItemSourse = new FeedListItemSourse(ID, Provider);
                        ItemSource.Add(new ShyHeaderItem
                        {
                            Header = "问答",
                            ItemSource = AnswerItemSourse
                        });
                    }
                    if (ArticleItemSourse == null || ArticleItemSourse.ID != ID)
                    {
                        CoolapkListProvider Provider = new CoolapkListProvider(
                            (p, firstItem, lastItem) => UriHelper.GetUri(UriType.GetProductFeeds, ID, p, string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}", string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}", "article"),
                            GetEntities,
                            idName);
                        ArticleItemSourse = new FeedListItemSourse(ID, Provider);
                        ItemSource.Add(new ShyHeaderItem
                        {
                            Header = "图文",
                            ItemSource = ArticleItemSourse
                        });
                    }
                    if (VideoItemSourse == null || VideoItemSourse.ID != ID)
                    {
                        CoolapkListProvider Provider = new CoolapkListProvider(
                            (p, firstItem, lastItem) => UriHelper.GetUri(UriType.GetProductFeeds, ID, p, string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}", string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}", "video"),
                            GetEntities,
                            idName);
                        VideoItemSourse = new FeedListItemSourse(ID, Provider);
                        ItemSource.Add(new ShyHeaderItem
                        {
                            Header = "视频",
                            ItemSource = VideoItemSourse
                        });
                    }
                    if (TradeItemSourse == null || TradeItemSourse.ID != ID)
                    {
                        CoolapkListProvider Provider = new CoolapkListProvider(
                            (p, firstItem, lastItem) => UriHelper.GetUri(UriType.GetProductFeeds, ID, p, string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}", string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}", "trade"),
                            GetEntities,
                            idName);
                        TradeItemSourse = new FeedListItemSourse(ID, Provider);
                        ItemSource.Add(new ShyHeaderItem
                        {
                            Header = "交易",
                            ItemSource = TradeItemSourse
                        });
                    }
                    base.ItemSource = ItemSource;
                }
            }

            protected override string GetTitleBarText(FeedListDetailBase detail) => (detail as ProductDetail)?.Title;

            public override async Task<FeedListDetailBase> GetDetail()
            {
                (bool isSucceed, JToken result) = await RequestHelper.GetDataAsync(UriHelper.GetUri(UriType.GetProductDetail, ID), true);
                if (!isSucceed) { return null; }

                JObject token = (JObject)result;
                FeedListDetailBase detail = null;

                if (token != null)
                {
                    detail = new ProductDetail(token);
                }

                return detail;
            }

            public override Task<bool> PinSecondaryTile(Entity entity) => Task.Run(() => false);
        }
    }

    public class FeedListItemSourse : EntityItemSourse
    {
        public string ID;

        public FeedListItemSourse(string id, CoolapkListProvider provider)
        {
            ID = id;
            Provider = provider;
        }

        protected override void AddItems(IList<Entity> items)
        {
            if (items != null)
            {
                foreach (Entity item in items)
                {
                    if (item is NullEntity) { continue; }
                    Add(item);
                }
            }
        }
    }
}
