using CoolapkUWP.Control;
using CoolapkUWP.Pages.FeedPages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.System;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;

namespace CoolapkUWP.Data
{
    static class Tools
    {
        static HttpClient mClient;
        public static NotificationsNum notifications = new NotificationsNum();
        public static Pages.MainPage mainPage = null;
        public static List<Popup> popups = new List<Popup>();
        static ObservableCollection<string> messageList = new ObservableCollection<string>();
        static bool isShowingMessage;
        public static bool isShowingProgressBar;
        static Tools()
        {
            mClient = new HttpClient();
            //mClient.DefaultRequestHeaders.UserAgent.ParseAdd($"{mClient.DefaultRequestHeaders.UserAgent} +CoolMarket/9.2.2-1905301");
            mClient.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
            mClient.DefaultRequestHeaders.Add("X-Sdk-Int", "28");
            mClient.DefaultRequestHeaders.Add("X-Sdk-Locale", "zh-CN");
            mClient.DefaultRequestHeaders.Add("X-App-Id", "com.coolapk.market");
            mClient.DefaultRequestHeaders.Add("X-App-Version", "9.2.2");
            mClient.DefaultRequestHeaders.Add("X-App-Code", "1905301");
            mClient.DefaultRequestHeaders.Add("X-Api-Version", "9");
            //mClient.DefaultRequestHeaders.Add("X-App-Device", "QRTBCOgkUTgsTat9WYphFI7kWbvFWaYByO1YjOCdjOxAjOxEkOFJjODlDI7ATNxMjM5MTOxcjMwAjN0AyOxEjNwgDNxITM2kDMzcTOgsTZzkTZlJ2MwUDNhJ2MyYzM");
            mClient.DefaultRequestHeaders.Add("Cookie", Settings.cookie);
            Popup popup = new Popup { RequestedTheme = Settings.GetBoolen("IsDarkMode") ? ElementTheme.Dark : ElementTheme.Light };
            StatusGrid statusGrid2 = new StatusGrid();
            popup.Child = statusGrid2;
            popups.Add(popup);
            popup.IsOpen = true;
        }

