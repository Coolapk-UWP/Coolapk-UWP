using CoolapkUWP.Helpers;
using System;
using System.ComponentModel;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace CoolapkUWP.Pages
{
    public sealed partial class ShellPage : Page, INotifyPropertyChanged
    {
        private bool progressRingActived;

        public bool ProgressRingActived
        {
            get => progressRingActived;
            private set
            {
                progressRingActived = value;
                RaisePropertyChangedEvent();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null)
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public ShellPage()
        {
            this.InitializeComponent();
            Windows.ApplicationModel.Core.CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            if (SettingsHelper.Get<bool>(SettingsHelper.CheckUpdateWhenLuanching))
                SettingsHelper.CheckUpdate();
            SystemNavigationManager.GetForCurrentView().BackRequested += (sender, ee) =>
            {
                int i = UIHelper.popups.Count - 1;
                if (i >= 0)
                {
                    ee.Handled = true;
                    var popup = UIHelper.popups[i];
                    popup.IsOpen = false;
                    UIHelper.popups.Remove(popup);
                }
                else if (Frame.CanGoBack)
                {
                    ee.Handled = true;
                    Frame.GoBack();
                }
            };
            Windows.UI.Notifications.TileUpdateManager.CreateTileUpdaterForApplication().Clear();
            UIHelper.ProgressRingIsActiveChanged += async (s, b) =>
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => ProgressRingActived = b);
            };
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            shellFrame.Navigate(typeof(MainPage));
            UIHelper.MainFrame = shellFrame;
            UIHelper.InAppNotification = AppNotification;
        }
    }
}