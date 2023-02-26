using CoolapkUWP.Models.Images;
using CoolapkUWP.Pages;
using CoolapkUWP.Pages.BrowserPages;
using CoolapkUWP.Pages.FeedPages;
using CoolapkUWP.ViewModels.BrowserPages;
using CoolapkUWP.ViewModels.FeedPages;
using Microsoft.Toolkit.Uwp.UI;
using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Resources;
using Windows.Data.Xml.Dom;
using Windows.Foundation.Metadata;
using Windows.UI.Core;
using Windows.UI.Notifications;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using System.Text.RegularExpressions;
using System.Linq;

namespace CoolapkUWP.Helpers
{
    internal static partial class UIHelper
    {
        public const int Duration = 3000;
        public static bool IsShowingProgressBar, IsShowingMessage;
        public static bool HasStatusBar => ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar");
        private static readonly ObservableCollection<string> MessageList = new ObservableCollection<string>();

        private static CoreDispatcher shellDispatcher;
        public static CoreDispatcher ShellDispatcher
        {
            get => shellDispatcher;
            set
            {
                if (shellDispatcher != value)
                {
                    shellDispatcher = value;
                }
            }
        }

        public static async void ShowProgressBar()
        {
            IsShowingProgressBar = true;
            if (HasStatusBar)
            {
                MainPage?.HideProgressBar();
                StatusBar.GetForCurrentView().ProgressIndicator.ProgressValue = null;
                await StatusBar.GetForCurrentView().ProgressIndicator.ShowAsync();
            }
            else
            {
                MainPage?.ShowProgressBar();
            }
        }

        public static async void ShowProgressBar(double value = 0)
        {
            IsShowingProgressBar = true;
            if (HasStatusBar)
            {
                MainPage?.HideProgressBar();
                StatusBar.GetForCurrentView().ProgressIndicator.ProgressValue = value * 0.01;
                await StatusBar.GetForCurrentView().ProgressIndicator.ShowAsync();
            }
            else
            {
                MainPage?.ShowProgressBar(value);
            }
        }

        public static async void PausedProgressBar()
        {
            IsShowingProgressBar = true;
            if (HasStatusBar)
            {
                await StatusBar.GetForCurrentView().ProgressIndicator.HideAsync();
            }
            MainPage?.PausedProgressBar();
        }

        public static async void ErrorProgressBar()
        {
            IsShowingProgressBar = true;
            if (HasStatusBar)
            {
                await StatusBar.GetForCurrentView().ProgressIndicator.HideAsync();
            }
            MainPage?.ErrorProgressBar();
        }

        public static async void HideProgressBar()
        {
            IsShowingProgressBar = false;
            if (HasStatusBar)
            {
                await StatusBar.GetForCurrentView().ProgressIndicator.HideAsync();
            }
            MainPage?.HideProgressBar();
        }

        public static async void ShowMessage(string message)
        {
            MessageList.Add(message);
            if (!IsShowingMessage)
            {
                IsShowingMessage = true;
                while (MessageList.Any())
                {
                    if (HasStatusBar)
                    {
                        StatusBar statusBar = StatusBar.GetForCurrentView();
                        if (!string.IsNullOrEmpty(MessageList[0]))
                        {
                            statusBar.ProgressIndicator.Text = $"[{MessageList.Count}] {MessageList[0].Replace("\n", " ")}";
                            statusBar.ProgressIndicator.ProgressValue = IsShowingProgressBar ? null : (double?)0;
                            await statusBar.ProgressIndicator.ShowAsync();
                            await Task.Delay(Duration);
                        }
                        MessageList.RemoveAt(0);
                        if (MessageList.Count == 0 && !IsShowingProgressBar) { await statusBar.ProgressIndicator.HideAsync(); }
                        statusBar.ProgressIndicator.Text = string.Empty;
                    }
                    else if (MainPage != null)
                    {
                        if (!string.IsNullOrEmpty(MessageList[0]))
                        {
                            string messages = $"[{MessageList.Count}] {MessageList[0].Replace("\n", " ")}";
                            MainPage.ShowMessage(messages);
                            await Task.Delay(Duration);
                        }
                        MessageList.RemoveAt(0);
                        if (MessageList.Count == 0)
                        {
                            MainPage.ShowMessage();
                        }
                    }
                }
                IsShowingMessage = false;
            }
        }

        public static void ShowInAppMessage(MessageType type, string message = null)
        {
            switch (type)
            {
                case MessageType.Message:
                    ShowMessage(message);
                    break;
                default:
                    ShowMessage(type.ConvertMessageTypeToMessage());
                    break;
            }
        }

