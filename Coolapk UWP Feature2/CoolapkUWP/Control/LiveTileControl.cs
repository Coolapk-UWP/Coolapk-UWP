using CoolapkUWP.Control.ViewModels;
using CoolapkUWP.Data;
using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Data.Json;
using Windows.Storage;
using Windows.UI.Notifications;

namespace CoolapkUWP.Control
{
    public sealed class LiveTileControl : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            BackgroundTaskDeferral deferral = taskInstance.GetDeferral();

            string uri = "https://api.coolapk.com/v6/page/dataList?url=V9_HOME_TAB_FOLLOW&type=circle";
            if (ApplicationData.Current.LocalSettings.Values["TileUrl"] != null)
            { uri = ApplicationData.Current.LocalSettings.Values["TileUrl"].ToString(); }
            else { ApplicationData.Current.LocalSettings.Values["TileUrl"] = uri.ToString(); }
            try { await GetData(uri); } catch { }

            deferral.Complete();
        }

        public static void UpdateTile()
        {
            string uri = "https://api.coolapk.com/v6/page/dataList?url=V9_HOME_TAB_FOLLOW&type=circle";
            if (ApplicationData.Current.LocalSettings.Values["TileUrl"] != null)
            { uri = ApplicationData.Current.LocalSettings.Values["TileUrl"].ToString(); }
            else { ApplicationData.Current.LocalSettings.Values["TileUrl"] = uri.ToString(); }
            try { _ = GetData(uri); } catch { }
        }

        private static async Task<JsonObject> GetJson(string uri)
        {
            string result = await UIHelper.GetHTML(uri, "XMLHttpRequest", true);
            return !string.IsNullOrEmpty(result) ? JsonObject.Parse(result) : null;
        }

        public static async Task GetData(string uri)
        {
            JsonObject token = await GetJson(uri);
            if (token != null && token.TryGetValue("data", out IJsonValue data))
            {
                int i = 0;
                foreach (IJsonValue v in data.GetArray())
                {
                    if (i >= 5) { break; }
                    JsonObject value = v.GetObject();
                    if (value.TryGetValue("entityType", out IJsonValue entityType))
                    {
                        if (entityType.GetString() == "feed" || entityType.GetString() == "discovery")
                        {
                            i++;
                            UpdateTitle(GetFeedTitle(value));
                        }
                        else if (entityType.GetString() == "contacts")
                        {
                            if (value.TryGetValue("fUserInfo", out IJsonValue fUserInfo))
                            {
                                i++;
                                UpdateTitle(GetUserTitle(fUserInfo.GetObject()));
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

        private static TileContent GetFeedTitle(JsonObject token)
        {
            FeedDetailViewModel FeedDetail = new FeedDetailViewModel(token);
            string Message = UIHelper.ReplaceHtml(FeedDetail.message);
            return new TileContent()
            {
                Visual = new TileVisual()
                {
                    Branding = TileBranding.NameAndLogo,
                    DisplayName = FeedDetail.info,
                    Arguments = FeedDetail.url,

                    TileMedium = new TileBinding()
                    {
                        Content = new TileBindingContentAdaptive()
                        {
                            BackgroundImage = !FeedDetail.havePic ? null : new TileBackgroundImage()
                            {
                                Source = FeedDetail.picUrl,
                                HintOverlay = 70
                            },

                            Children =
                            {
                                new AdaptiveText()
                                {
                                    Text = FeedDetail.username,
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
                            BackgroundImage = !FeedDetail.havePic ? null : new TileBackgroundImage()
                            {
                                Source = FeedDetail.picUrl,
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
                                                    Source = FeedDetail.userSmallAvatarUrl,
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
                                                    Text = FeedDetail.username,
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
                            BackgroundImage = !FeedDetail.havePic ? null : new TileBackgroundImage()
                            {
                                Source = FeedDetail.picUrl,
                                HintOverlay = 70
                            },

                            Children =
                            {
                                new AdaptiveText()
                                {
                                    Text = FeedDetail.username,
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

        private static TileContent GetUserTitle(JsonObject token)
        {
            UserViewModel UserDetail = new UserViewModel(token);
            Windows.ApplicationModel.Resources.ResourceLoader loader = Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse("FeedListPage");
            return new TileContent()
            {
                Visual = new TileVisual()
                {
                    Branding = TileBranding.NameAndLogo,
                    DisplayName = UserDetail.UserName,
                    Arguments = UserDetail.url,

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
                                    Text = UserDetail.FollowNum + loader.GetString("follow") + UserDetail.FansNum + loader.GetString("fan") + UserDetail.LoginTime + loader.GetString("active"),
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
                                                    Source = UserDetail.UserAvatarUrl,
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
                                                    Text = UserDetail.FollowNum + loader.GetString("follow") + UserDetail.FansNum + loader.GetString("fan") + UserDetail.LoginTime + loader.GetString("active"),
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
                                                    Source = UserDetail.UserAvatarUrl,
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
                                    Text = UserDetail.FollowNum + loader.GetString("follow") + UserDetail.FansNum + loader.GetString("fan") + UserDetail.LoginTime + loader.GetString("active"),
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
