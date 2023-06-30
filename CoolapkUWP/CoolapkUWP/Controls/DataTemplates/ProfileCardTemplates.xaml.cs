using CoolapkUWP.Helpers;
using CoolapkUWP.Models;
using CoolapkUWP.Pages.BrowserPages;
using CoolapkUWP.Pages.FeedPages;
using CoolapkUWP.ViewModels.BrowserPages;
using CoolapkUWP.ViewModels.FeedPages;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace CoolapkUWP.Controls.DataTemplates
{
    public sealed partial class ProfileCardTemplates : ResourceDictionary
    {
        public static readonly DependencyProperty XamlHostProperty =
            DependencyProperty.Register(
                nameof(XamlHost),
                typeof(DependencyObject),
                typeof(ProfileCardTemplates),
                null);

        public DependencyObject XamlHost
        {
            get => (DependencyObject)GetValue(XamlHostProperty);
            set => SetValue(XamlHostProperty, value);
        }

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
                FrameworkElement element = sender as FrameworkElement;
                OnTapped(element, element.Tag);
            }
        }

        private void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (e != null && !UIHelper.IsOriginSource(sender, e.OriginalSource)) { return; }
            FrameworkElement element = sender as FrameworkElement;
            OnTapped(element, element.Tag);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            OnTapped(element, element.Tag);
        }

        private void OnTapped(FrameworkElement element, object tag)
        {
            if (tag is string str)
            {
                if (str.Contains("我的常去"))
                {
                    _ = XamlHost.NavigateAsync(typeof(AdaptivePage), AdaptiveViewModel.GetHistoryProvider("我的常去"));
                }
                else if (str.Contains("浏览历史"))
                {
                    _ = XamlHost.NavigateAsync(typeof(AdaptivePage), AdaptiveViewModel.GetHistoryProvider("浏览历史"));
                }
                else if (str.Contains("我关注的话题"))
                {
                    _ = XamlHost.NavigateAsync(typeof(AdaptivePage), new AdaptiveViewModel("#/topic/userFollowTagList"));
                }
                else if (str.Contains("我的收藏单"))
                {
                }
                else if (str.Contains("我的问答"))
                {
                    string uid = SettingsHelper.Get<string>(SettingsHelper.Uid);
                    if (uid != null) { _ = XamlHost.NavigateAsync(typeof(AdaptivePage), AdaptiveViewModel.GetUserFeedsProvider(uid, "questionAndAnswer")); }
                }
                else
                {
                    _ = XamlHost.OpenLinkAsync(str);
                }
                FlyoutBase?.Hide();
            }
            else if (tag is IHasTitle model)
            {
                if (string.IsNullOrEmpty(model.Url) || model.Url == "/topic/quickList?quickType=list") { return; }
                string url = model.Url;
                if (url == "Login")
                {
                    _ = XamlHost.NavigateAsync(typeof(BrowserPage), new BrowserViewModel(UriHelper.LoginUri));
                }
                else if (url.IndexOf("/page", StringComparison.Ordinal) == 0)
                {
                    url = url.Replace("/page", "/page/dataList");
                    url += $"&title={model.Title}";
                    _ = XamlHost.NavigateAsync(typeof(AdaptivePage), new AdaptiveViewModel(url));
                }
                else if (url.IndexOf('#') == 0)
                {
                    _ = XamlHost.NavigateAsync(typeof(AdaptivePage), new AdaptiveViewModel($"{url}&title={model.Title}"));
                }
                else if (url.Contains("我的常去"))
                {
                    _ = XamlHost.NavigateAsync(typeof(AdaptivePage), AdaptiveViewModel.GetHistoryProvider("我的常去"));
                }
                else if (url.Contains("浏览历史"))
                {
                    _ = XamlHost.NavigateAsync(typeof(AdaptivePage), AdaptiveViewModel.GetHistoryProvider("浏览历史"));
                }
                else if (url.Contains("我关注的话题"))
                {
                    _ = XamlHost.NavigateAsync(typeof(AdaptivePage), new AdaptiveViewModel("#/topic/userFollowTagList"));
                }
                else if (url.Contains("我的收藏单"))
                {
                }
                else if (url.Contains("我的问答"))
                {
                    string uid = SettingsHelper.Get<string>(SettingsHelper.Uid);
                    if (uid != null) { _ = XamlHost.NavigateAsync(typeof(AdaptivePage), AdaptiveViewModel.GetUserFeedsProvider(uid, "questionAndAnswer")); }
                }
                else
                {
                    _ = XamlHost.OpenLinkAsync(url);
                }
                FlyoutBase?.Hide();
            }
        }
    }
}
