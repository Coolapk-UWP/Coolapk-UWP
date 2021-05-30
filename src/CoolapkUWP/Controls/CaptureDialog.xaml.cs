using CoolapkUWP.Core.Helpers;
using CoolapkUWP.Helpers;
using System;
using System.Net.Http;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace CoolapkUWP.Controls
{
    public sealed partial class CaptchaDialog : ContentDialog
    {
        private string unixTimeSpan;

        public CaptchaDialog()
        {
            InitializeComponent();
            SetImage();
        }

        private async void SetImage()
        {
            unixTimeSpan = $"{Utils.ConvertDateTimeToUnixTimeStamp(DateTime.Now)}";
            image.Source = await DataHelper.GetImageAsync($"https://api.coolapk.com/v6/account/captchaImage?time={unixTimeSpan}&w=192&h=80");
        }

        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            image.Source = null;
            _ = ImageCacheHelper.CleanCaptchaCacheAsync();
            using (MultipartFormDataContent content = new MultipartFormDataContent())
            using (StringContent type = new StringContent(Core.Exceptions.CoolapkMessageException.RequestCaptcha))
            using (StringContent code = new StringContent(textbox.Text))
            {
                content.Add(type, "type");
                content.Add(code, "code");
                (bool _, Newtonsoft.Json.Linq.JToken t) = await DataHelper.PostDataAsync(UriHelper.GetUri(UriType.RequestValidate), content);
                UIHelper.StatusBar_ShowMessage(t.ToString());
            }
        }

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            SetImage();
        }
    }
}