        #region UI相关
        public static void ShowPopup(Popup popup)
        {
            popup.RequestedTheme = Settings.GetBoolen("IsDarkMode") ? ElementTheme.Dark : ElementTheme.Light;
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
            if (Settings.HasStatusBar)
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
            if (Settings.HasStatusBar && !isShowingMessage) await StatusBar.GetForCurrentView().ProgressIndicator.HideAsync();
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
                    if (Settings.HasStatusBar)
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

        public static void ShowHttpExceptionMessage(HttpRequestException e)
        {
            if (e.Message.IndexOfAny(new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' }) != -1)
                ShowMessage($"服务器错误： {e.Message.Replace("Response status code does not indicate success: ", string.Empty)}");
            else if (e.Message == "An error occurred while sending the request.") ShowMessage("无法连接网络。");
            else ShowMessage($"请检查网络连接。 {e.Message}");
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
            if (str is null) return;
            if (str.Contains('?')) str = str.Substring(0, str.IndexOf('?'));
            if (str.Contains('%')) str = str.Substring(0, str.IndexOf('%'));
            if (str.Contains('&')) str = str.Substring(0, str.IndexOf('&'));
            if (str.IndexOf("/u/") == 0)
            {
                string u = str.Replace("/u/", string.Empty);
                if (int.TryParse(u, out int uu))
                    Navigate(typeof(FeedListPage), new object[] { FeedListType.UserPageList, u });
                else Navigate(typeof(FeedListPage), new object[] { FeedListType.UserPageList, await GetUserIDByName(u) });
            }
            else if (str.IndexOf("/feed/") == 0)
            {
                string u = str.Replace("/feed/", string.Empty);
                Navigate(typeof(FeedDetailPage), u);
            }
            else if (str.IndexOf("/t/") == 0)
            {
                string u = str.Replace("/t/", string.Empty);
                Navigate(typeof(FeedListPage), new object[] { FeedListType.TagPageList, u });
            }
            else if (str.IndexOf("/dyh/") == 0)
            {
                string u = str.Replace("/dyh/", string.Empty);
                Navigate(typeof(FeedListPage), new object[] { FeedListType.DYHPageList, u });
            }
            else if (str.IndexOf("/apk/") == 0)
            {
                string u = "http://www.coolapk.com" + str;
                Navigate(typeof(Pages.AppPages.AppPage), u);
            }
            else if (str.IndexOf("https") == 0)
            {
                if (str.Contains("coolapk.com"))
                    OpenLink(str.Replace("https://www.coolapk.com", string.Empty));
                else Navigate(typeof(Pages.BrowserPage), new object[] { false, str });
            }
            else if (str.IndexOf("http") == 0)
            {
                if (str.Contains("coolapk.com"))
                    OpenLink(str.Replace("http://www.coolapk.com", string.Empty));
                else Navigate(typeof(Pages.BrowserPage), new object[] { false, str });
            }
        }

        public static async void SetEmojiPadding(object control)
        {
            Microsoft.Toolkit.Uwp.UI.Controls.MarkdownTextBlock textBlock = control as Microsoft.Toolkit.Uwp.UI.Controls.MarkdownTextBlock;
            await textBlock.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                Border border = VisualTreeHelper.GetChild(textBlock, 0) as Border;
                StackPanel stackPanel = (border.Child as StackPanel);
                Thickness thickness = new Thickness(0);
                for (int j = 0; j < stackPanel.Children.Count; j++)
                {
                    if (stackPanel.Children[j] is RichTextBlock a)
                    {
                        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(a); i++)
                        {
                            if (VisualTreeHelper.GetChild(a, i) is ScrollViewer viewer)
                            {
                                Viewbox viewbox = viewer.Content as Viewbox;
                                HyperlinkButton b = viewbox.Child as HyperlinkButton;
                                Image image = b.Content as Image;
                                Windows.UI.Xaml.Media.Imaging.BitmapImage bitmapImage = (image.Source as Windows.UI.Xaml.Media.Imaging.BitmapImage);
                                if (bitmapImage.UriSource.AbsoluteUri.IndexOf("ms-appx") == 0)
                                    b.Padding = thickness;
                            }
                        }
                    }
                }
            });
        }
        #endregion

        //来源：https://blog.csdn.net/lindexi_gd/article/details/48951849
        public static string GetMD5(string inputString)
        {
            CryptographicHash objHash = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Md5).CreateHash();
            objHash.Append(CryptographicBuffer.ConvertStringToBinary(inputString, BinaryStringEncoding.Utf8));
            Windows.Storage.Streams.IBuffer buffHash1 = objHash.GetValueAndReset();
            return CryptographicBuffer.EncodeToHexString(buffHash1);
        }

