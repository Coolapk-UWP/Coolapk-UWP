using CoolapkUWP.Core.Helpers;
using CoolapkUWP.Core.Models;
using CoolapkUWP.Core.Providers;
using CoolapkUWP.Helpers;
using CoolapkUWP.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoolapkUWP.ViewModels.FeedDetailList
{
    internal class ViewModel : IViewModel, ICanComboBoxChangeSelectedIndex
    {
        private string replyListType = "lastupdate_desc";
        private string isFromAuthor = "0";

        public double[] VerticalOffsets { get; set; }
        public string Title { get; } = string.Empty;
        public int ComboBoxSelectedIndex { get; private set; }
        public int SelectedIndex { get; set; }

        internal string Id { get; }
        internal CoolapkListProvider ReplyProvider { get; }
        internal CoolapkListProvider LikeProvider { get; }
        internal CoolapkListProvider ShareProvider { get; }
        internal List<FeedReplyModel> HotReplys { get; }

        internal ViewModel(string id, FeedDetailModel model)
        {
            if (string.IsNullOrEmpty(id)) { throw new ArgumentException(nameof(id)); }
            Id = id;

            ReplyProvider =
                new CoolapkListProvider(
                    (p, page, firstItem, lastItem) =>
                        UriHelper.GetUri(
                            UriType.GetFeedReplies,
                            id,
                            replyListType,
                            p < 0 ? ++page : p,
                            page > 1 ? $"&firstItem={firstItem}&lastItem={lastItem}" : string.Empty,
                            isFromAuthor),
                    (a, b) => ((FeedReplyModel)a).Id == b.Value<int>("id"),
                    (o) => new Entity[] { new FeedReplyModel(o) },
                    MessageType.NoMoreReply,
                    "id");

            LikeProvider =
                new CoolapkListProvider(
                    (p, page, firstItem, lastItem) =>
                        UriHelper.GetUri(
                            UriType.GetLikeList,
                            id,
                            p < 0 ? ++page : p,
                            page > 1 ? $"&firstItem={firstItem}&lastItem={lastItem}" : string.Empty),
                    (a, b) => ((UserModel)a).Url == b.Value<string>("url"),
                    (o) => new Entity[] { new UserModel(o) },
                    MessageType.NoMoreLikeUser,
                    "uid");
            ShareProvider =
                new CoolapkListProvider(
                    (p, page, firstItem, lastItem) =>
                        UriHelper.GetUri(
                            UriType.GetShareList,
                            p == -2 ? true : false,
                            id,
                            p < 0 ? ++page : p),
                    (a, b) => ((SourceFeedModel)a).Url == b.Value<string>("url"),
                    (o) => new Entity[] { new SourceFeedModel(o) },
                    MessageType.NoMoreShare,
                    "id");

            HotReplys = model.HotReplies;
        }

        public async Task Refresh(int p = -1)
        {
            switch (SelectedIndex)
            {
                case 0:
                    await ReplyProvider.Refresh(p);
                    break;

                case 1:
                    await LikeProvider.Refresh(p);
                    break;

                case 2:
                    await ShareProvider.Refresh(p);
                    break;
            }
        }

        public async Task SetSelectedIndex(int value)
        {
            switch (value)
            {
                case -1: return;
                case 1:
                    if (LikeProvider.Models.Count == 0)
                    {
                        await LikeProvider.Refresh();
                    }

                    break;

                case 2:
                    if (ShareProvider.Models.Count == 0)
                    {
                        await ShareProvider.Refresh();
                    }

                    break;
            }
            SelectedIndex = value;
        }

        public async Task SetComboBoxSelectedIndex(int value)
        {
            switch (value)
            {
                case -1: return;
                case 0:
                    replyListType = "lastupdate_desc";
                    isFromAuthor = "0";
                    break;

                case 1:
                    replyListType = "dateline_desc";
                    isFromAuthor = "0";
                    break;

                case 2:
                    replyListType = "popular";
                    isFromAuthor = "0";
                    break;

                case 3:
                    replyListType = string.Empty;
                    isFromAuthor = "1";
                    break;
            }
            ComboBoxSelectedIndex = value;
            if (ReplyProvider != null)
            {
                ReplyProvider.Reset();
                await ReplyProvider.Refresh(1);
            }
        }
    }
}