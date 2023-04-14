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

        private SearchFeedItemSourse searchFeedItemSourse;
        public SearchFeedItemSourse SearchFeedItemSourse
        {
            get => searchFeedItemSourse;
            private set
            {
                searchFeedItemSourse = value;
                RaisePropertyChangedEvent();
            }
        }

        private SearchUserItemSourse searchUserItemSourse;
        public SearchUserItemSourse SearchUserItemSourse
        {
            get => searchUserItemSourse;
            private set
            {
                searchUserItemSourse = value;
                RaisePropertyChangedEvent();
            }
        }

        private SearchTopicItemSourse searchTopicItemSourse;
        public SearchTopicItemSourse SearchTopicItemSourse
        {
            get => searchTopicItemSourse;
            private set
            {
                searchTopicItemSourse = value;
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
                if (SearchFeedItemSourse == null)
                {
                    SearchFeedItemSourse = new SearchFeedItemSourse(Title);
                    SearchFeedItemSourse.LoadMoreStarted += UIHelper.ShowProgressBar;
                    SearchFeedItemSourse.LoadMoreCompleted += UIHelper.HideProgressBar;
                }
                else if (SearchFeedItemSourse.Keyword != Title)
                {
                    SearchFeedItemSourse.Keyword = Title;
                }
                if (SearchUserItemSourse == null)
                {
                    SearchUserItemSourse = new SearchUserItemSourse(Title);
                    SearchUserItemSourse.LoadMoreStarted += UIHelper.ShowProgressBar;
                    SearchUserItemSourse.LoadMoreCompleted += UIHelper.HideProgressBar;
                }
                else if (SearchUserItemSourse.Keyword != Title)
                {
                    SearchUserItemSourse.Keyword = Title;
                }
                if (SearchTopicItemSourse == null)
                {
                    SearchTopicItemSourse = new SearchTopicItemSourse(Title);
                    SearchTopicItemSourse.LoadMoreStarted += UIHelper.ShowProgressBar;
                    SearchTopicItemSourse.LoadMoreCompleted += UIHelper.HideProgressBar;
                }
                else if (SearchTopicItemSourse.Keyword != Title)
                {
                    SearchTopicItemSourse.Keyword = Title;
                }
            }
            await SearchFeedItemSourse?.Refresh(reset);
            await SearchUserItemSourse?.Refresh(reset);
            await SearchTopicItemSourse?.Refresh(reset);
        }

        bool IViewModel.IsEqual(IViewModel other) => other is SearchingViewModel model && IsEqual(model);

        public bool IsEqual(SearchingViewModel other) => Title == other.Title;
    }

    public class SearchFeedItemSourse : EntityItemSourse, INotifyPropertyChanged
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

        public SearchFeedItemSourse(string keyword)
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

    public class SearchUserItemSourse : EntityItemSourse
    {
        public string Keyword;

        public SearchUserItemSourse(string keyword)
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

    public class SearchTopicItemSourse : EntityItemSourse
    {
        public string Keyword;

        public SearchTopicItemSourse(string keyword)
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
