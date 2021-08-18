using CoolapkUWP.Control.ViewModels;
using CoolapkUWP.Data;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using Windows.Data.Json;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
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

    internal interface INotificationViewModel
    {
        double id { get; }
        void Initial(IJsonValue o);
    }

    internal class SimpleNotificationViewModel : INotifyPropertyChanged, INotificationViewModel
    {
        public ImageSource FromUserAvatar { get; private set; }
        public string FromUserName { get; private set; }
        public string FromUserUri { get; private set; }
        public string Dateline { get; private set; }
        public string Uri { get; private set; }
        public string Note { get; private set; }
        public double id { get; private set; }
        public event PropertyChangedEventHandler PropertyChanged;

        public void Initial(IJsonValue o)
        {
            JsonObject token = o.GetObject();

            if (token.TryGetValue("id", out IJsonValue Id))
            {
                id = Id.GetNumber();
            }
            if (token.TryGetValue("fromusername", out IJsonValue fromusername))
            {
                FromUserName = fromusername.GetString();
            }
            if (token.TryGetValue("url", out IJsonValue url))
            {
                FromUserUri = url.GetString();
            }
            if (token.TryGetValue("dateline", out IJsonValue dateline))
            {
                Dateline = UIHelper.ConvertTime(dateline.GetNumber());
            }
            string note = token["note"].GetString();
            Regex regex = new Regex("<a.*?>.*?</a>"), regex2 = new Regex("href=\".*"), regex3 = new Regex(">.*<");
            while (regex.IsMatch(note))
            {
                Match link = regex.Match(note);
                string content = regex3.Match(link.Value).Value.Replace(">", string.Empty);
                content = content.Replace("<", string.Empty);
                string href = regex2.Match(link.Value).Value.Replace("href=\"", string.Empty);
                if (href.IndexOf("\"", StringComparison.Ordinal) > 0)
                {
                    href = href.Substring(0, href.IndexOf("\"", StringComparison.Ordinal));
                }
                Uri = href;
                note = note.Replace(link.Value, content);
            }
            Note = note;
            if (token.TryGetValue("fromUserInfo", out IJsonValue v1))
            {
                JsonObject fromUserInfo = v1.GetObject();
                if (fromUserInfo.TryGetValue("userSmallAvatar", out IJsonValue userSmallAvatar))
                {
                    GetPic(userSmallAvatar.GetString());
                }
            }
            else if (token.TryGetValue("fromUserAvatar", out IJsonValue fromUserAvatar))
            {
                GetPic(fromUserAvatar.GetString());
            }
            if (SettingsHelper.IsSpecialUser && token.TryGetValue("block_status", out IJsonValue block_status) && block_status.GetNumber() != 0)
            { Dateline += " [已折叠]"; }
            if (token.TryGetValue("status", out IJsonValue status) && status.GetNumber() == -1)
            { Dateline += " [仅自己可见]"; }
        }

        private async void GetPic(string u)
        {
            if (!string.IsNullOrEmpty(u))
            {
                FromUserAvatar = ImageCache.defaultNoAvatarUrl.Contains(u) ? ImageCache.NoPic : await ImageCache.GetImage(ImageType.SmallAvatar, u);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FromUserAvatar)));
            }
        }
    }

    internal class LikeNotificationViewModel : INotifyPropertyChanged, INotificationViewModel
    {
        public ImageSource LikeUserAvatar { get; private set; }
        public string LikeUserName { get; private set; }
        public string LikeUserUri { get; private set; }
        public string Dateline { get; private set; }
        public string Uri { get; private set; }
        public string FeedMessage { get; private set; }
        public string Title { get; private set; }
        public double id { get; private set; }
        public event PropertyChangedEventHandler PropertyChanged;

        public void Initial(IJsonValue o)
        {
            JsonObject token = o.GetObject();
            if (token.TryGetValue("id", out IJsonValue Id))
            {
                id = Id.GetNumber();
            }
            if (token.TryGetValue("likeUsername", out IJsonValue likeUsername))
            {
                LikeUserName = likeUsername.GetString();
            }
            if (token.TryGetValue("likeUid", out IJsonValue likeUid))
            {
                LikeUserUri = "/u/" + likeUid.GetNumber();
            }
            if (token.TryGetValue("likeTime", out IJsonValue likeTime))
            {
                Dateline = UIHelper.ConvertTime(likeTime.GetNumber());
            }
            if (token.TryGetValue("url", out IJsonValue url))
            {
                Uri = url.GetString();
            }
            if (token.TryGetValue("likeAvatar", out IJsonValue likeAvatar))
            {
                GetPic(likeAvatar.GetString());
            }
            if (token.TryGetValue("feedTypeName", out IJsonValue feedTypeName))
            {
                Title = "赞了你的" + feedTypeName.GetString();
            }
            else if (token.TryGetValue("infoHtml", out IJsonValue infoHtml))
            {
                Title = "赞了你的" + infoHtml.GetString();
            }
            if (token.TryGetValue("message", out IJsonValue message))
            {
                FeedMessage = message.GetString();
            }
        }

        private async void GetPic(string u)
        {
            if (!string.IsNullOrEmpty(u))
            {
                LikeUserAvatar = ImageCache.defaultNoAvatarUrl.Contains(u) ? ImageCache.NoPic : await ImageCache.GetImage(ImageType.BigAvatar, u);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LikeUserAvatar)));
            }
        }
    }

    internal class AtCommentMeNotificationViewModel : INotifyPropertyChanged, INotificationViewModel
    {
        public ImageSource UserAvatar { get; private set; }
        public string UserName { get; private set; }
        public string UserUri { get; private set; }
        public string Dateline { get; private set; }
        public string Uri { get; private set; }
        public string Message { get; private set; }
        public string FeedMessage { get; private set; }
        public double id { get; private set; }
        public event PropertyChangedEventHandler PropertyChanged;

        public void Initial(IJsonValue o)
        {
            JsonObject token = o.GetObject();
            id = token["id"].GetNumber();

            UserName = token["username"].GetString();
            UserUri = "/u/" + token["uid"].GetNumber();
            Dateline = UIHelper.ConvertTime(token["dateline"].GetNumber());
            Uri = token["url"].GetString();
            GetPic(token["userAvatar"].GetString());
            Message = (string.IsNullOrEmpty(token["rusername"].GetString()) ? string.Empty : $"回复<a href=\"/u/{token["ruid"].GetNumber()}\">{token["rusername"].GetString()}</a>: ") + token["message"].GetString();
            FeedMessage = (token["extra_title"].GetString());
        }

        private async void GetPic(string u)
        {
            if (!string.IsNullOrEmpty(u))
            {
                UserAvatar = ImageCache.defaultNoAvatarUrl.Contains(u) ? ImageCache.NoPic : await ImageCache.GetImage(ImageType.BigAvatar, u);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UserAvatar)));
            }
        }
    }

    internal class MessageNotificationModel : INotifyPropertyChanged, INotificationViewModel
    {
        public ImageSource UserAvatar { get; private set; }
        public string UserName { get; private set; }
        public string UserUri { get; private set; }
        public string Dateline { get; private set; }
        public string Uri { get; private set; }
        public string FeedMessage { get; private set; }
        public double id { get; private set; }
        public event PropertyChangedEventHandler PropertyChanged;

        public void Initial(IJsonValue o)
        {
            JsonObject token = o.GetObject();
            if (token.TryGetValue("id", out IJsonValue Id))
            {
                id = Id.GetNumber();
                UserUri = "/u/" + id;
            }
            if (token.TryGetValue("dateline", out IJsonValue dateline))
            {
                Dateline = UIHelper.ConvertTime(dateline.GetNumber());
            }
            if (token.TryGetValue("messageUserInfo", out IJsonValue v1))
            {
                JsonObject messageUserInfo = v1.GetObject();
                if (messageUserInfo.TryGetValue("username", out IJsonValue username))
                {
                    UserName = username.GetString();
                }
                if (messageUserInfo.TryGetValue("userAvatar", out IJsonValue userAvatar))
                {
                    string avatar = userAvatar.GetString();
                    if (!string.IsNullOrEmpty(avatar))
                    {
                        GetPic(avatar);
                    }
                }
            }
            if (token.TryGetValue("ukey", out IJsonValue ukey))
            {
                Uri = ukey.GetString();
            }
            if (token.TryGetValue("message", out IJsonValue message))
            {
                FeedMessage = message.GetString();
            }
            if (token.TryGetValue("is_top", out IJsonValue is_top) && is_top.GetNumber() == 1)
            {
                Dateline += " " + "[置顶]";
            }
        }

        private async void GetPic(string u)
        {
            if (!string.IsNullOrEmpty(u))
            {
                UserAvatar = ImageCache.defaultNoAvatarUrl.Contains(u) ? ImageCache.NoPic : await ImageCache.GetImage(ImageType.BigAvatar, u);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UserAvatar)));
            }
        }
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
                    Load<SimpleNotificationViewModel>();
                    break;
                case NotificationPageType.AtMe:
                    setPageStyle("@我的动态");
                    Load();
                    break;
                case NotificationPageType.AtCommentMe:
                    setPageStyle("@我的评论");
                    uri = "atCommentMeList";
                    Load<AtCommentMeNotificationViewModel>();
                    break;
                case NotificationPageType.Like:
                    setPageStyle("我收到的赞");
                    uri = "feedLikeList";
                    Load<LikeNotificationViewModel>();
                    break;
                case NotificationPageType.Follow:
                    setPageStyle("好友关注");
                    uri = "contactsFollowList";
                    Load<SimpleNotificationViewModel>();
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
                            case NotificationPageType.Like: Load<LikeNotificationViewModel>(); break;
                            case NotificationPageType.Message: Load<MessageNotificationModel>(); break;
                            case NotificationPageType.Follow: Load<SimpleNotificationViewModel>(); break;
                            case NotificationPageType.Comment: Load<SimpleNotificationViewModel>(); break;
                            case NotificationPageType.AtCommentMe: Load<AtCommentMeNotificationViewModel>(); break;
                            default: break;
                        }
                        VScrollViewer.ChangeView(null, VScrollViewer.VerticalOffset - 1, null);
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

        private async void Load<T>(int p = -1) where T : INotificationViewModel, new()
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
                                  where (a as INotificationViewModel).id == b.GetObject()["id"].GetNumber()
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
                                  where (a as FeedViewModel).entityId == b.GetObject()["id"].GetNumber().ToString()
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
                case NotificationPageType.Like: Load<LikeNotificationViewModel>(1); break;
                case NotificationPageType.Message: Load<MessageNotificationModel>(1); break;
                case NotificationPageType.Follow: Load<SimpleNotificationViewModel>(1); break;
                case NotificationPageType.Comment: Load<SimpleNotificationViewModel>(1); break;
                case NotificationPageType.AtCommentMe: Load<AtCommentMeNotificationViewModel>(1); break;
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
            return item is FeedViewModel ? FeedViewModel : item is LikeNotificationViewModel ? LikeNotificationViewModel : item is AtCommentMeNotificationViewModel ? AtCommentMeNotificationViewModel : item is MessageNotificationModel ? MessageNotificationModel : SimpleNotificationViewModel;
        }
        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container) => SelectTemplateCore(item);
    }
}
