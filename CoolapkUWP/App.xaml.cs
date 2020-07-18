using CoolapkUWP.Helpers;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace CoolapkUWP
{
    /// <summary> 提供特定于应用程序的行为，以补充默认的应用程序类。 </summary>
    sealed partial class Application : Windows.UI.Xaml.Application
    {
        /// <summary> 初始化单一实例应用程序对象。这是执行的创作代码的第一行， 已执行，逻辑上等同于 main() 或 WinMain()。 </summary>
        public Application()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        /// <summary> 在应用程序由最终用户正常启动时进行调用。 将在启动应用程序以打开特定文件等情况下使用。 </summary>
        /// <param name="e"> 有关启动请求和过程的详细信息。 </param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            RegisterExceptionHandlingSynchronizationContext();
            this.UnhandledException += Application_UnhandledException;

            if (!(Window.Current.Content is Frame rootFrame))
            {
                rootFrame = new Frame();
                rootFrame.NavigationFailed += OnNavigationFailed;
                // 当导航堆栈尚未还原时，导航到第一页， 并通过将所需信息作为导航参数传入来配置 参数
                rootFrame.Navigate(typeof(Pages.ShellPage), e.Arguments);
                Window.Current.Content = rootFrame;
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

        private void Application_UnhandledException(object sender, Windows.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            if (!(!SettingsHelper.Get<bool>(SettingsHelper.ShowOtherException) || e.Exception is TaskCanceledException || e.Exception is OperationCanceledException))
            {
                var loader = ResourceLoader.GetForViewIndependentUse();
                UIHelper.ShowMessage($"{loader.GetString("ExceptionThrown")}\n{e.Exception.Message}\n{e.Exception.HResult}(0x{Convert.ToString(e.Exception.HResult, 16)})"
#if DEBUG
                    + $"\n{e.Exception.StackTrace}"
#endif
                );
                SettingsHelper.logManager.GetLogger("UnhandledException").Error($"\n{e.Exception.Message}\n{e.Exception.HResult}\n{e.Exception.StackTrace}");
            }
        }

        /// <summary> 在将要挂起应用程序执行时调用。 在不知道应用程序 无需知道应用程序会被终止还是会恢复， 并让内存内容保持不变。 </summary>
        /// <param name="sender"> 挂起的请求的源。 </param>
        /// <param name="e">      有关挂起请求的详细信息。 </param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
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
                var loader = ResourceLoader.GetForViewIndependentUse();
                if (e.Exception is System.Net.Http.HttpRequestException || e.Exception.HResult <= -2147012721 && e.Exception.HResult >= -2147012895)
                {
                    UIHelper.ShowMessage($"{loader.GetString("NetworkError")}(0x{Convert.ToString(e.Exception.HResult, 16)})");
                }
                else if (e.Exception is Core.Exceptions.CoolapkMessageException)
                {
                    UIHelper.ShowMessage(e.Exception.Message);
                }
                else if (e.Exception is Core.Exceptions.UserNameErrorException)
                {
                    UIHelper.ShowMessage(loader.GetString("UserNameError"));
                }
                else if (SettingsHelper.Get<bool>(SettingsHelper.ShowOtherException))
                {
                    UIHelper.ShowMessage($"{loader.GetString("ExceptionThrown")}\n{e.Exception.Message}\n{e.Exception.HResult}(0x{Convert.ToString(e.Exception.HResult, 16)})"
#if DEBUG
                        + $"\n{e.Exception.StackTrace}"
#endif
                    );
                    SettingsHelper.logManager.GetLogger("UnhandledException").Error($"\n{e.Exception.Message}\n{e.Exception.HResult}\n{e.Exception.StackTrace}");
                }
            }
        }
    }
}