using CoolapkUWP.ViewModels.FeedPages;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
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
        private static int PivotIndex = 0;

        private bool isLoaded;
        private Func<bool, Task> Refresh;

        public IndexPage() => InitializeComponent();

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            PivotIndex = Pivot.SelectedIndex;
        }

        private void Pivot_Loaded(object sender, RoutedEventArgs e)
        {
            if (!isLoaded)
            {
                Pivot.ItemsSource = GetMainItems();
                Pivot.SelectedIndex = PivotIndex;
                isLoaded = true;
            }
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
                _ = Frame.Navigate(typeof(AdaptivePage), new AdaptiveViewModel(url));
                Refresh = (reset) => _ = (Frame.Content as AdaptivePage).Refresh(reset);
            }
            else if ((Pivot.SelectedItem as PivotItem).Content is Frame __ && __.Content is AdaptivePage AdaptivePage)
            {
                Refresh = (reset) => _ = AdaptivePage.Refresh(reset);
            }
        }

        public static ObservableCollection<PivotItem> GetMainItems()
        {
            ResourceLoader loader = ResourceLoader.GetForCurrentView("IndexPage");
            ObservableCollection<PivotItem> items = new ObservableCollection<PivotItem>
            {
                new PivotItem() { Tag = "V9_HOME_TAB_HEADLINE", Header = loader.GetString("V9_HOME_TAB_HEADLINE"), Content = new Frame() },
                new PivotItem() { Tag = "V9_HOME_TAB_WENDA", Header = loader.GetString("V9_HOME_TAB_WENDA"), Content = new Frame() },
                new PivotItem() { Tag = "V11_FIND_COOLPIC", Header = loader.GetString("V11_FIND_COOLPIC"), Content = new Frame() },
                new PivotItem() { Tag = "V11_FIND_DYH", Header = loader.GetString("V11_FIND_DYH"), Content = new Frame() },
                new PivotItem() { Tag = "V9_HOME_TAB_RANKING", Header = loader.GetString("V9_HOME_TAB_RANKING"), Content = new Frame() },
                new PivotItem() { Tag = "V11_HOME_TAB_NEWS", Header = loader.GetString("V11_HOME_TAB_NEWS"), Content = new Frame() },
                new PivotItem() { Tag = "V11_HOME_TAB_JC", Header = loader.GetString("V11_HOME_TAB_JC"), Content = new Frame() },
                new PivotItem() { Tag = "V11_HOME_MEIHUA", Header = loader.GetString("V11_HOME_MEIHUA"), Content = new Frame() },
            };
            return items;
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e) => _ = Refresh(true);
    }
}
