using CoolapkUWP.Data;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using Windows.ApplicationModel;
using Windows.Data.Json;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace CoolapkUWP.Pages.SettingPages
{
    class CacheSizeViewModel : INotifyPropertyChanged
    {
        public double Size;
        public string SizeString;
        public string Title;
        public double totalSize;
        public ImageType type;
        public double TotalSize
        {
            get => totalSize;
            set
            {
                totalSize = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TotalSize)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SettingPage : Page
    {
        public SettingPage() => this.InitializeComponent();

        CancellationTokenSource source;
        ObservableCollection<CacheSizeViewModel> models = new ObservableCollection<CacheSizeViewModel>();

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            Tools.ShowProgressBar();
            IsNoPicsMode.IsOn = Settings.GetBoolen("IsNoPicsMode");
            IsUseOldEmojiMode.IsOn = Settings.GetBoolen("IsUseOldEmojiMode");
            IsDarkMode.IsOn = Settings.GetBoolen("IsDarkMode");
            CheckUpdateWhenLuanching.IsOn = Settings.GetBoolen("CheckUpdateWhenLuanching");
            IsBackgroundColorFollowSystem.IsOn = Settings.GetBoolen("IsBackgroundColorFollowSystem");
            IsDarkMode.Visibility = IsBackgroundColorFollowSystem.IsOn ? Visibility.Collapsed : Visibility.Visible;
            VersionTextBlock.Text = $"{Package.Current.Id.Version.Major}.{Package.Current.Id.Version.Minor}.{Package.Current.Id.Version.Build}";
            uidTextBox.Text = Settings.GetString("UserName");
#if DEBUG
            gotoTestPage.Visibility = Visibility.Visible;
#endif
            GetCacheSize();
            Tools.HideProgressBar();
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            if (source != null)
                source.Cancel();
        }

        async void GetCacheSize()
        {
            Tools.ShowProgressBar();
            CacheSizeTextBlock.Text = "正在加载……";
            CleanCacheButton.IsEnabled = RefreshCacheButton.IsEnabled = false;
            CacheSizeListView.SelectionMode = ListViewSelectionMode.None;
            models.Clear();
            double size = 0;
            for (int i = 0; i < 5; i++)
            {
                source = new CancellationTokenSource();
                string name = string.Empty;
                switch (i)
                {
                    case 0: name = "缩略图"; break;
                    case 1: name = "原图"; break;
                    case 2: name = "小头像"; break;
                    case 3: name = "大头像"; break;
                    case 4: name = "图标"; break;
                }
                double s = await ImageCache.GetCacheSize((ImageType)i, source.Token);
                size += s;
                models.Add(new CacheSizeViewModel
                {
                    TotalSize = s,
                    Size = s,
                    SizeString = Tools.GetSizeString(s),
                    Title = name,
                    type = (ImageType)i
                });
                source = null;
            }
            CacheSizeTextBlock.Text = Tools.GetSizeString(size);
            foreach (var item in models)
                item.TotalSize = size;
            RefreshCacheButton.IsEnabled = true;
            CacheSizeListView.SelectionMode = ListViewSelectionMode.Multiple;
            Tools.HideProgressBar();
        }

        private void CleanDataButton_Click(object sender, RoutedEventArgs e)
        {
            ApplicationData.Current.LocalSettings.Values.Clear();
            var cookieManager = new Windows.Web.Http.Filters.HttpBaseProtocolFilter().CookieManager;
            foreach (var item in cookieManager.GetCookies(new Uri("http://account.coolapk.com")))
                cookieManager.DeleteCookie(item);
        }

        private void ToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            ToggleSwitch toggle = sender as ToggleSwitch;
            Settings.Set(toggle.Name, toggle.IsOn);
            switch (toggle.Name as string)
            {
                case "IsDarkMode":
                    Settings.CheckTheme();
                    break;
                case "IsBackgroundColorFollowSystem":
                    Settings.Set("IsDarkMode", Settings.uISettings.GetColorValue(UIColorType.Background).Equals(Colors.Black) ? true : false);
                    Settings.CheckTheme();
                    IsDarkMode.IsOn = Settings.GetBoolen("IsDarkMode");
                    IsDarkMode.Visibility = toggle.IsOn ? Visibility.Collapsed : Visibility.Visible;
                    break;
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            switch (button.Tag as string)
            {
                case "gotoTestPage": Tools.Navigate(typeof(TestPage), null); break;
                case "checkUpdate": Settings.CheckUpdate(); break;
                case "RefreshCache": GetCacheSize(); break;
                case "CleanCache":
                    CleanCacheButton.IsEnabled = RefreshCacheButton.IsEnabled = false;
                    CacheSizeTextBlock.Text = "正在处理……";
                    foreach (var item in CacheSizeListView.SelectedItems)
                        await ImageCache.CleanCache((item as CacheSizeViewModel).type);
                    GetCacheSize();
                    CleanCacheButton.IsEnabled = RefreshCacheButton.IsEnabled = true;
                    break;
                case "fakeLogin":
                    try
                    {
                        string userName, uid, userAvatar;
                        userName = uid = userAvatar = string.Empty;
                        if (!string.IsNullOrEmpty(uidTextBox.Text))
                        {
                            uid = await Tools.GetUserIDByName(uidTextBox.Text);
                            JsonObject r = Tools.GetJSonObject(await Tools.GetJson("/user/space?uid=" + uid));
                            if (r != null)
                            {
                                userName = r["username"].GetString();
                                userAvatar = r["userSmallAvatar"].GetString();
                            }
                            else uid = string.Empty;
                        }
                        Settings.Set("UserName", userName);
                        Settings.Set("Uid", uid);
                        Settings.Set("UserAvatar", userAvatar);
                        Tools.mainPage.GetUserAvatar();
                    }
                    catch (System.Net.Http.HttpRequestException ex) { Tools.ShowHttpExceptionMessage(ex); }
                    catch (Exception ex) { await new MessageDialog($"出现错误，可能是用户名不正确。\n{ex}").ShowAsync(); }
                    break;
            }
        }

        private void CacheSizeListView_SelectionChanged(object sender, SelectionChangedEventArgs e) => CleanCacheButton.IsEnabled = e.AddedItems.Count > 0;
    }
}
