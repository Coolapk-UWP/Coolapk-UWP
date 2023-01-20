using CoolapkUWP.Helpers;
using CoolapkUWP.Models;
using CoolapkUWP.Pages.BrowserPages;
using CoolapkUWP.Pages.FeedPages;
using CoolapkUWP.ViewModels.BrowserPages;
using CoolapkUWP.ViewModels.FeedPages;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace CoolapkUWP.Controls.DataTemplates
{
    public sealed partial class IndexCardTemplates : ResourceDictionary
    {
        public IndexCardTemplates() => InitializeComponent();

        private void FlipView_SizeChanged(object sender, SizeChangedEventArgs e) => (sender as FrameworkElement).MaxHeight = e.NewSize.Width / 3;

        private void FlipView_Loaded(object sender, RoutedEventArgs e)
        {
            if (SettingsHelper.Get<bool>(SettingsHelper.IsNoPicsMode))
            {
                (sender as FrameworkElement).Visibility = Visibility.Collapsed;
            }
            else
            {
                FlipView view = sender as FlipView;
                view.MaxHeight = view.ActualWidth / 3;
                DispatcherTimer timer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(20)
                };
                timer.Tick += (o, a) =>
                {
                    if (view.SelectedIndex != -1)
                    {
                        if (view.SelectedIndex + 1 >= view.Items.Count)
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
                    }
                };
                view.Unloaded += (_, __) => timer.Stop();
                timer.Start();
            }
        }

        private void Grid_KeyDown(object sender, KeyRoutedEventArgs e)
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OnTapped((sender as FrameworkElement).Tag);
        }

        private void OnTapped(object tag)
        {
            if (tag is string strings)
            {
                //if (s.Contains("我的常去"))
                //{
                //    UIHelper.NavigateInSplitPane(typeof(HistoryPage), new ViewModels.HistoryPage.ViewModel("我的常去"));
                //}
                //else if (s.Contains("浏览历史"))
                //{
                //    UIHelper.NavigateInSplitPane(typeof(HistoryPage), new ViewModels.HistoryPage.ViewModel("浏览历史"));
                //}
                //else
                if (strings.Contains("我关注的话题"))
                {
                    UIHelper.Navigate(typeof(AdaptivePage), new AdaptiveViewModel("#/topic/userFollowTagList"));
                }
                else if (strings.Contains("我的收藏单"))
                {
                }
                //else if (s.Contains("我的问答"))
                //{
                //    string r = SettingsHelper.Get<string>(SettingsHelper.Uid);
                //    if (r != null) { UIHelper.NavigateInSplitPane(typeof(AdaptivePage), new ViewModel(r, ListType.UserFeed, "questionAndAnswer")); }
                //}
                else { UIHelper.OpenLinkAsync(strings); }
            }
            else if (tag is IHasTitle u)
            {
                if (string.IsNullOrEmpty(u.Url) || u.Url == "/topic/quickList?quickType=list") { return; }
                string str = u.Url;
                if (str == "Login")
                {
                    UIHelper.Navigate(typeof(BrowserPage), new BrowserViewModel(UriHelper.LoginUri));
                }
                else if (str.IndexOf("/page", StringComparison.Ordinal) == 0)
                {
                    str = str.Replace("/page", "/page/dataList");
                    str += $"&title={u.Title}";
                    UIHelper.Navigate(typeof(AdaptivePage), new AdaptiveViewModel(str));
                }
                else if (str.IndexOf('#') == 0)
                {
                    UIHelper.Navigate(typeof(AdaptivePage), new AdaptiveViewModel($"{str}&title={u.Title}"));
                }
                //else if (str.Contains("我的常去"))
                //{
                //    UIHelper.NavigateInSplitPane(typeof(HistoryPage), new ViewModels.HistoryPage.ViewModel("我的常去"));
                //}
                //else if (str.Contains("浏览历史"))
                //{
                //    UIHelper.NavigateInSplitPane(typeof(HistoryPage), new ViewModels.HistoryPage.ViewModel("浏览历史"));
                //}
                else if (str.Contains("我关注的话题"))
                {
                    UIHelper.Navigate(typeof(AdaptivePage), new AdaptiveViewModel("#/topic/userFollowTagList"));
                }
                else if (str.Contains("我的收藏单"))
                {
                }
                //else if (str.Contains("我的问答"))
                //{
                //    string r = SettingsHelper.Get<string>(SettingsHelper.Uid);
                //    if (r != null) { UIHelper.NavigateInSplitPane(typeof(AdaptivePage), new ViewModel(r, ListType.UserFeed, "questionAndAnswer")); }
                //}
                else { UIHelper.OpenLinkAsync(str); }
            }
        }
    }
}