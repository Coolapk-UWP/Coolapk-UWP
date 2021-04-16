using System.Net.Http;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace CoolapkUWP.Pages.AppPages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class DevPage : Page
    {
        public DevPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                GetDevMyList(await new HttpClient().GetStringAsync("https://developer.coolapk.com/do?c=apk&m=myList"));
            }
            catch (HttpRequestException ex) { }
            catch { throw; }
        }

        #region Task：任务

        private void GetDevMyList(string str)
        {

        }


        #endregion

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
                Frame.GoBack();
        }

        private void TitleBar_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
