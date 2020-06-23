using CoolapkUWP.Helpers;
using CoolapkUWP.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace CoolapkUWP.Pages.FeedPages
{
    public sealed partial class UserListPage : Page
    {
        private bool isFollowList;
        private string uid;
        private int page = 1;
        private double firstItem, lastItem;
        private readonly ObservableCollection<UserModel> infos = new ObservableCollection<UserModel>();

        public UserListPage() => this.InitializeComponent();

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            object[] vs = e.Parameter as object[];
            if (uid != vs[0] as string || (bool)vs[1] != isFollowList)
            {
                uid = vs[0] as string;
                isFollowList = (bool)vs[1];
                titleBar.Title = $"{vs[2] as string}的{(isFollowList ? "关注" : "粉丝")}";
                firstItem = lastItem = 0;
                infos.Clear();
                page = 1;
                LoadList(1);
            }
        }

        private async void LoadList(int p = -1)
        {
            titleBar.ShowProgressRing();
            JArray array = (JArray)await DataHelper.GetDataAsync(DataUriType.GetUserList,
                                                            isFollowList ? "followList" : "fansList",
                                                            uid,
                                                            p == -1 ? ++page : p,
                                                            firstItem == 0 ? string.Empty : $"&firstItem={firstItem}",
                                                            lastItem == 0 ? string.Empty : $"&lastItem={lastItem}");
            if (array != null && array.Count > 0)
            {
                if (p == 1 || page == 1)
                    firstItem = array.First.Value<int>("fuid");
                lastItem = array.Last.Value<int>("fuid");
                var d = (from a in infos
                         from b in array
                         where a.UserName == b.Value<string>(isFollowList ? "fusername" : "username")
                         select a).ToArray();
                foreach (var item in d)
                    infos.Remove(item);
                if (p == -1)
                    for (int i = 0; i < array.Count; i++)
                        infos.Add(new UserModel((JObject)(isFollowList ? array[i]["fUserInfo"] : array[i]["userInfo"])));
                else
                    for (int i = 0; i < array.Count; i++)
                        infos.Insert(i, new UserModel((JObject)(isFollowList ? array[i]["fUserInfo"] : array[i]["userInfo"])));
            }
            else if (p == -1)
            {
                page--;
            }

            titleBar.HideProgressRing();
        }

        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (!e.IsIntermediate && scrollViewer.VerticalOffset == scrollViewer.ScrollableHeight)
            {
                LoadList();
            }
        }

        private void TitleBar_RefreshButtonClick(object sender, RoutedEventArgs e)
        {
            LoadList(1);
            scrollViewer.ChangeView(null, 0, null);
        }

        private void TitleBar_BackButtonClick(object sender, RoutedEventArgs e) => Frame.GoBack();
    }
}