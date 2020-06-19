using CoolapkUWP.Controls;
using CoolapkUWP.Pages.FeedPages;
using CoolapkUWP.ViewModels.FeedListDataProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using InAppNotification = Microsoft.Toolkit.Uwp.UI.Controls.InAppNotification;

namespace CoolapkUWP.Helpers
{
    internal static class UIHelper
    {
        private const int duration = 4000;
        public static List<Popup> popups = new List<Popup>();

        /// <summary> 用于记录各种通知的数量。 </summary>
        public static NotificationNums NotificationNums { get; } = new NotificationNums();

        public static Frame MainFrame { get; set; }
        public static InAppNotification InAppNotification { get; set; }

        public static event EventHandler<ImageSource> UserAvatarChanged;

        public static event EventHandler<bool> ProgressRingIsActiveChanged;

        public static void RaiseUserAvatarChangedEvent(object sender, ImageSource source) => UserAvatarChanged?.Invoke(sender, source);

        public static void ShowPopup(Popup popup)
        {
            popup.RequestedTheme = SettingsHelper.Get<bool>(SettingsHelper.IsDarkMode) ? ElementTheme.Dark : ElementTheme.Light;
            popups.Add(popup);
            popup.IsOpen = true;
        }

        public static void Hide(this Popup popup)
        {
            popup.IsOpen = false;
            if (popups.Contains(popup)) popups.Remove(popup);
        }

        public static void ShowProgressRing() => ProgressRingIsActiveChanged?.Invoke(null, true);

        public static void HideProgressRing() => ProgressRingIsActiveChanged?.Invoke(null, false);

        public static void ShowMessage(string message) => InAppNotification?.Show(message, duration);

        public static void ShowImage(string url, ImageType type)
        {
            Popup popup = new Popup();
            ShowImageControl control = new ShowImageControl(popup);
            control.ShowImage(url, type);
            popup.Child = control;
            ShowPopup(popup);
        }

        public static void ShowImages(string[] urls, int index)
        {
            Popup popup = new Popup();
            ShowImageControl control = new ShowImageControl(popup);
            control.ShowImages(urls, ImageType.SmallImage, index);
            popup.Child = control;
            ShowPopup(popup);
        }

        public static void Navigate(Type pageType, object e = null) => MainFrame?.Navigate(pageType, e);

        public static async void OpenLinkAsync(string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return;
            if (str == "/contacts/fans")
            {
                Navigate(typeof(UserListPage), new object[] { SettingsHelper.Get<string>(SettingsHelper.Uid), false, "我" });
                return;
            }
            if (str.Contains('?')) str = str.Substring(0, str.IndexOf('?'));
            if (str.Contains('%')) str = str.Substring(0, str.IndexOf('%'));
            if (str.Contains('&')) str = str.Substring(0, str.IndexOf('&'));
            if (str.IndexOf("/u/") == 0)
            {
                string u = str.Replace("/u/", string.Empty);
                //if (int.TryParse(u, out _))
                //    Navigate(typeof(FeedListPage), new object[] { FeedListType.UserPageList, u });
                //else Navigate(typeof(FeedListPage), new object[] { FeedListType.UserPageList, await NetworkHelper.GetUserIDByName(u) });
                if (int.TryParse(u, out _))
                {
                    var f = FeedListDataProvider.GetProvider(FeedListType.UserPageList, u);
                    if (f != null)
                        Navigate(typeof(FeedListPage), f);
                }
                else
                {
                    var f = FeedListDataProvider.GetProvider(FeedListType.UserPageList, await NetworkHelper.GetUserIDByName(u));
                    if (f != null)
                        Navigate(typeof(FeedListPage), f);
                }
            }
            else if (str.IndexOf("/feed/") == 0)
            {
                string u = str.Replace("/feed/", string.Empty);
                Navigate(typeof(FeedDetailPage), u);
            }
            else if (str.IndexOf("/picture/") == 0)
            {
                string u = str.Replace("/picture/", string.Empty);
                Navigate(typeof(FeedDetailPage), u);
            }
            else if (str.IndexOf("/t/") == 0)
            {
                string u = str.Replace("/t/", string.Empty);
                var f = FeedListDataProvider.GetProvider(FeedListType.TagPageList, u);
                if (f != null)
                    Navigate(typeof(FeedListPage), f);
            }
            else if (str.IndexOf("t/") == 0)
            {
                string u = str.Replace("t/", string.Empty);
                var f = FeedListDataProvider.GetProvider(FeedListType.TagPageList, u);
                if (f != null)
                    Navigate(typeof(FeedListPage), f);
            }
            //else if (str.IndexOf("/product/") == 0)
            //{
            //    string u = str.Replace("/product/", string.Empty);
            //    Navigate(typeof(FeedListPage), new object[] { FeedListType.TagPageList, u });
            //}
            else if (str.IndexOf("/dyh/") == 0)
            {
                string u = str.Replace("/dyh/", string.Empty);
                //Navigate(typeof(FeedListPage), new object[] { FeedListType.DyhPageList, u });
                var f = FeedListDataProvider.GetProvider(FeedListType.DyhPageList, u);
                if (f != null)
                    Navigate(typeof(FeedListPage), f);
            }
            else if (str.IndexOf("http://image.coolapk.com/") == 0)
            {
                UIHelper.ShowImage(str, ImageType.SmallImage);
            }
            else if (str.IndexOf("https") == 0)
            {
                if (str.Contains("coolapk.com")) OpenLinkAsync(str.Replace("https://www.coolapk.com", string.Empty));
                else Navigate(typeof(Pages.BrowserPage), new object[] { false, str });
            }
            else if (str.IndexOf("http") == 0)
            {
                if (str.Contains("coolapk.com")) OpenLinkAsync(str.Replace("http://www.coolapk.com", string.Empty));
                else Navigate(typeof(Pages.BrowserPage), new object[] { false, str });
            }
            else
            {
                string u = str.Substring(1);
                u = u.Substring(u.IndexOf('/') + 1);
                Navigate(typeof(FeedDetailPage), u);
            }
        }
    }
}