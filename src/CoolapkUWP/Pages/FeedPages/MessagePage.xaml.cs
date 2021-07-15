using CoolapkUWP.Helpers;
using CoolapkUWP.Models;
using CoolapkUWP.ViewModels.MessagePage;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using RefreshContainer = Microsoft.UI.Xaml.Controls.RefreshContainer;

namespace CoolapkUWP.Pages.FeedPages
{
    public sealed partial class MessagePage : Page
    {
        private ViewModel provider;
        private void UIHelper_RequireIndexPageRefresh(object sender, EventArgs e) => _ = Refresh(-2);

        public MessagePage()
        {
            InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            ShowProgressRing();
            provider = e.Parameter as ViewModel;

            if (!provider.ShowTitleBar)
            {
                titleBar.Visibility = Visibility.Collapsed;
                //RefreshContainer.Margin = new Thickness(0);
                UIHelper.RequireIndexPageRefresh += UIHelper_RequireIndexPageRefresh;
            }

            try
            {
                FeedList.ItemsSource = provider.Models;
                await Refresh(-2);
                titleBar.Title = provider.Title;
                //_ = scrollViewer.ChangeView(null, provider.VerticalOffsets[0], null, true);
                HideProgressRing();
            }
            catch
            {
                if (!provider.ShowTitleBar)
                {
                    UIHelper.ErrorProgressBar();
                }
            }
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            //provider.VerticalOffsets[0] = scrollViewer.VerticalOffset;
            titleBar.Title = string.Empty;

            if (!provider.ShowTitleBar)
            {
                UIHelper.RequireIndexPageRefresh -= UIHelper_RequireIndexPageRefresh;
            }

            base.OnNavigatingFrom(e);
        }

        private void ShowProgressRing()
        {
            if (provider?.ShowTitleBar ?? true)
            {
                titleBar.ShowProgressRing();
            }
            else
            {
                UIHelper.ShowProgressBar();
            }
        }

        private void HideProgressRing()
        {
            if (provider?.ShowTitleBar ?? true)
            {
                titleBar.HideProgressRing();
            }
            else
            {
                UIHelper.HideProgressBar();
            }
        }

        private void VScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            //if (!e.IsIntermediate && scrollViewer.VerticalOffset == scrollViewer.ScrollableHeight)
            //{
            //    _ = Refresh();
            //}
        }

        private async Task Refresh(int p = -1)
        {
            ShowProgressRing();
            if (p == -2)
            {
                //_ = (scrollViewer?.ChangeView(null, 0, null));
                titleBar.Title = provider.Title;
            }
            await provider.Refresh(p);
            //if (p == -2 && scrollViewer.VerticalOffset != 0)
            //{
            //    _ = (scrollViewer?.ChangeView(null, 0, null));
            //}
            HideProgressRing();
        }

        private void TitleBar_BackButtonClick(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }

        private void TitleBar_RefreshEvent(object sender, RoutedEventArgs e) => _ = Refresh(-2);

        private static void OnTapped(object tag)
        {
            if (tag is string s)
            {
                if (s.Contains("我的常去", StringComparison.Ordinal))
                {
                    UIHelper.NavigateInSplitPane(typeof(HistoryPage), new ViewModels.HistoryPage.ViewModel("我的常去"));
                }
                else if (s.Contains("浏览历史", StringComparison.Ordinal))
                {
                    UIHelper.NavigateInSplitPane(typeof(HistoryPage), new ViewModels.HistoryPage.ViewModel("浏览历史"));
                }
                else if (s.Contains("我关注的话题", StringComparison.Ordinal))
                {
                    UIHelper.NavigateInSplitPane(typeof(IndexPage), new ViewModels.IndexPage.ViewModel("#/topic/userFollowTagList", true));
                }
                else if (s.Contains("我的收藏单", StringComparison.Ordinal))
                {
                }
                else if (s.Contains("我的问答", StringComparison.Ordinal))
                {
                    string r = SettingsHelper.Get<string>(SettingsHelper.Uid);
                    if (r != null) { UIHelper.NavigateInSplitPane(typeof(AdaptivePage), new ViewModel(r, ListType.UserFeed, "questionAndAnswer")); }
                }
                else { UIHelper.OpenLinkAsync(tag as string); }
            }
            else if (tag is IHasUriAndTitle u)
            {
                if (string.IsNullOrEmpty(u.Url) || u.Url == "/topic/quickList?quickType=list") { return; }
                string str = u.Url;
                if (str == "Login")
                {
                    UIHelper.Navigate(typeof(BrowserPage), new object[] { true, null });
                }
                else if (str.IndexOf("/page", StringComparison.Ordinal) == 0)
                {
                    str = str.Replace("/page", "/page/dataList", StringComparison.Ordinal);
                    str += $"&title={u.Title}";
                    UIHelper.Navigate(typeof(IndexPage), new ViewModels.IndexPage.ViewModel(str, true));
                }
                else if (str.IndexOf('#') == 0)
                {
                    UIHelper.Navigate(typeof(IndexPage), new ViewModels.IndexPage.ViewModel($"{str}&title={u.Title}", true));
                }
                else if (str.Contains("我的常去", StringComparison.Ordinal))
                {
                    UIHelper.NavigateInSplitPane(typeof(HistoryPage), new ViewModels.HistoryPage.ViewModel("我的常去"));
                }
                else if (str.Contains("浏览历史", StringComparison.Ordinal))
                {
                    UIHelper.NavigateInSplitPane(typeof(HistoryPage), new ViewModels.HistoryPage.ViewModel("浏览历史"));
                }
                else if (str.Contains("我关注的话题", StringComparison.Ordinal))
                {
                    UIHelper.NavigateInSplitPane(typeof(IndexPage), new ViewModels.IndexPage.ViewModel("#/topic/userFollowTagList", true));
                }
                else if (str.Contains("我的收藏单", StringComparison.Ordinal))
                {
                }
                else if (str.Contains("我的问答", StringComparison.Ordinal))
                {
                    string r = SettingsHelper.Get<string>(SettingsHelper.Uid);
                    if (r != null) { UIHelper.NavigateInSplitPane(typeof(AdaptivePage), new ViewModel(r, ListType.UserFeed, "questionAndAnswer")); }
                }
                else { UIHelper.OpenLinkAsync(str); }
            }
            else if (tag is IndexPageModel i && !string.IsNullOrEmpty(i.Url))
            {
                UIHelper.OpenLinkAsync(i.Url);
            }
        }

