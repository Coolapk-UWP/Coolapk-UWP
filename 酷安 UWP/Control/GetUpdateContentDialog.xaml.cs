using CoolapkUWP.Data;
using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using Windows.System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace CoolapkUWP.Control
{
    public sealed partial class GetUpdateContentDialog : ContentDialog
    {
        string url;
        string Body { get; set; }
        public GetUpdateContentDialog(string url,string body)
        {
            this.InitializeComponent();
            this.url = url;
            Body = body;
        }

        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            await Launcher.LaunchUriAsync(new Uri(url));
            Hide();
        }

        private void MarkdownTextBlock_ImageResolving(object sender, ImageResolvingEventArgs e)
        {
            if (Settings.GetBoolen("IsNoPicsMode"))
            {
                e.Handled = true;
                if (Settings.GetBoolen("IsDarkMode"))
                    e.Image = new BitmapImage(new Uri("ms-appx:/Assets/img_placeholder_night.png")) { DecodePixelHeight = 150, DecodePixelWidth = 150 };
                else e.Image = new BitmapImage(new Uri("ms-appx:/Assets/img_placeholder.png")) { DecodePixelHeight = 150, DecodePixelWidth = 150 };
            }
        }
    }
}
