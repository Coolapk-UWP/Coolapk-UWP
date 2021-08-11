﻿using CoolapkUWP.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace CoolapkUWP.Control
{
    //https://www.cnblogs.com/arcsinw/p/8638526.html
    public sealed partial class ShowImageControl : UserControl
    {
        private class ImageData
        {
            public ImageData(ImageType type, string url)
            {
                Type = type;
                Url = url;
            }
            public void ChangeType()
            {
                if (Type == ImageType.SmallAvatar) { Type = ImageType.BigAvatar; }
                else if (Type == ImageType.SmallImage) { Type = ImageType.OriginImage; }
            }
            public async Task<ImageSource> GetImage() => await ImageCache.GetImage(Type, Url, true);
            public ImageType Type { get; private set; }
            public string Url { get; set; }
        }

        private readonly Popup popup;
        private readonly List<ImageData> datas = new List<ImageData>();
        private readonly ObservableCollection<ImageSource> Images = new ObservableCollection<ImageSource>();

        public ShowImageControl(Popup popup)
        {
            InitializeComponent();
            ShowImageGrid.Margin = SettingsHelper.HasStatusBar ? ApplicationView.GetForCurrentView().Orientation == ApplicationViewOrientation.Portrait ? new Thickness(0, 24, 0, 0) : new Thickness(40, 0, 0, 0) : new Thickness(0, 0, 0, 0);
            Height = Window.Current.Bounds.Height;
            Width = Window.Current.Bounds.Width;
            Window.Current.SizeChanged += WindowSizeChanged;
            popup.Closed += (s, e) => Window.Current.SizeChanged -= WindowSizeChanged;
            this.popup = popup;
        }

        public void ShowImage(string url, ImageType type)
        {
            if (url.Substring(url.LastIndexOf('.')).ToLower().Contains("gif"))
            {
                if (type == ImageType.SmallImage)
                { datas.Add(new ImageData(ImageType.OriginImage, url)); }
                else
                { datas.Add(new ImageData(ImageType.BigAvatar, url)); }
            }
            else { datas.Add(new ImageData(type, url)); }
            Images.Add(null);
        }

        public void ShowImages(string[] urls, ImageType type, int index)
        {
            for (int i = 0; i < urls.Length; i++)
            {
                if (urls[i].EndsWith(".gif") || urls[i].EndsWith(".GIF"))
                {
                    if (type == ImageType.SmallImage)
                    { datas.Add(new ImageData(ImageType.OriginImage, urls[i])); }
                    else { datas.Add(new ImageData(ImageType.BigAvatar, urls[i])); }
                }
                else { datas.Add(new ImageData(type, urls[i])); }
                Images.Add(null);
            }
            _ = Task.Run(async () =>
              {
                  await Task.Delay(20);
                  await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => SFlipView.SelectedIndex = index);
              });
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

        private void ScrollViewerMain_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            ScrollViewer ScrollViewerMain = sender as ScrollViewer;
            _ = ScrollViewerMain.ZoomFactor != 2 ? ScrollViewerMain.ChangeView(null, null, 2) : ScrollViewerMain.ChangeView(null, null, 1);
        }

        private void ScrollViewerMain_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            ScrollViewer scrollViewer = (sender as FrameworkElement).Parent as ScrollViewer;
            _ = scrollViewer.ChangeView(scrollViewer.HorizontalOffset - e.Delta.Translation.X * scrollViewer.ZoomFactor, scrollViewer.VerticalOffset - e.Delta.Translation.Y * scrollViewer.ZoomFactor, null);
        }

        private void SFlipView_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            ((SFlipView.ContextFlyout as MenuFlyout).Items[0] as MenuFlyoutItem).Visibility = datas[SFlipView.SelectedIndex].Type == ImageType.BigAvatar || datas[SFlipView.SelectedIndex].Type == ImageType.OriginImage
                ? Visibility.Collapsed
                : Visibility.Visible;
            SFlipView.ContextFlyout.ShowAt(SFlipView);
        }

        private async void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem item = sender as MenuFlyoutItem;
            datas[SFlipView.SelectedIndex].ChangeType();
            Images[SFlipView.SelectedIndex] = await datas[SFlipView.SelectedIndex].GetImage();
            switch (item.Tag as string)
            {
                case "save":
                    string u = datas[SFlipView.SelectedIndex].Url;
                    string fileName = u.Substring(u.LastIndexOf('/') + 1);
                    FileSavePicker fileSavePicker = new FileSavePicker
                    {
                        SuggestedStartLocation = PickerLocationId.PicturesLibrary,
                        SuggestedFileName = fileName.Replace(fileName.Substring(fileName.LastIndexOf('.')), string.Empty)
                    };
                    string uu = fileName.Substring(fileName.LastIndexOf('.') + 1);
                    int i = uu.IndexOfAny(new char[] { '?', '%', '&' });
                    uu = uu.Substring(0, i == -1 ? uu.Length : i);
                    fileSavePicker.FileTypeChoices.Add($"{uu}文件", new string[] { "." + uu });
                    StorageFile file = await fileSavePicker.PickSaveFileAsync();
                    if (file != null)
                    {
                        HttpClient httpClient = new HttpClient();
                        using (Stream fs = await file.OpenStreamForWriteAsync())
                        using (Stream s = (await (await (await ApplicationData.Current.LocalCacheFolder.GetFolderAsync(datas[SFlipView.SelectedIndex].Type.ToString())).GetFileAsync(UIHelper.GetMD5(u))).OpenReadAsync()).AsStreamForRead())
                            await s.CopyToAsync(fs);
                    }
                    break;
                default:
                    break;
            }
        }

        private bool a = false;
        private async void SFlipView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int i = SFlipView.SelectedIndex;
            if (i == -1 || a)
            {
                return;
            }
            a = true;
            Images[i] = await datas[i].GetImage();
            Regex regex = new Regex(@"[^/]+(?!.*/)");
            TitleBar.Title = (regex.IsMatch(datas[i].Url) ? regex.Match(datas[i].Url).Value : "查看图片") + " (" + (i + 1).ToString() + "/" + datas.Count.ToString() + ")";
            a = false;
        }
    }
}
