using CoolapkUWP.Helpers;
using CoolapkUWP.Pages.FeedPages;
using CoolapkUWP.Pages.SettingPages;
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
        public Application()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        protected override async void OnActivated(IActivatedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame == null)
            {
                rootFrame = new Frame();
                Window.Current.Content = rootFrame;
            }
            rootFrame.Navigate(typeof(Pages.ShellPage));
            Window.Current.Activate();
            if (e.Kind == ActivationKind.Protocol)
            {
                var protocolArgs = (ProtocolActivatedEventArgs)e;
                switch (protocolArgs.Uri.Host)
                {
                    case "www.coolapk.com":
                        UIHelper.OpenLinkAsync(protocolArgs.Uri.AbsolutePath);
                        break;
                    case "coolapk.com":
                        UIHelper.OpenLinkAsync(protocolArgs.Uri.AbsolutePath);
                        break;
                    case "www.coolmarket.com":
                        UIHelper.OpenLinkAsync(protocolArgs.Uri.AbsolutePath);
                        break;
                    case "coolmarket.com":
                        UIHelper.OpenLinkAsync(protocolArgs.Uri.AbsolutePath);
                        break;
                    case "settings":
                        UIHelper.NavigateInSplitPane(typeof(SettingPage));
                        break;
                    case "flags":
                        UIHelper.NavigateInSplitPane(typeof(TestPage));
                        break;
                    case "history":
                        UIHelper.NavigateInSplitPane(typeof(HistoryPage), new ViewModels.HistoryPage.ViewModel("浏览历史"));
                        break;
                    default:
                        UIHelper.OpenLinkAsync("/" + protocolArgs.Uri.Host + protocolArgs.Uri.AbsolutePath);
                        break;
                }
            }
        }

        /// <summary> 在应用程序由最终用户正常启动时进行调用。将在启动应用程序以打开特定文件等情况下使用。</summary>
        /// <param name="e"> 有关启动请求和过程的详细信息。 </param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            RegisterExceptionHandlingSynchronizationContext();
            this.UnhandledException += Application_UnhandledException;

            if (!(Window.Current.Content is Frame rootFrame))
            {
                rootFrame = new Frame();
                rootFrame.NavigationFailed += OnNavigationFailed;
                // 当导航堆栈尚未还原时，导航到第一页，并通过将所需信息作为导航参数传入来配置参数
                rootFrame.Navigate(typeof(Pages.ShellPage), e.Arguments);
                Window.Current.Content = rootFrame;
                Core.Helpers.Utils.NeedShowInAppMessageEvent += (_, arg) =>
                {
                    switch (arg.Item1)
                    {
                        case Core.Helpers.MessageType.Message:
                            UIHelper.ShowMessage(arg.Item2);
                            break;
                        default:
                            //UIHelper.ShowMessage(UIHelper.ConvertMessageTypeToMessage(arg.Item1));
                            break;
                    }
                };
            }

            Window.Current.Activate();
        }

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

        /// <summary> 在将要挂起应用程序执行时调用。在不知道应用程序无需知道应用程序会被终止还是会恢复，并让内存内容保持不变。</summary>
        /// <param name="sender"> 挂起的请求的源。 </param>
        /// <param name="e"> 有关挂起请求的详细信息。 </param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: 保存应用程序状态并停止任何后台活动
            deferral.Complete();
        }

        private void RegisterExceptionHandlingSynchronizationContext()
        {
            ExceptionHandlingSynchronizationContext.Register().UnhandledException += SynchronizationContext_UnhandledException;
        }

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