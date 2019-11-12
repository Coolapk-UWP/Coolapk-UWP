using CoolapkUWP.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace CoolapkUWP.Control
{
    public sealed partial class ShowImageControl : UserControl
    {
        string url;
        Popup popup;
        public ShowImageControl(string url, Popup popup)
        {
            this.InitializeComponent();
            Height = Window.Current.Bounds.Height;
            Width = Window.Current.Bounds.Width;
            Window.Current.SizeChanged += WindowSizeChanged;
            popup.Closed += (s, e) => Window.Current.SizeChanged -= WindowSizeChanged;
            this.popup = popup;
            this.url = url;
            if (string.IsNullOrEmpty(url)) popup.Hide();
            ScrollViewerMain.ChangeView(null, null, 1);
            image.Source = new BitmapImage(new Uri(url));
        }

        private void WindowSizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            Height = e.Size.Height;
            Width = e.Size.Width;
        }

        private void CloseFlip_Click(object sender, RoutedEventArgs e) => popup.Hide();

        private void UserControl_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Escape)
            {
                e.Handled = true;
                popup.Hide();
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
