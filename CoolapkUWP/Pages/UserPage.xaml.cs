using CoolapkUWP.Helpers;
using CoolapkUWP.Pages.FeedPages;
using CoolapkUWP.ViewModels.FeedListDataProvider;
using Newtonsoft.Json.Linq;
using System.ComponentModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace CoolapkUWP.Pages
{
    public sealed partial class UserPage : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public readonly Models.Controls.UserHubModel model = new Models.Controls.UserHubModel();

        private Visibility logoutButtonVisibility;

        public Visibility LogoutButtonVisibility
        {
            get => logoutButtonVisibility;
            private set
            {
                logoutButtonVisibility = value;
                RaisePropertyChangedEvent();
            }
        }

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null)
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public UserPage()
        {
            this.InitializeComponent();
            Loaded += async (s, e) =>
            {
                string uid = SettingsHelper.Get<string>(SettingsHelper.Uid);
                if (string.IsNullOrEmpty(uid))
                {
                    LogoutButtonVisibility = Visibility.Collapsed;
                }
                else
                {
                    LogoutButtonVisibility = Visibility.Visible;
                    var o = (JObject)await DataHelper.GetDataAsync(DataUriType.GetUserProfile, uid);
                    string url = o.Value<string>("userAvatar");
                    var bitmapImage = await ImageCacheHelper.GetImageAsync(ImageType.BigAvatar, url);
                    model.Initialize(o, bitmapImage);
                    if (url != SettingsHelper.Get<string>(SettingsHelper.UserAvatar))
                    {
                        SettingsHelper.Set(SettingsHelper.UserAvatar, url);
                        UIHelper.RaiseUserAvatarChangedEvent(bitmapImage);
                    }
                }
            };
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as FrameworkElement).Tag as string)
            {
                case "feed":
                    var f = FeedListDataProvider.GetProvider(FeedListType.UserPageList, SettingsHelper.Get<string>(SettingsHelper.Uid));
                    if (f != null)
                        UIHelper.Navigate(typeof(FeedListPage), f);
                    break;

                case "follow":
                    Frame.Navigate(typeof(UserListPage), new object[] { SettingsHelper.Get<string>(SettingsHelper.Uid), true, model?.UserName ?? string.Empty });
                    break;

                case "fans":
                    Frame.Navigate(typeof(UserListPage), new object[] { SettingsHelper.Get<string>(SettingsHelper.Uid), false, model?.UserName ?? string.Empty });
                    break;

                case "settings":
                    Frame.Navigate(typeof(SettingPages.SettingPage));
                    break;

                case "login":
                    UIHelper.Navigate(typeof(BrowserPage), new object[] { true, null });
                    break;

                case "logout":
                    SettingsHelper.Logout();
                    LogoutButtonVisibility = Visibility.Collapsed;
                    break;
            }
        }
    }
}