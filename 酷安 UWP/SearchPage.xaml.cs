using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace 酷安_UWP
{
    public sealed partial class SearchPage : Page
    {
        MainPage mainPage;
        public SearchPage()
        {
            this.InitializeComponent();
        }
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is MainPage page)
            {
                mainPage = page;
                BackButton.Visibility = Visibility.Collapsed;
            }
            if (e.Parameter is object[] vs)
            {
                mainPage = vs[1] as MainPage;
                string appSearchLink = vs[0] as string;
                if (string.IsNullOrEmpty(appSearchLink)) return;
                await SearchApps(appSearchLink);
            }
            SearchTypeComboBox.Visibility = Visibility.Collapsed;
            DetailPivot.SelectedIndex = 2;
            DetailPivot.IsLocked = true;
        }

        private async Task SearchApps(string str)
        {
            AppsResultList.ItemsSource = null;
            LoadAppsInfo(await Web.GetHttp("https://www.coolapk.com/search?q=" + str));
        }

        private void LoadAppsInfo(String str)
        {
            mainPage.ActiveProgressRing();
            string body = Regex.Split(str, @"<div class=""left_nav"">")[1];
            body = Regex.Split(body, @"<div class=""panel-footer ex-card-footer text-center"">")[0];
            //&nbsp;处理
            body = body.Replace("&nbsp;", " ");
            string[] bodylist = Regex.Split(body, @"<a href=""");
            string[] bodys = Regex.Split(body, @"\n");
            List<AppInfo> infos = new List<AppInfo>();
            for (int i = 0; i < bodylist.Length - 1; i++)
            {
                infos.Add(new AppInfo
                {
                    GridTag = bodys[i * 15 + 5].Split('"')[1],
                    Icon = new BitmapImage(new Uri(bodys[i * 15 + 5 + 3].Split('"')[3], UriKind.RelativeOrAbsolute)),
                    AppName = bodys[i * 15 + 5 + 5].Split('>')[1].Split('<')[0],
                    Size = bodys[i * 15 + 5 + 6].Split('>')[1].Split('<')[0],
                    DownloadNum = bodys[i * 15 + 5 + 7].Split('>')[1].Split('<')[0]
                });
            }
            AppsResultList.ItemsSource = infos;
            mainPage.DeactiveProgressRing();
        }

        private void SearchList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AppsResultList.SelectedIndex == -1) return;

            mainPage.Frame.Navigate(typeof(AppPage), new object[] { "https://www.coolapk.com" + ((AppsResultList.Items[AppsResultList.SelectedIndex]) as AppInfo).GridTag, mainPage });
            AppsResultList.SelectedIndex = -1;
        }


        private void SearchTextBox_KeyUp(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
                SearchButton_Click(null, null);
        }


        private async void SearchButton_Click(object sender, RoutedEventArgs e) => await SearchApps(SearchText.Text);

        private void BackButton_Click(object sender, RoutedEventArgs e) => Frame.GoBack();

        private void SearchTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SearchTypeComboBox.SelectedIndex != -1 &&!(DetailPivot is null))
                DetailPivot.SelectedIndex = SearchTypeComboBox.SelectedIndex;
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (BackButton.Visibility == Visibility.Collapsed && e.NewSize.Width >= 640)
                BackButtonColumnDefinition.Width = new GridLength(0);
            else if (BackButton.Visibility == Visibility.Collapsed && e.NewSize.Width < 640)
                BackButtonColumnDefinition.Width = new GridLength(48);
        }

        private void DetailPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DetailPivot.SelectedIndex != -1 && !(SearchTypeComboBox is null))
                SearchTypeComboBox.SelectedIndex = DetailPivot.SelectedIndex;
        }
    }
    class AppInfo
    {
        public ImageSource Icon { get; set; }
        public string GridTag { get; set; }
        public string AppName { get; set; }
        public string Size { get; set; }
        public string DownloadNum { get; set; }
    }
}
