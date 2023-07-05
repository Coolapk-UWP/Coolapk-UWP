using CoolapkUWP.Controls;
using CoolapkUWP.Helpers;
using CoolapkUWP.Models;
using CoolapkUWP.Models.Feeds;
using CoolapkUWP.Models.Users;
using CoolapkUWP.ViewModels.DataSource;
using CoolapkUWP.ViewModels.Providers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;

namespace CoolapkUWP.ViewModels.FeedPages
{
    public abstract class FeedShellViewModel : IViewModel, INotifyPropertyChanged
    {
        protected string ID { get; set; }

        private string title = string.Empty;
        public string Title
        {
            get => title;
            protected set
            {
                if (title != value)
                {
                    title = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private FeedDetailModel feedDetail;
        public FeedDetailModel FeedDetail
        {
            get => feedDetail;
            protected set
            {
                if (feedDetail != value)
                {
                    feedDetail = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private List<ShyHeaderItem> itemSource;
        public List<ShyHeaderItem> ItemSource
        {
            get => itemSource;
            protected set
            {
                if (itemSource != value)
                {
                    itemSource = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        protected FeedShellViewModel(string id)
        {
            if (string.IsNullOrEmpty(id)) { throw new ArgumentException(nameof(id)); }
            ID = id;
        }

        protected virtual async Task<FeedDetailModel> GetFeedDetailAsync(string id)
        {
            (bool isSucceed, JToken result) = id.Contains("changeHistoryDetail") ? await RequestHelper.GetDataAsync(new Uri(UriHelper.BaseUri.ToString() + "v6/feed/" + id), true) : await RequestHelper.GetDataAsync(UriHelper.GetUri(UriType.GetFeedDetail, id), true);
            if (!isSucceed) { return null; }

            JObject detail = (JObject)result;
            return detail != null ? new FeedDetailModel(detail) : null;
        }

        public abstract Task Refresh(bool reset = false);

        bool IViewModel.IsEqual(IViewModel other) => other is FeedShellViewModel model && IsEqual(model);

        public bool IsEqual(FeedShellViewModel other) => ID == other.ID;
    }

    public class FeedDetailViewModel : FeedShellViewModel
    {
        public ReplyItemSource ReplyItemSource { get; private set; }
        public LikeItemSource LikeItemSource { get; private set; }
        public ShareItemSource ShareItemSource { get; private set; }

        internal FeedDetailViewModel(string id) : base(id) { }

        public override async Task Refresh(bool reset = false)
        {
            if (FeedDetail == null || reset)
            {
                FeedDetail = await GetFeedDetailAsync(ID);
                List<ShyHeaderItem> ItemSource = new List<ShyHeaderItem>();
                Title = FeedDetail.Title;
                if (ReplyItemSource == null || ReplyItemSource.ID != ID)
                {
                    ReplyItemSource = new ReplyItemSource(ID);
                    ReplyItemSource.LoadMoreStarted += UIHelper.ShowProgressBar;
                    ReplyItemSource.LoadMoreCompleted += UIHelper.HideProgressBar;
                }
                ItemSource.Add(new ShyHeaderItem
                {
                    Header = "回复",
                    ItemSource = ReplyItemSource
                });
                if (LikeItemSource == null || LikeItemSource.ID != ID)
                {
                    LikeItemSource = new LikeItemSource(ID);
                    LikeItemSource.LoadMoreStarted += UIHelper.ShowProgressBar;
                    LikeItemSource.LoadMoreCompleted += UIHelper.HideProgressBar;
                }
                ItemSource.Add(new ShyHeaderItem
                {
                    Header = "点赞",
                    ItemSource = LikeItemSource
                });
                if (ShareItemSource == null || ShareItemSource.ID != ID)
                {
                    ShareItemSource = new ShareItemSource(ID, FeedDetail.FeedType);
                    ShareItemSource.LoadMoreStarted += UIHelper.ShowProgressBar;
                    ShareItemSource.LoadMoreCompleted += UIHelper.HideProgressBar;
                }
                ItemSource.Add(new ShyHeaderItem
                {
                    Header = "转发",
                    ItemSource = ShareItemSource
                });
                base.ItemSource = ItemSource;
            }
            await ReplyItemSource?.Refresh(reset);
        }
    }

    public class QuestionViewModel : FeedShellViewModel
    {
        public QuestionItemSource ReplyItemSource { get; private set; }
        public QuestionItemSource LikeItemSource { get; private set; }
        public QuestionItemSource DatelineItemSource { get; private set; }

        internal QuestionViewModel(string id) : base(id) { }

        public override async Task Refresh(bool reset = false)
        {
            if (FeedDetail == null || reset)
            {
                FeedDetail = await GetFeedDetailAsync(ID);
                List<ShyHeaderItem> ItemSource = new List<ShyHeaderItem>();
                Title = FeedDetail.Title;
                if (ReplyItemSource == null || ReplyItemSource.ID != ID)
                {
                    ReplyItemSource = new QuestionItemSource(ID, "reply");
                    ReplyItemSource.LoadMoreStarted += UIHelper.ShowProgressBar;
                    ReplyItemSource.LoadMoreCompleted += UIHelper.HideProgressBar;
                }
                ItemSource.Add(new ShyHeaderItem
                {
                    Header = "热度排序",
                    ItemSource = ReplyItemSource
                });
                if (LikeItemSource == null || LikeItemSource.ID != ID)
                {
                    LikeItemSource = new QuestionItemSource(ID, "like");
                    LikeItemSource.LoadMoreStarted += UIHelper.ShowProgressBar;
                    LikeItemSource.LoadMoreCompleted += UIHelper.HideProgressBar;
                }
                ItemSource.Add(new ShyHeaderItem
                {
                    Header = "点赞排序",
                    ItemSource = LikeItemSource
                });
                if (DatelineItemSource == null || DatelineItemSource.ID != ID)
                {
                    DatelineItemSource = new QuestionItemSource(ID, "dateline");
                    DatelineItemSource.LoadMoreStarted += UIHelper.ShowProgressBar;
                    DatelineItemSource.LoadMoreCompleted += UIHelper.HideProgressBar;
                }
                ItemSource.Add(new ShyHeaderItem
                {
                    Header = "时间排序",
                    ItemSource = DatelineItemSource
                });
                base.ItemSource = ItemSource;
            }
            await ReplyItemSource?.Refresh(reset);
        }
    }

    public class VoteViewModel : FeedShellViewModel
    {
        internal VoteViewModel(string id) : base(id) { }

        public override async Task Refresh(bool reset = false)
        {
            if (FeedDetail == null || reset)
            {
                FeedDetail = await GetFeedDetailAsync(ID);
                if (FeedDetail == null) { return; }
                List<ShyHeaderItem> ItemSource = new List<ShyHeaderItem>();
                Title = FeedDetail.Title;
                if (FeedDetail.VoteType == 0)
                {
                    foreach (VoteItem vote in FeedDetail.VoteRows)
                    {
                        VoteItemSource VoteItemSource = new VoteItemSource(vote.ID.ToString(), vote.VoteID.ToString());
                        VoteItemSource.LoadMoreStarted += UIHelper.ShowProgressBar;
                        VoteItemSource.LoadMoreCompleted += UIHelper.HideProgressBar;
                        ItemSource.Add(new ShyHeaderItem
                        {
                            Header = vote.Title,
                            ItemSource = VoteItemSource
                        });
                    }
                }
                else
                {
                    VoteItemSource VoteItemSource = new VoteItemSource(string.Empty, FeedDetail.ID.ToString());
                    VoteItemSource.LoadMoreStarted += UIHelper.ShowProgressBar;
                    VoteItemSource.LoadMoreCompleted += UIHelper.HideProgressBar;
                    ItemSource.Add(new ShyHeaderItem
                    {
                        Header = "观点",
                        ItemSource = VoteItemSource
                    });
                    if (!string.IsNullOrEmpty(FeedDetail.VoteTag))
                    {
                        TagItemSource TagItemSource = new TagItemSource(FeedDetail.VoteTag);
                        TagItemSource.LoadMoreStarted += UIHelper.ShowProgressBar;
                        TagItemSource.LoadMoreCompleted += UIHelper.HideProgressBar;
                        ItemSource.Add(new ShyHeaderItem
                        {
                            Header = "话题",
                            ItemSource = TagItemSource
                        });
                    }
                }
                base.ItemSource = ItemSource;
            }
            await (ItemSource.FirstOrDefault()?.ItemSource as EntityItemSource)?.Refresh(reset);
        }
    }

    public class ReplyItemSource : EntityItemSource, INotifyPropertyChanged, ICanComboBoxChangeSelectedIndex, ICanToggleChangeSelectedIndex
    {
        public string ID;
        public List<string> ItemSource { get; private set; }
        private readonly ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("FeedShellPage");

        private bool toggleIsOn;
        public bool ToggleIsOn
        {
            get => toggleIsOn;
            set
            {
                if (toggleIsOn != value)
                {
                    toggleIsOn = value;
                    SetProvider();
                    RaisePropertyChangedEvent();
                }
            }
        }

        private string replyListType = "lastupdate_desc";
        public string ReplyListType
        {
            get => replyListType;
            set
            {
                if (replyListType != value)
                {
                    replyListType = value;
                    SetProvider();
                    RaisePropertyChangedEvent();
                }
            }
        }

        public int comboBoxSelectedIndex;
        public int ComboBoxSelectedIndex
        {
            get => comboBoxSelectedIndex;
            set
            {
                if (comboBoxSelectedIndex != value)
                {
                    comboBoxSelectedIndex = value;
                    SetComboBoxSelectedIndex(value);
                    RaisePropertyChangedEvent();
                }
            }
        }

        public ReplyItemSource(string id)
        {
            ID = id;
            ItemSource = new List<string>()
            {
                loader.GetString("lastupdate_desc"),
                loader.GetString("dateline_desc"),
                loader.GetString("popular")
            };
            SetProvider();
        }

        private async void SetProvider()
        {
            Provider = new CoolapkListProvider(
                (p, firstItem, lastItem) =>
                UriHelper.GetUri(
                    UriType.GetFeedReplies,
                    ID,
                    ReplyListType,
                    p,
                    p > 1 ? $"&firstItem={firstItem}&lastItem={lastItem}" : string.Empty,
                    toggleIsOn ? 1 : 0),
                GetEntities,
                "id");
            await Refresh(true);
        }

        private IEnumerable<Entity> GetEntities(JObject json)
        {
            yield return json.Value<string>("entityType") == "feed_reply" ? new FeedReplyModel(json) : (Entity)new NullEntity();
        }

        public void SetComboBoxSelectedIndex(int value)
        {
            switch (value)
            {
                case -1: return;
                case 0:
                    ReplyListType = "lastupdate_desc";
                    break;

                case 1:
                    ReplyListType = "dateline_desc";
                    break;

                case 2:
                    ReplyListType = "popular";
                    break;
            }
        }
    }

    public class LikeItemSource : EntityItemSource
    {
        public string ID;

        public LikeItemSource(string id)
        {
            ID = id;
            Provider = new CoolapkListProvider(
                (p, firstItem, lastItem) =>
                UriHelper.GetUri(
                    UriType.GetLikeList,
                    id,
                    p,
                    p > 1 ? $"&firstItem={firstItem}&lastItem={lastItem}" : string.Empty),
                GetEntities,
                "uid");
        }

        private IEnumerable<Entity> GetEntities(JObject json)
        {
            yield return new UserModel(json);
        }
    }

    public class ShareItemSource : EntityItemSource
    {
        public string ID;

        public ShareItemSource(string id, string feedtype = "feed")
        {
            ID = id;
            Provider = new CoolapkListProvider(
                (p, _, __) =>
                UriHelper.GetUri(
                    UriType.GetShareList,
                    id,
                    feedtype,
                    p),
                GetEntities,
                "id");
        }

        private IEnumerable<Entity> GetEntities(JObject json)
        {
            yield return new FeedModel(json);
        }
    }

    public class QuestionItemSource : EntityItemSource
    {
        public string ID;

        public QuestionItemSource(string id, string answerSortType = "reply")
        {
            ID = id;
            Provider = new CoolapkListProvider(
                (p, firstItem, lastItem) =>
                UriHelper.GetUri(
                    UriType.GetAnswers,
                    id,
                    answerSortType,
                    p,
                    string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}",
                    string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}"),
                GetEntities,
                "id");
        }

        private IEnumerable<Entity> GetEntities(JObject json)
        {
            yield return new FeedModel(json);
        }
    }

    public class VoteItemSource : EntityItemSource
    {
        public string ID;

        public VoteItemSource(string id, string fid)
        {
            ID = id;
            Provider = new CoolapkListProvider(
                (p, firstItem, lastItem) =>
                UriHelper.GetUri(
                    UriType.GetVoteComments,
                    fid,
                    string.IsNullOrEmpty(id) ? string.Empty : $"&extra_key={id}",
                    p,
                    string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}",
                    string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}"),
                GetEntities,
                "id");
        }

        private IEnumerable<Entity> GetEntities(JObject json)
        {
            yield return new FeedModel(json);
        }
    }

    public class TagItemSource : EntityItemSource
    {
        public string ID;

        public TagItemSource(string id)
        {
            ID = id;
            Provider = new CoolapkListProvider(
                (p, firstItem, lastItem) =>
                UriHelper.GetUri(
                    UriType.GetTagFeeds,
                    id,
                    p,
                    string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}",
                    string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}",
                    "lastupdate_desc"),
                GetEntities,
                "id");
        }

        private IEnumerable<Entity> GetEntities(JObject json)
        {
            yield return new FeedModel(json);
        }
    }
}
