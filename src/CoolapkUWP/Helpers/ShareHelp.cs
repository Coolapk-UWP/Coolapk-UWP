using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoolapkUWP.Helpers;
using CoolapkUWP.Pages.FeedPages;
using CoolapkUWP.ViewModels.FeedListPage;
using Windows.ApplicationModel.DataTransfer;

namespace CoolapkUWP.Helpers
{
    public sealed partial class ShareHelper
    {
        public string Uri;
        private string Type;

        public async void Share(String Text, String Title, String Description, String Type)
        {
            DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += IndexPage_DataRequested;
        }

        void IndexPage_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            //分享一个链接
            Uri shareLinkString = ValidateAndGetUri(Uri);
            if (shareLinkString != null)
            {
                //创建一个数据包
                DataPackage dataPackage = new DataPackage();

                //把要分享的链接放到数据包里
                dataPackage.SetWebLink(shareLinkString);

                //数据包的标题（内容和标题必须提供）
                dataPackage.Properties.Title = "动态分享";
                //数据包的描述
                dataPackage.Properties.Description = "链接";

                //给dataRequest对象赋值
                DataRequest request = args.Request;
                request.Data = dataPackage;
            }
            else
            {
                DataRequest request = args.Request;
                request.FailWithDisplayText("分享失败");
            }
        }

        private Uri ValidateAndGetUri(string uriString)
        {
            Uri uri = null;
            try
            {
                uri = new Uri(uriString);
            }
            finally { }
            return uri;
        }
    }
}
