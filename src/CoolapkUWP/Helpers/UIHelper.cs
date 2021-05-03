using CoolapkUWP.Controls;
using CoolapkUWP.Pages;
using CoolapkUWP.Pages.FeedPages;
using CoolapkUWP.ViewModels.FeedListPage;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Resources;
using Windows.Data.Xml.Dom;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Notifications;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media.Animation;
using InAppNotify = Microsoft.Toolkit.Uwp.UI.Controls.InAppNotification;

namespace CoolapkUWP.Helpers
{
    [DebuggerStepThrough]
    internal static partial class UIHelper
    {
        public static event EventHandler<bool> IsSplitViewPaneOpenedChanged;

        public static event EventHandler<bool> NeedMainPageProgressRing;

        public static event EventHandler RequireIndexPageRefresh;

        public static void ShowSplitView() => IsSplitViewPaneOpenedChanged?.Invoke(null, true);

        public static void HideSplitView() => IsSplitViewPaneOpenedChanged?.Invoke(null, false);

        public static void ShowMainPageProgressRing() => NeedMainPageProgressRing?.Invoke(null, true);

        public static void HideMainPageProgressRing() => NeedMainPageProgressRing?.Invoke(null, false);

        public static void RefreshIndexPage() => RequireIndexPageRefresh?.Invoke(null, null);
    }

    static partial class UIHelper
    {
        public const int duration = 3000;
        static bool isShowingMessage;
        private static InAppNotify inAppNotification;
        private static CoreDispatcher shellDispatcher;
        public static List<Popup> popups = new List<Popup>();
        static ObservableCollection<string> messageList = new ObservableCollection<string>();
        public static bool HasStatusBar => Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar");

        public static CoreDispatcher ShellDispatcher
        {
            get => shellDispatcher;
            set
            {
                if (shellDispatcher == null)
                {
                    shellDispatcher = value;
                }
            }
        }

        public static InAppNotify InAppNotification
        {
            get => inAppNotification;
            set
            {
                if (inAppNotification == null)
                {
                    inAppNotification = value;
                }
            }
        }

        /// <summary> 用于记录各种通知的数量。 </summary>
        public static NotificationNums NotificationNums { get; } = new NotificationNums();

        public static void ShowMessage(string message)
        {
            _ = InAppNotification?.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
              {
                  InAppNotification?.Show(message, duration);
              });
        }

        public static void ShowPopup(Popup popup)
        {
            popup.RequestedTheme = SettingsHelper.Get<bool>("IsBackgroundColorFollowSystem") ? ElementTheme.Default : (SettingsHelper.Get<bool>("IsDarkMode") ? ElementTheme.Dark : ElementTheme.Light);
            if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Desktop")
                popups.Insert(popups.Count - 1, popup);
            else
                popups.Add(popup);
            popup.IsOpen = true;
            popups.Last().IsOpen = false;
            popups.Last().IsOpen = true;
        }

        public static void Hide(this Popup popup)
        {
            popup.IsOpen = false;
            if (popups.Contains(popup)) popups.Remove(popup);
        }

