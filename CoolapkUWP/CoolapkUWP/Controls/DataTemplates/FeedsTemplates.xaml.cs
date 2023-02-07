using CoolapkUWP.Helpers;
using CoolapkUWP.Models;
using CoolapkUWP.Models.Feeds;
using CoolapkUWP.Pages.BrowserPages;
using CoolapkUWP.Pages.FeedPages;
using CoolapkUWP.ViewModels.BrowserPages;
using CoolapkUWP.ViewModels.FeedPages;
using Microsoft.Toolkit.Uwp.UI;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace CoolapkUWP.Controls.DataTemplates
{
    public partial class FeedsTemplates : ResourceDictionary
    {
        public FeedsTemplates() => InitializeComponent();

        private void OnTapped(object sender, TappedRoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            if (e != null && !UIHelper.IsOriginSource(sender, e.OriginalSource)) { return; }
            if ((element.DataContext as ICanCopy)?.IsCopyEnabled ?? false) { return; }

            if (e != null) { e.Handled = true; }

            UIHelper.OpenLinkAsync(element.Tag.ToString());
        }

        private void OnTopTapped(object sender, TappedRoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            if ((element.DataContext as ICanCopy)?.IsCopyEnabled ?? false) { return; }

            if (e != null) { e.Handled = true; }

            UIHelper.OpenLinkAsync(element.Tag.ToString());
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            switch (element.Name)
            {
                default:
                    UIHelper.OpenLinkAsync((sender as FrameworkElement).Tag.ToString());
                    break;
            }
        }

        private void ReplyButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement frameworkElement)
            {
                if ((frameworkElement.Tag as ICanCopy)?.IsCopyEnabled ?? false) { return; }
                UIHelper.Navigate(typeof(AdaptivePage), AdaptiveViewModel.GetReplyListProvider(((FeedReplyModel)frameworkElement.Tag).ID.ToString(), (FeedReplyModel)frameworkElement.Tag));
            }
        }

        private async void FeedButton_Click(object sender, RoutedEventArgs _)
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
                case "MakeReplyButton":
                    DisabledCopy();
                    break;

                case "LikeButton":
                    DisabledCopy();
                    await RequestHelper.ChangeLikeAsync(element.Tag as ICanLike, element.Dispatcher);
                    break;

                case "ReportButton":
                    DisabledCopy();
                    UIHelper.Navigate(typeof(BrowserPage), new BrowserViewModel(element.Tag.ToString()));
                    break;

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
                    else if (element.Tag is FeedReplyModel reply)
                    {
                        new CreateFeedControl
                        {
                            ReplyID = reply.ID,
                            FeedType = CreateFeedType.ReplyReply,
                            PopupTransitions = new TransitionCollection
                            {
                                new EdgeUIThemeTransition
                                {
                                    Edge = EdgeTransitionLocation.Bottom
                                }
                            }
                        }.Show();
                    }
                    DisabledCopy();
                    break;

                case "ShareButton":
                    DisabledCopy();
                    break;

                case "DeviceButton":
                    DisabledCopy();
                    //FeedListPageViewModelBase f = FeedListPageViewModelBase.GetProvider(FeedListType.DevicePageList, (sender as FrameworkElement).Tag as string);
                    //if (f != null)
                    //{
                    //    UIHelper.NavigateInSplitPane(typeof(FeedListPage), f);
                    //}
                    break;

                case "ChangeButton":
                    DisabledCopy();
                    //UIHelper.NavigateInSplitPane(typeof(AdaptivePage), new ViewModels.AdaptivePage.ViewModel((sender as FrameworkElement).Tag as string, ViewModels.AdaptivePage.ListType.FeedInfo, "changeHistory"));
                    break;

                default:
                    DisabledCopy();
                    UIHelper.OpenLinkAsync((sender as FrameworkElement).Tag as string);
                    break;
            }
        }

        private void CopyMenuItem_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            DataPackage dp = new DataPackage();
            dp.SetText(element.Tag.ToString());
            Clipboard.SetContent(dp);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            UserControl_SizeChanged(sender, null);
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UserControl UserControl = sender as UserControl;
            FrameworkElement StackPanel = UserControl.FindChild("BtnsPanel");
            double width = e is null ? UserControl.Width : e.NewSize.Width;
            StackPanel?.SetValue(Grid.RowProperty, width > 600 ? 1 : 20);
        }

        private void GridView_SelectionChanged(object sender, SelectionChangedEventArgs e) => (sender as GridView).SelectedIndex = -1;
    }
}
