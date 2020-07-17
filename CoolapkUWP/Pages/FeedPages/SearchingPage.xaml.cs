using CoolapkUWP.Helpers;
using CoolapkUWP.Helpers.Providers;
using CoolapkUWP.Models;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace CoolapkUWP.Pages.FeedPages
{
    public sealed partial class SearchingPage : Page
    {
        private ViewModels.SearchPage.ViewModel provider;

        public SearchingPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            provider = e.Parameter as ViewModels.SearchPage.ViewModel;

            SearchText.Text = provider.KeyWord;

            FeedList.ItemsSource = provider.providers[0].Models;
            UserList.ItemsSource = provider.providers[1].Models;
            TopicList.ItemsSource = provider.providers[2].Models;

            searchTypeComboBox.SelectedIndex = provider.TypeComboBoxSelectedIndex;

            await StartSearch();

            await Task.Delay(30);
            if (searchTypeComboBox.SelectedIndex > -1)
            {
                (FindName($"scrollViewer{searchTypeComboBox.SelectedIndex}") as ScrollViewer).ChangeView(null, provider.VerticalOffsets[0], null, true);
            }
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (searchTypeComboBox.SelectedIndex > -1)
            {
                provider.VerticalOffsets[0] = (FindName($"scrollViewer{searchTypeComboBox.SelectedIndex}") as ScrollViewer).VerticalOffset;
            }
            else
            {
                provider.VerticalOffsets[0] = 0;
            }

            base.OnNavigatingFrom(e);
        }

        private async Task StartSearch()
        {
            if (string.IsNullOrEmpty(SearchText?.Text))
            {
                detailPivot.Visibility = Visibility.Collapsed;
            }
            else if (detailPivot.SelectedIndex < provider.providers.Length)
            {
                progressRing.IsActive = true;
                progressRing.Visibility = Visibility.Visible;

                ViewModels.SearchPage.Base.SearchFeedTypeComboBoxSelectedIndex = SearchFeedTypeComboBox.SelectedIndex;
                ViewModels.SearchPage.Base.SearchFeedSortTypeComboBoxSelectedIndex = SearchFeedSortTypeComboBox.SelectedIndex;

                await provider.ChangeWordAndSearch(SearchText.Text, detailPivot.SelectedIndex);
                detailPivot.Visibility = Visibility.Visible;

                progressRing.Visibility = Visibility.Collapsed;
                progressRing.IsActive = false;
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e) => Frame.GoBack();

        private void SearchTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (provider == null) { return; }
            if (searchTypeComboBox.SelectedIndex != -1 && detailPivot != null)
            {
                detailPivot.SelectedIndex = searchTypeComboBox.SelectedIndex;
            }

            if (searchTypeComboBox.SelectedIndex + 1 == searchTypeComboBox.Items.Count || provider.providers[searchTypeComboBox.SelectedIndex].Page == 0)
            {
                _ = StartSearch();
            }
        }

        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            ScrollViewer viewer = sender as ScrollViewer;
            if (!e.IsIntermediate && viewer.VerticalOffset == viewer.ScrollableHeight)
            {
                _ = StartSearch();
            }
        }

        private void SearchFeedTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (provider == null) { return; }
            provider.providers[0].Reset();
            _ = StartSearch();
        }

        #region 搜索框相关

        private void SearchText_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            foreach (var item in provider.providers)
            {
                item.Reset();
            }
            StartSearch();
        }

        private async void SearchText_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                JArray array = (JArray)await DataHelper.GetDataAsync(UriProvider.GetUri(UriType.GetSearchWords, sender.Text), true);
                sender.ItemsSource = array != null && array.Count > 0 ? array.Select(i => new SearchWord(i as JObject)) : null;
            }
        }

        private void AutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            if (args.SelectedItem is SearchWord m)
                sender.Text = m.GetTitle();
        }

        #endregion 搜索框相关

        private void DetailPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (searchTypeComboBox.SelectedIndex != detailPivot.SelectedIndex)
            {
                searchTypeComboBox.SelectedIndex = detailPivot.SelectedIndex;
                if (searchTypeComboBox.SelectedIndex + 1 == searchTypeComboBox.Items.Count || provider.providers[searchTypeComboBox.SelectedIndex].Page == 0)
                    StartSearch();
            }
        }
    }
}