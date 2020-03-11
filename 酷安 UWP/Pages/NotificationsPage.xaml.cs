using CoolapkUWP.Control.ViewModels;
using CoolapkUWP.Data;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace CoolapkUWP.Pages
{
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
        double id { get; }
        void Initial(IJsonValue o);
    }
    class SimpleNotificationViewModel : INotifyPropertyChanged, INotificationViewModel
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
            id = token["id"].GetNumber();
            FromUserName = token["fromusername"].GetString();
            FromUserUri = token["url"].GetString();
            Dateline = Tools.ConvertTime(token["dateline"].GetNumber());
            GetPic(token["fromUserInfo"].GetObject()["userSmallAvatar"].GetString());
            Regex regex = new Regex("<a.*?>.*?</a>"), regex2 = new Regex("href=\".*"), regex3 = new Regex(">.*<");
            string s = token["note"].GetString();
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
                FromUserAvatar = await ImageCache.GetImage(ImageType.SmallAvatar, u);
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
        public double id { get; private set; }
        public event PropertyChangedEventHandler PropertyChanged;

        public void Initial(IJsonValue o)
        {
            JsonObject token = o.GetObject();
            id = token["id"].GetNumber();

            LikeUserName = token["likeUsername"].GetString();
            LikeUserUri = "/u/" + token["likeUid"].GetNumber();
            Dateline = Tools.ConvertTime(token["likeTime"].GetNumber());
            Uri = token["url"].GetString();
            GetPic(token["likeAvatar"].GetString());
            Title = "赞了你的" + (token.TryGetValue("feedTypeName", out IJsonValue value) ? value.GetString() : token["infoHtml"].GetString());
            FeedMessage = token["message"].GetString();
        }
        async void GetPic(string u)
        {
            if (!string.IsNullOrEmpty(u))
            {
                LikeUserAvatar = await ImageCache.GetImage(ImageType.BigAvatar, u);
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
        public double id { get; private set; }
        public event PropertyChangedEventHandler PropertyChanged;

        public void Initial(IJsonValue o)
        {
            JsonObject token = o.GetObject();
            id = token["id"].GetNumber();

            UserName = token["username"].GetString();
            UserUri = "/u/" + token["uid"].GetNumber();
            Dateline = Tools.ConvertTime(token["dateline"].GetNumber());
            Uri = token["url"].GetString();
            GetPic(token["userAvatar"].GetString());
            Message = (string.IsNullOrEmpty(token["rusername"].GetString()) ? string.Empty : $"回复<a href=\"/u/{token["ruid"].GetNumber()}\">{token["rusername"].GetString()}</a>: ") + token["message"].GetString();
            FeedMessage = (token["extra_title"].GetString());
        }
        async void GetPic(string u)
        {
            if (!string.IsNullOrEmpty(u))
            {
                UserAvatar = await ImageCache.GetImage(ImageType.BigAvatar, u);
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
                    MainListView.Padding = Settings.stackPanelMargin;
                    Load();
                    break;
                case NotificationPageType.AtCommentMe:
                    titleBar.Title = ("@我的评论");
                    MainListView.Padding = Settings.stackPanelMargin;
                    uri = "atCommentMeList";
                    Load<AtCommentMeNotificationViewModel>();
                    break;
                case NotificationPageType.Like:
                    titleBar.Title = ("我收到的赞");
                    MainListView.Padding = Settings.stackPanelMargin;
                    uri = "feedLikeList";
                    Load<LikeNotificationViewModel>();
                    break;
                case NotificationPageType.Follow:
                    titleBar.Title = ("好友关注");
                    MainListView.Padding = Settings.stackPanelMargin;
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
                    Tools.Navigate(typeof(NotificationsPage), NotificationPageType.AtMe);
                    break;
                case "atCommentMe":
                    Tools.Navigate(typeof(NotificationsPage), NotificationPageType.AtCommentMe);
                    break;
                case "like":
                    Tools.Navigate(typeof(NotificationsPage), NotificationPageType.Like);
                    break;
                case "follow":
                    Tools.Navigate(typeof(NotificationsPage), NotificationPageType.Follow);
                    break;
            }
        }

        private void Ellipse_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (e.OriginalSource is Windows.UI.Xaml.Shapes.Ellipse
                || (sender is Grid && !(e.OriginalSource is Windows.UI.Xaml.Shapes.Ellipse)))
                Tools.OpenLink((sender as FrameworkElement).Tag as string);
        }

        double firstItem, lastItem;
        int page;
        private void TitleBar_BackButtonClick(object sender, RoutedEventArgs e) => Frame.GoBack();

        async void Load<T>(int p = -1) where T : INotificationViewModel, new()
        {
            Tools.ShowProgressBar();
            JsonArray array = Tools.GetDataArray(await Tools.GetJson($"/notification/{uri}?page={(p == -1 ? ++page : p)}{(firstItem == 0 ? string.Empty : $"&firstItem={firstItem}")}{(lastItem == 0 ? string.Empty : $"&lastItem={lastItem}")}"));
            if (array != null && array.Count > 0)
            {
                if (p == 1 || page == 1)
                    firstItem = array.First().GetObject()["id"].GetNumber();
                lastItem = array.Last().GetObject()["id"].GetNumber();
                var d = (from a in itemCollection
                         from b in array
                         where (a as INotificationViewModel).id == b.GetObject()["id"].GetNumber()
                         select a).ToArray();
                foreach (var item in d)
                    itemCollection.Remove(item);
                for (int i = 0; i < array.Count; i++)
                {
                    T t = new T();
                    t.Initial(array[i].GetObject());
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
                    Tools.ShowMessage("没有更多了");
                }
                else Tools.ShowMessage("没有新的了");
            }
            Tools.HideProgressBar();
        }

        async void Load(int p = -1)
        {
            Tools.ShowProgressBar();
            JsonArray array = Tools.GetDataArray(await Tools.GetJson($"/notification/atMeList?page={(p == -1 ? ++page : p)}{(firstItem == 0 ? string.Empty : $"&firstItem={firstItem}")}{(lastItem == 0 ? string.Empty : $"&lastItem={lastItem}")}"));
            if (array != null && array.Count > 0)
            {
                if (p == 1 || page == 1)
                    firstItem = array.First().GetObject()["id"].GetNumber();
                lastItem = array.Last().GetObject()["id"].GetNumber();
                var d = (from a in itemCollection
                         from b in array
                         where (a as FeedViewModel).entityId == b.GetObject()["id"].GetNumber().ToString()
                         select a).ToArray();
                foreach (var item in d)
                    itemCollection.Remove(item);
                for (int i = 0; i < array.Count; i++)
                {
                    if (p == -1)
                        itemCollection.Add(new FeedViewModel(array[i]));
                    else
                        itemCollection.Insert(i, new FeedViewModel(array[i]));
                }
            }
            else
            {
                if (p == -1)
                {
                    page--;
                    Tools.ShowMessage("没有更多了");
                }
                else Tools.ShowMessage("没有新的了");
            }
            Tools.HideProgressBar();
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

    class TemplateSelector : DataTemplateSelector
    {
        public DataTemplate DataTemplate1 { get; set; }
        public DataTemplate DataTemplate2 { get; set; }
        public DataTemplate DataTemplate3 { get; set; }
        public DataTemplate DataTemplate4 { get; set; }
        protected override DataTemplate SelectTemplateCore(object item)
        {
            if (item is FeedViewModel) return DataTemplate1;
            if (item is LikeNotificationViewModel) return DataTemplate3;
            if (item is AtCommentMeNotificationViewModel) return DataTemplate4;
            else return DataTemplate2;
        }
        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container) => SelectTemplateCore(item);
    }
}
