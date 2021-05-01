using CoolapkUWP.Helpers;
using CoolapkUWP.Pages.FeedPages;
using System.ComponentModel;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace CoolapkUWP.Pages
{
    public sealed partial class MainPageV7 : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null)
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void UIHelper_NeedMainPageProgressRing(object sender, bool e)
        {
            if (e)
            {
                FindName(nameof(progressRing));
            }
            progressRing.IsActive = e;
            if (!e)
            {
                UnloadObject(progressRing);
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            UIHelper.NeedMainPageProgressRing += UIHelper_NeedMainPageProgressRing;
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            UIHelper.NeedMainPageProgressRing -= UIHelper_NeedMainPageProgressRing;
            base.OnNavigatingFrom(e);
        }

        public MainPageV7()
        {
            this.InitializeComponent();
            navigationView.SelectedItem = navigationView.MenuItems[0];
        }

        private void NavigationView_SelectionChanged(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected)
            {
                if (Frame.CurrentSourcePageType != typeof(SettingPages.SettingPage))
                {
                    Frame.Navigate(typeof(SettingPages.SettingPage));
                }
            }
            else
            {
                var selectedItem = (Microsoft.UI.Xaml.Controls.NavigationViewItem)args.SelectedItem;
                if ((string)selectedItem.Tag == "V9_HOME_TAB_HEADLINE")
                    navigationViewFrame.Navigate(typeof(IndexPage), new ViewModels.IndexPage.ViewModel("/main/indexV8", false), args.RecommendedNavigationTransitionInfo);
                else if ((string)selectedItem.Tag == "V11_FIND_DYH")
                    navigationViewFrame.Navigate(typeof(IndexPage), new ViewModels.IndexPage.ViewModel("/user/dyhSubscribe", false), args.RecommendedNavigationTransitionInfo);
                else navigationViewFrame.Navigate(typeof(IndexPage), new ViewModels.IndexPage.ViewModel((string)selectedItem.Tag, false), args.RecommendedNavigationTransitionInfo);
                //UIHelper.ShowMessage((string)selectedItem.Tag);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            UIHelper.RefreshIndexPage();
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ApplicationView view = ApplicationView.GetForCurrentView();
            bool isInFullScreenMode = view.IsFullScreenMode;
            if (isInFullScreenMode)
                navigationView.Margin = new Thickness(0, 32, 0, 0);
            else navigationView.Margin = new Thickness(0, 0, 0, 0);
        }
    }
}