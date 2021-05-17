#define LOAD_MAIN_PAGE

using CoolapkUWP.Core.Helpers;
using CoolapkUWP.Helpers;
using CoolapkUWP.Models;
using CoolapkUWP.Pages.FeedPages;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using muxc = Microsoft.UI.Xaml.Controls;

namespace CoolapkUWP.Pages
{
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        //private IEnumerable<MenuItem> allMenuItems;
        //private IEnumerable<MenuItem> menuItems;

        //private IEnumerable<MenuItem> MenuItems
        //{
        //    get => menuItems;
        //    set
        //    {
        //        menuItems = value;
        //        RaisePropertyChangedEvent();
        //    }
        //}

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null)
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void UIHelper_NeedMainPageProgressRing(object sender, bool e)
        {
            if (e)
            {
                FindName(nameof(progressRing));
            }
            progressRing.IsActive = e;
            if (!e)
            {
                UnloadObject(progressRing);
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            UIHelper.NeedMainPageProgressRing += UIHelper_NeedMainPageProgressRing;
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            UIHelper.NeedMainPageProgressRing -= UIHelper_NeedMainPageProgressRing;
            base.OnNavigatingFrom(e);
        }

        public MainPage()
        {
            InitializeComponent();
#if LOAD_MAIN_PAGE
            //GetIndexPageItems();
#endif
            navigationView.SelectedItem = navigationView.MenuItems[1];
        }

        private async void GetIndexPageItems()
        {
            #region 自动获取Items
            //var (isSucceed, result) = await DataHelper.GetDataAsync(UriHelper.GetUri(UriType.GetIndexPageNames), true);
            //if (!isSucceed) { return; }

            //var temp = (JArray)result;

            //MenuItems =
            //    from t in temp
            //    where t.Value<string>("entityTemplate") == "configCard"
            //    from ta in t["entities"]
            //    where ta.Value<string>("title") != "酷品"
            //    //where ta.Value<string>("title") != "看看号"
            //    where ta.Value<string>("title") != "直播"
            //    //where ta.Value<string>("title") != "视频"
            //    select new MenuItem(ta);

            //navigationView.SelectedItem = menuItems.First(i => i.Title == "头条");
            //navigationViewFrame.Navigate(typeof(IndexPage), new ViewModels.IndexPage.ViewModel("/main/indexV8", false));

            //allMenuItems = menuItems.Concat(
            //        from i in menuItems
            //        where i.Entities != null
            //        from j in i.Entities
            //        select j);
            #endregion
        }

        #region ItemInvoked
        //private void NavigationView_ItemInvoked(muxc.NavigationView sender, muxc.NavigationViewItemInvokedEventArgs args)
        //{
        //    if (args.IsSettingsInvoked)
        //    {
        //        if (Frame.CurrentSourcePageType != typeof(SettingPages.SettingPage))
        //        {
        //            Frame.Navigate(typeof(SettingPages.SettingPage));
        //        }
        //    }
        //    else if (args.InvokedItem is string title)
        //    {
        //        if (title == "关注") { return; }
        //        sender.Header = title;
        //        var item = allMenuItems.First(i => i.Title == title);
        //        var uri = title == "头条" ? "/main/indexV8" : $"{item.Uri}&title={title}";
        //        //UIHelper.ShowMessage(uri);
        //        navigationViewFrame.Navigate(typeof(IndexPage), new ViewModels.IndexPage.ViewModel(uri, false), args.RecommendedNavigationTransitionInfo);
        //    }
        //    else if (args.InvokedItem is MenuItem item1)
        //    {
        //        //UIHelper.ShowMessage(item1.Uri);
        //        navigationViewFrame.Navigate(typeof(IndexPage), new ViewModels.IndexPage.ViewModel(item1.Uri, false), args.RecommendedNavigationTransitionInfo);
        //    }
        //}
        #endregion

        private void NavigationView_SelectionChanged(muxc.NavigationView sender, muxc.NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected)
            {
                Frame.Navigate(typeof(SettingPages.SettingPage), args.RecommendedNavigationTransitionInfo);
            }
            else if (args.SelectedItemContainer != null)
            {
                var navItemTag = args.SelectedItemContainer.Tag.ToString();
                if (!navItemTag.StartsWith('V'))
                    navigationViewFrame.Navigate(typeof(IndexPage), new ViewModels.IndexPage.ViewModel("/page?url=V9_HOME_TAB_FOLLOW&type=" + navItemTag, false), args.RecommendedNavigationTransitionInfo);
                else if (navItemTag == "V9_HOME_TAB_HEADLINE")
                    navigationViewFrame.Navigate(typeof(IndexPage), new ViewModels.IndexPage.ViewModel("/main/indexV8", false), args.RecommendedNavigationTransitionInfo);
                else if (navItemTag == "V11_FIND_DYH")
                    navigationViewFrame.Navigate(typeof(IndexPage), new ViewModels.IndexPage.ViewModel("/user/dyhSubscribe", false), args.RecommendedNavigationTransitionInfo);
                else navigationViewFrame.Navigate(typeof(IndexPage), new ViewModels.IndexPage.ViewModel("/page?url=" + navItemTag, false), args.RecommendedNavigationTransitionInfo);
                //UIHelper.ShowMessage((string)selectedItem.Tag);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            UIHelper.RefreshIndexPage();
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ApplicationView view = ApplicationView.GetForCurrentView();
            bool isInFullScreenMode = view.IsFullScreenMode;
            if (isInFullScreenMode || view.ViewMode == ApplicationViewMode.CompactOverlay)
                navigationView.Margin = new Thickness(0, 32, 0, 0);
            else navigationView.Margin = new Thickness(0, 0, 0, 0);
        }
    }
}