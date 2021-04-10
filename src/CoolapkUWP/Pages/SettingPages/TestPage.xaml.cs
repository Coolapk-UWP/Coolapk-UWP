using CoolapkUWP.Helpers;
using CoolapkUWP.Pages.FeedPages;
using CoolapkUWP.ViewModels.FeedListPage;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace CoolapkUWP.Pages.SettingPages
{
    public sealed partial class TestPage : Page
    {
        string Url = "/feed/";
        int i = 0;

        public TestPage()
        {
            this.InitializeComponent();
            //var loader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView("EmojiId");
            //System.Diagnostics.Debug.WriteLine(
            //loader.GetString("?")
            //    );
        }

        void IndexPage_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            //分享一个链接
            System.Uri shareLinkString = ValidateAndGetUri(url.Text);
            if (shareLinkString != null)
            {
                //创建一个数据包
                DataPackage dataPackage = new DataPackage();

                //把要分享的链接放到数据包里
                dataPackage.SetWebLink(shareLinkString);

                //数据包的标题（内容和标题必须提供）
                dataPackage.Properties.Title = "链接分享测试";
                //数据包的描述
                dataPackage.Properties.Description = url.Text;

                //给dataRequest对象赋值
                DataRequest request = args.Request;
                request.Data = dataPackage;
            }
            else
            {
                DataPackage dataPackage = new DataPackage();
                dataPackage.SetText(url.Text);
                dataPackage.Properties.Title = "内容分享测试";
                dataPackage.Properties.Description = "内含文本";
                DataRequest request = args.Request;
                request.Data = dataPackage;
            }
        }

        private System.Uri ValidateAndGetUri(string uriString)
        {
            System.Uri uri = null;
            try
            {
                uri = new System.Uri(uriString);
            }
            catch (System.FormatException)
            {
                UIHelper.ShowMessage(url.Text + "并不是一个链接");
            }
            return uri;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var f = FeedListPageViewModelBase.GetProvider(FeedListType.UserPageList, await Core.Helpers.NetworkHelper.GetUserIDByNameAsync(uid.Text));
            if (f != null)
                UIHelper.NavigateInSplitPane(typeof(FeedListPage), f);
        }

        private void IDComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Add "using Windows.UI;" for Color and Colors.
            string Type = e.AddedItems[0].ToString();
            switch (Type)
            {
                case "动态":
                    Url = "/feed/";
                    i = 0;
                    break;
                case "酷图":
                    Url = "/picture/";
                    i = 0;
                    break;
                case "问答":
                    Url = "/question/";
                    i = 0;
                    break;
                case "用户":
                    Url = "/u/";
                    i = 0;
                    break;
                case "话题":
                    Url = "/t/";
                    i = 0;
                    break;
                case "应用":
                    i = 1;
                    break;
                case "数码":
                    Url = "/product/";
                    i = 0;
                    break;
                case "看看号":
                    Url = "/dyh/";
                    i = 0;
                    break;
                case "收藏集":
                    Url = "/collection/";
                    i = 0;
                    break;
                default:
                    Url = "/feed/";
                    i = 0;
                    break;
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
                Frame.GoBack();
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            UIHelper.ShowMessage(message.Text);
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            UIHelper.Navigate(typeof(BrowserPage), new object[] { false, "http://www.all-tool.cn/Tools/ua/" });
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            UIHelper.Navigate(typeof(Pages.BrowserPage), new object[] { false, "https://m.coolapk.com/mp/do?c=userDevice&m=myDevice" });
        }

        private void Button_Click_0(object sender, RoutedEventArgs e)
        {
            UIHelper.OpenLinkAsync(url.Text);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SettingPageNew));
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            if (i == 1)
            {
                var f = FeedListPageViewModelBase.GetProvider(FeedListType.AppPageList, ID.Text);
                if (f != null)
                {
                    UIHelper.NavigateInSplitPane(typeof(FeedListPage), f);
                }
            }
            else UIHelper.OpenLinkAsync(Url + ID.Text);
        }

        protected void ShowUIButton_Click(object sender, RoutedEventArgs e)
        {
            DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();

            dataTransferManager.DataRequested += IndexPage_DataRequested;

            DataTransferManager.ShowShareUI();
        }

        private void TitleBar_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}