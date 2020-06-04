using CoolapkUWP.Controls;
using CoolapkUWP.Pages.FeedPages;
using CoolapkUWP.Pages.FeedPages.ViewModels;
//using CoolapkUWP.Pages.FeedPages.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;

namespace CoolapkUWP.Helpers
{
    static class UIHelper
    {
        /// <summary>
        /// 用于记录各种通知的数量。
        /// </summary>
        public static NotificationNums NotificationNums { get; } = new NotificationNums();
        static Pages.MainPage mainPage = null;
        public static List<Popup> popups = new List<Popup>();
        static readonly ObservableCollection<string> messageList = new ObservableCollection<string>();
        static bool isShowingMessage;
        public static bool isShowingProgressBar;

        public static Pages.MainPage MainPage { set => mainPage = value; }
        public static ImageSource MainPageUserAvatar { set => mainPage.UserAvatar = value; }

        static UIHelper()
        {
            Popup popup = new Popup { RequestedTheme = SettingsHelper.Get<bool>("IsDarkMode") ? ElementTheme.Dark : ElementTheme.Light };
            StatusGrid statusGrid2 = new StatusGrid();
            popup.Child = statusGrid2;
            popups.Add(popup);
            popup.IsOpen = true;
        }

        public static void ShowPopup(Popup popup)
        {
            popup.RequestedTheme = SettingsHelper.Get<bool>("IsDarkMode") ? ElementTheme.Dark : ElementTheme.Light;
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

        public static async void ShowProgressBar()
        {
            isShowingProgressBar = true;
            if (SettingsHelper.HasStatusBar)
            {
                StatusBar.GetForCurrentView().ProgressIndicator.ProgressValue = null;
                await StatusBar.GetForCurrentView().ProgressIndicator.ShowAsync();
            }
            else if (popups.Last().Child is StatusGrid statusGrid)
                await statusGrid.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => statusGrid.ShowProgressBar());
        }

        public static async void HideProgressBar()
        {
            isShowingProgressBar = false;
            if (SettingsHelper.HasStatusBar && !isShowingMessage) await StatusBar.GetForCurrentView().ProgressIndicator.HideAsync();
            else if (popups.Last().Child is StatusGrid statusGrid) statusGrid.HideProgressBar();
        }

        public static async void ShowMessage(string message)
        {
            messageList.Add(message);
            if (!isShowingMessage)
            {
                isShowingMessage = true;
                while (messageList.Count > 0)
                {
                    string s = $"[1/{messageList.Count}]{messageList[0]}";
                    if (SettingsHelper.HasStatusBar)
                    {
                        StatusBar statusBar = StatusBar.GetForCurrentView();
                        statusBar.ProgressIndicator.Text = s;
                        if (isShowingProgressBar) statusBar.ProgressIndicator.ProgressValue = null;
                        else statusBar.ProgressIndicator.ProgressValue = 0;
                        await statusBar.ProgressIndicator.ShowAsync();
                        await Task.Delay(3000);
                        if (messageList.Count == 0 && !isShowingProgressBar) await statusBar.ProgressIndicator.HideAsync();
                        statusBar.ProgressIndicator.Text = string.Empty;
                        messageList.RemoveAt(0);
                    }
                    else if (popups.Last().Child is StatusGrid statusGrid)
                    {
                        await statusGrid.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => statusGrid.ShowMessage(s));
                        await Task.Delay(3000);
                        messageList.RemoveAt(0);
                        await statusGrid.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                        {
                            if (messageList.Count == 0) statusGrid.ShowMessage(string.Empty);
                            if (!isShowingProgressBar) HideProgressBar();
                        });
                    }
                }
                isShowingMessage = false;
            }
        }

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

        public static void Navigate(Type pageType, object e = null) => mainPage?.Frame.Navigate(pageType, e);

        public static async void OpenLink(string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return;
            if (str == "/contacts/fans")
            {
                Navigate(typeof(UserListPage), new object[] { SettingsHelper.Get<string>("Uid"), false, "我" });
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
                if (str.Contains("coolapk.com")) OpenLink(str.Replace("https://www.coolapk.com", string.Empty));
                else Navigate(typeof(Pages.BrowserPage), new object[] { false, str });
            }
            else if (str.IndexOf("http") == 0)
            {
                if (str.Contains("coolapk.com")) OpenLink(str.Replace("http://www.coolapk.com", string.Empty));
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
