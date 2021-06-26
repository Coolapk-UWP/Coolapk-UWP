using CoolapkUWP.Core.Helpers;
using CoolapkUWP.Helpers;
using CoolapkUWP.Models;
using Microsoft.Toolkit.Uwp.Notifications;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace CoolapkUWP.Pages.AppPages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class DevPage : Page
    {
        private FeedDetailModel FeedDetail { get; set; }
        private Models.Pages.FeedListPageModels.UserDetail UserDetail { get; set; }

        public DevPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            #region 测试
            //Uri uri = new Uri("https://qapi.ithome.com/api/content/getcontentdetail?id=5209");
            //(bool _, string result) = await DataHelper.GetHtmlAsync(uri, "XMLHttpRequest");
            //GetData(new Uri("http://api.coolapk.com/v6/user/followList?uid=536381&page=1"));
            //MultipartFormDataContent content = new MultipartFormDataContent();
            //content.Add(new StringContent("532743392602094571"), "dynamic_id");
            //(bool _, string _) = await DataHelper.PostHtmlAsync(new Uri("https://api.vc.bilibili.com/dynamic_svr/v1/dynamic_svr/get_dynamic_detail") , content);
            //UIHelper.StatusBar_ShowMessage(NetworkHelper.ExpandShortUrl(new Uri("https://b23.tv/M35xll")));
            //Models.Links.SourceFeedModel _ = new Models.Links.SourceFeedModel(JObject.Parse(result), Models.Links.LinkType.ITHome);
            //if (isSucceed)
            //{
            //    //main.Text = result;
            //    //var o = JObject.Parse(result);
            //    //webview.NavigateToString(o.TryGetValue("html", out JToken token) ? token.ToString() : "错误");
            //    //text.MessageText = o.TryGetValue("html", out JToken token) ? token.ToString() : "错误";
            //    //MarkdownText.Text = CSStoMarkDown(o.TryGetValue("html", out JToken token) ? token.ToString() : "错误");
            //    //title.Title = o.TryGetValue("title", out JToken Title) ? Title.ToString() : title.Title;
            //}
            //webview.NavigationCompleted += OnNavigationCompleted;
            //webview.NavigateToString("");
            //FeedDetail = await GetFeedDetailAsync("26933449");
            //content = new TileContent()
            //{
            //    Visual = new TileVisual()
            //    {
            //        Branding = TileBranding.NameAndLogo,
            //        DisplayName = FeedDetail.Info,
            //        TileMedium = new TileBinding()
            //        {
            //            Content = new TileBindingContentAdaptive()
            //            {
            //                Children =
            //                {
            //                    new AdaptiveText()
            //                    {
            //                        Text = FeedDetail.Username,
            //                        HintStyle = AdaptiveTextStyle.Base,
            //                    },

            //                    new AdaptiveText()
            //                    {
            //                        Text = FeedDetail.Message,
            //                        HintStyle = AdaptiveTextStyle.CaptionSubtle,
            //                        HintWrap = true
            //                    }
            //                }
            //            }
            //        },
            //    }
            //};
            #endregion
            //var notification = new TileNotification(content.GetXml());
            //TileUpdateManager.CreateTileUpdaterForApplication().Update(notification);
            #region 通知测试
            //MakeLikes(new Uri("https://api.coolapk.com/v6/user/feedList?uid=495798&page=1&isIncludeTop=1"));
            #endregion
        }

        protected static async Task<FeedDetailModel> GetFeedDetailAsync(string id)
        {
            (bool isSucceed, JToken result) = await DataHelper.GetDataAsync(UriHelper.GetUri(UriType.GetFeedDetail, id), true);
            if (!isSucceed) { return null; }

            JObject detail = (JObject)result;
            return detail != null ? new FeedDetailModel(detail) : null;
        }

        private async void OnNavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs e)
        {
            string userId = await webview.InvokeScriptAsync("eval", new[] { "return navigator.userAgent;" });
            main.Text = userId;
        }

        private static string CSStoMarkDown(string text)
        {
            Regex h1 = new Regex("<h1 style.*?>", RegexOptions.IgnoreCase);
            Regex h2 = new Regex("<h2 style.*?>", RegexOptions.IgnoreCase);
            Regex h3 = new Regex("<h3 style.*?>", RegexOptions.IgnoreCase);
            Regex div = new Regex("<div style.*?>", RegexOptions.IgnoreCase);
            Regex p = new Regex("<p style.*?>", RegexOptions.IgnoreCase);

            text = text.Replace("</h1>", "");
            text = text.Replace("</h2>", "");
            text = text.Replace("</h3>", "");
            text = text.Replace("</div>", "");
            text = text.Replace("</p>", "");

            text = h1.Replace(text, "#");
            text = h2.Replace(text, "##");
            text = h3.Replace(text, "###");
            text = text.Replace("<br>", "  \n");
            text = div.Replace(text, "");
            text = p.Replace(text, "");

            for (int i = 0; i < 20; i++) { text = text.Replace("(" + i.ToString() + ") ", " 1. "); }

            return text;
        }

        #region Task：任务

        private void GetDevMyList(string str)
        {
            //main.Text = str;
        }


        #endregion

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack) { Frame.GoBack(); }
        }

        private void TitleBar_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            Uri uri = new Uri(Uri.Text);
            (bool isSucceed, string result) = await DataHelper.GetHtmlAsync(uri, "XMLHttpRequest");
        }

        #region 磁铁测试
        private async Task<JObject> GetJson(Uri uri)
        {
            (bool isSucceed, string result) = await DataHelper.GetHtmlAsync(uri, "XMLHttpRequest");
            if (isSucceed) { return JObject.Parse(result); }
            else { return null; }
        }

        private async void GetData(Uri uri)
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

        private static TileContent GetUserTitle(JObject token)
        {
            UserModel UserDetail = new UserModel(token);
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
                                    Text = UserDetail.FollowNum + "关注" + UserDetail.FansNum + "粉丝"+ UserDetail.LoginTime+"活跃",
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
                                                    Text = UserDetail.FollowNum + "关注" + UserDetail.FansNum + "粉丝"+ UserDetail.LoginTime+"活跃",
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
                                    Text = UserDetail.FollowNum + "关注" + UserDetail.FansNum + "粉丝"+ UserDetail.LoginTime+"活跃",
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
        #endregion

        #region 通知测试
        public void SendUpdatableToastWithProgress()
        {
            // Define a tag (and optionally a group) to uniquely identify the notification, in order update the notification data later;
            string tag = "weekly-playlist";
            string group = "downloads";

            // Construct the toast content with data bound fields
            var content = new ToastContentBuilder()
                .AddText("一键点赞")
                .AddVisualChild(new AdaptiveProgressBar()
                {
                    Title = new BindableString("progressTitle"),
                    Value = new BindableProgressBarValue("progressValue"),
                    ValueStringOverride = new BindableString("progressValueString"),
                    Status = new BindableString("progressStatus")
                })
                .GetToastContent();

            // Generate the toast notification
            var toast = new ToastNotification(content.GetXml());

            // Assign the tag and group
            toast.Tag = tag;
            toast.Group = group;

            // Assign initial NotificationData values
            // Values must be of type string
            toast.Data = new NotificationData();
            toast.Data.Values["progressTitle"] = "正在准备点赞";
            toast.Data.Values["progressValue"] = "0";
            toast.Data.Values["progressValueString"] = "准备开始";
            toast.Data.Values["progressStatus"] = "加载中...";

            // Provide sequence number to prevent out-of-order updates, or assign 0 to indicate "always update"
            toast.Data.SequenceNumber = 1;

            // Show the toast notification to the user
            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }

        public void UpdateProgress(double num1,double num2, string id, string Title)
        {
            // Construct a NotificationData object;
            string tag = "weekly-playlist";
            string group = "downloads";

            // Create NotificationData and make sure the sequence number is incremented
            // since last update, or assign 0 for updating regardless of order
            var data = new NotificationData
            {
                SequenceNumber = 2
            };

            // Assign new values
            // Note that you only need to assign values that changed. In this example
            // we don't assign progressStatus since we don't need to change it
            data.Values["progressTitle"] = Title;
            data.Values["progressStatus"] = id;
            data.Values["progressValue"] = (num1 / num2).ToString();
            data.Values["progressValueString"] = num1 + "/" + num2 + "正在点赞";

            // Update the existing notification's data by using tag/group
            ToastNotificationManager.CreateToastNotifier().Update(data, tag, group);
        }

        private void MakeLikes(Uri uri)
        {
            SendUpdatableToastWithProgress();
            if (!string.IsNullOrEmpty(uri.ToString())) { GetJson2(uri); }
        }

        private async void GetJson2(Uri uri)
        {
            bool isSucceed;
            string result;
            (isSucceed, result) = await DataHelper.GetHtmlAsync(uri, "XMLHttpRequest");
            if (isSucceed && !string.IsNullOrEmpty(result))
            {
                JObject json = JObject.Parse(result);
                ReadJson(json);
            }
        }

        private void ReadJson(JObject token)
        {
            if (token.TryGetValue("data", out JToken data))
            {
                double Number = 0;
                double Number2 = (data as JArray).Count();
                string Title = "即将开始";
                string Title2 = "即将开始点赞";
                UpdateProgress(Number,Number2,Title,Title2);
                foreach (JObject v in (JArray)data)
                {
                    Number++;
                    if (v.TryGetValue("entityType", out JToken entityType))
                    {
                        if (entityType.ToString() == "feed" || entityType.ToString() == "discovery")
                        {
                            if (v.TryGetValue("id", out JToken id))
                            {
                                Title = id.ToString();
                                MakeLike(Title);
                            }
                            if (v.TryGetValue("message", out JToken message))
                            {
                                Title2 = message.ToString();
                            }
                            if (v.TryGetValue("userInfo", out JToken v2))
                            {
                                JObject userInfo = (JObject)v2;
                                if (userInfo.TryGetValue("username", out JToken username))
                                {
                                }
                            }
                            UpdateProgress(Number, Number2, Title, Title2);
                        }
                    }
                }
            }
        }

        private async void MakeLike(string id)
        {
            bool isSucceed;
            string result;
            (isSucceed, result) = await DataHelper.GetHtmlAsync(UriHelper.GetUri(UriType.OperateUnlike, string.Empty, id), "XMLHttpRequest");
        }
        #endregion
    }
}
