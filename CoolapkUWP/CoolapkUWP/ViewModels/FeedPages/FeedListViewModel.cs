using CoolapkUWP.BackgroundTasks;
using CoolapkUWP.Controls;
using CoolapkUWP.Controls.DataTemplates;
using CoolapkUWP.Helpers;
using CoolapkUWP.Models;
using CoolapkUWP.Models.Images;
using CoolapkUWP.Models.Pages;
using CoolapkUWP.Pages.FeedPages;
using CoolapkUWP.ViewModels.DataSource;
using CoolapkUWP.ViewModels.Providers;
using Microsoft.Toolkit.Uwp.Notifications;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
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
        private FeedListType ListType { get; }
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
                case FeedListType.CollectionPageList: return new CollectionViewModel(id);
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

        public async void CopyPic(ImageModel image)
        {
            DataPackage dataPackage = await GetImageDataPackage(image, "复制图片");
            Clipboard.SetContentWithOptions(dataPackage, null);
        }

        public async void SharePic(ImageModel image)
        {
            DataPackage dataPackage = await GetImageDataPackage(image, "分享图片");
            if (dataPackage != null)
            {
                DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
                dataTransferManager.DataRequested += (sender, args) => { args.Request.Data = dataPackage; };
                DataTransferManager.ShowShareUI();
            }
        }

        public async void SavePic(ImageModel imageModel)
        {
            string url = imageModel.Uri;
            StorageFile image = await ImageCacheHelper.GetImageFileAsync(ImageType.OriginImage, url);
            if (image == null)
            {
                string str = ResourceLoader.GetForViewIndependentUse().GetString("ImageLoadError");
                UIHelper.ShowMessage(str);
                return;
            }

            string fileName = GetTitle(url);
            FileSavePicker fileSavePicker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.PicturesLibrary,
                SuggestedFileName = fileName.Replace(fileName.Substring(fileName.LastIndexOf('.')), string.Empty)
            };

            string fileex = fileName.Substring(fileName.LastIndexOf('.') + 1);
            int index = fileex.IndexOfAny(new char[] { '?', '%', '&' });
            fileex = fileex.Substring(0, index == -1 ? fileex.Length : index);
            fileSavePicker.FileTypeChoices.Add($"{fileex}文件", new string[] { "." + fileex });

            StorageFile file = await fileSavePicker.PickSaveFileAsync();
            if (file != null)
            {
                using (Stream FolderStream = await file.OpenStreamForWriteAsync())
                {
                    using (IRandomAccessStreamWithContentType RandomAccessStream = await image.OpenReadAsync())
                    {
                        using (Stream ImageStream = RandomAccessStream.AsStreamForRead())
                        {
                            await ImageStream.CopyToAsync(FolderStream);
                        }
                    }
                }
            }
        }

        public async Task<DataPackage> GetImageDataPackage(ImageModel image, string title)
        {
            StorageFile file = await ImageCacheHelper.GetImageFileAsync(ImageType.OriginImage, image.Uri);
            if (file == null) { return null; }
            RandomAccessStreamReference bitmap = RandomAccessStreamReference.CreateFromFile(file);

            DataPackage dataPackage = new DataPackage();
            dataPackage.SetBitmap(bitmap);
            dataPackage.Properties.Title = title;
            dataPackage.Properties.Description = GetTitle(image.Uri);

            return dataPackage;
        }

        public async Task GetImageDataPackage(DataPackage dataPackage, ImageModel image, string title)
        {
            StorageFile file = await ImageCacheHelper.GetImageFileAsync(ImageType.OriginImage, image.Uri);
            if (file == null)
            {
                string str = ResourceLoader.GetForViewIndependentUse().GetString("ImageLoadError");
                UIHelper.ShowMessage(str);
                return;
            }
            RandomAccessStreamReference bitmap = RandomAccessStreamReference.CreateFromFile(file);

            dataPackage.SetBitmap(bitmap);
            dataPackage.Properties.Title = title;
            dataPackage.Properties.Description = GetTitle(image.Uri);
            dataPackage.SetStorageItems(new IStorageItem[] { file });
        }

        private string GetTitle(string url)
        {
            Regex regex = new Regex(@"[^/]+(?!.*/)");
            return regex.IsMatch(url) ? regex.Match(url).Value : "图片";
        }

        public abstract Task<bool> PinSecondaryTile(Entity entity);

        public abstract Task<FeedListDetailBase> GetDetail();

        public abstract Task Refresh(bool reset = false);

        bool IViewModel.IsEqual(IViewModel other) => other is FeedListViewModel model && IsEqual(model);

        public bool IsEqual(FeedListViewModel other) => ListType == other.ListType && ID == other.ID;

        protected abstract string GetTitleBarText(FeedListDetailBase detail);

        private IEnumerable<Entity> GetEntities(JObject jo)
        {
            yield return EntityTemplateSelector.GetEntity(jo);
        }

        public class UserViewModel : FeedListViewModel
        {
            public FeedListItemSource FeedItemSource { get; private set; }
            public FeedListItemSource HtmlFeedItemSource { get; private set; }
            public FeedListItemSource QAItemSource { get; private set; }
            public FeedListItemSource CollectionItemSource { get; private set; }

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
                    if (FeedItemSource == null || FeedItemSource.ID != ID)
                    {
                        CoolapkListProvider Provider = new CoolapkListProvider(
                            (p, firstItem, lastItem) => UriHelper.GetUri(UriType.GetUserFeeds, ID, p, string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}", string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}", "feed"),
                            GetEntities,
                            idName);
                        FeedItemSource = new FeedListItemSource(ID, Provider);
                        ItemSource.Add(new ShyHeaderItem
                        {
                            Header = "动态",
                            ItemSource = FeedItemSource
                        });
                    }
                    if (HtmlFeedItemSource == null || HtmlFeedItemSource.ID != ID)
                    {
                        CoolapkListProvider Provider = new CoolapkListProvider(
                            (p, firstItem, lastItem) => UriHelper.GetUri(UriType.GetUserFeeds, ID, p, string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}", string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}", "htmlFeed"),
                            GetEntities,
                            idName);
                        HtmlFeedItemSource = new FeedListItemSource(ID, Provider);
                        ItemSource.Add(new ShyHeaderItem
                        {
                            Header = "图文",
                            ItemSource = HtmlFeedItemSource
                        });
                    }
                    if (QAItemSource == null || QAItemSource.ID != ID)
                    {
                        CoolapkListProvider Provider = new CoolapkListProvider(
                            (p, firstItem, lastItem) => UriHelper.GetUri(UriType.GetUserFeeds, ID, p, string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}", string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}", "questionAndAnswer"),
                            GetEntities,
                            idName);
                        QAItemSource = new FeedListItemSource(ID, Provider);
                        ItemSource.Add(new ShyHeaderItem
                        {
                            Header = "问答",
                            ItemSource = QAItemSource
                        });
                    }
                    if (CollectionItemSource == null || CollectionItemSource.ID != ID)
                    {
                        CoolapkListProvider Provider = new CoolapkListProvider(
                            (p, firstItem, lastItem) => UriHelper.GetUri(UriType.GetCollectionList, ID, p, string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}", string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}"),
                            GetEntities,
                            idName);
                        CollectionItemSource = new FeedListItemSource(ID, Provider);
                        ItemSource.Add(new ShyHeaderItem
                        {
                            Header = "收藏单",
                            ItemSource = CollectionItemSource
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
                IUserModel user = (IUserModel)entity;

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
            public FeedListItemSource LastUpdateItemSource { get; private set; }
            public FeedListItemSource DatelineItemSource { get; private set; }
            public FeedListItemSource PopularItemSource { get; private set; }

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
                    if (LastUpdateItemSource == null || LastUpdateItemSource.ID != ID)
                    {
                        CoolapkListProvider Provider = new CoolapkListProvider(
                            (p, firstItem, lastItem) => UriHelper.GetUri(UriType.GetTagFeeds, ID, p, string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}", string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}", "lastupdate_desc"),
                            GetEntities,
                            idName);
                        LastUpdateItemSource = new FeedListItemSource(ID, Provider);
                        ItemSource.Add(new ShyHeaderItem
                        {
                            Header = "最近回复",
                            ItemSource = LastUpdateItemSource
                        });
                    }
                    if (DatelineItemSource == null || DatelineItemSource.ID != ID)
                    {
                        CoolapkListProvider Provider = new CoolapkListProvider(
                            (p, firstItem, lastItem) => UriHelper.GetUri(UriType.GetTagFeeds, ID, p, string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}", string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}", "dateline_desc"),
                            GetEntities,
                            idName);
                        DatelineItemSource = new FeedListItemSource(ID, Provider);
                        ItemSource.Add(new ShyHeaderItem
                        {
                            Header = "最近发布",
                            ItemSource = DatelineItemSource
                        });
                    }
                    if (PopularItemSource == null || PopularItemSource.ID != ID)
                    {
                        CoolapkListProvider Provider = new CoolapkListProvider(
                            (p, firstItem, lastItem) => UriHelper.GetUri(UriType.GetTagFeeds, ID, p, string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}", string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}", "popular"),
                            GetEntities,
                            idName);
                        PopularItemSource = new FeedListItemSource(ID, Provider);
                        ItemSource.Add(new ShyHeaderItem
                        {
                            Header = "热门动态",
                            ItemSource = PopularItemSource
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

            public override async Task<bool> PinSecondaryTile(Entity entity)
            {
                IHasSubtitle detail = (IHasSubtitle)entity;

                ResourceLoader loader = ResourceLoader.GetForViewIndependentUse();

                // Construct a unique tile ID, which you will need to use later for updating the tile
                string tileId = detail.Url.GetMD5();

                bool isPinned = SecondaryTile.Exists(tileId);
                if (isPinned)
                {
                    UIHelper.ShowMessage(loader.GetString("AlreadyPinnedTile"));
                }
                else
                {
                    // Use a display name you like
                    string displayName = detail.Title;

                    // Provide all the required info in arguments so that when user
                    // clicks your tile, you can navigate them to the correct content
                    string arguments = detail.Url;

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
                        TileContent tileContent = LiveTileTask.GetListTitle(detail);
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

        internal class DyhViewModel : FeedListViewModel
        {
            public FeedListItemSource AllItemSource { get; private set; }
            public FeedListItemSource SquareItemSource { get; private set; }

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
                    if (AllItemSource == null || AllItemSource.ID != ID)
                    {
                        CoolapkListProvider Provider = new CoolapkListProvider(
                            (p, firstItem, lastItem) => UriHelper.GetUri(UriType.GetDyhFeeds, ID, "all", p, string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}", string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}"),
                            GetEntities,
                            idName);
                        AllItemSource = new FeedListItemSource(ID, Provider);
                        ItemSource.Add(new ShyHeaderItem
                        {
                            Header = "精选",
                            ItemSource = AllItemSource
                        });
                    }
                    if (SquareItemSource == null || SquareItemSource.ID != ID)
                    {
                        CoolapkListProvider Provider = new CoolapkListProvider(
                            (p, firstItem, lastItem) => UriHelper.GetUri(UriType.GetTagFeeds, ID, "square", p, string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}", string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}"),
                            GetEntities,
                            idName);
                        SquareItemSource = new FeedListItemSource(ID, Provider);
                        ItemSource.Add(new ShyHeaderItem
                        {
                            Header = "广场",
                            ItemSource = SquareItemSource
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

            public override async Task<bool> PinSecondaryTile(Entity entity)
            {
                IHasDescription detail = (IHasDescription)entity;

                ResourceLoader loader = ResourceLoader.GetForViewIndependentUse();

                // Construct a unique tile ID, which you will need to use later for updating the tile
                string tileId = detail.Url.GetMD5();

                bool isPinned = SecondaryTile.Exists(tileId);
                if (isPinned)
                {
                    UIHelper.ShowMessage(loader.GetString("AlreadyPinnedTile"));
                }
                else
                {
                    // Use a display name you like
                    string displayName = detail.Title;

                    // Provide all the required info in arguments so that when user
                    // clicks your tile, you can navigate them to the correct content
                    string arguments = detail.Url;

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
                        TileContent tileContent = LiveTileTask.GetListTitle(detail);
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

        internal class ProductViewModel : FeedListViewModel
        {
            public FeedListItemSource FeedItemSource { get; private set; }
            public FeedListItemSource AnswerItemSource { get; private set; }
            public FeedListItemSource ArticleItemSource { get; private set; }
            public FeedListItemSource VideoItemSource { get; private set; }
            public FeedListItemSource TradeItemSource { get; private set; }

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
                    if (FeedItemSource == null || FeedItemSource.ID != ID)
                    {
                        CoolapkListProvider Provider = new CoolapkListProvider(
                            (p, firstItem, lastItem) => UriHelper.GetUri(UriType.GetProductFeeds, ID, p, string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}", string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}", "feed"),
                            GetEntities,
                            idName);
                        FeedItemSource = new FeedListItemSource(ID, Provider);
                        ItemSource.Add(new ShyHeaderItem
                        {
                            Header = "讨论",
                            ItemSource = FeedItemSource
                        });
                    }
                    if (AnswerItemSource == null || AnswerItemSource.ID != ID)
                    {
                        CoolapkListProvider Provider = new CoolapkListProvider(
                            (p, firstItem, lastItem) => UriHelper.GetUri(UriType.GetProductFeeds, ID, p, string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}", string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}", "answer"),
                            GetEntities,
                            idName);
                        AnswerItemSource = new FeedListItemSource(ID, Provider);
                        ItemSource.Add(new ShyHeaderItem
                        {
                            Header = "问答",
                            ItemSource = AnswerItemSource
                        });
                    }
                    if (ArticleItemSource == null || ArticleItemSource.ID != ID)
                    {
                        CoolapkListProvider Provider = new CoolapkListProvider(
                            (p, firstItem, lastItem) => UriHelper.GetUri(UriType.GetProductFeeds, ID, p, string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}", string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}", "article"),
                            GetEntities,
                            idName);
                        ArticleItemSource = new FeedListItemSource(ID, Provider);
                        ItemSource.Add(new ShyHeaderItem
                        {
                            Header = "图文",
                            ItemSource = ArticleItemSource
                        });
                    }
                    if (VideoItemSource == null || VideoItemSource.ID != ID)
                    {
                        CoolapkListProvider Provider = new CoolapkListProvider(
                            (p, firstItem, lastItem) => UriHelper.GetUri(UriType.GetProductFeeds, ID, p, string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}", string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}", "video"),
                            GetEntities,
                            idName);
                        VideoItemSource = new FeedListItemSource(ID, Provider);
                        ItemSource.Add(new ShyHeaderItem
                        {
                            Header = "视频",
                            ItemSource = VideoItemSource
                        });
                    }
                    if (TradeItemSource == null || TradeItemSource.ID != ID)
                    {
                        CoolapkListProvider Provider = new CoolapkListProvider(
                            (p, firstItem, lastItem) => UriHelper.GetUri(UriType.GetProductFeeds, ID, p, string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}", string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}", "trade"),
                            GetEntities,
                            idName);
                        TradeItemSource = new FeedListItemSource(ID, Provider);
                        ItemSource.Add(new ShyHeaderItem
                        {
                            Header = "交易",
                            ItemSource = TradeItemSource
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

        internal class CollectionViewModel : FeedListViewModel
        {
            internal CollectionViewModel(string id) : base(id, FeedListType.CollectionPageList) { }

            public override async Task Refresh(bool reset = false)
            {
                if (Detail == null || reset)
                {
                    Detail = await GetDetail();
                }
                if (ItemSource == null)
                {
                    (bool isSucceed, JToken result) = await RequestHelper.GetDataAsync(UriHelper.GetUri(UriType.GetCollectionContents, ID, "1", ""), true);
                    if (isSucceed)
                    {
                        JArray array = (JArray)result;
                        foreach (JObject item in array)
                        {
                            if (item.TryGetValue("entityTemplate", out JToken entityTemplate) && entityTemplate.ToString() == "selectorLinkCard")
                            {
                                if (item.TryGetValue("entities", out JToken v1))
                                {
                                    JArray entities = (JArray)v1;
                                    List<ShyHeaderItem> ItemSource = new List<ShyHeaderItem>();
                                    foreach (JObject entity in entities)
                                    {
                                        if (entity.TryGetValue("url", out JToken url) && !string.IsNullOrEmpty(url.ToString()))
                                        {
                                            CoolapkListProvider Provider = new CoolapkListProvider(
                                                (p, firstItem, lastItem) => UriHelper.GetUri(UriType.DataList, url.ToString().Replace("#", "%23").Replace("/", "%2F").Replace("?", "%3F").Replace("=", "%3D").Replace("&", "%26"), $"&page={p}" + (string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}") + (string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}")),
                                                GetEntities,
                                                "id");
                                            FeedListItemSource FeedListItemSource = new FeedListItemSource(ID, Provider);
                                            ShyHeaderItem ShyHeaderItem = new ShyHeaderItem { ItemSource = FeedListItemSource };
                                            if (entity.TryGetValue("title", out JToken title) && !string.IsNullOrEmpty(title.ToString()))
                                            {
                                                ShyHeaderItem.Header = title.ToString();
                                            }
                                            ItemSource.Add(ShyHeaderItem);
                                        }
                                    }
                                    this.ItemSource = ItemSource;
                                    break;
                                }
                            }
                        }
                        if (ItemSource == null)
                        {
                            List<ShyHeaderItem> ItemSource = new List<ShyHeaderItem>();
                            CoolapkListProvider Provider = new CoolapkListProvider(
                                (p, firstItem, lastItem) => UriHelper.GetUri(UriType.GetCollectionContents, ID, p, string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}", string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}"),
                                GetEntities,
                                "id");
                            FeedListItemSource FeedListItemSource = new FeedListItemSource(ID, Provider);
                            ShyHeaderItem ShyHeaderItem = new ShyHeaderItem
                            {
                                ItemSource = FeedListItemSource,
                                Header = Detail is CollectionDetail CollectionDetail && CollectionDetail.ItemNum > 0 ? $"全部({CollectionDetail.ItemNum})" : (object)$"全部"
                            };
                            ItemSource.Add(ShyHeaderItem);
                            this.ItemSource = ItemSource;
                        }
                    }
                }
            }
            protected override string GetTitleBarText(FeedListDetailBase detail) => (detail as CollectionDetail)?.Title;

            public override async Task<FeedListDetailBase> GetDetail()
            {
                (bool isSucceed, JToken result) = await RequestHelper.GetDataAsync(UriHelper.GetUri(UriType.GetCollectionDetail, ID), true);
                if (!isSucceed) { return null; }

                JObject token = (JObject)result;
                FeedListDetailBase detail = null;

                if (token != null)
                {
                    detail = new CollectionDetail(token);
                }

                return detail;
            }

            public override Task<bool> PinSecondaryTile(Entity entity) => Task.Run(() => false);
        }
    }

    public class FeedListItemSource : EntityItemSource
    {
        public string ID;

        public FeedListItemSource(string id, CoolapkListProvider provider)
        {
            ID = id;
            Provider = provider;
        }

        protected override async Task AddItemsAsync(IList<Entity> items)
        {
            if (items != null)
            {
                foreach (Entity item in items)
                {
                    if (item is NullEntity) { continue; }
                    await AddAsync(item);
                }
            }
        }
    }
}
