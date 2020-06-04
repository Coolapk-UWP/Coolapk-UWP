using CoolapkUWP.Controls.ViewModels;
using CoolapkUWP.Helpers;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace CoolapkUWP.Pages.FeedPages
{
    public sealed partial class NotificationsPage : Page
    {
        readonly ObservableCollection<object> itemCollection = new ObservableCollection<object>();
        NotificationPageType type;
        public NotificationsPage() => this.InitializeComponent();
        string uri;

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            type = (NotificationPageType)e.Parameter;
            switch (type)
            {
                case NotificationPageType.Comment:
                    FindName(nameof(NavigateItems));
                    titleBar.Title = ("通知");
                    uri = "list";
                    Load<SimpleNotificationViewModel>();
                    break;
                case NotificationPageType.AtMe:
                    titleBar.Title = ("@我的动态");
                    MainListView.Padding = SettingsHelper.StackPanelMargin;
                    uri = "atMeList";
                    Load();
                    break;
                case NotificationPageType.AtCommentMe:
                    titleBar.Title = ("@我的评论");
                    MainListView.Padding = SettingsHelper.StackPanelMargin;
                    uri = "atCommentMeList";
                    Load<AtCommentMeNotificationViewModel>();
                    break;
                case NotificationPageType.Like:
                    titleBar.Title = ("我收到的赞");
                    MainListView.Padding = SettingsHelper.StackPanelMargin;
                    uri = "feedLikeList";
                    Load<LikeNotificationViewModel>();
                    break;
                case NotificationPageType.Follow:
                    titleBar.Title = ("好友关注");
                    MainListView.Padding = SettingsHelper.StackPanelMargin;
                    uri = "contactsFollowList";
                    Load<SimpleNotificationViewModel>();
                    break;
            }
            await System.Threading.Tasks.Task.Delay(2000);
            (VisualTree.FindDescendantByName(MainListView, "ScrollViewer") as ScrollViewer).ViewChanged += (s, ee) =>
            {
                ScrollViewer scrollViewer = s as ScrollViewer;
                if (!ee.IsIntermediate)
                {
                    double a = scrollViewer.VerticalOffset;
                    if (a == scrollViewer.ScrollableHeight)
                        switch (type)
                        {
                            case NotificationPageType.Comment: Load<SimpleNotificationViewModel>(); break;
                            case NotificationPageType.AtMe: Load(); break;
                            case NotificationPageType.AtCommentMe: Load<AtCommentMeNotificationViewModel>(); break;
                            case NotificationPageType.Like: Load<LikeNotificationViewModel>(); break;
                            case NotificationPageType.Follow: Load<SimpleNotificationViewModel>(); break;
                        }
                }
            };
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
            }
        }

        private void Ellipse_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (e.OriginalSource is Windows.UI.Xaml.Shapes.Ellipse
                || (sender is Grid && !(e.OriginalSource is Windows.UI.Xaml.Shapes.Ellipse)))
                UIHelper.OpenLink((sender as FrameworkElement).Tag as string);
        }

        double firstItem, lastItem;
        int page;
        private void TitleBar_BackButtonClick(object sender, RoutedEventArgs e) => Frame.GoBack();

        async void Load<T>(int p = -1) where T : INotificationViewModel, new()
        {
            UIHelper.ShowProgressBar();
            JArray array = (JArray)await DataHelper.GetData(DataUriType.GetNotifications,
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
                         where (a as INotificationViewModel).Id == b.Value<int>("id")
                         select a).ToArray();
                foreach (var item in d)
                    itemCollection.Remove(item);
                for (int i = 0; i < array.Count; i++)
                {
                    T t = new T();
                    t.Initial(array[i]);
                    if (p == -1)
                        itemCollection.Add(t);
                    else
                        itemCollection.Insert(i, t);
                }
            }
            else
            {
                if (p == -1)
                {
                    page--;
                    UIHelper.ShowMessage("没有更多了");
                }
                else UIHelper.ShowMessage("没有新的了");
            }
            UIHelper.HideProgressBar();
        }

        async void Load(int p = -1)
        {
            UIHelper.ShowProgressBar();
            JArray array = (JArray)await DataHelper.GetData(DataUriType.GetNotifications,
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
                         where (a as FeedViewModel).EntityId == b.Value<int>("id").ToString()
                         select a).ToArray();
                foreach (var item in d)
                    itemCollection.Remove(item);
                for (int i = 0; i < array.Count; i++)
                {
                    if (p == -1)
                        itemCollection.Add(new FeedViewModel((JObject)array[i]));
                    else
                        itemCollection.Insert(i, new FeedViewModel((JObject)array[i]));
                }
            }
            else
            {
                if (p == -1)
                {
                    page--;
                    UIHelper.ShowMessage("没有更多了");
                }
                else UIHelper.ShowMessage("没有新的了");
            }
            UIHelper.HideProgressBar();
        }

        private void MainListView_RefreshRequested(object sender, EventArgs e)
        {
            switch (type)
            {
                case NotificationPageType.Comment: Load<SimpleNotificationViewModel>(1); break;
                case NotificationPageType.AtMe: Load(1); break;
                case NotificationPageType.AtCommentMe: Load<AtCommentMeNotificationViewModel>(1); break;
                case NotificationPageType.Like: Load<LikeNotificationViewModel>(1); break;
                case NotificationPageType.Follow: Load<SimpleNotificationViewModel>(1); break;
            }
        }
    }

    enum NotificationPageType
    {
        Comment,
        AtMe,
        AtCommentMe,
        Like,
        Follow,
    }
    
    interface INotificationViewModel
    {
        double Id { get; }
        void Initial(JToken o);
    }
    
    class SimpleNotificationViewModel : INotifyPropertyChanged, INotificationViewModel
    {
        public ImageSource FromUserAvatar { get; private set; }
        public string FromUserName { get; private set; }
        public string FromUserUri { get; private set; }
        public string Dateline { get; private set; }
        public string Uri { get; private set; }
        public string Note { get; private set; }
        public double Id { get; private set; }
        public event PropertyChangedEventHandler PropertyChanged;

        public void Initial(JToken o)
        {
            JObject token = o as JObject;
            Id = token.Value<int>("id");
            FromUserName = token.Value<string>("fromusername");
            FromUserUri = token.Value<string>("url");
            Dateline = DataHelper.ConvertTime(token.Value<int>("dateline"));
            GetPic(token["fromUserInfo"].Value<string>("userSmallAvatar"));
            string s = token.Value<string>("note");
            Regex regex = new Regex("<a.*?>.*?</a>"), regex2 = new Regex("href=\".*"), regex3 = new Regex(">.*<");
            while (regex.IsMatch(s))
            {
                var h = regex.Match(s);
                string t = regex3.Match(h.Value).Value.Replace(">", string.Empty);
                t = t.Replace("<", string.Empty);
                string tt = regex2.Match(h.Value).Value.Replace("href=\"", string.Empty);
                if (tt.IndexOf("\"") > 0) tt = tt.Substring(0, tt.IndexOf("\""));
                Uri = tt;
                s = s.Replace(h.Value, t);
            }
            Note = s;
        }
        async void GetPic(string u)
        {
            if (!string.IsNullOrEmpty(u))
            {
                FromUserAvatar = await ImageCacheHelper.GetImage(ImageType.SmallAvatar, u);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FromUserAvatar)));
            }
        }
    }

    class LikeNotificationViewModel : INotifyPropertyChanged, INotificationViewModel
    {
        public ImageSource LikeUserAvatar { get; private set; }
        public string LikeUserName { get; private set; }
        public string LikeUserUri { get; private set; }
        public string Dateline { get; private set; }
        public string Uri { get; private set; }
        public string FeedMessage { get; private set; }
        public string Title { get; private set; }
        public double Id { get; private set; }
        public event PropertyChangedEventHandler PropertyChanged;

        public void Initial(JToken o)
        {
            JObject token = o as JObject;
            Id = token.Value<int>("id");
            LikeUserUri = "/u/" + token.Value<int>("likeUid");
            Dateline = DataHelper.ConvertTime(token.Value<int>("likeTime"));
            LikeUserName = token.Value<string>("likeUsername");
            Uri = token.Value<string>("url");
            GetPic(token.Value<string>("likeAvatar"));
            Title = "赞了你的" + (token.TryGetValue("feedTypeName", out JToken value) ? value.ToString() : token.Value<string>("infoHtml"));
            FeedMessage = token.Value<string>("message");
        }
        async void GetPic(string u)
        {
            if (!string.IsNullOrEmpty(u))
            {
                LikeUserAvatar = await ImageCacheHelper.GetImage(ImageType.BigAvatar, u);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LikeUserAvatar)));
            }
        }
    }

    class AtCommentMeNotificationViewModel : INotifyPropertyChanged, INotificationViewModel
    {
        public ImageSource UserAvatar { get; private set; }
        public string UserName { get; private set; }
        public string UserUri { get; private set; }
        public string Dateline { get; private set; }
        public string Uri { get; private set; }
        public string Message { get; private set; }
        public string FeedMessage { get; private set; }
        public double Id { get; private set; }
        public event PropertyChangedEventHandler PropertyChanged;

        public void Initial(JToken o)
        {
            JObject token = o as JObject;
            Id = token.Value<int>("id");
            UserUri = "/u/" + token.Value<int>("uid");
            Dateline = DataHelper.ConvertTime(token.Value<int>("dateline"));
            UserName = token.Value<string>("username");
            Uri = token.Value<string>("url");
            GetPic(token.Value<string>("userAvatar"));
            FeedMessage = (token.Value<string>("extra_title"));
            Message = (string.IsNullOrEmpty(token.Value<string>("rusername")) ? string.Empty : $"回复<a href=\"/u/{token.Value<string>("ruid")}\">{token.Value<string>("rusername")}</a>: ") + token.Value<string>("message");
        }
        async void GetPic(string u)
        {
            if (!string.IsNullOrEmpty(u))
            {
                UserAvatar = await ImageCacheHelper.GetImage(ImageType.BigAvatar, u);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UserAvatar)));
            }
        }
    }

    class NotificationsPageTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Feed { get; set; }
        public DataTemplate Reply { get; set; }
        public DataTemplate Like { get; set; }
        public DataTemplate AtCommentMe { get; set; }
        protected override DataTemplate SelectTemplateCore(object item)
        {
            if (item is FeedViewModel) return Feed;
            if (item is LikeNotificationViewModel) return Like;
            if (item is AtCommentMeNotificationViewModel) return AtCommentMe;
            else return Reply;
        }
        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container) => SelectTemplateCore(item);
    }
}
