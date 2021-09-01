using CoolapkUWP.Control.ViewModels;
using CoolapkUWP.Data;
using CoolapkUWP.Helpers.DataHelpers;
using CoolapkUWP.Models.Pages;
using CoolapkUWP.ViewModels;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace CoolapkUWP.Pages.FeedPages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class FeedListPage : Page
    {
        private FeedListDS FeedListDS;
        private IFeedListDataProvider Provider;

        public FeedListPage() => InitializeComponent();
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            object[] vs = e.Parameter as object[];
            string str = vs[1] as string;
            FeedListType FeedListType = (FeedListType)vs[0];
            if (!string.IsNullOrEmpty(str))
            {
                if (FeedListType != Provider?.ListType || (FeedListType == Provider?.ListType && str != Provider.Id))
                {
                    switch (FeedListType)
                    {
                        case FeedListType.UserPageList:
                            if (str == "0") { Frame.GoBack(); }
                            else { Provider = new UserPageDataProvider(str); }
                            TitleBar.ComboBoxVisibility = Visibility.Visible;
                            TitleBar.ComboBoxItemsSource = new string[] { "动态", "图文", "问答" };
                            TitleBar.ComboBoxSelectedIndex = 0;
                            break;
                        case FeedListType.TagPageList:
                            Provider = new TagPageDataProvider(str);
                            TitleBar.ComboBoxVisibility = Visibility.Visible;
                            TitleBar.ComboBoxItemsSource = new string[] { "最近回复", "按时间排序", "按热度排序" };
                            TitleBar.ComboBoxSelectedIndex = 0;
                            break;
                        case FeedListType.DYHPageList:
                            Provider = new DYHPageDataProvider(str);
                            TitleBar.ComboBoxVisibility = Visibility.Collapsed;
                            TitleBar.ComboBoxItemsSource = new string[] { "精选", "广场" };
                            break;
                        case FeedListType.APPPageList:
                            Provider = new APPPageDataProvider(str);
                            TitleBar.ComboBoxVisibility = Visibility.Visible;
                            TitleBar.ComboBoxItemsSource = new string[] { "最近回复", "按时间排序", "按热度排序" };
                            TitleBar.ComboBoxSelectedIndex = 0;
                            break;
                        case FeedListType.ProductPageList:
                            Provider = new ProductPageDataProvider(str);
                            TitleBar.ComboBoxVisibility = Visibility.Visible;
                            TitleBar.ComboBoxItemsSource = new string[] { "讨论", "问答", "图文" };
                            TitleBar.ComboBoxSelectedIndex = 0;
                            break;
                        default:
                            break;
                    }
                    FeedListDS = new FeedListDS(Provider);
                    Refresh(-2);
                }
            }
            else { Frame.GoBack(); }
        }

        private void ListView_Loaded(object sender, RoutedEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    ScrollViewer ScrollViewer = VisualTree.FindDescendantByName(ListView, "ScrollViewer") as ScrollViewer;
                    ScrollViewer.ViewChanged += (s, ee) =>
                    {
                        if (!ee.IsIntermediate && ScrollViewer.VerticalOffset == ScrollViewer.ScrollableHeight)
                        {
                            Refresh();
                        }
                    };
                });
            });
        }

        private async void Refresh(int p = -1)
        {
            UIHelper.ShowProgressBar();
            switch (p)
            {
                case -1:
                    _ = await FeedListDS.LoadMoreItemsAsync(20);
                    break;
                case -2:
                    await FeedListDS.Refresh();
                    if (FeedListDS[0] is DYHDetail DYHDetail)
                    {
                        TitleBar.ComboBoxSelectedIndex = DYHDetail.SelectedIndex;
                        TitleBar.ComboBoxVisibility = DYHDetail.ShowComboBox ? Visibility.Visible : Visibility.Collapsed;
                    }
                    break;
                case -3:
                    if (FeedListDS.Count > 1)
                    {
                        for (int i = FeedListDS.Count - 1; i > 0; i--)
                        { FeedListDS.RemoveAt(i); }
                        _ = await FeedListDS.LoadMoreItemsAsync(20);
                    }
                    break;
                default:
                    break;
            }
            UIHelper.HideProgressBar();
        }

        private void TitleBar_BackButtonClick(object sender, RoutedEventArgs e) => Frame.GoBack();

        private void UserDetailBorder_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender is FrameworkElement fe)
            {
                if (fe != e.OriginalSource) { return; }
                if (fe.Tag is string s)
                {
                    if (s == (FeedListDS[0] as UserDetail).BackgroundUrl)
                    { UIHelper.ShowImage(s, ImageType.OriginImage); }
                    else { UIHelper.ShowImage(s, ImageType.SmallAvatar); }
                }
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            switch (button.Tag as string)
            {
                case "follow":
                    UIHelper.Navigate(typeof(UserListPage), new object[] { Provider.Id, true, TitleBar.Title });
                    break;
                case "fans":
                    UIHelper.Navigate(typeof(UserListPage), new object[] { Provider.Id, false, TitleBar.Title });
                    break;
                case "FollowUser":
                    JsonObject o = null;
                    switch ((FeedListDS[0] as UserDetail).FollowStatus)
                    {
                        case "关注":
                            o = JsonObject.Parse(await UIHelper.GetJson($"/user/follow?uid={Provider.Id}"));
                            break;
                        case "取消关注":
                            o = JsonObject.Parse(await UIHelper.GetJson($"/user/unfollow?uid={Provider.Id}"));
                            break;
                        default:
                            break;
                    }
                    if (o != null)
                    {
                        if (o.TryGetValue("message", out IJsonValue value))
                        {
                            UIHelper.ShowMessage($"{value.GetString()}");
                        }
                        else
                        {
                            FeedListDS.RemoveAt(0);
                            FeedListDS.Insert(0, await Provider.GetDetail());
                        }
                    }
                    break;
                default:
                    UIHelper.OpenLink(button.Tag as string);
                    break;
            }
        }

        private void FeedTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(Provider.Id)) { return; }
            if (FeedListDS != null)
            {
                ICanChangeSelectedIndex dataProvider = FeedListDS._provider as ICanChangeSelectedIndex;
                dataProvider.SelectedIndex = (sender as ComboBox).SelectedIndex;
                dataProvider.Reset();
                Refresh(-3);
            }
        }

        internal static void UserDetailBorder_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Border b = sender as Border;
            b.Height = e.NewSize.Width <= 400 ? e.NewSize.Width : 400;
        }

        private void ListView_RefreshRequested(object sender, EventArgs e) => Refresh(-2);

        private void TitleBar_RefreshEvent(object sender, RoutedEventArgs e) => Refresh(-2);
    }

    internal enum FeedListType
    {
        ProductPageList,
        UserPageList,
        TagPageList,
        DYHPageList,
        APPPageList
    }

    internal class TemplateSelector : DataTemplateSelector
    {
        public DataTemplate Feed { get; set; }
        public DataTemplate APPDetail { get; set; }
        public DataTemplate DYHDetail { get; set; }
        public DataTemplate UserDetail { get; set; }
        public DataTemplate TopicDetail { get; set; }
        public DataTemplate ProductDetail { get; set; }
        protected override DataTemplate SelectTemplateCore(object item)
        {
            return item is UserDetail ? UserDetail : item is TopicDetail ? TopicDetail : item is DYHDetail ? DYHDetail : item is ProductDetail ? ProductDetail : item is APPDetail ? APPDetail : Feed;
        }
        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container) => SelectTemplateCore(item);
    }

    /// <summary>
    /// Provide list of Minecraft Download Versions. <br/>
    /// You can bind this ds to ItemSource to enable incremental loading ,
    /// or call LoadMoreItemsAsync to load more.
    /// </summary>
    internal class FeedListDS : DataSourceBase<object>
    {
        public string Title;
        public IFeedListDataProvider _provider;

        internal FeedListDS(IFeedListDataProvider provider)
        {
            _provider = provider;
        }

        protected async override Task<IList<object>> LoadItemsAsync(uint count)
        {
            List<object> Items = new List<object>();
            if (_currentPage == 1)
            {
                object Header = await _provider.GetDetail();
                Title = _provider.GetTitleBarText(Header);
                Items.Insert(0, Header);
            }
            List<FeedViewModel> Feeds = await _provider.GetFeeds(_currentPage - 1);
            if (Feeds != null)
            {
                foreach (FeedViewModel item in Feeds)
                { Items.Add(item); }
            }
            return Items;
        }

        protected override void AddItems(IList<object> items)
        {
            if (items != null)
            {
                foreach (object news in items)
                {
                    if (news != null)
                    {
                        Add(news);
                    }
                }
            }
        }
    }
}
