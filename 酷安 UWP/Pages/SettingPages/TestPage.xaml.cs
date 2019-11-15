using CoolapkUWP.Control.ViewModels;
using CoolapkUWP.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace CoolapkUWP.Pages.SettingPages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class TestPage : Page
    {
        public TestPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            List<BitmapImage> v = new List<BitmapImage>();
            foreach (var item in await (await ApplicationData.Current.LocalCacheFolder.GetFolderAsync("SmallImage")).GetFilesAsync())
            {
                BitmapImage b = new BitmapImage();
                await b.SetSourceAsync(await item.OpenReadAsync());
                v.Add(b);
            }
            lis.ItemsSource = v;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            Tools.Navigate(typeof(FeedPages.UserPage), await Tools.GetUserIDByName(uid.Text));
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
                Frame.GoBack();
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            Tools.ShowMessage(message.Text);
        }

        private async void Button_Click_5(object sender, RoutedEventArgs e)
        {
            imagea.Source = await ImageCache.GetImage(ImageType.SmallImage, url.Text, true);
            //await new MessageDialog(await Tools.GetJson(url.Text)).ShowAsync();
        }
    }
}