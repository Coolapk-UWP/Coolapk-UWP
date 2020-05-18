using CoolapkUWP.Helpers;
using CoolapkUWP.Pages.FeedPages;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace CoolapkUWP.Pages
{
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        ImageSource userAvatar;
        public ImageSource UserAvatar
        {
            get => userAvatar;
            set
            {
                userAvatar = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UserAvatar)));
            }
        }

        public MainPage()
        {
            this.InitializeComponent();
            if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Desktop")
                Windows.ApplicationModel.Core.CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            if (SettingsHelper.Get<bool>("CheckUpdateWhenLuanching")) SettingsHelper.CheckUpdate();
            SystemNavigationManager.GetForCurrentView().BackRequested += (sender, ee) =>
            {
                int i = SettingsHelper.HasStatusBar ? UIHelper.popups.Count - 1 : UIHelper.popups.Count - 2;
                if (i >= 0)
                {
                    ee.Handled = true;
                    Windows.UI.Xaml.Controls.Primitives.Popup popup = UIHelper.popups[i];
                    popup.IsOpen = false;
                    UIHelper.popups.Remove(popup);
                }
                else if (Frame.CanGoBack)
                {
                    ee.Handled = true;
                    Frame.GoBack();
                }
            };
            var a = new WeakReference<ImageSource>(new Windows.UI.Xaml.Media.Imaging.BitmapImage());
            Windows.UI.Notifications.TileUpdateManager.CreateTileUpdaterForApplication().Clear();
            UIHelper.MainPage = this;
            GetIndexPageItems();
        }
        /* 搜索框相关
        #region 搜索框相关
        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width >= 768)
            {
                SearchBox.Visibility = Visibility.Visible;
                SearchButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                SearchBox.Visibility = Visibility.Collapsed;
                SearchButton.Visibility = Visibility.Visible;
            }
        }
        private async void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                JsonArray array = Tools.GetDataArray(await Tools.GetJson($"/search/suggestSearchWordsNew?searchValue={sender.Text}&type=app"));
                if (array != null && array.Count > 0)
                    sender.ItemsSource = from i in array select new SearchWord(i.GetObject());
                else
                    sender.ItemsSource = null;
            }
        }
        private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion is SearchWord word)
            {
                if (word.Symbol == Symbol.Contact)
                    Tools.Navigate(typeof(SearchPage), new object[] { 1, word.GetTitle() });
                else
                    Tools.Navigate(typeof(SearchPage), new object[] { 0, word.Title });
            }
            else if (args.ChosenSuggestion is null)
                Tools.Navigate(typeof(SearchPage), new object[] { 0, sender.Text });
        }
        private void AutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            if (args.SelectedItem is SearchWord m)
                sender.Text = m.GetTitle();
        }
        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            SearchBox.Visibility = Visibility.Visible;
            SearchButton.Visibility = Visibility.Collapsed;
            SearchBox.Focus(FocusState.Keyboard);
        }
        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (Window.Current.Bounds.Width < 768)
            {
                SearchBox.Visibility = Visibility.Collapsed;
                SearchButton.Visibility = Visibility.Visible;
            }
        }
        #endregion
       */

        private void SearchButton_Click(object sender, RoutedEventArgs e) => UIHelper.Navigate(typeof(SearchPage), new object[] { 0, null });
        private void NotificationCenterButton_Click(object sender, RoutedEventArgs e) => UIHelper.Navigate(typeof(NotificationsPage), NotificationPageType.Comment);

        #region toIndexPage
        string[] followUrls;
        int followIndex;
        async void GetIndexPageItems()
        {
            await SettingsHelper.CheckLoginInfo();
            UIHelper.notifications.BadgeNumberChanged += (sender, e) => { if (sender is NotificationsNum num) ChangeBadgeNum(num.BadgeNum); };
            ChangeBadgeNum(UIHelper.notifications.BadgeNum);

            JArray array = (JArray)await DataHelper.GetData(DataType.GetIndexPageNames);
            if (array != null && array.Count > 0)
            {
                string[] excludedTabs = new[] { "酷品", "看看号", "直播", "视频", "头条" };
                var tempTabList = (from a in array
                                   where a.Value<string>("entityTemplate") == "configCard"
                                   from b in a["entities"] as JArray
                                   where !excludedTabs.Contains(b.Value<string>("title"))
                                   select b).ToArray();
                var FollowList = from a in array
                                 where a.Value<string>("entityTemplate") == "configCard"
                                 from b in a["entities"] as JArray
                                 where b.Value<string>("title") == "关注"
                                 from c in b["entities"] as JArray
                                 where c.Value<string>("entityType") == "page"
                                 select c;
                var TabList = new JToken[tempTabList.Length + 1];
                TabList[0] = JObject.Parse("{title : \"头条\"}");
                for (int i = 1; i < TabList.Length; i++)
                    TabList[i] = tempTabList[i - 1];
                PivotItem j = new PivotItem();
                MainPivot.ItemsSource = TabList.Select(a =>
                {
                    var b = a as JObject;
                    string title = b.Value<string>("title");
                    var p = new PivotItem { Header = title };
                    p.Content = new Frame { Tag = title == "头条" ? "/main/indexV8" : $"{b.Value<string>("url")}&title={title}" };
                    if (title == "关注") j = p;
                    return p;
                });
                followUrls = FollowList.Select(a => a.Value<string>("url")).ToArray();
                followIndex = MainPivot.Items.IndexOf(j);
                PivotItemsComboBox.ItemsSource = FollowList.Select(a => a.Value<string>("title"));
                if (SettingsHelper.Get<int>("DefaultFollowPageIndex") > followUrls.Count()) SettingsHelper.Set("DefaultFollowPageIndex", 0);
                PivotItemsComboBox.SelectedIndex = SettingsHelper.Get<int>("DefaultFollowPageIndex");
            }
        }

        private async void ChangeBadgeNum(double num) =>
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => NotificationCenterButton.Icon = new FontIcon
            {
                FontFamily = new FontFamily("Segoe MDL2 Assets"),
                Glyph = num > 0 ? "" //"&#xED0C;" 
                                : "" //"&#xED0D;" 
            });
        #endregion

        private void PivotItemsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PivotItemsComboBox.SelectedIndex != -1 && MainPivot.SelectedIndex == followIndex && (MainPivot.SelectedItem as PivotItem).Content is Frame f)
            {
                f.Navigate(typeof(IndexPage), new object[] { followUrls[PivotItemsComboBox.SelectedIndex], true });
                SettingsHelper.Set("DefaultFollowPageIndex", PivotItemsComboBox.SelectedIndex);
                Task.Run(async () =>
                {
                    await Task.Delay(1000);
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                        (VisualTree.FindDescendantByName(MainPivot.SelectedItem as PivotItem, "ScrollViewer") as ScrollViewer).ViewChanged += (s, ee) =>
                        {
                            ScrollViewer VScrollViewer = s as ScrollViewer;
                            IndexPage page = f.Content as IndexPage;
                            if (!ee.IsIntermediate && VScrollViewer.VerticalOffset == VScrollViewer.ScrollableHeight && page.CanLoadMore)
                                page.GetUrlPage();
                        });
                });
            }
        }

        private void MainPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MainPivot.SelectedIndex == followIndex && PivotItemsComboBox.SelectedIndex > -1)
            {
                PivotItemsComboBox.Visibility = Visibility.Visible;
                if ((MainPivot.SelectedItem as PivotItem).Content is Frame f && f.Content is null)
                {
                    f.Navigate(typeof(IndexPage), new object[] { followUrls[PivotItemsComboBox.SelectedIndex], true });
                    Task.Run(async () =>
                    {
                        await Task.Delay(1000);
                        await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                            (VisualTree.FindDescendantByName(MainPivot.SelectedItem as PivotItem, "ScrollViewer") as ScrollViewer).ViewChanged += (s, ee) =>
                            {
                                ScrollViewer VScrollViewer = s as ScrollViewer;
                                IndexPage page = f.Content as IndexPage;
                                if (!ee.IsIntermediate && VScrollViewer.VerticalOffset == VScrollViewer.ScrollableHeight && page.CanLoadMore)
                                    page.GetUrlPage();
                            });
                    });
                }
            }
            else
            {
                PivotItemsComboBox.Visibility = Visibility.Collapsed;
                if ((MainPivot.SelectedItem as PivotItem).Content is Frame f && f.Content is null)
                {
                    f.Navigate(typeof(IndexPage), new object[] { f.Tag as string, true });
                    Task.Run(async () =>
                    {
                        await Task.Delay(1000);
                        await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                            (VisualTree.FindDescendantByName(MainPivot.SelectedItem as PivotItem, "ScrollViewer") as ScrollViewer).ViewChanged += (s, ee) =>
                            {
                                ScrollViewer VScrollViewer = s as ScrollViewer;
                                IndexPage page = f.Content as IndexPage;
                                if (!ee.IsIntermediate && VScrollViewer.VerticalOffset == VScrollViewer.ScrollableHeight && page.CanLoadMore)
                                    page.GetUrlPage();
                            });
                    });
                }
            }
        }
    }
}