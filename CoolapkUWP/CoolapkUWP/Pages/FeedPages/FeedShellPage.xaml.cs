﻿using CoolapkUWP.Controls;
using CoolapkUWP.Helpers;
using CoolapkUWP.Models;
using CoolapkUWP.Models.Feeds;
using CoolapkUWP.Pages.BrowserPages;
using CoolapkUWP.ViewModels.BrowserPages;
using CoolapkUWP.ViewModels.FeedPages;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.UserActivities;
using Windows.Foundation.Metadata;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using TwoPaneView = Microsoft.UI.Xaml.Controls.TwoPaneView;
using TwoPaneViewMode = Microsoft.UI.Xaml.Controls.TwoPaneViewMode;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace CoolapkUWP.Pages.FeedPages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class FeedShellPage : Page
    {
        private FeedShellViewModel Provider;
        private static UserActivitySession _currentActivity;

        public FeedShellPage() => InitializeComponent();

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is FeedShellViewModel ViewModel
                && (Provider == null || Provider.ID != ViewModel.ID))
            {
                Provider = ViewModel;
                DataContext = Provider;
                await Provider.Refresh(true);
                if (Provider.FeedDetail != null)
                {
                    SetLayout();
                    GenerateActivityAsync();
                }
            }
            await Task.Delay(30);
        }

        private void SetLayout()
        {
            TwoPaneView.MinWideModeWidth = Provider.FeedDetail?.IsFeedArticle ?? false ? 876 : 804;
            TwoPaneView.Pane1Length = new GridLength(Provider.FeedDetail?.IsFeedArticle ?? false ? 520 : 420);
        }

        private async void GenerateActivityAsync()
        {
            if (!ApiInformation.IsTypePresent("Windows.ApplicationModel.UserActivities.UserActivityChannel"))
            { return; }

            // Get the default UserActivityChannel and query it for our UserActivity. If the activity doesn't exist, one is created.
            UserActivityChannel channel = UserActivityChannel.GetDefault();
            UserActivity userActivity = await channel.GetOrCreateUserActivityAsync(Provider.FeedDetail.Url.GetMD5());

            // Populate required properties
            userActivity.VisualElements.DisplayText = Provider.Title;
            userActivity.VisualElements.AttributionDisplayText = Provider.Title;
            userActivity.VisualElements.Description = Provider.FeedDetail.Message.CSStoString();
            userActivity.ActivationUri = new Uri($"coolapk://{(Provider.FeedDetail.Url[0] == '/' ? Provider.FeedDetail.Url.Substring(1) : Provider.FeedDetail.Url)}");

            //Save
            await userActivity.SaveAsync(); //save the new metadata

            // Dispose of any current UserActivitySession, and create a new one.
            _currentActivity?.Dispose();
            _currentActivity = userActivity.CreateSession();
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
                    break;

                case "LikeButton":
                    DisabledCopy();
                    _ = (element.Tag as ICanLike).ChangeLike();
                    break;

                case "ReportButton":
                    DisabledCopy();
                    UIHelper.Navigate(typeof(BrowserPage), new BrowserViewModel(element.Tag.ToString()));
                    break;

                case "ShareButton":
                    DisabledCopy();
                    break;

                default:
                    DisabledCopy();
                    _ = UIHelper.OpenLinkAsync((sender as FrameworkElement).Tag as string);
                    break;
            }
        }

        #region 界面模式切换

        private void TwoPaneView_ModeChanged(TwoPaneView sender, object args)
        {
            // Remove details content from it's parent panel.
            if (HeaderControl.Parent != null)
            {
                (HeaderControl.Parent as Panel).Children.Remove(HeaderControl);
            }
            else
            {
                LeftGrid.Children.Remove(HeaderControl);
                RightGrid.Children.Remove(HeaderControl);
            }

            if (DetailControl.Parent != null)
            {
                (DetailControl.Parent as Panel).Children.Remove(DetailControl);
            }
            else
            {
                Pane1Grid.Children.Remove(DetailControl);
                Pane2Grid.Children.Remove(DetailControl);
            }

            if (BtnsPanel.Parent != null)
            {
                (BtnsPanel.Parent as Panel).Children.Remove(BtnsPanel);
            }
            else
            {
                LeftGrid.Children.Remove(BtnsPanel);
                RightGrid.Children.Remove(BtnsPanel);
            }

            // Single pane
            if (sender.Mode == TwoPaneViewMode.SinglePane)
            {
                ListControl.RefreshButtonVisibility = Visibility.Collapsed;
                // Add the details content to Pane1.
                RightGrid.Children.Add(HeaderControl);
                RightGrid.Children.Add(BtnsPanel);
                Pane2Grid.Children.Add(DetailControl);
            }
            // Dual pane.
            else
            {
                ListControl.RefreshButtonVisibility = Visibility.Visible;
                // Put details content in Pane2.
                LeftGrid.Children.Add(HeaderControl);
                LeftGrid.Children.Add(BtnsPanel);
                Pane1Grid.Children.Add(DetailControl);
            }
        }

        private void TwoPaneView_Loaded(object sender, RoutedEventArgs e)
        {
            TwoPaneView_ModeChanged(sender as TwoPaneView, null);
        }

        #endregion 界面模式切换
    }
}
