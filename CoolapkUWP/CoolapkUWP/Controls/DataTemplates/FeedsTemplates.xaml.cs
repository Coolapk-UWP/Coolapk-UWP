using CoolapkUWP.BackgroundTasks;
using CoolapkUWP.Helpers;
using CoolapkUWP.Models;
using CoolapkUWP.Models.Feeds;
using CoolapkUWP.Pages.BrowserPages;
using CoolapkUWP.Pages.FeedPages;
using CoolapkUWP.ViewModels.BrowserPages;
using CoolapkUWP.ViewModels.FeedPages;
using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.Toolkit.Uwp.UI;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Resources;
using Windows.UI.Notifications;
using Windows.UI.StartScreen;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;
using TileSize = Windows.UI.StartScreen.TileSize;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace CoolapkUWP.Controls.DataTemplates
{
    public partial class FeedsTemplates : ResourceDictionary
    {
        public FeedsTemplates() => InitializeComponent();

        private void OnTapped(object sender, TappedRoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            if (e != null && !UIHelper.IsOriginSource(sender, e.OriginalSource)) { return; }
            if ((element.DataContext as ICanCopy)?.IsCopyEnabled ?? false) { return; }

            if (e != null) { e.Handled = true; }

            _ = UIHelper.OpenLinkAsync(element.Tag.ToString());
        }

        private void OnTopTapped(object sender, TappedRoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            if ((element.DataContext as ICanCopy)?.IsCopyEnabled ?? false) { return; }

            if (e != null) { e.Handled = true; }

            _ = UIHelper.OpenLinkAsync(element.Tag.ToString());
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            switch (element.Name)
            {
                default:
                    _ = UIHelper.OpenLinkAsync((sender as FrameworkElement).Tag.ToString());
                    break;
            }
        }

        private void ReplyButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement frameworkElement)
            {
                if ((frameworkElement.Tag as ICanCopy)?.IsCopyEnabled ?? false) { return; }
                UIHelper.Navigate(typeof(AdaptivePage), AdaptiveViewModel.GetReplyListProvider(((FeedReplyModel)frameworkElement.Tag).ID.ToString(), (FeedReplyModel)frameworkElement.Tag));
            }
        }

        private void FeedButton_Click(object sender, RoutedEventArgs e)
        {
            void DisabledCopy()
            {
                if ((sender as FrameworkElement).DataContext is ICanCopy i)
                {
                    i.IsCopyEnabled = false;
                }
            }

            FrameworkElement element = sender as FrameworkElement;
            switch (element.Name)
            {
                case "MakeReplyButton":
                    DisabledCopy();
                    break;

                case "PinTileButton":
                    DisabledCopy();
                    _ = PinSecondaryTile(element.Tag as FeedModelBase);
                    break;

                case "LikeButton":
                    DisabledCopy();
                    _ = (element.Tag as ICanLike).ChangeLike();
                    break;

                case "ReportButton":
                    DisabledCopy();
                    UIHelper.Navigate(typeof(BrowserPage), new BrowserViewModel(element.Tag.ToString()));
                    break;

                case "ReplyButton":
                    DisabledCopy();
                    if (element.Tag is FeedModelBase feed)
                    {
                        new CreateFeedControl
                        {
                            ReplyID = feed.ID,
                            FeedType = CreateFeedType.Reply,
                            PopupTransitions = new TransitionCollection
                            {
                                new EdgeUIThemeTransition
                                {
                                    Edge = EdgeTransitionLocation.Bottom
                                }
                            }
                        }.Show();
                    }
                    else if (element.Tag is FeedReplyModel reply)
                    {
                        new CreateFeedControl
                        {
                            ReplyID = reply.ID,
                            FeedType = CreateFeedType.ReplyReply,
                            PopupTransitions = new TransitionCollection
                            {
                                new EdgeUIThemeTransition
                                {
                                    Edge = EdgeTransitionLocation.Bottom
                                }
                            }
                        }.Show();
                    }
                    DisabledCopy();
                    break;

                case "ShareButton":
                    DisabledCopy();
                    break;

                case "ChangeButton":
                    DisabledCopy();
                    //UIHelper.NavigateInSplitPane(typeof(AdaptivePage), new ViewModels.AdaptivePage.ViewModel((sender as FrameworkElement).Tag as string, ViewModels.AdaptivePage.ListType.FeedInfo, "changeHistory"));
                    break;

                default:
                    DisabledCopy();
                    _ = UIHelper.OpenLinkAsync((sender as FrameworkElement).Tag as string);
                    break;
            }
        }

        private async void DeviceHyperlink_Click(Hyperlink sender, HyperlinkClickEventArgs args)
        {
            UIHelper.ShowProgressBar();
            string device = (sender.Inlines.FirstOrDefault().ElementStart.VisualParent.DataContext as FeedModelBase).DeviceTitle;
            (bool isSucceed, JToken result) = await RequestHelper.GetDataAsync(UriHelper.GetUri(UriType.GetProductDetailByName, device), true);
            UIHelper.HideProgressBar();
            if (!isSucceed) { return; }

            JObject token = (JObject)result;

            if (token.TryGetValue("id", out JToken id))
            {
                FeedListViewModel provider = FeedListViewModel.GetProvider(FeedListType.ProductPageList, id.ToString());

                if (provider != null)
                {
                    UIHelper.Navigate(typeof(FeedListPage), provider);
                }
            }
        }

        private async Task<bool> PinSecondaryTile(FeedModelBase feed)
        {
            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse();

            // Construct a unique tile ID, which you will need to use later for updating the tile
            string tileId = feed.Url.GetMD5();

            bool isPinned = SecondaryTile.Exists(tileId);
            if (isPinned)
            {
                UIHelper.ShowMessage(loader.GetString("AlreadyPinnedTile"));
            }
            else
            {
                // Use a display name you like
                string displayName = $"{feed.UserInfo.UserName}的{feed.Info}";

                // Provide all the required info in arguments so that when user
                // clicks your tile, you can navigate them to the correct content
                string arguments = feed.Url;

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
                    TileContent tileContent = LiveTileTask.GetFeedTitle(feed);
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

        private void CopyMenuItem_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            DataPackage dp = new DataPackage();
            dp.SetText(element.Tag.ToString());
            Clipboard.SetContent(dp);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            UserControl_SizeChanged(sender, null);
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UserControl UserControl = sender as UserControl;
            FrameworkElement StackPanel = UserControl.FindChild("BtnsPanel");
            double width = e is null ? UserControl.Width : e.NewSize.Width;
            StackPanel?.SetValue(Grid.RowProperty, width > 600 ? 1 : 20);
        }

        private void GridView_SelectionChanged(object sender, SelectionChangedEventArgs e) => (sender as GridView).SelectedIndex = -1;
    }
}
