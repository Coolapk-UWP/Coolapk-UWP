using CoolapkUWP.Helpers;
using CoolapkUWP.Models;
using CoolapkUWP.Models.Feeds;
using CoolapkUWP.Models.Users;
using Microsoft.Toolkit.Uwp.Notifications;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Resources;
using Windows.UI.Notifications;

namespace CoolapkUWP.BackgroundTasks
{
    public sealed class LiveTileTask : IBackgroundTask
    {
        public static LiveTileTask Instance = new LiveTileTask();

        public LiveTileTask()
        {
            Instance = Instance ?? this;
        }

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            BackgroundTaskDeferral deferral = taskInstance.GetDeferral();
            UpdateTile();
            deferral.Complete();
        }

        public async void UpdateTile()
        {
            Uri uri = new Uri(SettingsHelper.Get<string>(SettingsHelper.TileUrl));
            try
            {
                await GetData(uri);
            }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.GetLogger(nameof(LiveTileTask)).Error(ex.ExceptionToMessage(), ex);
            }
        }

        private async Task GetData(Uri uri)
        {
            (bool isSucceed, JToken result) = await RequestHelper.GetDataAsync(uri, true);
            if (isSucceed)
            {
                JArray array = (JArray)result;
                if (array.Count < 1) { return; }
                int i = 0;
                foreach (JToken item in array)
                {
                    if (i >= 5) { break; }
                    if (((JObject)item).TryGetValue("entityType", out JToken entityType))
                    {
                        if (entityType.ToString() == "feed" || entityType.ToString() == "discovery")
                        {
                            i++;
                            UpdateTitle(GetFeedTitle((JObject)item));
                        }
                    }
                }
            }
        }

        private void UpdateTitle(TileContent tileContent)
        {
            try
            {
                TileUpdater tileUpdater = TileUpdateManager.CreateTileUpdaterForApplication();
                tileUpdater.EnableNotificationQueue(true);
                TileNotification tileNotification = new TileNotification(tileContent.GetXml());
                tileUpdater.Update(tileNotification);
            }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.GetLogger(nameof(LiveTileTask)).Error(ex.ExceptionToMessage(), ex);
            }
        }

        public static TileContent GetFeedTitle(JObject token)
        {
            FeedModel FeedDetail = new FeedModel(token);
            return GetFeedTitle(FeedDetail);
        }

        public static TileContent GetFeedTitle(FeedModelBase FeedDetail)
        {
            string Message = FeedDetail.Message.CSStoString();
            return new TileContent
            {
                Visual = new TileVisual
                {
                    Branding = TileBranding.NameAndLogo,
                    DisplayName = FeedDetail.Info,

                    TileMedium = new TileBinding
                    {
                        Content = new TileBindingContentAdaptive
                        {
                            BackgroundImage = FeedDetail.Pic == null ? null : new TileBackgroundImage
                            {
                                Source = FeedDetail.Pic.Uri,
                                HintOverlay = 70
                            },

                            Children =
                            {
                                new AdaptiveText
                                {
                                    Text = FeedDetail.UserInfo.UserName,
                                    HintStyle = AdaptiveTextStyle.Caption,
                                },

                                new AdaptiveText
                                {
                                    Text = Message,
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                    HintWrap = true,
                                    HintMaxLines = 3
                                }
                            }
                        }
                    },

                    TileWide = new TileBinding
                    {
                        Content = new TileBindingContentAdaptive
                        {
                            BackgroundImage = FeedDetail.Pic == null ? null : new TileBackgroundImage
                            {
                                Source = FeedDetail.Pic.Uri,
                                HintOverlay = 70
                            },

                            Children =
                            {
                                new AdaptiveGroup
                                {
                                    Children =
                                    {
                                        new AdaptiveSubgroup
                                        {
                                            HintWeight = 33,
                                            Children =
                                            {
                                                new AdaptiveImage
                                                {
                                                    Source = FeedDetail.UserInfo.UserAvatar.Uri,
                                                    HintCrop = AdaptiveImageCrop.Circle
                                                }
                                            },
                                        },
                                        new AdaptiveSubgroup
                                        {
                                            Children =
                                            {
                                                new AdaptiveText
                                                {
                                                    Text = FeedDetail.UserInfo.UserName,
                                                    HintStyle = AdaptiveTextStyle.Caption,
                                                },

                                                new AdaptiveText
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

                    TileLarge = new TileBinding
                    {
                        Content = new TileBindingContentAdaptive
                        {
                            BackgroundImage = FeedDetail.Pic == null ? null : new TileBackgroundImage
                            {
                                Source = FeedDetail.Pic.Uri,
                                HintOverlay = 70
                            },

                            Children =
                            {
                                new AdaptiveGroup
                                {
                                    Children =
                                    {
                                        new AdaptiveSubgroup
                                        {
                                            HintWeight = 33,
                                            Children =
                                            {
                                                new AdaptiveImage
                                                {
                                                    Source = FeedDetail.UserInfo.UserAvatar.Uri,
                                                    HintCrop = AdaptiveImageCrop.Circle
                                                },
                                            }
                                        },

                                        new AdaptiveSubgroup()
                                    }
                                },

                                new AdaptiveText
                                {
                                    Text = FeedDetail.UserInfo.UserName,
                                    HintStyle = AdaptiveTextStyle.Caption,
                                },

                                new AdaptiveText
                                {
                                    Text = Message,
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                    HintWrap = true,
                                    HintMaxLines = 3
                                }
                            }
                        }
                    }
                }
            };
        }

        public static TileContent GetUserTitle(JObject token)
        {
            UserModel UserDetail = new UserModel(token);
            return GetUserTitle(UserDetail);
        }

        public static TileContent GetUserTitle(IUserModel UserDetail)
        {
            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("FeedListPage");
            return new TileContent
            {
                Visual = new TileVisual
                {
                    Branding = TileBranding.NameAndLogo,
                    DisplayName = UserDetail.UserName,

                    TileSmall = new TileBinding
                    {
                        Content = new TileBindingContentAdaptive
                        {
                            BackgroundImage = new TileBackgroundImage
                            {
                                Source = UserDetail.Cover.Uri,
                                HintOverlay = 70
                            },

                            Children =
                            {
                                new AdaptiveImage
                                {
                                    Source = UserDetail.UserAvatar.Uri,
                                    HintCrop = AdaptiveImageCrop.Circle
                                }
                            }
                        }
                    },

                    TileMedium = new TileBinding
                    {
                        Content = new TileBindingContentAdaptive
                        {
                            BackgroundImage = new TileBackgroundImage
                            {
                                Source = UserDetail.Cover.Uri,
                                HintOverlay = 70
                            },

                            PeekImage = new TilePeekImage
                            {
                                Source = UserDetail.UserAvatar.Uri,
                                HintCrop = TilePeekImageCrop.Circle,
                            },

                            Children =
                            {
                                new AdaptiveText
                                {
                                    Text = UserDetail.UserName,
                                    HintStyle = AdaptiveTextStyle.Caption,
                                },

                                new AdaptiveText
                                {
                                    Text = $"{UserDetail.FollowNum}{loader.GetString("Follow")} {UserDetail.FansNum}{loader.GetString("Fan")}",
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                },

                                new AdaptiveText
                                {
                                    Text = $"{UserDetail.LoginTime}{loader.GetString("Active")}",
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                },

                                new AdaptiveText
                                {
                                    Text = UserDetail.Bio,
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                    HintWrap = true,
                                    HintMaxLines = 2
                                }
                            }
                        }
                    },

                    TileWide = new TileBinding
                    {
                        Content = new TileBindingContentAdaptive
                        {
                            BackgroundImage = new TileBackgroundImage
                            {
                                Source = UserDetail.Cover.Uri,
                                HintOverlay = 70
                            },

                            Children =
                            {
                                new AdaptiveGroup
                                {
                                    Children =
                                    {
                                        new AdaptiveSubgroup
                                        {
                                            HintWeight = 33,
                                            Children =
                                            {
                                                new AdaptiveImage
                                                {
                                                    Source = UserDetail.UserAvatar.Uri,
                                                    HintCrop = AdaptiveImageCrop.Circle
                                                }
                                            },
                                        },
                                        new AdaptiveSubgroup
                                        {
                                            Children =
                                            {
                                                new AdaptiveText
                                                {
                                                    Text = UserDetail.UserName,
                                                    HintStyle = AdaptiveTextStyle.Caption,
                                                },

                                                new AdaptiveText
                                                {
                                                    Text = $"{UserDetail.FollowNum}{loader.GetString("Follow")} {UserDetail.FansNum}{loader.GetString("Fan")}",
                                                    HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                                },

                                                new AdaptiveText
                                                {
                                                    Text = $"{UserDetail.LoginTime}{loader.GetString("Active")}",
                                                    HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                                },

                                                new AdaptiveText
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

                    TileLarge = new TileBinding
                    {
                        Content = new TileBindingContentAdaptive
                        {
                            BackgroundImage = new TileBackgroundImage
                            {
                                Source = UserDetail.Cover.Uri,
                                HintOverlay = 70
                            },

                            Children =
                            {
                                new AdaptiveGroup
                                {
                                    Children =
                                    {
                                        new AdaptiveSubgroup
                                        {
                                            HintWeight = 33,
                                            Children =
                                            {
                                                new AdaptiveImage
                                                {
                                                    Source = UserDetail.UserAvatar.Uri,
                                                    HintCrop = AdaptiveImageCrop.Circle
                                                }
                                            }
                                        },

                                        new AdaptiveSubgroup()
                                    }
                                },

                                new AdaptiveText
                                {
                                    Text = UserDetail.UserName,
                                    HintStyle = AdaptiveTextStyle.Caption,
                                },

                                new AdaptiveText
                                {
                                    Text = $"{UserDetail.FollowNum}{loader.GetString("Follow")} {UserDetail.FansNum}{loader.GetString("Fan")}",
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                },

                                new AdaptiveText
                                {
                                    Text = $"{UserDetail.LoginTime}{loader.GetString("Active")}",
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                },

                                new AdaptiveText
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
