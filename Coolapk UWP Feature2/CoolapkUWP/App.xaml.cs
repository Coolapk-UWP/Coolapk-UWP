using CoolapkUWP.Control;
using CoolapkUWP.Data;
using CoolapkUWP.Pages;
using CoolapkUWP.Pages.SettingPages;
using Microsoft.Toolkit.Uwp.Connectivity;
using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Background;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace CoolapkUWP
{
    /// <summary>
    /// 提供特定于应用程序的行为，以补充默认的应用程序类。
    /// </summary>
    public sealed partial class Application : Windows.UI.Xaml.Application
    {
        /// <summary>
        /// 初始化单一实例应用程序对象。这是执行的创作代码的第一行，
        /// 已执行，逻辑上等同于 main() 或 WinMain()。
        /// </summary>
        public Application()
        {
            InitializeComponent();
            this.Suspending += OnSuspending;
        }

        protected override void OnActivated(IActivatedEventArgs e)
        {
            if (!(Window.Current.Content is Frame rootFrame))
            {
                rootFrame = new Frame();
                rootFrame.NavigationFailed += OnNavigationFailed;
                // 当导航堆栈尚未还原时，导航到第一页，
                // 并通过将所需信息作为导航参数传入来配置
                // 参数
                ApplicationViewTitleBar view = ApplicationView.GetForCurrentView().TitleBar;
                view.ButtonBackgroundColor = view.InactiveBackgroundColor = view.ButtonInactiveBackgroundColor = Colors.Transparent;
                _ = rootFrame.Navigate(typeof(MainPage));
                Window.Current.Content = rootFrame;
            }
            Window.Current.Activate();
            if (e.Kind == ActivationKind.Protocol)
            {
                ProtocolActivatedEventArgs protocolArgs = (ProtocolActivatedEventArgs)e;
                switch (protocolArgs.Uri.Host)
                {
                    case "www.coolapk.com":
                        UIHelper.OpenLink(protocolArgs.Uri.AbsolutePath);
                        break;
                    case "coolapk.com":
                        UIHelper.OpenLink(protocolArgs.Uri.AbsolutePath);
                        break;
                    case "www.coolmarket.com":
                        UIHelper.OpenLink(protocolArgs.Uri.AbsolutePath);
                        break;
                    case "coolmarket.com":
                        UIHelper.OpenLink(protocolArgs.Uri.AbsolutePath);
                        break;
                    case "http":
                        UIHelper.OpenLink(protocolArgs.Uri.Host + ":" + protocolArgs.Uri.AbsolutePath);
                        break;
                    case "https":
                        UIHelper.OpenLink(protocolArgs.Uri.Host + ":" + protocolArgs.Uri.AbsolutePath);
                        break;
                    case "settings":
                        UIHelper.Navigate(typeof(SettingPage), false);
                        break;
                    case "flags":
                        UIHelper.Navigate(typeof(TestPage));
                        break;
                    default:
                        UIHelper.OpenLink("/" + protocolArgs.Uri.Host + protocolArgs.Uri.AbsolutePath);
                        break;
                }
            }
        }

        /// <summary>
        /// 在应用程序由最终用户正常启动时进行调用。
        /// 将在启动应用程序以打开特定文件等情况下使用。
        /// </summary>
        /// <param name="e">有关启动请求和过程的详细信息。</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            RegisterExceptionHandlingSynchronizationContext();
            UnhandledException += Application_UnhandledException;
            RegisterBackgroundTask();

            if (!(Window.Current.Content is Frame rootFrame))
            {
                rootFrame = new Frame();
                rootFrame.NavigationFailed += OnNavigationFailed;
                // 当导航堆栈尚未还原时，导航到第一页，
                // 并通过将所需信息作为导航参数传入来配置
                // 参数
                ApplicationViewTitleBar view = ApplicationView.GetForCurrentView().TitleBar;
                view.ButtonBackgroundColor = view.InactiveBackgroundColor = view.ButtonInactiveBackgroundColor = Colors.Transparent;
                _ = rootFrame.Navigate(typeof(MainPage), e.Arguments);
                Window.Current.Content = rootFrame;
                if (e.Arguments != null)
                {
                    switch (e.Arguments)
                    {
                        case "search":
                            UIHelper.Navigate(typeof(SearchPage));
                            break;
                        case "settings":
                            UIHelper.Navigate(typeof(SettingPage));
                            break;
                        case "test":
                            UIHelper.Navigate(typeof(TestPage));
                            break;
                        default:
                            UIHelper.OpenLink(e.Arguments);
                            break;
                    }
                }
                else if (e.TileActivatedInfo != null)
                {
                    if (e.TileActivatedInfo.RecentlyShownNotifications.Count > 0)
                    {
                        // Get arguments from the notifications that were recently displayed
                        string[] allArgs = e.TileActivatedInfo.RecentlyShownNotifications
                        .Select(i => i.Arguments)
                        .ToArray();
                        UIHelper.OpenLink(allArgs[0]);
                    }
                }
            }
            // 确保当前窗口处于活动状态
            Window.Current.Activate();
        }

        /// <summary>
        /// 导航到特定页失败时调用
        /// </summary>
        ///<param name="sender">导航失败的框架</param>
        ///<param name="e">有关导航失败的详细信息</param>
        private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        private void Application_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            if (!(e.Exception is TaskCanceledException) && !(e.Exception is OperationCanceledException))
            {
                if (Window.Current.Content != null)
                {
                    UIHelper.ShowMessage($"{e.Exception.Message}\n{e.Exception.StackTrace}");
                    UIHelper.HideProgressBar();
                }
            }
        }

        /// <summary>
        /// 在将要挂起应用程序执行时调用。  在不知道应用程序
        /// 无需知道应用程序会被终止还是会恢复，
        /// 并让内存内容保持不变。
        /// </summary>
        /// <param name="sender">挂起的请求的源。</param>
        /// <param name="e">有关挂起请求的详细信息。</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            SuspendingDeferral deferral = e.SuspendingOperation.GetDeferral();
            //TODO: 保存应用程序状态并停止任何后台活动
            deferral.Complete();
        }

        private void RegisterExceptionHandlingSynchronizationContext()
            => ExceptionHandlingSynchronizationContext.Register().UnhandledException += SynchronizationContext_UnhandledException;

        private void SynchronizationContext_UnhandledException(object sender, AysncUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            if (!(e.Exception is TaskCanceledException) && !(e.Exception is OperationCanceledException))
            {
                if (Window.Current.Content != null)
                {
                    UIHelper.ShowMessage($"{e.Exception.Message}\n{e.Exception.StackTrace}");
                    UIHelper.HideProgressBar();
                }
            }
        }

        private static async void RegisterBackgroundTask()
        {
            #region LiveTileTask
            if (SettingsHelper.WindowsVersion < 22000)
            {
                const string LiveTileTask = "LiveTileTask";

                // Check for background access (optional)
                _ = await BackgroundExecutionManager.RequestAccessAsync();

                // Register (Single Process)
                _ = BackgroundTaskHelper.Register(LiveTileTask, new TimeTrigger(15, false), true);
            }
            #endregion
        }

        protected override void OnBackgroundActivated(BackgroundActivatedEventArgs args)
        {
            base.OnBackgroundActivated(args);

            BackgroundTaskDeferral deferral = args.TaskInstance.GetDeferral();

            switch (args.TaskInstance.Task.Name)
            {
                case "LiveTileTask":
                    if (NetworkHelper.Instance.ConnectionInformation.IsInternetAvailable)
                    {
                        new LiveTileControl().Run(args.TaskInstance);
                    }
                    break;

                default:
                    deferral.Complete();
                    break;
            }

            deferral.Complete();
        }
    }
}
