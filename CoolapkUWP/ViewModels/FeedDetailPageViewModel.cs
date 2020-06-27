using CoolapkUWP.Helpers;
using CoolapkUWP.Helpers.Providers;
using CoolapkUWP.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;

namespace CoolapkUWP.ViewModels.FeedDetailPage
{
    internal abstract class ViewModel : IViewModel, INotifyPropertyChanged
    {
        protected string id;
        private FeedDetailModel feedDetail;

        public double[] VerticalOffsets { get; set; } = new double[3];
        public string Title { get; protected set; } = string.Empty;

        public FeedDetailModel FeedDetail
        {
            get => feedDetail;
            protected set
            {
                feedDetail = value;
                RaisePropertyChangedEvent();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null)
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        internal static async Task<ViewModel> GetViewModelAsync(string id)
        {
            if (string.IsNullOrEmpty(id)) { throw new ArgumentException(nameof(id)); }
            var detail = await GetFeedDetailAsync(id);
            if (detail != null)
            {
                if (detail.IsQuestionFeed)
                {
                    return new QuestionViewModel(id);
                }
                else
                {
                    return new FeedViewModel(id);
                }
            }
            else { return null; }
        }

        protected ViewModel(string id)
        {
            if (string.IsNullOrEmpty(id)) { throw new ArgumentException(nameof(id)); }
            this.id = id;
        }

        protected static async Task<FeedDetailModel> GetFeedDetailAsync(string id)
        {
            var detail = (JObject)await DataHelper.GetDataAsync(DataUriType.GetFeedDetail, id);
            if (detail != null)
            {
                return new FeedDetailModel(detail);
            }
            return null;
        }

        public abstract Task Refresh(int p = -1);
    }

    internal class FeedViewModel : ViewModel
    {
        public FeedDetailList.ViewModel ReplyListVM { get; private set; }

        internal FeedViewModel(string id) : base(id)
        {
        }

        public override async Task Refresh(int p = -1)
        {
            if (FeedDetail == null || p == 1)
            {
                var feedDetail = await GetFeedDetailAsync(id);
                Title = feedDetail.Title;
                if (ReplyListVM == null || ReplyListVM.Id != id)
                {
                    ReplyListVM = new FeedDetailList.ViewModel(id, feedDetail);
                }
                FeedDetail = feedDetail;
            }
            await ReplyListVM?.Refresh(p);
        }
    }

    internal class QuestionViewModel : ViewModel, ICanComboBoxChangeSelectedIndex
    {
        private readonly CoolapkListProvider provider;
        private string answerSortType = "reply";

        public int ComboBoxSelectedIndex { get; private set; }

        public ObservableCollection<Entity> Models { get => provider?.Models ?? null; }

        internal QuestionViewModel(string id) : base(id)
        {
            provider =
                new CoolapkListProvider(
                    async (p, page, firstItem, lastItem) =>
                        (JArray)await DataHelper.GetDataAsync(
                            DataUriType.GetAnswers,
                            id,
                            answerSortType,
                            p == -1 ? ++page : p,
                            string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}",
                            string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}"),
                    (a, b) => ((FeedModel)a).Url == b.Value<string>("url"),
                    (o) => new Entity[] { new FeedModel(o, FeedDisplayMode.notShowMessageTitle) },
                    "id");
        }

        public override async Task Refresh(int p = -1)
        {
            if (FeedDetail == null || p == 1)
            {
                FeedDetail = await GetFeedDetailAsync(id);
                Title = FeedDetail.Title;
            }
            await provider?.Refresh(p);
        }

        public async Task SetComboBoxSelectedIndex(int value)
        {
            switch (value)
            {
                case -1: return;
                case 0:
                    answerSortType = "reply";
                    break;

                case 1:
                    answerSortType = "like";
                    break;

                case 2:
                    answerSortType = "dateline";
                    break;
            }
            ComboBoxSelectedIndex = value;
            if (provider != null)
            {
                provider.Reset();
                await provider.Refresh(1);
            }
        }
    }
}