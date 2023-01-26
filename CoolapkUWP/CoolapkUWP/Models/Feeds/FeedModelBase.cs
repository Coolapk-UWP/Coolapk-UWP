﻿using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Images;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using Windows.UI.Xaml.Shapes;

namespace CoolapkUWP.Models.Feeds
{
    public class FeedModelBase : SourceFeedModel, ICanFollow, ICanLike, ICanReply, ICanStar, ICanCopy
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

        public int ID => EntityID;
        public int UID => UserInfo.UID;
        public int ShareNum { get; private set; }
        public int ReplyRowsCount { get; private set; }

        public bool Stared { get; set; }
        public bool ShowSourceFeed { get; private set; }
        public bool ShowRelationRows { get; private set; }
        public bool ShowLinkSourceFeed { get; private set; }

        public string Info { get; private set; }
        public string ExtraUrl { get; private set; }
        public string ExtraTitle { get; private set; }
        public string DeviceTitle { get; private set; }
        public string ExtraSubtitle { get; private set; }

        public ImageModel ExtraPic { get; private set; }
        public SourceFeedModel SourceFeed { get; private set; }
        public LinkFeedModel LinkSourceFeed { get; private set; }

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

            if (token.TryGetValue("device_title", out JToken device_title))
            {
                DeviceTitle = device_title.ToString();
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
                (token.TryGetValue("relationRows", out JToken relationRows) && relationRows.Any());

            if (ShowRelationRows)
            {
                ImmutableArray<RelationRowsItem>.Builder buider = ImmutableArray.CreateBuilder<RelationRowsItem>();
                if (location != null && !string.IsNullOrEmpty(location.ToString()))
                {
                    buider.Add(new RelationRowsItem { Title = location.ToString() });
                }

                if (ttitle != null && !string.IsNullOrEmpty(ttitle.ToString()))
                {
                    buider.Add(new RelationRowsItem { Title = ttitle.ToString(), Url = token.Value<string>("turl"), Logo = new ImageModel(token.Value<string>("tpic"), ImageType.Icon) });
                }

                if (EntityType != "article" && dyh_name != null && !string.IsNullOrEmpty(dyh_name.ToString()))
                {
                    buider.Add(new RelationRowsItem { Title = dyh_name.ToString(), Url = $"/dyh/{token["dyh_id"].ToString().Replace("\"", string.Empty)}" });
                }

                if (relationRows != null)
                {
                    foreach (JToken i in relationRows)
                    {
                        JObject item = i as JObject;
                        buider.Add(new RelationRowsItem
                        {
                            Title = item.Value<string>("title"),
                            Url = item.Value<string>("url"),
                            Logo = new ImageModel(item.Value<string>("logo"), ImageType.Icon)
                        });
                    }
                }

                ShowRelationRows = buider.Any();
                RelationRows = buider.ToImmutable();
            }

            if (token.TryGetValue("forwardSourceFeed", out JToken forwardSourceFeed)
                && !string.IsNullOrEmpty(forwardSourceFeed.ToString())
                && forwardSourceFeed.ToString() != "null")
            {
                ShowSourceFeed = true;
                SourceFeed = new SourceFeedModel(forwardSourceFeed as JObject);
            }
        }
    }

    public class RelationRowsItem
    {
        public string Url { get; set; }
        public string Title { get; set; }
        public ImageModel Logo { get; set; }
    }
}