        public static void ShowHttpExceptionMessage(HttpRequestException e)
        {
            if (e.Message.IndexOfAny(new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' }) != -1)
            { ShowInAppMessage(MessageType.Message, $"服务器错误： {e.Message.Replace("Response status code does not indicate success: ", string.Empty)}"); }
            else if (e.Message == "An error occurred while sending the request.") { ShowInAppMessage(MessageType.Message, "无法连接网络。"); }
            else { ShowInAppMessage(MessageType.Message, $"请检查网络连接。 {e.Message}"); }
        }

        public static bool IsOriginSource(object source, object originalSource)
        {
            bool r = false;
            DependencyObject DependencyObject = originalSource as DependencyObject;
            if (DependencyObject.FindAscendant<Button>() == null && DependencyObject.FindAscendant<AppBarButton>() == null && originalSource.GetType() != typeof(Button) && originalSource.GetType() != typeof(AppBarButton) && originalSource.GetType() != typeof(RichEditBox))
            {
                if (source is FrameworkElement FrameworkElement)
                {
                    r = source == DependencyObject.FindAscendant(FrameworkElement.Name);
                }
            }
            return source == originalSource || r;
        }

        public static string ConvertMessageTypeToMessage(this MessageType type)
        {
            switch (type)
            {
                case MessageType.NoMore:
                    return ResourceLoader.GetForViewIndependentUse("NotificationsPage").GetString("NoMore");

                case MessageType.NoMoreShare:
                    return ResourceLoader.GetForViewIndependentUse("NotificationsPage").GetString("NoMoreShare");

                case MessageType.NoMoreReply:
                    return ResourceLoader.GetForViewIndependentUse("NotificationsPage").GetString("NoMoreReply");

                case MessageType.NoMoreHotReply:
                    return ResourceLoader.GetForViewIndependentUse("NotificationsPage").GetString("NoMoreHotReply");

                case MessageType.NoMoreLikeUser:
                    return ResourceLoader.GetForViewIndependentUse("NotificationsPage").GetString("NoMoreLikeUser");

                default: return string.Empty;
            }
        }

        public static void SetBadgeNumber(string badgeGlyphValue)
        {
            // Get the blank badge XML payload for a badge number
            XmlDocument badgeXml =
                BadgeUpdateManager.GetTemplateContent(BadgeTemplateType.BadgeNumber);
            // Set the value of the badge in the XML to our number
            XmlElement badgeElement = badgeXml.SelectSingleNode("/badge") as XmlElement;
            badgeElement.SetAttribute("value", badgeGlyphValue);
            // Create the badge notification
            BadgeNotification badge = new BadgeNotification(badgeXml);
            // Create the badge updater for the application
            BadgeUpdater badgeUpdater =
                BadgeUpdateManager.CreateBadgeUpdaterForApplication();
            // And update the badge
            badgeUpdater.Update(badge);
        }

        public static string ExceptionToMessage(this Exception ex)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append('\n');
            if (!string.IsNullOrWhiteSpace(ex.Message)) { builder.AppendLine($"Message: {ex.Message}"); }
            builder.AppendLine($"HResult: {ex.HResult} (0x{Convert.ToString(ex.HResult, 16)})");
            if (!string.IsNullOrWhiteSpace(ex.StackTrace)) { builder.AppendLine(ex.StackTrace); }
            if (!string.IsNullOrWhiteSpace(ex.HelpLink)) { builder.Append($"HelperLink: {ex.HelpLink}"); }
            return builder.ToString();
        }

        public static TResult AwaitByTaskCompleteSource<TResult>(Func<Task<TResult>> function)
        {
            TaskCompletionSource<TResult> taskCompletionSource = new TaskCompletionSource<TResult>();
            Task<TResult> task = taskCompletionSource.Task;
            _ = Task.Run(async () =>
            {
                try
                {
                    TResult result = await function.Invoke().ConfigureAwait(false);
                    taskCompletionSource.SetResult(result);
                }
                catch (Exception e)
                {
                    taskCompletionSource.SetException(e);
                }
            });
            TResult taskResult = task.Result;
            return taskResult;
        }
    }

    public enum NavigationThemeTransition
    {
        Default,
        Entrance,
        DrillIn,
        Suppress
    }

    internal static partial class UIHelper
    {
        public static MainPage MainPage;

        public static void Navigate(Type pageType, object e = null, NavigationThemeTransition Type = NavigationThemeTransition.Default)
        {
            switch (Type)
            {
                case NavigationThemeTransition.DrillIn:
                    _ = (MainPage?.NavigationViewFrame.Navigate(pageType, e, new DrillInNavigationTransitionInfo()));
                    break;
                case NavigationThemeTransition.Entrance:
                    _ = (MainPage?.NavigationViewFrame.Navigate(pageType, e, new EntranceNavigationTransitionInfo()));
                    break;
                case NavigationThemeTransition.Suppress:
                    _ = (MainPage?.NavigationViewFrame.Navigate(pageType, e, new SuppressNavigationTransitionInfo()));
                    break;
                case NavigationThemeTransition.Default:
                    _ = (MainPage?.NavigationViewFrame.Navigate(pageType, e));
                    break;
                default:
                    _ = (MainPage?.NavigationViewFrame.Navigate(pageType, e));
                    break;
            }
        }

        public static async void ShowImage(ImageModel image)
        {
            CoreApplicationView View = CoreApplication.CreateNewView();
            int ViewId = 0;
            await View.ExecuteOnUIThreadAsync(() =>
            {
                CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
                Window window = Window.Current;
                WindowHelper.TrackWindow(window);
                Frame frame = new Frame();
                frame.Navigate(typeof(ShowImagePage), image);
                window.Content = frame;
                ThemeHelper.Initialize(window);
                window.Activate();
                ViewId = ApplicationView.GetForCurrentView().Id;
            });
            _ = await ApplicationViewSwitcher.TryShowAsStandaloneAsync(ViewId);
        }
    }

