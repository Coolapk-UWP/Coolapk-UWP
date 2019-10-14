using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using 酷安_UWP.UsersPage;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace 酷安_UWP
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class TestPage : Page
    {
        MainPage mainPage;

        public TestPage()
        {
            this.InitializeComponent();
            //Button_Click(null, null);
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            mainPage = e.Parameter as MainPage;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            mainPage.Frame.Navigate(typeof(UserPage), new object[] { await CoolApkSDK.GetUserIDByName(uid.Text), mainPage });
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            object[] vs = await Web.CheckUpdate();
            MessageDialog dialog = new MessageDialog(vs[1] as string);
            await dialog.ShowAsync();
        }

        private async void Button_Click_2(object sender, RoutedEventArgs e)
        {
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            string str = folder.Path + "\\" + @"\Emoji\[酷币2€].png";
            FileInfo file = new FileInfo(str);
            string a = file.Exists ? "y" : "n";
            MessageDialog dialog = new MessageDialog(a);
            await dialog.ShowAsync();
        }
    }
}
