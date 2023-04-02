using CoolapkUWP.BackgroundTasks;
using CoolapkUWP.Helpers;
using CoolapkUWP.Models;
using CoolapkUWP.Models.Feeds;
using CoolapkUWP.Models.Pages;
using CoolapkUWP.ViewModels.FeedPages;
using CoolapkUWP.ViewModels.Providers;
using System;
using System.ComponentModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace CoolapkUWP.Pages.FeedPages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class NotificationsPage : Page, INotifyPropertyChanged
    {
        private static readonly int PivotIndex = 0;

        private bool isLoaded;
        private Action Refresh;

        private NotificationsTask _notificationsTask;
        public NotificationsTask NotificationsTask
        {
            get => _notificationsTask;
            set
            {
                if (_notificationsTask != value)
                {
                    _notificationsTask = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        public NotificationsPage() => InitializeComponent();

        private void Pivot_Loaded(object sender, RoutedEventArgs e)
        {
            if (!isLoaded)
            {
                Pivot.SelectedIndex = PivotIndex;
                NotificationsTask = NotificationsTask.Instance;
                isLoaded = true;
            }
            NotificationsTask?.GetNums();
        }

        private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PivotItem MenuItem = Pivot.SelectedItem as PivotItem;
            if ((Pivot.SelectedItem as PivotItem).Content is Frame Frame && Frame.Content is null)
            {
                switch ((Pivot.SelectedItem as PivotItem).Tag.ToString())
                {
                    case "CommentMe":
                        _ = Frame.Navigate(typeof(AdaptivePage), new AdaptiveViewModel(
                            new CoolapkListProvider(
                                (p, firstItem, lastItem) =>
                                    UriHelper.GetUri(
                                        UriType.GetNotifications,
                                        "list",
                                        p,
                                        string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}",
                                        string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}"),
                                    (o) => new Entity[] { new SimpleNotificationModel(o) },
                                    "id")));
                        break;
                    case "AtMe":
                        _ = Frame.Navigate(typeof(AdaptivePage), new AdaptiveViewModel(
                            new CoolapkListProvider(
                                (p, firstItem, lastItem) =>
                                    UriHelper.GetUri(
                                        UriType.GetNotifications,
                                        "atMeList",
                                        p,
                                        string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}",
                                        string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}"),
                                    (o) => new Entity[] { new FeedModel(o) },
                                    "id")));
                        break;
                    case "AtCommentMe":
                        _ = Frame.Navigate(typeof(AdaptivePage), new AdaptiveViewModel(
                            new CoolapkListProvider(
                                (p, firstItem, lastItem) =>
                                    UriHelper.GetUri(
                                        UriType.GetNotifications,
                                        "atCommentMeList",
                                        p,
                                        string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}",
                                        string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}"),
                                    (o) => new Entity[] { new AtCommentMeNotificationModel(o) },
                                    "id")));
                        break;
                    case "FeedLike":
                        _ = Frame.Navigate(typeof(AdaptivePage), new AdaptiveViewModel(
                            new CoolapkListProvider(
                                (p, firstItem, lastItem) =>
                                    UriHelper.GetUri(
                                        UriType.GetNotifications,
                                        "feedLikeList",
                                        p,
                                        string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}",
                                        string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}"),
                                    (o) => new Entity[] { new LikeNotificationModel(o) },
                                    "id")));
                        break;
                    case "Follow":
                        _ = Frame.Navigate(typeof(AdaptivePage), new AdaptiveViewModel(
                            new CoolapkListProvider(
                                (p, firstItem, lastItem) =>
                                    UriHelper.GetUri(
                                        UriType.GetNotifications,
                                        "contactsFollowList",
                                        p,
                                        string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}",
                                        string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}"),
                                    (o) => new Entity[] { new SimpleNotificationModel(o) },
                                    "id")));
                        break;
                    case "Message":
                        _ = Frame.Navigate(typeof(AdaptivePage), new AdaptiveViewModel(
                            new CoolapkListProvider(
                                (p, firstItem, lastItem) =>
                                    UriHelper.GetUri(
                                        UriType.GetChats,
                                        p,
                                        string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}",
                                        string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}"),
                                    (o) => new Entity[] { new MessageNotificationModel(o) },
                                    "id")));
                        break;
                    default:
                        break;
                }
                Refresh = () => _ = (Frame.Content as AdaptivePage).Refresh(true);
            }
            else if ((Pivot.SelectedItem as PivotItem).Content is Frame __ && __.Content is AdaptivePage AdaptivePage)
            {
                Refresh = () => _ = AdaptivePage.Refresh(true);
            }
            NotificationsTask?.GetNums();
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            Refresh();
            NotificationsTask?.GetNums();
        }
    }
}
