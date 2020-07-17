using System;
using System.Diagnostics;

namespace CoolapkUWP.Helpers.Providers
{
    /// <summary> 程序支持的能从服务器中获取的数据的类型。 </summary>
    internal enum UriType
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

    internal static class UriProvider
    {
        [DebuggerStepThrough]
        public static Uri GetUri(UriType type, params object[] args)
        {
            var s = "https://api.coolapk.com/v6" + GetTemplate(type);
            var u = string.Format(s, args);
            return new Uri(u, UriKind.Absolute);
        }

        [DebuggerStepThrough]
        private static string GetTemplate(UriType type)
        {
            switch (type)
            {
                case UriType.CheckLoginInfo: return "/account/checkLoginInfo";
                case UriType.CreateFeed: return "/feed/createFeed";
                case UriType.CreateFeedReply: return "/feed/reply?id={0}&type=feed";
                case UriType.CreateReplyReply: return "/feed/reply?id={0}&type=reply";
                case UriType.GetAnswers: return "/question/answerList?id={0}&sort={1}&page={2}{3}{4}";
                case UriType.GetDyhDetail: return "/dyh/detail?dyhId={0}";
                case UriType.GetDyhFeeds: return "/dyhArticle/list?dyhId={0}&type={1}&page={2}{3}{4}";
                case UriType.GetFeedDetail: return "/feed/detail?id={0}";
                case UriType.GetFeedReplies: return "/feed/replyList?id={0}&listType={1}&page={2}{3}&discussMode=1&feedType=feed&blockStatus=0&fromFeedAuthor={4}";
                case UriType.GetHotReplies: return "/feed/hotReplyList?id={0}&page={1}{2}&discussMode=1";
                case UriType.GetIndexPage: return "{0}{1}page={2}";
                case UriType.GetIndexPageNames: return "/main/init";
                case UriType.GetLikeList: return "/feed/likeList?id={0}&listType=lastupdate_desc&page={1}{2}";
                case UriType.GetMyPageCard: return "/account/loadConfig?key=my_page_card_config";
                case UriType.GetNotifications: return "/notification/{0}?page={1}{2}{3}";
                case UriType.GetNotificationNumbers: return "/notification/checkCount";
                case UriType.GetReplyReplies: return "/feed/replyList?id={0}&listType=&page={1}{2}&discussMode=0&feedType=feed_reply&blockStatus=0&fromFeedAuthor=0";
                case UriType.GetSearchWords: return "/search/suggestSearchWordsNew?searchValue={0}&type=app";
                case UriType.GetShareList: return "/feed/forwardList?id={0}&type=feed&page={1}";
                case UriType.GetTagDetail: return "/topic/newTagDetail?tag={0}";
                case UriType.GetTagFeeds: return "/topic/tagFeedList?tag={0}&page={1}{2}{3}&listType={4}&blockStatus=0";
                case UriType.GetUserFeeds: return "/user/feedList?uid={0}&page={1}{2}{3}";
                case UriType.GetUserHistory: return "/user/hitHistoryList?page={0}{1}{2}";
                case UriType.GetUserList: return "/user/{0}?uid={1}&page={2}{3}{4}";
                case UriType.GetUserRecentHistory: return "/user/recentHistoryList?page={0}{1}{2}";
                case UriType.GetUserSpace: return "/user/space?uid={0}";
                case UriType.GetUserProfile: return "/user/profile?uid={0}";
                case UriType.OperateFollow: return "/user/follow?uid={0}";
                case UriType.OperateLike: return "/feed/like{0}?id={1}&detail=0";
                case UriType.OperateUnfollow: return "/user/follow?uid={0}";
                case UriType.OperateUnlike: return "/feed/unlike{0}?id={1}&detail=0";
                case UriType.RequestValidate: return "/account/requestValidate";
                case UriType.SearchFeeds: return "/search?type=feed&feedType={0}&sort={1}&searchValue={2}&page={3}{4}&showAnonymous=-1";
                case UriType.SearchTags: return "/search?type=feedTopic&searchValue={0}&page={1}{2}&showAnonymous=-1";
                case UriType.SearchUsers: return "/search?type=user&searchValue={0}&page={1}{2}&showAnonymous=-1";
                case UriType.SearchWords: return "/search/suggestSearchWordsNew?searchValue={0}&type=app";
                default: throw new ArgumentException($"{typeof(UriType).FullName}值错误");
            }
        }
    }
}