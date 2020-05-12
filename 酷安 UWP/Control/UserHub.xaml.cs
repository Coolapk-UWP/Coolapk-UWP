using CoolapkUWP.Data;
using CoolapkUWP.Pages;
using CoolapkUWP.Pages.FeedPages;
using System.ComponentModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace CoolapkUWP.Control
{
    public sealed partial class UserHub : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        ImageSource userAvatar;
        string userName;
        double followNum;
        double fansNum;
        double feedNum;
        double levelNum;

        public UserHub() => this.InitializeComponent();

        private void LoginButton_Click(object sender, RoutedEventArgs e) => Tools.Navigate(typeof(BrowserPage), new object[] { true, null });

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as FrameworkElement).Tag as string)
            {
                case "feed":
                    Tools.Navigate(typeof(FeedListPage), new object[] { FeedListType.UserPageList, Settings.Get<string>("Uid") });
                    break;
                case "follow":
                    Tools.Navigate(typeof(UserListPage), new object[] { Settings.Get<string>("Uid"), true, userName });
                    break;
                case "fans":
                    Tools.Navigate(typeof(UserListPage), new object[] { Settings.Get<string>("Uid"), false, userName });
                    break;
                case "settings":
                    Tools.Navigate(typeof(Pages.SettingPages.SettingPage));
                    break;
                case "logout":
                    Settings.Logout();
                    LoginButton.Visibility = Visibility.Visible;
                    LogoutButton.Visibility = UserDetailGrid.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            string uid = Settings.Get<string>("Uid");
            if (string.IsNullOrEmpty(uid))
            {
                LoginButton.Visibility = Visibility.Visible;
                LogoutButton.Visibility = UserDetailGrid.Visibility = Visibility.Collapsed;
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
    }
}
