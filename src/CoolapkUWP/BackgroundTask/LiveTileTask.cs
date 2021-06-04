using CoolapkUWP.Helpers;
using CoolapkUWP.Models;
using Microsoft.Toolkit.Uwp.Notifications;
using Newtonsoft.Json.Linq;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.UI.Notifications;

namespace CoolapkUWP.BackgroundTask
{
    public sealed class LiveTileTask : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            BackgroundTaskDeferral deferral = taskInstance.GetDeferral();

            await GetData(new Uri("http://api.coolapk.com/v6/page/dataList?url=V9_HOME_TAB_FOLLOW&type=circle&page=1"));

            deferral.Complete();
        }

        private static async Task<JObject> GetJson(Uri uri)
        {
            (bool isSucceed, string result) = await DataHelper.GetHtmlAsync(uri, "XMLHttpRequest");
            if (isSucceed) { return JObject.Parse(result); }
            else { return null; }
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
                    if (v.TryGetValue("entityType", out JToken entityType) && (entityType.ToString() == "feed" || entityType.ToString() == "discovery"))
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

        private static TileContent GetTitle(JObject token)
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
    }
}
