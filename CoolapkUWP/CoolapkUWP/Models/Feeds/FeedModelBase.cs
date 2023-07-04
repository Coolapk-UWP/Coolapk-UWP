using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Images;
using CoolapkUWP.Models.Users;
using Microsoft.Toolkit.Uwp.Helpers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.UI;

namespace CoolapkUWP.Models.Feeds
{
    public class FeedModelBase : SourceFeedModel, ICanFollow, ICanLike, ICanReply, ICanStar
    {
        private int likeNum;
        public int LikeNum
        {
            get => likeNum;
            set
            {
                if (likeNum != value)
                {
                    likeNum = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private int replyNum;
        public int ReplyNum
        {
            get => replyNum;
            set
            {
                if (replyNum != value)
                {
                    replyNum = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private int starNum;
        public int StarNum
        {
            get => starNum;
            set
            {
                if (starNum != value)
                {
                    starNum = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        public bool Liked
        {
            get => UserAction.Like;
            set
            {
                if (UserAction.Like != value)
                {
                    UserAction.Like = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        public bool Followed
        {
            get => UserAction.FollowAuthor;
            set
            {
                if (UserAction.FollowAuthor != value)
                {
                    UserAction.FollowAuthor = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private bool showButtons = true;
        public bool ShowButtons
        {
            get => showButtons;
            set
            {
                if (showButtons != value)
                {
                    showButtons = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        int ICanFollow.ID => UID;

        public int ID => EntityID;
        public int UID => UserInfo.UID;
        public int ShareNum { get; private set; }
        public int TotalVoteNum { get; private set; }
        public int ReplyRowsCount { get; private set; }
        public int TotalCommentNum { get; private set; }
        public int QuestionAnswerNum { get; private set; }
        public int QuestionFollowNum { get; private set; }

        public bool Stared { get; set; }
        public bool ShowSourceFeed { get; private set; }
        public bool EmptySourceFeed { get; private set; }
        public bool ShowRelationRows { get; private set; }
        public bool ShowLinkSourceFeed { get; private set; }

        public string Info { get; private set; }
        public string InfoHTML { get; private set; }
        public string ExtraUrl { get; private set; }
        public string MediaUrl { get; private set; }
        public string IPLocation { get; private set; }
        public string ExtraTitle { get; private set; }
        public string DeviceTitle { get; private set; }
        public string VoteEndTime { get; private set; }
        public string VoteStartTime { get; private set; }
        public string ExtraSubtitle { get; private set; }
        public string MediaSubtitle { get; private set; }

        public ImageModel ExtraPic { get; private set; }
        public ImageModel MediaPic { get; private set; }
        public SourceFeedModel SourceFeed { get; private set; }
        public LinkFeedModel LinkSourceFeed { get; private set; }

        public ImmutableArray<VoteItem> VoteRows { get; private set; } = ImmutableArray<VoteItem>.Empty;
        public ImmutableArray<RelationRowsItem> RelationRows { get; private set; } = ImmutableArray<RelationRowsItem>.Empty;
        public ImmutableArray<SourceFeedReplyModel> ReplyRows { get; private set; } = ImmutableArray<SourceFeedReplyModel>.Empty;

        public FeedModelBase(JObject token) : base(token)
        {
            if (token.TryGetValue("info", out JToken info) && !string.IsNullOrEmpty(info.ToString()))
            {
                Info = info.ToString();
            }
            else if (token.TryGetValue("feedTypeName", out JToken feedTypeName))
            {
                Info = feedTypeName.ToString();
            }

            InfoHTML = token.TryGetValue("infoHtml", out JToken infoHtml) && !string.IsNullOrEmpty(infoHtml.ToString())
                ? infoHtml.ToString()
                : Dateline;

            if (token.TryGetValue("likenum", out JToken likenum))
            {
                LikeNum = likenum.ToObject<int>();
            }

            if (token.TryGetValue("favnum", out JToken favnum))
            {
                StarNum = favnum.ToObject<int>();
            }

            if (token.TryGetValue("replynum", out JToken replynum))
            {
                ReplyNum = replynum.ToObject<int>();
            }

            if (token.TryGetValue("forwardnum", out JToken forwardnum))
            {
                ShareNum = forwardnum.ToObject<int>();
            }

            if (IsVoteFeed)
            {
                if (token.TryGetValue("vote", out JToken v))
                {
                    JObject vote = (JObject)v;
                    if (vote.TryGetValue("total_vote_num", out JToken total_vote_num))
                    {
                        TotalVoteNum = total_vote_num.ToObject<int>();
                    }

                    if (vote.TryGetValue("total_comment_num", out JToken total_comment_num))
                    {
                        TotalCommentNum = total_comment_num.ToObject<int>();
                    }

                    if (vote.TryGetValue("start_time", out JToken start_time))
                    {
                        VoteStartTime = start_time.ToObject<long>().ConvertUnixTimeStampToReadable(null);
                    }

                    if (vote.TryGetValue("end_time", out JToken end_time))
                    {
                        VoteEndTime = end_time.ToObject<long>().ConvertUnixTimeStampToReadable(null);
                    }

                    if (vote.TryGetValue("options", out JToken options))
                    {
                        VoteRows = options.Select(item => new VoteItem((JObject)item)).ToImmutableArray();
                    }
                }
            }

            if (IsQuestionFeed)
            {
                if (token.TryGetValue("question_answer_num", out JToken question_answer_num))
                {
                    QuestionAnswerNum = question_answer_num.ToObject<int>();
                }
                if (token.TryGetValue("question_follow_num", out JToken question_follow_num))
                {
                    QuestionFollowNum = question_follow_num.ToObject<int>();
                }
            }

            if (token.TryGetValue("device_title", out JToken device_title) && !string.IsNullOrEmpty(device_title.ToString()))
            {
                DeviceTitle = device_title.ToString();
            }
            else if (token.TryGetValue("device_name", out JToken device_name))
            {
                DeviceTitle = device_name.ToString();
            }

            if (token.TryGetValue("ip_location", out JToken ip_location))
            {
                IPLocation = ip_location.ToString();
            }

            if (token.TryGetValue("extra_title", out JToken extra_title) && !string.IsNullOrEmpty(extra_title.ToString()))
            {
                ExtraTitle = extra_title.ToString();

                if (token.TryGetValue("extra_url", out JToken extra_url))
                {
                    ExtraUrl = extra_url.ToString();

                    if (ExtraUrl.Contains("b23.tv") || ExtraUrl.Contains("t.cn"))
                    {
                        ExtraUrl = ExtraUrl.ValidateAndGetUri().ExpandShortUrl();
                    }

                    ExtraSubtitle = ExtraUrl.ValidateAndGetUri() is Uri ExtraUri && ExtraUri != null ? ExtraUri.Host : ExtraUrl;

                    if (token.TryGetValue("extra_pic", out JToken extra_pic))
                    {
                        ExtraPic = new ImageModel(extra_pic.ToString(), ImageType.Icon);
                    }

                    if (ExtraUrl.Contains("coolapk") && ExtraUrl.Contains("feed"))
                    {
                        LinkSourceFeed = new LinkFeedModel(new Uri(ExtraUrl), LinkType.Coolapk);
                        ShowLinkSourceFeed = true;
                    }
                    else if (ExtraUrl.Contains("bilibili") && ExtraUrl.Contains("t.bilibili"))
                    {
                        Regex GetID = new Regex(@"/t.*?/([\d|\w]+)");
                        Uri uri = UriHelper.GetLinkUri(UriType.GetBilibiliFeed, LinkType.Bilibili, GetID.Match(ExtraUrl).Groups[1].Value);
                        MultipartFormDataContent content = new MultipartFormDataContent { { new StringContent(GetID.Match(ExtraUrl).Groups[1].Value), "dynamic_id" } };
                        LinkSourceFeed = new LinkFeedModel(uri, LinkType.Bilibili, true, content);
                        ShowLinkSourceFeed = true;
                    }
                    else if (ExtraUrl.Contains("ithome") && ExtraUrl.Contains("qcontent"))
                    {
                        Regex GetID = new Regex(@"[%26|%3F]id%3D([\d|\w]+)");
                        Uri uri = UriHelper.GetLinkUri(UriType.GetITHomeFeed, LinkType.ITHome, GetID.Match(ExtraUrl).Groups[1].Value);
                        LinkSourceFeed = new LinkFeedModel(uri, LinkType.ITHome);
                        ShowLinkSourceFeed = true;
                    }
                }
            }

            if (token.TryGetValue("media_url", out JToken media_url))
            {
                MediaUrl = media_url.ToString();
                MediaSubtitle = MediaUrl.ValidateAndGetUri() is Uri ExtraUri && ExtraUri != null ? ExtraUri.Host : MediaUrl;

                if (token.TryGetValue("media_pic", out JToken media_pic))
                {
                    MediaPic = new ImageModel(media_pic.ToString(), ImageType.Icon);
                }
            }

            if (token.TryGetValue("replyRowsCount", out JToken replyRowsCount))
            {
                ReplyRowsCount = replyRowsCount.ToObject<int>();
            }

            if (token.TryGetValue("replyRows", out JToken replyRows))
            {
                ReplyRows = replyRows.Select(item => new SourceFeedReplyModel((JObject)item)).ToImmutableArray();
            }

            ShowRelationRows =
                (token.TryGetValue("location", out JToken location) && !string.IsNullOrEmpty(location.ToString())) |
                (token.TryGetValue("ttitle", out JToken ttitle) && !string.IsNullOrEmpty(ttitle.ToString())) |
                (token.TryGetValue("dyh_name", out JToken dyh_name) && !string.IsNullOrEmpty(dyh_name.ToString())) |
                (token.TryGetValue("relationRows", out JToken relationRows) && relationRows.Any()) |
                (token.TryGetValue("change_count", out JToken change_count) && change_count.ToObject<int>() > 0) |
                (token.TryGetValue("status", out JToken status) && status.ToObject<int>() == -1) |
                (token.TryGetValue("block_status", out JToken block_status) && block_status.ToObject<int>() != 0);

            if (ShowRelationRows)
            {
                ImmutableArray<RelationRowsItem>.Builder buider = ImmutableArray.CreateBuilder<RelationRowsItem>();
                if (location != null && !string.IsNullOrEmpty(location.ToString()))
                {
                    buider.Add(
                        new RelationRowsItem(
                            title: location.ToString(),
                            icon: "\uE707"));
                }

                if (ttitle != null && !string.IsNullOrEmpty(ttitle.ToString()))
                {
                    buider.Add(
                        new RelationRowsItem(
                            url: token.Value<string>("turl"),
                            title: ttitle.ToString(),
                            logo: token.Value<string>("tpic")));
                }

                if (EntityType != "article" && dyh_name != null && !string.IsNullOrEmpty(dyh_name.ToString()))
                {
                    buider.Add(
                        new RelationRowsItem(
                            url: $"/dyh/{token["dyh_id"]}",
                            title: dyh_name.ToString()));
                }

                if (relationRows != null)
                {
                    foreach (JToken i in relationRows)
                    {
                        JObject item = i as JObject;
                        buider.Add(
                            new RelationRowsItem(
                                url: item.Value<string>("url"),
                                title: item.Value<string>("title"),
                                logo: item.Value<string>("logo")));
                    }
                }

                if (change_count != null && change_count.ToObject<int>() > 0)
                {
                    buider.Add(
                        new RelationRowsItem(
                            url: $"/feed/changeHistoryList?id={ID}",
                            title: $"已编辑{change_count.ToObject<int>()}次",
                            icon: "\uE70F"));
                }

                if (status != null && status.ToObject<int>() == -1)
                {
                    buider.Add(
                        new RelationRowsItem(
                            title: "仅自己可见",
                            icon: "\uE727"));
                }

                if (block_status != null && block_status.ToObject<int>() != 0)
                {
                    buider.Add(
                        new RelationRowsItem(
                            title: "已折叠",
                            icon: "\uE7BA"));
                }

                ShowRelationRows = buider.Any();
                RelationRows = buider.ToImmutable();
            }

            if (!IsQuestionFeed
                && token.TryGetValue("source_id", out JToken source_id)
                && !string.IsNullOrEmpty(source_id.ToString()))
            {
                ShowSourceFeed = true;
                if (token.TryGetValue("forwardSourceFeed", out JToken forwardSourceFeed)
                    && !string.IsNullOrEmpty(forwardSourceFeed.ToString())
                    && forwardSourceFeed.ToString() != "null")
                {
                    SourceFeed = new SourceFeedModel(forwardSourceFeed as JObject);
                }
                else
                {
                    EmptySourceFeed = true;
                }
            }
        }

        public async Task ChangeLike()
        {
            UriType type = Liked ? UriType.PostFeedUnlike : UriType.PostFeedLike;
            (bool isSucceed, JToken result) = await RequestHelper.PostDataAsync(UriHelper.GetOldUri(type, string.Empty, ID), null, true);
            if (!isSucceed) { return; }
            Liked = !Liked;
            if (((JObject)result).TryGetValue("count", out JToken count))
            {
                LikeNum = count.ToObject<int>();
            }
        }

        public async Task ChangeFollow()
        {
            UriType type = Followed ? UriType.PostUserUnfollow : UriType.PostUserFollow;

            (bool isSucceed, _) = await RequestHelper.PostDataAsync(UriHelper.GetUri(type, UID), null, true);
            if (!isSucceed) { return; }

            Followed = !Followed;
        }
    }

    public class VoteItem
    {
        public int ID { get; set; }
        public int Order { get; set; }
        public int VoteID { get; set; }
        public int Status { get; set; }

        public string Title { get; set; }

        public Color Color { get; set; }

        public VoteItem(JObject token)
        {
            if (token.TryGetValue("id", out JToken id))
            {
                ID = id.ToObject<int>();
            }

            if (token.TryGetValue("order", out JToken order))
            {
                Order = order.ToObject<int>();
            }

            if (token.TryGetValue("vote_id", out JToken vote_id))
            {
                VoteID = vote_id.ToObject<int>();
            }

            if (token.TryGetValue("status", out JToken status))
            {
                Status = status.ToObject<int>();
            }

            if (token.TryGetValue("title", out JToken title))
            {
                Title = title.ToString();
            }

            if (token.TryGetValue("color", out JToken color))
            {
                if (!string.IsNullOrEmpty(color.ToString()))
                {
                    try
                    {
                        Color = color.ToString().ToColor();
                    }
                    catch
                    {
                        Color = Colors.Transparent;
                    }
                }
                else
                {
                    Color = Colors.Transparent;
                }
            }
        }
    }

    public class RelationRowsItem
    {
        public string Url { get; set; }
        public string Title { get; set; }

        public string Icon { get; set; }
        public ImageModel Logo { get; set; }

        public bool IsShowLogo => Logo != null;
        public bool IsShowIcon => Logo != null || !string.IsNullOrWhiteSpace(Icon);

        public RelationRowsItem(string url = null, string title = null, string icon = null, string logo = null)
        {
            Url = url;
            Title = title;
            Icon = icon;
            if (logo != null)
            {
                Logo = new ImageModel(logo, ImageType.Icon);
            }
        }
    }
}
