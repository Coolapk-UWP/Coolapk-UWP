using CoolapkUWP.Core.Helpers;
using CoolapkUWP.Helpers;
using CoolapkUWP.Models;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace CoolapkUWP.Pages.FeedPages
{
    public sealed partial class SearchingPage : Page
    {
        private ViewModels.SearchPage.ViewModel provider;

        public SearchingPage()
        {
            InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            provider = e.Parameter as ViewModels.SearchPage.ViewModel;

            SearchText.Text = provider.KeyWord;

            FeedList.ItemsSource = provider.providers[0].Models;
            UserList.ItemsSource = provider.providers[1].Models;
            TopicList.ItemsSource = provider.providers[2].Models;
            //ProductList.ItemsSource = provider.providers[3].Models;

            searchTypeComboBox.SelectedIndex = provider.TypeComboBoxSelectedIndex;

            await StartSearch();

            await Task.Delay(30);
            if (searchTypeComboBox.SelectedIndex > -1)
            {
                _ = (FindName($"scrollViewer{searchTypeComboBox.SelectedIndex}") as ScrollViewer).ChangeView(null, provider.VerticalOffsets[0], null, true);
            }
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            provider.VerticalOffsets[0] = searchTypeComboBox.SelectedIndex > -1
                ? (FindName($"scrollViewer{searchTypeComboBox.SelectedIndex}") as ScrollViewer).VerticalOffset
                : 0;

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
            if (args.ChosenSuggestion is AppModel app)
            {
                UIHelper.NavigateInSplitPane(typeof(AppPages.AppPage), "https://www.coolapk.com" + app.Url);
            }
            else
            {
                foreach (Core.Providers.SearchListProvider item in provider.providers)
                {
                    item.Reset();
                }
                _ = StartSearch();
            }
        }

        internal static async void SearchText_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                (bool isSucceed, JToken result) = await DataHelper.GetDataAsync(UriHelper.GetUri(UriType.GetSearchWords, sender.Text), true);
                if (isSucceed && result != null && result is JArray array && array.Count > 0)
                {
                    ObservableCollection<object> observableCollection = new ObservableCollection<object>();
                    sender.ItemsSource = observableCollection;
                    foreach (JToken token in array)
                    {
                        switch (token.Value<string>("entityType"))
                        {
                            case "apk":
                                observableCollection.Add(new AppModel(token as JObject));
                                break;
                            case "searchWord":
                            default:
                                observableCollection.Add(new SearchWord(token as JObject));
                                break;
                        }
                    }
                }
            }
        }

        internal static void AutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            if (args.SelectedItem is SearchWord m) { sender.Text = m.GetTitle(); }
        }

        #endregion 搜索框相关

        private void DetailPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (searchTypeComboBox.SelectedIndex != detailPivot.SelectedIndex)
            {
                searchTypeComboBox.SelectedIndex = detailPivot.SelectedIndex;
                if (searchTypeComboBox.SelectedIndex + 1 == searchTypeComboBox.Items.Count || provider.providers[searchTypeComboBox.SelectedIndex].Page == 0)
                {
                    _ = StartSearch();
                }
            }
        }
    }
}