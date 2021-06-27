using CoolapkUWP.Core.Helpers;
using CoolapkUWP.Helpers;
using Microsoft.Toolkit.Uwp.Notifications;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.UI.Notifications;

namespace CoolapkUWP.BackgroundTasks
{
    public sealed class MakeLikeTask : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            BackgroundTaskDeferral deferral = taskInstance.GetDeferral();

            deferral.Complete();
        }

        public static void MakeLikes(string uri)
        {
            _ = ChangePage(uri);
        }

        private static async Task ChangePage(string uri)
        {
            Regex page = new Regex(@"page=(\d*)", RegexOptions.IgnoreCase);
            Regex id = new Regex(@"[?|&](id|uid)=(\d*)", RegexOptions.IgnoreCase);
            if (page.Match(uri).Success)
            {
                int pagenum = 0;
                while (true)
                {
                    bool havemore;
                    pagenum++;
                    SendUpdatableToastWithProgress(id.Match(uri).Groups[2].Value, pagenum);
                    uri = page.Replace(uri, "page=" + pagenum.ToString());
                    havemore = await GetJson(id.Match(uri).Groups[2].Value, new Uri(uri));
                    await Task.Delay(500);
                    if (!havemore) { break; }
                }
            }
            else
            {
                SendUpdatableToastWithProgress(id.Match(uri).Groups[2].Value);
                if (uri != null) { await GetJson(id.Match(uri).Groups[2].Value, new Uri(uri)); }
            }
        }

        private static async Task<bool> GetJson(string id, Uri uri)
        {
            bool isSucceed;
            string result;
            (isSucceed, result) = await DataHelper.GetHtmlAsync(uri, "XMLHttpRequest");
            if (isSucceed && !string.IsNullOrEmpty(result))
            {
                JObject json = JObject.Parse(result);
                return await ReadJson(id, json);
            }
            else { return false; }
        }

        private static async Task<bool> ReadJson(string uid, JObject token)
        {
            if (token != null)
            {
                if (token.TryGetValue("data", out JToken data))
                {
                    double FinishNumber = 0, AllNumber = (data as JArray).Count();
                    string Status = "即将开始...", Title = "即将开始点赞";
                    UpdateProgress(uid, FinishNumber, AllNumber, Status, Title);
                    foreach (JObject v in (JArray)data)
                    {
                        FinishNumber++;
                        if (v.TryGetValue("entityType", out JToken entityType))
                        {
                            if (entityType.ToString() == "feed" || entityType.ToString() == "discovery")
                            {
                                Status = FinishNumber == AllNumber ? "点赞完成" : "正在点赞...";
                                if (v.TryGetValue("message", out JToken message))
                                {
                                    Title = Massage_Ex(message.ToString());
                                }
                                UpdateProgress(uid, FinishNumber, AllNumber, Status, Title);

                                if (v.TryGetValue("id", out JToken id))
                                {
                                    if (v.TryGetValue("userAction", out JToken v1))
                                    {
                                        JObject userAction = v1 as JObject;
                                        if (userAction.TryGetValue("like", out JToken like) && like.ToString() != "1")
                                        {
                                            await Operatelike(id.ToString());
                                            await Task.Delay(500);
                                            Status = FinishNumber == AllNumber ? "点赞完成" : "点赞好了...";
                                        }
                                        else
                                        {
                                            await Task.Delay(500);
                                            Status = FinishNumber == AllNumber ? "点赞完成" : "点赞过了...";
                                        }
                                    }
                                }
                                UpdateProgress(uid, FinishNumber, AllNumber, Status, Title);
                            }
                        }
                    }
                }
                return true;
            }
            else { return false; }
        }

        private static string Massage_Ex(string str)
        {
            Regex r = new Regex("<a.*?>", RegexOptions.IgnoreCase);
            Regex r1 = new Regex("<a.*?/>", RegexOptions.IgnoreCase);
            Regex r2 = new Regex("</a.*?>", RegexOptions.IgnoreCase);
            str = r.Replace(str, "");
            str = r1.Replace(str, "");
            str = r2.Replace(str, "");
            return str;
        }

        private static async Task Operatelike(string id)
        {
            (_, _) = await DataHelper.GetHtmlAsync(UriHelper.GetUri(UriType.OperateLike, string.Empty, id), "XMLHttpRequest");
        }

        public static void SendUpdatableToastWithProgress(string id, int page = 0)
        {
            // Define a tag (and optionally a group) to uniquely identify the notification, in order update the notification data later;
            string tag = id;
            string group = "makelike";

            // Construct the toast content with data bound fields
            ToastContent content = new ToastContentBuilder()
                .AddText("酷安一键点赞")
                .AddText(page == 0 ? "正在进行一键点赞" : "正在点赞第" + page.ToString() + "页")
                .AddVisualChild(new AdaptiveProgressBar()
                {
                    Title = new BindableString("progressTitle"),
                    Value = new BindableProgressBarValue("progressValue"),
                    ValueStringOverride = new BindableString("progressValueString"),
                    Status = new BindableString("progressStatus")
                })
                .GetToastContent();

            // Generate the toast notification
            ToastNotification toast = new ToastNotification(content.GetXml());

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

        public static void UpdateProgress(string id, double FinishNumber, double AllNumber, string Status, string Title)
        {
            // Construct a NotificationData object;
            string tag = id;
            string group = "makelike";

            // Create NotificationData and make sure the sequence number is incremented
            // since last update, or assign 0 for updating regardless of order
            NotificationData data = new NotificationData
            {
                SequenceNumber = 0
            };

            // Assign new values
            // Note that you only need to assign values that changed. In this example
            // we don't assign progressStatus since we don't need to change it
            data.Values["progressTitle"] = Title;
            data.Values["progressValue"] = (FinishNumber / AllNumber).ToString();
            data.Values["progressValueString"] = FinishNumber + "/" + AllNumber;
            data.Values["progressStatus"] = Status;

            // Update the existing notification's data by using tag/group
            _ = ToastNotificationManager.CreateToastNotifier().Update(data, tag, group);
        }
    }
}
