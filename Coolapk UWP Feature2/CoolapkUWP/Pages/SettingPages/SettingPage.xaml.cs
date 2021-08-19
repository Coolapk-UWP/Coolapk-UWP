using CoolapkUWP.Data;
using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace CoolapkUWP.Pages.SettingPages
{
    internal class CacheSizeViewModel : INotifyPropertyChanged
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
- [Color Thief](https://github.com/KSemenenko/ColorThief)
- [Html2Markdown](https://github.com/baynezy/Html2Markdown)
";
        }

        private CancellationTokenSource source;
        private readonly ObservableCollection<CacheSizeViewModel> models = new ObservableCollection<CacheSizeViewModel>();

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            object vs = e.Parameter;
            if (vs is bool boolean && !boolean)
            {
                TitleBar.Visibility = Visibility.Visible;
                listView.Padding = SettingsHelper.StackPanelMargin;
            }
            IsNoPicsMode.IsOn = SettingsHelper.GetBoolen("IsNoPicsMode");
            IsUseOldEmojiMode.IsOn = SettingsHelper.GetBoolen("IsUseOldEmojiMode");
            IsDarkMode.IsOn = SettingsHelper.GetBoolen("IsDarkMode");
            CheckUpdateWhenLuanching.IsOn = SettingsHelper.GetBoolen("CheckUpdateWhenLuanching");
            IsBackgroundColorFollowSystem.IsOn = SettingsHelper.GetBoolen("IsBackgroundColorFollowSystem");
            IsDarkMode.Visibility = IsBackgroundColorFollowSystem.IsOn ? Visibility.Collapsed : Visibility.Visible;
            VersionTextBlock.Text = $"{Package.Current.Id.Version.Major}.{Package.Current.Id.Version.Minor}.{Package.Current.Id.Version.Build}";
#if DEBUG
            gotoTestPage.Visibility = Visibility.Visible;
#endif
            _ = GetCacheSize();
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            if (source != null)
            {
                source.Cancel();
            }
        }

        private void MarkdownText_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            if (Uri.TryCreate(e.Link, UriKind.Absolute, out Uri link))
            {
                string str = link.ToString();
                UIHelper.OpenLink(str);
            }
        }

        private async Task GetCacheSize()
        {
            RefreshIcon.Visibility = Visibility.Collapsed;
            IsRefreshCache.Visibility = Visibility.Visible;
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
                catch { UIHelper.ShowMessage("缓存信息获取失败"); }
                size += s;
                models.Add(new CacheSizeViewModel
                {
                    TotalSize = s,
                    Size = s,
                    SizeString = s.GetSizeString(),
                    Title = name,
                    type = (ImageType)i
                });
                source = null;
            }
            CacheSizeTextBlock.Text = size.GetSizeString();
            foreach (CacheSizeViewModel item in models)
            { item.TotalSize = size; }
            RefreshCacheButton.IsEnabled = true;
            CacheSizeListView.SelectionMode = ListViewSelectionMode.Multiple;
            IsRefreshCache.Visibility = Visibility.Collapsed;
            RefreshIcon.Visibility = Visibility.Visible;
        }

        private void ToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            ToggleSwitch toggle = sender as ToggleSwitch;
            SettingsHelper.Set(toggle.Name, toggle.IsOn);
            switch (toggle.Name)
            {
                case "IsDarkMode":
                    SettingsHelper.CheckTheme();
                    break;
                case "IsBackgroundColorFollowSystem":
                    SettingsHelper.Set("IsDarkMode", SettingsHelper.UISetting.GetColorValue(UIColorType.Background).Equals(Colors.Black));
                    SettingsHelper.CheckTheme();
                    IsDarkMode.IsOn = SettingsHelper.GetBoolen("IsDarkMode");
                    IsDarkMode.Visibility = toggle.IsOn ? Visibility.Collapsed : Visibility.Visible;
                    break;
                default:
                    break;
            }
        }

        private async void CheckUpdate()
        {
            IsCheckUpdate.Visibility = Visibility.Visible;
            await SettingsHelper.CheckUpdate();
            IsCheckUpdate.Visibility = Visibility.Collapsed;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            switch (button.Tag as string)
            {
                case "gotoTestPage": UIHelper.Navigate(typeof(TestPage), null); break;
                case "checkUpdate": CheckUpdate(); break;
                case "RefreshCache": _ = GetCacheSize(); break;
                case "logout":
                    SettingsHelper.Logout();
                    if (AccountLogout.Flyout is Flyout flyout_logout)
                    {
                        flyout_logout.Hide();
                        UIHelper.ShowMessage("已注销");
                    }
                    break;
                case "reset":
                    {
                        ApplicationData.Current.LocalSettings.Values.Clear();
                        SettingsHelper.Logout();
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
                    RefreshIcon.Visibility = Visibility.Collapsed;
                    IsRefreshCache.Visibility = Visibility.Visible;
                    CacheSizeListView.SelectionMode = ListViewSelectionMode.None;
                    CacheSizeTextBlock.Text = "正在处理……";
                    foreach (object item in CacheSizeListView.SelectedItems)
                    { await ImageCache.CleanCache((item as CacheSizeViewModel).type); }
                    CleanAllCacheButton.IsEnabled = true;
                    await GetCacheSize();
                    IsRefreshCache.Visibility = Visibility.Collapsed;
                    RefreshIcon.Visibility = Visibility.Visible;
                    CleanCacheButton.IsEnabled = RefreshCacheButton.IsEnabled = true;
                    break;
                case "CleanAllCache":
                    if (source != null) { source.Cancel(); }
                    CleanCacheButton.IsEnabled = CleanAllCacheButton.IsEnabled = RefreshCacheButton.IsEnabled = false;
                    RefreshIcon.Visibility = Visibility.Collapsed;
                    IsRefreshCache.Visibility = Visibility.Visible;
                    CacheSizeListView.SelectionMode = ListViewSelectionMode.None;
                    CacheSizeTextBlock.Text = "正在处理……";
                    for (int i = 0; i < 5; i++)
                    { await ImageCache.CleanCache((ImageType)i); }
                    CleanAllCacheButton.IsEnabled = true;
                    await GetCacheSize();
                    IsRefreshCache.Visibility = Visibility.Collapsed;
                    RefreshIcon.Visibility = Visibility.Visible;
                    CleanCacheButton.IsEnabled = RefreshCacheButton.IsEnabled = true;
                    break;
                case "AccountSetting":
                    UIHelper.Navigate(typeof(BrowserPage), new object[] { false, "https://account.coolapk.com/account/settings" });
                    break;
                case "MyDevice":
                    UIHelper.Navigate(typeof(BrowserPage), new object[] { false, "https://m.coolapk.com/mp/do?c=userDevice&m=myDevice" });
                    break;
                default:
                    break;
            }
        }

        private void CacheSizeListView_SelectionChanged(object sender, SelectionChangedEventArgs e) => CleanCacheButton.IsEnabled = (sender as ListView).SelectedItems.Count > 0;

        private void TitleBar_BackButtonClick(object sender, RoutedEventArgs e) => Frame.GoBack();
    }
}
