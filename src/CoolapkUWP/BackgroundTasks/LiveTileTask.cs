using CoolapkUWP.Helpers;
using CoolapkUWP.Models;
using Microsoft.Toolkit.Uwp.Notifications;
using Newtonsoft.Json.Linq;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Storage;
using Windows.UI.Notifications;

namespace CoolapkUWP.BackgroundTasks
{
    public sealed class LiveTileTask : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            BackgroundTaskDeferral deferral = taskInstance.GetDeferral();

            Uri uri = new Uri("https://api.coolapk.com/v6/page/dataList?url=V9_HOME_TAB_FOLLOW&type=circle");
            if (ApplicationData.Current.LocalSettings.Values["TileUrl"] != null)
            { uri = new Uri(ApplicationData.Current.LocalSettings.Values["TileUrl"].ToString()); }
            else { ApplicationData.Current.LocalSettings.Values["TileUrl"] = uri.ToString(); }
            try { await GetData(uri); } catch { }
            UIHelper.NotificationNums.GetNums();

            deferral.Complete();
        }

        public static void UpdateTile()
        {
            Uri uri = new Uri("https://api.coolapk.com/v6/page/dataList?url=V9_HOME_TAB_FOLLOW&type=circle");
            if (ApplicationData.Current.LocalSettings.Values["TileUrl"] != null)
            { uri = new Uri(ApplicationData.Current.LocalSettings.Values["TileUrl"].ToString()); }
            else { ApplicationData.Current.LocalSettings.Values["TileUrl"] = uri.ToString(); }
            try { _ = GetData(uri); } catch { }
        }

        private static async Task<JObject> GetJson(Uri uri)
        {
            (bool isSucceed, string result) = await DataHelper.GetHtmlAsync(uri, "XMLHttpRequest");
            return isSucceed ? JObject.Parse(result) : null;
        }

        public static async Task GetData(Uri uri)
        {
            JObject token = await GetJson(uri);
            if (token.TryGetValue("data", out JToken data))
            {
                int i = 0;
                foreach (JObject v in (JArray)data)
                {
                    if (i >= 5) { break; }
                    if (v.TryGetValue("entityType", out JToken entityType))
                    {
                        if (entityType.ToString() == "feed" || entityType.ToString() == "discovery")
                        {
                            i++;
                            UpdateTitle(GetFeedTitle(v));
                        }
                        else if (entityType.ToString() == "contacts")
                        {
                            if (v.TryGetValue("fUserInfo", out JToken fUserInfo))
                            {
                                i++;
                                UpdateTitle(GetUserTitle((JObject)fUserInfo));
                            }
                        }
                    }
                }
            }
        }

        private static void UpdateTitle(TileContent tileContent)
        {
            try
            {
                TileUpdateManager.CreateTileUpdaterForApplication().EnableNotificationQueue(true);
                TileNotification tileNotification = new TileNotification(tileContent.GetXml());
                TileUpdateManager.CreateTileUpdaterForApplication().Update(tileNotification);
            }
            catch { }
        }

        private static TileContent GetFeedTitle(JObject token)
        {
            FeedDetailModel FeedDetail = new FeedDetailModel(token);
            Regex r = new Regex("<a.*?>", RegexOptions.IgnoreCase);
            Regex r1 = new Regex("<a.*?/>", RegexOptions.IgnoreCase);
            Regex r2 = new Regex("</a.*?>", RegexOptions.IgnoreCase);
            string Message = r.Replace(FeedDetail.Message, "");
            Message = r1.Replace(Message, "");
            Message = r2.Replace(Message, "");
            return new TileContent()
            {
                Visual = new TileVisual()
                {
                    Branding = TileBranding.NameAndLogo,
                    DisplayName = FeedDetail.Info,
                    Arguments = FeedDetail.Url,

                    TileMedium = new TileBinding()
                    {
                        Content = new TileBindingContentAdaptive()
                        {
                            BackgroundImage = !FeedDetail.HavePic ? null : new TileBackgroundImage()
                            {
                                Source = FeedDetail.Pic.Uri,
                                HintOverlay = 70
                            },

                            Children =
                            {
                                new AdaptiveText()
                                {
                                    Text = FeedDetail.Username,
                                    HintStyle = AdaptiveTextStyle.Caption,
                                },

                                new AdaptiveText()
                                {
                                    Text = Message,
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                    HintWrap = true,
                                    HintMaxLines = 3
                                }
                            }
                        }
                    },
                    TileWide = new TileBinding()
                    {
                        Content = new TileBindingContentAdaptive()
                        {
                            BackgroundImage = !FeedDetail.HavePic ? null : new TileBackgroundImage()
                            {
                                Source = FeedDetail.Pic.Uri,
                                HintOverlay = 70
                            },

                            Children =
                            {
                                new AdaptiveGroup()
                                {
                                    Children =
                                    {
                                        new AdaptiveSubgroup()
                                        {
                                            HintWeight = 33,
                                            Children =
                                            {
                                                new AdaptiveImage()
                                                {
                                                    Source = FeedDetail.UserSmallAvatar.Uri,
                                                    HintCrop = AdaptiveImageCrop.Circle
                                                }
                                            },
                                        },
                                        new AdaptiveSubgroup()
                                        {
                                            Children =
                                            {
                                                new AdaptiveText()
                                                {
                                                    Text = FeedDetail.Username,
                                                    HintStyle = AdaptiveTextStyle.Caption,
                                                },

                                                new AdaptiveText()
                                                {
                                                    Text = Message,
                                                    HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                                    HintWrap = true,
                                                    HintMaxLines = 3
                                                }
                                            },
                                        }
                                    }
                                },
                            }
                        }
                    },
                    TileLarge = new TileBinding()
                    {
                        Content = new TileBindingContentAdaptive()
                        {
                            BackgroundImage = !FeedDetail.HavePic ? null : new TileBackgroundImage()
                            {
                                Source = FeedDetail.Pic.Uri,
                                HintOverlay = 70
                            },

                            Children =
                            {
                                new AdaptiveText()
                                {
                                    Text = FeedDetail.Username,
                                    HintStyle = AdaptiveTextStyle.Base,
                                },

                                new AdaptiveText()
                                {
                                    Text = Message,
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                    HintWrap = true
                                }
                            }
                        }
                    }
                }
            };
        }

        private static TileContent GetUserTitle(JObject token)
        {
            UserModel UserDetail = new UserModel(token);
            Windows.ApplicationModel.Resources.ResourceLoader loader = Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse("FeedListPage");
            return new TileContent()
            {
                Visual = new TileVisual()
                {
                    Branding = TileBranding.NameAndLogo,
                    DisplayName = UserDetail.UserName,
                    Arguments = UserDetail.Url,

                    TileMedium = new TileBinding()
                    {
                        Content = new TileBindingContentAdaptive()
                        {
                            BackgroundImage = new TileBackgroundImage()
                            {
                                Source = UserDetail.Background.Uri,
                                HintOverlay = 70
                            },

                            Children =
                            {
                                new AdaptiveText()
                                {
                                    Text = UserDetail.UserName,
                                    HintStyle = AdaptiveTextStyle.Caption,
                                },

                                new AdaptiveText()
                                {
                                    Text = UserDetail.FollowNum + loader.GetString("follow") + UserDetail.FansNum + loader.GetString("fans") + UserDetail.LoginTime + loader.GetString("active"),
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                },

                                new AdaptiveText()
                                {
                                    Text = UserDetail.Bio,
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                    HintWrap = true,
                                    HintMaxLines = 2
                                }
                            }
                        }
                    },
                    TileWide = new TileBinding()
                    {
                        Content = new TileBindingContentAdaptive()
                        {
                            BackgroundImage = new TileBackgroundImage()
                            {
                                Source = UserDetail.Background.Uri,
                                HintOverlay = 70
                            },

                            Children =
                            {
                                new AdaptiveGroup()
                                {
                                    Children =
                                    {
                                        new AdaptiveSubgroup()
                                        {
                                            HintWeight = 33,
                                            Children =
                                            {
                                                new AdaptiveImage()
                                                {
                                                    Source = UserDetail.UserAvatar.Uri,
                                                    HintCrop = AdaptiveImageCrop.Circle
                                                }
                                            },
                                        },
                                        new AdaptiveSubgroup()
                                        {
                                            Children =
                                            {
                                                new AdaptiveText()
                                                {
                                                    Text = UserDetail.UserName,
                                                    HintStyle = AdaptiveTextStyle.Caption,
                                                },

                                                new AdaptiveText()
                                                {
                                                    Text = UserDetail.FollowNum + loader.GetString("follow") + UserDetail.FansNum + loader.GetString("fans") + UserDetail.LoginTime + loader.GetString("active"),
                                                    HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                                },

                                                new AdaptiveText()
                                                {
                                                    Text = UserDetail.Bio,
                                                    HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                                    HintWrap = true,
                                                    HintMaxLines = 2
                                                }
                                            },
                                        }
                                    }
                                },
                            }
                        }
                    },
                    TileLarge = new TileBinding()
                    {
                        Content = new TileBindingContentAdaptive()
                        {
                            BackgroundImage = new TileBackgroundImage()
                            {
                                Source = UserDetail.Background.Uri,
                                HintOverlay = 70
                            },

                            Children =
                            {
                                new AdaptiveGroup()
                                {
                                    Children =
                                    {
                                        new AdaptiveSubgroup()
                                        {
                                            HintWeight = 2,
                                            Children =
                                            {
                                                new AdaptiveImage()
                                                {
                                                    Source = UserDetail.UserAvatar.Uri,
                                                    HintCrop = AdaptiveImageCrop.Circle
                                                }
                                            }
                                        },

                                        new AdaptiveSubgroup()
                                        {
                                            HintWeight = 1
                                        },

                                        new AdaptiveSubgroup()
                                        {
                                            HintWeight = 2
                                        },
                                    }
                                },

                                new AdaptiveText()
                                {
                                    Text = UserDetail.UserName,
                                    HintStyle = AdaptiveTextStyle.Caption,
                                },

                                new AdaptiveText()
                                {
                                    Text = UserDetail.FollowNum + loader.GetString("follow") + UserDetail.FansNum + loader.GetString("fans") + UserDetail.LoginTime + loader.GetString("active"),
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                },

                                new AdaptiveText()
                                {
                                    Text = UserDetail.Bio,
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                    HintWrap = true,
                                }
                            }
                        }
                    }
                }
            };
        }
    }
}