    internal static partial class UIHelper
    {
        public static async void OpenLinkAsync(string link)
        {
            if (string.IsNullOrWhiteSpace(link)) { return; }

            string origin = link;

            if (link.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                link = link.Replace("http://", string.Empty).Replace("https://", string.Empty);
                if (link.StartsWith("image.coolapk.com"))
                {
                    ShowImage(new ImageModel(origin, ImageType.SmallImage));
                    return;
                }
                else
                {
                    Regex coolapk = new Regex(@"\w*?.?coolapk.\w*/");
                    if (coolapk.IsMatch(link))
                    {
                        link =coolapk.Replace(link, string.Empty);
                    }
                    else
                    {
                        Navigate(typeof(BrowserPage), new BrowserViewModel(origin));
                        return;
                    }
                }
            }

            if (link.FirstOrDefault() != '/')
            {
                link = $"/{link}";
            }

            if (link == "/contacts/fans")
            {
                Navigate(typeof(AdaptivePage), AdaptiveViewModel.GetUserListProvider(SettingsHelper.Get<string>(SettingsHelper.Uid), false, "我"));
            }
            else if (link == "/user/myFollowList")
            {
                Navigate(typeof(AdaptivePage), AdaptiveViewModel.GetUserListProvider(SettingsHelper.Get<string>(SettingsHelper.Uid), true, "我"));
            }
            else if (link.StartsWith("/page?", StringComparison.OrdinalIgnoreCase))
            {
                string url = link.Replace("/page?", string.Empty);
                Navigate(typeof(AdaptivePage), new AdaptiveViewModel(url));
            }
            else if (link.StartsWith("/u/", StringComparison.OrdinalIgnoreCase))
            {
                string url = link.Replace("/u/", string.Empty);
                string uid = int.TryParse(url, out _) ? url : (await NetworkHelper.GetUserInfoByNameAsync(url)).UID;
                FeedListViewModel provider = FeedListViewModel.GetProvider(FeedListType.UserPageList, uid);
                if (provider != null)
                {
                    Navigate(typeof(FeedListPage), provider);
                }
            }
            else if (link.StartsWith("/feed/", StringComparison.OrdinalIgnoreCase))
            {
                string id = link.Substring(6);
                if (int.TryParse(id, out _))
                {
                    Navigate(typeof(FeedShellPage), new FeedDetailViewModel(id));
                }
                else
                {
                    ShowMessage("暂不支持");
                }
            }
            else if (link.StartsWith("/picture/", StringComparison.OrdinalIgnoreCase))
            {
                string id = link.Substring(10);
                if (int.TryParse(id, out _))
                {
                    Navigate(typeof(FeedShellPage), new FeedDetailViewModel(id));
                }
            }
            else if (link.StartsWith("/question/", StringComparison.OrdinalIgnoreCase))
            {
                string id = link.Substring(10);
                if (int.TryParse(id, out _))
                {
                    Navigate(typeof(FeedShellPage), new QuestionViewModel(id));
                }
            }
            else if (link.StartsWith("/t/", StringComparison.OrdinalIgnoreCase))
            {
                int end = link.IndexOf('?');
                string tag = end > 3 ? link.Substring(3) : link.Substring(3, end - 3);
                FeedListViewModel provider = FeedListViewModel.GetProvider(FeedListType.TagPageList, tag);
                if (provider != null)
                {
                    Navigate(typeof(FeedListPage), provider);
                }
            }
            else if (link.StartsWith("/dyh/", StringComparison.OrdinalIgnoreCase))
            {
                string tag = link.Substring(5);
                FeedListViewModel provider = FeedListViewModel.GetProvider(FeedListType.DyhPageList, tag);
                if (provider != null)
                {
                    Navigate(typeof(FeedListPage), provider);
                }
            }
            else if (link.StartsWith("/product/", StringComparison.OrdinalIgnoreCase))
            {
                if (link.StartsWith("/product/categoryList", StringComparison.OrdinalIgnoreCase))
                {
                    Navigate(typeof(AdaptivePage), new AdaptiveViewModel(link));
                }
                else
                {
                    string tag = link.Substring(9);
                    FeedListViewModel provider = FeedListViewModel.GetProvider(FeedListType.ProductPageList, tag);
                    if (provider != null)
                    {
                        Navigate(typeof(FeedListPage), provider);
                    }
                }
            }
            else if (link.StartsWith("/mp/", StringComparison.OrdinalIgnoreCase))
            {
                Navigate(typeof(HTMLPage), new HTMLViewModel(link));
            }
            else
            {
                Navigate(typeof(BrowserPage), new BrowserViewModel(origin));
            }
        }
    }

    public enum MessageType
    {
        Message,
        NoMore,
        NoMoreReply,
        NoMoreLikeUser,
        NoMoreShare,
        NoMoreHotReply,
    }
}
