using CoolapkUWP.Control.ViewModels;
using CoolapkUWP.Data;
using CoolapkUWP.Pages.AppPages;
using CoolapkUWP.Pages.FeedPages;
using CoolapkUWP.Pages.SettingPages;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace CoolapkUWP.Pages
{
    public sealed partial class MainPage : Page
    {
        static int seletedItem =
#if DEBUG
            0;
#else
        1;
#endif
        public MainPage()
        {
            this.InitializeComponent();
            Tools.mainPage = this;
            hamburgerMenuControl.ItemsSource = MenuItem.GetMainItems();
            hamburgerMenuControl.OptionsItemsSource = MenuItem.GetOptionsItems();
            if (Settings.IsMobile) TopBar.Margin = new Thickness(0);
            UpdateUserInfo();
            Settings.CheckTheme();
            Task.Run(async () =>
            {
                await Task.Delay(300);
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                 {
                     var aa = VisualTreeHelper.GetChild(hamburgerMenuControl, 0) as Grid;
                     var a = aa.FindName("HamburgerButton") as Button;
                     a.Click += (sender, ee) =>
                     {
                         ObservableCollection<MenuItem> items = hamburgerMenuControl.ItemsSource as ObservableCollection<MenuItem>;
                         items.RemoveAt(0);
                         if (hamburgerMenuControl.IsPaneOpen) items.Insert(0, new MenuItem() { Icon = Symbol.Find, Name = "搜索", PageType = null, Index = -3, Image = new BitmapImage() });
                         else items.Insert(0, new MenuItem() { Icon = Symbol.Find, Name = "搜索", PageType = null, Index = -2, Image = new BitmapImage() });
                     };
                 });
            });
        }

        public void UpdateUserInfo()
        {
            ObservableCollection<MenuItem> items = hamburgerMenuControl.OptionsItemsSource as ObservableCollection<MenuItem>;
            if (items.Count == 2) items.RemoveAt(0);
            MenuItem item = new MenuItem
            {
                Index = -1,
                PageType = typeof(UserPage),
                Image = string.IsNullOrEmpty(Settings.GetString("UserAvatar")) ? new BitmapImage() : new BitmapImage(new Uri(Settings.GetString("UserAvatar"))),
                Name = Settings.GetString("UserName"),
                Icon = Symbol.Contact
            };
            items.Insert(0, item);
            UserButton.DataContext = item;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            Type pageType = typeof(SettingPage);
            switch (seletedItem)
            {
                case 0:
                    pageType = typeof(SettingPage);
                    rect3.Foreground = Application.Current.Resources["SystemControlBackgroundAccentBrush"] as SolidColorBrush;
                    hamburgerMenuControl.SelectedIndex = -1;
                    hamburgerMenuControl.SelectedOptionsIndex = 1;
                    break;
                case 1:
                    pageType = typeof(InitialPage);
                    rect1.Foreground = Application.Current.Resources["SystemControlBackgroundAccentBrush"] as SolidColorBrush;
                    hamburgerMenuControl.SelectedIndex = 1;
                    hamburgerMenuControl.SelectedOptionsIndex = -1;
                    break;
                case 2:
                    pageType = typeof(AppRecommendPage);
                    rect2.Foreground = Application.Current.Resources["SystemControlBackgroundAccentBrush"] as SolidColorBrush;
                    hamburgerMenuControl.SelectedIndex = 2;
                    hamburgerMenuControl.SelectedOptionsIndex = -1;
                    break;
            }
            VFrame.Navigate(pageType, this);
        }

        private void OnMenuItemClick(object sender, ItemClickEventArgs e)
        {
            MenuItem menuItem = e.ClickedItem as MenuItem;
            if (string.IsNullOrEmpty(menuItem.Name) || menuItem.Index == -3) return;
            else if (menuItem.Name == Settings.GetString("UserName")) Frame.Navigate(typeof(UserPage), Settings.GetString("Uid"));
            else if (menuItem.Index == -2)
            {
                hamburgerMenuControl.IsPaneOpen = true;
                ObservableCollection<MenuItem> items = hamburgerMenuControl.ItemsSource as ObservableCollection<MenuItem>;
                items.RemoveAt(0);
                items.Insert(0, new MenuItem() { Icon = Symbol.Find, Name = "搜索", PageType = null, Index = -3, Image = new BitmapImage() });
                return;
            }
            else
            {
                VFrame.Navigate(menuItem.PageType, this);
                seletedItem = menuItem.Index;
                switch (seletedItem)
                {
                    case 0:
                        rect3.Foreground = Application.Current.Resources["SystemControlBackgroundAccentBrush"] as SolidColorBrush;
                        break;
                    case 1:
                        rect1.Foreground = Application.Current.Resources["SystemControlBackgroundAccentBrush"] as SolidColorBrush;
                        break;
                    case 2:
                        rect2.Foreground = Application.Current.Resources["SystemControlBackgroundAccentBrush"] as SolidColorBrush;
                        break;
                }
                ChangeButtonForeground();
            }
        }

        private void UserButton_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = (sender as FrameworkElement).DataContext as MenuItem;
            if (string.IsNullOrEmpty(menuItem.Name)) return;
            else if (menuItem.Name == Settings.GetString("UserName")) Frame.Navigate(typeof(UserPage), Settings.GetString("Uid"));
        }

        public void ResetRowHeight()
        {
            if (Window.Current.Bounds.Width < 769)
                TopBar.Height = BottomNavBar.Height = 48;
        }

        public bool ChangeRowHeight(double height)
        {
            if (height < 0 && height < -BottomNavBar.ActualHeight) height = -BottomNavBar.ActualHeight;
            if (Window.Current.Bounds.Width >= 769 ||
                (BottomNavBar.ActualHeight + height > 48) ||
                (BottomNavBar.ActualHeight + height < 0)) return false;
            TopBar.Height = BottomNavBar.Height = BottomNavBar.ActualHeight + height;
            return true;
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            Type pageType = typeof(SettingPage);
            switch ((sender as FrameworkElement).Tag as string)
            {
                case "0":
                    pageType = typeof(SettingPage);
                    seletedItem = 0;
                    rect3.Foreground = Application.Current.Resources["SystemControlBackgroundAccentBrush"] as SolidColorBrush;
                    hamburgerMenuControl.SelectedIndex = -1;
                    hamburgerMenuControl.SelectedOptionsIndex = 1;
                    break;
                case "1":
                    pageType = typeof(InitialPage);
                    seletedItem = 1;
                    rect1.Foreground = Application.Current.Resources["SystemControlBackgroundAccentBrush"] as SolidColorBrush;
                    hamburgerMenuControl.SelectedOptionsIndex = -1;
                    hamburgerMenuControl.SelectedIndex = 1;
                    break;
                case "2":
                    pageType = typeof(AppRecommendPage);
                    seletedItem = 2;
                    rect2.Foreground = Application.Current.Resources["SystemControlBackgroundAccentBrush"] as SolidColorBrush;
                    hamburgerMenuControl.SelectedOptionsIndex = -1;
                    hamburgerMenuControl.SelectedIndex = 2;
                    break;
            }
            ChangeButtonForeground();
            VFrame.Navigate(pageType, this);
        }

        public void ChangeButtonForeground()
        {
            Color color = Settings.GetBoolen("IsDarkMode") ? Colors.White : Colors.Black;
            switch (seletedItem)
            {
                case 0:
                    rect1.Foreground = rect2.Foreground = new SolidColorBrush(color);
                    break;
                case 1:
                    rect3.Foreground = rect2.Foreground = new SolidColorBrush(color);
                    break;
                case 2:
                    rect1.Foreground = rect3.Foreground = new SolidColorBrush(color);
                    break;
            }
        }

        private async void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.SuggestionChosen) return;
            string r = await Tools.GetJson($"/search/suggestSearchWordsNew?searchValue={sender.Text}&type=app");
            JsonArray array = Tools.GetDataArray(r);
            if (array != null && array.Count > 0)
            {
                ObservableCollection<object> observableCollection = new ObservableCollection<object>();
                sender.ItemsSource = observableCollection;
                foreach (var ite in array)
                {
                    JsonObject item = ite.GetObject();
                    switch (item["entityType"].GetString())
                    {
                        case "apk":
                            observableCollection.Add(new AppViewModel
                            {
                                AppName = item["title"].GetString(),
                                DownloadNum = $"{item["score"].GetString()}分 {item["downCount"].GetString()}下载",
                                Url = item["url"].GetString(),
                                Icon = new BitmapImage(new Uri(item["logo"].GetString())),
                                Size = item["apksize"].GetString(),
                            });
                            break;
                        case "searchWord":
                        default:
                            Symbol s = Symbol.Find;
                            if (item["logo"].GetString().Contains("cube")) s = Symbol.Shop;
                            else if (item["logo"].GetString().Contains("xitongguanli")) s = Symbol.Contact;
                            observableCollection.Add(new SearchWord { Symbol = s, Title = item["title"].GetString() });
                            break;
                    }
                }
            }
        }

        private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion is AppViewModel info)
            {
                Tools.rootPage.Navigate(typeof(AppPage), "https://www.coolapk.com" + info.Url);
                sender.Text = info.AppName;
            }
            else if (args.ChosenSuggestion is SearchWord searchWord)
            {
                switch (searchWord.Symbol)
                {
                    case Symbol.Shop:
                        Tools.rootPage.Navigate(typeof(SearchPage), new object[] { 3, searchWord.Title.Substring(5) });
                        sender.Text = searchWord.Title.Substring(5);
                        break;
                    case Symbol.Contact:
                        Tools.rootPage.Navigate(typeof(SearchPage), new object[] { 1, searchWord.Title.Substring(5) });
                        sender.Text = searchWord.Title.Substring(5);
                        break;
                    case Symbol.Find:
                        Tools.rootPage.Navigate(typeof(SearchPage), new object[] { 0, searchWord.Title });
                        sender.Text = searchWord.Title;
                        break;
                }
            }
            else if (args.ChosenSuggestion is null) Tools.rootPage.Navigate(typeof(SearchPage), new object[] { 0, sender.Text });
        }
    }

    public class MenuItem
    {
        public Symbol Icon { get; set; }
        public ImageSource Image { get; set; }
        public string Name { get; set; }
        public Type PageType { get; set; }
        public int Index;

        public static ObservableCollection<MenuItem> GetMainItems()
        {
            ObservableCollection<MenuItem> items = new ObservableCollection<MenuItem>
            {
                new MenuItem() { Icon = Symbol.Find, Name = "搜索", PageType = null, Index = -3, Image=new BitmapImage()},
                new MenuItem() { Icon = Symbol.Home, Name = "首页", PageType = typeof(InitialPage), Index = 1, Image=new BitmapImage()},
                new MenuItem() { Icon = Symbol.Shop, Name = "应用游戏", PageType = typeof(AppRecommendPage), Index = 2, Image=new BitmapImage()}
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

    public class HamberTemplateSelector : DataTemplateSelector
    {
        public DataTemplate DataTemplate1 { get; set; }
        public DataTemplate DataTemplate2 { get; set; }
        protected override DataTemplate SelectTemplateCore(object item)
        {
            MenuItem menuItem = item as MenuItem;
            if (menuItem.Index == -3) return DataTemplate2;
            return DataTemplate1;
        }
    }
}