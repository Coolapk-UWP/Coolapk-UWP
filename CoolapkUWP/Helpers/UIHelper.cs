using CoolapkUWP.Pages.FeedPages;
using CoolapkUWP.ViewModels.FeedListDataProvider;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using InAppNotify = Microsoft.Toolkit.Uwp.UI.Controls.InAppNotification;

namespace CoolapkUWP.Helpers
{
    internal static class UIHelper
    {
        public const int duration = 3500;
        private static Frame mainFrame;
        private static Frame paneFrame;
        private static InAppNotify inAppNotification;

        /// <summary> 用于记录各种通知的数量。 </summary>
        public static NotificationNums NotificationNums { get; } = new NotificationNums();

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

        public static event EventHandler<bool> IsSplitViewPaneOpenedChanged;

        public static void ShowSplitView() => IsSplitViewPaneOpenedChanged?.Invoke(null, true);

        public static void HideSplitView() => IsSplitViewPaneOpenedChanged?.Invoke(null, false);

        public static void ShowMessage(string message)
        {
            _ = InAppNotification?.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
              {
                  InAppNotification?.Show(message, duration);
              });
        }

        private static async void ShowImageWindow(object args)
        {
            var applicationView = CoreApplication.CreateNewView();
            int viewId = 0;
            await applicationView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                Frame frame = new Frame();
                frame.Navigate(typeof(Pages.ShowImagePage), args);
                Window.Current.Content = frame;
                Window.Current.Activate();
                var loader = Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse("Feed");
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

        public static void Navigate(Type pageType, object e = null)
        {
            mainFrame?.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                mainFrame?.Navigate(pageType, e);
            });
        }

        public static void NavigateInSplitPane(Type pageType, object e = null)
        {
            paneFrame?.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                IsSplitViewPaneOpenedChanged?.Invoke(null, true);
                paneFrame?.Navigate(pageType, e);
            });
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
                    await item.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
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
                        break;

                    case ElementTheme.Dark:
                        BackColor = Color.FromArgb(255, 23, 23, 23);
                        ForeColor = Colors.White;
                        ButtonForeInactiveColor = Color.FromArgb(255, 200, 200, 200);
                        ButtonBackPressedColor = Color.FromArgb(255, 50, 50, 50);
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
            if(VisualTree.FindAscendant(originalSource as DependencyObject, typeof(Button)) == null && VisualTree.FindAscendant(originalSource as DependencyObject, typeof(AppBarButton)) == null && originalSource.GetType() != typeof(Button) && originalSource.GetType() != typeof(AppBarButton) && originalSource.GetType() != typeof(RichEditBox))
            {
                r = source == VisualTree.FindAscendant(originalSource as DependencyObject, source.GetType());
            }
            return source == originalSource || r;
        }

        private static readonly string[] routes = new string[]
        {
            "/u/",
            "/feed/",
            "/picture/",
            "/t/",
            "t/",
            "/dyh/",
            "http://image.coolapk.com/",
            "https",
            "http",
        };

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
            if (string.IsNullOrWhiteSpace(str)) { return; }

            if (str == "/contacts/fans")
            {
                NavigateInSplitPane(typeof(UserListPage), new object[] { SettingsHelper.Get<string>(SettingsHelper.Uid), false, "我" });
                return;
            }
            else if (str == "/user/myFollowList")
            {
                NavigateInSplitPane(typeof(UserListPage), new object[] { SettingsHelper.Get<string>(SettingsHelper.Uid), true, "我" });
                return;
            }

            if (str.Contains('?')) str = str.Substring(0, str.IndexOf('?'));
            if (str.Contains('%')) str = str.Substring(0, str.IndexOf('%'));
            if (str.Contains('&')) str = str.Substring(0, str.IndexOf('&'));
            int i = 0;
            if (str.IsFirst(i++))
            {
                var u = str.Replace(i - 1);
                var uid = int.TryParse(u, out _) ? u : await NetworkHelper.GetUserIDByNameAsync(u);
                var f = FeedListDataProvider.GetProvider(FeedListType.UserPageList, uid);
                if (f != null)
                    NavigateInSplitPane(typeof(FeedListPage), f);
            }
            else if (str.IsFirst(i++) || str.IsFirst(i++))
            {
                Navigate(typeof(FeedShellPage), str.Replace(i - 1));
            }
            else if (str.IsFirst(i++) || str.IsFirst(i++))
            {
                string u = str.Replace(i - 1);
                var f = FeedListDataProvider.GetProvider(FeedListType.TagPageList, u);
                if (f != null)
                    NavigateInSplitPane(typeof(FeedListPage), f);
            }
            //else if (str.IndexOf("/product/") == 0)
            //{
            //    string u = str.Replace("/product/", string.Empty);
            //    Navigate(typeof(FeedListPage), new object[] { FeedListType.TagPageList, u });
            //}
            else if (str.IsFirst(i++))
            {
                string u = str.Replace(i - 1);
                var f = FeedListDataProvider.GetProvider(FeedListType.DyhPageList, u);
                if (f != null)
                    NavigateInSplitPane(typeof(FeedListPage), f);
            }
            else if (str.IsFirst(i++))
            {
                ShowImage(str, ImageType.SmallImage);
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
                    Navigate(typeof(Pages.BrowserPage), new object[] { false, str });
                }
            }
            //else
            //{
            //    string u = str.Substring(1);
            //    u = u.Substring(u.IndexOf('/') + 1);
            //    Navigate(typeof(FeedDetailPage), u);
            //}
        }
    }
}