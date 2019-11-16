using CoolapkUWP.Control.ViewModels;
using CoolapkUWP.Data;
using CoolapkUWP.Pages.AppPages;
using CoolapkUWP.Pages.FeedPages;
using CoolapkUWP.Pages.SettingPages;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Windows.Data.Json;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
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
            if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Desktop")
                Windows.ApplicationModel.Core.CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            Application.Current.LeavingBackground += ChangeColor;
            Settings.uISettings.ColorValuesChanged += ChangeColor;
            Settings.InitializeSettings();
            if (Settings.GetBoolen("CheckUpdateWhenLuanching")) Settings.CheckUpdate();

            SystemNavigationManager.GetForCurrentView().BackRequested += (sender, ee) =>
            {
                if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Desktop" && Tools.popups.Count > 1)
                {
                    ee.Handled = true;
                    Popup popup = Tools.popups[Tools.popups.Count - 2];
                    popup.IsOpen = false;
                    Tools.popups.Remove(popup);
                }
                else if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile" && Tools.popups.Count > 0)
                {
                    ee.Handled = true;
                    Popup popup = Tools.popups[Tools.popups.Count - 1];
                    popup.IsOpen = false;
                    Tools.popups.Remove(popup);
                }
                else if (Frame.CanGoBack)
                {
                    ee.Handled = true;
                    Frame.GoBack();
                }
            };

            hamburgerMenuControl.ItemsSource = MenuItem.GetMainItems();
            hamburgerMenuControl.OptionsItemsSource = MenuItem.GetOptionsItems();
            if (Settings.IsMobile) TopBar.Margin = new Thickness(0);
            UpdateUserInfo();
            Settings.CheckTheme();
        }

        private async void ChangeColor(object sender, object e)
        {
            if (Settings.GetBoolen("IsBackgroundColorFollowSystem"))
            {
                Settings.Set("IsDarkMode", Settings.uISettings.GetColorValue(UIColorType.Background).Equals(Colors.Black) ? true : false);
                await Dispatcher.RunAsync(CoreDispatcherPriority.High, () => Settings.CheckTheme());
            }
        }

        public async void UpdateUserInfo()
        {
            ObservableCollection<MenuItem> items = hamburgerMenuControl.OptionsItemsSource as ObservableCollection<MenuItem>;
            if (items.Count == 2) items.RemoveAt(0);
            MenuItem item = new MenuItem
            {
                Index = -1,
                PageType = typeof(UserPage),
                Name = Settings.GetString("UserName"),
                Icon = Symbol.Contact
            };
            items.Insert(0, item);
            UserButton.DataContext = item;
            item.Image = string.IsNullOrEmpty(Settings.GetString("UserAvatar")) ? null : await ImageCache.GetImage(ImageType.SmallAvatar, Settings.GetString("UserAvatar"));
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
                items.Insert(0, new MenuItem() { Icon = Symbol.Find, Name = "搜索", PageType = null, Index = -3 });
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
                                Icon = await ImageCache.GetImage(ImageType.Icon, (item["logo"].GetString())),
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
            if (args.ChosenSuggestion is AppViewModel app)
                Tools.Navigate(typeof(AppPage), "https://www.coolapk.com" + app.Url);
            else if (args.ChosenSuggestion is SearchWord word)
            {
                switch (word.Symbol)
                {
                    case Symbol.Shop:
                        Tools.Navigate(typeof(SearchPage), new object[] { 3, word.Title.Substring(5) });
                        break;
                    case Symbol.Contact:
                        Tools.Navigate(typeof(SearchPage), new object[] { 1, word.Title.Substring(5) });
                        break;
                    case Symbol.Find:
                        Tools.Navigate(typeof(SearchPage), new object[] { 0, word.Title });
                        break;
                }
            }
            else if (args.ChosenSuggestion is null) Tools.Navigate(typeof(SearchPage), new object[] { 0, sender.Text });
        }

        private void HamburgerButton_Click(object sender, RoutedEventArgs e)
        {
            (sender as Button).Click += (s, ee) =>
            {
                ObservableCollection<MenuItem> items = hamburgerMenuControl.ItemsSource as ObservableCollection<MenuItem>;
                items.RemoveAt(0);
                if (hamburgerMenuControl.IsPaneOpen) items.Insert(0, new MenuItem() { Icon = Symbol.Find, Name = "搜索", PageType = null, Index = -3 });
                else items.Insert(0, new MenuItem() { Icon = Symbol.Find, Name = "搜索", PageType = null, Index = -2 });
            };
        }

        private void AutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            if (args.SelectedItem is AppViewModel app) sender.Text = app.AppName;
            else if (args.SelectedItem is SearchWord word)
            {
                switch (word.Symbol)
                {
                    case Symbol.Shop:
                        sender.Text = word.Title.Substring(5);
                        break;
                    case Symbol.Contact:
                        sender.Text = word.Title.Substring(5);
                        break;
                    case Symbol.Find:
                        sender.Text = word.Title;
                        break;
                }
            }
        }
    }

    public class MenuItem : INotifyPropertyChanged
    {
        public Symbol Icon { get; set; }
        private ImageSource image;
        public event PropertyChangedEventHandler PropertyChanged;
        public ImageSource Image
        {
            get => image;
            set
            {
                image = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Image)));
            }
        }
        public string Name { get; set; }
        public Type PageType { get; set; }
        public int Index { get; set; }

        public static ObservableCollection<MenuItem> GetMainItems()
        {
            ObservableCollection<MenuItem> items = new ObservableCollection<MenuItem>
            {
                new MenuItem() { Icon = Symbol.Find, Name = "搜索", PageType = null, Index = -3},
                new MenuItem() { Icon = Symbol.Home, Name = "首页", PageType = typeof(InitialPage), Index = 1},
                new MenuItem() { Icon = Symbol.Shop, Name = "应用游戏", PageType = typeof(AppRecommendPage), Index = 2}
            };
            return items;
        }

        public static ObservableCollection<MenuItem> GetOptionsItems()
        {
            ObservableCollection<MenuItem> items = new ObservableCollection<MenuItem>
            {
                new MenuItem() { Icon = Symbol.Contact, Name = string.Empty, PageType = typeof(UserPage), Index = -1},
                new MenuItem() { Icon = Symbol.Setting, Name = "设置", PageType = typeof(SettingPage), Index = 0}
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