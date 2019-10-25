using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Foundation.Metadata;
using Windows.Storage;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using 酷安_UWP.Model;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace 酷安_UWP
{
    public sealed partial class MainPage : Page
    {
        //public static TextBlock _User_Name;
        //public static ImageBrush _User_Face;
        //public static ColumnDefinition _dcd, _lcd;
        public static int seletedItem =
#if DEBUG
            0;
#else
        4;
#endif
        public MainPage()
        {
            this.InitializeComponent();
            SystemNavigationManager.GetForCurrentView().BackRequested += App_BackRequested;
            hamburgerMenuControl.ItemsSource = MenuItem.GetMainItems();
            hamburgerMenuControl.OptionsItemsSource = MenuItem.GetOptionsItems();

            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            UISettings uiSettings = new UISettings();
            Application.Current.LeavingBackground += Current_Resuming;
            Application.Current.Resuming += Current_Resuming;
            uiSettings.ColorValuesChanged += async (sender, args) =>
            {
                if (Convert.ToBoolean(localSettings.Values["IsBackgroundColorFollowSystem"]))
                {
                    localSettings.Values["IsDarkMode"] = uiSettings.GetColorValue(UIColorType.Background).Equals(Colors.Black) ? true : false;
                    await Dispatcher.RunAsync(CoreDispatcherPriority.High, () => SettingPage.CheckTheme());
                }
                //if (Convert.ToBoolean(localSettings.Values["IsAccentColorFollowSystem"]))
                //    await Dispatcher.RunAsync(CoreDispatcherPriority.High, () => Application.Current.Resources["SystemAccentColor"] = uiSettings.GetColorValue(UIColorType.Accent));
            };
            UpdateUserInfo(localSettings);
            SettingPage.CheckTheme();
        }

        public void UpdateUserInfo(ApplicationDataContainer localSettings)
        {
            ObservableCollection<MenuItem> items = hamburgerMenuControl.OptionsItemsSource as ObservableCollection<MenuItem>;
            if (items.Count == 2) items.RemoveAt(0);
            MenuItem item = new MenuItem
            {
                Index = -1,
                PageType = typeof(UserPage),
                Image = string.IsNullOrEmpty(localSettings.Values["UserAvatar"] as string) ? new BitmapImage() : new BitmapImage(new Uri(localSettings.Values["UserAvatar"] as string)),
                Name = localSettings.Values["UserName"] as string,
                Icon = Symbol.Contact
            };
            items.Insert(0, item);
        }

        private void Current_Resuming(object sender, object e)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            UISettings uiSettings = new UISettings();
            if (Convert.ToBoolean(localSettings.Values["IsBackgroundColorFollowSystem"]))
            {
                localSettings.Values["IsDarkMode"] = uiSettings.GetColorValue(UIColorType.Background).Equals(Colors.Black) ? true : false;
                SettingPage.CheckTheme();
            }
            //if (Convert.ToBoolean(localSettings.Values["IsAccentColorFollowSystem"]))
            //    await Dispatcher.RunAsync(CoreDispatcherPriority.High, () => Application.Current.Resources["SystemAccentColor"] = uiSettings.GetColorValue(UIColorType.Accent));
        }

        public static async void CheckUpdate(bool canShowDialog)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            Octokit.GitHubClient client = new Octokit.GitHubClient(new Octokit.ProductHeaderValue("Coolapk-UWP"));
            var release = await client.Repository.Release.GetLatest("Tangent-90", "Coolapk-UWP");
            var ver = release.TagName.Replace("v", string.Empty).Split('.');
            if (ushort.Parse(ver[0]) > Package.Current.Id.Version.Major)
            {
                GetUpdateContentDialog dialog = new GetUpdateContentDialog(release.HtmlUrl, release.Body) { RequestedTheme = Convert.ToBoolean(localSettings.Values["IsDarkMode"]) ? ElementTheme.Dark : ElementTheme.Light };
                await dialog.ShowAsync();
            }
            else if (ushort.Parse(ver[0]) == Package.Current.Id.Version.Major && ushort.Parse(ver[1]) > Package.Current.Id.Version.Minor)
            {
                GetUpdateContentDialog dialog = new GetUpdateContentDialog(release.HtmlUrl, release.Body) { RequestedTheme = Convert.ToBoolean(localSettings.Values["IsDarkMode"]) ? ElementTheme.Dark : ElementTheme.Light };
                await dialog.ShowAsync();
            }
            else if (ushort.Parse(ver[0]) == Package.Current.Id.Version.Major && ushort.Parse(ver[1]) == Package.Current.Id.Version.Minor && ushort.Parse(ver[2]) > Package.Current.Id.Version.Build)
            {
                GetUpdateContentDialog dialog = new GetUpdateContentDialog(release.HtmlUrl, release.Body) { RequestedTheme = Convert.ToBoolean(localSettings.Values["IsDarkMode"]) ? ElementTheme.Dark : ElementTheme.Light };
                await dialog.ShowAsync();
            }
            else if (canShowDialog)
                await new Windows.UI.Popups.MessageDialog("当前无可用更新。").ShowAsync();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            Type pageType = typeof(SettingPage);
            switch (seletedItem)
            {
                case 0:
                    pageType = typeof(SettingPage);
                    break;
                case 1:
                    pageType = typeof(InitialPage);
                    break;
                case 2:
                    pageType = typeof(SearchPage);
                    break;
                case 3:
                    pageType = typeof(AppRecommendPage);
                    break;
            }
            if (hamburgerMenuControl.SelectedIndex != seletedItem - 1)
            {
                hamburgerMenuControl.SelectedIndex = seletedItem - 1;
                hamburgerMenuControl.SelectedOptionsIndex = seletedItem - 1 < 0 ? 0 : -1;
            }
            if (hamburgerMenuControl.SelectedOptionsIndex == seletedItem)
                hamburgerMenuControl.SelectedOptionsIndex = 1;
            VFrame.Navigate(pageType, this);
        }

        private void OnMenuItemClick(object sender, ItemClickEventArgs e)
        {
            MenuItem menuItem = e.ClickedItem as MenuItem;
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            if (string.IsNullOrEmpty(menuItem.Name)) return;
            else if (Equals(menuItem.Name, localSettings.Values["UserName"])) Frame.Navigate(typeof(UserPage), new object[] { localSettings.Values["Uid"], this });
            else
            {
                VFrame.Navigate(menuItem.PageType, this);
                seletedItem = menuItem.Index;
            }
            if (hamburgerMenuControl.DisplayMode != SplitViewDisplayMode.CompactInline)
                hamburgerMenuControl.IsPaneOpen = false;
        }

        public static void User_Load()
        {
            /*
            _User_Name.Text = LoginPage.UserName;
            _User_Face.ImageSource = new BitmapImage(new Uri(LoginPage.UserFace, UriKind.RelativeOrAbsolute));

            //Save
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values["name"] = LoginPage.UserName;
            localSettings.Values["face"] = LoginPage.UserFace;
            localSettings.Values["login"] = "1";
            */
        }

        private void App_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                e.Handled = true;
                Frame.GoBack();
            }
        }

        public void ActiveProgressRing() => progressRing.IsActive = true;
        public void DeactiveProgressRing() => progressRing.IsActive = false;
    }

    public class MenuItem
    {
        public Symbol Icon { get; set; }
        public ImageSource Image { get; set; }
        public string Name { get; set; }
        public Type PageType { get; set; }
        public int Index;

        public static List<MenuItem> GetMainItems()
        {
            List<MenuItem> items = new List<MenuItem>
            {
                new MenuItem() { Icon = Symbol.Home, Name = "首页", PageType = typeof(InitialPage), Index = 1, Image=new BitmapImage()},
                new MenuItem() { Icon = Symbol.Find, Name = "搜素", PageType = typeof(SearchPage), Index = 2 , Image=new BitmapImage()},
                new MenuItem() { Icon = Symbol.Shop, Name = "应用游戏", PageType = typeof(AppRecommendPage), Index = 3, Image=new BitmapImage()}
            };
            return items;
        }

        public static ObservableCollection<MenuItem> GetOptionsItems()
        {
            ObservableCollection<MenuItem> items = new ObservableCollection<MenuItem>
            {
                new MenuItem() { Icon = Symbol.Contact, Name = string.Empty, PageType = typeof(UserPage), Index = -1, Image=new BitmapImage()},
                new MenuItem() { Icon = Symbol.Setting, Name = "设置", PageType = typeof(SettingPage), Index = 0, Image=new BitmapImage()}
            };
            return items;
        }
    }
}