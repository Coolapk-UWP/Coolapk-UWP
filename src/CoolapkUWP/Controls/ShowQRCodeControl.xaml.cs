using QRCoder;
using CoolapkUWP.Helpers;
using System;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.ApplicationModel.DataTransfer;

namespace CoolapkUWP.Controls
{
    public sealed partial class ShowQRCodeControl : UserControl
    {
        private string qrCodeText;

        public string QRCodeText
        {
            get => qrCodeText;
            set
            {
                qrCodeText = value;
                //UIHelper.ShowMessage(qrCodeText);
                GetShare();
                RefreshQRCode();
            }
        }

        void FeedPage_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            System.Uri shareLinkString = ValidateAndGetUri(qrCodeText);
            if (shareLinkString != null)
            {
                DataPackage dataPackage = new DataPackage();
                dataPackage.SetWebLink(shareLinkString);
                dataPackage.Properties.Title = "动态分享";
                dataPackage.Properties.Description = "链接";
                DataRequest request = args.Request;
                request.Data = dataPackage;
            }
            else
            {
                DataRequest request = args.Request;
                request.FailWithDisplayText("分享失败");
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
            }
            return uri;
        }

        protected void GetShare()
        {
            DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += FeedPage_DataRequested;
        }

        private async void RefreshQRCode()
        {
            using (var qrGenerator = new QRCodeGenerator())
            {
                var qrCodeData = qrGenerator.CreateQrCode("https://www.coolapk.com", QRCodeGenerator.ECCLevel.Q);
                if (QRCodeText != null)
                    qrCodeData = qrGenerator.CreateQrCode(QRCodeText, QRCodeGenerator.ECCLevel.Q);
                using (var qrCodeBmp = new PngByteQRCode(qrCodeData))
                {
                    byte[] qrCodeImageBmp = qrCodeBmp.GetGraphic(
                        20,
                        new byte[] { 0, 0, 0, 0xFF },
                        new byte[] { 0xFF, 0xFF, 0xFF, 0xFF });
                    using (var stream = new InMemoryRandomAccessStream())
                    {
                        using (var writer = new DataWriter(stream.GetOutputStreamAt(0)))
                        {
                            writer.WriteBytes(qrCodeImageBmp);
                            await writer.StoreAsync();
                        }
                        var image = new BitmapImage();
                        await image.SetSourceAsync(stream);

                        qrCodeImage.Source = image;
                    }
                }
            }
        }

        public ShowQRCodeControl()
        {
            this.InitializeComponent();
        }
    }
}