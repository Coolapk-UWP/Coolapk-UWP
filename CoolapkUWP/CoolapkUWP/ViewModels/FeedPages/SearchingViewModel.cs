using CoolapkUWP.Helpers;
using CoolapkUWP.Models;
using CoolapkUWP.Models.Feeds;
using CoolapkUWP.Models.Users;
using CoolapkUWP.ViewModels.DataSource;
using CoolapkUWP.ViewModels.Providers;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace CoolapkUWP.ViewModels.FeedPages
{
    public class SearchingViewModel : IViewModel
    {
        public int PivotIndex = -1;

        private string title = string.Empty;
        public string Title
        {
            get => title;
            set
            {
                title = value;
                RaisePropertyChangedEvent();
            }
        }

        private SearchFeedItemSource searchFeedItemSource;
        public SearchFeedItemSource SearchFeedItemSource
        {
            get => searchFeedItemSource;
            private set
            {
                searchFeedItemSource = value;
                RaisePropertyChangedEvent();
            }
        }

        private SearchUserItemSource searchUserItemSource;
        public SearchUserItemSource SearchUserItemSource
        {
            get => searchUserItemSource;
            private set
            {
                searchUserItemSource = value;
                RaisePropertyChangedEvent();
            }
        }

        private SearchTopicItemSource searchTopicItemSource;
        public SearchTopicItemSource SearchTopicItemSource
        {
            get => searchTopicItemSource;
            private set
            {
                searchTopicItemSource = value;
                RaisePropertyChangedEvent();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        public SearchingViewModel(string keyword, int index = -1)
        {
            Title = keyword;
            PivotIndex = index;
        }

        public async Task Refresh(bool reset = false)
        {
            if (reset)
            {
                List<PivotItem> ItemSource = new List<PivotItem>();
                if (SearchFeedItemSource == null)
                {
                    SearchFeedItemSource = new SearchFeedItemSource(Title);
                    SearchFeedItemSource.LoadMoreStarted += UIHelper.ShowProgressBar;
                    SearchFeedItemSource.LoadMoreCompleted += UIHelper.HideProgressBar;
                }
                else if (SearchFeedItemSource.Keyword != Title)
                {
                    SearchFeedItemSource.Keyword = Title;
                }
                if (SearchUserItemSource == null)
                {
                    SearchUserItemSource = new SearchUserItemSource(Title);
                    SearchUserItemSource.LoadMoreStarted += UIHelper.ShowProgressBar;
                    SearchUserItemSource.LoadMoreCompleted += UIHelper.HideProgressBar;
                }
                else if (SearchUserItemSource.Keyword != Title)
                {
                    SearchUserItemSource.Keyword = Title;
                }
                if (SearchTopicItemSource == null)
                {
                    SearchTopicItemSource = new SearchTopicItemSource(Title);
                    SearchTopicItemSource.LoadMoreStarted += UIHelper.ShowProgressBar;
                    SearchTopicItemSource.LoadMoreCompleted += UIHelper.HideProgressBar;
                }
                else if (SearchTopicItemSource.Keyword != Title)
                {
                    SearchTopicItemSource.Keyword = Title;
                }
            }
            await SearchFeedItemSource?.Refresh(reset);
            await SearchUserItemSource?.Refresh(reset);
            await SearchTopicItemSource?.Refresh(reset);
        }

        bool IViewModel.IsEqual(IViewModel other) => other is SearchingViewModel model && IsEqual(model);

        public bool IsEqual(SearchingViewModel other) => Title == other.Title;
    }

    public class SearchFeedItemSource : EntityItemSource, INotifyPropertyChanged
    {
        public string Keyword;

        private int searchFeedTypeComboBoxSelectedIndex = 0;
        public int SearchFeedTypeComboBoxSelectedIndex
        {
            get => searchFeedTypeComboBoxSelectedIndex;
            set
            {
                searchFeedTypeComboBoxSelectedIndex = value;
                RaisePropertyChangedEvent();
                UpdateProvider();
                _ = Refresh(true);
            }
        }

        private int searchFeedSortTypeComboBoxSelectedIndex = 0;
        public int SearchFeedSortTypeComboBoxSelectedIndex
        {
            get => searchFeedSortTypeComboBoxSelectedIndex;
            set
            {
                searchFeedSortTypeComboBoxSelectedIndex = value;
                RaisePropertyChangedEvent();
                UpdateProvider();
                _ = Refresh(true);
            }
        }

        public SearchFeedItemSource(string keyword)
        {
            Keyword = keyword;
            string feedType = string.Empty;
            string sortType = string.Empty;
            switch (SearchFeedTypeComboBoxSelectedIndex)
            {
                case 0: feedType = "all"; break;
                case 1: feedType = "feed"; break;
                case 2: feedType = "feedArticle"; break;
                case 3: feedType = "rating"; break;
                case 4: feedType = "picture"; break;
                case 5: feedType = "question"; break;
                case 6: feedType = "answer"; break;
                case 7: feedType = "video"; break;
                case 8: feedType = "ershou"; break;
                case 9: feedType = "vote"; break;
            }
            switch (SearchFeedSortTypeComboBoxSelectedIndex)
            {
                case 0: sortType = "default"; break;
                case 1: sortType = "hot"; break;
                case 2: sortType = "reply"; break;
            }
            Provider = new CoolapkListProvider(
                (p, firstItem, lastItem) =>
                UriHelper.GetUri(
                    UriType.SearchFeeds,
                    feedType,
                    sortType,
                    keyword,
                    p,
                    p > 1 ? $"&firstItem={firstItem}&lastItem={lastItem}" : string.Empty),
                GetEntities,
                "id");
        }

        private IEnumerable<Entity> GetEntities(JObject jo)
        {
            yield return new FeedModel(jo);
        }

        private void UpdateProvider()
        {
            string feedType = string.Empty;
            string sortType = string.Empty;
            switch (SearchFeedTypeComboBoxSelectedIndex)
            {
                case 0: feedType = "all"; break;
                case 1: feedType = "feed"; break;
                case 2: feedType = "feedArticle"; break;
                case 3: feedType = "rating"; break;
                case 4: feedType = "picture"; break;
                case 5: feedType = "question"; break;
                case 6: feedType = "answer"; break;
                case 7: feedType = "video"; break;
                case 8: feedType = "ershou"; break;
                case 9: feedType = "vote"; break;
            }
            switch (SearchFeedSortTypeComboBoxSelectedIndex)
            {
                case 0: sortType = "default"; break;
                case 1: sortType = "hot"; break;
                case 2: sortType = "reply"; break;
            }
            Provider = new CoolapkListProvider(
                (p, firstItem, lastItem) =>
                UriHelper.GetUri(
                    UriType.SearchFeeds,
                    feedType,
                    sortType,
                    Keyword,
                    p,
                    p > 1 ? $"&firstItem={firstItem}&lastItem={lastItem}" : string.Empty),
                GetEntities,
                "uid");
        }
    }

    public class SearchUserItemSource : EntityItemSource
    {
        public string Keyword;

        public SearchUserItemSource(string keyword)
        {
            Keyword = keyword;
            Provider = new CoolapkListProvider(
                (p, firstItem, lastItem) =>
                UriHelper.GetUri(
                    UriType.SearchUsers,
                    keyword,
                    p,
                    p > 1 ? $"&firstItem={firstItem}&lastItem={lastItem}" : string.Empty),
                GetEntities,
                "uid");
        }

        private IEnumerable<Entity> GetEntities(JObject jo)
        {
            yield return new UserModel(jo);
        }
    }

    public class SearchTopicItemSource : EntityItemSource
    {
        public string Keyword;

        public SearchTopicItemSource(string keyword)
        {
            Keyword = keyword;
            Provider = new CoolapkListProvider(
                (p, firstItem, lastItem) =>
                UriHelper.GetUri(
                    UriType.SearchTags,
                    keyword,
                    p,
                    p > 1 ? $"&firstItem={firstItem}&lastItem={lastItem}" : string.Empty),
                GetEntities,
                "id");
        }

        private IEnumerable<Entity> GetEntities(JObject jo)
        {
            yield return new TopicModel(jo);
        }
    }
}
