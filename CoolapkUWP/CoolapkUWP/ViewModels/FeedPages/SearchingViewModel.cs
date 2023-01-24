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

        private SeachFeedItemSourse seachFeedItemSourse;
        public SeachFeedItemSourse SeachFeedItemSourse
        {
            get => seachFeedItemSourse;
            private set
            {
                seachFeedItemSourse = value;
                RaisePropertyChangedEvent();
            }
        }

        private SeachUserItemSourse seachUserItemSourse;
        public SeachUserItemSourse SeachUserItemSourse
        {
            get => seachUserItemSourse;
            private set
            {
                seachUserItemSourse = value;
                RaisePropertyChangedEvent();
            }
        }

        private SeachTopicItemSourse seachTopicItemSourse;
        public SeachTopicItemSourse SeachTopicItemSourse
        {
            get => seachTopicItemSourse;
            private set
            {
                seachTopicItemSourse = value;
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
                if (SeachFeedItemSourse == null)
                {
                    SeachFeedItemSourse = new SeachFeedItemSourse(Title);
                    SeachFeedItemSourse.OnLoadMoreStarted += UIHelper.ShowProgressBar;
                    SeachFeedItemSourse.OnLoadMoreCompleted += UIHelper.HideProgressBar;
                }
                else if (SeachFeedItemSourse.Keyword != Title)
                {
                    SeachFeedItemSourse.Keyword = Title;
                }
                if (SeachUserItemSourse == null)
                {
                    SeachUserItemSourse = new SeachUserItemSourse(Title);
                    SeachUserItemSourse.OnLoadMoreStarted += UIHelper.ShowProgressBar;
                    SeachUserItemSourse.OnLoadMoreCompleted += UIHelper.HideProgressBar;
                }
                else if (SeachUserItemSourse.Keyword != Title)
                {
                    SeachUserItemSourse.Keyword = Title;
                }
                if (SeachTopicItemSourse == null)
                {
                    SeachTopicItemSourse = new SeachTopicItemSourse(Title);
                    SeachTopicItemSourse.OnLoadMoreStarted += UIHelper.ShowProgressBar;
                    SeachTopicItemSourse.OnLoadMoreCompleted += UIHelper.HideProgressBar;
                }
                else if (SeachTopicItemSourse.Keyword != Title)
                {
                    SeachTopicItemSourse.Keyword = Title;
                }
            }
            await SeachFeedItemSourse?.Refresh(reset);
            await SeachUserItemSourse?.Refresh(reset);
            await SeachTopicItemSourse?.Refresh(reset);
        }
    }

    public class SeachFeedItemSourse : EntityItemSourse, INotifyPropertyChanged
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

        protected override event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        public SeachFeedItemSourse(string keyword)
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

    public class SeachUserItemSourse : EntityItemSourse
    {
        public string Keyword;

        public SeachUserItemSourse(string keyword)
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

    public class SeachTopicItemSourse : EntityItemSourse
    {
        public string Keyword;

        public SeachTopicItemSourse(string keyword)
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
