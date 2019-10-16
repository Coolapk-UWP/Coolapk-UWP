using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace 酷安_UWP
{
    public sealed partial class GetUpdateContentDialog : ContentDialog
    {
        string updateBrowserDownloadUrl;
        string Body { get; set; }
        public GetUpdateContentDialog(string BrowserDownloadUrl,string body)
        {
            this.InitializeComponent();
            updateBrowserDownloadUrl = BrowserDownloadUrl;
            Body = body;
        }

        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            await Launcher.LaunchUriAsync(new Uri(updateBrowserDownloadUrl));
            Hide();
        }
    }
}
