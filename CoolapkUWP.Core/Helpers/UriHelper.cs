using System;
using System.Diagnostics;

namespace CoolapkUWP.Core.Helpers
{
    /// <summary> 程序支持的能从服务器中获取的数据的类型。 </summary>
    public enum UriType
    {
        CheckLoginInfo,
        CreateFeed,
        CreateFeedReply,
        CreateReplyReply,
        GetAnswers,
        GetDyhDetail,
        GetDyhFeeds,
        GetFeedDetail,
        GetFeedReplies,
        GetHotReplies,
        GetIndexPage,
        GetIndexPageNames,
        GetLikeList,
        GetMyPageCard,
        GetNotifications,
        GetNotificationNumbers,
        GetReplyReplies,
        GetSearchWords,
        GetShareList,
        GetTagDetail,
        GetTagFeeds,
        GetUserFeeds,
        GetUserHistory,
        GetUserList,
        GetUserRecentHistory,
        GetUserSpace,
        GetUserProfile,
        OperateFollow,
        OperateLike,
        OperateUnfollow,
        OperateUnlike,
        RequestValidate,
        SearchFeeds,
        SearchTags,
        SearchUsers,
        SearchWords,
    }

    [DebuggerStepThrough]
    public static class UriHelper
    {
        public static readonly Uri BaseUri = new Uri("https://api.coolapk.com");

        public static Uri GetUri(UriType type, params object[] args)
        {
            var u = string.Format(GetTemplate(type), args);
            return new Uri(BaseUri, u);
        }

        private static string GetTemplate(UriType type)
        {
            switch (type)
            {
                case UriType.CheckLoginInfo: return "/v6/account/checkLoginInfo";
                case UriType.CreateFeed: return "/v6/feed/createFeed";
                case UriType.CreateFeedReply: return "/v6/feed/reply?id={0}&type=feed";
                case UriType.CreateReplyReply: return "/v6/feed/reply?id={0}&type=reply";
                case UriType.GetAnswers: return "/v6/question/answerList?id={0}&sort={1}&page={2}{3}{4}";
                case UriType.GetDyhDetail: return "/v6/dyh/detail?dyhId={0}";
                case UriType.GetDyhFeeds: return "/v6/dyhArticle/list?dyhId={0}&type={1}&page={2}{3}{4}";
                case UriType.GetFeedDetail: return "/v6/feed/detail?id={0}";
                case UriType.GetFeedReplies: return "/v6/feed/replyList?id={0}&listType={1}&page={2}{3}&discussMode=1&feedType=feed&blockStatus=0&fromFeedAuthor={4}";
                case UriType.GetHotReplies: return "/v6/feed/hotReplyList?id={0}&page={1}{2}&discussMode=1";
                case UriType.GetIndexPage: return "/v6{0}{1}page={2}";
                case UriType.GetIndexPageNames: return "/v6/main/init";
                case UriType.GetLikeList: return "/v6/feed/likeList?id={0}&listType=lastupdate_desc&page={1}{2}";
                case UriType.GetMyPageCard: return "/v6/account/loadConfig?key=my_page_card_config";
                case UriType.GetNotifications: return "/v6/notification/{0}?page={1}{2}{3}";
                case UriType.GetNotificationNumbers: return "/v6/notification/checkCount";
                case UriType.GetReplyReplies: return "/v6/feed/replyList?id={0}&listType=&page={1}{2}&discussMode=0&feedType=feed_reply&blockStatus=0&fromFeedAuthor=0";
                case UriType.GetSearchWords: return "/v6/search/suggestSearchWordsNew?searchValue={0}&type=app";
                case UriType.GetShareList: return "/v6/feed/forwardList?id={0}&type=feed&page={1}";
                case UriType.GetTagDetail: return "/v6/topic/newTagDetail?tag={0}";
                case UriType.GetTagFeeds: return "/v6/topic/tagFeedList?tag={0}&page={1}{2}{3}&listType={4}&blockStatus=0";
                case UriType.GetUserFeeds: return "/v6/user/feedList?uid={0}&page={1}{2}{3}";
                case UriType.GetUserHistory: return "/v6/user/hitHistoryList?page={0}{1}{2}";
                case UriType.GetUserList: return "/v6/user/{0}?uid={1}&page={2}{3}{4}";
                case UriType.GetUserRecentHistory: return "/v6/user/recentHistoryList?page={0}{1}{2}";
                case UriType.GetUserSpace: return "/v6/user/space?uid={0}";
                case UriType.GetUserProfile: return "/v6/user/profile?uid={0}";
                case UriType.OperateFollow: return "/v6/user/follow?uid={0}";
                case UriType.OperateLike: return "/v6/feed/like{0}?id={1}&detail=0";
                case UriType.OperateUnfollow: return "/v6/user/follow?uid={0}";
                case UriType.OperateUnlike: return "/v6/feed/unlike{0}?id={1}&detail=0";
                case UriType.RequestValidate: return "/v6/account/requestValidate";
                case UriType.SearchFeeds: return "/v6/search?type=feed&feedType={0}&sort={1}&searchValue={2}&page={3}{4}&showAnonymous=-1";
                case UriType.SearchTags: return "/v6/search?type=feedTopic&searchValue={0}&page={1}{2}&showAnonymous=-1";
                case UriType.SearchUsers: return "/v6/search?type=user&searchValue={0}&page={1}{2}&showAnonymous=-1";
                case UriType.SearchWords: return "/v6/search/suggestSearchWordsNew?searchValue={0}&type=app";
                default: throw new ArgumentException($"{typeof(UriType).FullName}值错误");
            }
        }
    }
}