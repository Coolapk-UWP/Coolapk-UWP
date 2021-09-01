﻿using CoolapkUWP.Control.ViewModels;
using CoolapkUWP.Data;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using Windows.Data.Json;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace CoolapkUWP.Pages
{
    public sealed partial class SearchPage : Page
    {
        private int[] pages = new int[3];
        private string[] lastItems = new string[3];
        public SearchPage()
        {
            InitializeComponent();
            AppsResultList.ItemsSource = new ObservableCollection<AppViewModel>();
            FeedList.ItemsSource = new ObservableCollection<FeedViewModel>();
            UserList.ItemsSource = new ObservableCollection<UserViewModel>();
            TopicList.ItemsSource = new ObservableCollection<TopicViewModel>();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is object[] vs)
            {
                if (!string.IsNullOrEmpty(vs[1] as string))
                {
                    SearchText.Text = vs[1] as string;
                    SearchTypeComboBox.SelectedIndex = Convert.ToInt32(vs[0]);
                }
            }
        }

        private async void SearchFeeds(string keyWord)
        {
            UIHelper.ShowProgressBar();
            string feedType = string.Empty;
            string sortType = string.Empty;
            switch (SearchFeedTypeComboBox.SelectedIndex)
            {
                case 0:
                    feedType = "all";
                    break;
                case 1:
                    feedType = "feed";
                    break;
                case 2:
                    feedType = "feedArticle";
                    break;
                case 3:
                    feedType = "rating";
                    break;
                case 4:
                    feedType = "picture";
                    break;
                case 5:
                    feedType = "question";
                    break;
                case 6:
                    feedType = "answer";
                    break;
                case 7:
                    feedType = "video";
                    break;
                case 8:
                    feedType = "ershou";
                    break;
                case 9:
                    feedType = "vote";
                    break;
                default:
                    break;
            }
            switch (SearchFeedSortTypeComboBox.SelectedIndex)
            {
                case 0:
                    sortType = "default";
                    break;
                case 1:
                    sortType = "hot";
                    break;
                case 2:
                    sortType = "reply";
                    break;
                default:
                    break;
            }
            string r = await UIHelper.GetJson($"/search?type=feed&feedType={feedType}&sort={sortType}&searchValue={keyWord}&page={++pages[0]}{(pages[0] > 1 ? "&lastItem=" + lastItems[0] : string.Empty)}&showAnonymous=-1");
            JsonArray Root = UIHelper.GetDataArray(r);
            ObservableCollection<FeedViewModel> FeedsCollection = FeedList.ItemsSource as ObservableCollection<FeedViewModel>;
            if (pages[0] == 1) { FeedsCollection.Clear(); }
            if (Root.Count != 0)
            {
                lastItems[0] = Root.Last().GetObject()["id"].GetString();
                foreach (IJsonValue i in Root)
                { FeedsCollection.Add(new FeedViewModel(i.GetObject())); }
            }
            else { pages[0]--; }
            UIHelper.HideProgressBar();
        }

        private async void SearchUsers(string keyWord)
        {
            UIHelper.ShowProgressBar();
            ObservableCollection<UserViewModel> infos = UserList.ItemsSource as ObservableCollection<UserViewModel>;
            string r = await UIHelper.GetJson($"/search?type=user&searchValue={keyWord}&page={++pages[1]}{(pages[1] > 1 ? "&lastItem=" + lastItems[1] : string.Empty)}&showAnonymous=-1");
            JsonArray array = UIHelper.GetDataArray(r);
            if (array.Count > 0)
            {
                lastItems[1] = array.Last().GetObject()["uid"].GetString();
                if (infos.Count > 0)
                {
                    UserViewModel[] d = (from a in infos
                                         from b in array
                                         where a.UserName == b.GetObject()["username"].ToString()
                                         select a).ToArray();
                    foreach (UserViewModel item in d)
                    { infos.Remove(item); }
                }
                for (int i = 0; i < array.Count; i++)
                {
                    IJsonValue t = array[i];
                    infos.Add(new UserViewModel(t));
                }
            }
            else { pages[1]--; }
            UIHelper.HideProgressBar();
        }

        private async void SearchTopic(string keyWord)
        {
            UIHelper.ShowProgressBar();
            JsonArray Root = UIHelper.GetDataArray(await UIHelper.GetJson($"/search?type=feedTopic&searchValue={keyWord}&page={++pages[2]}{(pages[2] > 1 ? "&lastItem=" + lastItems[2] : string.Empty)}&showAnonymous=-1"));
            ObservableCollection<TopicViewModel> FeedsCollection = TopicList.ItemsSource as ObservableCollection<TopicViewModel>;
            if (pages[2] == 1) { FeedsCollection.Clear(); }
            if (Root.Count != 0)
            {
                lastItems[2] = Root.Last().GetObject()["id"].GetString();
                foreach (IJsonValue i in Root)
                { FeedsCollection.Add(new TopicViewModel(i)); }
            }
            else { pages[2]--; }
            UIHelper.HideProgressBar();
        }
        #region SearchApp
        private async void SearchApps(string keyWord)
        {
            UIHelper.ShowProgressBar();
            ObservableCollection<AppViewModel> infos = AppsResultList.ItemsSource as ObservableCollection<AppViewModel>;
            infos.Clear();
            try
            {
                string str = await new HttpClient().GetStringAsync(new Uri("https://www.coolapk.com/search?q=" + keyWord));
                string body = Regex.Split(str, @"<div class=""left_nav"">")[1];
                body = Regex.Split(body, @"<div class=""panel-footer ex-card-footer text-center"">")[0];
                //&nbsp;处理
                body = body.Replace("&nbsp;", " ");
                string[] bodylist = Regex.Split(body, @"<a href=""");
                string[] bodys = Regex.Split(body, @"\n");
                for (int i = 0; i < bodylist.Length - 1; i++)
                {
                    infos.Add(new AppViewModel
                    {
                        Url = bodys[i * 15 + 5].Split('"')[1],
                        Icon = new BitmapImage(new Uri(bodys[i * 15 + 5 + 3].Split('"')[3], UriKind.RelativeOrAbsolute)),
                        AppName = bodys[i * 15 + 5 + 5].Split('>')[1].Split('<')[0],
                        Size = bodys[i * 15 + 5 + 6].Split('>')[1].Split('<')[0],
                        DownloadNum = bodys[i * 15 + 5 + 7].Split('>')[1].Split('<')[0]
                    });
                }
                UIHelper.HideProgressBar();
            }
            catch (HttpRequestException e) { UIHelper.ShowHttpExceptionMessage(e); }
            catch { throw; }
        }

        private void AppResultList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AppsResultList.SelectedIndex == -1) { return; }
            UIHelper.Navigate(typeof(AppPages.AppPage), "https://www.coolapk.com" + (AppsResultList.Items[AppsResultList.SelectedIndex] as AppViewModel).Url);
            AppsResultList.SelectedIndex = -1;
        }
        #endregion
        private void SearchTextBox_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter) { SearchText_QuerySubmitted(sender as AutoSuggestBox, null); }
        }

        private void StartSearch()
        {
            if (string.IsNullOrEmpty(SearchText?.Text))
            { DetailPivot.Visibility = Visibility.Collapsed; }
            else
            {
                switch (DetailPivot.SelectedIndex)
                {
                    case 0:
                        SearchFeeds(SearchText.Text);
                        break;
                    case 1:
                        SearchUsers(SearchText.Text);
                        break;
                    case 2:
                        SearchTopic(SearchText.Text);
                        break;
                    case 3:
                        SearchApps(SearchText.Text);
                        break;
                    default:
                        break;
                }
                DetailPivot.Visibility = Visibility.Visible;
            }
        }

        private void Grid_Tapped(object sender, TappedRoutedEventArgs e) => UIHelper.OpenLink((sender as FrameworkElement).Tag as string);

        private void BackButton_Click(object sender, RoutedEventArgs e) => Frame.GoBack();

        private void SearchTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SearchTypeComboBox.SelectedIndex != -1 && !(DetailPivot is null))
            { DetailPivot.SelectedIndex = SearchTypeComboBox.SelectedIndex; }
            if (SearchTypeComboBox.SelectedIndex + 1 == SearchTypeComboBox.Items.Count || pages[SearchTypeComboBox.SelectedIndex] == 0)
            { StartSearch(); }
        }

        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            ScrollViewer viewer = sender as ScrollViewer;
            if (!e.IsIntermediate)
            {
                if (viewer.VerticalOffset == viewer.ScrollableHeight)
                { StartSearch(); }
            }
        }

        private void SearchFeedTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            pages[0] = 0;
            lastItems[0] = string.Empty;
            StartSearch();
        }

        private void SearchText_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            pages = new int[3];
            lastItems = new string[3];
            (FeedList.ItemsSource as ObservableCollection<FeedViewModel>).Clear();
            (UserList.ItemsSource as ObservableCollection<UserViewModel>).Clear();
            (AppsResultList.ItemsSource as ObservableCollection<AppViewModel>).Clear();
            (TopicList.ItemsSource as ObservableCollection<TopicViewModel>).Clear();
            StartSearch();
        }

        private async void SearchText_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                JsonArray array = UIHelper.GetDataArray(await UIHelper.GetJson($"/search/suggestSearchWordsNew?searchValue={sender.Text}&type=app"));
                if (array != null && array.Count > 0)
                {
                    ObservableCollection<object> observableCollection = new ObservableCollection<object>();
                    sender.ItemsSource = observableCollection;
                    foreach (IJsonValue ite in array)
                    {
                        JsonObject item = ite.GetObject();
                        switch (item["entityType"].GetString())
                        {
                            case "apk":
                                observableCollection.Add(new AppViewModel
                                {
                                    AppName = item["title"].GetString(),
                                    DownloadNum = $"{item["score"].GetString()}分 {item["downCount"].ToString().Replace("\"", string.Empty)}下载",
                                    Url = item["url"].GetString(),
                                    Icon = await ImageCache.GetImage(ImageType.Icon, item["logo"].GetString()),
                                    Size = item["apksize"].GetString(),
                                });
                                break;
                            case "searchWord":
                            default:
                                Symbol s = Symbol.Find;
                                if (item["logo"].GetString().Contains("cube")) { s = Symbol.Shop; }
                                else if (item["logo"].GetString().Contains("xitongguanli")) { s = Symbol.AllApps; }
                                observableCollection.Add(new SearchWord { Symbol = s, Title = item["title"].GetString() });
                                break;
                        }
                    }
                }
            }
        }

        private void SearchText_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                SearchText_QuerySubmitted(sender as AutoSuggestBox, null);
            }
        }
    }
}
