using CoolapkUWP.Data;
using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text.RegularExpressions;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace CoolapkUWP.Pages.AppPages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class AppRecommendPage : Page
    {
        public AppRecommendPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter != null)
            {
                Pivot1.IsLocked = true;
                Pivot1.SelectedIndex = (int)e.Parameter;
            }
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                //网络信息
                //LoadNewUpdate(await new HttpClient().GetStringAsync("https://www.coolapk.com/"));
                Tools.ShowProgressBar();
                LoadHotApp(await new HttpClient().GetStringAsync("https://www.coolapk.com/apk/recommend"));
                LoadDeveloperApp(await new HttpClient().GetStringAsync("https://www.coolapk.com/apk/developer"));
                LoadHotGame(await new HttpClient().GetStringAsync("https://www.coolapk.com/game/"));
                LoadHotGame(await new HttpClient().GetStringAsync("https://www.coolapk.com/game?p=2"));
                LoadHotGame(await new HttpClient().GetStringAsync("https://www.coolapk.com/game?p=3"));
            }
            catch (HttpRequestException ex) { Tools.ShowHttpExceptionMessage(ex); }
            catch { throw; }
        }

        #region Task：任务

        private void LoadHotGame(string str)
        {
            //绑定一个列表
            ObservableCollection<AppData> gameCollection = new ObservableCollection<AppData>();
            gamelist.ItemsSource = gameCollection;

            //循环添加AppData
            String bod = Regex.Split(str, @"<div class=""applications"">")[1];
            for (int i = 0; i < 4; i++)
            {
                AppData date = new AppData()
                {
                    Tag = "/game/" + Regex.Split(bod, "game/")[i + 1].Split('"')[0],
                    Thumbnail = new Uri(Regex.Split(bod, "src=")[i + 1].Split('"')[1], UriKind.RelativeOrAbsolute),
                    Title = Regex.Split(bod, @"sp-name""")[i + 1].Split('>')[1].Split('<')[0],
                    Describe = Regex.Split(bod, @"sp-time""")[i + 1].Split('>')[1].Split('<')[0]
                };
                gameCollection.Add(date);
            }


            //循环添加AppData
            String body = Regex.Split(str, @"<div class=""game_left_three"">")[1];
            String[] bodys = Regex.Split(body, @"\n");
            for (int i = 0; i < 9; i++)
            {
                AppData date = new AppData()
                {
                    Tag = bodys[i * 15 + 6].Split('"')[1].Split('/')[2],
                    Thumbnail = new Uri(bodys[i * 15 + 6 + 3].Split('"')[3], UriKind.RelativeOrAbsolute),
                    Title = Tools.ReplaceHtml(bodys[i * 15 + 6 + 5].Split('>')[1].Split('<')[0]),
                    Describe = Tools.ReplaceHtml(bodys[i * 15 + 6 + 7].Split('>')[1].Split('<')[0])
                };
                gameCollection.Add(date);
            }

        }

        private void LoadHotApp(String str)
        {
            //绑定一个列表
            ObservableCollection<AppData> hotCollection = new ObservableCollection<AppData>();
            hotview.ItemsSource = hotCollection;


            //循环添加AppData
            String body = Regex.Split(str, @"<div class=""app_list_left"">")[1];
            String[] bodys = Regex.Split(body, @"\n");
            for (int i = 0; i < 20; i++)
            {
                AppData date = new AppData()
                {
                    Tag = bodys[i * 15 + 5].Split('"')[1],
                    Thumbnail = new Uri(bodys[i * 15 + 5 + 3].Split('"')[3], UriKind.RelativeOrAbsolute),
                    Title = bodys[i * 15 + 5 + 5].Split('>')[1].Split('<')[0],
                    Describe = bodys[i * 15 + 5 + 7].Split('>')[1].Split('<')[0].Split(' ')[0]
                };
                hotCollection.Add(date);
            }
        }

        private void LoadDeveloperApp(String str)
        {
            //绑定一个列表
            ObservableCollection<AppData> developerCollection = new ObservableCollection<AppData>();
            developerview.ItemsSource = developerCollection;


            //循环添加AppData
            String body = Regex.Split(str, @"<div class=""left_nav"">")[1];
            String[] bodys = Regex.Split(body, @"\n");
            for (int i = 0; i < 20; i++)
            {
                AppData date = new AppData()
                {
                    Tag = bodys[i * 15 + 4].Split('"')[1],
                    Thumbnail = new Uri(bodys[i * 15 + 4 + 3].Split('"')[3], UriKind.RelativeOrAbsolute),
                    Title = bodys[i * 15 + 4 + 5].Split('>')[1].Split('<')[0],
                    Describe = bodys[i * 15 + 4 + 9].Split('>')[1].Split('<')[0].Replace("开发者:", "")
                };
                developerCollection.Add(date);
            }
        }

        #endregion

        private void Classify_Click(object sender, RoutedEventArgs e)
            => Tools.Navigate(typeof(SearchPage), new object[] { 3, ((Button)sender).Content.ToString() });

        private void Updateview_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is AppData data)
                OpenAppPage("https://www.coolapk.com" + data.Tag);
        }

        public void OpenAppPage(string link) => Tools.Navigate(typeof(AppPage), link);

        private void Gamelist_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is AppData date)
                OpenAppPage("https://www.coolapk.com/apk/" + date.Tag);
        }
    }
}
