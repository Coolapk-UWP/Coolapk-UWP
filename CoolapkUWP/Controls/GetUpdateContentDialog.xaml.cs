using CoolapkUWP.Helpers;
using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using Windows.System;
using Windows.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace CoolapkUWP.Controls
{
    public sealed partial class GetUpdateContentDialog : ContentDialog
    {
        readonly string url;
        string Body { get; set; }
        public GetUpdateContentDialog(string url, string body)
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

        private async void MarkdownTextBlock_ImageResolving(object sender, ImageResolvingEventArgs e)
        {
            var deferral = e.GetDeferral();
            e.Image = await ImageCacheHelper.GetImage(ImageType.OriginImage, e.Url);
            e.Handled = true;
            deferral.Complete();
        }

        private void MarkdownTextBlock_ImageClicked(object sender, LinkClickedEventArgs e) => UIHelper.ShowImage(e.Link, ImageType.OriginImage);
    }
}
