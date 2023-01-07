using CoolapkUWP.Helpers;
using CoolapkUWP.ViewModels.FeedPages;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace CoolapkUWP.Pages.FeedPages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class IndexPage : Page
    {
        private IndexViewModel Provider;

        public IndexPage() => InitializeComponent();

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is IndexViewModel ViewModel)
            {
                Provider = ViewModel;
                DataContext = Provider;
                Provider.OnLoadMoreStarted += UIHelper.ShowProgressBar;
                Provider.OnLoadMoreCompleted += UIHelper.HideProgressBar;
                await Refresh(true);
            }
            else
            {
                //TitleBar.Title = ResourceLoader.GetForCurrentView("MainPage").GetString("Home");
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            Provider.OnLoadMoreStarted -= UIHelper.ShowProgressBar;
            Provider.OnLoadMoreCompleted -= UIHelper.HideProgressBar;
        }

        private async Task Refresh(bool reset = false) => await Provider.Refresh(reset);

        //private void TitleBar_RefreshEvent(TitleBar sender, object e) => _ = Refresh(true);

        private async void ListView_RefreshRequested(object sender, EventArgs e) => await Refresh(true);
    }
}
