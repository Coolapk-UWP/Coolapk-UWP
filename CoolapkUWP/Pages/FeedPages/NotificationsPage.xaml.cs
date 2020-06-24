using CoolapkUWP.Helpers;
using CoolapkUWP.Models;
using CoolapkUWP.Models.Pages.NotificationsPageModels;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace CoolapkUWP.Pages.FeedPages
{
    public sealed partial class NotificationsPage : Page
    {
        private readonly ObservableCollection<object> itemCollection = new ObservableCollection<object>();
        private NotificationPageType type;

        public NotificationsPage() => this.InitializeComponent();

        private string uri;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            type = (NotificationPageType)e.Parameter;
            var loader = ResourceLoader.GetForViewIndependentUse("NotificationsPage");
            switch (type)
            {
                case NotificationPageType.Comment:
                    titleBar.Title = loader.GetString("notification");
                    uri = "list";
                    FindName(nameof(NavigateItems));
                    Load<SimpleNotificationModel>();
                    break;

                case NotificationPageType.AtMe:
                    titleBar.Title = loader.GetString("AtMeText");
                    uri = "atMeList";
                    Load();
                    break;

                case NotificationPageType.AtCommentMe:
                    titleBar.Title = loader.GetString("AtMeCommentText");
                    uri = "atCommentMeList";
                    Load<AtCommentMeNotificationModel>();
                    break;

                case NotificationPageType.Like:
                    titleBar.Title = loader.GetString("LikedText");
                    uri = "feedLikeList";
                    Load<LikeNotificationModel>();
                    break;

                case NotificationPageType.Follow:
                    titleBar.Title = loader.GetString("FollowedText");
                    uri = "contactsFollowList";
                    Load<SimpleNotificationModel>();
                    break;
            }
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            StackPanel item = e.ClickedItem as StackPanel;
            switch (item.Tag as string)
            {
                case "atMe":
                    Frame.Navigate(typeof(NotificationsPage), NotificationPageType.AtMe);
                    break;

                case "atCommentMe":
                    Frame.Navigate(typeof(NotificationsPage), NotificationPageType.AtCommentMe);
                    break;

                case "like":
                    Frame.Navigate(typeof(NotificationsPage), NotificationPageType.Like);
                    break;

                case "follow":
                    Frame.Navigate(typeof(NotificationsPage), NotificationPageType.Follow);
                    break;
            }
        }

        private void Ellipse_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (e.OriginalSource is Windows.UI.Xaml.Shapes.Ellipse
                || (sender is Grid && !(e.OriginalSource is Windows.UI.Xaml.Shapes.Ellipse)))
                UIHelper.OpenLinkAsync((sender as FrameworkElement).Tag as string);
        }

        private double firstItem, lastItem;
        private int page;

        private void TitleBar_BackButtonClick(object sender, RoutedEventArgs e) => Frame.GoBack();

        private async void Load<T>(int p = -1) where T : IFeedModel, new()
        {
            titleBar.ShowProgressRing();
            JArray array = (JArray)await DataHelper.GetDataAsync(DataUriType.GetNotifications,
                                                            uri,
                                                            p == -1 ? ++page : p,
                                                            firstItem == 0 ? string.Empty : $"&firstItem={firstItem}",
                                                            lastItem == 0 ? string.Empty : $"&lastItem={lastItem}");
            if (array != null && array.Count > 0)
            {
                if (p == 1 || page == 1)
                    firstItem = array.First.Value<int>("id");
                lastItem = array.Last.Value<int>("id");
                var d = (from a in itemCollection
                         from b in array
                         where (a as NotificationModel).Id == b.Value<int>("id")
                         select a).ToArray();
                foreach (var item in d)
                    itemCollection.Remove(item);
                for (int i = 0; i < array.Count; i++)
                {
                    T t = new T();
                    t.Initial((JObject)array[i]);
                    if (p == -1)
                        itemCollection.Add(t);
                    else
                        itemCollection.Insert(i, t);
                }
            }
            else
            {
                var loader = ResourceLoader.GetForViewIndependentUse("NotificationsPage");
                if (p == -1)
                {
                    page--;
                    UIHelper.ShowMessage(loader.GetString("noMore"));
                }
                else
                {
                    UIHelper.ShowMessage(loader.GetString("noNew"));
                }
            }
            titleBar.HideProgressRing();
        }

        private async void Load(int p = -1)
        {
            titleBar.ShowProgressRing();
            JArray array = (JArray)await DataHelper.GetDataAsync(
                                        DataUriType.GetNotifications,
                                        uri,
                                        p == -1 ? ++page : p,
                                        firstItem == 0 ? string.Empty : $"&firstItem={firstItem}",
                                        lastItem == 0 ? string.Empty : $"&lastItem={lastItem}");
            if (array != null && array.Count > 0)
            {
                if (p == 1 || page == 1)
                    firstItem = array.First.Value<int>("id");
                lastItem = array.Last.Value<int>("id");
                var d = (from a in itemCollection
                         from b in array
                         where (a as FeedModel).EntityId == $"{b.Value<int>("id")}"
                         select a).ToArray();
                foreach (var item in d)
                    itemCollection.Remove(item);
                for (int i = 0; i < array.Count; i++)
                {
                    FeedModel item = new FeedModel((JObject)array[i]);
                    if (p == -1)
                        itemCollection.Add(item);
                    else
                        itemCollection.Insert(i, item);
                }
            }
            else
            {
                var loader = ResourceLoader.GetForViewIndependentUse("NotificationsPage");
                if (p == -1)
                {
                    page--;
                    UIHelper.ShowMessage(loader.GetString("noMore"));
                }
                else
                {
                    UIHelper.ShowMessage(loader.GetString("noNew"));
                }
            }
            titleBar.HideProgressRing();
        }

        private void scrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (!e.IsIntermediate && scrollViewer.VerticalOffset == scrollViewer.ScrollableHeight)
            {
                switch (type)
                {
                    case NotificationPageType.Comment: Load<SimpleNotificationModel>(); break;
                    case NotificationPageType.AtMe: Load(); break;
                    case NotificationPageType.AtCommentMe: Load<AtCommentMeNotificationModel>(); break;
                    case NotificationPageType.Like: Load<LikeNotificationModel>(); break;
                    case NotificationPageType.Follow: Load<SimpleNotificationModel>(); break;
                }
            }
        }

        private void MainListView_RefreshRequested(object sender, EventArgs e)
        {
            switch (type)
            {
                case NotificationPageType.Comment: Load<SimpleNotificationModel>(1); break;
                case NotificationPageType.AtMe: Load(1); break;
                case NotificationPageType.AtCommentMe: Load<AtCommentMeNotificationModel>(1); break;
                case NotificationPageType.Like: Load<LikeNotificationModel>(1); break;
                case NotificationPageType.Follow: Load<SimpleNotificationModel>(1); break;
            }
        }
    }

    internal enum NotificationPageType
    {
        Comment,
        AtMe,
        AtCommentMe,
        Like,
        Follow,
    }

    internal class NotificationsPageTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Feed { get; set; }
        public DataTemplate Reply { get; set; }
        public DataTemplate Like { get; set; }
        public DataTemplate AtCommentMe { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            switch (item)
            {
                default: return Reply;
                case FeedModel _: return Feed;
                case LikeNotificationModel _: return Like;
                case AtCommentMeNotificationModel _: return AtCommentMe;
            }
        }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container) => SelectTemplateCore(item);
    }
}