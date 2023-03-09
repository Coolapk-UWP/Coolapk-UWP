using CoolapkUWP.Helpers;
using CoolapkUWP.Models;
using CoolapkUWP.Pages.BrowserPages;
using CoolapkUWP.Pages.FeedPages;
using CoolapkUWP.ViewModels.BrowserPages;
using CoolapkUWP.ViewModels.FeedPages;
using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace CoolapkUWP.Controls.DataTemplates
{
    public sealed partial class ProfileCardTemplates : ResourceDictionary
    {
        public static readonly DependencyProperty FlyoutBaseProperty =
            DependencyProperty.Register(
                nameof(FlyoutBase),
                typeof(FlyoutBase),
                typeof(ProfileCardTemplates),
                null);

        public FlyoutBase FlyoutBase
        {
            get => (FlyoutBase)GetValue(FlyoutBaseProperty);
            set => SetValue(FlyoutBaseProperty, value);
        }

        public ProfileCardTemplates() => InitializeComponent();

        private void Grid_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter || e.Key == Windows.System.VirtualKey.Space)
            {
                _ = OnTapped((sender as FrameworkElement).Tag);
            }
        }

        private void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (e != null && !UIHelper.IsOriginSource(sender, e.OriginalSource)) { return; }

            _ = OnTapped((sender as FrameworkElement).Tag);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _ = OnTapped((sender as FrameworkElement).Tag);
        }

        private async Task OnTapped(object tag)
        {
            if (tag is string str)
            {
                if (str.Contains("我的常去"))
                {
                    UIHelper.Navigate(typeof(AdaptivePage), AdaptiveViewModel.GetHistoryProvider("我的常去"));
                }
                else if (str.Contains("浏览历史"))
                {
                    UIHelper.Navigate(typeof(AdaptivePage), AdaptiveViewModel.GetHistoryProvider("浏览历史"));
                }
                else if (str.Contains("我关注的话题"))
                {
                    UIHelper.Navigate(typeof(AdaptivePage), new AdaptiveViewModel("#/topic/userFollowTagList"));
                }
                else if (str.Contains("我的收藏单"))
                {
                }
                else if (str.Contains("我的问答"))
                {
                    string uid = SettingsHelper.Get<string>(SettingsHelper.Uid);
                    if (uid != null) { UIHelper.Navigate(typeof(AdaptivePage), AdaptiveViewModel.GetUserFeedsProvider(uid, "questionAndAnswer")); }
                }
                else
                {
                    await UIHelper.OpenLinkAsync(str);
                }
                FlyoutBase?.Hide();
            }
            else if (tag is IHasTitle model)
            {
                if (string.IsNullOrEmpty(model.Url) || model.Url == "/topic/quickList?quickType=list") { return; }
                string url = model.Url;
                if (url == "Login")
                {
                    UIHelper.Navigate(typeof(BrowserPage), new BrowserViewModel(UriHelper.LoginUri));
                }
                else if (url.IndexOf("/page", StringComparison.Ordinal) == 0)
                {
                    url = url.Replace("/page", "/page/dataList");
                    url += $"&title={model.Title}";
                    UIHelper.Navigate(typeof(AdaptivePage), new AdaptiveViewModel(url));
                }
                else if (url.IndexOf('#') == 0)
                {
                    UIHelper.Navigate(typeof(AdaptivePage), new AdaptiveViewModel($"{url}&title={model.Title}"));
                }
                else if (url.Contains("我的常去"))
                {
                    UIHelper.Navigate(typeof(AdaptivePage), AdaptiveViewModel.GetHistoryProvider("我的常去"));
                }
                else if (url.Contains("浏览历史"))
                {
                    UIHelper.Navigate(typeof(AdaptivePage), AdaptiveViewModel.GetHistoryProvider("浏览历史"));
                }
                else if (url.Contains("我关注的话题"))
                {
                    UIHelper.Navigate(typeof(AdaptivePage), new AdaptiveViewModel("#/topic/userFollowTagList"));
                }
                else if (url.Contains("我的收藏单"))
                {
                }
                else if (url.Contains("我的问答"))
                {
                    string uid = SettingsHelper.Get<string>(SettingsHelper.Uid);
                    if (uid != null) { UIHelper.Navigate(typeof(AdaptivePage), AdaptiveViewModel.GetUserFeedsProvider(uid, "questionAndAnswer")); }
                }
                else
                {
                    await UIHelper.OpenLinkAsync(url);
                }
                FlyoutBase?.Hide();
            }
        }
    }
}
