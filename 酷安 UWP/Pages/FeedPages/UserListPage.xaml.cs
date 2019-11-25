using CoolapkUWP.Control.ViewModels;
using CoolapkUWP.Data;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace CoolapkUWP.Pages.FeedPages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class UserListPage : Page
    {
        bool isFollowList;
        string uid;
        int page = 1;
        double firstItem, lastItem;
        ObservableCollection<UserViewModel> infos = new ObservableCollection<UserViewModel>();
        ScrollViewer VScrollViewer = null;

        public UserListPage()
        {
            this.InitializeComponent();
            UserList.ItemsSource = infos;
            Task.Run(async () =>
            {
                await Task.Delay(300);
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    VScrollViewer = VisualTree.FindDescendantByName(UserList, "ScrollViewer") as ScrollViewer;
                    VScrollViewer.ViewChanged += ScrollViewer_ViewChanged;
                });
            });
        }

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

        async void LoadList(int p = -1)
        {
            Tools.ShowProgressBar();
            JsonArray array = Tools.GetDataArray(await Tools.GetJson($"/user/{(isFollowList ? "followList" : "fansList")}?uid={uid}&page={(p == -1 ? ++page : p)}{(firstItem == 0 ? string.Empty : $"&firstItem={firstItem}")}{(lastItem == 0 ? string.Empty : $"&lastItem={lastItem}")}"));
            if (array != null && array.Count > 0)
            {
                if (p == 1 || page == 1)
                    firstItem = array.First().GetObject()["fuid"].GetNumber();
                lastItem = array.Last().GetObject()["fuid"].GetNumber();
                var d = (from a in infos
                         from b in array
                         where a.UserName == b.GetObject()[isFollowList ? "fusername" : "username"].GetString()
                         select a).ToArray();
                foreach (var item in d)
                    infos.Remove(item);
                if (p == -1)
                    for (int i = 0; i < array.Count; i++)
                        infos.Add(new UserViewModel(isFollowList ? array[i].GetObject()["fUserInfo"] : array[i].GetObject()["userInfo"]));
                else
                    for (int i = 0; i < array.Count; i++)
                        infos.Insert(i, new UserViewModel(isFollowList ? array[i].GetObject()["fUserInfo"] : array[i].GetObject()["userInfo"]));
            }
            else if (p == -1) page--;
            Tools.HideProgressBar();
        }

        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (!e.IsIntermediate)
                if (VScrollViewer.VerticalOffset == VScrollViewer.ScrollableHeight)
                    LoadList();
        }

        private void TitleBar_RefreshButtonClick(object sender, RoutedEventArgs e)
        {
            LoadList(1);
            VScrollViewer.ChangeView(null, 0, null);
        }

        private void UserList_RefreshRequested(object sender, EventArgs e) => LoadList(1);

        private void TitleBar_BackButtonClick(object sender, RoutedEventArgs e) => Frame.GoBack();
    }
}
