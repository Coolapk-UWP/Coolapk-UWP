using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;

namespace CoolapkUWP.Helpers
{
    enum DataType
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
    static class DataHelper
    {
        static readonly Dictionary<string, string> uris = new Dictionary<string, string>();

        static DataHelper()
        {
            uris.Add(DataType.CheckLoginInfo.ToString(), "/account/checkLoginInfo");
            uris.Add(DataType.GetAnswers.ToString(), "/question/answerList?id={0}&sort={1}&page={2}{3}{4}");
            uris.Add(DataType.GetDyhDetail.ToString(), "/dyh/detail?dyhId={0}");
            uris.Add(DataType.GetDyhFeeds.ToString(), "/dyhArticle/list?dyhId={0}&type={1}&page={2}{3)}{4}");
            uris.Add(DataType.GetFeedDetail.ToString(), "/feed/detail?id={0}");
            uris.Add(DataType.GetFeedReplies.ToString(), "/feed/replyList?id={0}&listType={1}&page={2}{3}{4}&discussMode=1&feedType=feed&blockStatus=0&fromFeedAuthor={5}");
            uris.Add(DataType.GetHotReplies.ToString(), "/feed/hotReplyList?id={0}&page={1}{2}&discussMode=1");
            uris.Add(DataType.GetLikeList.ToString(), "/feed/likeList?id={0}&listType=lastupdate_desc&page={1}{2}{3}");
            uris.Add(DataType.GetIndexPage.ToString(), "{0}{1}page={2}");
            uris.Add(DataType.GetIndexPageNames.ToString(), "/main/init");
            uris.Add(DataType.GetNotifications.ToString(), "/notification/{0}?page={1}{2}{3}");
            uris.Add(DataType.GetNotificationNumbers.ToString(), "/notification/checkCount");
            uris.Add(DataType.GetReplyReplies.ToString(), "/feed/replyList?id={0}&listType=&page={1}{2}&discussMode=0&feedType=feed_reply&blockStatus=0&fromFeedAuthor=0");
            uris.Add(DataType.GetSearchWords.ToString(), "/search/suggestSearchWordsNew?searchValue={0}&type=app");
            uris.Add(DataType.GetShareList.ToString(), "/feed/forwardList?id={0}&type=feed&page={1}");
            uris.Add(DataType.GetTagDetail.ToString(), "/topic/newTagDetail?tag={0}");
            uris.Add(DataType.GetTagFeeds.ToString(), "/topic/tagFeedList?tag={0}&page={1}{2}{3}&listType={4}&blockStatus=0");
            uris.Add(DataType.GetUserFeeds.ToString(), "/user/feedList?uid={0}&page={1}{2}{3}");
            uris.Add(DataType.GetUserList.ToString(), "/user/{0}?uid={1}&page={2}{3}{4}");
            uris.Add(DataType.GetUserSpace.ToString(), "/user/space?uid={0}");
            uris.Add(DataType.GetUserProfile.ToString(), "/user/profile?uid={0}");
            uris.Add(DataType.OperateFollow.ToString(), "/user/follow?uid={0}");
            uris.Add(DataType.OperateLike.ToString(), "/feed/like{0}?id={1}&detail=0");
            uris.Add(DataType.OperateUnfollow.ToString(), "/user/follow?uid={0}");
            uris.Add(DataType.OperateUnlike.ToString(), "/feed/unlike{0}?id={1}&detail=0");
            uris.Add(DataType.SearchFeeds.ToString(), "/search?type=feed&feedType={0}&sort={1}&searchValue={2}&page={3}{4}&showAnonymous=-1");
            uris.Add(DataType.SearchTags.ToString(), "/search?type=feedTopic&searchValue={0}&page={1}{2}&showAnonymous=-1");
            uris.Add(DataType.SearchUsers.ToString(), "/search?type=user&searchValue={0}&page={1}{2}&showAnonymous=-1");
        }

        //来源：https://blog.csdn.net/lindexi_gd/article/details/48951849
        public static string GetMD5(string inputString)
        {
            CryptographicHash objHash = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Md5).CreateHash();
            objHash.Append(CryptographicBuffer.ConvertStringToBinary(inputString, BinaryStringEncoding.Utf8));
            Windows.Storage.Streams.IBuffer buffHash1 = objHash.GetValueAndReset();
            return CryptographicBuffer.EncodeToHexString(buffHash1);
        }

        public static async Task<object> GetData(bool returnArray, DataType dataType, params object[] args)
        {
            var json = await NetworkHelper.GetJson(string.Format(uris[dataType.ToString()], args));
            try
            {
                if (string.IsNullOrEmpty(json)) return null;
                if (returnArray) return JsonObject.Parse(json)["data"].GetArray();
                else return JsonObject.Parse(json)["data"].GetObject();
            }
            catch
            {
                if (JsonObject.Parse(json).TryGetValue("message", out IJsonValue value))
                    UIHelper.ShowMessage($"{value.GetString()}");
                return null;
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