using CoolapkUWP.Helpers;
using CoolapkUWP.Pages;
using CoolapkUWP.Pages.FeedPages;
using Newtonsoft.Json.Linq;
using System.ComponentModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace CoolapkUWP.Controls
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
        double nextLevelExperience;
        double nextLevelPercentage;
        string nextLevelNowExperience;
        string levelTodayMessage;

        public UserHub() => this.InitializeComponent();

        private void LoginButton_Click(object sender, RoutedEventArgs e) => UIHelper.Navigate(typeof(BrowserPage), new object[] { true, null });

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as FrameworkElement).Tag as string)
            {
                case "feed":
                    UIHelper.Navigate(typeof(FeedListPage), new object[] { FeedListType.UserPageList, SettingsHelper.Get<string>("Uid") });
                    break;
                case "follow":
                    UIHelper.Navigate(typeof(UserListPage), new object[] { SettingsHelper.Get<string>("Uid"), true, userName });
                    break;
                case "fans":
                    UIHelper.Navigate(typeof(UserListPage), new object[] { SettingsHelper.Get<string>("Uid"), false, userName });
                    break;
                case "settings":
                    UIHelper.Navigate(typeof(Pages.SettingPages.SettingPage));
                    break;
                case "logout":
                    SettingsHelper.Logout();
                    LoginButton.Visibility = Visibility.Visible;
                    LogoutButton.Visibility = UserDetailGrid.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            string uid = SettingsHelper.Get<string>("Uid");
            if (string.IsNullOrEmpty(uid))
            {
                LoginButton.Visibility = Visibility.Visible;
                LogoutButton.Visibility = UserDetailGrid.Visibility = Visibility.Collapsed;
            }
            else
            {
                LoginButton.Visibility = Visibility.Collapsed;
                UserDetailGrid.Visibility = Visibility.Visible;
                var o = (JObject)await DataHelper.GetData(DataType.GetUserProfile, uid);
                UIHelper.MainPageUserAvatar = userAvatar = await ImageCacheHelper.GetImage(ImageType.BigAvatar, o.Value<string>("userAvatar"));
                userName = o.Value<string>("username");
                feedNum = o.Value<int>("feed");
                followNum = o.Value<int>("follow");
                fansNum = o.Value<int>("fans");
                levelNum = o.Value<int>("level");
                nextLevelExperience = o.Value<int>("next_level_experience");
                nextLevelPercentage = o.Value<double>("next_level_percentage");
                levelTodayMessage = o.Value<string>("level_today_message");
                nextLevelNowExperience = $"{nextLevelPercentage / 100 * nextLevelExperience:F0}/{nextLevelExperience}";
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(userAvatar)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(userName)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(fansNum)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(feedNum)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(followNum)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(levelNum)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(nextLevelPercentage)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(levelTodayMessage)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(nextLevelNowExperience)));
            }

        }
    }
}
