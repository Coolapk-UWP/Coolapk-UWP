using CoolapkUWP.ViewModels.FeedPages;
using System.Collections.ObjectModel;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

#if FEATURE2
using GalaSoft.MvvmLight.Command;
#endif

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace CoolapkUWP.Pages.FeedPages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class IndexPage : Page
    {
        private static int PivotIndex = 0;

        public IndexPage() => InitializeComponent();

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            PivotIndex = Pivot.SelectedIndex;
        }

        private void Pivot_Loaded(object sender, RoutedEventArgs e)
        {
            Pivot.ItemsSource = GetMainItems();
            Pivot.SelectedIndex = PivotIndex;
        }

        private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PivotItem MenuItem = Pivot.SelectedItem as PivotItem;
            if ((Pivot.SelectedItem as PivotItem).Content is Frame Frame && Frame.Content is null)
            {
                string url = MenuItem.Tag.ToString() == "V9_HOME_TAB_HEADLINE"
                    ? "/main/indexV8"
                    : MenuItem.Tag.ToString() == "V11_FIND_DYH"
                        ? "/user/dyhSubscribe"
                        : $"/page?url={MenuItem.Tag}";
                Frame.Navigate(typeof(AdaptivePage), new AdaptiveViewModel(url));
                RelayCommand RefreshButtonCommand = new RelayCommand(async () => await (Frame.Content as AdaptivePage).Refresh(true));
                RefreshButton.Command = RefreshButtonCommand;
            }
            else if ((Pivot.SelectedItem as PivotItem).Content is Frame __ && __.Content is AdaptivePage AdaptivePage)
            {
                RelayCommand RefreshButtonCommand = new RelayCommand(async () => await AdaptivePage.Refresh(true));
                RefreshButton.Command = RefreshButtonCommand;
            }
        }

        public static ObservableCollection<PivotItem> GetMainItems()
        {
            ResourceLoader loader = ResourceLoader.GetForCurrentView("IndexPage");
            ObservableCollection<PivotItem> items = new ObservableCollection<PivotItem>
            {
                new PivotItem() { Tag = "V9_HOME_TAB_HEADLINE", Header = loader.GetString("V9_HOME_TAB_HEADLINE"), Content = new Frame() },
                new PivotItem() { Tag = "V9_HOME_TAB_RANKING", Header = loader.GetString("V9_HOME_TAB_RANKING"), Content = new Frame() },
            };
            return items;
        }
    }
}
