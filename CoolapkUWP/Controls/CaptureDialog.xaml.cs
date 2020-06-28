using CoolapkUWP.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;

namespace CoolapkUWP.Controls
{
    public sealed partial class CaptchaDialog : ContentDialog
    {
        private string unixTimeSpan;

        public CaptchaDialog()
        {
            this.InitializeComponent();
            SetImage();
        }

        private async void SetImage()
        {
            unixTimeSpan = DataHelper.ConvertTimeToUnix(DateTime.Now);
            image.Source = await DataHelper.GetImageAsync($"https://api.coolapk.com/v6/account/captchaImage?time={unixTimeSpan}&w=192&h=80");
        }

        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            using (var content = new HttpMultipartFormDataContent())
            using (var type = new HttpStringContent(CoolapkMessageException.RequestCaptcha))
            using (var code = new HttpStringContent(textbox.Text))
            {
                content.Add(type, "type");
                content.Add(code, "code");
                UIHelper.ShowMessage((await DataHelper.PostDataAsync(DataUriType.RequestValidate, content)).ToString());
            }
        }

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            SetImage();
        }
    }
}
