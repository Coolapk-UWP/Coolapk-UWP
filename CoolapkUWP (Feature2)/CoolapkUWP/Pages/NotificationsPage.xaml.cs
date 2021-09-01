using CoolapkUWP.Control.ViewModels;
using CoolapkUWP.Data;
using CoolapkUWP.Models.Pages;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Data.Json;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace CoolapkUWP.Pages
{
    internal enum NotificationPageType
    {
        Comment,
        AtMe,
        AtCommentMe,
        Like,
        Follow,
        Message
    }

    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class NotificationsPage : Page
    {
        ObservableCollection<object> itemCollection = new ObservableCollection<object>();
        NotificationPageType type;
        public NotificationsPage() => InitializeComponent();

        private string uri;

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            type = (NotificationPageType)e.Parameter;
            switch (type)
            {
                case NotificationPageType.Comment:
                    _ = FindName(nameof(NavigateItems));
                    uri = "list";
                    Load<SimpleNotificationModel>();
                    break;
                case NotificationPageType.AtMe:
                    setPageStyle("@我的动态");
                    Load();
                    break;
                case NotificationPageType.AtCommentMe:
                    setPageStyle("@我的评论");
                    uri = "atCommentMeList";
                    Load<AtCommentMeNotificationModel>();
                    break;
                case NotificationPageType.Like:
                    setPageStyle("我收到的赞");
                    uri = "feedLikeList";
                    Load<LikeNotificationModel>();
                    break;
                case NotificationPageType.Follow:
                    setPageStyle("好友关注");
                    uri = "contactsFollowList";
                    Load<SimpleNotificationModel>();
                    break;
                case NotificationPageType.Message:
                    setPageStyle("私信");
                    uri = "messageList";
                    Load<MessageNotificationModel>();
                    break;
                default:
                    break;
            }
            void setPageStyle(string t)
            {
                MainListView.Padding = SettingsHelper.StackPanelMargin;
                _ = FindName(nameof(titleBar));
                titleBar.Title = t;
            }
            await System.Threading.Tasks.Task.Delay(1000);
            (VisualTree.FindDescendantByName(MainListView, "ScrollViewer") as ScrollViewer).ViewChanged += ScrollViewer_ViewChanged;
        }

        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            ScrollViewer VScrollViewer = sender as ScrollViewer;
            if (!e.IsIntermediate)
            {
                if (itemCollection.Count != 0)
                {
                    if (VScrollViewer.VerticalOffset >= VScrollViewer.ScrollableHeight)
                    {
                        switch (type)
                        {
                            case NotificationPageType.AtMe: Load(); break;
                            case NotificationPageType.Like: Load<LikeNotificationModel>(); break;
                            case NotificationPageType.Message: Load<MessageNotificationModel>(); break;
                            case NotificationPageType.Follow: Load<SimpleNotificationModel>(); break;
                            case NotificationPageType.Comment: Load<SimpleNotificationModel>(); break;
                            case NotificationPageType.AtCommentMe: Load<AtCommentMeNotificationModel>(); break;
                            default: break;
                        }
                        _ = VScrollViewer.ChangeView(null, VScrollViewer.VerticalOffset - 1, null);
                    }
                }
            }
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            StackPanel item = e.ClickedItem as StackPanel;
            switch (item.Tag as string)
            {
                case "atMe":
                    UIHelper.Navigate(typeof(NotificationsPage), NotificationPageType.AtMe);
                    break;
                case "atCommentMe":
                    UIHelper.Navigate(typeof(NotificationsPage), NotificationPageType.AtCommentMe);
                    break;
                case "like":
                    UIHelper.Navigate(typeof(NotificationsPage), NotificationPageType.Like);
                    break;
                case "follow":
                    UIHelper.Navigate(typeof(NotificationsPage), NotificationPageType.Follow);
                    break;
                case "message":
                    UIHelper.Navigate(typeof(NotificationsPage), NotificationPageType.Message);
                    break;
                default:
                    break;
            }
        }

        private void Ellipse_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (e.OriginalSource is Windows.UI.Xaml.Shapes.Ellipse
                || (sender is Grid && !(e.OriginalSource is Windows.UI.Xaml.Shapes.Ellipse)))
            { UIHelper.OpenLink((sender as FrameworkElement).Tag as string); }
        }

        private void OnTapped(object sender, TappedRoutedEventArgs e)
            => UIHelper.OpenLink((sender as FrameworkElement).Tag as string);

        private double firstItem, lastItem;
        private int page;
        private void TitleBar_BackButtonClick(object sender, RoutedEventArgs e) => Frame.GoBack();

        private async void Load<T>(int p = -1) where T : INotificationModel, new()
        {
            UIHelper.ShowProgressBar();
            string json = string.Empty;

            if (uri == "messageList")
            {
                json = await UIHelper.GetJson($"/message/list?page={(p == -1 ? ++page : p)}{(firstItem == 0 ? string.Empty : $"&firstItem={firstItem}")}{(lastItem == 0 ? string.Empty : $"&lastItem={lastItem}")}");
            }
            else
            {
                json = await UIHelper.GetJson($"/notification/{uri}?page={(p == -1 ? ++page : p)}{(firstItem == 0 ? string.Empty : $"&firstItem={firstItem}")}{(lastItem == 0 ? string.Empty : $"&lastItem={lastItem}")}");
            }

            if (!string.IsNullOrEmpty(json))
            {
                JsonArray array = UIHelper.GetDataArray(json);
                if (array != null && array.Count > 0)
                {
                    if (p == 1 || page == 1)
                    { firstItem = array.First().GetObject()["id"].GetNumber(); }
                    lastItem = array.Last().GetObject()["id"].GetNumber();
                    object[] d = (from a in itemCollection
                                  from b in array
                                  where (a as INotificationModel).id == b.GetObject()["id"].GetNumber()
                                  select a).ToArray();
                    foreach (object item in d)
                    { itemCollection.Remove(item); }
                    for (int i = 0; i < array.Count; i++)
                    {
                        T t = new T();
                        t.Initial(array[i].GetObject());
                        if (p == -1) { itemCollection.Add(t); }
                        else { itemCollection.Insert(i, t); }
                    }
                }
                else
                {
                    if (p == -1)
                    {
                        page--;
                        UIHelper.ShowMessage("没有更多了");
                    }
                    else { UIHelper.ShowMessage("没有新的了"); }
                }
                UIHelper.HideProgressBar();
            }
            else
            {
                UIHelper.ErrorProgressBar();
            }
        }

        private async void Load(int p = -1)
        {
            UIHelper.ShowProgressBar();
            string json = await UIHelper.GetJson($"/notification/atMeList?page={(p == -1 ? ++page : p)}{(firstItem == 0 ? string.Empty : $"&firstItem={firstItem}")}{(lastItem == 0 ? string.Empty : $"&lastItem={lastItem}")}");
            if (!string.IsNullOrEmpty(json))
            {
                JsonArray array = UIHelper.GetDataArray(json);
                if (array != null && array.Count > 0)
                {
                    if (p == 1 || page == 1)
                    { firstItem = array.First().GetObject()["id"].GetNumber(); }
                    lastItem = array.Last().GetObject()["id"].GetNumber();
                    object[] d = (from a in itemCollection
                                  from b in array
                                  where (a as FeedViewModel).EntityId == b.GetObject()["id"].GetNumber().ToString()
                                  select a).ToArray();
                    foreach (object item in d) { _ = itemCollection.Remove(item); }
                    for (int i = 0; i < array.Count; i++)
                    {
                        if (p == -1) { itemCollection.Add(new FeedViewModel(array[i])); }
                        else { itemCollection.Insert(i, new FeedViewModel(array[i])); }
                    }
                }
                else
                {
                    if (p == -1)
                    {
                        page--;
                        UIHelper.ShowMessage("没有更多了");
                    }
                    else { UIHelper.ShowMessage("没有新的了"); }
                }
                UIHelper.HideProgressBar();
            }
            else
            {
                UIHelper.ErrorProgressBar();
            }
        }

        private void MainListView_RefreshRequested(object sender, EventArgs e)
        {
            switch (type)
            {
                case NotificationPageType.AtMe: Load(1); break;
                case NotificationPageType.Like: Load<LikeNotificationModel>(1); break;
                case NotificationPageType.Message: Load<MessageNotificationModel>(1); break;
                case NotificationPageType.Follow: Load<SimpleNotificationModel>(1); break;
                case NotificationPageType.Comment: Load<SimpleNotificationModel>(1); break;
                case NotificationPageType.AtCommentMe: Load<AtCommentMeNotificationModel>(1); break;
                default: break;
            }
        }
    }

    internal class TemplateSelector : DataTemplateSelector
    {
        public DataTemplate FeedViewModel { get; set; }
        public DataTemplate MessageNotificationModel { get; set; }
        public DataTemplate LikeNotificationViewModel { get; set; }
        public DataTemplate SimpleNotificationViewModel { get; set; }
        public DataTemplate AtCommentMeNotificationViewModel { get; set; }
        protected override DataTemplate SelectTemplateCore(object item)
        {
            return item is FeedViewModel ? FeedViewModel : item is LikeNotificationModel ? LikeNotificationViewModel : item is AtCommentMeNotificationModel ? AtCommentMeNotificationViewModel : item is MessageNotificationModel ? MessageNotificationModel : SimpleNotificationViewModel;
        }
        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container) => SelectTemplateCore(item);
    }
}
