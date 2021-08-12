using CoolapkUWP.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace CoolapkUWP.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class ShowImagePage : Page
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

        private readonly List<ImageData> datas = new List<ImageData>();
        private readonly ObservableCollection<ImageSource> Images = new ObservableCollection<ImageSource>();

        public ShowImagePage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            object[] vs = e.Parameter as object[];
            if (vs[0] is string url && vs[1] is ImageType type)
            {
                TitleOrigin.Visibility = RightOrigin.Visibility = type == ImageType.SmallImage || type == ImageType.SmallAvatar
                    ? Visibility.Visible
                    : Visibility.Collapsed;
                ShowImage(url, type);
            }
            else if (vs[0] is string[] urls && vs[1] is ImageType types && vs[2] is int index)
            {
                ShowImages(urls, types, index);
                TitleOrigin.Visibility = RightOrigin.Visibility = types == ImageType.SmallImage || types == ImageType.SmallAvatar
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
        }

        public void ShowImage(string url, ImageType type)
        {
            if (url.EndsWith(".gif") || url.EndsWith(".GIF"))
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

        private void CloseFlip_Click(object sender, RoutedEventArgs e) => Frame.GoBack();

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
            SFlipView.ContextFlyout.ShowAt(SFlipView);
        }

        private async void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem item = sender as MenuFlyoutItem;
            datas[SFlipView.SelectedIndex].ChangeType();
            UIHelper.ShowProgressBar();
            Images[SFlipView.SelectedIndex] = await datas[SFlipView.SelectedIndex].GetImage();
            UIHelper.HideProgressBar();
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
                        using (Stream fs = await file.OpenStreamForWriteAsync())
                        using (Stream s = (await (await (await ApplicationData.Current.LocalCacheFolder.GetFolderAsync(datas[SFlipView.SelectedIndex].Type.ToString())).GetFileAsync(UIHelper.GetMD5(u))).OpenReadAsync()).AsStreamForRead())
                        { await s.CopyToAsync(fs); }
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
            if (i == -1 || a) { return; }
            a = true;
            UIHelper.ShowProgressBar();
            try
            {
                Images[i] = await datas[i].GetImage();
                UIHelper.HideProgressBar();
            }
            catch
            {
                UIHelper.ErrorProgressBar();
            }
            Regex regex = new Regex(@"[^/]+(?!.*/)");
            TitleBar.Title = (regex.IsMatch(datas[i].Url) ? regex.Match(datas[i].Url).Value : "查看图片") + " (" + (i + 1).ToString() + "/" + datas.Count.ToString() + ")";
            TitleOrigin.Visibility = RightOrigin.Visibility = datas[i].Type == ImageType.SmallImage || datas[i].Type == ImageType.SmallAvatar
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            a = false;
        }
    }
}
