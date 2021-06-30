using CoolapkUWP.Helpers;
using CoolapkUWP.Models;
using CoolapkUWP.Pages.FeedPages;
using CoolapkUWP.ViewModels.FeedListPage;
using System;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel.UserActivities;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace CoolapkUWP.Controls
{
    public sealed partial class FeedShellDetailControl : UserControl, INotifyPropertyChanged
    {
        private FeedDetailModel feedDetail;

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        public FeedDetailModel FeedDetail
        {
            get => feedDetail;
            set
            {
                feedDetail = value;
                if (value?.IsQuestionFeed ?? true)
                {
                    if (makeFeed != null)
                    {
                        UnloadObject(makeFeed);
                    }
                }
                else
                {
                    FindName(nameof(makeFeed));
                }
                GenerateActivityAsync();
                RaisePropertyChangedEvent();
            }
        }

        internal Grid FeedArticleTitle { get => feedArticleTitle; }

        public FeedShellDetailControl()
        {
            InitializeComponent();
        }

        public event EventHandler RequireRefresh;

        internal static void Image_Tapped(object sender, TappedRoutedEventArgs _) => UIHelper.ShowImage((sender as FrameworkElement).Tag as ImageModel);

        private void StackPanel_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (FeedDetail.IsCopyEnabled || (e != null && !UIHelper.IsOriginSource(sender, e.OriginalSource))) { return; }

            UIHelper.OpenLinkAsync((sender as FrameworkElement).Tag as string);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            switch (element.Name)
            {
                case "makeReplyButton":
                    FeedDetail.IsCopyEnabled = false;
                    makeFeed.Visibility = (makeFeed.Visibility == Visibility.Visible) ? Visibility.Collapsed : Visibility.Visible;
                    break;

                case "reportButton":
                    FeedDetail.IsCopyEnabled = false;
                    UIHelper.Navigate(typeof(Pages.BrowserPage), new object[] { false, $"https://m.coolapk.com/mp/do?c=feed&m=report&type=feed&id={FeedDetail.Id}" });
                    break;

                case "deviceButton":
                    FeedDetail.IsCopyEnabled = false;
                    FeedListPageViewModelBase f = FeedListPageViewModelBase.GetProvider(FeedListType.DevicePageList, (sender as FrameworkElement).Tag as string);
                    if (f != null)
                    {
                        UIHelper.NavigateInSplitPane(typeof(FeedListPage), f);
                    }
                    break;

                case "changeButton":
                    FeedDetail.IsCopyEnabled = false;
                    UIHelper.NavigateInSplitPane(typeof(AdaptivePage), new ViewModels.AdaptivePage.ViewModel((sender as FrameworkElement).Tag as string, ViewModels.AdaptivePage.ListType.FeedInfo, "changeHistory"));
                    break;

                default:
                    FeedDetail.IsCopyEnabled = false;
                    UIHelper.OpenLinkAsync((sender as Button).Tag as string);
                    break;
            }
        }

        public static void Grid_Tapped(object sender, TappedRoutedEventArgs _)
        {
            if ((sender as FrameworkElement).Tag is string s)
            {
                UIHelper.OpenLinkAsync(s);
            }
        }

        private void makeFeed_MakedFeedSuccessful(object sender, EventArgs e)
        {
            RequireRefresh?.Invoke(this, new EventArgs());
        }

        private void Grid_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            moreButton.Flyout.ShowAt(this);
        }

        private void Grid_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter || e.Key == Windows.System.VirtualKey.Space)
            {
                StackPanel_Tapped(sender, null);
            }
            else if (e.Key == Windows.System.VirtualKey.Menu)
            {
                moreButton.Flyout.ShowAt(this);
            }
        }

        private static string Massage_Ex(string str)
        {
            Regex r = new Regex("<a.*?>", RegexOptions.IgnoreCase);
            Regex r1 = new Regex("<a.*?/>", RegexOptions.IgnoreCase);
            Regex r2 = new Regex("</a.*?>", RegexOptions.IgnoreCase);
            str = r.Replace(str, "");
            str = r1.Replace(str, "");
            str = r2.Replace(str, "");
            return str;
        }

        UserActivitySession _currentActivity;
        private async Task GenerateActivityAsync()
        {
            // Get the default UserActivityChannel and query it for our UserActivity. If the activity doesn't exist, one is created.
            UserActivityChannel channel = UserActivityChannel.GetDefault();
            UserActivity userActivity = await channel.GetOrCreateUserActivityAsync(FeedDetail.QRUrl);

            // Populate required properties
            userActivity.VisualElements.DisplayText = FeedDetail.MessageTitle;
            userActivity.VisualElements.AttributionDisplayText = FeedDetail.MessageTitle;
            userActivity.VisualElements.Description = Massage_Ex(FeedDetail.Message);
            userActivity.ActivationUri = new Uri("coolapk://" + FeedDetail.QRUrl);

            //Save
            await userActivity.SaveAsync(); //save the new metadata

            // Dispose of any current UserActivitySession, and create a new one.
            _currentActivity?.Dispose();
            _currentActivity = userActivity.CreateSession();
        }
    }
}