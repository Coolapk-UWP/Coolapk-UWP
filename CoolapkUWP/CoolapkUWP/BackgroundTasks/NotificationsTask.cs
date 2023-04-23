using CoolapkUWP.Helpers;
using Newtonsoft.Json.Linq;
using Windows.ApplicationModel.Background;

namespace CoolapkUWP.BackgroundTasks
{
    public sealed class NotificationsTask : IBackgroundTask
    {
        public static NotificationsTask Instance = new NotificationsTask();

        public NotificationsTask()
        {
            Instance = Instance ?? this;
        }

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            BackgroundTaskDeferral deferral = taskInstance.GetDeferral();
            UpdateNotifications();
            deferral.Complete();
        }

        private async void UpdateNotifications()
        {
            (bool isSucceed, JToken result) = await RequestHelper.GetDataAsync(UriHelper.GetUri(UriType.GetNotificationNumbers), true);
            if (!isSucceed) { return; }
            JObject token = (JObject)result;
            if (token != null)
            {
                if (token.TryGetValue("badge", out JToken badge) && badge != null)
                {
                    UIHelper.SetBadgeNumber(badge.ToString());
                }
            }
        }
    }
}
