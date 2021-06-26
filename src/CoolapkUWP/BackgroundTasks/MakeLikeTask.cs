using CoolapkUWP.Core.Helpers;
using CoolapkUWP.Helpers;
using Microsoft.Toolkit.Uwp.Notifications;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
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

        public void MakeLikes(Uri uri)
        {
            SendUpdatableToastWithProgress();
            if (!string.IsNullOrEmpty(uri.ToString())) { GetJson(uri); }
        }

        private async void GetJson(Uri uri)
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

        private async void ReadJson(JObject token)
        {
            if (token.TryGetValue("data", out JToken data))
            {
                double FinishNumber = 0, AllNumber = (data as JArray).Count();
                string Status = "即将开始...", Title = "即将开始点赞";
                UpdateProgress(FinishNumber, AllNumber, Status, Title);
                foreach (JObject v in (JArray)data)
                {
                    FinishNumber++;
                    if (v.TryGetValue("entityType", out JToken entityType))
                    {
                        if (entityType.ToString() == "feed" || entityType.ToString() == "discovery")
                        {
                            Status = "正在点赞...";
                            UpdateProgress(FinishNumber, AllNumber, Status, Title);
                            if (v.TryGetValue("message", out JToken message))
                            {
                                Title = message.ToString();
                            }
                            Status = "点赞完成...";
                            UpdateProgress(FinishNumber, AllNumber, Status, Title);
                        }
                    }
                }
            }
        }

        private static async Task Operatelike(string id)
        {
            (_, _) = await DataHelper.GetHtmlAsync(UriHelper.GetUri(UriType.OperateUnlike, string.Empty, id), "XMLHttpRequest");
        }

        public static void SendUpdatableToastWithProgress()
        {
            // Define a tag (and optionally a group) to uniquely identify the notification, in order update the notification data later;
            string tag = "weekly-playlist";
            string group = "downloads";

            // Construct the toast content with data bound fields
            ToastContent content = new ToastContentBuilder()
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

        public static void UpdateProgress(double FinishNumber, double AllNumber, string Status, string Title)
        {
            // Construct a NotificationData object;
            string tag = "weekly-playlist";
            string group = "downloads";

            // Create NotificationData and make sure the sequence number is incremented
            // since last update, or assign 0 for updating regardless of order
            NotificationData data = new NotificationData
            {
                SequenceNumber = 2
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
