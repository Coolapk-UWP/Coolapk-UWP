using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace CoolapkUWP.Control
{
    public sealed partial class ShowImageControl : UserControl
    {
        bool _Hide;
        string url;
        public bool Hide
        {
            get => _Hide;
            set
            {
                Visibility = value ? Visibility.Collapsed : Visibility.Visible;
                _Hide = value;
            }
        }

        public ShowImageControl()
        {
            this.InitializeComponent();
            Hide = true;
        }

        public void ShowImage(string url)
        {
            this.url = url;
            if (string.IsNullOrEmpty(url)) return;
            Hide = false;
            ScrollViewerMain.ChangeView(null, null, 1);
            image.Source = new BitmapImage(new Uri(url));
        }

        private void CloseFlip_Click(object sender, RoutedEventArgs e) => Hide = true;

        private void UserControl_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Escape)
            {
                e.Handled = true;
                Hide = true;
            }
        }

        private async void SaveImage_Click(object sender, RoutedEventArgs e)
        {
            string fileName = url.Substring(url.LastIndexOf('/') + 1);
            FileSavePicker fileSavePicker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.PicturesLibrary,
                SuggestedFileName = fileName.Replace(fileName.Substring(fileName.LastIndexOf('.')), string.Empty)
            };
            fileSavePicker.FileTypeChoices.Add($"{fileName.Substring(fileName.LastIndexOf('.') + 1)}文件", new List<string>() { $"{fileName.Substring(fileName.LastIndexOf('.')) }" });
            StorageFile file = await fileSavePicker.PickSaveFileAsync();
            if (!(file is null))
            {
                HttpClient httpClient = new HttpClient();
                using (Stream s = await httpClient.GetStreamAsync(url))
                using (Stream fs = await file.OpenStreamForWriteAsync())
                    await s.CopyToAsync(fs);
            }
        }

        private void ScrollViewerMain_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (ScrollViewerMain.ZoomFactor != 2) ScrollViewerMain.ChangeView(null, null, 2);
            else ScrollViewerMain.ChangeView(null, null, 1);
        }
    }
}
