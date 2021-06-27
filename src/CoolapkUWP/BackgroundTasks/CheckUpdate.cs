using CoolapkUWP.Helpers;
using Microsoft.Toolkit.Uwp.Notifications;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Background;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace CoolapkUWP.BackgroundTasks
{
    public sealed class CheckUpdate : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            BackgroundTaskDeferral deferral = taskInstance.GetDeferral();

            await CheckUpdateAsync(false, true);

            deferral.Complete();
        }

        public static async Task CheckUpdateAsync(bool showmassage = true, bool showtoast = false)
        {
            Windows.ApplicationModel.Resources.ResourceLoader loader = Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse();
            try
            {
                bool isSucceed;
                string result;
                try { (isSucceed, result) = await DataHelper.GetHtmlAsync(new Uri("https://api.github.com/repos/Tangent-90/Coolapk-UWP/releases/latest"), null); }
                catch { (isSucceed, result) = await DataHelper.GetHtmlAsync(new Uri("https://v2.kkpp.cc/repos/Tangent-90/Coolapk-UWP/releases/latest"), null); }
                if (!isSucceed) { return; }
                JObject keys = JObject.Parse(result);
                string[] ver = keys.Value<string>("tag_name").Replace("v", string.Empty).Split('.');
                if (ushort.Parse(ver[0]) > Package.Current.Id.Version.Major ||
                   (ushort.Parse(ver[0]) == Package.Current.Id.Version.Major && ushort.Parse(ver[1]) > Package.Current.Id.Version.Minor) ||
                   (ushort.Parse(ver[0]) == Package.Current.Id.Version.Major && ushort.Parse(ver[1]) == Package.Current.Id.Version.Minor && ushort.Parse(ver[2]) > Package.Current.Id.Version.Build))
                {
                    if (showtoast)
                    {
                        string tag = "update";
                        string group = "downloads";

                        ToastContent content = new ToastContentBuilder()
                            .AddText(loader.GetString("HasUpdateTitle"))
                            .AddText(string.Format(
                                loader.GetString("HasUpdate"),
                                Package.Current.Id.Version.Major,
                                Package.Current.Id.Version.Minor,
                                Package.Current.Id.Version.Build,
                                keys.Value<string>("tag_name")))
                            .GetToastContent();

                        ToastNotification toast = new ToastNotification(content.GetXml());

                        toast.Tag = tag;
                        toast.Group = group;

                        ToastNotificationManager.CreateToastNotifier().Show(toast);
                    }
                    else
                    {
                        Grid grid = new Grid();
                        TextBlock textBlock = new TextBlock
                        {
                            Text = string.Format(
                                loader.GetString("HasUpdate"),
                                Package.Current.Id.Version.Major,
                                Package.Current.Id.Version.Minor,
                                Package.Current.Id.Version.Build,
                                keys.Value<string>("tag_name")),
                            HorizontalAlignment = HorizontalAlignment.Left,
                            VerticalAlignment = VerticalAlignment.Center
                        };
                        Button button = new Button
                        {
                            Content = loader.GetString("GotoGithub"),
                            HorizontalAlignment = HorizontalAlignment.Right,
                            VerticalAlignment = VerticalAlignment.Center
                        };
                        button.Click += async (_, __) =>
                        {
                            _ = await Windows.System.Launcher.LaunchUriAsync(new Uri(keys.Value<string>("html_url")));
                        };
                        grid.Children.Add(textBlock);
                        grid.Children.Add(button);
                        UIHelper.InAppNotification.Show(grid, 6000);
                    }
                }
                else if (showmassage) { UIHelper.StatusBar_ShowMessage(loader.GetString("NoUpdate")); }
            }
            catch (HttpRequestException) { UIHelper.StatusBar_ShowMessage(loader.GetString("NetworkError")); }
        }
    }
}