        private void ListViewItem_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter || e.Key == Windows.System.VirtualKey.Space)
            {
                OnTapped((sender as FrameworkElement).Tag);
            }
        }

        private void ListViewItem_Tapped_1(object sender, TappedRoutedEventArgs e)
        {
            IndexPageModel model = (sender as FrameworkElement).DataContext as IndexPageModel;
            string u = $"/page/dataList?url={model.Url.Replace("#", "%23", StringComparison.Ordinal)}&title={model.Title}";
            if (provider.TabProviders.Count > 0)
            {
                provider.TabProviders[provider.ComboBoxSelectedIndex].ChangeGetDataFunc(
                    ViewModels.IndexPage.ViewModel.GetUri(u, false),
                    (a) => a?.EntityType == "feed");
            }
            else
            {
                provider.Provider.ChangeGetDataFunc(
                    ViewModels.IndexPage.ViewModel.GetUri(u, false),
                    (a) => a?.EntityType == "topic");
            }
            _ = Refresh(-2);
        }

        private void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (e != null && !UIHelper.IsOriginSource(sender, e.OriginalSource)) { return; }

            OnTapped((sender as FrameworkElement).Tag);
        }

        private void Pivot_Loaded(object sender, RoutedEventArgs e)
        {
            Pivot pivot = sender as Pivot;
            if (pivot.Items.Count == 0)
            {
                bool needAddTabs = provider.TabProviders.IsEmpty;
                int index = provider.ComboBoxSelectedIndex;
                foreach (IndexPageModel model in pivot.Tag as System.Collections.IEnumerable)
                {
                    if (needAddTabs)
                    {
                        provider.AddTab($"/page/dataList?url={model.Url.Replace("#", "%23", StringComparison.Ordinal)}&title={model.Title}");
                    }

                    PivotItem pivotItem = new PivotItem
                    {
                        Header = model.Title
                    };
                    pivot.Items.Add(pivotItem);
                }

                for (int i = 0; i < provider.TabProviders.Count; i++)
                {
                    ItemsRepeater list = new ItemsRepeater
                    {
                        ItemTemplate = Resources["CardTemplateSelector"] as DataTemplateSelector,
                        ItemsSource = provider.TabProviders[i].Models,
                    };
                    ((PivotItem)pivot.Items[i]).Content = list;
                }
                pivot.SelectedIndex = index;
            }
        }

        private async void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Pivot element = sender as Pivot;
            if (element.SelectedIndex == -1) { return; }

            ShowProgressRing();
            await provider.SetComboBoxSelectedIndex(element.SelectedIndex);
            HideProgressRing();
        }

        private async void RefreshContainer_RefreshRequested(Windows.UI.Xaml.Controls.RefreshContainer sender, Windows.UI.Xaml.Controls.RefreshRequestedEventArgs args)
        {
            using (Windows.Foundation.Deferral RefreshCompletionDeferral = args.GetDeferral())
            {
                await Refresh(-2);
            }
        }
    }
}