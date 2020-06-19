using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Pages.FeedListPageModels;
using CoolapkUWP.ViewModels.FeedListDataProvider;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace CoolapkUWP.Pages.FeedPages
{
    public sealed partial class FeedListPage : Page
    {
        private FeedListDataProvider provider;
        private ScrollViewer VScrollViewer;
        private bool isLoading = true;

        public FeedListPage() => this.InitializeComponent();

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            provider = e.Parameter as FeedListDataProvider;
            listView.ItemsSource = provider.itemCollection;
            switch (provider.ListType)
            {
                case FeedListType.UserPageList:
                    titleBar.ComboBoxVisibility = Visibility.Collapsed;
                    break;

                case FeedListType.TagPageList:
                    titleBar.ComboBoxVisibility = Visibility.Visible;
                    titleBar.ComboBoxItemsSource = new string[] { "最近回复", "按时间排序", "按热度排序" };
                    titleBar.ComboBoxSelectedIndex = (provider as ICanChangeSelectedIndex).SelectedIndex;
                    break;

                case FeedListType.DyhPageList:
                    titleBar.ComboBoxVisibility = Visibility.Collapsed;
                    titleBar.ComboBoxItemsSource = new string[] { "精选", "广场" };
                    break;
            }
            Refresh();

            if (VScrollViewer is null)
            {
                Task.Run(async () =>
                {
                    await Task.Delay(300);
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        VScrollViewer = VisualTree.FindDescendantByName(listView, "ScrollViewer") as ScrollViewer;
                        VScrollViewer.ViewChanged += async (s, ee) =>
                        {
                            if (!ee.IsIntermediate && VScrollViewer.VerticalOffset == VScrollViewer.ScrollableHeight)
                            {
                                UIHelper.ShowProgressRing();
                                await provider.LoadNextPage();
                                UIHelper.HideProgressRing();
                            }
                        };
                    });
                });
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (!isLoading)
                titleBar.ComboBoxSelectionChange -= FeedTypeComboBox_SelectionChanged;

            base.OnNavigatedFrom(e);
        }

        private async void Refresh()
        {
            UIHelper.ShowProgressRing();
            await provider.Refresh();
            if (provider.itemCollection.Count > 0)
            {
                if (provider.itemCollection[0] is DyhDetail detail)
                {
                    titleBar.ComboBoxSelectedIndex = detail.SelectedIndex;
                    titleBar.ComboBoxVisibility = detail.ShowComboBox ? Visibility.Visible : Visibility.Collapsed;
                }
                titleBar.Title = provider.Title;
                if (isLoading)
                {
                    titleBar.ComboBoxSelectionChange += FeedTypeComboBox_SelectionChanged;
                    isLoading = false;
                }
            }

            UIHelper.HideProgressRing();
        }

        private void TitleBar_BackButtonClick(object sender, RoutedEventArgs e) => Frame.GoBack();

        private void UserDetailBorder_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender is FrameworkElement fe)
            {
                if (fe != e.OriginalSource) return;
                if (fe.Tag is string s)
                    if (s == (provider.itemCollection[0] as UserDetail).BackgroundUrl)
                        UIHelper.ShowImage(s, ImageType.OriginImage);
                    else UIHelper.ShowImage(s, ImageType.BigAvatar);
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            string str = (sender as Button).Tag as string;
            switch (str)
            {
                case "follow":
                    UIHelper.Navigate(typeof(UserListPage), new object[] { provider.Id, true, titleBar.Title });
                    break;

                case "fans":
                    UIHelper.Navigate(typeof(UserListPage), new object[] { provider.Id, false, titleBar.Title });
                    break;

                case "FollowUser":
                    switch ((provider.itemCollection[0] as UserDetail).FollowStatus)
                    {
                        case "关注":
                            await DataHelper.GetDataAsync(DataUriType.OperateFollow, provider.Id);
                            break;

                        case "取消关注":
                            await DataHelper.GetDataAsync(DataUriType.OperateUnfollow, provider.Id);
                            break;
                    }
                    Refresh();
                    break;

                default:
                    UIHelper.OpenLinkAsync(str);
                    break;
            }
        }

        private void FeedTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(provider.Id)) return;
            ICanChangeSelectedIndex dataProvider = provider as ICanChangeSelectedIndex;
            dataProvider.SelectedIndex = (sender as ComboBox).SelectedIndex;
            dataProvider.Reset();
            if (provider.itemCollection.Count > 1)
            {
                for (int i = provider.itemCollection.Count - 1; i > 0; i--)
                    provider.itemCollection.RemoveAt(i);
                Refresh();
            }
        }
    }

    internal enum FeedListType
    {
        UserPageList,
        TagPageList,
        DyhPageList
    }

    internal class FeedListPageTemplateSelector : DataTemplateSelector
    {
        public DataTemplate UserHeader { get; set; }
        public DataTemplate Feed { get; set; }
        public DataTemplate TopicHeader { get; set; }
        public DataTemplate DyhHeader { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            switch (item)
            {
                case UserDetail _: return UserHeader;
                case TopicDetail _: return TopicHeader;
                case DyhDetail _: return DyhHeader;
                default: return Feed;
            }
        }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container) => SelectTemplateCore(item);
    }
}