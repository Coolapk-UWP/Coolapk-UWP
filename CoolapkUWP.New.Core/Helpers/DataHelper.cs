using System;
using System.Security.Cryptography;
using System.Text;

namespace CoolapkUWP.New.Core.Helpers
{
    public enum DataUriType
    {
        CheckLoginInfo,
        GetAnswers,
        GetDyhDetail,
        GetDyhFeeds,
        GetFeedDetail,
        GetFeedReplies,
        GetHotReplies,
        GetLikeList,
        GetIndexPage,
        GetIndexPageNames,
        GetNotifications,
        GetNotificationNumbers,
        GetReplyReplies,
        GetSearchWords,
        GetShareList,
        GetTagDetail,
        GetTagFeeds,
        GetUserFeeds,
        GetUserList,
        GetUserSpace,
        GetUserProfile,
        OperateFollow,
        OperateLike,
        OperateUnfollow,
        OperateUnlike,
        SearchFeeds,
        SearchTags,
        SearchUsers,
    }

    public static class DataHelper
    {
        public static string GetMD5(string inputString)
        {
            using (var md5 = MD5.Create())
            {
                var r1 = md5.ComputeHash(Encoding.UTF8.GetBytes(inputString));
                var r2 = BitConverter.ToString(r1);
                var r3 = r2.Replace("-", "").ToLower();
                return r3;
            }
        }

        public static string GetUriStringTemplate(DataUriType type)
        {
            switch (type)
            {
                case DataUriType.CheckLoginInfo: return "/account/checkLoginInfo";
                case DataUriType.GetAnswers: return "/question/answerList?id={0}&sort={1}&page={2}{3}{4}";
                case DataUriType.GetDyhDetail: return "/dyh/detail?dyhId={0}";
                case DataUriType.GetDyhFeeds: return "/dyhArticle/list?dyhId={0}&type={1}&page={2}{3}{4}";
                case DataUriType.GetFeedDetail: return "/feed/detail?id={0}";
                case DataUriType.GetFeedReplies: return "/feed/replyList?id={0}&listType={1}&page={2}{3}{4}&discussMode=1&feedType=feed&blockStatus=0&fromFeedAuthor={5}";
                case DataUriType.GetHotReplies: return "/feed/hotReplyList?id={0}&page={1}{2}&discussMode=1";
                case DataUriType.GetLikeList: return "/feed/likeList?id={0}&listType=lastupdate_desc&page={1}{2}{3}";
                case DataUriType.GetIndexPage: return "{0}{1}page={2}";
                case DataUriType.GetIndexPageNames: return "/main/init";
                case DataUriType.GetNotifications: return "/notification/{0}?page={1}{2}{3}";
                case DataUriType.GetNotificationNumbers: return "/notification/checkCount";
                case DataUriType.GetReplyReplies: return "/feed/replyList?id={0}&listType=&page={1}{2}&discussMode=0&feedType=feed_reply&blockStatus=0&fromFeedAuthor=0";
                case DataUriType.GetSearchWords: return "/search/suggestSearchWordsNew?searchValue={0}&type=app";
                case DataUriType.GetShareList: return "/feed/forwardList?id={0}&type=feed&page={1}";
                case DataUriType.GetTagDetail: return "/topic/newTagDetail?tag={0}";
                case DataUriType.GetTagFeeds: return "/topic/tagFeedList?tag={0}&page={1}{2}{3}&listType={4}&blockStatus=0";
                case DataUriType.GetUserFeeds: return "/user/feedList?uid={0}&page={1}{2}{3}";
                case DataUriType.GetUserList: return "/user/{0}?uid={1}&page={2}{3}{4}";
                case DataUriType.GetUserSpace: return "/user/space?uid={0}";
                case DataUriType.GetUserProfile: return "/user/profile?uid={0}";
                case DataUriType.OperateFollow: return "/user/follow?uid={0}";
                case DataUriType.OperateLike: return "/feed/like{0}?id={1}&detail=0";
                case DataUriType.OperateUnfollow: return "/user/follow?uid={0}";
                case DataUriType.OperateUnlike: return "/feed/unlike{0}?id={1}&detail=0";
                case DataUriType.SearchFeeds: return "/search?type=feed&feedType={0}&sort={1}&searchValue={2}&page={3}{4}&showAnonymous=-1";
                case DataUriType.SearchTags: return "/search?type=feedTopic&searchValue={0}&page={1}{2}&showAnonymous=-1";
                case DataUriType.SearchUsers: return "/search?type=user&searchValue={0}&page={1}{2}&showAnonymous=-1";
                default: return string.Empty;
            }
        }

        public static string ConvertTime(double timestr)
        {
            DateTime time = new DateTime(1970, 1, 1).ToLocalTime().Add(new TimeSpan(Convert.ToInt64(timestr) * 10000000));
            TimeSpan temptime = DateTime.Now.Subtract(time);
            if (temptime.TotalDays > 30) return $"{time.Year}/{time.Month}/{time.Day}";
            else if (temptime.Days > 0) return $"{temptime.Days}天前";
            else if (temptime.Hours > 0) return $"{temptime.Hours}小时前";
            else if (temptime.Minutes > 0) return $"{temptime.Minutes}分钟前";
            else return "刚刚";
        }
    }
}