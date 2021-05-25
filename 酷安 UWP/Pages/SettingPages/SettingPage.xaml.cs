using CoolapkUWP.Data;
using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
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
        public SettingPage()
        {
            InitializeComponent();
            main.Text =
@"
#### 欢迎使用酷安 UWP！
##### 声明
1. 本程序是[酷安](https://coolapk.com)的第三方客户端，仅用作学习交流使用，禁止用于商业用途。
2. 本程序是开源软件，因此，在使用时请确保程序是来自[本Github仓库](https://github.com/Tangent-90/Coolapk-UWP)，以确保您的数据安全。
3. 若程序来源无异常，程序运行过程中您的所有数据都仅用于与酷安的服务器交流或储存于本地，开发者不会窃取您的任何数据。但即便如此，也请注意使用环境的安全性。
4. 若您对[酷安](https://coolapk.com)如何处理您的数据存在疑虑，请访问[酷安用户服务协议](https://m.coolapk.com/mp/user/agreement)、[酷安隐私保护政策](https://m.coolapk.com/mp/user/privacy)、[酷安二手安全条约](https://m.coolapk.com/mp/user/ershouAgreement)。
##### 鸣谢
|                                       贡献                                     |                作者               |
| ----------------------------------------------------------------------------- | -------------------------------- |
| [原作(CoolApk-UWP)](https://github.com/oboard/CoolApk-UWP)                     | [oboard](https://github.com/oboard)|
| [Token获取方法(CoolapkTokenCrack)](https://github.com/ZCKun/CoolapkTokenCrack) | [ZCKun](https://github.com/ZCKun/)|
##### 引用及参考
- [Coolapk-kotlin](https://github.com/bjzhou/Coolapk-kotlin)
- [UWP Community Toolkit](https://github.com/Microsoft/UWPCommunityToolkit/)
- [Win UI](https://github.com/microsoft/microsoft-ui-xaml)
- [Json.NET](https://www.newtonsoft.com/json)
- [QRCoder](https://github.com/codebude/QRCoder)
- [Metro Log](https://github.com/novotnyllc/MetroLog)
- [Color Thief](https://github.com/KSemenenko/ColorThief)
";
        }

        private CancellationTokenSource source;
        private readonly ObservableCollection<CacheSizeViewModel> models = new ObservableCollection<CacheSizeViewModel>();

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var vs = e.Parameter;
            if (!(bool)vs)
            {
                TitleBar.Visibility = Visibility.Visible;
                listView.Padding = Settings.stackPanelMargin;
            }
            Tools.ShowProgressBar();
            IsNoPicsMode.IsOn = Settings.GetBoolen("IsNoPicsMode");
            IsUseOldEmojiMode.IsOn = Settings.GetBoolen("IsUseOldEmojiMode");
            IsDarkMode.IsOn = Settings.GetBoolen("IsDarkMode");
            CheckUpdateWhenLuanching.IsOn = Settings.GetBoolen("CheckUpdateWhenLuanching");
            IsBackgroundColorFollowSystem.IsOn = Settings.GetBoolen("IsBackgroundColorFollowSystem");
            IsDarkMode.Visibility = IsBackgroundColorFollowSystem.IsOn ? Visibility.Collapsed : Visibility.Visible;
            VersionTextBlock.Text = $"{Package.Current.Id.Version.Major}.{Package.Current.Id.Version.Minor}.{Package.Current.Id.Version.Build}";
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
            {
                source.Cancel();
                Tools.HideProgressBar();
            }
        }

        private async void MarkdownText_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            if (Uri.TryCreate(e.Link, UriKind.Absolute, out Uri link))
            {
                string str = link.ToString();
                Tools.OpenLink(str);
            }
        }

        private async void GetCacheSize()
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
                double s = 0;
                try { s = await ImageCache.GetCacheSize((ImageType)i, source.Token); }
                catch { Tools.ShowMessage("缓存信息获取失败"); }
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

        private void ToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            ToggleSwitch toggle = sender as ToggleSwitch;
            Settings.Set(toggle.Name, toggle.IsOn);
            switch (toggle.Name)
            {
                case "IsDarkMode":
                    Settings.CheckTheme();
                    break;
                case "IsBackgroundColorFollowSystem":
                    Settings.Set("IsDarkMode", Settings.uISettings.GetColorValue(UIColorType.Background).Equals(Colors.Black));
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
                case "logout":
                    Settings.Logout();
                    if (AccountLogout.Flyout is Flyout flyout_logout)
                    {
                        flyout_logout.Hide();
                    }
                    _ = Frame.Navigate(typeof(SettingPage));
                    Frame.GoBack();
                    break;
                case "reset":
                    {
                        ApplicationData.Current.LocalSettings.Values.Clear();
                        Settings.Logout();
                        if (reset.Flyout is Flyout flyout_reset)
                        {
                            flyout_reset.Hide();
                        }
                        _ = Frame.Navigate(typeof(SettingPage));
                        Frame.GoBack();
                    }
                    break;
                case "CleanCache":
                    CleanCacheButton.IsEnabled = CleanAllCacheButton.IsEnabled = RefreshCacheButton.IsEnabled = false;
                    CacheSizeTextBlock.Text = "正在处理……";
                    foreach (var item in CacheSizeListView.SelectedItems)
                        await ImageCache.CleanCache((item as CacheSizeViewModel).type);
                    CleanAllCacheButton.IsEnabled = true;
                    GetCacheSize();
                    CleanCacheButton.IsEnabled = RefreshCacheButton.IsEnabled = true;
                    break;
                case "CleanAllCache":
                    if (source != null) source.Cancel();
                    CleanCacheButton.IsEnabled = CleanAllCacheButton.IsEnabled = RefreshCacheButton.IsEnabled = false;
                    CacheSizeTextBlock.Text = "正在处理……";
                    for (int i = 0; i < 5; i++)
                        await ImageCache.CleanCache((ImageType)i);
                    CleanAllCacheButton.IsEnabled = true;
                    GetCacheSize();
                    CleanCacheButton.IsEnabled = RefreshCacheButton.IsEnabled = true;
                    break;
                case "AccountSetting":
                    Tools.Navigate(typeof(BrowserPage), new object[] { false, "https://account.coolapk.com/account/settings" });
                    break;
                case "MyDevice":
                    Tools.Navigate(typeof(BrowserPage), new object[] { false, "https://m.coolapk.com/mp/do?c=userDevice&m=myDevice" });
                    break;
                default:
                    break;
            }
        }

        private void CacheSizeListView_SelectionChanged(object sender, SelectionChangedEventArgs e) => CleanCacheButton.IsEnabled = (sender as ListView).SelectedItems.Count > 0;

        private void TitleBar_BackButtonClick(object sender, RoutedEventArgs e) => Frame.GoBack();
    }
}