        //https://github.com/ZCKun/CoolapkTokenCrack
        static string GetCoolapkAppToken()
        {
            string DEVICE_ID = Guid.NewGuid().ToString();
            long UnixDate = (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000;
            string t = UnixDate.ToString();
            string hex_t = "0x" + string.Format("{0:x}", UnixDate);
            // 时间戳加密
            string md5_t = GetMD5(t);
            string a = "token://com.coolapk.market/c67ef5943784d09750dcfbb31020f0ab?" + md5_t + "$" + DEVICE_ID + "&com.coolapk.market";
            string md5_a = GetMD5(Convert.ToBase64String(Encoding.UTF8.GetBytes(a)));
            string token = md5_a + DEVICE_ID + hex_t;
            return token;
        }

        public static async Task<string> GetJson(string url, bool isBackground = false)
        {
            try
            {
                if (url != "/notification/checkCount") notifications?.RefreshNotificationsNum();
                mClient.DefaultRequestHeaders.Remove("X-App-Token");
                mClient.DefaultRequestHeaders.Add("X-App-Token", GetCoolapkAppToken());
                mClient.DefaultRequestHeaders.Remove("Cookie");
                mClient.DefaultRequestHeaders.Add("Cookie", Settings.cookie);
                return await mClient.GetStringAsync(new Uri("https://api.coolapk.com/v6" + url));
            }
            catch (HttpRequestException e)
            {
                if (!isBackground) ShowHttpExceptionMessage(e);
                return string.Empty;
            }
            catch
            {
                if (isBackground) return string.Empty;
                else throw;
            }
        }

        public static async Task<bool> Post(string url, HttpContent content)
        {
            try
            {
                if (url != "/notification/checkCount") notifications?.RefreshNotificationsNum();
                mClient.DefaultRequestHeaders.Remove("X-App-Token");
                mClient.DefaultRequestHeaders.Add("X-App-Token", GetCoolapkAppToken());
                mClient.DefaultRequestHeaders.Remove("Cookie");
                mClient.DefaultRequestHeaders.Add("Cookie", Settings.cookie);
                var a = await mClient.PostAsync(new Uri("https://api.coolapk.com/v6" + url), content);
                return !(GetJSonObject(await a.Content.ReadAsStringAsync()) is null);
            }
            catch (HttpRequestException e)
            {
                ShowHttpExceptionMessage(e);
                return false;
            }
            catch { throw; }

        }

        public static JsonObject GetJSonObject(string json)
        {
            try
            {
                if (string.IsNullOrEmpty(json)) return null;
                return JsonObject.Parse(json)["data"].GetObject();
            }
            catch
            {
                if (JsonObject.Parse(json).TryGetValue("message", out IJsonValue value))
                    ShowMessage($"{value.GetString()}");
                return null;
            }
        }

        public static string GetObjectStrigInJson(string json)
        {
            try
            {
                if (string.IsNullOrEmpty(json)) return null;
                return JsonObject.Parse(json)["data"].ToString().Replace("\"", string.Empty);
            }
            catch
            {
                if (JsonObject.Parse(json).TryGetValue("message", out IJsonValue value))
                    ShowMessage($"{value.GetString()}");
                return string.Empty;
            }
        }

        public static JsonArray GetDataArray(string json)
        {
            try
            {
                if (string.IsNullOrEmpty(json)) return null;
                return JsonObject.Parse(json)["data"].GetArray();
            }
            catch
            {
                if (JsonObject.Parse(json).TryGetValue("message", out IJsonValue value))
                    ShowMessage($"{value.GetString()}");
                return null;
            }
        }

        public static string GetMessageText(string s)
        {
            StringBuilder builder = new StringBuilder(s);
            builder = builder.Replace("\\", "\\\\");
            builder = builder.Replace("&#039;", "\'");
            builder = builder.Replace("&nbsp;", "\\&nbsp;");
            builder = builder.Replace("</a>", "</a> ");
            builder = builder.Replace(">", "\\>");
            builder = builder.Replace("#", "\\#");
            builder = builder.Replace("`", "\\`");
            builder = builder.Replace("*", "\\*");
            builder = builder.Replace("(", "\\(");
            builder = builder.Replace("~", "\\~");
            builder = builder.Replace("^", "\\^");
            builder = builder.Replace("[", "\\[");
            builder = builder.Replace("+", "\\+");
            builder = builder.Replace("-", "\\-");
            builder = builder.Replace("\\-\\-\\>", "-->");
            builder = builder.Replace("!\\-\\-", "!--");
            builder = builder.Replace(".", "\\.");
            builder = builder.Replace(":", "\\:");
            builder = builder.Replace("\n", "\n\n");
            foreach (var i in Emojis.emojis)
            {
                s = builder.ToString();
                if (s.Contains(i))
                {
                    if (i.Contains("(")) builder = builder.Replace($"\\#\\{i})", $"\n![{i})](ms-appx:/Assets/Emoji/{i}.png =20)");
                    else if (Settings.GetBoolen("IsUseOldEmojiMode") && Emojis.oldEmojis.Contains(i))
                        builder = builder.Replace($"\\{i}", $"\n![{i}(ms-appx:/Assets/Emoji/{i}2.png =20)");
                    else builder = builder.Replace($"\\{i}", $"\n![{i}(ms-appx:/Assets/Emoji/{i}.png =20)");
                }
            }
            Regex regex = new Regex("<a.*?\\>\\S*"), regex2 = new Regex("href=\".*"), regex3 = new Regex(">.*<");
            while (regex.IsMatch(s))
            {
                s = builder.ToString();
                var h = regex.Match(s);
                if (!h.Value.Contains("</a\\>"))
                {
                    builder = builder.Replace(h.Value + " ", h.Value + "&nbsp;");
                    continue;
                }
                string t = regex3.Match(h.Value).Value.Replace(">", string.Empty);
                t = t.Replace("<", string.Empty);
                string tt = regex2.Match(h.Value).Value.Replace("href=\"", string.Empty);
                if (tt.IndexOf("\"") > 0)
                {
                    tt = tt.Substring(0, tt.IndexOf("\""));
                    tt = tt.Replace("\\:", ":");
                    tt = tt.Replace("\\.", ".");
                    tt = tt.Replace("\'", "&#039;");
                    tt = tt.Replace("\\&nbsp;", "&nbsp;");
                    tt = tt.Replace("\\>", ">");
                    tt = tt.Replace("\\#", "#");
                    tt = tt.Replace("\\`", "`");
                    tt = tt.Replace("\\*", "*");
                    tt = tt.Replace("\\(", "(");
                    tt = tt.Replace("\\~", "~");
                    tt = tt.Replace("\\^", "^");
                    tt = tt.Replace("\\[", "[");
                    tt = tt.Replace("\\+", "+");
                    tt = tt.Replace("\\-", "-");
                }
                if (t == "查看更多") tt = "get";
                builder = builder.Replace(h.Value, $"[{t}]({tt})");
            }
            builder = builder.Replace(" ", "&nbsp;");
            builder = builder.Replace("&nbsp;=20)", " =20)");
            return builder.ToString();
        }

        public static string ConvertTime(double timestr)
        {
            DateTime time = new DateTime(1970, 1, 1).ToLocalTime().Add(new TimeSpan(Convert.ToInt64(timestr) * 10000000));
            TimeSpan temptime = DateTime.Now.Subtract(time);
            if (temptime.TotalDays > 30) return $"{time.Year}/{time.Month}/{time.Day}";
            else if (temptime.Days > 0) return $"{temptime.Days}天前";
            else if (temptime.Hours > 0) return $"{temptime.Hours}小时前";
            else if (temptime.Minutes > 0) return $"{temptime.Minutes}分钟前";
            else return "刚刚";
        }

        public static string ReplaceHtml(string str)
        {
            //换行和段落
            string s = str.Replace("<br>", "\n").Replace("<br>", "\n").Replace("<br/>", "\n").Replace("<br />", "\n").Replace("<p>", "").Replace("</p>", "\n").Replace("&nbsp;", " ");
            //链接彻底删除！
            while (s.IndexOf("<a") > 0)
            {
                s = s.Replace(@"<a href=""" + Regex.Split(Regex.Split(s, @"<a href=""")[1], @""">")[0] + @""">", "");
                s = s.Replace("</a>", "");
            }
            return s;
        }

        public static async Task<string> GetUserIDByName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                ShowMessage("请输入用户名");
                return "0";
            }

            try
            {
                string uid = await new HttpClient().GetStringAsync($"https://www.coolapk.com/n/name");
                uid = uid.Split(new string[] { "coolmarket://www.coolapk.com/u/" }, StringSplitOptions.RemoveEmptyEntries)[1];
                uid = uid.Split(new string[] { @"""" }, StringSplitOptions.RemoveEmptyEntries)[0];
                return uid;
            }
            catch (HttpRequestException e)
            {
                if (e.Message.Contains("404")) ShowMessage("用户不存在");
                else ShowHttpExceptionMessage(e);
                return "0";
            }
        }

        public static string GetSizeString(double size)
        {
            int index = 0;
            while (true)
            {
                index++;
                size /= 1024;
                if (size > 0.7 && size < 716.8) break;
                else if (size >= 716.8) continue;
                else if (size <= 0.7)
                {
                    size *= 1024;
                    index--;
                    break;
                }
            }
            string str = string.Empty;
            switch (index)
            {
                case 0: str = "B"; break;
                case 1: str = "KB"; break;
                case 2: str = "MB"; break;
                case 3: str = "GB"; break;
                case 4: str = "TB"; break;
            }
            return $"{size.ToString("N2")} {str}";
        }
    }
}