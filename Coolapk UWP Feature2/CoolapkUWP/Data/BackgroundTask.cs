using CoolapkUWP.Control;
using Windows.ApplicationModel.Background;

namespace CoolapkUWP.Data
{
    public class BackgroundTask : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            BackgroundTaskDeferral deferral = taskInstance.GetDeferral();
            await new NotificationsNum().RefreshNotificationsNum(true);
            deferral.Complete();
        }
    }
}
