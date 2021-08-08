using CoolapkUWP.Control.ViewModels;
using CoolapkUWP.Data;
using Microsoft.Toolkit.Uwp.Notifications;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.UI.Notifications;

namespace CoolapkUWP.Control
{
    internal class LiveTileControl
    {
        private static async Task<JsonObject> GetJson(string uri)
        {
            string result = await UIHelper.GetJson(uri, true);
            return JsonObject.Parse(result);
        }

        public static async Task GetData(string uri)
        {
            JsonObject token = await GetJson(uri);
            if (token.TryGetValue("data", out IJsonValue data))
            {
                int i = 0;
                foreach (JsonObject v in (JsonArray)data)
                {
                    if (i >= 5) { break; }
                    if (v.TryGetValue("entityType", out IJsonValue entityType) && (entityType.ToString() == "feed" || entityType.ToString() == "discovery"))
                    {
                        i++;
                        UpdateTitle(GetTitle(v));
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

        private static TileContent GetTitle(JsonObject token)
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
    }
}
