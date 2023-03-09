using CoolapkUWP.Helpers;
using CoolapkUWP.Models;
using CoolapkUWP.Models.Users;
using CoolapkUWP.Pages.BrowserPages;
using CoolapkUWP.Pages.FeedPages;
using CoolapkUWP.ViewModels.BrowserPages;
using CoolapkUWP.ViewModels.FeedPages;
using Microsoft.Toolkit.Uwp.UI;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace CoolapkUWP.Controls
{
    public sealed partial class ProfileFlyoutControl : UserControl
    {
        private DateTime dateTime = default;
        private readonly ProfileFlyoutViewModel Provider;

        public static readonly DependencyProperty FlyoutBaseProperty =
            DependencyProperty.Register(
                nameof(FlyoutBase),
                typeof(FlyoutBase),
                typeof(ProfileFlyoutControl),
                null);

        public FlyoutBase FlyoutBase
        {
            get => (FlyoutBase)GetValue(FlyoutBaseProperty);
            set => SetValue(FlyoutBaseProperty, value);
        }

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
                case "CreateFeedButton":
                    new CreateFeedControl
                    {
                        FeedType = CreateFeedType.Feed,
                        PopupTransitions = new TransitionCollection
                        {
                            new EdgeUIThemeTransition
                            {
                                Edge = EdgeTransitionLocation.Bottom
                            }
                        }
                    }.Show();
                    break;
                default:
                    break;
            }
            FlyoutBase?.Hide();
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

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!Provider.IsLogin || dateTime == default || DateTime.UtcNow - dateTime == TimeSpan.FromMinutes(1))
            {
                _ = Provider.Refresh(true);
                dateTime = DateTime.UtcNow;
            }
        }

        private void Grid_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            _ = Provider.Refresh(true);
            dateTime = DateTime.UtcNow;
        }
    }
}
