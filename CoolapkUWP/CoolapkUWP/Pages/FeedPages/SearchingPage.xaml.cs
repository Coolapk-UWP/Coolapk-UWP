using CoolapkUWP.Helpers;
using CoolapkUWP.Models;
using CoolapkUWP.ViewModels.DataSource;
using CoolapkUWP.ViewModels.FeedPages;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using muxc = Microsoft.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace CoolapkUWP.Pages.FeedPages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SearchingPage : Page
    {
        private static int PivotIndex = 0;
        private SearchingViewModel Provider;

        private Action Refresh;

        public SearchingPage() => InitializeComponent();

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is SearchingViewModel ViewModel)
            {
                Provider = ViewModel;
                DataContext = Provider;
                if (Provider.PivotIndex != -1)
                { PivotIndex = Provider.PivotIndex; }
                await Provider.Refresh(true);
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            PivotIndex = Pivot.SelectedIndex;
        }

        private void Pivot_Loaded(object sender, RoutedEventArgs e)
        {
            Pivot.SelectedIndex = PivotIndex;
        }

        private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PivotItem MenuItem = Pivot.SelectedItem as PivotItem;
            if ((Pivot.SelectedItem as PivotItem).Content is muxc.RefreshContainer RefreshContainer
                && RefreshContainer.Content is ListView ListView
                && ListView.ItemsSource is EntityItemSourse ItemsSource)
            {
                Refresh = () => _ = ItemsSource.Refresh(true);
            }
            RightHeader.Visibility = Pivot.SelectedIndex == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void RefreshContainer_RefreshRequested(muxc.RefreshContainer sender, muxc.RefreshRequestedEventArgs args)
        {
            if (sender.Content is ListView ListView && ListView.ItemsSource is EntityItemSourse ItemsSource)
            {
                _ = ItemsSource.Refresh(true);
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            if (Refresh != null)
            {
                Refresh();
            }
            else if ((Pivot.SelectedItem as PivotItem).Content is muxc.RefreshContainer RefreshContainer
                && RefreshContainer.Content is ListView ListView
                && ListView.ItemsSource is EntityItemSourse ItemsSource)
            {
                _ = ItemsSource.Refresh(true);
            }
        }
    }
}
