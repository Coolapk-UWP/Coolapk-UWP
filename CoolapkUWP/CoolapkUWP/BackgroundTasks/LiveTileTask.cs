using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Feeds;
using Microsoft.Toolkit.Uwp.Notifications;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.UI.Notifications;

namespace CoolapkUWP.BackgroundTasks
{
    public sealed class LiveTileTask : IBackgroundTask
    {
        public static LiveTileTask Instance = new LiveTileTask(false);

        public LiveTileTask(bool renew = true)
        {
            if (renew) { Instance = this; }
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
            try { await GetData(uri); } catch { }
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
                TileUpdateManager.CreateTileUpdaterForApplication().EnableNotificationQueue(true);
                TileNotification tileNotification = new TileNotification(tileContent.GetXml());
                TileUpdateManager.CreateTileUpdaterForApplication().Update(tileNotification);
            }
            catch { }
        }

        private TileContent GetFeedTitle(JObject token)
        {
            FeedModel FeedDetail = new FeedModel(token);
            string Message = FeedDetail.Message.CSStoString();
            return new TileContent
            {
                Visual = new TileVisual
                {
                    Branding = TileBranding.NameAndLogo,
                    DisplayName = FeedDetail.Info,
                    Arguments = FeedDetail.Url,

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
                                new AdaptiveText
                                {
                                    Text = FeedDetail.UserInfo.UserName,
                                    HintStyle = AdaptiveTextStyle.Base,
                                },

                                new AdaptiveText
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
    }
}
