﻿using Microsoft.Toolkit.Uwp.Connectivity;
using Windows.ApplicationModel.Background;

namespace CoolapkUWP.Data
{
    public class BackgroundTask : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            BackgroundTaskDeferral deferral = taskInstance.GetDeferral();
            if (NetworkHelper.Instance.ConnectionInformation.IsInternetAvailable)
            {
                await new NotificationsNum().RefreshNotificationsNum(true);
            }
            deferral.Complete();
        }
    }
}
