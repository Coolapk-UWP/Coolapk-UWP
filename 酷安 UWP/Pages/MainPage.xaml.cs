using CoolapkUWP.Control.ViewModels;
using CoolapkUWP.Data;
using CoolapkUWP.Pages.FeedPages;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Data.Json;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

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
            if (Settings.Get<bool>("CheckUpdateWhenLuanching")) Settings.CheckUpdate();
            SystemNavigationManager.GetForCurrentView().BackRequested += (sender, ee) =>
            {
                int i = Settings.HasStatusBar ? Tools.popups.Count - 1 : Tools.popups.Count - 2;
                if (i >= 0)
                {
                    ee.Handled = true;
                    Windows.UI.Xaml.Controls.Primitives.Popup popup = Tools.popups[i];
                    popup.IsOpen = false;
                    Tools.popups.Remove(popup);
                }
                else if (Frame.CanGoBack)
                {
                    ee.Handled = true;
                    Frame.GoBack();
                }
            };
            Windows.UI.Notifications.TileUpdateManager.CreateTileUpdaterForApplication().Clear();
            Tools.mainPage = this;
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

        private void SearchButton_Click(object sender, RoutedEventArgs e) => Tools.Navigate(typeof(SearchPage), new object[] { 0, null });
        private void NotificationCenterButton_Click(object sender, RoutedEventArgs e) => Tools.Navigate(typeof(NotificationsPage), NotificationPageType.Comment);

        #region toIndexPage
        string[] followUrls;
        int followIndex;
        async void GetIndexPageItems()
        {
            await Settings.CheckLoginInfo();
            Tools.notifications.BadgeNumberChanged += (sender, e) => { if (sender is NotificationsNum num) ChangeBadgeNum(num.BadgeNum); };
            ChangeBadgeNum(Tools.notifications.BadgeNum);

            JsonArray array = Tools.GetDataArray(await Tools.GetJson("/main/init"));
            if (array != null && array.Count > 0)
            {
                string[] excludedTabs = new[] { "酷品", "看看号", "直播", "视频" };
                var TabList = from a in array
                              where a.GetObject()["entityTemplate"].GetString() == "configCard"
                              from b in a.GetObject()["entities"].GetArray()
                              where !excludedTabs.Contains(b.GetObject()["title"].GetString())
                              select b;
                var FollowList = from a in array
                                 where a.GetObject()["entityTemplate"].GetString() == "configCard"
                                 from b in a.GetObject()["entities"].GetArray()
                                 where b.GetObject()["title"].GetString() == "关注"
                                 from c in b.GetObject()["entities"].GetArray()
                                 where c.GetObject()["entityType"].GetString() == "page"
                                 select c;
                PivotItem i, j;
                i = j = new PivotItem();
                MainPivot.ItemsSource = TabList.Select(a =>
                {
                    var b = a.GetObject();
                    string title = b["title"].GetString();
                    var p = new PivotItem { Header = title };
                    p.Content = new Frame { Tag = title == "头条" ? "/main/indexV8" : $"{b["url"].GetString()}&title={title}" };
                    if (title == "头条") i = p;
                    else if (title == "关注") j = p;
                    return p;
                });
                MainPivot.SelectedIndex = MainPivot.Items.IndexOf(i);
                followUrls = FollowList.Select(a => a.GetObject()["url"].GetString()).ToArray();
                followIndex = MainPivot.Items.IndexOf(j);
                PivotItemsComboBox.ItemsSource = FollowList.Select(a => a.GetObject()["title"].GetString());
                if (Settings.Get<int>("DefaultFollowPageIndex") > followUrls.Count()) Settings.Set("DefaultFollowPageIndex", 0);
                PivotItemsComboBox.SelectedIndex = Settings.Get<int>("DefaultFollowPageIndex");
            }
        }

        private async void ChangeBadgeNum(double num)
         => await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => NotificationCenterButton.Icon = new FontIcon
         {
             FontFamily = new FontFamily("Segoe MDL2 Assets"),
             Glyph = num > 0 ? "" //"&#xED0C;" 
                             : "" //"&#xED0D;" 
         });
        #endregion

        private void PivotItemsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PivotItemsComboBox.SelectedIndex != -1 && MainPivot.SelectedIndex == 0 && (MainPivot.SelectedItem as PivotItem).Content is Frame f)
            {
                f.Navigate(typeof(IndexPage), new object[] { followUrls[PivotItemsComboBox.SelectedIndex], true });
                Settings.Set("DefaultFollowPageIndex", PivotItemsComboBox.SelectedIndex);
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

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }
    }
}