using CoolapkUWP.Control;
using CoolapkUWP.Data;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Foundation.Metadata;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace CoolapkUWP.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class RootPage : Page
    {
        public RootPage()
        {
            this.InitializeComponent();
            Tools.rootPage = this;
            if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Desktop")
            {
                Windows.ApplicationModel.Core.CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
                var view = ApplicationView.GetForCurrentView().TitleBar;
                view.ButtonBackgroundColor = view.ButtonInactiveBackgroundColor = Colors.Transparent;

            }
            else statusGrid.Visibility = Visibility.Collapsed;
            Application.Current.LeavingBackground += Current_Resuming;
            Application.Current.Resuming += Current_Resuming;
            new UISettings().ColorValuesChanged += Current_Resuming;

            try
            {
                Settings.InitializeSettings();
                if (Settings.GetBoolen("CheckUpdateWhenLuanching")) Settings.CheckUpdate();
            }
            catch { }

            SystemNavigationManager.GetForCurrentView().BackRequested += (sender, ee) =>
            {
                if (!ShowImageControl.Hide)
                    ShowImageControl.Hide = true;
                else if (rootFrame.CanGoBack)
                {
                    ee.Handled = true;
                    rootFrame.GoBack();
                }
            };
            rootFrame.Navigate(typeof(MainPage));
        }

        private async void Current_Resuming(object sender, object e)
        {
            if (Settings.GetBoolen("IsBackgroundColorFollowSystem"))
            {
                Settings.Set("IsDarkMode", new UISettings().GetColorValue(UIColorType.Background).Equals(Colors.Black) ? true : false);
                await Dispatcher.RunAsync(CoreDispatcherPriority.High, () => Settings.CheckTheme());
            }
        }

        public async void ShowMessage(string message)
        {
            if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Desktop")
            {
                messageTextBlock.Text = message;
                await Task.Delay(3000);
                messageTextBlock.Text = string.Empty;
            }
            else if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                StatusBar statusBar = StatusBar.GetForCurrentView();
                await statusBar.ProgressIndicator.ShowAsync();
                statusBar.ProgressIndicator.Text = message;
                await Task.Delay(3000);
                await statusBar.ProgressIndicator.HideAsync();
                statusBar.ProgressIndicator.Text = string.Empty;
            }
        }

        public void ShowHttpExceptionMessage(HttpRequestException e)
        {
            if (e.Message.IndexOfAny(new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' }) != -1)
                ShowMessage($"服务器错误： {e.Message.Replace("Response status code does not indicate success: ", string.Empty)}");
            else if (e.Message == "An error occurred while sending the request.") ShowMessage("无法连接网络。");
            else ShowMessage($"请检查网络连接。 {e.Message}");
        }

        public void ShowImage(string url) => ShowImageControl.ShowImage(url);

        public void Navigate(Type pageType, object e) => rootFrame.Navigate(pageType, e);

        public async void ShowProgressBar()
        {
            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
                await StatusBar.GetForCurrentView().ProgressIndicator.ShowAsync();

            else
            {
                statusBar.Visibility = Visibility.Visible;
                statusBar.IsIndeterminate = true;
            }
        }

        public async void HideProgressBar()
        {
            if (!ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                statusBar.Visibility = Visibility.Collapsed;
                statusBar.IsIndeterminate = false;
            }
            else if (string.IsNullOrEmpty(StatusBar.GetForCurrentView().ProgressIndicator.Text))
                await StatusBar.GetForCurrentView().ProgressIndicator.HideAsync();
        }
    }
}
