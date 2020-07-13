using QRCoder;
using System;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

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
                RefreshQRCode();
            }
        }

        private async void RefreshQRCode()
        {
            using (var qrGenerator = new QRCodeGenerator())
            {
                var qrCodeData = qrGenerator.CreateQrCode(QRCodeText, QRCodeGenerator.ECCLevel.Q);
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