        public static async void StatusBar_ShowMessage(string message)
        {
            messageList.Add(message);
            if (!isShowingMessage)
            {
                isShowingMessage = true;
                while (messageList.Count > 0)
                {
                    string s = $"[1/{messageList.Count}]{messageList[0]}";
                    if (HasStatusBar)
                    {
                        StatusBar statusBar = StatusBar.GetForCurrentView();
                        statusBar.ProgressIndicator.Text = s;
                        //if (isShowingProgressBar) statusBar.ProgressIndicator.ProgressValue = null;
                        //else statusBar.ProgressIndicator.ProgressValue = 0;
                        await statusBar.ProgressIndicator.ShowAsync();
                        await Task.Delay(3000);
                        //if (messageList.Count == 0 && !isShowingProgressBar) await statusBar.ProgressIndicator.HideAsync();
                        statusBar.ProgressIndicator.Text = string.Empty;
                        messageList.RemoveAt(0);
                    }
                    else if (popups.Last().Child is StatusGrid statusGrid)
                    {
                        await statusGrid.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => statusGrid.ShowMessage(s));
                        await Task.Delay(3000);
                        messageList.RemoveAt(0);
                        await statusGrid.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            if (messageList.Count == 0) statusGrid.Rectangle_PointerExited();
                            //if (!isShowingProgressBar) HideProgressBar();
                        });
                    }
                }
                isShowingMessage = false;
            }
        }

        private static async void ShowImageWindow(object args)
        {
            var applicationView = CoreApplication.CreateNewView();
            int viewId = 0;
            await applicationView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                Frame frame = new Frame();
                frame.Navigate(typeof(ShowImagePage), args);
                Window.Current.Content = frame;
                Window.Current.Activate();
                var loader = ResourceLoader.GetForViewIndependentUse("Feed");
                ApplicationView.GetForCurrentView().Title = loader.GetString("seePic");

                viewId = ApplicationView.GetForCurrentView().Id;
            });

            var viewShown = await ApplicationViewSwitcher.TryShowAsStandaloneAsync(viewId);
        }

        public static void ShowImage(string url, ImageType type)
        {
            ShowImageWindow(new object[] { url, type });
        }

        public static void ShowImage(Models.ImageModel model)
        {
            ShowImageWindow(model);
        }

        public static async void CheckTheme()
        {
            while (Window.Current?.Content is null)
            {
                await Task.Delay(100);
            }

            if (Window.Current.Content is FrameworkElement frameworkElement)
            {
                foreach (var item in CoreApplication.Views)
                {
                    await item.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        (Window.Current.Content as FrameworkElement).RequestedTheme = SettingsHelper.Theme;
                    });
                }

                Color? BackColor, ForeColor, ButtonForeInactiveColor, ButtonBackPressedColor;
                BackColor = ForeColor = ButtonBackPressedColor = ButtonForeInactiveColor = null;
                switch (SettingsHelper.Theme)
                {
                    case ElementTheme.Light:
                        BackColor = Color.FromArgb(255, 242, 242, 242);
                        ForeColor = Colors.Black;
                        ButtonForeInactiveColor = Color.FromArgb(255, 50, 50, 50);
                        ButtonBackPressedColor = Color.FromArgb(255, 200, 200, 200);
                        SettingsHelper.UiSettingChanged.Invoke(UiSettingChangedType.LightMode);
                        break;

                    case ElementTheme.Dark:
                        BackColor = Color.FromArgb(255, 23, 23, 23);
                        ForeColor = Colors.White;
                        ButtonForeInactiveColor = Color.FromArgb(255, 200, 200, 200);
                        ButtonBackPressedColor = Color.FromArgb(255, 50, 50, 50);
                        SettingsHelper.UiSettingChanged.Invoke(UiSettingChangedType.DarkMode);
                        break;
                }

                var view = ApplicationView.GetForCurrentView().TitleBar;
                view.ButtonBackgroundColor = view.InactiveBackgroundColor = view.ButtonInactiveBackgroundColor = Colors.Transparent;
                view.ForegroundColor = view.ButtonForegroundColor = view.ButtonHoverForegroundColor = view.ButtonPressedForegroundColor = ForeColor;
                view.InactiveForegroundColor = view.ButtonInactiveForegroundColor = ButtonForeInactiveColor;
                view.ButtonHoverBackgroundColor = BackColor;
                view.ButtonPressedBackgroundColor = ButtonBackPressedColor;
            }
        }

        public static bool IsOriginSource(object source, object originalSource)
        {
            var r = false;
            if (VisualTree.FindAscendant(originalSource as DependencyObject, typeof(Button)) == null && VisualTree.FindAscendant(originalSource as DependencyObject, typeof(AppBarButton)) == null && originalSource.GetType() != typeof(Button) && originalSource.GetType() != typeof(AppBarButton) && originalSource.GetType() != typeof(RichEditBox))
            {
                r = source == VisualTree.FindAscendant(originalSource as DependencyObject, source.GetType());
            }
            return source == originalSource || r;
        }

        public static string ConvertMessageTypeToMessage(Core.Helpers.MessageType type)
        {
            switch (type)
            {
                case Core.Helpers.MessageType.NoMore:
                    return ResourceLoader.GetForViewIndependentUse("NotificationsPage").GetString("noMore");

                case Core.Helpers.MessageType.NoMoreReply:
                    return ResourceLoader.GetForViewIndependentUse("FeedShellListControl").GetString("noMoreReply");

                case Core.Helpers.MessageType.NoMoreLikeUser:
                    return ResourceLoader.GetForViewIndependentUse("FeedShellListControl").GetString("noMoreLikeUser");

                case Core.Helpers.MessageType.NoMoreShare:
                    return ResourceLoader.GetForViewIndependentUse("FeedShellListControl").GetString("noMoreShare");

                case Core.Helpers.MessageType.NoMoreHotReply:
                    return ResourceLoader.GetForViewIndependentUse("FeedShellListControl").GetString("noMoreHotReply");

                default: return string.Empty;
            }
        }
    }

    static partial class UIHelper
    {
        private static Frame mainFrame;
        private static Frame paneFrame;
        static ShellPage shellPage = new ShellPage();

        public static Frame MainFrame
        {
            get => mainFrame;
            set
            {
                if (mainFrame == null)
                {
                    mainFrame = value;
                }
            }
        }

        public static Frame PaneFrame
        {
            get => paneFrame;
            set
            {
                if (paneFrame == null)
                {
                    paneFrame = value;
                }
            }
        }

        public static void Navigate(Type pageType, object e = null)
        {
            shellPage.CloseSplit();
            mainFrame?.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                mainFrame?.Navigate(pageType, e, new EntranceNavigationTransitionInfo());
            });
        }

        public static void NavigateInSplitPane(Type pageType, object e = null)
        {
            paneFrame?.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                IsSplitViewPaneOpenedChanged?.Invoke(null, true);
                paneFrame?.Navigate(pageType, e, new EntranceNavigationTransitionInfo());
            });
        }

        private static readonly ImmutableArray<string> routes = new string[]
        {
            "/page?",
            "/u/",
            "/feed/",
            "/picture/",
            "/question/",
            "/t/",
            "t/",
            "/dyh/",
            "/collection/",
            "http://image.coolapk.com/",
            "https",
            "http",
            "www.coolapk.com",
            "/product/",
            "/game/",
            "/apk/",
        }.ToImmutableArray();

        private static bool IsFirst(this string str, int i) => str.IndexOf(routes[i], StringComparison.Ordinal) == 0;

        private static string Replace(this string str, int oldText)
        {
            if (oldText == -1)
            {
                return str.Replace("https://www.coolapk.com", string.Empty, StringComparison.Ordinal);
            }
            else if (oldText == -2)
            {
                return str.Replace("http://www.coolapk.com", string.Empty, StringComparison.Ordinal);
            }
            else if (oldText == -3)
            {
                return str.Replace("www.coolapk.com", string.Empty, StringComparison.Ordinal);
            }
            else if (oldText < 0)
            {
                throw new Exception($"i = {oldText}");
            }
            else
            {
                return str.Replace(routes[oldText], string.Empty, StringComparison.Ordinal);
            }
        }

        public static async void OpenLinkAsync(string str)
        {
            //UIHelper.ShowMessage(str);

#if DEBUG
            var rawstr = str;
#endif

            if (string.IsNullOrWhiteSpace(str)) { return; }

            if (str == "/contacts/fans")
            {
                NavigateInSplitPane(typeof(UserListPage), new ViewModels.UserListPage.ViewModel(SettingsHelper.Get<string>(SettingsHelper.Uid), false, "我"));
                return;
            }
            else if (str == "/user/myFollowList")
            {
                NavigateInSplitPane(typeof(UserListPage), new ViewModels.UserListPage.ViewModel(SettingsHelper.Get<string>(SettingsHelper.Uid), true, "我"));
                return;
            }

            int i = 0;
            if (str.IsFirst(i++))
            {
                Navigate(typeof(IndexPage), new ViewModels.IndexPage.ViewModel(str, true));
            }

            if (str.Contains('?')) str = str.Substring(0, str.IndexOf('?'));
            if (str.Contains('%')) str = str.Substring(0, str.IndexOf('%'));
            if (str.Contains('&')) str = str.Substring(0, str.IndexOf('&'));

            if (str.IsFirst(i++))
            {
                var u = str.Replace(i - 1);
                var uid = int.TryParse(u, out _) ? u : await Core.Helpers.NetworkHelper.GetUserIDByNameAsync(u);
                var f = FeedListPageViewModelBase.GetProvider(FeedListType.UserPageList, uid);
                if (f != null)
                {
                    NavigateInSplitPane(typeof(FeedListPage), f);
                }
            }
            else if (str.IsFirst(i++) || str.IsFirst(i++))
            {
                if (str == "/feed/writer")
                    UIHelper.ShowMessage("暂不支持");
                else Navigate(typeof(FeedShellPage), new ViewModels.FeedDetailPage.FeedViewModel(str.Replace(i - 1)));
            }
            else if (str.IsFirst(i++))
            {
                Navigate(typeof(FeedShellPage), new ViewModels.FeedDetailPage.QuestionViewModel(str.Replace(i - 1)));
            }
            else if (str.IsFirst(i++) || str.IsFirst(i++))
            {
                string u = str.Replace(i - 1);
                var f = FeedListPageViewModelBase.GetProvider(FeedListType.TagPageList, u);
                if (f != null)
                {
                    NavigateInSplitPane(typeof(FeedListPage), f);
                }
            }
            else if (str.IsFirst(i++))
            {
                string u = str.Replace(i - 1);
                var f = FeedListPageViewModelBase.GetProvider(FeedListType.DyhPageList, u);
                if (f != null)
                {
                    NavigateInSplitPane(typeof(FeedListPage), f);
                }
            }
            else if (str.IsFirst(i++))
            {
                string u = str.Replace(i - 1);
                var f = FeedListPageViewModelBase.GetProvider(FeedListType.CollectionPageList, u);
                if (f != null)
                {
                    NavigateInSplitPane(typeof(FeedListPage), f);
                }
            }
            else if (str.IsFirst(i++))
            {
                ShowImage(str, ImageType.SmallImage);
            }
            else if (str == "https://m.coolapk.com/mp/user/communitySpecification")
            {
                Navigate(typeof(Pages.BrowserPage), new object[] { false, str });
            }
            else if (str.IsFirst(i++))
            {
                if (str.Contains("coolapk.com", StringComparison.Ordinal))
                {
                    OpenLinkAsync(str.Replace(-1));
                }
                else
                {
                    Navigate(typeof(Pages.BrowserPage), new object[] { false, str });
                }
            }
            else if (str.IsFirst(i++))
            {
                if (str.Contains("coolapk.com", StringComparison.Ordinal))
                {
                    OpenLinkAsync(str.Replace(-2));
                }
                else
                {
                    Navigate(typeof(BrowserPage), new object[] { false, str });
                }
            }
            else if (str.IsFirst(i++))
            {
                OpenLinkAsync(str.Replace(-3));
            }
            else if (str.IsFirst(i++))
            {
                string u = str.Replace(i - 1);
                //UIHelper.ShowMessage(u);
                var f = FeedListPageViewModelBase.GetProvider(FeedListType.ProductPageList, u);
                if (f != null)
                {
                    NavigateInSplitPane(typeof(FeedListPage), f);
                }
            }
            else if (str.IsFirst(i++))
            {
                NavigateInSplitPane(typeof(Pages.AppPages.AppPage), "https://www.coolapk.com" + str);
                //string u = str.Replace(i - 1);
                ////UIHelper.ShowMessage(u);
                //var f = FeedListPageViewModelBase.GetProvider(FeedListType.AppPageList, u);
                //if (f != null)
                //{
                //    UIHelper.ShowMessage(u);
                //    NavigateInSplitPane(typeof(FeedListPage), f);
                //}
            }
            else if (str.IsFirst(i++))
            {
                NavigateInSplitPane(typeof(Pages.AppPages.AppPage), "https://www.coolapk.com" + str);
                //string u = str.Replace(i - 1);
                //var f = FeedListPageViewModelBase.GetProvider(FeedListType.AppPageList, u);
                //if (f != null)
                //{
                //    UIHelper.ShowMessage(u);
                //    NavigateInSplitPane(typeof(FeedListPage), f);
                //}
            }
            //else
            //{
            //    string u = str.Substring(1);
            //    u = u.Substring(u.IndexOf('/') + 1);
            //    Navigate(typeof(FeedDetailPage), u);
            //}
#if DEBUG
            else
            {
                Navigate(typeof(IndexPage), new ViewModels.IndexPage.ViewModel(rawstr, true));
            }
#endif
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
    }
}