using CoolapkUWP.Helpers;
using Windows.UI.Xaml.Controls;
using Windows.Web.Http;
using Windows.Web.Http.Filters;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace CoolapkUWP.Controls
{
    public sealed partial class LoginDialog : ContentDialog
    {
        private string _UID = SettingsHelper.Get<string>(SettingsHelper.Uid);
        internal string UID
        {
            get => _UID;
            set
            {
                if (_UID != value)
                {
                    _UID = value;
                    CheckText();
                }
            }
        }

        private string userName = SettingsHelper.Get<string>(SettingsHelper.UserName);
        internal string UserName
        {
            get => userName;
            set
            {
                if (userName != value)
                {
                    userName = value;
                    CheckText();
                }
            }
        }

        private string token = SettingsHelper.Get<string>(SettingsHelper.Token);
        internal string Token
        {
            get => token;
            set
            {
                if (token != value)
                {
                    token = value;
                    CheckText();
                }
            }
        }

        public LoginDialog()
        {
            InitializeComponent();
            CheckText();
        }

        private void OnClosing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            if (args.Result == ContentDialogResult.Primary)
            {
                if (string.IsNullOrWhiteSpace(UID) && !string.IsNullOrWhiteSpace(UserName))
                {
                    GetText(UserName);
                }
                else if (string.IsNullOrWhiteSpace(UserName) && !string.IsNullOrWhiteSpace(UID))
                {
                    GetText(UID);
                }
                using (HttpBaseProtocolFilter filter = new HttpBaseProtocolFilter())
                {
                    HttpCookieManager cookieManager = filter.CookieManager;
                    HttpCookie uid = new HttpCookie("uid", ".coolapk.com", "/");
                    HttpCookie username = new HttpCookie("username", ".coolapk.com", "/");
                    HttpCookie token = new HttpCookie("token", ".coolapk.com", "/");
                    uid.Value = UID;
                    username.Value = UserName;
                    token.Value = Token;
                    cookieManager.SetCookie(uid);
                    cookieManager.SetCookie(username);
                    cookieManager.SetCookie(token);
                }
            }
        }

        private void CheckText() => IsPrimaryButtonEnabled = !string.IsNullOrEmpty(Token) && (!string.IsNullOrEmpty(UID) || !string.IsNullOrEmpty(UserName));

        private void GetText(string name)
        {
            (string UID, string UserName, string UserAvatar) results = UIHelper.AwaitByTaskCompleteSource(() => NetworkHelper.GetUserInfoByNameAsync(name));
            if (!string.IsNullOrWhiteSpace(results.UID))
            {
                UID = results.UID;
            }
            if (!string.IsNullOrWhiteSpace(results.UserName))
            {
                UserName = results.UserName;
            }
        }
    }
}
