using CoolapkUWP.Data;
using CoolapkUWP.Pages.FeedPages;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace CoolapkUWP.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class UserHubPage : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        ImageSource userAvatar;
        string userName;
        double followNum;
        double fansNum;
        double feedNum;
        double levelNum;

        public UserHubPage() => this.InitializeComponent();
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            string uid = Settings.GetString("Uid");
            if (string.IsNullOrEmpty(uid))
            {
                LoginButton.Visibility = Visibility.Visible;
                UserDetailGrid.Visibility = Visibility.Collapsed;
            }
            else
            {
                LoginButton.Visibility = Visibility.Collapsed;
                UserDetailGrid.Visibility = Visibility.Visible;
                var o = Tools.GetJSonObject(await Tools.GetJson("/user/profile?uid=" + uid));
                Tools.mainPage.UserAvatar = userAvatar = await ImageCache.GetImage(ImageType.BigAvatar, o["userAvatar"].GetString());
                userName = o["username"].GetString();
                feedNum = o["feed"].GetNumber();
                followNum = o["follow"].GetNumber();
                fansNum = o["fans"].GetNumber();
                levelNum = o["level"].GetNumber();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(userAvatar)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(userName)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(fansNum)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(feedNum)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(followNum)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(levelNum)));
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e) => Tools.Navigate(typeof(Pages.BrowserPage), new object[] { true, null });

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as FrameworkElement).Tag as string)
            {
                case "logout":
                    Settings.Logout();
                    LoginButton.Visibility = Visibility.Visible;
                    UserDetailGrid.Visibility = Visibility.Collapsed;
                    break;
                case "feed":
                    Tools.Navigate(typeof(FeedListPage), new object[] { FeedListType.UserPageList, Settings.GetString("Uid") });
                    break;
                case "follow":
                    Tools.Navigate(typeof(UserListPage), new object[] { Settings.GetString("Uid"), true, userName });
                    break;
                case "fans":
                    Tools.Navigate(typeof(UserListPage), new object[] { Settings.GetString("Uid"), false, userName });
                    break;
                case "settings":
                    Tools.Navigate(typeof(SettingPages.SettingPage), null);
                    break;
            }
        }

    }
}
