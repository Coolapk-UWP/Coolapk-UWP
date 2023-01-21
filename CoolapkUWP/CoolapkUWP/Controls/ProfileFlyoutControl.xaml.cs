using CoolapkUWP.Helpers;
using CoolapkUWP.Pages.BrowserPages;
using CoolapkUWP.Pages.FeedPages;
using CoolapkUWP.ViewModels.BrowserPages;
using CoolapkUWP.ViewModels.FeedPages;
using Microsoft.Toolkit.Uwp.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace CoolapkUWP.Controls
{
    public sealed partial class ProfileFlyoutControl : UserControl
    {
        private ProfileFlyoutViewModel Provider;

        public ProfileFlyoutControl()
        {
            InitializeComponent();
            Provider = new ProfileFlyoutViewModel();
            DataContext = Provider;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as FrameworkElement).Tag.ToString())
            {
                case "FeedsButton":
                    UIHelper.Navigate(typeof(FeedListPage), FeedListViewModel.GetProvider(FeedListType.UserPageList, Provider.ProfileDetail.EntityID.ToString()));
                    break;
                case "FollowsButton":
                    UIHelper.Navigate(typeof(AdaptivePage), AdaptiveViewModel.GetUserListProvider(SettingsHelper.Get<string>(SettingsHelper.Uid), true, "我"));
                    break;
                case "FansButton":
                    UIHelper.Navigate(typeof(AdaptivePage), AdaptiveViewModel.GetUserListProvider(SettingsHelper.Get<string>(SettingsHelper.Uid), false, "我"));
                    break;
                case "LoginButton":
                    UIHelper.Navigate(typeof(BrowserPage), new BrowserViewModel(UriHelper.LoginUri));
                    break;
                default:
                    break;
            }
        }

        private void Button_Loaded(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            ContentPresenter content = element.FindDescendant<ContentPresenter>();
            if (content != null)
            {
                switch (element.Tag.ToString())
                {
                    case "FeedsButton":
                        content.CornerRadius = new CornerRadius(4, 0, 0, 4);
                        break;
                    case "FollowsButton":
                        content.CornerRadius = new CornerRadius(0);
                        break;
                    case "FansButton":
                        content.CornerRadius = new CornerRadius(0, 4, 4, 0);
                        break;
                    default:
                        break;
                }
            }
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e) => await Provider.Refresh(true);
    }
}
