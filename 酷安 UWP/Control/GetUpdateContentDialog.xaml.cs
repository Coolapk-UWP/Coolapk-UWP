using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace 酷安_UWP
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
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            if (Convert.ToBoolean(localSettings.Values["IsNoPicsMode"]))
            {
                e.Handled = true;
                if (Convert.ToBoolean(localSettings.Values["IsDarkMode"]))
                    e.Image = new BitmapImage(new Uri("ms-appx:/Assets/img_placeholder_night.png")) { DecodePixelHeight = 150, DecodePixelWidth = 150 };
                else e.Image = new BitmapImage(new Uri("ms-appx:/Assets/img_placeholder.png")) { DecodePixelHeight = 150, DecodePixelWidth = 150 };
            }
        }
    }
}
