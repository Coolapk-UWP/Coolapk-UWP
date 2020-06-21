using CoolapkUWP.Controls;
using CoolapkUWP.Helpers;
using System.ComponentModel;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace CoolapkUWP.Pages
{
    public sealed partial class ShellPage : Page, INotifyPropertyChanged
    {
        private Symbol paneOpenSymbolIcon = Symbol.ClosePane;

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null)
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public Symbol PaneOpenSymbolIcon
        {
            get => paneOpenSymbolIcon;
            private set
            {
                paneOpenSymbolIcon = value;
                RaisePropertyChangedEvent();
            }
        }

        public ShellPage()
        {
            this.InitializeComponent();
            Windows.ApplicationModel.Core.CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            if (SettingsHelper.Get<bool>(SettingsHelper.CheckUpdateWhenLuanching))
                SettingsHelper.CheckUpdate();
            SystemNavigationManager.GetForCurrentView().BackRequested += (sender, ee) =>
            {
                int i = UIHelper.Popups.Count - 1;
                if (i > 0)
                {
                    ee.Handled = true;
                    var popup = UIHelper.Popups[i];
                    popup.IsOpen = false;
                    UIHelper.Popups.Remove(popup);
                }
                else if (Frame.CanGoBack)
                {
                    ee.Handled = true;
                    Frame.GoBack();
                }
            };
            Windows.UI.Notifications.TileUpdateManager.CreateTileUpdaterForApplication().Clear();
            UIHelper.ShowPopup(new Popup { Child = new NotifyPopup() });
            UIHelper.IsSplitViewPaneOpenedChanged += (s, e) =>
            {
                splitView.IsPaneOpen = e;
                PaneOpenSymbolIcon = e ? Symbol.OpenPane : Symbol.ClosePane;
            };
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            shellFrame.Navigate(typeof(MainPage));
            UIHelper.MainFrame = shellFrame;
            UIHelper.PaneFrame = paneFrame;
        }

        private void Button_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            splitView.IsPaneOpen = !splitView.IsPaneOpen;
            PaneOpenSymbolIcon = splitView.IsPaneOpen ? Symbol.OpenPane : Symbol.ClosePane;
        }
    }
}