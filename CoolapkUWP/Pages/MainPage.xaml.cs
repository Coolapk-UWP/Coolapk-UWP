#define LOAD_MAIN_PAGE

using CoolapkUWP.Helpers;
using CoolapkUWP.Helpers.Providers;
using CoolapkUWP.Pages.FeedPages;
using System;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using MenuItem = CoolapkUWP.Models.Json.IndexPageHeaderItemModel.Item;

namespace CoolapkUWP.Pages
{
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        private ImmutableArray<MenuItem> allMenuItems = ImmutableArray<MenuItem>.Empty;
        private ImmutableArray<MenuItem> menuItems = ImmutableArray<MenuItem>.Empty;

        private ImmutableArray<MenuItem> MenuItems
        {
            get => menuItems;
            set
            {
                menuItems = value;
                RaisePropertyChangedEvent();
            }
        }

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
            this.InitializeComponent();
#if LOAD_MAIN_PAGE
            GetIndexPageItems();
#endif
        }

        private async void GetIndexPageItems()
        {
            var temp = await DataHelper.GetDataAsync<Models.Json.IndexPageHeaderItemModel.Rootobject>(UriProvider.GetObject(UriType.GetIndexPageNames).GetUri());

            MenuItems = await Task.Run(() =>
                (from t in temp.Data
                 where t.EntityTemplate == "configCard"
                 from ta in t.Entities
                 where ta.Title != "酷品"
                 where ta.Title != "看看号"
                 where ta.Title != "直播"
                 where ta.Title != "视频"
                 select new MenuItem(ta)).ToImmutableArray()
            );
            await Task.Run(() =>
                allMenuItems = menuItems.Concat(from i in menuItems
                                                where i.Entities != null
                                                from j in i.Entities
                                                select j).ToImmutableArray()
            );
            await Task.Delay(200);
            navigationView.SelectedItem = menuItems.First(i => i.Title == "头条");
            navigationViewFrame.Navigate(typeof(IndexPage), new ViewModels.IndexPage.ViewModel("/main/indexV8", false));
        }

        private void NavigationView_ItemInvoked(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewItemInvokedEventArgs args)
        {
            if (args.InvokedItem is string title)
            {
                if (title == "关注") { return; }
                var item = allMenuItems.First(i => i.Title == title);
                var uri = title == "头条" ? "/main/indexV8" : $"{item.Uri}&title={title}";
                navigationViewFrame.Navigate(typeof(IndexPage), new ViewModels.IndexPage.ViewModel(uri, false), args.RecommendedNavigationTransitionInfo);
            }
            else if (args.InvokedItem is MenuItem item1)
            {
                navigationViewFrame.Navigate(typeof(IndexPage), new ViewModels.IndexPage.ViewModel(item1.Uri, false), args.RecommendedNavigationTransitionInfo);
            }
        }

        private void Button_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            UIHelper.RefreshIndexPage();
        }
    }
}