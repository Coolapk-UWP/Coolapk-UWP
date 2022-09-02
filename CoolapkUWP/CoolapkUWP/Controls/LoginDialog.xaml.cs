using CoolapkUWP.Helpers;
using System;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Controls;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace CoolapkUWP.Controls
{
    public sealed partial class LoginDialog : ContentDialog
    {
        private string UID = SettingsHelper.Get<string>(SettingsHelper.Uid);
        private string UserName = SettingsHelper.Get<string>(SettingsHelper.UserName);
        private string Token = SettingsHelper.Get<string>(SettingsHelper.Token);

        public LoginDialog()
        {
            InitializeComponent();
            PrimaryButtonClick += OnPrimaryButtonClick;
        }

        private void OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            ResourceLoader loader = ResourceLoader.GetForCurrentView("BrowserPage");
            if (SettingsHelper.LoginIn(UID, UserName, Token))
            {
                UIHelper.ShowMessage(loader.GetString("LoginSuccessfully"));
            }
            else
            {
                UIHelper.ShowMessage("登录失败");
            }
        }
    }
}
