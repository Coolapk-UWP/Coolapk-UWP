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
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;

namespace CoolapkUWP.ViewModels.FeedPages
{
    public abstract class FeedShellViewModel : IViewModel, INotifyPropertyChanged
    {
        public string ID { get; protected set; }
        public double[] VerticalOffsets { get; set; } = new double[3];

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
    }

    public class FeedDetailViewModel : FeedShellViewModel
    {
        public ReplyItemSourse ReplyItemSourse { get; private set; }
        public LikeItemSourse LikeItemSourse { get; private set; }
        public ShareItemSourse ShareItemSourse { get; private set; }

        internal FeedDetailViewModel(string id) : base(id) { }

        public override async Task Refresh(bool reset = false)
        {
            if (FeedDetail == null || reset)
            {
                FeedDetail = await GetFeedDetailAsync(ID);
                List<ShyHeaderItem> ItemSource = new List<ShyHeaderItem>();
                Title = FeedDetail.Title;
                if (ReplyItemSourse == null || ReplyItemSourse.ID != ID)
                {
                    ReplyItemSourse = new ReplyItemSourse(ID);
                    ReplyItemSourse.LoadMoreStarted += UIHelper.ShowProgressBar;
                    ReplyItemSourse.LoadMoreCompleted += UIHelper.HideProgressBar;
                }
                ItemSource.Add(new ShyHeaderItem
                {
                    Header = "回复",
                    ItemSource = ReplyItemSourse
                });
                if (LikeItemSourse == null || LikeItemSourse.ID != ID)
                {
                    LikeItemSourse = new LikeItemSourse(ID);
                    LikeItemSourse.LoadMoreStarted += UIHelper.ShowProgressBar;
                    LikeItemSourse.LoadMoreCompleted += UIHelper.HideProgressBar;
                }
                ItemSource.Add(new ShyHeaderItem
                {
                    Header = "点赞",
                    ItemSource = LikeItemSourse
                });
                if (ShareItemSourse == null || ShareItemSourse.ID != ID)
                {
                    ShareItemSourse = new ShareItemSourse(ID, FeedDetail.FeedType);
                    ShareItemSourse.LoadMoreStarted += UIHelper.ShowProgressBar;
                    ShareItemSourse.LoadMoreCompleted += UIHelper.HideProgressBar;
                }
                ItemSource.Add(new ShyHeaderItem
                {
                    Header = "转发",
                    ItemSource = ShareItemSourse
                });
                base.ItemSource = ItemSource;
            }
            await ReplyItemSourse?.Refresh(reset);
        }
    }

    public class QuestionViewModel : FeedShellViewModel
    {
        public QuestionItemSourse ReplyItemSourse { get; private set; }
        public QuestionItemSourse LikeItemSourse { get; private set; }
        public QuestionItemSourse DatelineItemSourse { get; private set; }

        internal QuestionViewModel(string id) : base(id) { }

        public override async Task Refresh(bool reset = false)
        {
            if (FeedDetail == null || reset)
            {
                FeedDetail = await GetFeedDetailAsync(ID);
                List<ShyHeaderItem> ItemSource = new List<ShyHeaderItem>();
                Title = FeedDetail.Title;
                if (ReplyItemSourse == null || ReplyItemSourse.ID != ID)
                {
                    ReplyItemSourse = new QuestionItemSourse(ID, "reply");
                    ReplyItemSourse.LoadMoreStarted += UIHelper.ShowProgressBar;
                    ReplyItemSourse.LoadMoreCompleted += UIHelper.HideProgressBar;
                }
                ItemSource.Add(new ShyHeaderItem
                {
                    Header = "热度排序",
                    ItemSource = ReplyItemSourse
                });
                if (LikeItemSourse == null || LikeItemSourse.ID != ID)
                {
                    LikeItemSourse = new QuestionItemSourse(ID, "like");
                    LikeItemSourse.LoadMoreStarted += UIHelper.ShowProgressBar;
                    LikeItemSourse.LoadMoreCompleted += UIHelper.HideProgressBar;
                }
                ItemSource.Add(new ShyHeaderItem
                {
                    Header = "点赞排序",
                    ItemSource = LikeItemSourse
                });
                if (DatelineItemSourse == null || DatelineItemSourse.ID != ID)
                {
                    DatelineItemSourse = new QuestionItemSourse(ID, "dateline");
                    DatelineItemSourse.LoadMoreStarted += UIHelper.ShowProgressBar;
                    DatelineItemSourse.LoadMoreCompleted += UIHelper.HideProgressBar;
                }
                ItemSource.Add(new ShyHeaderItem
                {
                    Header = "时间排序",
                    ItemSource = DatelineItemSourse
                });
                base.ItemSource = ItemSource;
            }
            await ReplyItemSourse?.Refresh(reset);
        }
    }

    public class ReplyItemSourse : EntityItemSourse, INotifyPropertyChanged, ICanComboBoxChangeSelectedIndex, ICanToggleChangeSelectedIndex
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

        public ReplyItemSourse(string id)
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

    public class LikeItemSourse : EntityItemSourse
    {
        public string ID;

        public LikeItemSourse(string id)
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

    public class ShareItemSourse : EntityItemSourse
    {
        public string ID;

        public ShareItemSourse(string id, string feedtype = "feed")
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

    public class QuestionItemSourse : EntityItemSourse
    {
        public string ID;

        public QuestionItemSourse(string id, string answerSortType = "reply")
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
}
