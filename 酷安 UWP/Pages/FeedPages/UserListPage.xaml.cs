using CoolapkUWP.Control.ViewModels;
using CoolapkUWP.Data;
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
        string uid;
        bool isFollowList;
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
                    VScrollViewer = (VisualTreeHelper.GetChild(UserList, 0) as Border).FindName("ScrollViewer") as ScrollViewer;
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
            if (p == 1)
            {
                string url = isFollowList ? $"/user/followList?uid={uid}&page={p}" : $"/user/fansList?uid={uid}&page={p}";
                string r = await Tools.GetJson(url);
                JsonArray array = Tools.GetDataArray(r);
                if (array != null && array.Count > 0)
                {
                    firstItem = array.First().GetObject()["fuid"].GetNumber();
                    lastItem = array.Last().GetObject()["fuid"].GetNumber();
                    if (infos.Count > 0)
                    {
                        var d = (from a in infos
                                 from b in array
                                 where a.UserName == b.GetObject()[isFollowList ? "fusername" : "username"].GetString()
                                 select a).ToArray();
                        foreach (var item in d)
                            infos.Remove(item);
                    }
                    for (int i = 0; i < array.Count; i++)
                    {
                        IJsonValue t = isFollowList ? array[i].GetObject()["fUserInfo"] : array[i].GetObject()["userInfo"];
                        infos.Insert(i, new UserViewModel(t));
                    }
                }
            }
            else if (p == -1)
            {
                string url = isFollowList ? $"/user/followList?uid={uid}&page={++page}&firstItem={firstItem}&lastItem={lastItem}" : $"/user/fansList?uid={uid}&page={++page}&firstItem={firstItem}&lastItem={lastItem}";
                string r = await Tools.GetJson(url);
                JsonArray array = Tools.GetDataArray(r);
                if (array != null && array.Count > 0)
                {
                    lastItem = array.Last().GetObject()["fuid"].GetNumber();
                    for (int i = 0; i < array.Count; i++)
                    {
                        IJsonValue t = isFollowList ? array[i].GetObject()["fUserInfo"] : array[i].GetObject()["userInfo"];
                        infos.Add(new UserViewModel(t));
                    }
                }
                else page--;
            }
            Tools.HideProgressBar();
        }

        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (!e.IsIntermediate)
            {
                if (VScrollViewer.VerticalOffset == 0)
                {
                    LoadList(1);
                    VScrollViewer.ChangeView(null, 20, null);
                    refreshText.Visibility = Visibility.Collapsed;
                }
                else if (VScrollViewer.VerticalOffset == VScrollViewer.ScrollableHeight)
                    LoadList();
            }
            else refreshText.Visibility = Visibility.Visible;
        }

        private void TitleBar_RefreshButtonClick(object sender, RoutedEventArgs e)
        {
            LoadList(1);
            VScrollViewer.ChangeView(null, 20, null);
        }

        private void TitleBar_BackButtonClick(object sender, RoutedEventArgs e) => Frame.GoBack();
    }
}
