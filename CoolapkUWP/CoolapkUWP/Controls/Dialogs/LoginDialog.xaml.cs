using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Exceptions;
using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

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
            Closing += OnClosing;
            CheckText();
        }

        private void OnClosing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            UIHelper.ShowProgressBar();
            if (args.Result == ContentDialogResult.Primary)
            {
                args.Cancel = true;
                ResourceLoader loader = ResourceLoader.GetForCurrentView("BrowserPage");
                if (UIHelper.AwaitByTaskCompleteSource(CheckLogin))
                {
                    _ = Dispatcher.AwaitableRunAsync(() => { UIHelper.ShowMessage(loader.GetString("LoginSuccessfully")); });
                    args.Cancel = false;
                }
                else
                {
                    _ = Dispatcher.AwaitableRunAsync(() => { UIHelper.ShowMessage(loader.GetString("LoginFailed")); });
                }
            }
            UIHelper.HideProgressBar();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e) => CheckText();

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e) => CheckText();

        private async Task<bool> CheckLogin()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(UID) && !string.IsNullOrWhiteSpace(UserName))
                {
                    await GetText(UserName);
                }
                else if (string.IsNullOrWhiteSpace(UserName) && !string.IsNullOrWhiteSpace(UID))
                {
                    await GetText(UID);
                }
                return await SettingsHelper.Login(UID, UserName, Token);
            }
            catch (CoolapkMessageException message)
            {
                _ = Dispatcher.AwaitableRunAsync(() => { UIHelper.ShowMessage(message.Message); });
                return false;
            }
            catch (HttpRequestException hex)
            {
                SettingsHelper.LogManager.GetLogger(nameof(LoginDialog)).Warn(hex.ExceptionToMessage(), hex);
                _ = Dispatcher.AwaitableRunAsync(() => { UIHelper.ShowHttpExceptionMessage(hex); });
                return false;
            }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.GetLogger(nameof(LoginDialog)).Error(ex.ExceptionToMessage(), ex);
                _ = Dispatcher.AwaitableRunAsync(() => { UIHelper.ShowMessage(ex.ExceptionToMessage()); });
                return false;
            }
        }

        private void CheckText() => IsPrimaryButtonEnabled = !string.IsNullOrEmpty(Token) && (!string.IsNullOrEmpty(UID) || !string.IsNullOrEmpty(UserName));

        private async Task GetText(string name)
        {
            (string UID, string UserName, string UserAvatar) results = await NetworkHelper.GetUserInfoByNameAsync(name, true);
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
