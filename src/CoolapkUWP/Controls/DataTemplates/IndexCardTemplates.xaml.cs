using CoolapkUWP.Helpers;
using CoolapkUWP.Models;
using CoolapkUWP.Pages;
using CoolapkUWP.Pages.FeedPages;
using CoolapkUWP.ViewModels.AdaptivePage;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using System;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace CoolapkUWP.Controls.DataTemplates
{
    public partial class IndexCardTemplates : ResourceDictionary
    {
        public IndexCardTemplates()
        {
            InitializeComponent();
        }

        private void ListViewItem_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter || e.Key == Windows.System.VirtualKey.Space)
            {
                OnTapped((sender as FrameworkElement).Tag);
            }
        }

        private void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (e != null && !UIHelper.IsOriginSource(sender, e.OriginalSource)) { return; }

            OnTapped((sender as FrameworkElement).Tag);
        }

        internal static void FlipView_Loaded(object sender, RoutedEventArgs _)
        {
            FlipView view = sender as FlipView;
            view.MaxHeight = view.ActualWidth / 3;
            DispatcherTimer timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(20)
            };
            timer.Tick += (o, a) =>
            {
                if (view.SelectedIndex + 1 >= view.Items.Count())
                {
                    while (view.SelectedIndex > 0)
                    {
                        view.SelectedIndex -= 1;
                    }
                }
                else
                {
                    view.SelectedIndex += 1;
                }
            };

            timer.Start();
        }

        internal static void FlipView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            FlipView view = sender as FlipView;
            view.MaxHeight = e.NewSize.Width / 3;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OnTapped((sender as FrameworkElement).Tag);
        }

        internal static void TextBlockEx_RichTextBlockLoaded(object sender, EventArgs e)
        {
            TextBlockEx b = (TextBlockEx)sender;
            b.MaxLine = 4;
        }

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
    }
}